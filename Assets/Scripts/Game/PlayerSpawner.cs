using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PlayerSpawner))]
public class PlayerSpawnerEditor : Editor
{
	public float ArrowSize = 1.5f;

	void OnSceneGUI()
	{
		PlayerSpawner t = target as PlayerSpawner;

		Handles.color = new Color(0f, 1f, 1f);
		Handles.ArrowHandleCap(0, t.transform.position, t.transform.rotation, ArrowSize, EventType.Repaint);
	}
}
#endif

public class PlayerSpawner : MonoBehaviour
{
	public static PlayerSpawner Instance;

	private bool spawning;

	private void Start()
	{
		Instance = this;
	}

	private void Update()
	{
        if (Instance != null && Instance != this)
		{
			Destroy(Instance.gameObject);
			Instance = this;
		}


        if (PlayerController.Instance == null && !spawning)
		{
            StartCoroutine(SpawnPlayer(0f));
		}
	}

	IEnumerator SpawnPlayer(float seconds)
	{
		spawning = true;

		yield return new WaitForSeconds(seconds);

		Physics.Raycast(transform.position, Vector3.down, out RaycastHit info);

		yield return new WaitUntil(() => GameManager.Instance != null);

		PlayerController player = Instantiate(GameManager.Instance.PlayerPrefab, info.point + Vector3.up, Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f), transform);

		player.transform.rotation = transform.rotation;
		player.MeshContainer.transform.rotation = transform.rotation;

		spawning = false;
	}
}
