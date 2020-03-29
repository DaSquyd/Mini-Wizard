using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fire and Ice/Player/Camera Settings")]
public class CameraSettings : ScriptableObject
{
	public float CameraSpeed;

	public float Fov;
	public float AttackFov;

	public float MinOffset;
	public float MaxOffset;

	public float MinOffsetPitch;
	public float MaxOffsetPitch;

	public float MinDistance;
	public float MaxDistance;

	public float MinAngle;
	public float MaxAngle;

	public float Buffer;
}
