using System.Collections;
using System;
using UnityEngine;

public class BubbleState 
{
	static int IDCounter = 0;

	public enum EBubbleStatus
	{
		Alive,
		Dead,
	}

	public EBubbleStatus Status;

	public float Size = 1f; 
	public float Speed = 1f;
	public Vector2 StartPosition;

	public int ID = 0;

	public BubbleState()
	{
		Status = EBubbleStatus.Alive;
		ID = ++IDCounter;
	}

	public string SerializeToString()
	{
		return ID.ToString()
		+ "|"+ Size.ToString()
		+ "|" + Speed.ToString()
		+ "|" + StartPosition.x
		+ "|" + StartPosition.y;
	}

	public void DeserializeFromString(string str)
	{
		int i = 0;
		var blocks = str.Split('|');
		ID = int.Parse(blocks[i++]);
		Size = float.Parse(blocks[i++]);
		Speed = float.Parse(blocks[i++]);
		StartPosition.x = float.Parse(blocks[i++]);
		StartPosition.y = float.Parse(blocks[i++]);
	}
}
