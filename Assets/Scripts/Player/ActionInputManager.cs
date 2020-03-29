using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InControl;

[RequireComponent(typeof(GameManager))]
public class ActionInputManager : MonoBehaviour
{
	public static ActionInputManager Current;

	public ActionInputSettings Settings;

	public struct Action
	{
		public string Name;
		public ActionInputSettings.ActionInput Input;
		public float State;
		public float LastState;

		public float FixedState;
		public float LastFixedState;
	}

	public static Action[] Actions;

	private void Start()
	{
		Current = this;

		Actions = new Action[Settings.Inputs.Count];

		for (int i = 0; i < Settings.Inputs.Count; i++)
		{
			Actions[i] = new Action() {
				Name = Settings.Inputs[i].Name,
				Input = Settings.Inputs[i]
			};
		}
	}

	private void Update()
	{
		InputDevice inputDevice = InputManager.ActiveDevice;

		for (int i = 0; i < Actions.Length; i++)
		{

			Actions[i].LastState = Actions[i].State;

			Actions[i].State = 0f;

			Actions[i].State += Mathf.Clamp01(inputDevice.GetControl(Actions[i].Input.Gamepad).Value * (Actions[i].Input.GamepadInverse ? -1f : 1f));
			Actions[i].State += Input.GetKey(Actions[i].Input.Key1) ? 1f : 0f;
			Actions[i].State += Input.GetKey(Actions[i].Input.Key2) ? 1f : 0f;

			Actions[i].State = Mathf.Clamp01(Actions[i].State);
		}
	}

	private void FixedUpdate()
	{
		InputDevice inputDevice = InputManager.ActiveDevice;

		for (int i = 0; i < Actions.Length; i++)
		{
			Actions[i].LastFixedState = Actions[i].FixedState;

			Actions[i].FixedState = 0f;

			Actions[i].FixedState += Mathf.Clamp01(inputDevice.GetControl(Actions[i].Input.Gamepad).Value * (Actions[i].Input.GamepadInverse ? -1f : 1f));
			Actions[i].FixedState += Input.GetKey(Actions[i].Input.Key1) ? 1f : 0f;
			Actions[i].FixedState += Input.GetKey(Actions[i].Input.Key2) ? 1f : 0f;

			Actions[i].FixedState = Mathf.Clamp01(Actions[i].FixedState);
		}
	}

	public static float GetInput(string name)
	{
		for (int i = 0; i < Actions.Length; i++)
		{
			if (Actions[i].Name != name)
				continue;

			return Actions[i].State;
		}

		Debug.LogError($"Action \"{name}\" was not found!");
		return 0f;
	}
	public static bool GetInputDown(string name)
	{
		for (int i = 0; i < Actions.Length; i++)
		{
			if (Actions[i].Name != name)
				continue;

			return Actions[i].State == 1f && Actions[i].LastState == 0f;
		}

		Debug.LogError($"Action \"{name}\" was not found!");
		return false;
	}
	public static bool GetInputUp(string name)
	{
		for (int i = 0; i < Actions.Length; i++)
		{
			if (Actions[i].Name != name)
				continue;

			return Actions[i].State == 0f && Actions[i].LastState == 1f;
		}

		Debug.LogError($"Action \"{name}\" was not found!");
		return false;
	}


	public static float GetFixedInput(string name)
	{
		for (int i = 0; i < Actions.Length; i++)
		{
			if (Actions[i].Name != name)
				continue;

			Debug.Log(Input.GetKey(Actions[i].Input.Key1));

			return Actions[i].FixedState;
		}

		Debug.LogError($"Action \"{name}\" was not found!");
		return 0f;
	}
	public static bool GetFixedInputDown(string name)
	{
		for (int i = 0; i < Actions.Length; i++)
		{
			if (Actions[i].Name != name)
				continue;

			return Actions[i].FixedState == 1f && Actions[i].LastFixedState == 0f;
		}

		return false;
	}
	public static bool GetFixedInputUp(string name)
	{
		for (int i = 0; i < Actions.Length; i++)
		{
			if (Actions[i].Name != name)
				continue;

			return Actions[i].FixedState == 0f && Actions[i].LastFixedState == 1f;
		}

		Debug.LogError($"Action \"{name}\" was not found!");
		return false;
	}
}
