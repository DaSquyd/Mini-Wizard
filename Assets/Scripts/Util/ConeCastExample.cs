using UnityEngine;

public class ConeCastExample : MonoBehaviour
{
	public float depth;
	public float angle;

	void FixedUpdate()
	{
		LayerMask layerMask = LayerMask.GetMask("Default");

		RaycastHit[] coneHits = ConeCast.ConeCastAll(transform.position, transform.forward, depth, angle, layerMask);

		if (coneHits.Length > 0)
		{
			Debug.Log(coneHits.Length);
			for (int i = 0; i < coneHits.Length; i++)
			{
				//do something with collider information
				
				coneHits[i].collider.GetComponent<MeshRenderer>().materials[0].SetColor("_BaseColor", new Color(0f, 1f, 1f, 1f));

				//Debug.Log(coneHits[i].collider.GetComponent<MeshRenderer>().materials[0].GetTexturePropertyNames()[1]);
			}
		}
	}
}