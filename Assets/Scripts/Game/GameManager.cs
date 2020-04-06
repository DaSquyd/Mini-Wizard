using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Cinemachine;

[RequireComponent(typeof(ActionInputManager))]
public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	public GameObject LoadingScreen;
	public Image ProgressBar;

	public PlayerController PlayerPrefab;

	public CinemachineVirtualCamera PlayerVcam;

	public GameObject MainMenu;
	public GameObject PauseMenu;
	public GameObject WinMenu;
	public GameObject LoseMenu;

	string currentLoadedScene;

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
		//PauseMenu.SetActive(false);
	}

	private void Start()
	{
		gameObject.SetActive(true);
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
		MainMenu.SetActive(false);
		LoadingScreen.gameObject.SetActive(true);

		scenesLoading.Add(SceneManager.LoadSceneAsync("Ice Cave", LoadSceneMode.Additive));
		
		StartCoroutine(GetSceneLoadProgress("Ice Cave"));
	}


	float totalSceneProgress;
	public IEnumerator GetSceneLoadProgress(string sceneName)
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
		scenesLoading.Clear();

		currentLoadedScene = sceneName;

		ProgressBar.fillAmount = 1f;

		UnPauseGame();

		yield return new WaitForSeconds(1f);

		LoadingScreen.gameObject.SetActive(false);
	}

	public void ReturnToMainMenu()
	{
		if (currentLoadedScene != null)
			SceneManager.UnloadSceneAsync(currentLoadedScene);

		MainMenu.SetActive(true);
		PauseMenu.SetActive(false);
		WinMenu.SetActive(false);
		LoseMenu.SetActive(false);
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

	public void Win()
	{
		PauseGame();
		WinMenu.SetActive(true);
	}

	public void Lose()
	{
		PauseGame();
		LoseMenu.SetActive(true);
	}

	public void Quit()
	{
		Application.Quit();
	}
}
