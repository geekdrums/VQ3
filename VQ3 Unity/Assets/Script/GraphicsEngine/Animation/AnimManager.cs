#define MUSIC_ENGINE

using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public enum AnimParamType
{
	// for RemoveAnim
	Any,

	//Transform
	Scale,
	ScaleX,
	ScaleY,
	ScaleZ,
	Rotation,
	RotationX,
	RotationY,
	RotationZ,
	Position,
	PositionX,
	PositionY,
	PositionZ,

	//RectTransform
	SizeDelta,
	SizeDeltaX,
	SizeDeltaY,
	AnchoredPosition,
	AnchoredPositionX,
	AnchoredPositionY,

	//MidairPrimitive
	PrimitiveRadius,
	PrimitiveWidth,
	PrimitiveArc,
	PrimitiveStartArc,

	//Gauge
	GaugeLength,
	GaugeRate,
	GaugeStartRate,
	GaugeWidth,

	//IColoredObject
	Color,
	AlphaColor,

	//Text
	Text,
	TextColor,
	TextAlphaColor,

	// Shader
	ShaderFloat,
	ShaderVector2,
	ShaderVector3,
	ShaderColor,

	// OnOff
	TurnOn,
	TurnOff,
	Blink,
	Flash,
}

public enum InterpType
{
	Linear,
	BackIn,
	BackOut,
	BackInOut,
	QuadIn,
	QuadOut,
	QuadInOut,
	CubicIn,
	CubicOut,
	CubicInOut,
	QuartIn,
	QuartOut,
	QuartInOut,
	ExpoIn,
	ExpoOut,
	ExpoInOut,
	CircIn,
	CircOut,
	CircInOut,
	SinIn,
	SinOut,
	SinInOut,
	Step,
}

public enum TimeUnitType
{
	Sec,
	MSec,
	Bar,
	Beat,
	Unit,
}

public static class TimeUtility
{
	public static float DefaultBPM = 220;

	public static float ConvertTime(float time, TimeUnitType from, TimeUnitType to = TimeUnitType.Sec)
	{
		if( from == to ) return time;

		float sec = time;
		if( from == TimeUnitType.Sec )
		{
			sec = time;
		}
		else if( from == TimeUnitType.MSec )
		{
			sec = time / 1000.0f;
		}
		else
		{
#if false //MUSIC_ENGINE
			if( Music.HasValidMeter )
			{
				switch( from )
				{
					case TimeUnitType.Bar:
						sec = time * (float)Music.Meter.SecPerBar;
						break;
					case TimeUnitType.Beat:
						sec = time * (float)Music.Meter.SecPerBeat;
						break;
					case TimeUnitType.Unit:
						sec = time * (float)Music.Meter.SecPerUnit;
						break;
				}
			}
#endif
			switch( from )
			{
				case TimeUnitType.Bar:
					sec = time * (60.0f * 4.0f / DefaultBPM);
					break;
				case TimeUnitType.Beat:
					sec = time * (60.0f / DefaultBPM);
					break;
				case TimeUnitType.Unit:
					sec = time * (60.0f / 4.0f / DefaultBPM);
					break;
			}
		}
		
		if( to == TimeUnitType.Sec )
		{
			return sec;
		}
		else if( to == TimeUnitType.MSec )
		{
			return sec * 1000.0f;
		}
		else
		{
#if false //MUSIC_ENGINE
			if( Music.HasValidMeter )
			{
				switch( to )
				{
					case TimeUnitType.Bar:
						return sec / (float)Music.Meter.SecPerBar;
					case TimeUnitType.Beat:
						return sec / (float)Music.Meter.SecPerBeat;
					case TimeUnitType.Unit:
						return sec / (float)Music.Meter.SecPerUnit;
				}
			}
#endif
			switch( to )
			{
				case TimeUnitType.Bar:
					return sec / (60.0f * 4.0f / DefaultBPM);
				case TimeUnitType.Beat:
					return sec / (60.0f / DefaultBPM);
				case TimeUnitType.Unit:
					return sec / (60.0f / 4.0f / DefaultBPM);
			}
		}

		return sec;
	}
}

public enum AnimEndOption
{
	None,
	Loop,
	LoopBack,
	Destroy,
	Deactivate,
}

public abstract class AnimationBase
{
	#region properties

	public GameObject Object { get; protected set; }
	public AnimParamType Param { get; protected set; }
	public InterpType Interp { get; protected set; }
	public TimeUnitType TimeUnit { get; protected set; }
	public float DelayTimeSec { get; protected set; }
	public float AnimTimeSec { get; protected set; }
	public AnimEndOption EndOption { get; protected set; }

	protected object targetValue_;
	protected object initialValue_;

	protected float normalizedValue_;
	protected float animValue_;
	protected float remainDelayTime_;
	
	public enum AnimationState
	{
		Ready,
		Delay,
		Playing,
		End,
	}
	public AnimationState State { get; protected set; }

	public bool IsEnd
	{
		get
		{
			return normalizedValue_ >= 1.0f;
		}
	}
	public float TotalTimeSec { get { return DelayTimeSec + AnimTimeSec; } }
	public bool IsPlaying { get { return State == AnimationState.Playing; } }

