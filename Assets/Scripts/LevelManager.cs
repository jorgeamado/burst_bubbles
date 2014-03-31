using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NetworkPeer;
using System.Xml.Serialization;
using System.IO;

public partial class LevelManager : MonoBehaviour
{
	private const float BUBBLES_DEPTH = 10f;

	public LevelInfo level;
	public float bubbleInstatiateTimer = 0f;
	public float levelTimer = 0f;

	//Bubbles on the scene
	private List<BubbleObject> bubbles = new List<BubbleObject>();
	private Dictionary<int, BubbleObject> opponentBubbles = new Dictionary<int, BubbleObject>();

	private ResourceLoader resourceLoader = new ResourceLoader ();
	private Network network;
	private Object BubblePrefab = null;

	void Start()
	{
		StartCoroutine(LoadAssets());
		network = FindObjectOfType<Network>();
		InputManager.Instance.OnClick += OnClickEventHandler;
	}

	IEnumerator LoadAssets()
	{
		var www = resourceLoader.LoadBundle("/AssetBundles/Bubble.unity3d");
		yield return www;
		var request = resourceLoader.Load("LevelConfig", typeof(TextAsset));
		yield return request;
		StringReader reader = new StringReader(((TextAsset)request.asset).text);
		XmlSerializer serializer = new XmlSerializer(typeof(LevelInfo));
		level = serializer.Deserialize(reader) as LevelInfo;
		level.LevelState = LevelInfo.ELevelState.Stoped;
		var prefabRequest = resourceLoader.Load("Bubble", typeof(GameObject));
		yield return prefabRequest;
		BubblePrefab = prefabRequest.asset as GameObject;

//		level.LevelState = LevelInfo.ELevelState.Playing;

	}

	void Update()
	{	
		if (level != null)
		{
			switch (level.LevelState)
			{
				case LevelInfo.ELevelState.Playing:
					{
						UpdateLevelTimers();
						foreach (var bubble in bubbles)
						{
							if (bubble.transform.position.y <= LevelBounds.z + bubble.BubbleState.Size / 2f)
							{
								level.BubbleMissed(bubble.BubbleState);
								network.SendMessageToOtherPlayer(Network.EMessageType.BubbleMissed.ToString(),
									bubble.BubbleState.ID.ToString());
								bubble.Hide();
							}
						}
						ClearDeadBubbles();
						break;
					}
				case LevelInfo.ELevelState.Failed:
					{
						level.Reset();
						break;
					}
			}
		}
	}

	void ClearDeadBubbles()
	{
		if (level.LevelState == LevelInfo.ELevelState.Failed)
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

	void OnGUI()
	{
		GUI.Label(new Rect(20, Screen.height - 20, 100, 100), levelTimer.ToString());
		GUI.Label(new Rect(20, Screen.height - 40, 100, 100), "SCORE: " + level.LevelScore.ToString());
		GUI.Label(new Rect(20, Screen.height - 60, 100, 100), "missed bubbles: " + level.MissedBubbles.ToString() + "/" + level.MaxBubblesMissed);

		if (level.LevelState == LevelInfo.ELevelState.Failed)
			GUI.Label(new Rect(Screen.width/2f, Screen.height/2f, 100, 100), "Level Failed");

	}

	void UpdateLevelTimers()
	{
		if (levelTimer <= 0)
		{
			levelTimer = level.LevelTime;
			IncrementLevel();
		}

		if (bubbleInstatiateTimer <= 0)
		{
			bubbleInstatiateTimer = level.BubblesInstantiationPeriod;
			InstantiateBubble();
		}
		levelTimer -= Time.deltaTime;
		bubbleInstatiateTimer -= Time.deltaTime;
	}

	void IncrementLevel()
	{
		level.IncrementLevel();
		ClearCurrentLevel();
		TextureFactory.Instance.GenerateTextures();
	}

	void OnClickEventHandler(Vector2 inputPosition)
	{
		if (level.LevelState == LevelInfo.ELevelState.Playing)
		{
			foreach (var bubble in bubbles)
			{
				if (bubble.collider2D.OverlapPoint(Camera.main.ScreenToWorldPoint(inputPosition)))
				{
					HandleBubbleClick(bubble);
					break;
				}
			}
		}
	}

	Vector4 ? levelBounds = null; // x,y - axis X, z,w - Y
	Vector4 LevelBounds
	{
		get 
		{
			if (levelBounds == null)
			{
				var downLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10));
				var upRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10));
				levelBounds = new Vector4(downLeft.x, upRight.x, downLeft.y, upRight.y);
			}
			return  levelBounds.Value;
		}
	}

	private void InstantiateBubble()
	{
		var bubbleState = level.GetBubble(LevelBounds);
		var bubbleObject = InstanatiateBubble(bubbleState);
		bubbleObject.generatedtTexture = TextureFactory.Instance.GetTexture(bubbleState.Size / level.BubblesSizeRange.y);
		bubbles.Add(bubbleObject);
		network.SendMessageToOtherPlayer(Network.EMessageType.BubbleCreated.ToString()
			, bubbleState.SerializeToString());
	}

	private BubbleObject InstanatiateBubble(BubbleState bubbleState)
	{
		var bubbleObject = (Instantiate(BubblePrefab) as GameObject).GetComponent<BubbleObject>();
		bubbleObject.BubbleState = bubbleState;
		return bubbleObject;
	}

	void HandleBubbleClick(BubbleObject bubble)
	{
		level.BubbleBursted(bubble.BubbleState);
		network.SendMessageToOtherPlayer(Network.EMessageType.BubbleBursted.ToString(),
			bubble.BubbleState.ID.ToString());
		bubble.Hide();
	}

	void OnDestroy()
	{
		ClearCurrentLevel();

		if(null != InputManager.Instance)
			InputManager.Instance.OnClick -= OnClickEventHandler;
	}

	void ClearCurrentLevel()
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
}
