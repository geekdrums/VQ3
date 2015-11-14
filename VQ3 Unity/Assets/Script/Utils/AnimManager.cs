using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IColoredObject
{
	void SetColor( Color color );
	Color GetColor();
}

public enum ParamType
{
	//Transform
	Scale,
	RotationZ,

	//MidairPrimitive
	Radius,
	Width,
	Arc,

	//Gauge
	GaugeRate,
	GaugeWidth,

	Color,
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

	object initialValue_;
	float normalizedValue_;
	float animValue_;

	Transform transform_;
	MidairPrimitive primitive_;
	GaugeRenderer gauge_;
	IColoredObject coloredObj_;

	public bool IsEnd { get { return normalizedValue_ >= 1.0f; } private set { normalizedValue_ = 1.0f; } }
	private float currentValue { get { return (float)initialValue_ + ((float)Target - (float)initialValue_) * animValue_; } }
	private static float overshoot = 1.70158f;


	public AnimInfo(GameObject obj, object target, ParamType paramType, AnimType animType, float factor = 0.1f)
	{
		Object = obj;
		Param = paramType;
		Anim = animType;
		Target = target;
		Factor = factor;

		normalizedValue_ = 0;
		animValue_ = 0;
		switch( Param )
		{
		case ParamType.Scale:
			transform_ = Object.transform;
			initialValue_ = (float)transform_.localScale.x;
			break;
		case ParamType.RotationZ:
			transform_ = Object.transform;
			initialValue_ = (float)(transform_.rotation.eulerAngles.z + 360)%360;
			break;
		case ParamType.Radius:
			primitive_ = Object.GetComponent<MidairPrimitive>();
			initialValue_ = (float)primitive_.Radius;
			break;
		case ParamType.Width:
			primitive_ = Object.GetComponent<MidairPrimitive>();
			initialValue_ = (float)primitive_.Width;
			break;
		case ParamType.Arc:
			primitive_ = Object.GetComponent<MidairPrimitive>();
			initialValue_ = (float)primitive_.ArcRate;
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
		}

		if( initialValue_ is float && (float)initialValue_ == (float)target )
		{
			IsEnd = true;
		}
		else if( initialValue_ is Color && Color.Equals(initialValue_, target) )
		{
			IsEnd = true;
		}
	}

	public void Update()
	{
		if( Object == null )
		{
			IsEnd = true;
			return;
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
			transform_.localScale = Vector3.one *  currentValue;
			break;
		case ParamType.RotationZ:
			transform_.localRotation = Quaternion.AngleAxis(currentValue, Vector3.forward);
			break;
		case ParamType.Radius:
			primitive_.SetSize(currentValue);
			break;
		case ParamType.Width:
			primitive_.SetWidth(currentValue);
			break;
		case ParamType.Arc:
			primitive_.SetArc(currentValue);
			break;
		case ParamType.GaugeRate:
			gauge_.SetRate(currentValue);
			break;
		case ParamType.GaugeWidth:
			gauge_.SetWidth(currentValue);
			break;
		case ParamType.Color:
			coloredObj_.SetColor(Color.Lerp((Color)initialValue_, (Color)Target, animValue_));
			break;
		}
	}
}

public class AnimManager : MonoBehaviour {

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
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		foreach( AnimInfo anim in Animations )
		{
			anim.Update();
		}
		Animations.RemoveAll((AnimInfo anim) => anim.IsEnd);
	}

	public static void AddAnim(GameObject obj, object target, ParamType paramType, AnimType animType = AnimType.Linear, float factor = 0.1f)
	{
		instance.Animations.RemoveAll((AnimInfo anim) => anim.Object == obj && anim.Param == paramType);
		instance.Animations.Add(new AnimInfo(obj, target, paramType, animType, factor));
	}

	public static bool IsAnimating(GameObject obj)
	{
		return instance.Animations.Find((AnimInfo anim) => anim.Object == obj) != null;
	}

	public static void RemoveAnim(GameObject obj, ParamType paramType)
	{
		instance.Animations.RemoveAll((AnimInfo anim) => anim.Object == obj && anim.Param == paramType);
	}
}
