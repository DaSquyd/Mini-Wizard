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

	public void RightStartEvent()
	{

	}
	public void RightHitEvent()
	{

	}

	public void LeftStartEvent()
	{

	}
	public void LeftHitEvent()
	{

	}
}
