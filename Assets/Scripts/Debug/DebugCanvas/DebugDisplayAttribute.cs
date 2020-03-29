using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DebugDisplayAttribute : Attribute
{
	public string DisplayName;

	public DebugDisplayAttribute()
	{
	}

	public DebugDisplayAttribute(string name)
	{
		DisplayName = name;
	}
}