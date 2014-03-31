using System;
using UnityEngine;

public partial class LevelManager
{
	public void InstantiateOpponentBubble(BubbleState bubbleState)
	{
		Debug.Log(bubbleState.StartPosition);
		bubbleState.StartPosition = new Vector2(bubbleState.StartPosition.x + (LevelBounds.y - LevelBounds.x) / 2f
			, bubbleState.StartPosition.y);
		Debug.Log(bubbleState.StartPosition);
		var bubbleObject = InstanatiateBubble(bubbleState);
		bubbleObject.generatedtTexture = TextureFactory.Instance.GetTexture(bubbleState.Size / level.BubblesSizeRange.y, true);

		//for opponnent bubbles - right part of screen
		var position = bubbleObject.transform.position;
		position.x = Mathf.Lerp(LevelBounds.y / 2f, LevelBounds.y, position.x / LevelBounds.y);
		bubbleObject.transform.position = position;
		opponentBubbles.Add(bubbleState.ID, bubbleObject);
	}

	public void OpponetBubbleBursted(int id)
	{
		opponentBubbles[id].Hide();
	}

	public void OpponentBubbleMissed(int id)
	{
		opponentBubbles[id].Hide();
	}
}


