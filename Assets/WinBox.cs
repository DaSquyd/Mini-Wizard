using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WinBox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
		if (other.tag == "Player")
		{
			GameManager.Instance.Win();
		}
    }

}
