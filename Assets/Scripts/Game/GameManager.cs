﻿using System.Collections;
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

	[System.Serializable]
	public struct Level
	{
		public string Name;
		public AudioClip Music;
		public Material Skybox;

		[Header("Ambient Light")]
		public Color AmbientSkyColor;
		public Color AmbientEquatorColor;
		public Color AmbientGroundColor;

		[Header("Fog")]
		public bool FogEnabled;
		public Color FogColor;
		public float FogStartDistance;
		public float FogEndDistance;
	}
	public Level[] levels;

	[Header("Loading")]
	public GameObject LoadingScreen;
	public GameObject LoadingTextObject;
	public Image ProgressBar;
	public GameObject ProgressBarBack;
	public Image LoadingBackground;

	[Header("Player")]
	public PlayerController PlayerPrefab;

	public CinemachineVirtualCamera PlayerVcam;

	[Header("Menu")]
	public GameObject MainMenu;
	public GameObject MainUI;
	public GameObject LevelSelectMenu;
	public GameObject HelpMenu;
	public GameObject PauseMenu;
	public GameObject WinMenu;
	public GameObject LoseMenu;

	[Header("Health Icon")]
	public Image HealthImage;
	public Sprite HealthFull;
	public Sprite Health2;
	public Sprite Health1;
	public Sprite Health0;

	[Header("Audio")]
	public AudioSource MusicAudioSource;
	public AudioClip MenuMusic;

	string currentLoadedScene;

	bool isPlaying = false;

	public bool IsPaused;
	public bool Pausible;

	private ActionInputManager actionInputManager;
	private EventSystem eventSystem;

	public enum BeatLevels
	{
		None = 0,
		Ice = 1,
		Fire = 2,
		Boss = 4
	}
	public BeatLevels BeatLevel;

	private void Awake()
	{
		Instance = this;
		//DontDestroyOnLoad(gameObject);
		//DontDestroyOnLoad(LoadingScreen.gameObject);

		eventSystem = GetComponent<EventSystem>();
		EventSystem.current = eventSystem;

		actionInputManager = GetComponent<ActionInputManager>();

		IsPaused = true;
		//PauseMenu.SetActive(false);
	}

	private void Start()
	{
		gameObject.SetActive(true);

#if UNITY_EDITOR
		if (SceneManager.GetActiveScene().name != "Persistent")
		{
			Debug.Log(SceneManager.GetActiveScene().name);
			IsPaused = false;
			MainMenu.SetActive(false);
			MusicAudioSource.Stop();
			LoadingScreen.SetActive(false);
		}
#endif
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

		if (ActionInputManager.GetInputDown("Pause") && Pausible)
		{
			if (IsPaused)
				UnPauseGame();
			else
				PauseGame();
		}

		ProgressBar.fillAmount = Mathf.MoveTowards(ProgressBar.fillAmount, totalSceneProgress, Time.deltaTime * 5f);

		if (PlayerController.Instance == null)
		{
			HealthImage.gameObject.SetActive(false);
		}
		else
		{
			HealthImage.gameObject.SetActive(true);

			if (PlayerController.Instance.Health == 3)
			{
				HealthImage.sprite = HealthFull;
			}
			else if (PlayerController.Instance.Health == 2)
			{
				HealthImage.sprite = Health2;
			}
			else if (PlayerController.Instance.Health == 1)
			{
				HealthImage.sprite = Health1;
			}
			else
			{
				HealthImage.sprite = Health0;
			}
		}
	}


	List<AsyncOperation> scenesLoading = new List<AsyncOperation>();

	public void LoadGame(int id)
	{
		PlayerVcam.gameObject.SetActive(false);

		Pausible = false;

		if (id >= levels.Length || id < 0)
		{
			Debug.LogWarning($"Requested level {id} does not exist!");
			return;
		}

		MainMenu.SetActive(false);
		LoadingScreen.gameObject.SetActive(true);
		LoadingTextObject.SetActive(true);
		LoadingBackground.gameObject.SetActive(true);
		ProgressBarBack.SetActive(true);
		LoadingBackground.color = Color.black;

		scenesLoading.Add(SceneManager.LoadSceneAsync(levels[id].Name, LoadSceneMode.Additive));

		if (currentLoadedScene != null)
		{
			Debug.Log("Unload");
			SceneManager.UnloadSceneAsync(currentLoadedScene);
		}

		StartCoroutine(GetSceneLoadProgress(id, false));
	}

	float fill = 0f;
	float totalSceneProgress = 0f;
	float barVelocity = 0f;
	public IEnumerator GetSceneLoadProgress(int id, bool isHub)
	{
		Level level = levels[id];

		for (int i = 0; i < scenesLoading.Count; i++)
		{
			while (!scenesLoading[i].isDone)
			{
				fill = 0f;
				totalSceneProgress = 0f;
				barVelocity = 0f;

				foreach (AsyncOperation operation in scenesLoading)
				{
					totalSceneProgress += operation.progress;
				}

				totalSceneProgress = (totalSceneProgress / scenesLoading.Count);

				yield return null;
			}
		}

		totalSceneProgress = 1f;
		ProgressBar.gameObject.SetActive(true);

		RenderSettings.skybox = level.Skybox;
		RenderSettings.ambientSkyColor = level.AmbientSkyColor;
		RenderSettings.ambientEquatorColor = level.AmbientEquatorColor;
		RenderSettings.ambientGroundColor = level.AmbientGroundColor;
		RenderSettings.fog = level.FogEnabled;
		RenderSettings.fogColor = level.FogColor;
		RenderSettings.fogMode = FogMode.Linear;
		RenderSettings.fogStartDistance = level.FogStartDistance;
		RenderSettings.fogEndDistance = level.FogEndDistance;
		scenesLoading.Clear();
		currentLoadedScene = level.Name;

		UnPauseGame();

		if (!isHub)
		{
			yield return new WaitForSeconds(1f);

			FadeIn(4f);
		}
		else
		{
			yield return new WaitForSeconds(2f);
			LoadingBackground.color = Color.black;
			FadeIn(2f);
		}
		MusicAudioSource.loop = true;
		MusicAudioSource.clip = level.Music;
		MusicAudioSource.Play();
	}

	public void FadeIn(float totalTime)
	{
		StartCoroutine(FadeInCoroutine(totalTime));
	}
	IEnumerator FadeInCoroutine(float totalTime, float fadeProgress = 0f)
	{
		Pausible = false;

		ProgressBarBack.gameObject.SetActive(false);
		LoadingBackground.color = Color.Lerp(LoadingBackground.color, Color.clear, fadeProgress);

		fadeProgress = Mathf.MoveTowards(LoadingBackground.color.a, 0f, Time.deltaTime / totalTime);

		if (fadeProgress == 0f)
		{
			Pausible = true;
			LoadingScreen.gameObject.SetActive(false);
		}
		else
		{
			yield return null;
			StartCoroutine(FadeInCoroutine(totalTime, fadeProgress));
		}
	}

	public void FadeToColor(float totalTime, Color startingColor, Color endingColor)
	{
		Pausible = false;

		StartCoroutine(FadeToColorCoroutine(totalTime, startingColor, endingColor));
	}
	IEnumerator FadeToColorCoroutine(float TotalTime, Color startingColor, Color endingColor, float fadeProgress = 0f)
	{
		ProgressBar.gameObject.SetActive(false);
		LoadingScreen.gameObject.SetActive(true);
		LoadingBackground.color = Color.Lerp(startingColor, endingColor, fadeProgress);

		fadeProgress = Mathf.MoveTowards(fadeProgress, 1f, Time.deltaTime / TotalTime);

		if (fadeProgress == 1f)
		{
		}
		else
		{
			yield return null;
			StartCoroutine(FadeToColorCoroutine(TotalTime, startingColor, endingColor, fadeProgress));
		}
	}

	public void ReturnToMainMenu(bool restartMusic = false)
	{
		if (currentLoadedScene != null)
			SceneManager.UnloadSceneAsync(currentLoadedScene);

		currentLoadedScene = null;

		MainMenu.SetActive(true);
		MainUI.SetActive(true);
		LevelSelectMenu.SetActive(false);
		HelpMenu.SetActive(false);
		PauseMenu.SetActive(false);
		WinMenu.SetActive(false);
		LoseMenu.SetActive(false);

		MusicAudioSource.clip = MenuMusic;
		MusicAudioSource.loop = false;
		if (restartMusic)
			MusicAudioSource.Play();

		isPlaying = false;
	}

	public void Play()
	{
		MainUI.SetActive(false);
		LevelSelectMenu.SetActive(true);
	}

	public void Help()
	{
		MainUI.SetActive(false);
		HelpMenu.SetActive(true);
	}

	public void PauseGame(bool loadMenu = true)
	{
		Time.timeScale = 0.0f;
		IsPaused = true;
		PauseMenu.SetActive(loadMenu);
		MusicAudioSource.Pause();
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	public void UnPauseGame()
	{
		Time.timeScale = 1.0f;
		IsPaused = false;
		PauseMenu.SetActive(false);
		MusicAudioSource.UnPause();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	public void Win()
	{
		FadeToColor(4f, Color.clear, Color.white);
		PlayerController.Instance.Invincible = true;
		StartCoroutine(WinCoroutine());
	}
	IEnumerator WinCoroutine()
	{
		yield return new WaitForSeconds(5f);
		LoadGame(0);
	}

	public void Lose()
	{
		FadeToColor(4f, Color.clear, Color.black);
	}

	public void Quit()
	{
		Application.Quit();
	}

	public void ReturnToHub()
	{
		if (currentLoadedScene == "Hub")
		{
			UnPauseGame();
		}
		else
		{
			LoadGame(0);
		}
	}
}
