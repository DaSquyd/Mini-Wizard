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
		public DebugCanvasVariable DebugCanvasVariable;
		public MemberInfo MemberInfo;
		public object Obj;
		public string DisplayName;
	}

	public static DebugCanvas Instance;

	public static GameObject GameObject
	{
		get
		{
			return Instance.gameObject;
		}
	}

	public static List<DebugTextSet> TextBoxes = new List<DebugTextSet>();
	public static List<HorizontalLayoutGroup> Groups = new List<HorizontalLayoutGroup>();
	public static List<TMP_Text> Titles = new List<TMP_Text>();

	public DebugDisplaySettings Settings;
	public GameObject Panel;
	public VerticalLayoutGroup Parent;
	public TMP_Text DividerPrefab;
	public HorizontalLayoutGroup HorizontalLayoutPrefab;

	private void Awake()
	{
		Instance = this;
	}

	private void Load()
	{
		MonoBehaviour[] sceneActive = FindObjectsOfType<MonoBehaviour>();

		foreach (MonoBehaviour mono in sceneActive)
		{
			BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			Type type = mono.GetType();

			FieldInfo[] objectFields = type.GetFields(flags);
			PropertyInfo[] objectProperties = type.GetProperties(flags);

			bool titleAdded = false;

			if (objectFields.Length > 0)
			{
				for (int i = 0; i < objectFields.Length; i++)
				{
					if (Attribute.GetCustomAttribute(objectFields[i], typeof(DebugDisplayAttribute)) is DebugDisplayAttribute attribute)
					{
						InstantiateCanvasVariable(objectFields[i], mono, attribute.DisplayName, ref titleAdded);
					}
				}
			}

			if (objectProperties.Length > 0)
			{
				for (int i = 0; i < objectProperties.Length; i++)
				{
					if (Attribute.GetCustomAttribute(objectProperties[i], typeof(DebugDisplayAttribute)) is DebugDisplayAttribute attribute)
					{
						InstantiateCanvasVariable(objectProperties[i], mono, attribute.DisplayName, ref titleAdded);
					}
				}
			}
		}
	}

	private void Unload()
	{
		foreach (HorizontalLayoutGroup group in Groups)
		{
			Destroy(group.gameObject);
		}

		foreach (TMP_Text title in Titles)
		{
			Destroy(title.gameObject);
		}

		TextBoxes.Clear();
		Groups.Clear();
		Titles.Clear();
	}

	public void Reload()
	{
		Unload();
		Load();
	}

	private void InstantiateCanvasVariable(MemberInfo memberInfo, MonoBehaviour mono, string displayName, ref bool titleAdded)
	{
		if (!titleAdded)
		{
			TMP_Text titleText = Instantiate(DividerPrefab, Parent.transform);
			titleText.text = mono.ToString();
			Titles.Add(titleText);
			titleAdded = true;
		}

		HorizontalLayoutGroup group = Instantiate(HorizontalLayoutPrefab, Parent.transform);

		DebugTextSet debugTextSet = new DebugTextSet
		{
			DebugCanvasVariable = group.GetComponent<DebugCanvasVariable>(),
			MemberInfo = memberInfo,
			Obj = mono,
			DisplayName = displayName
		};

		TextBoxes.Add(debugTextSet);
		Groups.Add(group);
	}

	private void Update()
	{
		if (this != Instance)
		{
			Destroy(gameObject);
			return;
		}

		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Equals))
		{
			Panel.gameObject.SetActive(!Panel.gameObject.activeSelf);

			if (Panel.gameObject.activeSelf)
				Load();
			else
				Unload();
		}

		if (!Panel.gameObject.activeSelf)
			return;

		if (TextBoxes == null)
			Load();

		foreach (DebugTextSet debugTextSet in TextBoxes)
		{
			TMP_Text nameText = debugTextSet.DebugCanvasVariable.NameText;
			TMP_Text valueText = debugTextSet.DebugCanvasVariable.ValueText;
			MemberInfo info = debugTextSet.MemberInfo;
			object obj = debugTextSet.Obj;
			string display = debugTextSet.DisplayName;

			debugTextSet.DebugCanvasVariable.name = info.Name;

			nameText.text = (display ?? info.Name) + ": ";

			object value = null;

			if (info.MemberType.Equals(MemberTypes.Field))
			{
				value = (info as FieldInfo).GetValue(obj);
			}
			else if (info.MemberType.Equals(MemberTypes.Property))
			{
				value = (info as PropertyInfo).GetValue(obj);
			}
			else
			{
				continue;
			}

			if (value == null)
				continue;

			if (value.GetType().Equals(typeof(Color)))
			{
				valueText.text = Settings.ColorString;
				valueText.color = (Color) value;
				valueText.faceColor = (Color) value;
				valueText.fontStyle = FontStyles.Highlight;
			}
			else if (value.GetType().Equals(typeof(bool)))
			{

				valueText.text = value.ToString();

				if ((bool) value)
					valueText.color = Settings.Bool.TrueColor;
				else
					valueText.color = Settings.Bool.FalseColor;

				valueText.fontStyle = Settings.Bool.FontStyle;
			}
			else if (value.GetType().Equals(typeof(string)))
			{
				UpdateTextBox(valueText, value, Settings.String);
			}
			else if (value.GetType().Equals(typeof(char)))
			{
				UpdateTextBox(valueText, value, Settings.Char);
			}
			else if (value.GetType().Equals(typeof(byte)))
			{
				UpdateTextBox(valueText, value, Settings.Byte, false);
			}
			else if (value.GetType().Equals(typeof(sbyte)))
			{
				UpdateTextBox(valueText, value, Settings.Sbyte, true);
			}
			else if (value.GetType().Equals(typeof(short)))
			{
				UpdateTextBox(valueText, value, Settings.Short, true);
			}
			else if (value.GetType().Equals(typeof(ushort)))
			{
				UpdateTextBox(valueText, value, Settings.Ushort, false);
			}
			else if (value.GetType().Equals(typeof(int)))
			{
				UpdateTextBox(valueText, value, Settings.Int, true);
			}
			else if (value.GetType().Equals(typeof(uint)))
			{
				UpdateTextBox(valueText, value, Settings.Uint, false);
			}
			else if (value.GetType().Equals(typeof(long)))
			{
				UpdateTextBox(valueText, value, Settings.Long, true);
			}
			else if (value.GetType().Equals(typeof(ulong)))
			{
				UpdateTextBox(valueText, value, Settings.Ulong, false);
			}
			else if (value.GetType().Equals(typeof(float)))
			{
				UpdateTextBox(valueText, ((float) value).ToString("0.0000"), Settings.Float);
			}
			else if (value.GetType().Equals(typeof(double)))
			{
				UpdateTextBox(valueText, ((double) value).ToString("0.0000"), Settings.Double);
			}
			else if (value.GetType().Equals(typeof(Vector2)) || value.GetType().Equals(typeof(Vector2Int)) || value.GetType().Equals(typeof(Vector3)) || value.GetType().Equals(typeof(Vector3Int)) || value.GetType().Equals(typeof(Vector4)))
			{
				UpdateTextBox(valueText, value, Settings.Vector);
			}
			else
			{
				UpdateTextBox(valueText, value, Settings.Default);
			}
		}
	}

	private void UpdateTextBox(TMP_Text valueText, object value, DebugDisplaySettings.TypeData typeData, bool forcePositive = false)
	{
		valueText.color = typeData.Color;
		valueText.fontStyle = typeData.FontStyle;
		string prefix = "";
		if (forcePositive && ((int) value >= 0))
			prefix = "+";
		valueText.text = prefix + value.ToString() + typeData.Suffix;
		typeData.Color.a = 1f;
	}
}
