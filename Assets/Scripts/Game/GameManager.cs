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
	public static GameManager instance;
	
	public GameObject loadingScreen;
	public Image progressBar;

	public PlayerController playerPrefab;

	public CinemachineVirtualCamera playerVcam;

	private ActionInputManager _actionInputManager;
	private EventSystem _eventSystem;

	private void Awake()
	{
		instance = this;
		DontDestroyOnLoad(gameObject);
		DontDestroyOnLoad(loadingScreen.gameObject);

		_eventSystem = GetComponent<EventSystem>();
		EventSystem.current = _eventSystem;

		_actionInputManager = GetComponent<ActionInputManager>();
	}

	private void Start()
	{
		//LoadGame();
	}

	private void Update()
	{
		if (instance != this)
		{
			Destroy(gameObject);
			return;
		}

		if (instance == null)
			instance = this;

		if (EventSystem.current != _eventSystem)
		{
			Destroy(EventSystem.current.gameObject);

			EventSystem.current = _eventSystem;
		}
		/*
		if (Camera.current != null && Camera.current != _camera)
		{
			Destroy(Camera.current.gameObject);
		}*/

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}


	List<AsyncOperation> scenesLoading = new List<AsyncOperation>();

	public void LoadGame()
	{
		loadingScreen.gameObject.SetActive(true);

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

				progressBar.fillAmount = totalSceneProgress;

				yield return null;
			}
		}

		loadingScreen.gameObject.SetActive(false);
	}
}
