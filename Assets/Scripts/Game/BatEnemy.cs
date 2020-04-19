﻿using System.Collections;
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

	private void FixedUpdate()
	{
		switch (attackState)
		{
			case State.Idle:
				if (IsTargeting)
					attackState = State.Aggressive;
				else
					IdleFixedUpdate();
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
	void IdleFixedUpdate()
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
			waitTime = Mathf.MoveTowards(waitTime, 0f, Time.fixedDeltaTime);

			if (waitTime == 0f)
				waiting = false;
		}
		else
		{
			move = destination - position;

			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(destination - position), Settings.turnSpeed * Time.fixedDeltaTime);
			transform.position = Vector3.MoveTowards(position, destination, Settings.idleMoveSpeed * Time.fixedDeltaTime);
		}
	}

	int moveDirection = 0;


	void AggressiveUpdate()
	{
		Transform playerTransform = PlayerController.Instance.transform;

		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(PlayerController.Instance.transform.position - transform.position), Settings.turnSpeed * Time.deltaTime);

		if (Vector3.Distance(PlayerController.Instance.transform.position, transform.position) < Settings.aggressiveMinDistance)
		{
			transform.position = Vector3.MoveTowards(transform.position, playerTransform.position - transform.position, Settings.aggressiveMoveSpeed * Time.deltaTime);
		}

		if (transform.position.y - playerTransform.position.y < Settings.aggressiveMinVerticalOffset)
		{
			transform.position = transform.position + Vector3.up * Settings.aggressiveMoveSpeed * Time.deltaTime;
		}
		else if (transform.position.y - playerTransform.position.y > Settings.aggressiveMaxVerticalOffset)
		{
			transform.position = transform.position + Vector3.down * Settings.aggressiveMoveSpeed * Time.deltaTime;
		}

		if (waiting)
		{
			if (moveDirection != 0)
			{
				transform.position = transform.position + transform.right * Settings.aggressiveMoveSpeed * moveDirection * Time.deltaTime;
			}

			waitTime = Mathf.MoveTowards(waitTime, 0f, Time.deltaTime);

			if (waitTime == 0f)
				waiting = false;
		}
		else
		{
			waitTime = Random.Range(Settings.aggressiveWaitMin, Settings.aggressiveWaitMax);
			waiting = true;
			if (moveDirection == 0)
				moveDirection = Random.value < 0.5f ? -1 : 1;
			else
				moveDirection = 0;
		}
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
