using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum ColorPropertyType
{
	H,
	S,
	V,
	A,
}

public enum ColorPropertyEditType
{
	None,
	Add,
	Minus,
	Multiply,
	Override,
}

public static class ColorPropertyUtil
{
	public static void ApplyEdit(ref float value, ColorPropertyType propertyType, ColorPropertyEditType editType, float editValue)
	{
		switch( editType )
		{
			case ColorPropertyEditType.Add:
				value += editValue;
				break;
			case ColorPropertyEditType.Minus:
				value -= editValue;
				break;
			case ColorPropertyEditType.Multiply:
				value *= editValue;
				break;
			case ColorPropertyEditType.Override:
				value = editValue;
				break;
			case ColorPropertyEditType.None:
				return;
		}

		switch( propertyType )
		{
			case ColorPropertyType.H:
				if( value < 0 )
				{
					value = (value % 1) + 1;
				}
				else
				{
					value = value % 1;
				}
				break;
			case ColorPropertyType.S:
			case ColorPropertyType.V:
			case ColorPropertyType.A:
				value = Mathf.Clamp01(value);
				break;
		}
	}

	public static bool CalcTargetEditValue(float source, float target, ColorPropertyEditType editType, out float editValue)
	{
		switch( editType )
		{
			case ColorPropertyEditType.Add:
				editValue = target - source;
				return true;
			case ColorPropertyEditType.Minus:
				editValue = source - target;
				return true;
			case ColorPropertyEditType.Multiply:
				if( source == 0.0f )
				{
					editValue = 0;
					return false;
				}
				else
				{
					editValue = target / source;
					return true;
				}
			case ColorPropertyEditType.Override:
				editValue = target;
				return true;
			default: 
				editValue = 0;
				return false;
		}
	}

	public static Color MakeAlpha(Color color, float alpha)
	{
		return new Color(color.r, color.g, color.b, color.a * alpha);
	}

	public static Color FromHSVA(float h, float s, float v, float a)
	{
		Color color = Color.HSVToRGB(h, s, v);
		return new Color(color.r, color.g, color.b, a);
	}

	public static void ToHSVA(Color rgba, out float h, out float s, out float v, out float a)
	{
		a = rgba.a;
		Color.RGBToHSV(rgba, out h, out s, out v);
	}
}