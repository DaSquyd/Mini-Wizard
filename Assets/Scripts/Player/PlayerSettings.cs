using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fire and Ice/Player/Player Settings")]
public class PlayerSettings : ScriptableObject
{
	[Header("Health")]
	public int MaxHealth;
	public float InvulnerabilityTime = 2f;
	public float StunTime = 1f;

	[Header("Movement")]
	public float Speed;
	public float GroundAcceleration;
	public float AirAcceleration;
	public float TurnSpeed;
	public float LockedTurnSpeed;
	public float MaxSlope;
	
	public float MaxFallSpeed;
	public byte MaxAirJumps;
	public float GroundJumpStrength;
	public float AirJumpStrength;
	public float JumpCooldown;
	public float JumpForgiveness;
	public float Gravity;

	[Header("Melee Attack")]
	public float AttackDownForce;
	public float AttackTurnSpeed;
	[System.Serializable]
	public struct Attack
	{
		public float Wait;
		public float ComboForgiveness;
		public float Force;
		public float Time;
		public int Damage;
	}
	public Attack[] AttackData;


	[Header("Projectile Attack")]
	public float TargetingMaxDistance;
	public float TargetingMaxAngle;
	public float TargetingDistanceWeight;


	[Header("Camera Controls")]
	public float CameraJoystickSpeed;
	public float CameraMouseSpeed;

	[System.Serializable]
	public struct CameraSettings
	{
		public float AutoCooldown;
		public float AutoCooldownResetAir;
		public float AutoCooldownResetMove;
		public float SettingToAutoCooldown;
		public float AutoSpeed;
		public float SettingToAutoSpeed;
		public float AutoFineTuneAngle;
		public float SettingToAutoFineTuneAngle;
		public float TrackAngle;
		public float DistanceAngle;
		public float ActivationMinDistance;
		public float ActivationMaxDistance;

		public float MaxCameraRelativeHeight;
		public float MinCameraRelativeHeight;
		public float CameraSmoothTime;
	}
	[Header("Camera Settings")]
	public CameraSettings Camera;
}
