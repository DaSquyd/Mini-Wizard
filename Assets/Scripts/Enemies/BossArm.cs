using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DebugIgnore]
public class BossArm : Entity
{
	Boss boss;
	public SphereCollider Collider
	{
		get; private set;
	}

	protected override void OnStart()
	{
		boss = GetComponentInParent<Boss>();
		Collider = GetComponent<SphereCollider>();

		MaxHealth = 10000;
		Health = 10000;
	}

	protected override void OnReceiveDamage(Entity attacker, int amount, Vector3 direction, DamageType type, Element sourceElement)
	{
		boss.ApplyDamage(attacker, amount, direction, type, sourceElement);
	}
}
