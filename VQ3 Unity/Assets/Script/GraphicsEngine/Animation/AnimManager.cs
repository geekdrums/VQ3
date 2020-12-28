#define MUSIC_ENGINE

using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class AnimManager : MonoBehaviour
{
	public int FPS = 60;

	static AnimManager Instance
	{
		get
		{
			if( instance_ == null )
			{
				instance_ = UnityEngine.Object.FindObjectOfType<AnimManager>();
			}
			return instance_;
		}
	}
	static AnimManager instance_;

	List<AnimationBase> animations_ = new List<AnimationBase>();
	List<AnimationBase> removeAnims_ = new List<AnimationBase>();

	// Use this for initialization
	void Start()
	{
		Application.targetFrameRate = FPS;
	}
	
	// Update is called once per frame
	void Update()
	{
		// Removeされるように命令されたものをEndしてから抜く
		foreach( AnimationBase removeAnim in removeAnims_ )
		{
			animations_.Remove(removeAnim);
		}
		removeAnims_.Clear();

		// Updateしつつ、終わったものはRemoveリストに入れる
		foreach( AnimationBase anim in animations_ )
		{
			anim.Update();
			if( anim.IsEnd )
			{
				removeAnims_.Add(anim);
			}
		}
	}

	public static GameObject ToGameObject(Object obj)
	{
		GameObject gameObject = obj as GameObject;
		if( gameObject == null )
		{
			if( obj is Component )
			{
				gameObject = (obj as Component).gameObject;
			}
			else
			{
				throw new System.Exception("obj is not GameObject or Component");
			}
		}
		return gameObject;
	}

	public static AnimationBase CreateAnim(Object obj, object target, AnimParamType paramType, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec, float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		GameObject gameObject = ToGameObject(obj);
		AnimationBase anim = null;

		switch( paramType )
		{
			case AnimParamType.Scale:
			case AnimParamType.ScaleX:
			case AnimParamType.ScaleY:
			case AnimParamType.ScaleZ:
			case AnimParamType.Rotation:
			case AnimParamType.RotationX:
			case AnimParamType.RotationY:
			case AnimParamType.RotationZ:
			case AnimParamType.Position:
			case AnimParamType.PositionX:
			case AnimParamType.PositionY:
			case AnimParamType.PositionZ:
			{
				anim = new TransformAnimaion(gameObject, target, paramType, interpType, timeUnit, time, delay, endOption);
			}
			break;
			case AnimParamType.SizeDelta:
			case AnimParamType.SizeDeltaX:
			case AnimParamType.SizeDeltaY:
			case AnimParamType.AnchoredPosition:
			case AnimParamType.AnchoredPositionX:
			case AnimParamType.AnchoredPositionY:
			{
				anim = new RectTransformAnimaion(gameObject, target, paramType, interpType, timeUnit, time, delay, endOption);
			}
			break;
			case AnimParamType.PrimitiveRadius:
			case AnimParamType.PrimitiveWidth:
			case AnimParamType.PrimitiveArc:
			case AnimParamType.PrimitiveStartArc:
			{
				anim = new PrimitiveAnimaion(gameObject, target, paramType, interpType, timeUnit, time, delay, endOption);
			}
			break;
			case AnimParamType.GaugeLength:
			case AnimParamType.GaugeRate:
			case AnimParamType.GaugeStartRate:
			case AnimParamType.GaugeWidth:
			{
				anim = new GaugeAnimaion(gameObject, target, paramType, interpType, timeUnit, time, delay, endOption);
			}
			break;
			case AnimParamType.Color:
			case AnimParamType.AlphaColor:
			{
				anim = new ColorAnimaion(gameObject, target, paramType, interpType, timeUnit, time, delay, endOption);
			}
			break;
			case AnimParamType.Text:
			{
				anim = new TextAnimaion(gameObject, target, timeUnit, time, delay, endOption);
			}
			break;
			case AnimParamType.TextColor:
			case AnimParamType.TextAlphaColor:
			{
				anim = new TextColorAnimaion(gameObject, target, paramType, interpType, timeUnit, time, delay, endOption);
			}
			break;
			case AnimParamType.ShaderFloat:
			case AnimParamType.ShaderVector2:
			case AnimParamType.ShaderVector3:
			case AnimParamType.ShaderColor:
			{
				anim = new ShaderAnimation(gameObject, target, paramType, interpType, timeUnit, time, delay, endOption);
			}
			break;
			case AnimParamType.Blink:
			{
				anim = new BlinkAnimaion(gameObject, (int)target, timeUnit, time, delay, endOption);
			}
			break;
			case AnimParamType.Flash:
			{
				anim = new FlashAnimaion(gameObject, timeUnit, time, delay, endOption);
			}
			break;
			case AnimParamType.TurnOn:
			case AnimParamType.TurnOff:
			{
				anim = new OnOffAnimaion(gameObject, paramType, timeUnit, time, delay, endOption);
			}
			break;
			default:
			{
				print("unknown param type! " + paramType.ToString());
			}
			break;
		}

		return anim;
	}

	public static AnimationBase AddAnim(Object obj, object target, AnimParamType paramType, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		AnimationBase anim = CreateAnim(obj, target, paramType, interpType, timeUnit, time, delay, endOption);
		if( anim != null )
		{
			AddAnim(anim);
		}
		return anim;
	}

	public static AnimationBase AddAnim(AnimationBase animInfo)
	{
		Instance.animations_.Add(animInfo);
		return animInfo;
	}

	public static bool IsAnimating(Object obj, AnimParamType paramType = AnimParamType.Any, AnimationBase self = null)
	{
		GameObject gameObject = ToGameObject(obj);
		return Instance.animations_.Find
			((AnimationBase anim) => 
				(self == null || self != anim)
				&& (paramType == AnimParamType.Any || paramType == anim.Param)
				&& anim.Object == gameObject
				&& anim.State == AnimationBase.AnimationState.Playing
			) != null;
	}

	public static void RemoveAnim(AnimationBase anim)
	{
		if( Instance.removeAnims_.Contains(anim) == false && Instance.animations_.Contains(anim) )
		{
			Instance.removeAnims_.Add(anim);
		}
	}

	public static void RemoveOtherAnim(AnimationBase anim, bool includeDaly = false)
	{
		foreach( AnimationBase foundAnim in Instance.animations_.FindAll((AnimationBase other) => other != anim && other.Object == anim.Object && other.Param == anim.Param && (includeDaly || other.IsPlaying)) )
		{
			if( Instance.removeAnims_.Contains(foundAnim) == false )
			{
				foundAnim.End();
				Instance.removeAnims_.Add(foundAnim);
			}
		}
	}

	public static void RemoveOtherAnim(Object obj, AnimParamType type = AnimParamType.Any, bool includeDaly = true)
	{
		foreach( AnimationBase foundAnim in Instance.animations_.FindAll((AnimationBase other) => other.Object == ToGameObject(obj) && (type == AnimParamType.Any || type == other.Param) && (includeDaly || other.IsPlaying)) )
		{
			if( Instance.removeAnims_.Contains(foundAnim) == false )
			{
				foundAnim.End();
				Instance.removeAnims_.Add(foundAnim);
			}
		}
	}

	public static void RemoveChildAnim(Object obj, AnimParamType type = AnimParamType.Any, bool includeDaly = true)
	{
		foreach( AnimationBase foundAnim in Instance.animations_.FindAll((AnimationBase other) => other.Object.transform.IsChildOf(ToGameObject(obj).transform) && (type == AnimParamType.Any || type == other.Param) && (includeDaly || other.IsPlaying)) )
		{
			if( Instance.removeAnims_.Contains(foundAnim) == false )
			{
				foundAnim.End();
				Instance.removeAnims_.Add(foundAnim);
			}
		}
	}
}

