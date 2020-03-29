using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Fire and Ice/Audio/SFXEvent")]
public class SFXEvent : AudioEvent
{
	public AudioClip[] Sfx;
	public AudioMixerGroup AudioOutput;

	[Range(0f, 2f)]
	public RangedFloat Volume;

	[Range(0f, 2f)]
	public RangedFloat Pitch;

	public override void Play(AudioSource source)
	{
		if (Sfx.Length == 0)
			return;

		source.clip = Sfx[Random.Range(0, Sfx.Length)];
		source.volume = Random.Range(Volume.MinValue, Volume.MaxValue);
		source.pitch = Random.Range(Pitch.MinValue, Pitch.MaxValue);
		source.outputAudioMixerGroup = AudioOutput;
		source.Play();
	}

	public void Play()
	{

	}
}
