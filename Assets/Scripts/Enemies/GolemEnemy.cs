using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GolemEnemy : Entity
{
	public GolemSettings Settings;

	public bool IsTargeting
	{
		get; private set;
	}

	enum State
	{
		Idle,
		Walk,
		Notice,
		Run,
		Attack,
		Stunned
	}
	[DebugDisplay]
	State state = State.Idle;

	Vector3 origin;

	NavMeshAgent agent;
	Rigidbody rb;

	Animator animator;

	protected override void Start()
	{
		base.Start();

		MaxHealth = Settings.MaxHealth;
		Health = MaxHealth;

		origin = transform.position;

		agent = GetComponent<NavMeshAgent>();
		agent.acceleration = Settings.Acceleration;

		rb = GetComponent<Rigidbody>();
		rb.isKinematic = true;

		animator = GetComponentInChildren<Animator>();
	}

	protected override void Update()
	{
		base.Update();

		if (PlayerController.Instance == null)
			return;

		RaycastHit[] coneHit = ConeCast.ConeCastAll(transform.position, transform.forward, Settings.SightMaxDistance, Settings.SightAngle, LayerMask.GetMask("Default", "Terrain", "Player"));

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
		{
			IsTargeting = true;
		}

		switch (state)
		{
			case State.Idle:
				agent.speed = 0f;
				animator.SetInteger("State", 0);

				if (!findingNewLocation)
				{
					findNewLocationCoroutine = StartCoroutine(FindNewLocation());
				}

				if (IsTargeting)
				{
					if (findNewLocationCoroutine != null)
						StopCoroutine(findNewLocationCoroutine);

					state = State.Notice;
				}
				break;
			case State.Walk:
				agent.speed = Settings.WalkSpeed;
				animator.SetFloat("Speed", 1f);
				animator.SetInteger("State", 1);

				if (Vector3.Distance(transform.position, agent.destination) <= 0.5f)
				{
					state = State.Idle;
				}

				if (IsTargeting)
					state = State.Notice;
				break;
			case State.Notice:
				agent.speed = 0f;
				animator.SetInteger("State", 0);

				if (!noticing)
				{
					if (noticeWaitCoroutine != null)
						StopCoroutine(noticeWaitCoroutine);

					noticeWaitCoroutine = StartCoroutine(NoticeWait());
				}

				break;
			case State.Run:
				agent.SetDestination(PlayerController.Instance.transform.position);
				agent.speed = Settings.WalkSpeed * Settings.RunMultiplier;
				animator.SetFloat("Speed", Settings.RunMultiplier);
				animator.SetInteger("State", 1);

				if (!IsTargeting)
				{
					state = State.Idle;
					findingNewLocation = false;
				}
				Vector3 playerPosition = PlayerController.Instance.transform.position;
				if (Vector3.Distance(transform.position, playerPosition) <= 2f && Quaternion.Angle(transform.rotation, Quaternion.LookRotation(playerPosition - transform.position)) < 25f)
				{
					state = State.Attack;
				}
				break;
			case State.Attack:
				agent.speed = 0f;
				animator.SetInteger("State", 2);

				if (!attackWaiting)
				{
					attackWaiting = false;
					attackVelocity = 0f;
					attackWaitCoroutine = StartCoroutine(AttackWait());
				}
				break;
		}
	}

	bool overCliff = false;
	void FixedUpdate()
	{
		if (state == State.Attack && attackVelocity > 0f)
		{
			transform.position = transform.position + transform.forward * attackVelocity * Time.fixedDeltaTime;
			attackVelocity = Mathf.MoveTowards(attackVelocity, 0f, Time.fixedDeltaTime);
		}

		if (state == State.Stunned)
		{
			agent.speed = 0f;
			// Change to 4 later
			animator.SetInteger("State", 0);
			agent.updatePosition = false;
			rb.isKinematic = false;
			bool hit = Physics.Raycast(transform.position, Vector3.down, LayerMask.GetMask("Terrain"));
			if (hit)
			{
				agent.nextPosition = transform.position;
			}
			else
			{
				overCliff = true;
			}

			if (PlayerController.Instance.IsLocked && playerAttackIndex == PlayerController.Instance.CurrentMeleeAttack)
			{
				rb.velocity = PlayerController.Instance.Rigidbody.velocity * 1f;
				//transform.position = PlayerController.Instance.transform.position + hitRelative;
			}
		}
		else if (!overCliff)
		{
			agent.updatePosition = true;
			rb.isKinematic = true;
		}
	}

	Coroutine findNewLocationCoroutine;
	bool findingNewLocation;
	IEnumerator FindNewLocation()
	{
		findingNewLocation = true;
		yield return new WaitForSeconds(Random.Range(Settings.IdleWaitMin, Settings.IdleWaitMax));

		agent.SetDestination(RandomNavmeshLocation(Random.Range(Settings.IdleRadiusMin, Settings.IdleRadiusMax)));

		state = State.Walk;
		findingNewLocation = false;
	}

	Coroutine noticeWaitCoroutine;
	bool noticing;
	IEnumerator NoticeWait()
	{
		noticing = true;
		yield return new WaitForSeconds(Random.Range(Settings.NoticeTimeMin, Settings.NoticeTimeMax));

		noticing = false;

		if (IsTargeting)
		{
			state = State.Run;
		}
		else
		{
			state = State.Idle;
			findingNewLocation = false;
		}
	}

	Coroutine attackWaitCoroutine;
	bool attackWaiting;
	float attackVelocity;
	IEnumerator AttackWait()
	{
		attackWaiting = true;
		attackVelocity = 0f;

		yield return new WaitForSeconds(1.4f);

		attackVelocity = 3f;

		yield return new WaitForSeconds(0.6f);

		state = State.Run;
		attackWaiting = false;
	}

	IEnumerator Stunned()
	{
		yield return new WaitForSeconds(1f);

		state = State.Run;
	}

	public Vector3 RandomNavmeshLocation(float radius)
	{
		Vector3 randomDirection = Random.insideUnitCircle.normalized * radius;
		randomDirection.z = randomDirection.y;
		randomDirection.y = 0f;
		randomDirection += origin;
		Vector3 finalPosition = Vector3.zero;
		if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, radius, 1))
		{
			finalPosition = hit.position;
		}
		return finalPosition;
	}

	Vector3 hitRelative;
	[DebugDisplay] byte playerAttackIndex;
	protected override void OnReceiveDamage(Entity attacker, int amount, Vector3 direction, DamageType type, Element sourceElement)
	{
		if (type == DamageType.Melee)
		{
			agent.speed = 0f;
			// Change to 4 later
			animator.SetInteger("State", 0);
			agent.updatePosition = false;
			rb.isKinematic = false;

			//transform.position = PlayerController.Instance.transform.position + hitRelative;
			if (Vector3.Distance(PlayerController.Instance.transform.position, transform.position) < 1.5)
			{
				float dist = 1.75f;
				bool hit = Physics.SphereCast(transform.position, 0.75f, PlayerController.Instance.MeshContainer.transform.forward, out RaycastHit hitInfo, dist, LayerMask.GetMask("Default", "Terrain"));
				if (hit)
					dist = hitInfo.distance;
				transform.position = transform.position + PlayerController.Instance.MeshContainer.transform.forward * (dist - 0.75f);
				rb.velocity = PlayerController.Instance.Rigidbody.velocity;
			}

			playerAttackIndex = PlayerController.Instance.CurrentMeleeAttack;

			StopAllCoroutines();

			findingNewLocation = false;
			noticing = false;
			attackWaiting = false;

			StartCoroutine(Stunned());
			state = State.Stunned;
			hitRelative = transform.position - PlayerController.Instance.transform.position;
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		OnHit(collision);
	}

	public void OnHit(Collision collision)
	{
		if (collision.gameObject.tag == "Player" && attackVelocity > 0f && state == State.Attack)
		{
			PlayerController.Instance.ApplyDamage(this, 1, collision.impulse.normalized * -5f, DamageType.Melee, Element.None);
		}
	}
}
