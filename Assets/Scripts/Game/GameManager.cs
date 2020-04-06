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

	public GameObject PauseMenu;

	public bool IsPaused
	{
		get; private set;
	}

	private ActionInputManager actionInputManager;
	private EventSystem eventSystem;

	private void Awake()
	{
		Instance = this;
		//DontDestroyOnLoad(gameObject);
		//DontDestroyOnLoad(LoadingScreen.gameObject);

		eventSystem = GetComponent<EventSystem>();
		EventSystem.current = eventSystem;

		actionInputManager = GetComponent<ActionInputManager>();

		IsPaused = false;
		PauseMenu.SetActive(false);
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

		if (ActionInputManager.GetInputDown("Pause"))
		{
			if (IsPaused)
				UnPauseGame();
			else
				PauseGame();
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

	public void PauseGame()
	{
		Time.timeScale = 0.0f;
		IsPaused = true;
		PauseMenu.SetActive(true);
	}

	public void UnPauseGame()
	{
		Time.timeScale = 1.0f;
		IsPaused = false;
		PauseMenu.SetActive(false);
	}
}
