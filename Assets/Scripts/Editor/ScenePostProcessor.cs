using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;

public class ScenePostProcess
{
	[PostProcessScene(0)]
	public static void OnPostProcessScene()
	{
		if (DebugCanvas.current == null)
		{
			var debugCanvasPrefab = Resources.Load("Prefabs/Debug/DebugCanvas", typeof(GameObject));
			Object.Instantiate(debugCanvasPrefab);
		}

		if (GameManager.instance == null)
		{
			var gameManagerPrefab = Resources.Load("Prefabs/Game/GameManager", typeof(GameObject));
			//Object.Instantiate(gameManagerPrefab);
		}
	}
}
