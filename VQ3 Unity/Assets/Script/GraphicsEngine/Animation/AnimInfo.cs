using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class AnimInfo
{
	public float TargetFloat;
	public int TargetInt;
	public Color TargetColor;
	public Vector3 TargetVector;
	public string TargetString;

	public float InitialFloat;
	public Color InitialColor;
	public Vector3 InitialVector;

	public UnityEngine.Object Object;
	public AnimParamType AnimParam;
	public InterpType Interp;
	public TimeUnitType TimeUnit;
	public float Time;
	public float Delay;
	public AnimEndOption EndOption;

	public bool HasInitialValue;
	public string PropertyName;

	public bool IsSelected;

	public object GetTarget()
	{
		switch( AnimParam )
		{
			case AnimParamType.Scale:
			case AnimParamType.ScaleX:
			case AnimParamType.ScaleY:
			case AnimParamType.ScaleZ:
			case AnimParamType.RotationX:
			case AnimParamType.RotationY:
			case AnimParamType.RotationZ:
			case AnimParamType.PositionX:
			case AnimParamType.PositionY:
			case AnimParamType.PositionZ:
			case AnimParamType.SizeDeltaX:
			case AnimParamType.SizeDeltaY:
			case AnimParamType.AnchoredPositionX:
			case AnimParamType.AnchoredPositionY:
			case AnimParamType.PrimitiveRadius:
			case AnimParamType.PrimitiveWidth:
			case AnimParamType.PrimitiveArc:
			case AnimParamType.PrimitiveStartArc:
			case AnimParamType.GaugeLength:
			case AnimParamType.GaugeRate:
			case AnimParamType.GaugeStartRate:
			case AnimParamType.GaugeWidth:
			case AnimParamType.AlphaColor:
			case AnimParamType.TextAlphaColor:
			case AnimParamType.ShaderFloat:
			{
				return TargetFloat;
			}
			case AnimParamType.Position:
			case AnimParamType.ShaderVector3:
			{
				return TargetVector;
			}
			case AnimParamType.AnchoredPosition:
			case AnimParamType.SizeDelta:
			case AnimParamType.ShaderVector2:
			{
				return (Vector2)TargetVector;
			}
			case AnimParamType.Color:
			case AnimParamType.TextColor:
			case AnimParamType.ShaderColor:
			{
				return TargetColor;
			}
			case AnimParamType.Text:
			{
				return TargetString;
			}
			case AnimParamType.Rotation:
			{
				return Quaternion.Euler(TargetVector);
			}
			case AnimParamType.Blink:
			{
				return TargetInt;
			}
			case AnimParamType.Flash:
			case AnimParamType.TurnOn:
			case AnimParamType.TurnOff:
			default:
			{
				return null;
			}
		}
	}

	public object GetInitial()
	{
		if( HasInitialValue == false )
		{
			return null;
		}

		switch( AnimParam )
		{
			case AnimParamType.Scale:
			case AnimParamType.ScaleX:
			case AnimParamType.ScaleY:
			case AnimParamType.ScaleZ:
			case AnimParamType.RotationX:
			case AnimParamType.RotationY:
			case AnimParamType.RotationZ:
			case AnimParamType.PositionX:
			case AnimParamType.PositionY:
			case AnimParamType.PositionZ:
			case AnimParamType.SizeDeltaX:
			case AnimParamType.SizeDeltaY:
			case AnimParamType.AnchoredPositionX:
			case AnimParamType.AnchoredPositionY:
			case AnimParamType.PrimitiveRadius:
			case AnimParamType.PrimitiveWidth:
			case AnimParamType.PrimitiveArc:
			case AnimParamType.PrimitiveStartArc:
			case AnimParamType.GaugeLength:
			case AnimParamType.GaugeRate:
			case AnimParamType.GaugeStartRate:
			case AnimParamType.GaugeWidth:
			case AnimParamType.AlphaColor:
			case AnimParamType.TextAlphaColor:
			case AnimParamType.ShaderFloat:
			{
				return InitialFloat;
			}
			case AnimParamType.Position:
			case AnimParamType.ShaderVector3:
			{
				return InitialVector;
			}
			case AnimParamType.AnchoredPosition:
			case AnimParamType.SizeDelta:
			case AnimParamType.ShaderVector2:
			{
				return (Vector2)InitialVector;
			}
			case AnimParamType.Color:
			case AnimParamType.TextColor:
			case AnimParamType.ShaderColor:
			{
				return InitialColor;
			}
			case AnimParamType.Rotation:
			{
				return Quaternion.Euler(InitialVector);
			}
			case AnimParamType.Text:
			case AnimParamType.TurnOn:
			case AnimParamType.TurnOff:
			default:
			{
				return null;
			}
		}
	}

	public override string ToString()
	{
		return string.Format("{0}	| {1}	| {2}", AnimParam.ToString(), GetTarget() != null ? GetTarget().ToString() : "", Interp.ToString());
	}
}