public static class AnimFunctionExtents
{
	public static void RemoveOtherAnim(this GameObject obj)
	{
		AnimManager.RemoveOtherAnim(obj);
	}

	// --------------- AnimateGameObject --------------- //
	public static AnimationBase AnimateON(this GameObject obj, float time, TimeUnitType timeUnit = TimeUnitType.Sec, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(new OnOffAnimaion(obj, AnimParamType.TurnOn, timeUnit, time, 0.0f, endOption));
	}
	public static AnimationBase AnimateOFF(this GameObject obj, float time, TimeUnitType timeUnit = TimeUnitType.Sec, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(new OnOffAnimaion(obj, AnimParamType.TurnOff, timeUnit, time, 0.0f, endOption));
	}
	public static AnimationBase AnimateBlink(this GameObject obj, int interval, float time, float delay = 0.0f, TimeUnitType timeUnit = TimeUnitType.Sec, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(new BlinkAnimaion(obj, interval, timeUnit, time, delay, endOption));
	}
	public static AnimationBase AnimateFlash(this GameObject obj, float time, float delay = 0.0f, TimeUnitType timeUnit = TimeUnitType.Sec, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(new FlashAnimaion(obj, timeUnit, time, delay, endOption));
	}
	public static AnimationBase AnimateShake(this GameObject obj, Vector3 initialVector, float range, float time, float updateTime, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(new ShakeAnimaion(obj, initialVector, range, time, updateTime, AnimParamType.Position, interpType, timeUnit, delay, endOption));
	}

