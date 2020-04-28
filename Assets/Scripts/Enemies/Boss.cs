using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : Entity
{
	public BossSettings Settings;

	public enum State
	{
		Preassemble,
		Assemble,
		IdleTurn,
		PrepareAttack,
		Attack,
		Death
	}
	[DebugDisplay("State")] State _state = State.Preassemble;
	public void SetState(State newState)
	{
		Color c = HealthBack.color;
		Debug.Log("State Change: " + newState);
		switch (newState)
		{
			case State.Preassemble:
				animator.SetFloat("AssembleSpeed", 0f);
				animator.SetInteger("State", 0);
				animator.SetTrigger("Reset");
				HealthCanvas.enabled = false;
				HealthBar.gameObject.SetActive(false);
				HealthBar.fillAmount = 0f;
				HealthBack.color = new Color(c.r, c.g, c.b, 1f);
				break;
			case State.Assemble:
				animator.SetFloat("AssembleSpeed", 0f);
				assembleSpeed = 0f;
				animator.SetInteger("State", 0);
				animator.SetTrigger("Assemble");
				HealthCanvas.enabled = true;
				HealthBar.gameObject.SetActive(false);
				HealthBar.fillAmount = 0f;
				HealthBack.color = new Color(c.r, c.g, c.b, 1f);
				break;
			case State.IdleTurn:
				animator.SetInteger("State", 1);
				if (selectAttackCoroutine != null)
					StopCoroutine(selectAttackCoroutine);
				selectAttackCoroutine = StartCoroutine(SelectAttack());
				animator.SetInteger("Attack", 0);
				break;
			case State.Attack:
				animator.SetInteger("State", 3);
				break;
			case State.Death:
				isDead = true;
				animator.SetBool("IsDead", true);
				animator.SetInteger("Attack", 0);
				break;
		}

		_state = newState;
		return;
	}

	public BossArm LeftArm;
	public BossArm RightArm;

	public Canvas HealthCanvas;
	public Image HealthBar;
	public Image HealthBack;

	public CutsceneHandler DeathCutscene;

	Animator animator;

	float assembleSpeed = 0f;

	float yawCurrent;
	float yawTarget;
	[DebugDisplay] float yawVelocity;

	PlayerController player;
	Transform playerTransform;

	bool isDead;

	public bool DeathAnimEnd;

	protected override void OnStart()
	{
		MaxHealth = Settings.MaxHealth;
		Health = MaxHealth;

		animator = GetComponentInChildren<Animator>();

		Invincible = true;
	}

	protected override void OnUpdate(float deltaTime)
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
		if (_state != State.Preassemble && HealthBar.gameObject.activeSelf)
		{
			HealthBar.fillAmount = Mathf.MoveTowards(HealthBar.fillAmount, (float) Health / MaxHealth, deltaTime / 4f);
		}

		if (DeathAnimEnd)
		{
			Color c = HealthBack.color;
			HealthBack.color = new Color(c.r, c.g, c.b, Mathf.MoveTowards(c.a, 0f, deltaTime / 4f));
		}

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
				assembleSpeed = Mathf.MoveTowards(assembleSpeed, Settings.AssembleMaxSpeed, Settings.AssembleAcceleration * deltaTime);
				animator.SetFloat("AssembleSpeed", assembleSpeed);
				break;

			case State.IdleTurn:
				if (selectedAttack == 0)
					UpdateYaw(1f);
				else
					UpdateYaw(10f);
				transform.rotation = Quaternion.Euler(0f, yawCurrent, 0f);

				if (Mathf.Abs(yawVelocity) < 5f)
				{
					animator.SetInteger("State", 1);
				}
				else
				{
					animator.SetInteger("State", 2);
				}

				break;

			case State.Attack:

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
#if UNITY_EDITOR
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
	int selectedAttack = 0;
	Coroutine selectAttackCoroutine;
	IEnumerator SelectAttack()
	{
		selectedAttack = 0;

		yield return new WaitForSeconds(Random.Range(Settings.AttackMinWait, Settings.AttackMaxWait));

		for (int i = 1; i <= 3; i++)
		{
			if (lastAttacks[0] == i && lastAttacks[1] == i)
			{
				selectedAttack = SelectRandom(i % 3 + 1, (i + 1) % 3 + 1);
			}
		}

		if (selectedAttack == 0)
		{
			selectedAttack = SelectRandom(1, 2, 3);
		}

		lastAttacks[1] = lastAttacks[0];
		lastAttacks[0] = selectedAttack;

		animator.SetInteger("Attack", selectedAttack);
	}
	int SelectRandom(params int[] ints)
	{
		if (ints.Length == 0)
			return 0;

		float rand = Random.value;
		for (int i = 0; i < ints.Length; i++)
		{
			Debug.Log($"{i + 1}/{ints.Length} = {(i + 1f) / ints.Length}\t\t{rand}");
			if (rand <= (i + 1f) / ints.Length)
			{
				return ints[i];
			}
		}

		return ints[ints.Length - 1];
	}

	protected override void OnDeath()
	{
		SetState(State.Death);
	}
}
