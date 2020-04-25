using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class Entity : MonoBehaviour
{
	public int MaxHealth
	{
		get; protected set;
	}

	int _health;
	[DebugDisplay]
	public int Health
	{
		get
		{
			return _health;
		}
		set
		{
			_health = Mathf.Clamp(value, 0, MaxHealth);
		}
	}

	public bool Invincible
	{
		get;
		protected set;
	}

	public Element Element;
	protected Renderer[] renderers;
	protected virtual void Start()
	{
#if UNITY_EDITOR
		if (DebugCanvas.Instance != null)
		{
			DebugCanvas.Instance.Reload();
		}
#endif

		renderers = GetComponentsInChildren<Renderer>();
	}


	Vector3 lightDirection;
	Vector3 lightDirectionVelocity;
	float lightIntensity;
	float lightIntensityVelocity;
	Vector3 lightColorVector;
	Vector3 lightColorVectorVelocity;
	float shadowStrength;
	float shadowStrengthVelocity;

	public List<Light> Lights = new List<Light>();
	protected virtual void Update()
	{
		if (transform.position.y <= -15)
			ApplyDamage(null, 1000, Vector3.up, DamageType.Other, Element.None);

		Vector3 entityPosition = transform.position;

		Light closestLight = null;
		float closestLightDistance = 0f;
		foreach (Light light in Lights)
		{
			float distance = Vector3.Distance(entityPosition, light.transform.position);

			if (closestLight == null)
			{
				closestLight = light;
				closestLightDistance = distance;
				continue;
			}

			if (distance < closestLightDistance)
			{
				closestLight = light;
				closestLightDistance = distance;
			}
		}

		Vector3 newDirection = Vector3.zero;
		float newIntensity = 0f;
		Color newLightColorVector = Color.white;
		float newShadowStrength = 0f;

		if (closestLight != null && closestLightDistance < closestLight.range && closestLight != ToonLight.Main)
		{
			Transform closestLightTransform = closestLight.transform;

			switch (closestLight.type)
			{
				case LightType.Spot:
				case LightType.Point:
					newDirection = closestLightTransform.position - entityPosition;
					break;
				case LightType.Rectangle:
				case LightType.Disc:
					newDirection = -closestLightTransform.forward;
					break;
			}

			if (closestLight.type == LightType.Point)
				newDirection = closestLightTransform.position - entityPosition;
			float weightedDistance = Mathf.Pow(closestLightDistance, closestLight.intensity / 1.5f) / Mathf.Pow(closestLight.range, closestLight.intensity / 1.5f);
			newIntensity = Mathf.Lerp(0.9f, 0f, weightedDistance);
			newLightColorVector = Color.Lerp(ToonLight.Main.color, closestLight.color, Mathf.Lerp(0.9f, 0f, weightedDistance));
			newShadowStrength = Mathf.Lerp(0.9f, 0.65f, weightedDistance);
		}
		else
		{
			newDirection = -ToonLight.Main.transform.forward;
			newIntensity = Mathf.Log10(ToonLight.Main.intensity + 1f);
			newLightColorVector = ToonLight.Main.color;
			newShadowStrength = 0.65f;
		}

		var changeTime = 0.1f;
		lightDirection = Vector3.SmoothDamp(lightDirection, newDirection, ref lightDirectionVelocity, changeTime);
		lightIntensity = Mathf.SmoothDamp(lightIntensity, newIntensity, ref lightIntensityVelocity, changeTime);
		lightColorVector = Vector3.SmoothDamp(lightColorVector, new Vector3(newLightColorVector.r, newLightColorVector.g, newLightColorVector.b), ref lightColorVectorVelocity, changeTime);
		shadowStrength = Mathf.SmoothDamp(shadowStrength, newShadowStrength, ref shadowStrengthVelocity, changeTime);

		Color newLightColor = new Color(lightColorVector.x, lightColorVector.y, lightColorVector.z);

		for (int i = 0; i < renderers.Length; i++)
		{
			foreach (Material mat in renderers[i].materials)
			{
				mat.SetVector("_ToonLightDirection", lightDirection);
				mat.SetFloat("_ToonLightIntensity", lightIntensity);
				mat.SetFloat("_ToonShadowIntensity", lightIntensity);
				mat.SetColor("_ToonLightColor", newLightColor);
				mat.SetFloat("_ToonShadowStrength", shadowStrength);
			}
		}
	}

	private Entity lastAttacker;
	protected float lastDamage = 0f;
	public float ApplyDamage(Entity attacker, int amount, Vector3 direction, DamageType type, Element sourceElement)
	{
		if (Invincible)
			return Health;

		lastAttacker = attacker;

		int mult = 1;

		if ((sourceElement == Element.Fire && Element == Element.Ice)
			|| (sourceElement == Element.Ice && Element == Element.Fire))
			mult = 3;

		int oldHealth = Health;
		Health = (int) Mathf.MoveTowards(Health, 0f, amount * mult);
		lastDamage = oldHealth - Health;

		OnReceiveDamage(attacker, amount * mult, direction, type, sourceElement);

		if (Health == 0f)
		{
			OnDeath();
		}

		return Health;
	}

	protected virtual void OnDeath()
	{
#if UNITY_EDITOR
		DebugCanvas.Instance.Reload();
#endif
		Destroy(gameObject);
	}

	public enum DamageType
	{
		Melee,
		Projectile,
		Other
	}

	protected virtual void OnReceiveDamage(Entity attacker, int amount, Vector3 direction, DamageType type, Element sourceElement)
	{
	}
}