	protected float currentValueFloat
	{
		get
		{
			if( targetValue_ is float == false || initialValue_ is float == false )
			{
				Debug.LogError(string.Format("value is not float! Object: {0}, Param: {1}", Object, Param));
				return 0;
			}
			return (float)initialValue_ + ((float)targetValue_ - (float)initialValue_) * animValue_;
		}
	}
	protected Vector3 currentValueVector3 { get { return (Vector3)initialValue_ + ((Vector3)targetValue_ - (Vector3)initialValue_) * animValue_; } }
	protected Vector2 currentValueVector2 { get { return (Vector2)initialValue_ + ((Vector2)targetValue_ - (Vector2)initialValue_) * animValue_; } }
	protected Color currentValueColor { get { return Color.Lerp((Color)initialValue_, (Color)targetValue_, animValue_); } }

	protected event System.Action<AnimationBase> OnStart;
	protected event System.Action<AnimationBase> OnEnd;

	#endregion


	public AnimationBase(GameObject obj, object target, AnimParamType paramType, InterpType interpType, TimeUnitType timeUnit, float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
	{
		Object = obj;
		targetValue_ = target;
		Param = paramType;
		Interp = interpType;
		TimeUnit = timeUnit;
		AnimTimeSec = TimeUtility.ConvertTime(time, timeUnit);
		DelayTimeSec = TimeUtility.ConvertTime(delay, timeUnit);
		EndOption = endOption;

		normalizedValue_ = 0;
		animValue_ = 0;
		remainDelayTime_ = DelayTimeSec;

		if( targetValue_ is int )
		{
			int intTarget = (int)targetValue_;
			targetValue_ = (float)intTarget;
		}

		State = AnimationState.Ready;
	}


	#region play / stop

	public void Play()
	{
		AnimManager.AddAnim(this);
	}

	public void Stop()
	{
		AnimManager.RemoveAnim(this);
	}

	public void Reset()
	{
		if( initialValue_ == null )
		{
			CacheInitialValue();
		}
		normalizedValue_ = 0;
		animValue_ = 0;
		remainDelayTime_ = DelayTimeSec;

		State = AnimationState.Ready;
		UpdateAnimation();
		AnimManager.RemoveAnim(this);
	}

	public void End()
	{
		if( initialValue_ == null )
		{
			CacheInitialValue();
		}
		normalizedValue_ = 1.0f;
		animValue_ = 1.0f;
		remainDelayTime_ = 0;

		State = AnimationState.End;
		UpdateAnimation();
		if( OnEnd != null )
		{
			OnEnd(this);
		}
	}

	#endregion


	#region update

	public void Update()
	{
		if( Object == null )
		{
			normalizedValue_ = 1.0f;
			animValue_ = 1.0f;
			remainDelayTime_ = 0;
			State = AnimationState.End;
			return;
		}

		if( State == AnimationState.Ready )
		{
			if( remainDelayTime_ <= 0 )
			{
				OnStartAnim();
			}
			else // if( remainDelayTime_ > 0 )
			{
				State = AnimationState.Delay;
			}
		}

		if( State == AnimationState.Delay )
		{
			remainDelayTime_ -= UnityEngine.Time.deltaTime;
			if( remainDelayTime_ <= 0 )
			{
				OnStartAnim();
			}
		}

		if( State == AnimationState.Playing )
		{
			UpdateTimeValue();
			UpdateAnimation();

			if( IsEnd )
			{
				OnEndAnim();
			}
		}
	}

	protected abstract void CacheInitialValue();

	public void ClearInitialValue()
	{
		initialValue_ = null;
	}

	protected void OnStartAnim()
	{
		if( AnimManager.IsAnimating(Object, Param, self: this) )
		{
			//Debug.Log("still animating!");
			return;
		}

		State = AnimationState.Playing;

		if( OnStart != null )
		{
			OnStart(this);
		}

		if( initialValue_ == null )
		{
			CacheInitialValue();
		}

		if( ( initialValue_ is float	&& targetValue_ is float && (float)initialValue_ == (float)targetValue_ )
		 || ( initialValue_ is Color	&& Color.Equals(initialValue_, targetValue_) )
		 || ( initialValue_ is Vector3	&& Vector3.Equals(initialValue_, targetValue_) ) )
		{
			normalizedValue_ = 1.0f;
			animValue_ = 1.0f;
			remainDelayTime_ = 0;
			State = AnimationState.End;
			OnEndAnim();
		}
	}
	
	protected void OnEndAnim()
	{
		if( Object == null ) return;

		if( EndOption == AnimEndOption.Loop )
		{
			normalizedValue_ = 0;
			animValue_ = 0;
			return;
		}
		else if( EndOption == AnimEndOption.LoopBack )
		{
			object newTarget = initialValue_;
			initialValue_ = targetValue_;
			targetValue_ = newTarget;
			normalizedValue_ = 0;
			animValue_ = 0;
			return;
		}
		else if( AnimManager.IsAnimating(Object, self: this) == false )
		{
			if( EndOption == AnimEndOption.Destroy )
			{
				GameObject.Destroy(Object.gameObject);
			}
			else if( EndOption == AnimEndOption.Deactivate )
			{
				Object.gameObject.SetActive(false);
			}
		}

		State = AnimationState.End;

		if( OnEnd != null )
		{
			OnEnd(this);
		}
	}

	protected void UpdateTimeValue()
	{
		normalizedValue_ += UnityEngine.Time.deltaTime / AnimTimeSec;
		normalizedValue_ = Mathf.Clamp01(normalizedValue_);
		float t = normalizedValue_;
		// reference: https://github.com/gamereat/EasingFunction-Unity/blob/master/Scripts/EasingLerps.cs
		switch( Interp )
		{
			case InterpType.Linear:
			animValue_ = t;
			break;
			case InterpType.BackIn:
			{
				float r = -t;
				animValue_ = -r * r * ((overshoot + 1) * r + overshoot);
			}
			break;
			case InterpType.BackOut:
			{
				float r = t - 1;
				animValue_ = r * r * ((overshoot + 1) * r + overshoot) + 1;
			}
			break;
			case InterpType.BackInOut:
			{
				float r = t * 2.0f;
				float s = overshoot * (1.525f);
				if( r < 1.0f )
				{
					animValue_ = 0.5f * (r * r * (s + 1) * r - s);
				}
				else
				{
					r -= 2.0f;
					animValue_ = 0.5f * (r * r * (s + 1) * r + s) + 2;
				}
			}
			break;
			case InterpType.QuadIn:
			{
				animValue_ = t * t;
			}
			break;
			case InterpType.QuadOut:
			{
				animValue_ = -t * (t - 2);
			}
			break;
			case InterpType.QuadInOut:
			{
				float r = t * 2.0f;
				if( r < 1.0f )
				{
					animValue_ = 0.5f * r * r;
				}
				else
				{
					r -= 1.0f;
					animValue_ = -0.5f * (r * (r - 2) - 1);
				}
			}
			break;
			case InterpType.CubicIn:
			{
				animValue_ = t * t * t;
			}
			break;
			case InterpType.CubicOut:
			{
				float r = t - 1.0f;
				animValue_ = r * r * r + 1;
			}
			break;
			case InterpType.CubicInOut:
			{
				float r = t * 2.0f;
				if( r < 1.0f )
				{
					animValue_ = 0.5f * r * r * r;
				}
				else
				{
					r -= 2.0f;
					animValue_ = 0.5f * (r * r * r + 2);
				}
			}
			break;
			case InterpType.QuartIn:
			{
				animValue_ = t * t * t * t;
			}
			break;
			case InterpType.QuartOut:
			{
				float r = t - 1.0f;
				animValue_ = -(r * r * r * r - 1);
			}
			break;
			case InterpType.QuartInOut:
			{
				float r = t * 2.0f;
				if( r < 1.0f )
				{
					animValue_ = 0.5f * r * r * r * r;
				}
				else
				{
					r -= 2.0f;
					animValue_ = -0.5f * (r * r * r * r - 2);
				}
			}
			break;
			case InterpType.ExpoIn:
			{
				if( t <= 0.0f )
				{
					animValue_ = 0;
				}
				else
				{
					animValue_ = (float)System.Math.Pow(2, 10 * (t - 1));
				}
			}
			break;
			case InterpType.ExpoOut:
			{
				if( t >= 1.0f )
				{
					animValue_ = 1.0f;
				}
				else
				{
					animValue_ = 1.0f - (float)System.Math.Pow(2, -10 * t);
				}
			}
			break;
			case InterpType.ExpoInOut:
			{
				if( t <= 0.0f )
				{
					animValue_ = 0;
				}
				else if( t >= 1.0f )
				{
					animValue_ = 1.0f;
				}
				else
				{
					float r = t * 2.0f;
					if( r < 1.0f )
					{
						animValue_ = 0.5f * Mathf.Pow(2, 10 * (r - 1));
					}
					else
					{
						r -= 1.0f;
						animValue_ = 0.5f * (-Mathf.Pow(2, -10 * r) + 2.0f);
					}
				}
			}
			break;
			case InterpType.CircIn:
			{
				animValue_ = -(Mathf.Sqrt(1 - t * t) - 1);
			}
			break;
			case InterpType.CircOut:
			{
				float r = t - 1.0f;
				animValue_ = Mathf.Sqrt(1 - r * r);
			}
			break;
			case InterpType.SinIn:
			{
				animValue_ = Mathf.Sin((normalizedValue_ + 3) * Mathf.PI / 2) + 1;
			}
			break;
			case InterpType.SinOut:
			{
				animValue_ = Mathf.Sin(normalizedValue_ * Mathf.PI / 2);
			}
			break;
			case InterpType.SinInOut:
			{
				animValue_ = (Mathf.Sin((2 * normalizedValue_ + 3) * Mathf.PI / 2) + 1) / 2.0f;
			}
			break;
			case InterpType.CircInOut:
			{
				float r = t * 2.0f;
				if( r < 1.0f )
				{
					animValue_ = 0.5f * -((float)System.Math.Sqrt(1 - t * t) - 1.0f);
				}
				else
				{
					r -= 2.0f;
					animValue_ = 0.5f * ((float)System.Math.Sqrt(1 - r * r) + 1.0f);
				}
			}
			break;
			case InterpType.Step:
			{
				animValue_ = normalizedValue_ < 1.0f ? 0 : 1.0f;
			}
			break;
		}
	}

	protected abstract void UpdateAnimation();

	#endregion


	#region set

	public AnimationBase From(object initValue)
	{
		initialValue_ = initValue;
		UpdateAnimation();
		return this;
	}

	public AnimationBase SetOnStartEvent(System.Action<AnimationBase> startEvent)
	{
		OnStart += startEvent;
		return this;
	}

	public AnimationBase SetOnEndEvent(System.Action<AnimationBase> endEvent)
	{
		OnEnd += endEvent;
		return this;
	}

	public AnimationBase SetDelay(float delay, TimeUnitType timeUnitType = TimeUnitType.Sec)
	{
		remainDelayTime_ = TimeUtility.ConvertTime(delay, timeUnitType);
		return this;
	}

	public AnimationBase AddDelay(float delay, TimeUnitType timeUnitType = TimeUnitType.Sec)
	{
		remainDelayTime_ += TimeUtility.ConvertTime(delay, timeUnitType);
		return this;
	}

	public AnimationBase Sequence(AnimationBase nextAnim)
	{
		OnEnd += (AnimationBase anim) =>
		{
			if( nextAnim.State == AnimationState.Ready )
			{
				AnimManager.AddAnim(nextAnim);
			}
		};
		return this;
	}

	#endregion


	#region utils

	public static float overshoot = 1.70158f;

	#endregion
}

public class OnOffAnimaion : AnimationBase
{
	public OnOffAnimaion(GameObject obj, AnimParamType paramType, TimeUnitType timeUnit, float time, float delay, AnimEndOption endOption = AnimEndOption.None)
		: base(obj, paramType == AnimParamType.TurnOn, paramType, InterpType.Linear, timeUnit, time, delay, endOption)
	{
		if( paramType != AnimParamType.TurnOn && paramType != AnimParamType.TurnOff )
		{
			throw new System.Exception("OnOffAnimaion: wrong param type! paramType = " + paramType.ToString());
		}
	}

