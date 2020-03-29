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
		public Color Color;
		[EnumFlag] public FontStyles FontStyle;
		public string Suffix;
	}

	[System.Serializable]
	public struct BooleanData
	{
		public Color TrueColor;
		public Color FalseColor;
		[EnumFlag] public FontStyles FontStyle;
	}

	public string ColorString;

	public TypeData Default;

	public BooleanData Bool;
	public TypeData Byte;
	public TypeData Sbyte;
	public TypeData Short;
	public TypeData Ushort;
	public TypeData Int;
	public TypeData Uint;
	public TypeData Long;
	public TypeData Ulong;
	public TypeData Float;
	public TypeData Double;
	public TypeData Char;
	public TypeData String;
	public TypeData Vector;

	private void OnEnable()
	{
		Bool.TrueColor.a = 1f;
		Bool.FalseColor.a = 1f;

		FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

		foreach (FieldInfo info in fields)
		{
			if (info.GetValue(this).GetType().Equals(typeof(TypeData)))
			{
				TypeData data = (TypeData) info.GetValue(this);

				data.Color.a = 1f;

				info.SetValue(this, data);
			}
		}
	}
}
