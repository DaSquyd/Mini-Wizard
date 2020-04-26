using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fire and Ice/Enemy/Boss Settings")]
public class BossSettings : ScriptableObject
{
	public int MaxHealth = 500;
	public float TurnSpeed = 180f;

}
