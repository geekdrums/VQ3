using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// ColorSourceのTransitionに用いる既定クラス。
/// Updateだけを行う時のためにジェネリクスを使わずに共通化できる処理のみを抽出。
/// </summary>
public abstract class ColorTransitionBase
{
	public abstract string Name { get; }
	protected float time_;
	protected float transitionTime_;

	public virtual bool RemainingTime()
	{
		return time_ > 0.0f;
	}

	public void Update()
	{
		time_ -= Time.deltaTime;
		if( time_ < 0 ) time_ = 0.0f;
	}

	public void SetTransitionTime(float time)
	{
		time_ = transitionTime_ = time;
	}

	public float GetRate()
	{
		return 1.0f - (time_ / transitionTime_);
	}
}

public class ColorTransition : ColorTransitionBase
{
	string name_;
	Color baseColor_;
	Color targetColor_;

	// GlobalStateによるSwitch/BlendColorのTransitionの場合、
	// 遷移のTimeやRateはGlobalを参照するけどBase/TargetColorはローカルで違うので
	// ローカルで保持したBase/TargetColorを使いつつGlobalTransitionに従って遷移させる。
	ColorTransitionBase globalTransition_;

	public override string Name { get { return name_; } }

	public void Init(string name, Color initColor, float transitionTime)
	{
		name_ = name;
		baseColor_ = targetColor_ = initColor;
		transitionTime_ = transitionTime;
	}

	public void SetTarget(Color target, ColorTransitionBase globalTransition = null)
	{
		baseColor_ = GetCurrentColor();
		targetColor_ = target;
		SetTransitionTime(transitionTime_);
		globalTransition_ = globalTransition;
	}

	public Color GetCurrentColor()
	{
		if( globalTransition_ != null )
		{
			return Color.Lerp(baseColor_, targetColor_, globalTransition_.GetRate());
		}
		else
		{
			return Color.Lerp(baseColor_, targetColor_, GetRate());
		}
	}

	public override bool RemainingTime()
	{
		return globalTransition_ != null ? globalTransition_.RemainingTime() : base.RemainingTime();
	}
}

/// <summary>
/// StateとParameterの両方に対応するためのTransitionクラス。
/// </summary>
/// <typeparam name="InputType">ゲームからの入力となる値。Stateの場合はstring、Parameterの場合はfloat</typeparam>
/// <typeparam name="OutputType">PropertyEditTypeへの出力となる値。Stateの場合配列のインデックス(int)、Parameterの場合正規化された値(float)</typeparam>
/// <typeparam name="PropertyEditType">OutputTypeを受け取って実際の色の変化量に変換するクラス</typeparam>
public class ColorPropertyTransition<InputType, OutputType, PropertyEditType> : ColorTransitionBase
	where PropertyEditType : ColorPropertyEditBase<OutputType>
{
	float[] baseValues_;
	float[] targetValues_;
	ColorGameSync<InputType, OutputType, PropertyEditType> gameSync_;
	public override string Name
	{
		get { return gameSync_.Name; }
	}

	public void Init(ColorGameSync<InputType, OutputType, PropertyEditType> gameSync, InputType initialValue)
	{
		gameSync_ = gameSync;

		baseValues_ = new float[gameSync_.Count];
		targetValues_ = new float[gameSync_.Count];
		for( int i = 0; i < gameSync_.Count; ++i )
		{
			baseValues_[i] = targetValues_[i] = gameSync_.GetEditValue(i, initialValue);
		}
		time_ = 0.0f;
	}

	public void Apply(ref float h, ref float s, ref float v, ref float a)
	{
		for( int i = 0; i < gameSync_.Count; ++i )
		{
			gameSync_[i].Apply(GetCurrentValue(i), ref h, ref s, ref v, ref a);
		}
	}

	public void SetTarget(InputType input)
	{
		for( int i = 0; i < gameSync_.Count; ++i )
		{
			baseValues_[i] = GetCurrentValue(i);
			targetValues_[i] = gameSync_.GetEditValue(i, input);
		}
		SetTransitionTime(gameSync_.TransitionTime);
	}

	public float GetCurrentValue(int index)
	{
		return Mathf.Lerp(baseValues_[index], targetValues_[index], GetRate());
	}
}

public class ColorStateTransition : ColorPropertyTransition<string, int, ColorPropertyEditByIndex> { }

public class ColorParameterTransition : ColorPropertyTransition<float, float, ColorPropertyEditByRate> { }
