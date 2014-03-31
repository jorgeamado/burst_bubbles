using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour
{
	public enum EInputState
	{
		Normal,
		MouseDown,
	}

	private EInputState inputState;

	private static InputManager instance = null;
	public static InputManager Instance
	{ get { return instance; } }

	public System.Action<Vector2> OnClick = null;
	void Awake()
	{
		if (null != instance)
		{
			Debug.LogError("There are more than one InputManager on the scene");
		}
		instance = this;

		inputState = EInputState.Normal;
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0) && inputState != EInputState.MouseDown)
		{
			inputState = EInputState.MouseDown;
			if (null != OnClick)
				OnClick(Input.mousePosition);

		}
		else if (Input.GetMouseButtonUp(0))
		{
			inputState = EInputState.Normal;
		}
	}

	void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}
		OnClick = null;
	}
}
