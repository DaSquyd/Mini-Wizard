using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fire and Ice/Player/Projectile Settings")]
public class ProjectileSettings : ScriptableObject
{
	public float speed;
	public float rotationSpeed;
	public float maxRotationAngle;
	public float damage;
	public float lifetime;
}
