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

			AnimationBase animation = AnimManager.CreateAnim(ai.Object, ai.GetTarget(), ai.AnimParam, ai.Interp, ai.TimeUnit, ai.Time, ai.Delay, ai.EndOption);

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

	public void Play(float delay = 0.0f)
	{
		if( gameObject.activeSelf == false )
		{
			gameObject.SetActive(true);
		}

		if( State == AnimState.Playing || State == AnimState.End )
		{
			ResetAnim();
		}
		else if( State == AnimState.Invalid )
		{
			Create();
		}

		float totalDelaySec = delay + TimeUtility.ConvertTime(Delay, DelayTimeUnit);

		foreach( AnimationBase anim in animations_ )
		{
			AnimManager.AddAnim(anim).AddDelay(totalDelaySec);
		}

		foreach( AnimComponent child in ChildAnimList )
		{
			child.Play(totalDelaySec);
		}

		State = AnimState.Playing;
	}

	/// <summary>
	/// アニメーションを再生開始時の状態へと戻す。
	/// </summary>
	public void ResetAnim()
	{
		if( State == AnimState.Invalid )
		{
			Create();
		}

		for( int i = animations_.Count - 1; i >= 0; --i )
		{
			if( animations_[i].State == AnimationBase.AnimationState.Ready )
			{
				animations_[i].Stop();
			}
			else
			{
				animations_[i].Reset();
				animations_[i].ClearInitialValue();
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
		State = AnimState.End;
		if( ResetOnEnd )
		{
			if( animations_.Find((AnimationBase a) => a.State < AnimationBase.AnimationState.End) == null )
			{
				ResetAnim();
			}
		}
	}
}
