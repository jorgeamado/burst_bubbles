using UnityEngine;
using System.Collections;

public class StartPopup : MonoBehaviour
{
	ApplicationManager applicationManager;

	void Start()
	{
		applicationManager = FindObjectOfType<ApplicationManager>();
	}

	string ip = "127.0.0.1";
	string port = "0";

	void OnGUI()
	{
		if (applicationManager.GameStatus == ApplicationManager.EGameStatus.InitializationNetwork)
		{
			GUI.Box(new Rect(10, 10, 250, 120), "");
			GUI.Label(new Rect(20, 40, 80, 20), "Name");
			applicationManager.PlayerName = GUI.TextField(new Rect(80, 40, 80, 20), applicationManager.PlayerName);

			GUI.Label(new Rect(20, 60, 80, 20), "Address");
			ip = GUI.TextField(new Rect(80, 60, 80, 20), ip);
			port = GUI.TextField(new Rect(160, 60, 50, 20), port);

			if (GUI.Button(new Rect(20, 100, 80, 20), "StartGame"))
			{
				applicationManager.ip = ip;
				if (int.TryParse(port, out applicationManager.port))
				{
					applicationManager.StartListening();
				}
			}
		}
	}
}
