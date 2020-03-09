using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;

public class ScenePostProcess
{
	public static bool isDevelopmentBuild = false;

	[PostProcessScene(0)]
	public static void OnPostProcessScene()
	{
		if (DebugCanvas.current == null)
		{
			var debugCanvasPrefab = Resources.Load("Prefabs/Debug/DebugCanvas", typeof(GameObject));
			Object.Instantiate(debugCanvasPrefab);
		}
	}
}
