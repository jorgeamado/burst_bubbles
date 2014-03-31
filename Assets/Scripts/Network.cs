using UnityEngine;
using System.Collections;
using NetworkPeer;

public class Network : MonoBehaviour 
{
	Peer peer;
	Connection OtherPlayer = null;

	public string PlayerName = "Unity";
	public string ip = "127.0.0.1";
	public int port = 14000;
	public string OtherPlayerName = "Unknown";

	LevelManager levelmanager;

	public enum EUIStatus
	{
		EnteringName,
		WaitingForConnection,
		Ready,
		Playing,
	}
	public EUIStatus UIStatus;

	public enum EMessageType
	{
		Unknown,
		GetName,
		SendName,
		Ready,
		GameStart,
		GameFinished,
		BubbleCreated,
		BubbleBursted,
		BubbleMissed,
	}

	void Start () 
	{
		Application.runInBackground = true;
		levelmanager = FindObjectOfType<LevelManager>();
		UIStatus = EUIStatus.EnteringName;
	}

	void Update()
	{
		if(peer != null)
		{
			peer.HearBeat();
		}
	}

	void NewConnectionEventHandler(Connection connection)
	{
		OtherPlayer = connection;
		connection.OnMessageReceived += MessageHandler;
		OtherPlayer.SendMessage(EMessageType.GetName.ToString());
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
					if (levelmanager.level.LevelState == LevelInfo.ELevelState.Ready)
					{
						levelmanager.level.LevelState = LevelInfo.ELevelState.Playing;
						UIStatus = EUIStatus.Playing;
					}
					else
					{
						levelmanager.level.LevelState = LevelInfo.ELevelState.Waiting;
						UIStatus = EUIStatus.WaitingForConnection;
					}
					break;
				}
			case EMessageType.BubbleCreated:
				{
					var bubbleState = new BubbleState();
					bubbleState.DeserializeFromString(blocks[1]);
					levelmanager.InstantiateOpponentBubble(bubbleState);
					break;
				}
			case EMessageType.BubbleBursted:
				{
					int id = int.Parse(blocks[1]);
					levelmanager.OpponetBubbleBursted(id);
					break;
				}
			case EMessageType.BubbleMissed:
				{
					int id = int.Parse(blocks[1]);
					levelmanager.OpponentBubbleMissed(id);
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
		
	void OnDestroy()
	{
		if (null != OtherPlayer)
		{
			OtherPlayer.OnMessageReceived -= MessageHandler;
			OtherPlayer = null;
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
			UIStatus = EUIStatus.WaitingForConnection;
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning(ex.Message);
		}
	}

	public void Ready()
	{
		if (levelmanager.level.LevelState == LevelInfo.ELevelState.Waiting)
		{
			levelmanager.level.LevelState = LevelInfo.ELevelState.Playing;
			UIStatus = EUIStatus.Playing;
		}
		else
		{
			levelmanager.level.LevelState = LevelInfo.ELevelState.Ready;
			UIStatus = EUIStatus.WaitingForConnection;
		}
		OtherPlayer.SendMessage(EMessageType.Ready.ToString());
	}

	public void ConnectTo(string ip, int port)
	{
		peer.Connect(System.Net.IPAddress.Parse(ip), port);
	}
}
