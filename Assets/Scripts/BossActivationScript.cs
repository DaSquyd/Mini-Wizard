using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossActivationScript : MonoBehaviour
{

    public BossScript bossScript;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            bossScript.enabled = true;
        }
    }

}
