using UnityEngine;
using System.Collections;

public class Teleport : MonoBehaviour {
	
public ParticleSystem TeleportVideoParticles;
public ParticleSystem SmokeParticles;
public ParticleSystem SparkParticles;
public Light TeleportLight;
public AudioSource TeleportAudio;

private float fadeStart = 5;
private float fadeEnd = 0;
private float fadeTime = 2.3f;
private float t = 0.0f;

void Update (){
   
   if (Input.GetButtonDown("Fire1")) //check to see if the left mouse was pushed.
  
    {
       TeleportVideoParticles.Play();
       SmokeParticles.Play();
       SparkParticles.Play();
       TeleportAudio.Play();
	StartCoroutine("FadeLight");
      
     }
       
   
}

	IEnumerator FadeLight (){
   
  
              while (t < fadeTime) 
              
              {
               t += Time.deltaTime;
               
               TeleportLight.intensity = Mathf.Lerp(fadeStart, fadeEnd, t / fadeTime);
               yield return 0;
               
              }              
            
t = 0;
   
   
}


}