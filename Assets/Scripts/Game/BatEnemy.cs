using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatEnemy : Entity
{
	public EnemySettings Settings;

	public bool IsTargeting
	{
		get; private set;
	}

	enum State
	{
		Idle,
		Aggressive,
		ChargingProjectile,
		ShootingProjectile,
		ChargingSwoop,
		Swooping,
		Dying
	}
	State attackState = State.Idle;

	// Start is called before the first frame update
	protected override void Start()
	{
		base.Start();


		MaxHealth = Settings.MaxHealth;
		Health = MaxHealth;
	}

	// Update is called once per frame
	protected override void Update()
	{
		base.Update();

		RaycastHit[] coneHit = ConeCast.ConeCastAll(transform.position, transform.forward, Settings.sightMaxDistance, Settings.sightAngle, LayerMask.GetMask("Default"));

		IsTargeting = false;
		for (int i = 0; i < coneHit.Length; i++)
		{
			if (coneHit[i].collider.tag == "Player")
			{
				IsTargeting = true;
				break;
			}
		}

		switch (attackState)
		{
			case State.Idle:
				IdleUpdate();
				break;
			case State.Aggressive:
				AggressiveUpdate();
				break;
			case State.ChargingProjectile:
				ChargineProjectileUpdate();
				break;
			case State.ShootingProjectile:
				ShootingProjectileUpdate();
				break;
			case State.ChargingSwoop:
				ChargingSwoopUpdate();
				break;
			case State.Swooping:
				SwoopingUpdate();
				break;
			case State.Dying:
				DyingUpdate();
				break;
		}
	}

	void IdleUpdate()
	{

	}

	void AggressiveUpdate()
	{

	}

	void ChargineProjectileUpdate()
	{

	}

	void ShootingProjectileUpdate()
	{

	}

	void ChargingSwoopUpdate()
	{

	}

	void SwoopingUpdate()
	{

	}

	void DyingUpdate()
	{

	}
}
