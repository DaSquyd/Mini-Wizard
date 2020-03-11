using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fire and Ice/Player/Player Settings")]
public class PlayerSettings : ScriptableObject
{
	[System.Serializable]
	public struct AttackCooldown
	{
		public float wait;
		public float comboForgiveness;
		public float force;
		public float time;
	}

	[System.Serializable]
	public struct CameraSettings
	{
		public float autoCooldown;
		public float settingToAutoCooldown;
		public float autoSpeed;
		public float settingToAutoSpeed;
		public float autoFineTuneAngle;
		public float settingToAutoFineTuneAngle;
		public float trackAngle;
		public float distanceAngle;
		public float activationMinDistance;
		public float activationMaxDistance;
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

	public CameraSettings camera;
}
