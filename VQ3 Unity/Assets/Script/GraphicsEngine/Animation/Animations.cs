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
	public AnimationState State
	{
		get { return state_; }
		protected set
		{
			if( state_ != value )
			{
				oldState_ = state_;
				state_ = value;
				OnStateChanged();
			}
		}
	}
	public AnimationState OldState { get { return oldState_; } }
	protected AnimationState state_ = AnimationState.Ready;
	protected AnimationState oldState_ = AnimationState.Ready;

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

		if( (initialValue_ is float && targetValue_ is float && (float)initialValue_ == (float)targetValue_)
		 || (initialValue_ is Color && Color.Equals(initialValue_, targetValue_))
		 || (initialValue_ is Vector3 && Vector3.Equals(initialValue_, targetValue_)) )
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

	protected virtual void OnStateChanged() { }

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

	public AnimationBase SetSpeed(float speed)
	{
		float speedFactor = speed > 0.0f ? (1.0f / speed) : 1.0f;
		remainDelayTime_ *= speedFactor;
		AnimTimeSec *= speedFactor;
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
	}

	protected override void OnStateChanged()
	{
		if( State == AnimationState.Playing )
		{
			Object.SetActive((bool)targetValue_);
		}
		else if( State == AnimationState.Ready )
		{
			Object.SetActive(!(bool)targetValue_);
		}
	}
}

public class BlinkAnimaion : AnimationBase
{
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
		if( State == AnimationState.Playing )
		{
			++frameCnt_;
			if( interval_ <= frameCnt_ )
			{
				frameCnt_ = 0;
				Object.SetActive(!Object.activeSelf);
			}
		}
		else
		{
			frameCnt_ = 0;
		}
	}

	protected override void OnStateChanged()
	{
		if( OldState == AnimationState.Playing )
		{
			Object.SetActive((bool)initialValue_);
		}
	}
}

public class FlashAnimaion : AnimationBase
{
	public FlashAnimaion(GameObject obj, TimeUnitType timeUnit, float time, float delay = 0.0f, AnimEndOption endOption = AnimEndOption.None)
		: base(obj, target: true, AnimParamType.Flash, InterpType.Linear, timeUnit, time, delay, endOption)
	{
	}

	protected override void CacheInitialValue()
	{
	}

	protected override void UpdateAnimation()
	{
	}

	protected override void OnStateChanged()
	{
		if( State == AnimationState.Playing )
		{
			Object.SetActive(true);
		}
		else if( OldState == AnimationState.Playing )
		{
			Object.SetActive(false);
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
		switch( Param )
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
