using UnityEngine;
using System.Collections;

public class StartGamePopup : MonoBehaviour
{
	private Network network;
	void Start ()
	{
		network = FindObjectOfType<Network>();
	}

	string ip = "127.0.0.1";
	int port = 14001;

	void OnGUI()
	{
		if(network.UIStatus == Network.EUIStatus.WaitingForConnection)
		{
			GUI.Box(new Rect(10,10,250,90), "");
			GUI.Label(new Rect(20, 20, 60, 20), network.PlayerName);

			GUI.Label(new Rect(20, 40, 60, 20), network.OtherPlayerName);
			GUI.Label(new Rect(100, 40, 100, 20), network.OpponentInfo);

			if (GUI.Button(new Rect(20, 60, 100, 20), "Start Game"))
			{
				network.Ready();
			}

			if (!network.IsOtherPlayerConnected)
			{
				GUI.Label(new Rect(320, 60, 80, 20), "Address");
				ip = GUI.TextField(new Rect(380, 60, 80, 20), ip);
				port = int.Parse(GUI.TextField(new Rect(460, 60, 50, 20), port.ToString()));

				if (GUI.Button(new Rect(320, 60, 60, 20), "Connect"))
				{
					network.ConnectTo(ip, port);
				}
			}
		}
	}
}
