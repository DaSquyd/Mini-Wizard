using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using TMPro;

[CreateAssetMenu(menuName = "Fire and Ice/Debug/Debug Display Settings")]
public class DebugDisplaySettings : ScriptableObject
{
	[System.Serializable]
	public struct TypeData
	{
		public Color color;
		[EnumFlag] public FontStyles fontStyle;
		public string suffix;
	}

	[System.Serializable]
	public struct BooleanData
	{
		public Color trueColor;
		public Color falseColor;
		[EnumFlag] public FontStyles fontStyle;
	}

	public string colorString;

	public TypeData _default;

	public BooleanData _bool;
	public TypeData _byte;
	public TypeData _sbyte;
	public TypeData _short;
	public TypeData _ushort;
	public TypeData _int;
	public TypeData _uint;
	public TypeData _long;
	public TypeData _ulong;
	public TypeData _float;
	public TypeData _double;
	public TypeData _char;
	public TypeData _string;
	public TypeData _vector;

	private void OnEnable()
	{
		_bool.trueColor.a = 1f;
		_bool.falseColor.a = 1f;

		FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

		foreach (FieldInfo info in fields)
		{
			if (info.GetValue(this).GetType().Equals(typeof(TypeData)))
			{
				TypeData data = (TypeData) info.GetValue(this);

				data.color.a = 1f;

				info.SetValue(this, data);
			}
		}
	}
}
