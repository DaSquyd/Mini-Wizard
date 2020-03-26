using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    void Start()
	{
		SceneManager.LoadScene(2, LoadSceneMode.Single);
		SceneManager.LoadScene("Persistent", LoadSceneMode.Additive);
	}
}
