using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeTest : MonoBehaviour
{
	[DebugDisplay] public byte testByte = 255;
	[DebugDisplay] public sbyte testSByte = 127;
	[DebugDisplay] public short testShort = 32767;
	[DebugDisplay] public ushort testUShort = 65535;
	[DebugDisplay] public int testInt = 1000000;
	[DebugDisplay] public uint testUInt = 1000000;
	[DebugDisplay] public long testLong = 1000000;
	[DebugDisplay] public ulong testULong = 2000000;
	[DebugDisplay] public float testFloat = 0.03f;
	[DebugDisplay] public double testDouble = 0.09f;
	[DebugDisplay] public char testChar = 'c';
	[DebugDisplay] public string testString = "String";
	[DebugDisplay] public Vector3 vectorTest = new Vector3(1f, 2f, 3f);
	[DebugDisplay] public Color colorTest = new Color(1f, 0.5f, 0.2f);
}
