using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GoBackScript : MonoBehaviour
{
   
    public void GoToMainMenu()
    {
        FindObjectOfType<GameManager>().enabled = true;
        FindObjectOfType<GameManager>().gameObject.GetComponent<EventSystem>().enabled = true;
        FindObjectOfType<GameManager>().transform.GetChild(0).gameObject.SetActive(true);
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
    }

}
