using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;

[ExecuteInEditMode, RequireComponent(typeof(Light))]
public class ToonLight : MonoBehaviour
{
	public static Light Main;

	public Vector2 Area;


	private new Light light;

	private LightType type;

	public new Collider collider;

	private void Awake()
	{
		light = GetComponent<Light>();
		type = light.type;

		if (type == LightType.Directional)
			Main = light;
	}

	void OnEnable()
	{
		if (collider == null)
			switch (type)
			{
				case LightType.Rectangle:
					collider = gameObject.AddComponent<BoxCollider>();
					break;
				case LightType.Disc:
					collider = gameObject.AddComponent<CapsuleCollider>();
					break;
				case LightType.Point:
					collider = gameObject.AddComponent<SphereCollider>();
					break;
			}
	}

	void Update()
	{
		type = light.type;

#if UNITY_EDITOR
		if (transform.localScale != Vector3.one)
			transform.localScale = Vector3.one;

		if (type == LightType.Point)
		{
			if (transform.rotation != new Quaternion())
				transform.rotation = new Quaternion();

			if (light.range > 10f)
				light.range = 10f;

			if (light.intensity > 100f)
				light.intensity = 100f;
		}
#endif

		if (collider == null)
			collider = GetComponent<Collider>();

		if (collider != null)
		{
			switch (type)
			{
				case LightType.Directional:
					break;
				case LightType.Area:
					if (collider.GetType() != typeof(BoxCollider))
					{
						Collider[] colliders = GetComponents<Collider>();
						foreach (Collider c in colliders)
						{
							DestroyImmediate(c);
						}
						collider = gameObject.AddComponent<BoxCollider>();
					}
					(collider as BoxCollider).center = Vector3.forward * Mathf.Max(light.range, 1f) / 2f;
					(collider as BoxCollider).size = new Vector3(Area.x, Area.y, Mathf.Max(light.range, 1f));
					break;
				case LightType.Disc:
					if (collider.GetType() != typeof(CapsuleCollider))
					{
						Collider[] colliders = GetComponents<Collider>();
						foreach (Collider c in colliders)
						{
							DestroyImmediate(c);
						}
						collider = gameObject.AddComponent<CapsuleCollider>();
					}
					(collider as CapsuleCollider).radius = Area.x;
					(collider as CapsuleCollider).height = light.range + Area.x * 2f;
					(collider as CapsuleCollider).center = Vector3.forward * light.range / 2f;
					(collider as CapsuleCollider).direction = 2;
					break;
				case LightType.Point:
					if (collider.GetType() != typeof(SphereCollider))
					{
						Collider[] colliders = GetComponents<Collider>();
						foreach (Collider c in colliders)
						{
							DestroyImmediate(c);
						}
						collider = gameObject.AddComponent<SphereCollider>();
					}
					(collider as SphereCollider).radius = light.range;
					break;
			}

			if (!collider.isTrigger)
				collider.isTrigger = true;
		}
		if (Main == light)
		{
			Shader.SetGlobalVector("_ToonLightDirection", -transform.forward);
			Shader.SetGlobalColor("_ToonLightColor", light.color);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent(out Entity entity))
		{
			Debug.Log("Light Added");
			entity.Lights.Add(light);
		}
	}
	private void OnTriggerExit(Collider other)
	{
		if (other.TryGetComponent(out Entity entity))
		{
			if (entity.Lights.Contains(light))
			{
				Debug.Log("Light Removed");
				entity.Lights.Remove(light);
			}
		}
	}
}
