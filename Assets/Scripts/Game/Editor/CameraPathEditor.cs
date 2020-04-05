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
	SerializedProperty sourcePitch;

	SerializedProperty newIndex;

	float[] min;
	float[] max;
	float[] pitch;

	private void OnEnable()
	{
		sourceMin = serializedObject.FindProperty("min");
		sourceMax = serializedObject.FindProperty("max");
		sourcePitch = serializedObject.FindProperty("pitch");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		min = new float[sourceMin.arraySize];
		max = new float[sourceMax.arraySize];
		pitch = new float[sourcePitch.arraySize];

		EditorGUILayout.BeginVertical();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("", GUILayout.Width(25f));
		EditorGUILayout.LabelField("Min", GUILayout.Width(25f));
		EditorGUILayout.LabelField("");
		EditorGUILayout.LabelField("Max", GUILayout.Width(25f));
		EditorGUILayout.LabelField("Pitch", GUILayout.Width(25f));
		EditorGUILayout.EndHorizontal();

		for (int i = 0; i < sourcePitch.arraySize; i++)
		{
			min[i] = sourceMin.GetArrayElementAtIndex(i).floatValue;
			max[i] = sourceMax.GetArrayElementAtIndex(i).floatValue;
			pitch[i] = sourcePitch.GetArrayElementAtIndex(i).floatValue;

			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField(i + ":", GUILayout.Width(25f));

			min[i] = EditorGUILayout.FloatField(min[i], GUILayout.Width(25f));
			EditorGUILayout.MinMaxSlider(ref min[i], ref max[i], 1f, 25f);
			max[i] = EditorGUILayout.FloatField(max[i], GUILayout.Width(25f));
			pitch[i] = EditorGUILayout.FloatField(pitch[i], GUILayout.Width(25f));

			EditorGUILayout.EndHorizontal();

			sourceMin.GetArrayElementAtIndex(i).floatValue = min[i];
			sourceMax.GetArrayElementAtIndex(i).floatValue = max[i];
			sourcePitch.GetArrayElementAtIndex(i).floatValue = pitch[i];
		}

		EditorGUILayout.EndVertical();

		serializedObject.ApplyModifiedProperties();
	}
}
