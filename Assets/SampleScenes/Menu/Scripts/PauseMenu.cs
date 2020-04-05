using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{

    private GameObject pauseMenu;
    private bool paused;

    private void Awake()
    {
        pauseMenu = GameObject.Find("Pause");
        pauseMenu.SetActive(false);
        pauseMenu.transform.GetChild(1).gameObject.SetActive(true);
        pauseMenu.transform.GetChild(2).gameObject.SetActive(true);
    }

    private void MenuOn ()
    {        
        Time.timeScale = 0.0f;
        paused = true;
        pauseMenu.SetActive(true);
    }


    public void MenuOff ()
    {
        Time.timeScale = 1.0f;
        paused = false;
        pauseMenu.SetActive(false);
    }


    public void OnMenuStatusChange ()
    {
        if (paused)
            MenuOn();
        else
            MenuOff();
    }


	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.Escape))
		{
            paused = !paused;
            OnMenuStatusChange();
        }
	}

}
