using UnityEngine;
using System.Collections;
using System;
using System.Xml.Serialization;
using System.IO;

[Serializable]
public class LevelInfo
{
	public enum ELevelState
	{
		Waiting,
		Ready,
		Stoped,
		Playing,
		Failed,
	}
	[XmlIgnore]
	public ELevelState LevelState;

	[SerializeField]
	[XmlElement]
	private float LevelSpeed = 0.0001f;
	public Vector2 BubblesSpeedRange = new Vector2(-0.005f, 0.05f);
	public Vector2 BubblesSizeRange = new Vector2(0.5f, 3f);
	public Vector2 BubblesInstantiationPeriodRange = new Vector2(0.1f, 1f);
	public float BubblesInstantiationPeriod = 1f;//seconds
	public float LevelTime = 10f; //seconds
	public int LevelNumber = 0;
	public readonly int MaxBubblesMissed = 10;

	private int levelScore = 0;
	[XmlIgnore]
	public int LevelScore
	{get { return levelScore; }}

	public int missedBubbles = 0;
	[XmlIgnore]
	public int MissedBubbles
	{get { return missedBubbles; }}

	public BubbleState GetBubble(Vector4 LevelBounds)
	{
		var bubble = new BubbleState();
		bubble.Size = UnityEngine.Random.Range(BubblesSizeRange.x, BubblesSizeRange.y);
		var halfOfScreen = (LevelBounds.y + LevelBounds.x) / 2f;
		Debug.Log(halfOfScreen);
		var posX = UnityEngine.Random.Range(LevelBounds.x + bubble.Size / 2f,  halfOfScreen - bubble.Size / 2f);
		var posY = LevelBounds.w - bubble.Size / 2f;
		bubble.StartPosition = new Vector2(posX, posY);
		bubble.Speed = LevelNumber * LevelSpeed + Mathf.Lerp(BubblesSpeedRange.y, BubblesSpeedRange.x, bubble.Size / BubblesSizeRange.y);
		return bubble;
	}

	public void IncrementLevel()
	{
		++LevelNumber;
		BubblesInstantiationPeriod = Mathf.Lerp(BubblesInstantiationPeriodRange.y,
			BubblesInstantiationPeriodRange.x, (float)LevelNumber / 40f);
	}

	public void Reset()
	{
		LevelNumber = 0;
		BubblesInstantiationPeriod = BubblesInstantiationPeriodRange.y;
		LevelState = ELevelState.Stoped;
	}

	public void BubbleBursted(BubbleState state)
	{
		this.levelScore += (int)(state.Size * 1000); //People love big numbers!
	}

	public void BubbleMissed(BubbleState state)
	{
		if (++missedBubbles > MaxBubblesMissed)
		{
			LevelState = ELevelState.Failed;
		}
	}
}
