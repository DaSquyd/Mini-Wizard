using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleFollowPlayer : MonoBehaviour
{
	void Update()
	{
		if (Camera.main != null)
			transform.position = Camera.main.transform.position;
	}
}
