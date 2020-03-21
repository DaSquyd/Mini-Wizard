using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightIntensityShift : MonoBehaviour
{
	Light _light;

    // Start is called before the first frame update
    void Start()
    {
		_light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
		_light.intensity = 0.01f + Mathf.PingPong(Time.time * 0.05f, 0.99f);
    }
}
