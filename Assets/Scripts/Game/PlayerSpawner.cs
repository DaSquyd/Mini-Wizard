using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PlayerSpawner))]
public class PlayerSpawnerEditor : Editor
{
	public float arrowSize = 1.5f;

	void OnSceneGUI()
	{
		PlayerSpawner t = target as PlayerSpawner;

		Handles.color = new Color(0f, 1f, 1f);
		Handles.ArrowHandleCap(0, t.transform.position, t.transform.rotation, arrowSize, EventType.Repaint);
	}
}
#endif

public class PlayerSpawner : MonoBehaviour
{
	public static PlayerSpawner instance;

	private bool _spawning;

	private void Start()
	{
		instance = this;
	}

	private void Update()
	{
		if (instance != null && instance != this)
		{
			Destroy(instance.gameObject);
			instance = this;
		}

		if (PlayerController.instance == null && !_spawning)
		{
			Debug.Log(PlayerController.instance);
			StartCoroutine(SpawnPlayer(0.5f));
		}
	}

	IEnumerator SpawnPlayer(float seconds)
	{
		_spawning = true;

		yield return new WaitForSeconds(seconds);

		Physics.Raycast(transform.position, Vector3.down, out RaycastHit info);

		yield return new WaitUntil(() => GameManager.instance != null);

		PlayerController player = Instantiate(GameManager.instance.playerPrefab, info.point + Vector3.up, Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f));

		player.transform.rotation = transform.rotation;
		player.meshContainer.transform.rotation = transform.rotation;
	}
}
