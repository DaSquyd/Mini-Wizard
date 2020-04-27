using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemAnimationHandler : MonoBehaviour
{
	GolemEnemy golem;

	private void Start()
	{
		golem = GetComponentInParent<GolemEnemy>();
	}

	public void VelocityStartEvent()
	{
		golem.AttackVelocity = golem.Settings.AttackVelocity;
	}

	public void HitStartEvent()
	{
		golem.CanHit = true;
	}

	public void HitEndEvent()
	{
		golem.CanHit = false;
	}

	public void AnimEndEvent()
	{
		golem.SetState(GolemEnemy.State.Run);
	}

	public void DeathAnimEvent()
	{

	}

	public void DeathEndEvent()
	{
		Destroy(golem.gameObject);
	}
}
