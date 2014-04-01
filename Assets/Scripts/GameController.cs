using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NetworkPeer;
using System.Xml.Serialization;
using System.IO;

public partial class GameController : MonoBehaviour
{
	private const float BUBBLES_DEPTH = 10f;
	private Vector4 PlayerLevelBounds;
	private Vector4 OpponentLevelBounds;

	public Level level;
	public float bubbleInstatiateTimer = 0f;
	public float levelTimer = 0f;

	//Bubbles on the scene
	private List<BubbleObject> bubbles = new List<BubbleObject>();
	private Dictionary<int, BubbleObject> opponentBubbles = new Dictionary<int, BubbleObject>();

	private ResourceLoader resourceLoader = new ResourceLoader ();
	private ApplicationManager network;
	private Object BubblePrefab = null;

	private void Start()
	{
		SetupLevelBounds();
		StartCoroutine(LoadAssets());
		network = FindObjectOfType<ApplicationManager>();
		InputManager.Instance.OnClick += OnClickEventHandler;
	}

	private void SetupLevelBounds()
	{
		var downLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10));
		var upRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height, 10));
		PlayerLevelBounds = new Vector4(downLeft.x, upRight.x, downLeft.y, upRight.y);
		var opponentDownLeft = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0, 10));
		var opponentUpRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width , Screen.height, 10));
		OpponentLevelBounds = new Vector4(opponentDownLeft.x, opponentUpRight.x, opponentDownLeft.y, opponentUpRight.y);
	}

	private IEnumerator LoadAssets()
	{
		//Load bundle
		var www = resourceLoader.LoadBundle("/AssetBundles/Bubble.unity3d");
		yield return www;

		//load Level Config
		var request = resourceLoader.Load("LevelConfig", typeof(TextAsset));
		yield return request;
		StringReader reader = new StringReader(((TextAsset)request.asset).text);
		XmlSerializer serializer = new XmlSerializer(typeof(Level));
		level = serializer.Deserialize(reader) as Level;
		level.LevelState = Level.ELevelState.Loading;

		//load Bubble prefab
		var prefabRequest = resourceLoader.Load("Bubble", typeof(GameObject));
		yield return prefabRequest;
		BubblePrefab = prefabRequest.asset as GameObject;
	}

	private void OnDestroy()
	{
		ClearCurrentLevel();

		if(null != InputManager.Instance)
			InputManager.Instance.OnClick -= OnClickEventHandler;
	}

	private void Update()
	{	
		if (level != null)
		{
			switch (level.LevelState)
			{
				case Level.ELevelState.Playing:
					{
						UpdateLevelTimers();
						foreach (var bubble in bubbles)
						{
							if (bubble.transform.position.y <= PlayerLevelBounds.z + bubble.BubbleState.Size / 2f)
							{
								level.BubbleMissed(bubble.BubbleState);
								network.SendMessageToOtherPlayer(EMessageType.BubbleMissed.ToString(),
									bubble.BubbleState.ID.ToString());
								bubble.Hide();
							}
						}
						ClearDeadBubbles();
						break;
					}
				case Level.ELevelState.Failed:
					{
						ClearCurrentLevel();
						break;
					}
			}
		}
	}

	private void UpdateLevelTimers()
	{
		if (levelTimer <= 0)
		{
			levelTimer = level.LevelTime;
			level.IncrementLevel();
			ClearCurrentLevel();
			TextureFactory.Instance.GenerateTextures();
		}

		if (bubbleInstatiateTimer <= 0)
		{
			bubbleInstatiateTimer = level.BubblesInstantiationPeriod;
			InstantiateBubble();
		}

		levelTimer -= Time.deltaTime;
		bubbleInstatiateTimer -= Time.deltaTime;
	}

	private void ClearDeadBubbles()
	{
		if (level.LevelState == Level.ELevelState.Failed)
		{
			ClearCurrentLevel();
		}
		else
		{
			//Clear opponents bubbles;
			var RemoveItems = new List<KeyValuePair<int, BubbleObject>>();
			foreach (var bubble in opponentBubbles)
			{
				if (!bubble.Value.gameObject.activeSelf || bubble.Value.IsDead())
					RemoveItems.Add(bubble);
			}
			foreach (var toRemove in RemoveItems)
			{
				opponentBubbles.Remove(toRemove.Key);
				GameObject.Destroy(toRemove.Value.gameObject);
			}

			//Clear player bubbles
			for (int i = 0, n = bubbles.Count; i < n; ++i)
			{
				if (!bubbles[i].gameObject.activeSelf || bubbles[i].IsDead())
				{
					GameObject.Destroy(bubbles[i].gameObject);
					bubbles[i] = null;
				}
			}
			bubbles.RemoveAll(bubble => null == bubble);
		}
	}

	private void ClearCurrentLevel()
	{
		var itemsToRemove = bubbles;
		itemsToRemove.AddRange(opponentBubbles.Values);
		opponentBubbles = new Dictionary<int, BubbleObject>();
		bubbles = new List<BubbleObject>();
		foreach (var item in itemsToRemove)
		{
			if(item != null && item.gameObject != null)
				GameObject.Destroy(item.gameObject);
		}
	}

	private void OnClickEventHandler(Vector2 inputPosition)
	{
		if (level.LevelState == Level.ELevelState.Playing)
		{
			foreach (var bubble in bubbles)
			{
				if (bubble.collider2D.OverlapPoint(Camera.main.ScreenToWorldPoint(inputPosition)))
				{
					level.BubbleBursted(bubble.BubbleState);
					network.SendMessageToOtherPlayer(EMessageType.BubbleBursted.ToString(),
						bubble.BubbleState.ID.ToString());
					bubble.Hide();
					break;
				}
			}
		}
	}

	private void InstantiateBubble()
	{
		var bubbleState = level.GetBubble(PlayerLevelBounds);
		var bubbleObject = InstanatiateBubble(bubbleState);
		bubbleObject.generatedtTexture = TextureFactory.Instance.GetTexture(bubbleState.Size / level.BubblesSizeRange.y);
		bubbles.Add(bubbleObject);
		network.SendMessageToOtherPlayer(EMessageType.BubbleCreated.ToString()
			, bubbleState.SerializeToString());
	}

	private BubbleObject InstanatiateBubble(BubbleState bubbleState)
	{
		var bubbleObject = (Instantiate(BubblePrefab) as GameObject).GetComponent<BubbleObject>();
		bubbleObject.BubbleState = bubbleState;
		return bubbleObject;
	}

	#region Level Hotkeys

	public void StartLevel()
	{
		level.Reset();
		level.LevelState = Level.ELevelState.Playing;
	}

	public void StopLevel()
	{
		level.LevelState = Level.ELevelState.Stopped;
		ClearCurrentLevel();
	}

	public void RestartLevel()
	{
		ClearCurrentLevel();
		level.Reset();
		StartLevel();
	}

	#endregion

	private void OnGUI()
	{
		GUI.Label(new Rect(20, Screen.height - 20, 100, 100), levelTimer.ToString());
		GUI.Label(new Rect(20, Screen.height - 40, 100, 100), "SCORE: " + level.LevelScore.ToString());
		GUI.Label(new Rect(20, Screen.height - 60, 100, 100), "missed bubbles: " + level.MissedBubbles.ToString() + "/" + level.MaxBubblesMissed);

		if (level.LevelState == Level.ELevelState.Failed)
			GUI.Label(new Rect(Screen.width/2f, Screen.height/2f, 100, 100), "Level Failed");

	}
}