	protected override void CacheInitialValue()
	{
		initialValue_ = Object.activeSelf;
	}

	protected override void UpdateAnimation()
	{
		Object.SetActive(IsEnd ? (bool)targetValue_ : !(bool)targetValue_);
	}
}

public class BlinkAnimaion : AnimationBase
{
	//protected float interval_;
	int interval_;
	int frameCnt_;

	public BlinkAnimaion(GameObject obj, int interval, TimeUnitType timeUnit, float time, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
		: base(obj, target: true, AnimParamType.Blink, InterpType.Linear, timeUnit, time, delay, endOption)
	{
		interval_ = interval;
	}

	protected override void CacheInitialValue()
	{
		initialValue_ = Object.activeSelf;
	}

	protected override void UpdateAnimation()
	{
		if( IsEnd )
		{
			Object.SetActive((bool)targetValue_);
			frameCnt_ = 0;
		}
		else if( State == AnimationState.Playing )
		{
			//float time = normalizedValue_ * AnimTimeSec;
			//Object.SetActive(time % (interval_ * 2) > interval_);
			++frameCnt_;
			if( interval_ <= frameCnt_ )
			{
				frameCnt_ = 0;
				Object.SetActive(!Object.activeSelf);
			}
		}
		else
		{
			Object.SetActive((bool)initialValue_);
			frameCnt_ = 0;
		}
	}
}

public class FlashAnimaion : AnimationBase
{
	protected float interval_;

