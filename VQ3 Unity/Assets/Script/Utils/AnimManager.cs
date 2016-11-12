using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IColoredObject
{
	void SetColor(Color color);
	Color GetColor();
}

public enum ParamType
{
	//Transform
	Scale,
	ScaleX,
	ScaleY,
	ScaleZ,
	RotationZ,
	Position,
	PositionX,
	PositionY,
	PositionZ,

	//MidairPrimitive
	PrimitiveRadius,
	PrimitiveWidth,
	PrimitiveArc,

	//Gauge
	GaugeLength,
	GaugeRate,
	GaugeWidth,

	//IColoredObject
	Color,

	TextColor,

	// for RemoveAnim
	Any,
}

public enum AnimType
{
	Linear,
	Time,
	BounceIn,
	BounceOut
}

public class AnimInfo
{
	public GameObject Object;
	public ParamType Param;
	public AnimType Anim;
	public object Target;
	public float Factor;
	public float Delay;
	public bool DestroyAtEnd;

	object initialValue_;
	float normalizedValue_;
	float animValue_;

	Transform transform_;
	MidairPrimitive primitive_;
	GaugeRenderer gauge_;
	IColoredObject coloredObj_;
	TextMesh text_;

	public bool IsEnd { get { return normalizedValue_ >= 1.0f; } private set { normalizedValue_ = 1.0f; } }
	private float currentValueFloat { get { return (float)initialValue_ + ((float)Target - (float)initialValue_) * animValue_; } }
	private Vector3 currentValueVector3 { get { return (Vector3)initialValue_ + ((Vector3)Target - (Vector3)initialValue_) * animValue_; } }
	private static float overshoot = 1.70158f;



	public AnimInfo(GameObject obj, object target, ParamType paramType, AnimType animType, float factor = 0.1f, float delay = 0.0f, bool destroyAtEnd = false)
	{
		Object = obj;
		Param = paramType;
		Anim = animType;
		Target = target;
		Factor = factor;
		Delay = delay;
		DestroyAtEnd = destroyAtEnd;

		normalizedValue_ = 0;
		animValue_ = 0;

		if( Delay <= 0 )
		{
			InitValue();
		}
	}

	public void Update()
	{
		if( Object == null )
		{
			IsEnd = true;
			return;
		}

		if( Delay > 0 )
		{
			Delay -= Time.deltaTime;
			if( Delay <= 0 )
			{
				InitValue();
			}
			else
			{
				return;
			}
		}

		if( Anim == AnimType.Linear )
		{
			normalizedValue_ = Mathf.Lerp(normalizedValue_, 1.0f, Factor);
			if( Mathf.Abs(normalizedValue_ - 1.0f) < 0.01f )
			{
				normalizedValue_ = 1.0f;
			}
			animValue_ = normalizedValue_;
		}
		else
		{
			normalizedValue_ += Time.deltaTime / Factor;
			normalizedValue_ = Mathf.Clamp01(normalizedValue_);
			switch( Anim )
			{
			case AnimType.Time:
				animValue_ = normalizedValue_;
				break;
			case AnimType.BounceIn:
				{
					float r = normalizedValue_ - 1;
					animValue_ = r * r * ((overshoot + 1) * r + overshoot) + 1;
				}
				break;
			case AnimType.BounceOut:
				{
					float r = 1.0f - normalizedValue_ - 1;
					animValue_ = 1.0f - (r * r * ((overshoot + 1) * r + overshoot) + 1);
				}
				break;
			}
		}
		switch( Param )
		{
		case ParamType.Scale:
			transform_.localScale = currentValueVector3;
			break;
		case ParamType.ScaleX:
			transform_.localScale = new Vector3(currentValueFloat, transform_.localScale.y, transform_.localScale.z);
			break;
		case ParamType.ScaleY:
			transform_.localScale = new Vector3(transform_.localScale.x, currentValueFloat, transform_.localScale.z);
			break;
		case ParamType.ScaleZ:
			transform_.localScale = new Vector3(transform_.localScale.x, transform_.localScale.y, currentValueFloat);
			break;
		case ParamType.Position:
			transform_.localPosition = currentValueVector3;
			break;
		case ParamType.PositionX:
			transform_.localPosition = new Vector3(currentValueFloat, transform_.localPosition.y, transform_.localPosition.z);
			break;
		case ParamType.PositionY:
			transform_.localPosition = new Vector3(transform_.localPosition.x, currentValueFloat, transform_.localPosition.z);
			break;
		case ParamType.PositionZ:
			transform_.localPosition = new Vector3(transform_.localPosition.x, transform_.localPosition.y, currentValueFloat);
			break;
		case ParamType.RotationZ:
			transform_.localRotation = Quaternion.AngleAxis(currentValueFloat, Vector3.forward);
			break;
		case ParamType.PrimitiveRadius:
			primitive_.SetSize(currentValueFloat);
			break;
		case ParamType.PrimitiveWidth:
			primitive_.SetWidth(currentValueFloat);
			break;
		case ParamType.PrimitiveArc:
			primitive_.SetArc(currentValueFloat);
			break;
		case ParamType.GaugeLength:
			gauge_.Length = currentValueFloat;
			break;
		case ParamType.GaugeRate:
			gauge_.SetRate(currentValueFloat);
			break;
		case ParamType.GaugeWidth:
			gauge_.SetWidth(currentValueFloat);
			break;
		case ParamType.Color:
			coloredObj_.SetColor(Color.Lerp((Color)initialValue_, (Color)Target, animValue_));
			break;
		case ParamType.TextColor:
			text_.color = Color.Lerp((Color)initialValue_, (Color)Target, animValue_);
			break;
		}

		if( IsEnd && DestroyAtEnd )
		{
			GameObject.Destroy(Object);
			return;
		}
	}

