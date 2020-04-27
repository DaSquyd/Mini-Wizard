using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class Entity : ToonLitObject
{
	public int MaxHealth
	{
		get; protected set;
	}

	int _health;
	[DebugDisplay]
	public int Health
	{
		get
		{
			return _health;
		}
		set
		{
			_health = Mathf.Clamp(value, 0, MaxHealth);
		}
	}

	public bool Invincible
	{
		get;
		set;
	}

	public Element Element;
	protected sealed override void Start()
	{
		base.Start();
#if UNITY_EDITOR
		if (DebugCanvas.Instance != null)
		{
			DebugCanvas.Instance.Reload();
		}
#endif

		OnStart();
	}
	protected virtual void OnStart()
	{
	}
	
	protected sealed override void Update()
	{
		base.Update();

		if (transform.position.y <= -15)
		{
			Destroy(gameObject);
		}

		OnUpdate(Time.deltaTime);
	}
	protected virtual void OnUpdate(float deltaTime)
	{
	}

	protected sealed override void FixedUpdate()
	{
		OnFixedUpdate(Time.fixedDeltaTime);
	}
	protected virtual void OnFixedUpdate(float deltaTime)
	{
	}

	private Entity lastAttacker;
	protected float lastDamage = 0f;
	public float ApplyDamage(Entity attacker, int amount, Vector3 direction, DamageType type, Element sourceElement)
	{
		if (Invincible)
			return Health;

		lastAttacker = attacker;

		int mult = 1;

		if ((sourceElement == Element.Fire && Element == Element.Ice)
			|| (sourceElement == Element.Ice && Element == Element.Fire))
			mult = 3;

		int oldHealth = Health;
		Health = (int) Mathf.MoveTowards(Health, 0f, amount * mult);
		lastDamage = oldHealth - Health;

		OnReceiveDamage(attacker, amount * mult, direction, type, sourceElement);

		if (Health == 0f)
		{
			OnDeath();
		}

		return Health;
	}

	protected virtual void OnDeath()
	{
#if UNITY_EDITOR
		DebugCanvas.Instance.Reload();
#endif
		Destroy(gameObject);
	}

	public enum DamageType
	{
		Melee,
		Projectile,
		Other
	}

	protected virtual void OnReceiveDamage(Entity attacker, int amount, Vector3 direction, DamageType type, Element sourceElement)
	{
	}
}