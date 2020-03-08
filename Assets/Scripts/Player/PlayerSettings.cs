using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fire and Ice/Player/Player Settings")]
public class PlayerSettings : ScriptableObject
{
	public float speed;
	public float acceleration;

	public float maxFallSpeed;
	public float jumpStrength;
	public float jumpCooldown;
	public float gravity;

	public float cameraMinDistance;
	public float cameraMaxDistance;

	public float cameraMinAngle;
	public float cameraMaxAngle;

	public float cameraJoystickSpeed;
	public float cameraMouseSpeed;
}
