using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			PlayerController.Instance.ApplyDamage(PlayerController.Instance, 1000, new Vector3(), Entity.DamageType.Other, Element.None);
		}
	}
}
