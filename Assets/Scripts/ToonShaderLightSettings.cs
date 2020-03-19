using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(Light))]
public class ToonShaderLightSettings : MonoBehaviour
{
	public static Light main;

	public bool isMain;

	public static Vector2Int PlayerChunk
	{
		get
		{
			PlayerController player = PlayerController.current;
			if (player == null)
				return Vector2Int.zero;

			int x = Mathf.Abs(Mathf.FloorToInt(player.transform.position.x / ChunkSize) % ChunkSquareCount);
			int z = Mathf.Abs(Mathf.FloorToInt(player.transform.position.z / ChunkSize) % ChunkSquareCount);

			return new Vector2Int(x, z);
		}
	}

	public static readonly int ChunkSquareCount = 10;
	public static readonly int ChunkSize = 10;
	public static readonly List<Light>[,] LightChunks = new List<Light>[ChunkSquareCount, ChunkSquareCount];

	private Light _light;
	private Vector2 _chunk;

	void OnEnable()
	{
		_light = GetComponent<Light>();

		if (main == null)
			main = _light;

		var x = Mathf.Abs(Mathf.FloorToInt(transform.position.x / ChunkSize) % ChunkSquareCount);
		var z = Mathf.Abs(Mathf.FloorToInt(transform.position.z / ChunkSize) % ChunkSquareCount);

		_chunk = new Vector2(x, z);

		if (LightChunks[x, z] == null)
			LightChunks[x, z] = new List<Light>();

		LightChunks[x, z].Add(_light);
	}

	void Update()
	{
		if (main == null)
			main = _light;

		if (isMain)
		{
			main.GetComponent<ToonShaderLightSettings>().isMain = false;
			main = _light;
		}

		if (main == _light)
		{
			isMain = true;
			Shader.SetGlobalVector("_ToonLightDirection", -transform.forward);
			Shader.SetGlobalColor("_ToonLightColor", _light.color);
		}
		else
		{
			isMain = false;

			if (PlayerController.current != null)
			{
				if (_chunk == PlayerChunk)
				{
					PlayerController player = PlayerController.current;

					MeshRenderer renderer = player.meshContainer.GetComponentInChildren<MeshRenderer>();

					//renderer.material.SetVector("_ToonLightDirection", transform.position - player.transform.position);
				}
			}
		}
	}
}