	void InitValue()
	{
		switch( Param )
		{
		case ParamType.Scale:
			transform_ = Object.transform;
			initialValue_ = transform_.localScale;
			break;
		case ParamType.ScaleX:
			transform_ = Object.transform;
			initialValue_ = (float)transform_.localScale.x;
			break;
		case ParamType.ScaleY:
			transform_ = Object.transform;
			initialValue_ = (float)transform_.localScale.y;
			break;
		case ParamType.ScaleZ:
			transform_ = Object.transform;
			initialValue_ = (float)transform_.localScale.z;
			break;
		case ParamType.Position:
			transform_ = Object.transform;
			initialValue_ = transform_.localPosition;
			break;
		case ParamType.PositionX:
			transform_ = Object.transform;
			initialValue_ = (float)transform_.localPosition.x;
			break;
		case ParamType.PositionY:
			transform_ = Object.transform;
			initialValue_ = (float)transform_.localPosition.y;
			break;
		case ParamType.PositionZ:
			transform_ = Object.transform;
			initialValue_ = (float)transform_.localPosition.z;
			break;
		case ParamType.RotationZ:
			transform_ = Object.transform;
			initialValue_ = (float)(transform_.rotation.eulerAngles.z + 360) % 360;
			break;
		case ParamType.PrimitiveRadius:
			primitive_ = Object.GetComponent<MidairPrimitive>();
			initialValue_ = (float)primitive_.Radius;
			break;
		case ParamType.PrimitiveWidth:
			primitive_ = Object.GetComponent<MidairPrimitive>();
			initialValue_ = (float)primitive_.Width;
			break;
		case ParamType.PrimitiveArc:
			primitive_ = Object.GetComponent<MidairPrimitive>();
			initialValue_ = (float)primitive_.ArcRate;
			break;
		case ParamType.GaugeLength:
			gauge_ = Object.GetComponent<GaugeRenderer>();
			initialValue_ = (float)gauge_.Length;
			break;
		case ParamType.GaugeRate:
			gauge_ = Object.GetComponent<GaugeRenderer>();
			initialValue_ = (float)gauge_.Rate;
			break;
		case ParamType.GaugeWidth:
			gauge_ = Object.GetComponent<GaugeRenderer>();
			initialValue_ = (float)gauge_.Width;
			break;
		case ParamType.Color:
			coloredObj_ = Object.GetComponent<IColoredObject>();
			initialValue_ = coloredObj_.GetColor();
			break;
		case ParamType.TextColor:
			text_ = Object.GetComponent<TextMesh>();
			initialValue_ = text_.color;
			break;
		}

		if( initialValue_ is float && (float)initialValue_ == (float)Target )
		{
			IsEnd = true;
		}
		else if( initialValue_ is Color && Color.Equals(initialValue_, Target) )
		{
			IsEnd = true;
		}
		else if( initialValue_ is Vector3 && Vector3.Equals(initialValue_, Target) )
		{
			IsEnd = true;
		}
	}
}

public class AnimManager : MonoBehaviour
{

	static AnimManager instance
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

	public List<AnimInfo> Animations = new List<AnimInfo>();

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		foreach( AnimInfo anim in Animations )
		{
			anim.Update();
		}
		Animations.RemoveAll((AnimInfo anim) => anim.IsEnd);
	}

	public static void AddAnim(GameObject obj, object target, ParamType paramType, AnimType animType = AnimType.Linear, float factor = 0.1f, float delay = 0.0f, bool destroyAtEnd = false)
	{
		if( instance.Animations.Find((AnimInfo anim) => anim.Object == obj && anim.Param == paramType && anim.Target == target) != null ) return;
		//delayを考慮してやらないといけないので一旦諦め
		//instance.Animations.RemoveAll((AnimInfo anim) => anim.Object == obj && anim.Param == paramType);
		instance.Animations.Add(new AnimInfo(obj, target, paramType, animType, factor, delay, destroyAtEnd));
	}

	public static bool IsAnimating(GameObject obj)
	{
		return instance.Animations.Find((AnimInfo anim) => anim.Object == obj) != null;
	}

	public static void RemoveAnim(GameObject obj, ParamType paramType = ParamType.Any)
	{
		instance.Animations.RemoveAll((AnimInfo anim) => anim.Object == obj && (paramType == ParamType.Any || anim.Param == paramType));
	}
}
