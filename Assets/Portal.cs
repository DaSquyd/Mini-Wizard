using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
	CutsceneHandler cutscene;
	bool playing;

	private void Start()
	{
		cutscene = GetComponent<CutsceneHandler>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player" && !playing)
		{
			cutscene.Play();
			playing = true;
		}
	}

	public void White()
	{
		GameManager.Instance.FadeToColor(1f, Color.clear, Color.white);
	}

	public void Load(int level)
	{
		GameManager.Instance.LoadGame(level);
	}
}
