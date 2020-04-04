using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraPath))]
[CanEditMultipleObjects]
public class CameraPathEditor : Editor
{
	SerializedProperty sourceMin;
	SerializedProperty sourceMax;

	SerializedProperty newIndex;

	float[] min;
	float[] max;

	private void OnEnable()
	{
		sourceMin = serializedObject.FindProperty("min");
		sourceMax = serializedObject.FindProperty("max");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		min = new float[sourceMin.arraySize];
		max = new float[sourceMax.arraySize];

		EditorGUILayout.BeginVertical();

		for (int i = 0; i < sourceMin.arraySize; i++)
		{
			min[i] = sourceMin.GetArrayElementAtIndex(i).floatValue;
			max[i] = sourceMax.GetArrayElementAtIndex(i).floatValue;

			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField(i + ":", GUILayout.MaxWidth(20f));

			min[i] = EditorGUILayout.FloatField(min[i], GUILayout.MaxWidth(100f));
			EditorGUILayout.MinMaxSlider(ref min[i], ref max[i], 1f, 25f);
			max[i] = EditorGUILayout.FloatField(max[i], GUILayout.MaxWidth(100f));

			EditorGUILayout.EndHorizontal();

			sourceMin.GetArrayElementAtIndex(i).floatValue = min[i];
			sourceMax.GetArrayElementAtIndex(i).floatValue = max[i];
		}

		EditorGUILayout.EndVertical();

		serializedObject.ApplyModifiedProperties();
	}
}
