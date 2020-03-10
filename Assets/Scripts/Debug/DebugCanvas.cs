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
		public string displayName;
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

	public DebugDisplaySettings settings;
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
						InstantiateCanvasVariable(objectFields[i], mono, attribute.displayName, ref dividerAdded);
					}
				}
			}

			if (objectProperties.Length > 0)
			{
				for (int i = 0; i < objectProperties.Length; i++)
				{
					if (Attribute.GetCustomAttribute(objectProperties[i], typeof(DebugDisplayAttribute)) is DebugDisplayAttribute attribute)
					{
						InstantiateCanvasVariable(objectProperties[i], mono, attribute.displayName, ref dividerAdded);
					}
				}
			}
		}
	}

	private void InstantiateCanvasVariable(MemberInfo memberInfo, MonoBehaviour mono, string displayName, ref bool dividerAdded)
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
			obj = mono,
			displayName = displayName
		};

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
			string display = debugTextSet.displayName;

			debugTextSet.debugCanvasVariable.name = info.Name;

			nameText.text = (display == null ? info.Name : display) + ": ";

			object value = null;

			if (info.MemberType.Equals(MemberTypes.Field))
			{
				value = ((FieldInfo) info).GetValue(obj);
			}
			else if (info.MemberType.Equals(MemberTypes.Property))
			{
				value = ((PropertyInfo) info).GetValue(obj);
			}

			if (value.GetType().Equals(typeof(Color)))
			{
				valueText.text = settings.ColorString;
				valueText.color = (Color) value;
				valueText.faceColor = (Color) value;
				valueText.fontStyle = FontStyles.Highlight;
			}
			else if (value.GetType().Equals(typeof(bool)))
			{

				valueText.text = value.ToString();

				if ((bool) value)
					valueText.color = settings._bool.trueColor;
				else
					valueText.color = settings._bool.falseColor;

				valueText.fontStyle = settings._bool.fontStyle;
			}
			else if (value.GetType().Equals(typeof(string)))
			{
				UpdateTextBox(valueText, value, settings._string);
			}
			else if (value.GetType().Equals(typeof(char)))
			{
				UpdateTextBox(valueText, value, settings._char);
			}
			else if (value.GetType().Equals(typeof(byte)))
			{
				UpdateTextBox(valueText, value, settings._byte);
			}
			else if (value.GetType().Equals(typeof(sbyte)))
			{
				UpdateTextBox(valueText, value, settings._sbyte, true);
			}
			else if (value.GetType().Equals(typeof(short)))
			{
				UpdateTextBox(valueText, value, settings._short, true);
			}
			else if (value.GetType().Equals(typeof(ushort)))
			{
				UpdateTextBox(valueText, value, settings._ushort);
			}
			else if (value.GetType().Equals(typeof(int)))
			{
				UpdateTextBox(valueText, value, settings._int, true);
			}
			else if (value.GetType().Equals(typeof(uint)))
			{
				UpdateTextBox(valueText, value, settings._uint);
			}
			else if (value.GetType().Equals(typeof(long)))
			{
				UpdateTextBox(valueText, value, settings._long, true);
			}
			else if (value.GetType().Equals(typeof(ulong)))
			{
				UpdateTextBox(valueText, value, settings._ulong);
			}
			else if (value.GetType().Equals(typeof(float)))
			{
				UpdateTextBox(valueText, ((float) value).ToString("+0.0###;-0.0###"), settings._float);
			}
			else if (value.GetType().Equals(typeof(double)))
			{
				UpdateTextBox(valueText, ((double) value).ToString(""), settings._double);
			}
			else if (value.GetType().Equals(typeof(Vector2)) || value.GetType().Equals(typeof(Vector2Int)) || value.GetType().Equals(typeof(Vector3)) || value.GetType().Equals(typeof(Vector3Int)) || value.GetType().Equals(typeof(Vector4)))
			{
				UpdateTextBox(valueText, value, settings._vector);
			}
			else
			{
				UpdateTextBox(valueText, value, settings._default);
			}
		}
	}

	private void UpdateTextBox(TMP_Text valueText, object value, DebugDisplaySettings.TypeData typeData, bool forcePositive = false) {
		valueText.color = typeData.color;
		valueText.fontStyle = typeData.fontStyle;
		string prefix = "";
		if (forcePositive && ((int) value >= 0))
			prefix = "+";
		valueText.text = prefix + value.ToString() + typeData.suffix;
		typeData.color.a = 1f;
	}
}
