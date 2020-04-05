using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{

    private GameObject pauseMenu;
    private bool paused;

    //Whenever the player spawns in it will disable the pause menu so it is ready for use
    private void Awake()
    {
        pauseMenu = GameObject.Find("Pause");
        pauseMenu.SetActive(false);
        pauseMenu.transform.GetChild(1).gameObject.SetActive(true);
        pauseMenu.transform.GetChild(2).gameObject.SetActive(true);
    }

    //This is a function to turn the pause menu on by setting the pause menu object to be active and setting the time scale to 0
    private void MenuOn()
    {        
        Time.timeScale = 0.0f;
        paused = true;
        pauseMenu.SetActive(true);
    }

    //This is a function to turn the pause menu off by setting the pause menu object to not be active and setting the time scale to 1
    public void MenuOff()
    {
        Time.timeScale = 1.0f;
        paused = false;
        pauseMenu.SetActive(false);
    }

    //This is a function we can call so the game pauses or unpauses
    public void OnMenuStatusChange()
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
