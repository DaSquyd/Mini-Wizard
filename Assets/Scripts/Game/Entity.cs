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

	protected MeshRenderer _meshRenderer;
	protected virtual void Start()
	{
		if (Debug.isDebugBuild)
		{
			DebugCanvas.current.Reload();
		}

		_meshRenderer = GetComponentInChildren<MeshRenderer>();
	}


	Vector3 _lightDirection;
	Vector3 _lightDirectionVelocity;
	float _lightIntensity;
	float _lightIntensityVelocity;
	Vector3 _lightColorVector;
	Vector3 _lightColorVectorVelocity;
	float _shadowStrength;
	float _shadowStrengthVelocity;

	public List<Light> lights = new List<Light>();
	protected virtual void Update()
	{
		if (transform.position.y <= -15)
			Destroy(gameObject);

		Vector3 entityPosition = transform.position;

		Light closestLight = null;
		float closestLightDistance = 0f;
		foreach (Light light in lights)
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

		if (closestLight != null && closestLightDistance < closestLight.range && closestLight != ToonLight.main)
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
			newLightColorVector = Color.Lerp(ToonLight.main.color, closestLight.color, Mathf.Lerp(0.9f, 0f, weightedDistance));
			newShadowStrength = Mathf.Lerp(0.9f, 0.65f, weightedDistance);
		}
		else
		{
			newDirection = -ToonLight.main.transform.forward;
			newIntensity = Mathf.Log10(ToonLight.main.intensity + 1f);
			newLightColorVector = ToonLight.main.color;
			newShadowStrength = 0.65f;
		}

		var changeTime = 0.1f;
		_lightDirection = Vector3.SmoothDamp(_lightDirection, newDirection, ref _lightDirectionVelocity, changeTime);
		_lightIntensity = Mathf.SmoothDamp(_lightIntensity, newIntensity, ref _lightIntensityVelocity, changeTime);
		_lightColorVector = Vector3.SmoothDamp(_lightColorVector, new Vector3(newLightColorVector.r, newLightColorVector.g, newLightColorVector.b), ref _lightColorVectorVelocity, changeTime);
		_shadowStrength = Mathf.SmoothDamp(_shadowStrength, newShadowStrength, ref _shadowStrengthVelocity, changeTime);

		Color newLightColor = new Color(_lightColorVector.x, _lightColorVector.y, _lightColorVector.z);

		foreach (Material mat in _meshRenderer.materials)
		{
			mat.SetVector("_ToonLightDirection", _lightDirection);
			mat.SetFloat("_ToonLightIntensity", _lightIntensity);
			mat.SetFloat("_ToonShadowIntensity", _lightIntensity);
			mat.SetColor("_ToonLightColor", newLightColor);
			mat.SetFloat("_ToonShadowStrength", _shadowStrength);
		}

		
	}

	private Entity _lastAttacker;
	private float _lastDamage = 0f;
	public float ApplyDamage(Entity attacker, int amount)
	{
		if (Invincible)
			return Health;

		_lastAttacker = attacker;

		int oldHealth = Health;
		Health = (int) Mathf.MoveTowards(Health, 0f, amount);
		_lastDamage = oldHealth - Health;

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