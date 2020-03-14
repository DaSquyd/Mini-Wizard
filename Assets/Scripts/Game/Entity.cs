using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
	public float MaxHealth
	{
		get; protected set;
	}

	float _health;
	public float Health
	{
		get
		{
			return _health;
		}
		set
		{
			_health = Mathf.Clamp(value, 0f, MaxHealth);
		}
	}

	public float ApplyDamageToEntity(Entity target, float amount)
	{
		target.Health -= amount;

		target.OnReceiveDamage(this, amount);

		return target.Health;
	}

	public virtual void OnReceiveDamage(Entity attacker, float damageAmount)
	{
	}
}