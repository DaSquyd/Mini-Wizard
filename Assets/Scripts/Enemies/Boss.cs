using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Entity
{
	public BossSettings Settings;

	public enum State
	{
		Preassemble,
		Assemble,
		IdleTurn,
		PrepareAttack,
		Throw,
		LeftAttack,
		RightAttack,
		DoubleAttack,
		Death
	}
	[DebugDisplay("State")] State _state;
	public void SetState(State newState)
	{
		Debug.Log("State Change: " + newState);
		switch (newState)
		{
			case State.Preassemble:
				animator.SetFloat("AssembleSpeed", 0f);
				animator.SetInteger("State", 0);
				animator.SetTrigger("Reset");
				break;
			case State.Assemble:
				animator.SetFloat("AssembleSpeed", 0f);
				assembleSpeed = 0f;
				animator.SetInteger("State", 0);
				animator.SetTrigger("Assemble");
				break;
			case State.IdleTurn:
				animator.SetInteger("State", 1);
				Debug.Log(animator.GetInteger("State"));
				if (selectAttackCoroutine != null)
					StopCoroutine(selectAttackCoroutine);
				selectAttackCoroutine = StartCoroutine(SelectAttack());
				break;
			case State.Throw:
				animator.SetInteger("State", 0);
				break;
			case State.LeftAttack:
				break;
			case State.RightAttack:
				break;
			case State.DoubleAttack:
				break;
			case State.Death:
				break;
		}

		_state = newState;
		return;
	}

	Animator animator;

	float assembleSpeed = 0f;

	float yawCurrent;
	float yawTarget;
	[DebugDisplay] float yawVelocity;

	PlayerController player;
	Transform playerTransform;

	bool isDead;

	protected override void Start()
	{
		base.Start();

		MaxHealth = Settings.MaxHealth;
		Health = MaxHealth;

		animator = GetComponentInChildren<Animator>();

		Invincible = true;
	}

	protected override void Update()
	{
#if UNITY_EDITOR
		if (Input.anyKeyDown)
		{
			for (int i = 0; i <= 8; i++)
			{
				if (Input.GetKeyDown((KeyCode) i + 48))
				{
					SetState((State) i);
				}
			}
		}
#endif

		base.Update();

		if (isDead)
			return;
		if (player == null)
		{
			PlayerController player = PlayerController.Instance;

			if (player == null)
			{
				return;
			}
			else
			{
				playerTransform = player.transform;
			}
		}

		switch (_state)
		{
			case State.Preassemble:

				break;

			case State.Assemble:
				assembleSpeed = Mathf.MoveTowards(assembleSpeed, Settings.AssembleMaxSpeed, Settings.AssembleAcceleration * Time.deltaTime);
				animator.SetFloat("AssembleSpeed", assembleSpeed);
				break;

			case State.IdleTurn:
				UpdateYaw(1f);
				transform.rotation = Quaternion.Euler(0f, yawCurrent, 0f);
				animator.SetFloat("WalkSpeed", Mathf.Min(Mathf.Abs(yawVelocity), 20f) * 0.1f);

				if (Mathf.Abs(yawVelocity) < 5f)
				{
					animator.SetInteger("State", 1);
				}
				else
				{
					animator.SetInteger("State", 2);
				}

				break;

			case State.Throw:

				break;

			case State.LeftAttack:
				break;

			case State.RightAttack:
				break;

			case State.DoubleAttack:
				break;

			case State.Death:
				break;
		}
	}

	public void UpdateYaw(float multiplier)
	{
		yawTarget = Quaternion.LookRotation(playerTransform.position - transform.position).eulerAngles.y;
		yawCurrent = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, yawTarget, ref yawVelocity, Settings.TurnTime / multiplier, Settings.TurnSpeed * multiplier);
	}
	
	int[] lastAttacks = new int[2];
# if UNITY_EDITOR
	[DebugDisplay]
	int Last1
	{
		get
		{
			return lastAttacks[0];
		}
	}
	[DebugDisplay]
	int Last2
	{
		get
		{
			return lastAttacks[1];
		}
	}
#endif
	Coroutine selectAttackCoroutine;
	IEnumerator SelectAttack()
	{
		yield return new WaitForSeconds(Random.Range(Settings.AttackMinWait, Settings.AttackMaxWait));

		int selection = 0;

		for (int i = 1; i <= 3; i++)
		{
			if (lastAttacks[0] == i && lastAttacks[1] == i)
			{
				selection = SelectRandom(i % 3 + 1, (i + 1) % 3 + 1);
			}
		}

		if (selection == 0)
		{
			selection = SelectRandom(1, 2, 3);
		}

		lastAttacks[1] = lastAttacks[0];
		lastAttacks[0] = selection;

		animator.SetInteger("Attack", selection);
	}
	int SelectRandom(params int[] ints)
	{
		float rand = Random.value;
		for (int i = 0; i < ints.Length; i++)
		{
			Debug.Log((float) i / ints.Length);
			if (rand <= (float) i / ints.Length)
			{
				return i;
			}
		}
		return ints.Length - 1;
	}

	public void EnterIdleState()
	{

	}
}
