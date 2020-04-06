using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowParticle : MonoBehaviour
{
	PlayerController player;
	
    void Update()
    {
		if (player == null)
		{
			player = PlayerController.Instance;
			return;
		}

		transform.position = player.transform.position;
    }
}
