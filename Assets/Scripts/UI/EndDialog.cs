using UnityEngine;
using System.Collections;

public class EndDialog : MonoBehaviour
{
	ApplicationManager applicationManager;
	GameController gameController;

	void Start()
	{
		applicationManager = FindObjectOfType<ApplicationManager>();
		gameController = FindObjectOfType<GameController>();
	}

	void OnGUI()
	{
		if (applicationManager.GameStatus == ApplicationManager.EGameStatus.GameFinished)
		{
			string message = gameController.level.LevelState != Level.ELevelState.Failed ? 
				"You win!" : "You lost the game";
			GUI.Box(new Rect(10, 10, 250, 120), message);


			if (GUI.Button(new Rect(20, 40, 80, 20), "Replay"))
			{
				applicationManager.Replay();
			}

			if (GUI.Button(new Rect(100, 40, 80, 20), "Leave"))
			{
				applicationManager.Leave();
			}
		}
	}


}
