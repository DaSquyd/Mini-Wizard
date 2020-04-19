using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fire and Ice/Enemy/Enemy Settings")]
public class EnemySettings : ScriptableObject
{
	public int MaxHealth;

	public int ProjectileDamage;
	public int MeleeDamage;

	[Header("Movement")]
	public float idleMoveSpeed = 1f;
	public float maxVerticalOffset = 3f;
	public float maxDistance = 8f;
	public float idleWaitMin = 1f;
	public float idleWaitMax = 4f;
	public float turnSpeed = 180f;

	[Header("Sensing")]
	public float sightMaxDistance;
	public float sightAngle;
	public float sensingRadius;
}
