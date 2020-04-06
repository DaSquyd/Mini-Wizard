using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSpikePlatform : MonoBehaviour
{
	private IEnumerator Start()
	{
		Animation animation = GetComponent<Animation>();

		yield return new WaitForSeconds(Random.Range(0f, 8f));
		animation.Play();
	}
}
