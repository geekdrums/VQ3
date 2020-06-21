using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class ColorGameSyncByParameter : ColorGameSync<float, float, ColorPropertyEditByRate>
{
	public override string Name { get { return ParameterName; } }

	public string ParameterName;
	public float MinParam = 0, MaxParam = 1;

	public override float Process(float input)
	{
		if( MaxParam <= MinParam ) return 0;
		return Mathf.Clamp01((input - MinParam) / (MaxParam - MinParam));
	}
}

[Serializable]
public class ColorPropertyEditByRate : ColorPropertyEditBase<float>
{
	public float MinValue, MaxValue;
	public override float GetEditValue(float rate) { return MinValue + rate * (MaxValue - MinValue); }
}