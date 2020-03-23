using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;

[ExecuteInEditMode, RequireComponent(typeof(Light))]
public class ToonLight : MonoBehaviour
{
	public static Light main;

	public static Vector2Int PlayerChunk
	{
		get
		{
			PlayerController player = PlayerController.instance;
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

	public Vector2 area;

	private Light _light;
	private Vector2 _chunk;

	private LightType _type;

	public Collider _collider;

	private void Awake()
	{
		_light = GetComponent<Light>();
		_type = _light.type;

		if (_type == LightType.Directional)
			main = _light;
	}

	void OnEnable()
	{
		if (_collider == null)
			switch (_type)
			{
				case LightType.Rectangle:
					_collider = gameObject.AddComponent<BoxCollider>();
					break;
				case LightType.Disc:
					_collider = gameObject.AddComponent<CapsuleCollider>();
					break;
				case LightType.Point:
					_collider = gameObject.AddComponent<SphereCollider>();
					break;
			}


		/*
		var x = Mathf.Abs(Mathf.FloorToInt(transform.position.x / ChunkSize) % ChunkSquareCount);
		var z = Mathf.Abs(Mathf.FloorToInt(transform.position.z / ChunkSize) % ChunkSquareCount);

		_chunk = new Vector2(x, z);

		if (LightChunks[x, z] == null)
			LightChunks[x, z] = new List<Light>();

		LightChunks[x, z].Add(_light);
		*/
	}

	void Update()
	{
		_type = _light.type;

		if (_collider == null)
			_collider = GetComponent<Collider>();

		if (_collider != null)
		{
			switch (_type)
			{
				case LightType.Directional:
					break;
				case LightType.Area:
					if (_collider.GetType() != typeof(BoxCollider))
					{
						Collider[] colliders = GetComponents<Collider>();
						foreach (Collider c in colliders)
						{
							DestroyImmediate(c);
						}
						_collider = gameObject.AddComponent<BoxCollider>();
					}
					(_collider as BoxCollider).center = Vector3.forward * Mathf.Max(_light.range, 1f) / 2f;
					(_collider as BoxCollider).size = new Vector3(area.x, area.y, Mathf.Max(_light.range, 1f));
					break;
				case LightType.Disc:
					if (_collider.GetType() != typeof(CapsuleCollider))
					{
						Collider[] colliders = GetComponents<Collider>();
						foreach (Collider c in colliders)
						{
							DestroyImmediate(c);
						}
						_collider = gameObject.AddComponent<CapsuleCollider>();
					}
					(_collider as CapsuleCollider).radius = area.x;
					(_collider as CapsuleCollider).height = _light.range + area.x * 2f;
					(_collider as CapsuleCollider).center = Vector3.forward * _light.range / 2f;
					(_collider as CapsuleCollider).direction = 2;
					break;
				case LightType.Point:
					if (_collider.GetType() != typeof(SphereCollider))
					{
						Collider[] colliders = GetComponents<Collider>();
						foreach (Collider c in colliders)
						{
							DestroyImmediate(c);
						}
						_collider = gameObject.AddComponent<SphereCollider>();
					}
					(_collider as SphereCollider).radius = _light.range;
					break;
			}

			if (!_collider.isTrigger)
				_collider.isTrigger = true;
		}
		if (main == _light)
		{
			Shader.SetGlobalVector("_ToonLightDirection", -transform.forward);
			Shader.SetGlobalColor("_ToonLightColor", _light.color);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent(out Entity entity))
		{
			Debug.Log("Light Added");
			entity.lights.Add(_light);
		}
	}
	private void OnTriggerExit(Collider other)
	{
		if (other.TryGetComponent(out Entity entity))
		{
			if (entity.lights.Contains(_light))
			{
				Debug.Log("Light Removed");
				entity.lights.Remove(_light);
			}
		}
	}
}
