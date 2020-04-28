using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
	PlayerController player;

	public GameObject Magic;

	private void Start()
	{
		player = PlayerController.Instance;
	}

	public void TeleportStartEvent()
	{
		player = PlayerController.Instance;
		Debug.Log(player);
		GameObject obj = Instantiate(Magic, player.transform.position, new Quaternion());
	}

	public void DeathEndEvent()
	{
		GameManager.Instance.Lose();
	}
}
