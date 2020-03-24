using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DebugDisplayAttribute : Attribute
{
	public string displayName;

	public DebugDisplayAttribute()
	{
	}

	public DebugDisplayAttribute(string name)
	{
		displayName = name;
	}
}