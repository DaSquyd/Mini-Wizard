using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DebugIgnore]
public class BossBody : Entity
{
	Boss boss;

    // Start is called before the first frame update
    protected override void OnStart()
    {
		MaxHealth = 10000;
		Health = 10000;

		boss = GetComponentInParent<Boss>();
    }

	protected override void OnReceiveDamage(Entity attacker, int amount, Vector3 direction, DamageType type, Element sourceElement)
	{
		boss.ApplyDamage(attacker, amount, direction, type, sourceElement);
	}
}
