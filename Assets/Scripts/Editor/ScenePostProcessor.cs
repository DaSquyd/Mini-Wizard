using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.Callbacks;

public class ScenePostProcess
{
	[PostProcessScene(0)]
	public static void OnPostProcessScene()
	{
		if (DebugCanvas.Instance == null && Debug.isDebugBuild)
		{
			var debugCanvasPrefab = Resources.Load("Prefabs/Debug/DebugCanvas", typeof(GameObject));
			Object.Instantiate(debugCanvasPrefab);
			Application.Quit();
		}

		if (GameManager.Instance == null)
		{
			if (SceneManager.GetActiveScene().name != "Persistent")
				SceneManager.LoadScene("Persistent", LoadSceneMode.Additive);
		}
	}
}
