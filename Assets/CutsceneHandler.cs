﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CutsceneHandler : MonoBehaviour
{
	public bool RunOnEnter;

	[System.Serializable]
	public struct EventRef
	{
		public UnityEvent Event;
		public AudioEvent AudioEvent;
		public AudioSource Source;
		public float Seconds;
		public bool Played
		{
			get; set;
		}
	}
	public EventRef[] Events;
	float elapsed = 0f;

	private void Start()
	{
		Play();
	}

	public void EnterScene()
	{

	}

	public void Play()
	{
		elapsed = 0f;
		for (int i = 0; i < Events.Length; i++)
		{
			Events[i].Played = false;
		}
		StartCoroutine(PlayRoutine());
	}

	IEnumerator PlayRoutine()
	{
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
		if (PlayerController.Instance != null)
		{
			PlayerController.Instance.Invincible = true;
			PlayerController.Instance.MoveLock = true;
		}

=======
>>>>>>> parent of 9d7b92d... shtuff
=======
>>>>>>> parent of 9d7b92d... shtuff
=======
>>>>>>> parent of 9d7b92d... shtuff
=======
>>>>>>> parent of 9d7b92d... shtuff
		for (int i = 0; i < Events.Length; i++)
		{
			EventRef e = Events[i];
			if (!e.Played && elapsed > e.Seconds)
			{
				if (e.Event != null)
					e.Event.Invoke();
				Events[i].Played = true;
			}
		}
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
		if (elapsed >= TotalTime && PlayerController.Instance != null)
		{
			PlayerController.Instance.Invincible = false;
			PlayerController.Instance.MoveLock = false;
		}
		else
		{
			yield return null;
			elapsed += Time.deltaTime;
			StartCoroutine(PlayRoutine());
		}
=======
		yield return null;
		elapsed += Time.deltaTime;
		StartCoroutine(PlayRoutine());
>>>>>>> parent of 9d7b92d... shtuff
=======
		yield return null;
		elapsed += Time.deltaTime;
		StartCoroutine(PlayRoutine());
>>>>>>> parent of 9d7b92d... shtuff
=======
		yield return null;
		elapsed += Time.deltaTime;
		StartCoroutine(PlayRoutine());
>>>>>>> parent of 9d7b92d... shtuff
=======
		yield return null;
		elapsed += Time.deltaTime;
		StartCoroutine(PlayRoutine());
>>>>>>> parent of 9d7b92d... shtuff
	}

	public void SetBrainBlendStyleToCut()
	{
		Cinemachine.CinemachineBrain brain = FindObjectOfType<Cinemachine.CinemachineBrain>();
		if (brain != null)
		{
			brain.m_DefaultBlend.m_Style = Cinemachine.CinemachineBlendDefinition.Style.Cut;
		}
	}
	public void SetBrainBlendStyleToEase()
	{
		Cinemachine.CinemachineBrain brain = FindObjectOfType<Cinemachine.CinemachineBrain>();
		if (brain != null)
		{
			brain.m_DefaultBlend.m_Style = Cinemachine.CinemachineBlendDefinition.Style.EaseInOut;
		}
	}
	public void SetBrainBlendTime(float time)
	{
		Cinemachine.CinemachineBrain brain = FindObjectOfType<Cinemachine.CinemachineBrain>();
		if (brain != null)
		{
			brain.m_DefaultBlend.m_Time = time;
		}
	}


}
