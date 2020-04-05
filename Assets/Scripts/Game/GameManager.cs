using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Cinemachine;

[RequireComponent(typeof(ActionInputManager), typeof(EventSystem))]
public class GameManager : MonoBehaviour
{
	public static GameManager Instance;
	
	public GameObject LoadingScreen;
	public Image ProgressBar;

	public PlayerController PlayerPrefab;

	public CinemachineVirtualCamera PlayerVcam;

	private ActionInputManager actionInputManager;
	private EventSystem eventSystem;

	private void Awake()
	{
		Instance = this;
		DontDestroyOnLoad(gameObject);
		DontDestroyOnLoad(LoadingScreen.gameObject);

		eventSystem = GetComponent<EventSystem>();
		EventSystem.current = eventSystem;

		actionInputManager = GetComponent<ActionInputManager>();
	}

	private void Start()
	{
		//LoadGame();
	}

	private void Update()
	{
		if (Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		if (Instance == null)
			Instance = this;

		if (EventSystem.current != eventSystem)
		{
			Destroy(EventSystem.current.gameObject);

			EventSystem.current = eventSystem;
		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}


	List<AsyncOperation> scenesLoading = new List<AsyncOperation>();

	public void LoadGame()
	{
		LoadingScreen.gameObject.SetActive(true);

		scenesLoading.Add(SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive));

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

				ProgressBar.fillAmount = totalSceneProgress;

				yield return null;
			}
		}

		LoadingScreen.gameObject.SetActive(false);
	}
}