	public FlashAnimaion(GameObject obj, TimeUnitType timeUnit, float time, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
		: base(obj, target: true, AnimParamType.Flash, InterpType.Linear, timeUnit, time, delay, endOption)
	{
	}

	protected override void CacheInitialValue()
	{
	}

	protected override void UpdateAnimation()
	{
		if( IsEnd || State == AnimationState.Ready )
		{
			Object.SetActive(false);
		}
		else
		{
			Object.SetActive(true);
		}
	}
}

public class TransformAnimaion : AnimationBase
{
	protected Transform transform_;

	public TransformAnimaion(GameObject obj, object target, AnimParamType paramType, InterpType interpType, TimeUnitType timeUnit, float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
		: base(obj, target, paramType, interpType, timeUnit, time, delay, endOption)
	{
		if( AnimParamType.PositionZ < paramType )
		{
			throw new System.Exception("TransformAnimaion: wrong param type! paramType = " + paramType.ToString());
		}

		transform_ = Object.transform;
	}

	protected override void CacheInitialValue()
	{
		switch( Param )
		{
		case AnimParamType.Scale:
			initialValue_ = (float)transform_.localScale.x;
			break;
		case AnimParamType.ScaleX:
			initialValue_ = (float)transform_.localScale.x;
			break;
		case AnimParamType.ScaleY:
			initialValue_ = (float)transform_.localScale.y;
			break;
		case AnimParamType.ScaleZ:
			initialValue_ = (float)transform_.localScale.z;
			break;
		case AnimParamType.Rotation:
			initialValue_ = transform_.localRotation;
			break;
		case AnimParamType.RotationX:
			initialValue_ = (float)(transform_.rotation.eulerAngles.y + 360) % 360;
			break;
		case AnimParamType.RotationY:
			initialValue_ = (float)(transform_.rotation.eulerAngles.y + 360) % 360;
			break;
		case AnimParamType.RotationZ:
			initialValue_ = (float)(transform_.rotation.eulerAngles.z + 360) % 360;
			break;
		case AnimParamType.Position:
			initialValue_ = transform_.localPosition;
			break;
		case AnimParamType.PositionX:
			initialValue_ = (float)transform_.localPosition.x;
			break;
		case AnimParamType.PositionY:
			initialValue_ = (float)transform_.localPosition.y;
			break;
		case AnimParamType.PositionZ:
			initialValue_ = (float)transform_.localPosition.z;
			break;
		}
	}

	protected override void UpdateAnimation()
	{
		switch( Param )
		{
		case AnimParamType.Scale:
			float uniformScale = currentValueFloat;
			transform_.localScale = new Vector3(uniformScale, uniformScale, uniformScale);
			break;
		case AnimParamType.ScaleX:
			transform_.localScale = new Vector3(currentValueFloat, transform_.localScale.y, transform_.localScale.z);
			break;
		case AnimParamType.ScaleY:
			transform_.localScale = new Vector3(transform_.localScale.x, currentValueFloat, transform_.localScale.z);
			break;
		case AnimParamType.ScaleZ:
			transform_.localScale = new Vector3(transform_.localScale.x, transform_.localScale.y, currentValueFloat);
			break;
		case AnimParamType.Rotation:
			transform_.localRotation = Quaternion.Lerp((Quaternion)initialValue_, (Quaternion)targetValue_, animValue_);
			break;
		case AnimParamType.RotationX:
			transform_.localRotation = Quaternion.AngleAxis(currentValueFloat, Vector3.right);
			break;
		case AnimParamType.RotationY:
			transform_.localRotation = Quaternion.AngleAxis(currentValueFloat, Vector3.up);
			break;
		case AnimParamType.RotationZ:
			transform_.localRotation = Quaternion.AngleAxis(currentValueFloat, Vector3.forward);
			break;
		case AnimParamType.Position:
			transform_.localPosition = currentValueVector3;
			break;
		case AnimParamType.PositionX:
			transform_.localPosition = new Vector3(currentValueFloat, transform_.localPosition.y, transform_.localPosition.z);
			break;
		case AnimParamType.PositionY:
			transform_.localPosition = new Vector3(transform_.localPosition.x, currentValueFloat, transform_.localPosition.z);
			break;
		case AnimParamType.PositionZ:
			transform_.localPosition = new Vector3(transform_.localPosition.x, transform_.localPosition.y, currentValueFloat);
			break;
		}
	}
}

public class RectTransformAnimaion : AnimationBase
{
	protected RectTransform rect_;