	// --------------- AnimatePosition --------------- //
	public static AnimationBase AnimatePosition(this Transform obj, Vector3 target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.Position, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimatePositionX(this Transform obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.PositionX, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimatePositionY(this Transform obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.PositionY, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimatePositionZ(this Transform obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.PositionZ, interpType, timeUnit, time, delay, endOption);
	}
	
	// --------------- AnimateScale --------------- //
	public static AnimationBase AnimateScale(this Transform obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.Scale, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateScaleX(this Transform obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.ScaleX, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateScaleY(this Transform obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.ScaleY, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateScaleZ(this Transform obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.ScaleZ, interpType, timeUnit, time, delay, endOption);
	}

	// --------------- AnimateRotation --------------- //
	public static AnimationBase AnimateRotation(this Transform obj, Quaternion target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.Rotation, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateRotationX(this Transform obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.RotationX, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateRotationY(this Transform obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.RotationY, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateRotationZ(this Transform obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.RotationZ, interpType, timeUnit, time, delay, endOption);
	}

	// --------------- AnimateAnchoredPosition --------------- //
	public static AnimationBase AnimateAnchoredPosition(this RectTransform obj, Vector2 target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.AnchoredPosition, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateAnchoredPositionX(this RectTransform obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.AnchoredPositionX, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateAnchoredPositionY(this RectTransform obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.AnchoredPositionY, interpType, timeUnit, time, delay, endOption);
	}

	// --------------- AnimateSizeDelta --------------- //
	public static AnimationBase AnimateSizeDelta(this RectTransform obj, Vector2 target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.SizeDelta, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateSizeDeltaX(this RectTransform obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.SizeDeltaX, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateSizeDeltaY(this RectTransform obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.SizeDeltaY, interpType, timeUnit, time, delay, endOption);
	}
	
	// --------------- AnimateUIMidairPrimitive --------------- //
	public static AnimationBase AnimateRadius(this UIMidairPrimitive obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.PrimitiveRadius, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateWidth(this UIMidairPrimitive obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.PrimitiveWidth, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateArc(this UIMidairPrimitive obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.PrimitiveArc, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateStartArc(this UIMidairPrimitive obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec, float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.PrimitiveStartArc, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateColor(this UIMidairPrimitive obj, Color target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.Color, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateAlphaColor(this UIMidairPrimitive obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.AlphaColor, interpType, timeUnit, time, delay, endOption);
	}

	// --------------- AnimateMidairPrimitive --------------- //
	public static AnimationBase AnimateRadius(this MidairPrimitive obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.PrimitiveRadius, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateWidth(this MidairPrimitive obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.PrimitiveWidth, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateArc(this MidairPrimitive obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.PrimitiveArc, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateStartArc(this MidairPrimitive obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec, float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.PrimitiveStartArc, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateColor(this MidairPrimitive obj, Color target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.Color, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateAlphaColor(this MidairPrimitive obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.AlphaColor, interpType, timeUnit, time, delay, endOption);
	}

	// --------------- AnimateUIGaugeRenderer --------------- //
	public static AnimationBase AnimateLength(this UIGaugeRenderer obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.GaugeLength, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateRate(this UIGaugeRenderer obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.GaugeRate, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateStartRate(this UIGaugeRenderer obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec, float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.GaugeStartRate, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateWidth(this UIGaugeRenderer obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.GaugeWidth, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateColor(this UIGaugeRenderer obj, Color target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.Color, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateAlphaColor(this UIGaugeRenderer obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.AlphaColor, interpType, timeUnit, time, delay, endOption);
	}

	// --------------- AnimateGaugeRenderer --------------- //
	public static AnimationBase AnimateLength(this GaugeRenderer obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.GaugeLength, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateRate(this GaugeRenderer obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.GaugeRate, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateStartRate(this GaugeRenderer obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec, float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.GaugeStartRate, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateWidth(this GaugeRenderer obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.GaugeWidth, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateColor(this GaugeRenderer obj, Color target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.Color, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateAlphaColor(this GaugeRenderer obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.AlphaColor, interpType, timeUnit, time, delay, endOption);
	}


	// --------------- AnimateColor --------------- //
	public static AnimationBase AnimateColor(this Image obj, Color target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec, float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.Color, interpType, timeUnit, time, delay, endOption);
	}

	// --------------- AnimateTextColor --------------- //
	public static AnimationBase AnimateColor(this Text obj, Color target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.TextColor, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateAlphaColor(this Text obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.TextAlphaColor, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateColor(this TextMesh obj, Color target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.TextColor, interpType, timeUnit, time, delay, endOption);
	}
	public static AnimationBase AnimateAlphaColor(this TextMesh obj, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec,  float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		return AnimManager.AddAnim(obj.gameObject, target, AnimParamType.TextAlphaColor, interpType, timeUnit, time, delay, endOption);
	}

	// --------------- AnimateShader --------------- //
	public static AnimationBase AnimateFloat(this Renderer obj, string propName, float target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec, float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		ShaderAnimation anim = (ShaderAnimation)AnimManager.CreateAnim(obj.gameObject, target, AnimParamType.TextColor, interpType, timeUnit, time, delay, endOption);
		anim.SetPropertyName(propName);
		return AnimManager.AddAnim(anim);
	}
	public static AnimationBase AnimateVector2(this Renderer obj, string propName, Vector2 target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec, float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		ShaderAnimation anim = (ShaderAnimation)AnimManager.CreateAnim(obj.gameObject, target, AnimParamType.TextColor, interpType, timeUnit, time, delay, endOption);
		anim.SetPropertyName(propName);
		return AnimManager.AddAnim(anim);
	}
	public static AnimationBase AnimateVector3(this Renderer obj, string propName, Vector3 target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec, float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		ShaderAnimation anim = (ShaderAnimation)AnimManager.CreateAnim(obj.gameObject, target, AnimParamType.TextColor, interpType, timeUnit, time, delay, endOption);
		anim.SetPropertyName(propName);
		return AnimManager.AddAnim(anim);
	}
	public static AnimationBase AnimateColor(this Renderer obj, string propName, Color target, InterpType interpType = InterpType.Linear, TimeUnitType timeUnit = TimeUnitType.Sec, float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		ShaderAnimation anim = (ShaderAnimation)AnimManager.CreateAnim(obj.gameObject, target, AnimParamType.TextColor, interpType, timeUnit, time, delay, endOption);
		anim.SetPropertyName(propName);
		return AnimManager.AddAnim(anim);
	}

}
