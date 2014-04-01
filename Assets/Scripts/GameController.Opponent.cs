using System;
using UnityEngine;

public partial class GameController
{
	public void InstantiateOpponentBubble(BubbleState bubbleState)
	{
		//for opponnent bubbles - right part of screen
		bubbleState.StartPosition = new Vector2(bubbleState.StartPosition.x + OpponentLevelBounds.x - PlayerLevelBounds.x
			, bubbleState.StartPosition.y);
		var bubbleObject = InstanatiateBubble(bubbleState);
		bubbleObject.generatedtTexture = TextureFactory.Instance.GetTexture(bubbleState.Size / level.BubblesSizeRange.y, true);
		opponentBubbles.Add(bubbleState.ID, bubbleObject);
	}

	public void OpponetBubbleBursted(int id)
	{
		if(opponentBubbles.ContainsKey(id))
			opponentBubbles[id].Hide();
	}

	public void OpponentBubbleMissed(int id)
	{
		if(opponentBubbles.ContainsKey(id))
			opponentBubbles[id].Hide();
	}
}


