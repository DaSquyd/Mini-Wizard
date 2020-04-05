using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineSmoothPath)), ExecuteInEditMode]
public class CameraPath : MonoBehaviour
{
	public float[] min = new float[0];
	public float[] max = new float[0];

	public float[] pitch = new float[0];

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

		if (smoothPath.m_Waypoints.Length == min.Length && min.Length == max.Length && smoothPath.m_Waypoints.Length == pitch.Length)
			return;

		int length = smoothPath.m_Waypoints.Length;

		float[] newMin = new float[length];
		float[] newMax = new float[length];
		float[] newPitch = new float[length];

		for (int i = 0; i < length; i++)
		{
			if (i < min.Length)
				newMin[i] = min[i];
			else
				newMin[i] = 4f;

			if (i < max.Length)
				newMax[i] = max[i];
			else
				newMax[i] = 10f;

			if (i < pitch.Length)
				newPitch[i] = pitch[i];
			else
				newPitch[i] = 20f;
		}

		min = newMin;
		max = newMax;
		pitch = newPitch;
	}
}
