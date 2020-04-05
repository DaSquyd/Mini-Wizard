using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fire and Ice/Enemy/Enemy Settings")]
public class EnemySettings : ScriptableObject
{
	public int MaxHealth;

	public int ProjectileDamage;
	public int MeleeDamage;


	[Header("Sensing")]
	public float sightMaxDistance;
	public float sightAngle;
	public float sensingRadius;
}
