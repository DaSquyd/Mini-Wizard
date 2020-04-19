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
	[DebugDisplay]
	State attackState = State.Idle;

	Vector3 origin;

	[DebugDisplay]
	Vector3 move;
	new SphereCollider collider;

	// Start is called before the first frame update
	protected override void Start()
	{
		base.Start();


		MaxHealth = Settings.MaxHealth;
		Health = MaxHealth;

		origin = transform.position;

		collider = GetComponent<SphereCollider>();
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

		if (PlayerController.Instance != null && Vector3.Distance(transform.position, PlayerController.Instance.transform.position) <= Settings.sensingRadius)
			IsTargeting = true;

		switch (attackState)
		{
			case State.Idle:
				if (IsTargeting)
					attackState = State.Aggressive;
				else
					IdleUpdate();
				break;
			case State.Aggressive:
				if (IsTargeting)
					AggressiveUpdate();
				else
					attackState = State.Idle;
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

	[DebugDisplay]
	Vector3 destination;
	[DebugDisplay]
	bool hasReachedDestination;
	[DebugDisplay]
	bool waiting;
	[DebugDisplay]
	float waitTime = 0f;

	[DebugDisplay]
	Vector3 hitLoc;
	void IdleUpdate()
	{
		Vector3 position = transform.position;
		if (destination == new Vector3() || hasReachedDestination)
		{
			destination = Random.insideUnitCircle * Settings.maxDistance;
			destination.z = destination.y;
			destination.y = Random.Range(-Settings.maxVerticalOffset, Settings.maxVerticalOffset);
			destination = destination + origin;

			bool hit = Physics.SphereCast(position, collider.radius, destination - position, out RaycastHit hitInfo, Vector3.Distance(destination, position) * 2f);

			Debug.DrawRay(position, (destination - position).normalized * Vector3.Distance(destination, position), Color.white, 5f);

			if (hit)
			{
				destination = hitInfo.point;
				hitLoc = hitInfo.point;
			}

			Debug.Log($"Set new dest: {destination}");

			hasReachedDestination = false;
		}


		if (Vector3.Distance(destination, position) <= collider.radius * 2f)
		{
			hasReachedDestination = true;
			waitTime = Random.Range(Settings.idleWaitMin, Settings.idleWaitMax);
			waiting = true;
		}

		if (waiting)
		{
			waitTime = Mathf.MoveTowards(waitTime, 0f, Time.deltaTime);

			if (waitTime == 0f)
				waiting = false;
		}
		else
		{
			move = destination - position;

			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(destination - position), Settings.turnSpeed * Time.deltaTime);
			transform.position = Vector3.MoveTowards(position, destination, Settings.idleMoveSpeed * Time.deltaTime);
		}
	}

	void AggressiveUpdate()
	{
		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(PlayerController.Instance.transform.position - transform.position), Settings.turnSpeed * Time.deltaTime);
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
