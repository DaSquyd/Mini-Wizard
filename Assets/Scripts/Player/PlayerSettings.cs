using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fire and Ice/Player/Player Settings")]
public class PlayerSettings : ScriptableObject
{
	public float wallPush;

	public float speed;
	public float groundAcceleration;
	public float airAcceleration;
	public float turnSpeed;
	public float maxSlope;

	public float maxFallSpeed;
	public float jumpStrength;
	public float jumpCooldown;
	public float jumpForgiveness;
	public float gravity;

	public float cameraJoystickSpeed;
	public float cameraMouseSpeed;
}
