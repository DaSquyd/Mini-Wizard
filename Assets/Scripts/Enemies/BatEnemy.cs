using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatEnemy : Entity
{
	public BatSettings Settings;

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
		Stunned,
		Dying
	}
	State state = State.Idle;

	Vector3 origin;
	
	Vector3 move;
	new SphereCollider collider;

	Rigidbody rb;

	// Start is called before the first frame update
	protected override void Start()
	{
		base.Start();
		
		MaxHealth = Settings.MaxHealth;
		Health = MaxHealth;

		origin = transform.position;

		collider = GetComponent<SphereCollider>();

		rb = GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	protected override void Update()
	{
		base.Update();

		RaycastHit[] coneHit = ConeCast.ConeCastAll(transform.position, transform.forward, Settings.SightMaxDistance, Settings.SightAngle, LayerMask.GetMask("Default"));

		IsTargeting = false;
		for (int i = 0; i < coneHit.Length; i++)
		{
			if (coneHit[i].collider.tag == "Player")
			{
				IsTargeting = true;
				break;
			}
		}

		if (PlayerController.Instance != null && Vector3.Distance(transform.position, PlayerController.Instance.transform.position) <= Settings.SensingRadius)
			IsTargeting = true;

		switch (state)
		{
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

		if (state != State.Stunned)
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}
	}

	private void FixedUpdate()
	{
		switch (state)
		{
			case State.Idle:
				if (IsTargeting)
					state = State.Aggressive;
				else
					IdleFixedUpdate();
				break;
			case State.Aggressive:
				if (IsTargeting)
					AggressiveFixedUpdate();
				else
					state = State.Idle;
				break;
			case State.Stunned:
				StunnedFixedUpdate();
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
			destination = Random.insideUnitCircle * Settings.MaxDistance;
			destination.z = destination.y;
			destination.y = Random.Range(-Settings.MaxVerticalOffset, Settings.MaxVerticalOffset);
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
			waitTime = Random.Range(Settings.IdleWaitMin, Settings.IdleWaitMax);
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

			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(destination - position), Settings.TurnSpeed * Time.fixedDeltaTime);
			transform.position = Vector3.MoveTowards(position, destination, Settings.IdleMoveSpeed * Time.fixedDeltaTime);
		}
	}

	int moveDirectionH = 0;
	int moveDirectionV = 0;


	void AggressiveFixedUpdate()
	{
		Vector3 position = transform.position;
		Quaternion rotation = transform.rotation;
		Vector3 playerPosition = PlayerController.Instance.transform.position;

		transform.rotation = Quaternion.RotateTowards(rotation, Quaternion.LookRotation(playerPosition - position), Settings.TurnSpeed * Time.fixedDeltaTime);

		float dist = Vector3.Distance(playerPosition, position);
		if (dist < Settings.AggressiveMinDistance)
		{
			transform.position = Vector3.MoveTowards(position, playerPosition - position, Settings.AggressiveMoveSpeed * Time.fixedDeltaTime);
		}
		else if (dist > Settings.AggressiveMaxDistance)
		{
			transform.position = Vector3.MoveTowards(position, playerPosition - position, Settings.AggressiveMoveSpeed * Time.fixedDeltaTime);
		}

		if (position.y - playerPosition.y < Settings.AggressiveMinVerticalOffset)
		{
			transform.position = position + Vector3.up * Settings.AggressiveMoveSpeed * Time.fixedDeltaTime;
		}
		else if (position.y - playerPosition.y > Settings.AggressiveMaxVerticalOffset)
		{
			transform.position = transform.position + Vector3.down * Settings.AggressiveMoveSpeed * Time.fixedDeltaTime;
		}

		if (waiting)
		{
			if (moveDirectionH != 0 && dist > Settings.AggressiveMinDistance)
			{
				transform.position = position + transform.right * Settings.AggressiveSideSpeed * moveDirectionH * Time.fixedDeltaTime;
			}

			if (moveDirectionV != 0 && dist > Settings.AggressiveMinDistance)
			{
				transform.position = position + transform.up * Settings.IdleMoveSpeed * moveDirectionV * Time.fixedDeltaTime;
			}

			waitTime = Mathf.MoveTowards(waitTime, 0f, Time.fixedDeltaTime);

			if (waitTime == 0f)
			{
				float rand = Random.value;

				if (rand <= 0.2f)
				{
					state = State.ChargingProjectile;
					waitTime = Settings.ProjectileChargeTime;
				}
				else if (rand <= 0.4f)
				{
					state = State.ChargingSwoop;
					waitTime = Settings.SwoopChargeTime;
				}

				waiting = false;
			}
		}
		else
		{
			waitTime = Random.Range(Settings.AggressiveWaitMin, Settings.AggressiveWaitMax);
			waiting = true;
			if (moveDirectionH == 0)
				moveDirectionH = Random.value < 0.5f ? -1 : 1;
			else
				moveDirectionH = 0;
		}
	}

	void ChargineProjectileUpdate()
	{
		if (waiting)
		{
			waitTime = Mathf.MoveTowards(waitTime, 0f, Time.fixedDeltaTime);

			if (waitTime == 0f)
			{
				waiting = false;
			}
		}
		else
		{
			waitTime = Random.Range(Settings.AggressiveWaitMin, Settings.AggressiveWaitMax);
			state = State.ShootingProjectile;
			waiting = true;
		}
	}

	public Transform ProjectileOrigin;
	void ShootingProjectileUpdate()
	{
		Debug.Log("Shoot!");
		state = State.Aggressive;
		ProjectileController projectile = Instantiate(Settings.FireProjectile, ProjectileOrigin.position, transform.rotation);
		projectile.Owner = this;
	}

	void ChargingSwoopUpdate()
	{
		if (waiting)
		{
			waitTime = Mathf.MoveTowards(waitTime, 0f, Time.fixedDeltaTime);

			if (waitTime == 0f)
			{
				waiting = false;
			}
		}
		else
		{
			waitTime = Random.Range(Settings.AggressiveWaitMin, Settings.AggressiveWaitMax);
			state = State.Swooping;
			waiting = true;
		}
	}

	void SwoopingUpdate()
	{
		Debug.Log("Swoop");
		state = State.Aggressive;
	}

	public int stunAttack;
	void StunnedFixedUpdate()
	{
		if (stunAttack == PlayerController.Instance.CurrentMeleeAttack)
		{
			Vector3 velocity = PlayerController.Instance.Rigidbody.velocity;
			rb.velocity = velocity + velocity.normalized;
		}
	}

	void DyingUpdate()
	{

	}

	protected override void OnReceiveDamage(Entity attacker, int amount, Vector3 direction, Element sourceElement)
	{
		Debug.Log("Damage Taken!");

		rb.AddForce(direction * 5f);

		state = State.Stunned;
		StopAllCoroutines();
		StartCoroutine(Stun());
	}

	IEnumerator Stun()
	{
		yield return new WaitForSeconds(1f);

		state = State.Aggressive;
		Debug.Log("Return to State");
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Player" && state != State.Stunned)
		{
			PlayerController.Instance.ApplyDamage(this, 1, collision.impulse.normalized * -1f, Element.None);
		}
	}
}