	public RectTransformAnimaion(GameObject obj, object target, AnimParamType paramType, InterpType interpType, TimeUnitType timeUnit, float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
		: base(obj, target, paramType, interpType, timeUnit, time, delay, endOption)
	{
		if( paramType < AnimParamType.SizeDelta || AnimParamType.AnchoredPositionY < paramType )
		{
			throw new System.Exception("RectTransformAnimaion: wrong param type! paramType = " + paramType.ToString());
		}

		rect_ = Object.GetComponent<RectTransform>();
	}

	protected override void CacheInitialValue()
	{
		switch( Param )
		{
		case AnimParamType.SizeDelta:
			initialValue_ = rect_.sizeDelta;
			break;
		case AnimParamType.SizeDeltaX:
			initialValue_ = (float)rect_.sizeDelta.x;
			break;
		case AnimParamType.SizeDeltaY:
			initialValue_ = (float)rect_.sizeDelta.y;
			break;
		case AnimParamType.AnchoredPosition:
			initialValue_ = rect_.anchoredPosition;
			break;
		case AnimParamType.AnchoredPositionX:
			initialValue_ = (float)rect_.anchoredPosition.x;
			break;
		case AnimParamType.AnchoredPositionY:
			initialValue_ = (float)rect_.anchoredPosition.y;
			break;
		}
	}

	protected override void UpdateAnimation()
	{
		switch( Param )
		{
		case AnimParamType.SizeDelta:
			rect_.sizeDelta = currentValueVector2;
			break;
		case AnimParamType.SizeDeltaX:
			rect_.sizeDelta = new Vector2(currentValueFloat, rect_.sizeDelta.y);
			break;
		case AnimParamType.SizeDeltaY:
			rect_.sizeDelta = new Vector2(rect_.sizeDelta.x, currentValueFloat);
			break;
		case AnimParamType.AnchoredPosition:
			rect_.anchoredPosition = currentValueVector2;
			break;
		case AnimParamType.AnchoredPositionX:
			rect_.anchoredPosition = new Vector2(currentValueFloat, rect_.anchoredPosition.y);
			break;
		case AnimParamType.AnchoredPositionY:
			rect_.anchoredPosition = new Vector2(rect_.anchoredPosition.x, currentValueFloat);
			break;
		}
	}
}

public class PrimitiveAnimaion : AnimationBase
{
	protected MidairPrimitive primitive_;
	protected UIMidairPrimitive uiprimitive_;

