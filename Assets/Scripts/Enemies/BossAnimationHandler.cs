using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAnimationHandler : MonoBehaviour
{
	Boss boss;

	private void Start()
	{
		boss = GetComponentInParent<Boss>();
	}

	public void AssembleEndEvent()
	{
		boss.SetState(Boss.State.IdleTurn);
		boss.Invincible = false;
	}
	public void StartHealthFillEvent()
	{
		boss.HealthBar.gameObject.SetActive(true);
	}
	
	public void AttackSwingEvent(int selected)
	{
		boss.SetState(Boss.State.Attack);
	}
	public void AttackHitEvent(int selected)
	{
		if (selected == 1)
		{
			Debug.Log("Fire!");
		}
		else if (selected == 2)
		{
			Debug.Log("Ice!");
		}
	}
	public void RecoverEndEvent()
	{
		boss.SetState(Boss.State.IdleTurn);
	}

	public void ThrowSwingEvent()
	{
		boss.SetState(Boss.State.Attack);
	}
	public void ThrowEndEvent()
	{
		Debug.Log("Throw Ended");
		boss.SetState(Boss.State.IdleTurn);
	}

	public void DeathEndEvent()
	{
		boss.DeathAnimEnd = true;
	}
}
