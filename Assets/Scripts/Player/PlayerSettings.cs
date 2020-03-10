using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fire and Ice/Player/Player Settings")]
public class PlayerSettings : ScriptableObject
{
	[System.Serializable]
	public struct AttackCooldown
	{
		public float _wait;
		public float _comboForgiveness;
		public float _force;
		public float _time;
	}


	public float wallPush;

	public float speed;
	public float groundAcceleration;
	public float airAcceleration;
	public float turnSpeed;
	public float lockedTurnSpeed;
	public float maxSlope;

	public float maxFallSpeed;
	public byte maxAirJumps;
	public float groundJumpStrength;
	public float airJumpStrength;
	public float jumpCooldown;
	public float jumpForgiveness;
	public float gravity;

	public float attackDownForce;
	public float attackTurnSpeed;
	public AttackCooldown[] attackCooldowns;


	public float cameraJoystickSpeed;
	public float cameraMouseSpeed;

	public bool autoCameraEnabled;
	public float autoCameraCooldown;
	public float autoCameraSpeed;
	public float manualCameraResetSpeed;
}
