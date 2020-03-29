using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fire and Ice/Player/Projectile Settings")]
public class ProjectileSettings : ScriptableObject
{
	public float Speed;
	public float RotationSpeed;
	public float MaxRotationAngle;
	public int Damage;
	public float Lifetime;
}
