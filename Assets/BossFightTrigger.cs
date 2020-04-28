using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFightTrigger : MonoBehaviour
{
	public CutsceneHandler Cutscene;

	BoxCollider collider;

    void Start()
    {
		collider = GetComponent<BoxCollider>();
    }

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			PlayerController player = PlayerController.Instance;

			Cutscene.Play();
		}
	}
}
