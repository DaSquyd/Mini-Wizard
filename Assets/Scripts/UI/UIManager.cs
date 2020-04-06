using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    public GameObject howtoplay;
    public GameObject mainmenu;

    public void HowToPlay()
    {
        mainmenu.SetActive(false);
        howtoplay.SetActive(true);
    }

    public void MainMenu()
    {
        mainmenu.SetActive(true);
        howtoplay.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
