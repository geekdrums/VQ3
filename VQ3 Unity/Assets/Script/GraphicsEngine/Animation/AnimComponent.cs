using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Serialization;

public class AnimComponent : MonoBehaviour
{
	public bool PlayOnStart = false;
	public bool ResetOnEnd = false;
	public bool IsAnimListFolded = false;
	public float Delay;
	public float Speed = 1.0f;
	public TimeUnitType DelayTimeUnit;

	public List<AnimInfo> AnimInfoList = new List<AnimInfo>();

	public List<AnimComponent> ChildAnimList = new List<AnimComponent>();

	public enum AnimState
	{
		Invalid,
		Ready,
		Playing,
		End,
	}
	public AnimState State { get; protected set; } = AnimState.Invalid;

	public float TotalTimeSec
	{
		get
		{
			float max = 0;
			foreach( AnimInfo ai in AnimInfoList )
			{
				float time = TimeUtility.ConvertTime(ai.Delay + ai.Time, ai.TimeUnit);
				max = Mathf.Max(max, time);
			}
			return Mathf.Max(1.0f, max);
		}
	}


	protected List<AnimationBase> animations_ = new List<AnimationBase>();

	// Start is called before the first frame update
	void Start()
	{
		if( PlayOnStart )
		{
			Play();
		}
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void Create()
	{
		if( State == AnimState.Playing || State == AnimState.End )
		{
			ResetAnim();
		}
		animations_.Clear();
		foreach( AnimInfo ai in AnimInfoList )
		{
			if( ai.Object == null )
			{
				print(string.Format("AnimationComponent {0} Object is null!", this.name));
				continue;
			}

			float speedFactor = Speed > 0.0f ? (1.0f / Speed) : 1.0f;
			AnimationBase animation = AnimManager.CreateAnim(ai.Object, ai.GetTarget(), ai.AnimParam, ai.Interp, ai.TimeUnit, ai.Time * speedFactor, ai.Delay * speedFactor, ai.EndOption);

			animation.SetOnEndEvent(OnEndAnim);

			if( ai.HasInitialValue && ai.GetInitial() != null )
			{
				animation.From(ai.GetInitial());
			}
			if( animation is TextAnimaion )
			{
				animation.From("");
			}
			if( animation is ShaderAnimation )
			{
				(animation as ShaderAnimation).SetPropertyName(ai.PropertyName);
			}

			animations_.Add(animation);
		}

		foreach( AnimComponent child in ChildAnimList )
		{
			child.Create();
		}

		State = AnimState.Ready;
	}

	public void Play(float in_delay = 0.0f, float in_speed = 1.0f)
	{
		if( State != AnimState.Ready || State == AnimState.End )
		{
			ResetAnim();
		}

		float totalDelaySec = in_delay + TimeUtility.ConvertTime(Delay, DelayTimeUnit);
		
		foreach( AnimationBase anim in animations_ )
		{
			AnimManager.AddAnim(anim).AddDelay(totalDelaySec).SetSpeed(in_speed);
		}

		foreach( AnimComponent child in ChildAnimList )
		{
			child.Play(totalDelaySec, Speed * in_speed);
		}

		State = AnimState.Playing;
	}

	/// <summary>
	/// アニメーションを途中で止める
	/// </summary>
	public void Stop()
	{
		for( int i = animations_.Count - 1; i >= 0; --i )
		{
			animations_[i].Stop();
		}
	}

	/// <summary>
	/// アニメーションを再生開始時の状態へと戻す。
	/// </summary>
	public void ResetAnim()
	{
		if( State == AnimState.Invalid )
		{
			Create();
			return;
		}

		for( int i = animations_.Count - 1; i >= 0; --i )
		{
			if( animations_[i].State < AnimationBase.AnimationState.Playing )
			{
				animations_[i].Stop();
			}
			else
			{
				animations_[i].Reset();
			}
		}

		foreach( AnimComponent child in ChildAnimList )
		{
			child.ResetAnim();
		}

		State = AnimState.Ready;
	}

	/// <summary>
	/// アニメーションを途中で、または再生前から終了時点の状態へと飛ばす。
	/// </summary>
	public void EndAnim()
	{
		if( State == AnimState.Invalid )
		{
			Create();
		}
		
		// すべて終わりまで飛ばす
		foreach( AnimationBase anim in animations_ )
		{
			anim.End();
		}

		foreach( AnimComponent child in ChildAnimList )
		{
			child.EndAnim();
		}

		State = AnimState.End;
	}

	void OnEndAnim(AnimationBase anim)
	{
		if( animations_.Find((AnimationBase a) => a.State < AnimationBase.AnimationState.End) == null )
		{
			if( ResetOnEnd )
			{
				ResetAnim();
			}
			State = AnimState.End;
		}
	}
}
