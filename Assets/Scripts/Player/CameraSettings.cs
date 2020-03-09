using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fire and Ice/Player/Camera Settings")]
public class CameraSettings : ScriptableObject
{
	public float verticalOffset;

	public float minDistance;
	public float maxDistance;

	public float minAngle;
	public float maxAngle;

	public float buffer;
}
