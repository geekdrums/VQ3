using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class ColorGameSyncByState : ColorGameSync<string, int, ColorPropertyEditByIndex>
{
	public override string Name { get { return GroupName; } }

	public string GroupName;
	public List<string> States = new List<string>();

	public override int Process(string input)
	{
		if( string.IsNullOrEmpty(input) ) return -1;
		else return States.IndexOf(input);
	}

	public int IndexOf(string state) { return States.IndexOf(state); }
}

[Serializable]
public class ColorPropertyEditByIndex : ColorPropertyEditBase<int>
{
	public float[] Values;
	public override float GetEditValue(int index)
	{
		if( 0 <= index && index < Values.Length )
		{
			return Values[index];
		}
		else
		{
			return 0.0f;
		}
	}
}
