using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class winScript : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        Time.timeScale = 0;
        FindObjectOfType<GameManager>().enabled = false;
        FindObjectOfType<GameManager>().gameObject.GetComponent<EventSystem>().enabled = false;
        FindObjectOfType<GameManager>().transform.GetChild(0).gameObject.SetActive(false);
        GameObject.Find("Win").transform.GetChild(0).gameObject.SetActive(true);
        GameObject.Find("Win").transform.GetChild(1).gameObject.SetActive(true);
    }

}
