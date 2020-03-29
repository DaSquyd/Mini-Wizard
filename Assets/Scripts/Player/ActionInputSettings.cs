using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using InControl;

[CreateAssetMenu(menuName = "Fire and Ice/Player/Action Input Settings")]
public class ActionInputSettings : ScriptableObject
{
	[Serializable]
	public struct ActionInput
	{
		public string Name;
		public InputControlType Gamepad;
		public bool GamepadInverse;
		public KeyCode Key1;
		public KeyCode Key2;
	}

	public List<ActionInput> Inputs;
}
