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
		Aggressive
	}
	State state = State.Idle;

	Vector3 origin;

	NavMeshAgent agent;

	protected override void Start()
	{
		base.Start();

		MaxHealth = Settings.MaxHealth;
		Health = MaxHealth;

		origin = transform.position;

		agent = GetComponent<NavMeshAgent>();
	}

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
	}

	private void FixedUpdate()
	{
		if (IsTargeting)
			agent.SetDestination(PlayerController.Instance.transform.position);
	}
}
