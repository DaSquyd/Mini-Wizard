using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InControl;

public class ActionInputManager : MonoBehaviour
{
	public static ActionInputManager current;

	public ActionInputSettings settings;

	public struct Action
	{
		public string name;
		public ActionInputSettings.ActionInput input;
		public float state;
		public float lastState;

		public float fixedState;
		public float lastFixedState;
	}

	public static Action[] actions;

	private void Start()
	{
		current = this;

		actions = new Action[settings.inputs.Count];

		for (int i = 0; i < settings.inputs.Count; i++)
		{
			actions[i] = new Action() {
				name = settings.inputs[i].name,
				input = settings.inputs[i]
			};
		}
	}

	private void Update()
	{
		InputDevice inputDevice = InputManager.ActiveDevice;

		for (int i = 0; i < actions.Length; i++)
		{

			actions[i].lastState = actions[i].state;

			actions[i].state = 0f;

			actions[i].state += Mathf.Clamp01(inputDevice.GetControl(actions[i].input.gamepad).Value * (actions[i].input.gamepadInverse ? -1f : 1f));
			actions[i].state += Input.GetKey(actions[i].input.key1) ? 1f : 0f;
			actions[i].state += Input.GetKey(actions[i].input.key2) ? 1f : 0f;

			actions[i].state = Mathf.Clamp01(actions[i].state);
		}
	}

	private void FixedUpdate()
	{
		InputDevice inputDevice = InputManager.ActiveDevice;

		for (int i = 0; i < actions.Length; i++)
		{
			actions[i].lastFixedState = actions[i].fixedState;

			actions[i].fixedState = 0f;

			actions[i].fixedState += Mathf.Clamp01(inputDevice.GetControl(actions[i].input.gamepad).Value * (actions[i].input.gamepadInverse ? -1f : 1f));
			actions[i].fixedState += Input.GetKey(actions[i].input.key1) ? 1f : 0f;
			actions[i].fixedState += Input.GetKey(actions[i].input.key2) ? 1f : 0f;

			actions[i].fixedState = Mathf.Clamp01(actions[i].fixedState);
		}
	}

	public static float GetInput(string name)
	{
		for (int i = 0; i < actions.Length; i++)
		{
			if (actions[i].name != name)
				continue;

			return actions[i].state;
		}

		Debug.LogError($"Action \"{name}\" was not found!");
		return 0f;
	}
	public static bool GetInputDown(string name)
	{
		for (int i = 0; i < actions.Length; i++)
		{
			if (actions[i].name != name)
				continue;

			return actions[i].state == 1f && actions[i].lastState == 0f;
		}

		Debug.LogError($"Action \"{name}\" was not found!");
		return false;
	}
	public static bool GetInputUp(string name)
	{
		for (int i = 0; i < actions.Length; i++)
		{
			if (actions[i].name != name)
				continue;

			return actions[i].state == 0f && actions[i].lastState == 1f;
		}

		Debug.LogError($"Action \"{name}\" was not found!");
		return false;
	}


	public static float GetFixedInput(string name)
	{
		for (int i = 0; i < actions.Length; i++)
		{
			if (actions[i].name != name)
				continue;

			Debug.Log(Input.GetKey(actions[i].input.key1));

			return actions[i].fixedState;
		}

		Debug.LogError($"Action \"{name}\" was not found!");
		return 0f;
	}
	public static bool GetFixedInputDown(string name)
	{
		for (int i = 0; i < actions.Length; i++)
		{
			if (actions[i].name != name)
				continue;

			return actions[i].fixedState == 1f && actions[i].lastFixedState == 0f;
		}

		return false;
	}
	public static bool GetFixedInputUp(string name)
	{
		for (int i = 0; i < actions.Length; i++)
		{
			if (actions[i].name != name)
				continue;

			return actions[i].fixedState == 0f && actions[i].lastFixedState == 1f;
		}

		Debug.LogError($"Action \"{name}\" was not found!");
		return false;
	}
}
