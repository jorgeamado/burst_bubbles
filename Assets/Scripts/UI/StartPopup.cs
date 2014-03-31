using UnityEngine;
using System.Collections;

public class StartPopup : MonoBehaviour
{
	Network network;
	LevelManager levelmanager;

	void Start()
	{
		network = FindObjectOfType<Network>();
	}

	void OnGUI()
	{
		if (network.UIStatus == Network.EUIStatus.EnteringName)
		{
			GUI.Box(new Rect(10, 10, 250, 120), "");
			GUI.Label(new Rect(20, 40, 80, 20), "Name");
			network.PlayerName = GUI.TextField(new Rect(80, 40, 80, 20), network.PlayerName);

			GUI.Label(new Rect(20, 60, 80, 20), "Address");
			network.ip = GUI.TextField(new Rect(80, 60, 80, 20), network.ip);
			network.port = int.Parse(GUI.TextField(new Rect(160, 60, 50, 20), network.port.ToString()));

			if (GUI.Button(new Rect(20, 100, 80, 20), "StartGame"))
			{
				network.StartListening();
			}
		}
	}
}
