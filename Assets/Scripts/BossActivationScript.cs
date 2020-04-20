using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossActivationScript : MonoBehaviour
{

    public BossScript bossScript;
    public GameObject healthBar;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if (other.tag == "Player")
        {
            bossScript.enabled = true;
            healthBar.SetActive(true);
        }
    }

}
