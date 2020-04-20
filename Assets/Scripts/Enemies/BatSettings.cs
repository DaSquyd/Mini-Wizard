using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fire and Ice/Enemy/Bat Settings")]
public class BatSettings : ScriptableObject
{
	public int MaxHealth;

	public int ProjectileDamage;
	public int MeleeDamage;
	public float turnSpeed = 180f;

	[Header("Idle")]
	public float idleMoveSpeed = 1f;
	public float maxVerticalOffset = 3f;
	public float maxDistance = 8f;
	public float idleWaitMin = 1f;
	public float idleWaitMax = 4f;

	[Header("Aggressive")]
	public float aggressiveMoveSpeed = 2f;
	public float aggressiveSideSpeed = 2f;
	public float aggressiveMaxDistance = 8f;
	public float aggressiveMinDistance = 4f;
	public float aggressiveMaxVerticalOffset = 3f;
	public float aggressiveMinVerticalOffset = 0.5f;
	public float aggressiveWaitMin = 0.75f;
	public float aggressiveWaitMax = 2.5f;

	[Header("Sensing")]
	public float sightMaxDistance;
	public float sightAngle;
	public float sensingRadius;
}
