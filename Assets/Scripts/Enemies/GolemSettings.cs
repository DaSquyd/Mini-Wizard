using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fire and Ice/Enemy/Golem Settings")]
public class GolemSettings : ScriptableObject
{
	public int MaxHealth = 24;

	[Header("Idle")]
	public float IdleRadiusMin = 3f;
	public float IdleRadiusMax = 7f;
	public float IdleWaitMin = 1f;
	public float IdleWaitMax = 3f;

	[Header("Movement")]
	public float WalkSpeed = 1f;
	public float RunMultiplier = 5f;
	public float Acceleration = 20f;

	[Header("Sensing")]
	public float SightMaxDistance = 16f;
	public float SightAngle = 170f;
	public float SensingRadius = 5f;
	public float NoticeTimeMin = 0.9f;
	public float NoticeTimeMax = 1.1f;
}