	public PrimitiveAnimaion(GameObject obj, object target, AnimParamType paramType, InterpType interpType, TimeUnitType timeUnit, float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
		: base(obj, target, paramType, interpType, timeUnit, time, delay, endOption)
	{
		if( paramType < AnimParamType.PrimitiveRadius || AnimParamType.PrimitiveStartArc < paramType )
		{
			throw new System.Exception("PrimitiveAnimaion: wrong param type! paramType = " + paramType.ToString());
		}

		primitive_ = Object.GetComponent<MidairPrimitive>();
		uiprimitive_ = Object.GetComponent<UIMidairPrimitive>();
	}

	protected override void CacheInitialValue()
	{
		if( primitive_ != null )
		{
			switch( Param )
			{
			case AnimParamType.PrimitiveRadius:
				initialValue_ = (float)primitive_.Radius;
				break;
			case AnimParamType.PrimitiveWidth:
				initialValue_ = (float)primitive_.Width;
				break;
			case AnimParamType.PrimitiveArc:
				initialValue_ = (float)primitive_.ArcRate;
				break;
			case AnimParamType.PrimitiveStartArc:
				initialValue_ = (float)primitive_.StartArcRate;
				break;
			}
		}
		else if( uiprimitive_ != null )
		{
			switch( Param )
			{
			case AnimParamType.PrimitiveRadius:
				initialValue_ = (float)uiprimitive_.Radius;
				break;
			case AnimParamType.PrimitiveWidth:
				initialValue_ = (float)uiprimitive_.Width;
				break;
			case AnimParamType.PrimitiveArc:
				initialValue_ = (float)uiprimitive_.ArcRate;
				break;
			case AnimParamType.PrimitiveStartArc:
				initialValue_ = (float)uiprimitive_.StartArcRate;
				break;
			}
		}
	}

	protected override void UpdateAnimation()
	{
		if( primitive_ != null )
		{
			switch( Param )
			{
			case AnimParamType.PrimitiveRadius:
				primitive_.SetSize(currentValueFloat);
				break;
			case AnimParamType.PrimitiveWidth:
				primitive_.SetWidth(currentValueFloat);
				break;
			case AnimParamType.PrimitiveArc:
				primitive_.SetArc(currentValueFloat);
				break;
			case AnimParamType.PrimitiveStartArc:
				primitive_.SetStartArc(currentValueFloat);
				break;
			}
		}
		else if( uiprimitive_ != null )
		{
			switch( Param )
			{
			case AnimParamType.PrimitiveRadius:
				uiprimitive_.SetSize(currentValueFloat);
				break;
			case AnimParamType.PrimitiveWidth:
				uiprimitive_.SetWidth(currentValueFloat);
				break;
			case AnimParamType.PrimitiveArc:
				uiprimitive_.SetArc(currentValueFloat);
				break;
			case AnimParamType.PrimitiveStartArc:
				uiprimitive_.SetStartArc(currentValueFloat);
				break;
			}
		}
	}
}

public class GaugeAnimaion : AnimationBase
{
	protected GaugeRenderer gauge_;
	protected UIGaugeRenderer uigauge_;

	public GaugeAnimaion(GameObject obj, object target, AnimParamType paramType, InterpType interpType, TimeUnitType timeUnit, float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
		: base(obj, target, paramType, interpType, timeUnit, time, delay, endOption)
	{
		if( paramType < AnimParamType.GaugeLength || AnimParamType.GaugeWidth < paramType )
		{
			throw new System.Exception("GaugeAnimaion: wrong param type! paramType = " + paramType.ToString());
		}

		gauge_ = Object.GetComponent<GaugeRenderer>();
		uigauge_ = Object.GetComponent<UIGaugeRenderer>();
	}
	
	protected override void CacheInitialValue()
	{
		gauge_ = Object.GetComponent<GaugeRenderer>();
		uigauge_ = Object.GetComponent<UIGaugeRenderer>();
		if( gauge_ != null )
		{
			switch( Param )
			{
			case AnimParamType.GaugeLength:
				initialValue_ = (float)gauge_.Length;
				break;
			case AnimParamType.GaugeRate:
				initialValue_ = (float)gauge_.Rate;
				break;
			case AnimParamType.GaugeStartRate:
				initialValue_ = (float)gauge_.StartRate;
				break;
			case AnimParamType.GaugeWidth:
				initialValue_ = (float)gauge_.Width;
				break;
			}
		}
		else if( uigauge_ != null )
		{
			switch( Param )
			{
			case AnimParamType.GaugeLength:
				initialValue_ = (float)uigauge_.Length;
				break;
			case AnimParamType.GaugeRate:
				initialValue_ = (float)uigauge_.Rate;
				break;
			case AnimParamType.GaugeStartRate:
				initialValue_ = (float)uigauge_.StartRate;
				break;
			case AnimParamType.GaugeWidth:
				initialValue_ = (float)uigauge_.Width;
				break;
			}
		}
	}

	protected override void UpdateAnimation()
	{
		if( gauge_ != null )
		{
			switch( Param )
			{
			case AnimParamType.GaugeLength:
				gauge_.Length = currentValueFloat;
				break;
			case AnimParamType.GaugeRate:
				gauge_.SetRate(currentValueFloat);
				break;
			case AnimParamType.GaugeStartRate:
				gauge_.SetStartRate(currentValueFloat);
				break;
			case AnimParamType.GaugeWidth:
				gauge_.SetWidth(currentValueFloat);
				break;
			}
		}
		else if( uigauge_ != null )
		{
			switch( Param )
			{
			case AnimParamType.GaugeLength:
				uigauge_.SetLength(currentValueFloat);
				break;
			case AnimParamType.GaugeRate:
				uigauge_.SetRate(currentValueFloat);
				break;
			case AnimParamType.GaugeStartRate:
				uigauge_.SetStartRate(currentValueFloat);
				break;
			case AnimParamType.GaugeWidth:
				uigauge_.SetWidth(currentValueFloat);
				break;
			}
		}
	}
}

public class ColorAnimaion : AnimationBase
{
	protected IColoredObject coloredObj_;
	protected Image image_;
	protected Color baseColor_;

