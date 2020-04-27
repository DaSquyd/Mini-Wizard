using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fire and Ice/Enemy/Boss Settings")]
public class BossSettings : ScriptableObject
{
	public int MaxHealth = 500;
	public float AssembleAcceleration = 0.1f;
	public float AssembleMaxSpeed = 1f;

	[Header("Idle/Turn")]
	public float TurnSpeed = 180f;
	public float TurnMultiplier = 2f;
	public float TurnTime = 1f;

	[Header("Attack")]
	public float AttackMinWait = 4f;
	public float AttackMaxWait = 8f;
}
