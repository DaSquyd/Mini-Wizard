using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossArm : Entity
{
	

	Boss boss;

	protected override void Start()
	{
		boss = GetComponentInParent<Boss>();

		MaxHealth = 10000;
		Health = 10000;
	}

	protected override void OnReceiveDamage(Entity attacker, int amount, Vector3 direction, DamageType type, Element sourceElement)
	{
		boss.ApplyDamage(attacker, amount, direction, type, sourceElement);
	}
}
