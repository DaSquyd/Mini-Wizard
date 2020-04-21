using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fire and Ice/Enemy/Golem Settings")]
public class GolemSettings : ScriptableObject
{
	public int MaxHealth = 24;

	[Header("Sensing")]
	public float SightMaxDistance = 16f;
	public float SightAngle = 170f;
	public float SensingRadius = 5f;
}