	public ColorAnimaion(GameObject obj, object target, AnimParamType paramType, InterpType interpType, TimeUnitType timeUnit, float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
		: base(obj, target, paramType, interpType, timeUnit, time, delay, endOption)
	{
		if( paramType != AnimParamType.Color && paramType != AnimParamType.AlphaColor )
		{
			throw new System.Exception("ColorAnimaion: wrong param type! paramType = " + paramType.ToString());
		}

		coloredObj_ = Object.GetComponent<IColoredObject>();
		image_ = Object.GetComponent<Image>();
		if( Param == AnimParamType.AlphaColor )
		{
			baseColor_ = coloredObj_ != null ? coloredObj_.GetColor() : image_.color;
			baseColor_.a = 1.0f;
		}
	}

	protected override void CacheInitialValue()
	{
		if( coloredObj_ != null )
		{
			if( Param == AnimParamType.AlphaColor )
			{
				initialValue_ = coloredObj_.GetColor().a;
			}
			else
			{
				initialValue_ = coloredObj_.GetColor();
			}
		}
		else if( image_ != null )
		{
			if( Param == AnimParamType.AlphaColor )
			{
				initialValue_ = image_.color.a;
			}
			else
			{
				initialValue_ = image_.color;
			}
		}
	}

	protected override void UpdateAnimation()
	{
		if( coloredObj_ != null )
		{
			if( Param == AnimParamType.Color )
			{
				coloredObj_.SetColor(currentValueColor);
			}
			else if( Param == AnimParamType.AlphaColor )
			{
				coloredObj_.SetColor(ColorPropertyUtil.MakeAlpha(baseColor_, Mathf.Lerp((float)initialValue_, (float)targetValue_, animValue_)));
			}
		}
		else if( image_ != null )
		{
			if( Param == AnimParamType.Color )
			{
				image_.color = currentValueColor;
			}
			else if( Param == AnimParamType.AlphaColor )
			{
				image_.color = ColorPropertyUtil.MakeAlpha(baseColor_, Mathf.Lerp((float)initialValue_, (float)targetValue_, animValue_));
			}
		}
	}
}

public class TextAnimaion : AnimationBase
{
	protected TextMesh text_;
	protected Text uitext_;

	public TextAnimaion(GameObject obj, object target, TimeUnitType timeUnit, float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
		: base(obj, target, AnimParamType.Text, InterpType.Linear, timeUnit, time, delay, endOption)
	{
		uitext_ = Object.GetComponent<Text>();
		text_ = Object.GetComponent<TextMesh>();

		if( target == null || target is string == false )
		{
			throw new System.Exception("TextAnimaion: wrong target! target = " + target.ToString());
		}
	}

	protected override void CacheInitialValue()
	{
	}

	protected override void UpdateAnimation()
	{
		if( text_ != null )
		{
			text_.text = ((string)targetValue_).Substring(0, (int)(normalizedValue_ * ((string)targetValue_).Length));
		}
		else if( uitext_ != null )
		{
			uitext_.text = ((string)targetValue_).Substring(0, (int)(normalizedValue_ * ((string)targetValue_).Length));
		}
	}
}


public class TextColorAnimaion : AnimationBase
{
	protected TextMesh text_;
	protected Text uitext_;
	protected Color baseColor_;

	public TextColorAnimaion(GameObject obj, object target, AnimParamType paramType, InterpType interpType, TimeUnitType timeUnit, float time = 0.1f, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
		: base(obj, target, paramType, interpType, timeUnit, time, delay, endOption)
	{
		if( paramType != AnimParamType.TextColor && paramType != AnimParamType.TextAlphaColor )
		{
			throw new System.Exception("TextColorAnimaion: wrong param type! paramType = " + paramType.ToString());
		}

		uitext_ = Object.GetComponent<Text>();
		text_ = Object.GetComponent<TextMesh>();
		if( Param == AnimParamType.TextAlphaColor )
		{
			baseColor_ = (Color)uitext_.color;
			baseColor_.a = 1.0f;
		}
	}

	protected override void CacheInitialValue()
	{
		if( uitext_ != null )
		{
			if( Param == AnimParamType.TextAlphaColor )
			{
				initialValue_ = baseColor_.a;
			}
			else
			{
				initialValue_ = uitext_.color;
			}
		}
		else if( text_ != null )
		{
			if( Param == AnimParamType.TextAlphaColor )
			{
				initialValue_ = baseColor_.a;
			}
			else
			{
				initialValue_ = text_.color;
			}
		}
	}

	protected override void UpdateAnimation()
	{
		if( text_ != null )
		{
			if( Param == AnimParamType.TextColor )
			{
				text_.color = currentValueColor;
			}
			else if( Param == AnimParamType.TextAlphaColor )
			{
				text_.color = ColorPropertyUtil.MakeAlpha(baseColor_, Mathf.Lerp((float)initialValue_, (float)targetValue_, animValue_));
			}
		}
		else if( uitext_ != null )
		{
			if( Param == AnimParamType.TextColor )
			{
				uitext_.color = Color.Lerp((Color)initialValue_, (Color)targetValue_, animValue_);
			}
			else if( Param == AnimParamType.TextAlphaColor )
			{
				uitext_.color = ColorPropertyUtil.MakeAlpha(baseColor_, Mathf.Lerp((float)initialValue_, (float)targetValue_, animValue_));
			}
		}
	}
}

public class ShakeAnimaion : TransformAnimaion
{
	protected Vector3 initialVector_;
	protected float updateTime_;
	protected float timer_;

