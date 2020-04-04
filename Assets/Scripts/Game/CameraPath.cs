using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineSmoothPath)), ExecuteInEditMode]
public class CameraPath : MonoBehaviour
{
	public float[] min = new float[0];
	public float[] max = new float[0];

	public CinemachineSmoothPath smoothPath;

	public int newIndex;

	Vector3[] positions;

	private void Start()
	{
		smoothPath = GetComponent<CinemachineSmoothPath>();
	}

	private void Update()
	{
		positions = new Vector3[smoothPath.m_Waypoints.Length];
		for (int i = 0; i < positions.Length; i++)
		{
			positions[i] = smoothPath.m_Waypoints[i].position;
		}

		if (smoothPath.m_Waypoints.Length == min.Length && min.Length == max.Length)
			return;

		float[] newMin = new float[smoothPath.m_Waypoints.Length];
		float[] newMax = new float[smoothPath.m_Waypoints.Length];

		for (int i = 0; i < smoothPath.m_Waypoints.Length; i++)
		{
			if (i < min.Length)
				newMin[i] = min[i];
			else
				newMin[i] = 4f;

			if (i < max.Length)
				newMax[i] = max[i];
			else
				newMax[i] = 10f;
		}

		min = newMin;
		max = newMax;
	}
}
