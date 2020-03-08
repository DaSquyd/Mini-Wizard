using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Fire and Ice/Audio/SFXEvent")]
public class SFXEvent : AudioEvent
{
	public AudioClip[] sFX;
	public AudioMixerGroup audioOutput;

	[Range(0f, 2f)]
	public RangedFloat volume;

	[Range(0f, 2f)]
	public RangedFloat pitch;

	public override void Play(AudioSource source)
	{
		if (sFX.Length == 0)
			return;

		source.clip = sFX[Random.Range(0, sFX.Length)];
		source.volume = Random.Range(volume.minValue, volume.maxValue);
		source.pitch = Random.Range(pitch.minValue, pitch.maxValue);
		source.outputAudioMixerGroup = audioOutput;
		source.Play();
	}

	public void Play()
	{

	}
}