	public ShakeAnimaion(GameObject obj, Vector3 initialVector, float range, float time, float updateTime, AnimParamType paramType, InterpType interpType, TimeUnitType timeUnit, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
		: base(obj, range, paramType, interpType, timeUnit, time, delay, endOption)
	{
		if( paramType < AnimParamType.Position || AnimParamType.PositionZ < paramType )
		{
			throw new System.Exception("ShakeAnimInfo: wrong param type! paramType = " + paramType.ToString());
		}

		initialVector_ = initialVector;
		timer_ = 0;
		updateTime_ = updateTime;
	}

	protected override void CacheInitialValue()
	{
		base.CacheInitialValue();
		transform_.localPosition = (Vector3)initialValue_ + initialVector_ * (float)targetValue_;
	}

	protected override void UpdateAnimation()
	{
		timer_ += UnityEngine.Time.deltaTime;
		float shakeValue = Mathf.Sqrt(1.0f - normalizedValue_);
		if( timer_ >= updateTime_ || shakeValue <= 0.0f )
		{
			switch( Param )
			{
			case AnimParamType.Position:
				transform_.localPosition = (Vector3)initialValue_ + Random.insideUnitSphere * (float)targetValue_ * shakeValue;
				break;
			case AnimParamType.PositionX:
				transform_.localPosition = new Vector3((float)initialValue_ + Random.Range(-(float)targetValue_, (float)targetValue_) * shakeValue, transform_.localPosition.y, transform_.localPosition.z);
				break;
			case AnimParamType.PositionY:
				transform_.localPosition = new Vector3(transform_.localPosition.x, (float)initialValue_ + Random.Range(-(float)targetValue_, (float)targetValue_) * shakeValue, transform_.localPosition.z);
				break;
			case AnimParamType.PositionZ:
				transform_.localPosition = new Vector3(transform_.localPosition.x, transform_.localPosition.y, (float)initialValue_ + Random.Range(-(float)targetValue_, (float)targetValue_) * shakeValue);
				break;
			}
			timer_ %= updateTime_;
		}
	}
}

public class ShaderAnimation : AnimationBase
{
	protected Material material_;
	protected int propID_ = -1;

	public ShaderAnimation(GameObject obj, object target, AnimParamType paramType, InterpType interpType, TimeUnitType timeUnit, float time, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
		: base(obj, target, paramType, interpType, timeUnit, time, delay, endOption)
	{
		if( paramType < AnimParamType.ShaderFloat || AnimParamType.ShaderColor < paramType )
		{
			throw new System.Exception("ShaderAnimation: wrong param type! paramType = " + paramType.ToString());
		}

		material_ = obj.GetComponent<Renderer>().material;
	}

	public void SetPropertyName(string propName)
	{
		propID_ = Shader.PropertyToID(propName);
	}

	protected override void CacheInitialValue()
	{
		if( propID_ < 0 )
		{
			throw new System.Exception("ShaderAnimation: wrong propName! propID_ = " + propID_.ToString());
		}
		switch(Param)
		{
			case AnimParamType.ShaderFloat:
			{
				initialValue_ = material_.GetFloat(propID_);
			}
			break;
			case AnimParamType.ShaderVector2:
			{
				initialValue_ = (Vector2)material_.GetVector(propID_);
			}
			break;
			case AnimParamType.ShaderVector3:
			{
				initialValue_ = (Vector3)material_.GetVector(propID_);
			}
			break;
			case AnimParamType.ShaderColor:
			{
				initialValue_ = material_.GetColor(propID_);
			}
			break;
		}
	}

	protected override void UpdateAnimation()
	{
		if( propID_ < 0 )
		{
			return;
		}
		switch( Param )
		{
			case AnimParamType.ShaderFloat:
			{
				material_.SetFloat(propID_, currentValueFloat);
			}
			break;
			case AnimParamType.ShaderVector2:
			{
				material_.SetVector(propID_, currentValueVector2);
			}
			break;
			case AnimParamType.ShaderVector3:
			{
				material_.SetVector(propID_, currentValueVector3);
			}
			break;
			case AnimParamType.ShaderColor:
			{
				material_.SetColor(propID_, currentValueColor);
			}
			break;
		}
	}
}

public class AnimManager : MonoBehaviour
{
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
		if( Instance.removeAnims_.Contains(anim) == false )
		{
			anim.End();
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
