using UnityEngine;
using System.Collections;

public class Teleport_Slow : MonoBehaviour
{

	public ParticleSystem TeleportVideoParticles;
	public ParticleSystem SmokeParticles;
	public ParticleSystem SparkParticles;
	public Light TeleportLight;
	public AudioSource TeleportAudio;

	private float fadeStart = 10;
	private float fadeEnd = 0;
	private float fadeTime = 4.6f;
	private float t = 0.0f;

	private void Start()
	{
		TeleportVideoParticles.Play();
		SmokeParticles.Play();
		SparkParticles.Play();
		TeleportAudio.Play();
		StartCoroutine("FadeLight");
	}

	IEnumerator FadeLight()
	{
		while (t < fadeTime)
		{
			t += Time.deltaTime;

			TeleportLight.intensity = Mathf.Lerp(fadeStart, fadeEnd, t / fadeTime);
			yield return 0;
		}

		t = 0;
	}
}