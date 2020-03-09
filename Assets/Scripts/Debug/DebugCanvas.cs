using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class DebugCanvas : MonoBehaviour
{
	public struct DebugTextSet
	{
		public DebugCanvasVariable debugCanvasVariable;
		public MemberInfo memberInfo;
		public object obj;
	}

	public static DebugCanvas current;

	public static GameObject CurrentGameObject
	{
		get
		{
			return current.gameObject;
		}
	}

	public static List<DebugTextSet> textboxes = new List<DebugTextSet>();

	public GameObject panel;
	public VerticalLayoutGroup parent;
	public TMP_Text dividerPrefab;
	public HorizontalLayoutGroup horizontalLayoutPrefab;

	private void Awake()
	{
		current = this;
	}

	private void Start()
	{
		MonoBehaviour[] sceneActive = FindObjectsOfType<MonoBehaviour>();

		foreach (MonoBehaviour mono in sceneActive)
		{
			BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			Type type = mono.GetType();

			FieldInfo[] objectFields = type.GetFields(flags);
			PropertyInfo[] objectProperties = type.GetProperties(flags);

			bool dividerAdded = false;

			if (objectFields.Length > 0)
			{
				for (int i = 0; i < objectFields.Length; i++)
				{
					if (Attribute.GetCustomAttribute(objectFields[i], typeof(DebugDisplayAttribute)) is DebugDisplayAttribute attribute)
					{
						InstantiateCanvasVariable(objectFields[i], mono, ref dividerAdded);
					}
				}
			}

			if (objectProperties.Length > 0)
			{
				for (int i = 0; i < objectProperties.Length; i++)
				{
					if (Attribute.GetCustomAttribute(objectProperties[i], typeof(DebugDisplayAttribute)) is DebugDisplayAttribute attribute)
					{
						InstantiateCanvasVariable(objectProperties[i], mono, ref dividerAdded);
					}
				}
			}
		}
	}

	private void InstantiateCanvasVariable(MemberInfo memberInfo, MonoBehaviour mono, ref bool dividerAdded)
	{
		if (!dividerAdded)
		{
			TMP_Text dividerText = Instantiate(dividerPrefab, parent.transform);
			dividerText.text = mono.ToString();
			dividerAdded = true;
		}

		HorizontalLayoutGroup group = Instantiate(horizontalLayoutPrefab, parent.transform);

		DebugTextSet debugTextSet = new DebugTextSet
		{
			debugCanvasVariable = group.GetComponent<DebugCanvasVariable>(),
			memberInfo = memberInfo,
			obj = mono
		};
		Debug.Log($"<color=green>Match: {memberInfo}</color>");

		textboxes.Add(debugTextSet);
	}

	private void Update()
	{
		if (this != current)
		{
			Destroy(gameObject);
			return;
		}

		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Equals))
		{
			panel.gameObject.SetActive(!panel.gameObject.activeSelf);
		}

		foreach (DebugTextSet debugTextSet in textboxes)
		{
			TMP_Text nameText = debugTextSet.debugCanvasVariable.nameText;
			TMP_Text valueText = debugTextSet.debugCanvasVariable.valueText;
			MemberInfo info = debugTextSet.memberInfo;
			object obj = debugTextSet.obj;

			debugTextSet.debugCanvasVariable.name = info.Name;

			nameText.text = info.Name + ": ";

			if (info.MemberType.Equals(MemberTypes.Field))
			{
				valueText.text = ((FieldInfo) info).GetValue(obj).ToString();
			}
			else if (info.MemberType.Equals(MemberTypes.Property))
			{
				valueText.text = ((PropertyInfo) info).GetValue(obj).ToString();
			}
		}
	}
}
