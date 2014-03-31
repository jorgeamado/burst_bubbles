using UnityEngine;
using System.Collections;

public class BubbleObject : MonoBehaviour
{
	public BubbleState BubbleState;
	public Texture2D generatedtTexture;

	public bool IsDead()
	{
		return BubbleState.EBubbleStatus.Dead == this.BubbleState.Status;
	}

	void Start()
	{
		this.transform.localScale = Vector3.one * BubbleState.Size;
		renderer.material.mainTexture = generatedtTexture;
		renderer.material.color = new Color(Random.value, Random.value, Random.value);
		this.transform.position = new Vector3(BubbleState.StartPosition.x, BubbleState.StartPosition.y, 10);
	}

	void Update()
	{
		if(this.BubbleState.Status == BubbleState.EBubbleStatus.Alive)
			transform.position += BubbleState.Speed * Vector3.down;
	}

	public void Hide()
	{
		this.collider2D.enabled = false;
		this.BubbleState.Status = BubbleState.EBubbleStatus.Dead;
		this.gameObject.SetActive(false);
		//Animation, effects, etc
	}

	void OnDestroy()
	{
		renderer.material.mainTexture = null;
		generatedtTexture = null;
	}
}
