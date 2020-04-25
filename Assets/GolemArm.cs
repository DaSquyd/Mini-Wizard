using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemArm : MonoBehaviour
{
	GolemEnemy golem;

	private void Start()
	{
		golem = GetComponentInParent<GolemEnemy>();
	}

	private void OnCollisionStay(Collision collision)
	{
		golem.OnHit(collision);
	}
}
