using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ToonLitObject : MonoBehaviour
{
	protected Renderer[] renderers;

	protected virtual void Start()
    {
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

	[HideInInspector]
	public List<Light> Lights = new List<Light>();
	protected virtual void Update()
    {
		Vector3 entityPosition = transform.position;

		if (renderers == null)
			return;

		Light closestLight = null;
		float closestLightDistance = 0f;
		List<Light> remove = new List<Light>();
		foreach (Light light in Lights)
		{
			if (light == null)
			{
				remove.Add(light);
				return;
			}

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

		foreach (Light light in remove)
		{
			Lights.Remove(light);
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

	protected virtual void FixedUpdate()
	{
	}
}
