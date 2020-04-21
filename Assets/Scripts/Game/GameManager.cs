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

	public GameObject LoadingScreen;
	public Image ProgressBar;

	public PlayerController PlayerPrefab;

	public CinemachineVirtualCamera PlayerVcam;

	public GameObject MainMenu;
	public GameObject MainUI;
	public GameObject LevelSelectMenu;
	public GameObject HelpMenu;
	public GameObject PauseMenu;
	public GameObject WinMenu;
	public GameObject LoseMenu;

	public Image HealthImage;
	public Sprite HealthFull;
	public Sprite Health2;
	public Sprite Health1;
	public Sprite Health0;

	public AudioSource MusicAudioSource;
	public AudioClip MenuMusic;

	string currentLoadedScene;

	bool isPlaying = false;

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

#if UNITY_EDITOR
		if (SceneManager.GetActiveScene().name != "Persistent")
		{
			MainMenu.SetActive(false);
			MusicAudioSource.Stop();
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

		if (ActionInputManager.GetInputDown("Pause") && isPlaying)
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
		if (id >= levels.Length || id < 0)
		{
			Debug.LogWarning($"Requested level {id} does not exist!");
			return;
		}

		MainMenu.SetActive(false);
		LoadingScreen.gameObject.SetActive(true);

		scenesLoading.Add(SceneManager.LoadSceneAsync(levels[id].Name, LoadSceneMode.Additive));

		StartCoroutine(GetSceneLoadProgress(id));
	}

	float fill = 0f;
	float totalSceneProgress = 0f;
	float barVelocity = 0f;
	public IEnumerator GetSceneLoadProgress(int id)
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

			totalSceneProgress = 1f;
		}

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

		MusicAudioSource.clip = level.Music;
		MusicAudioSource.loop = true;
		MusicAudioSource.Play();

		UnPauseGame();

		yield return new WaitForSeconds(1f);

		isPlaying = true;

		LoadingScreen.gameObject.SetActive(false);
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
	}

	public void UnPauseGame()
	{
		Time.timeScale = 1.0f;
		IsPaused = false;
		PauseMenu.SetActive(false);
		MusicAudioSource.UnPause();
	}

	public void Win()
	{
		PauseGame(false);
		WinMenu.SetActive(true);
	}

	public void Lose()
	{
		PauseGame(false);
		LoseMenu.SetActive(true);
	}

	public void Quit()
	{
		Application.Quit();
	}
}
