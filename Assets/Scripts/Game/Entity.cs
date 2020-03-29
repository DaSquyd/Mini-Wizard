using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	protected Renderer renderer;
	protected virtual void Start()
	{
		if (Debug.isDebugBuild)
		{
			DebugCanvas.Instance.Reload();
		}

		renderer = GetComponentInChildren<Renderer>();
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
			Destroy(gameObject);

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

		foreach (Material mat in renderer.materials)
		{
			mat.SetVector("_ToonLightDirection", lightDirection);
			mat.SetFloat("_ToonLightIntensity", lightIntensity);
			mat.SetFloat("_ToonShadowIntensity", lightIntensity);
			mat.SetColor("_ToonLightColor", newLightColor);
			mat.SetFloat("_ToonShadowStrength", shadowStrength);
		}

		
	}

	private Entity lastAttacker;
	private float lastDamage = 0f;
	public float ApplyDamage(Entity attacker, int amount)
	{
		if (Invincible)
			return Health;

		lastAttacker = attacker;

		int oldHealth = Health;
		Health = (int) Mathf.MoveTowards(Health, 0f, amount);
		lastDamage = oldHealth - Health;

		OnReceiveDamage();

		if (Health == 0f)
		{
			Destroy(gameObject);
		}

		return Health;
	}

	protected virtual void OnReceiveDamage()
	{
	}
}