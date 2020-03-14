using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public static GameManager instance;
	public GameObject loadingScreen;
	public Image progressBar;

	private void Awake()
	{
		instance = this;
		DontDestroyOnLoad(gameObject);
	}

	List<AsyncOperation> scenesLoading = new List<AsyncOperation>();

	public void LoadGame()
	{
		loadingScreen.gameObject.SetActive(true);

		StartCoroutine(GetSceneLoadProgress());
	}


	float totalSceneProgress;
	public IEnumerator GetSceneLoadProgress()
	{
		for (int i = 0; i < scenesLoading.Count; i++)
		{
			while (!scenesLoading[i].isDone)
			{
				totalSceneProgress = 0f;

				foreach (AsyncOperation operation in scenesLoading)
				{
					totalSceneProgress += operation.progress;
				}

				totalSceneProgress = (totalSceneProgress / scenesLoading.Count);

				progressBar.fillAmount = totalSceneProgress;

				yield return null;
			}
		}

		loadingScreen.gameObject.SetActive(false);
	}
}
