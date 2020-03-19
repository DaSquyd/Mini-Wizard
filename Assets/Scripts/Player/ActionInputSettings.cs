using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InControl;

[CreateAssetMenu(menuName = "Fire and Ice/Player/Action Input Settings")]
public class ActionInputSettings : ScriptableObject
{
	[System.Serializable]
	public struct ActionInput
	{
		public string name;
		public InputControlType gamepad;
		public bool gamepadInverse;
		public KeyCode key1;
		public KeyCode key2;
	}

	public List<ActionInput> inputs;
}
