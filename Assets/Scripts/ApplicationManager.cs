using UnityEngine;
using System.Collections;
using NetworkPeer;

public class ApplicationManager : MonoBehaviour 
{
	Peer peer;
	Connection OtherPlayer = null;

	public string PlayerName = "Player Name";
	public string ip = "127.0.0.1";
	public int port = 0;
	public string OtherPlayerName = "Unknown";

	GameController gameController;

	public enum EGameStatus
	{
		GameFinished,
		InitializationNetwork,
		CreatingNetwork,
		WaitingForAnswer,
		OpponentReady,
		Playing,

	}
	public EGameStatus GameStatus;

	void Start () 
	{
		Application.runInBackground = true;
		gameController = FindObjectOfType<GameController>();
		GameStatus = EGameStatus.InitializationNetwork;
	}

	void Update()
	{
		if(peer != null)
		{
			peer.HearBeat();
		}

		if(GameStatus == EGameStatus.Playing && gameController.level.LevelState == Level.ELevelState.Failed)
		{
			GameStatus = EGameStatus.GameFinished;
			SendMessageToOtherPlayer(EMessageType.GameLost.ToString());
		}
	}

	void NewConnectionEventHandler(Connection connection)
	{
		OtherPlayer = connection;
		connection.OnMessageReceived += MessageHandler;
		connection.OnDisconnected += ResetGame;
		OtherPlayer.SendMessage(EMessageType.GetName.ToString());
	}

	void ResetGame()
	{
		GameStatus = EGameStatus.CreatingNetwork;
		gameController.StopLevel();
		gameController.level.Reset();
		ResetOpponentConnection();
	}

	void MessageHandler(string msg)
	{
		var blocks = msg.Split(Connection.MESSAGE_SEPARATOR.ToCharArray(), 2);
		var messageType = Helper.StringToEnum<EMessageType>(blocks[0], EMessageType.Unknown);
		switch (messageType)
		{
			case EMessageType.GetName:
				{
					OtherPlayer.SendMessage(EMessageType.SendName.ToString(), PlayerName);
					break;
				}
			case EMessageType.SendName:
				{
					OtherPlayerName = blocks[1];
					break;
				}
			case EMessageType.Ready:
				{
					if (GameStatus == EGameStatus.WaitingForAnswer)
					{
						gameController.StartLevel();
						GameStatus = EGameStatus.Playing;
					}
					else
					{
						GameStatus = EGameStatus.OpponentReady;
					}
					break;
				}
			case EMessageType.BubbleCreated:
				{
					var bubbleState = new BubbleState();
					bubbleState.DeserializeFromString(blocks[1]);
					gameController.InstantiateOpponentBubble(bubbleState);
					break;
				}
			case EMessageType.BubbleBursted:
				{
					int id = int.Parse(blocks[1]);
					gameController.OpponetBubbleBursted(id);
					break;
				}
			case EMessageType.BubbleMissed:
				{
					int id = int.Parse(blocks[1]);
					gameController.OpponentBubbleMissed(id);
					break;
				}
			case EMessageType.GameLost:
				{
					gameController.StopLevel();
					GameStatus = EGameStatus.GameFinished;
					break;
				}
			case EMessageType.ReplayGame:
				{
					gameController.RestartLevel();
					GameStatus = EGameStatus.Playing;
					break;
				}
			case EMessageType.Leave:
				{
					ResetGame();
					break;
				}
			default:
				{
					Logger.LogWarning("Unknown state: " + blocks[0]);
					break;
				}
		}
	}

	public bool IsOtherPlayerConnected
	{
		get { return null != OtherPlayer && OtherPlayer.Status == Connection.EConnectionStatus.Connected; }
	}

	public string PlayerInfo
	{
		get { return null == peer ? "No connections" : peer.LocalEndPoint.ToString();}
	}

	public string OpponentInfo
	{
		get 
		{
			return null == OtherPlayer ? "No connections" : OtherPlayer.EndPoint.ToString();
		}
	}
	
	public void SendMessageToOtherPlayer(params string[] msg)
	{
		if(IsOtherPlayerConnected)
			OtherPlayer.SendMessage(msg);
	}
		
	private void ResetOpponentConnection()
	{
		OtherPlayer.OnMessageReceived -= MessageHandler;
		OtherPlayer.OnDisconnected -= ResetGame;
		OtherPlayer = null;
	}

	private void OnDestroy()
	{
		if (null != OtherPlayer)
		{
			ResetOpponentConnection();
		}

		if (null != peer)
		{
			peer.OnNewConnection -= NewConnectionEventHandler;
			peer.Stop();
			peer = null;
		}
	}

	public void StartListening()
	{
		var config = new NetworkConfiguration(ip, port, 1); //just for one player
		try
		{
			peer = new Peer(config);
			peer.OnNewConnection += NewConnectionEventHandler;
			peer.Start();
			GameStatus = EGameStatus.CreatingNetwork;
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning(ex.Message);
		}
	}

	public void Ready()
	{
		if (GameStatus == EGameStatus.OpponentReady)
		{
			gameController.StartLevel();
			GameStatus = EGameStatus.Playing;
		}
		else
		{
			GameStatus = EGameStatus.WaitingForAnswer;
		}
		OtherPlayer.SendMessage(EMessageType.Ready.ToString());
	}

	public void Replay()
	{
		if(null != OtherPlayer)
			OtherPlayer.SendMessage(EMessageType.ReplayGame.ToString());
		gameController.RestartLevel();
		GameStatus = EGameStatus.Playing;
	}

	public void Leave()
	{
		if(null != OtherPlayer)
			OtherPlayer.SendMessage(EMessageType.Leave.ToString());
		ResetGame();
	}

	public void ConnectTo(string ip, int port)
	{
		peer.Connect(System.Net.IPAddress.Parse(ip), port);
	}
}
