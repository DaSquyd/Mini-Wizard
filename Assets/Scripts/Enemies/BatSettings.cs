using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fire and Ice/Enemy/Bat Settings")]
public class BatSettings : ScriptableObject
{
	public int MaxHealth = 24;
	public float TurnSpeed = 180f;

	[Header("Idle")]
	public float IdleMoveSpeed = 1f;
	public float MaxVerticalOffset = 1.2f;
	public float MaxDistance = 5f;
	public float IdleWaitMin = 1f;
	public float IdleWaitMax = 4f;

	[Header("Aggressive")]
	public float AggressiveMoveSpeed = 2f;
	public float AggressiveSideSpeed = 5f;
	public float AggressiveMaxDistance = 8f;
	public float AggressiveMinDistance = 4f;
	public float AggressiveMaxVerticalOffset = 2f;
	public float AggressiveMinVerticalOffset = 0f;
	public float AggressiveWaitMin = 0.75f;
	public float AggressiveWaitMax = 1.5f;

	[Header("Projectile")]
	public ProjectileController FireProjectile;
	public ProjectileController IceProjectile;
	public float ProjectileChargeTime = 0.9f;

	[Header("Swoop")]
	public float SwoopChargeTime = 0.9f;

	[Header("Sensing")]
	public float SightMaxDistance = 16f;
	public float SightAngle = 170f;
	public float SensingRadius = 5f;
}
