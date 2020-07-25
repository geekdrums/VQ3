using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BlendColorSource : ColorSourceBase
{
	[SerializeField]
	protected ColorSourceBase SourceBegin;

	[SerializeField]
	protected ColorSourceBase SourceEnd;

	[SerializeField]
	protected string ParameterName = "";
	[SerializeField]
	protected float ParameterValue = 0;

	ColorTransition transition_ = null;
	ColorGameSyncByParameter parameter_ = null;

	public override Color SourceColor
	{
		get
		{
			if( transition_ != null && playingTransitions_.Contains(transition_) )
			{
				return transition_.GetCurrentColor();
			}

#if UNITY_EDITOR
			if( UnityEditor.EditorApplication.isPlaying == false || parameter_ == null )
			{
				if( ColorManager.ContainsParameter(ParameterName) )
				{
					parameter_ = ColorManager.GetParameter(ParameterName);
				}
				else
				{
					parameter_ = null;
				}
			}
#endif
			if( parameter_ != null )
			{
				return GetSourceColor(parameter_.IsGlobal ? parameter_.GlobalValue : ParameterValue);
			}
			else
			{
				return Color.clear;
			}
		}
	}


	public Color GetSourceColor(float value)
	{
		float rate = 0.0f;
		if( parameter_ != null )
		{
			rate = parameter_.Process(value);
		}
		if( SourceBegin != null && SourceEnd != null )
		{
			return Color.Lerp(SourceBegin, SourceEnd, rate);
		}
		else
		{
			return Color.clear;
		}
	}

	public void OnGlobalParameterTransitionStart(string parameterName, float value, ColorTransitionBase globalTransition)
	{
		if( parameterName != this.ParameterName )
		{
			return;
		}

		if( transition_ != null )
		{
			transition_.SetTarget(GetSourceColor(value), globalTransition);
			if( playingTransitions_.Contains(transition_) == false )
			{
				playingTransitions_.Add(transition_);
			}
		}
	}

	
	#region override functions

	// unity function
	protected override void Awake()
	{
		parameter_ = null;
		if( ColorManager.ContainsParameter(ParameterName) )
		{
			parameter_ = ColorManager.GetParameter(ParameterName);
		}

		base.Awake();
	}

	// reference
	public override IEnumerable<ColorSourceBase> GetReferenceColors()
	{
		if( SourceBegin != null )
		{
			yield return SourceBegin;
			foreach( ColorSourceBase sourceRefColors in SourceBegin.GetReferenceColors() )
			{
				yield return sourceRefColors;
			}
		}

		if( SourceEnd != null )
		{
			yield return SourceEnd;
			foreach( ColorSourceBase sourceRefColors in SourceEnd.GetReferenceColors() )
			{
				yield return sourceRefColors;
			}
		}
	}
	protected override void NotifyMyReferences()
	{
		base.NotifyMyReferences();

		if( ColorManager.ContainsParameter(ParameterName) )
		{
			ColorManager.GetParameter(ParameterName).BeReferencedBy(this);
		}
	}

	// state
	public override void SetParameter(string parameterName, float value)
	{
		base.SetParameter(parameterName, value);

		if( this.ParameterName == parameterName )
		{
			this.ParameterValue = value;
			if( transition_ != null )
			{
				transition_.SetTarget(GetSourceColor(value));
				if( playingTransitions_.Contains(transition_) == false )
				{
					playingTransitions_.Add(transition_);
				}
			}

			SetReferenceColorDirty();
		}
	}
	public override bool HasParameter(string parameterName)
	{
		return ParameterName == parameterName || base.HasParameter(parameterName);
	}

	// transition
	protected override void InitTransitionDict()
	{
		base.InitTransitionDict();
		
		if( ColorManager.ContainsParameter(ParameterName) && ColorManager.GetParameter(ParameterName).NeedTransition )
		{
			transition_ = new ColorTransition();
			transition_.Init(ParameterName, SourceColor, ColorManager.GetParameter(ParameterName).TransitionTime);
		}
	}

	#endregion

	public override void ApplySourceInstance()
	{
		if( SourceInstance is BlendColorSource == false )
		{
			return;
		}

		base.ApplySourceInstance();

		this.SourceBegin = (SourceInstance as BlendColorSource).SourceBegin;
		this.SourceEnd = (SourceInstance as BlendColorSource).SourceEnd;
		this.ParameterName = (SourceInstance as BlendColorSource).ParameterName;
	}
}
