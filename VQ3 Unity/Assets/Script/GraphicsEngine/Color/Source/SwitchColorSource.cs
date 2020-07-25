using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SwitchColorSource : ColorSourceBase
{
	[SerializeField]
	protected List<ColorSourceBase> SourceList = new List<ColorSourceBase>();

	[SerializeField]
	protected string StateGroupName = "";
	[SerializeField]
	protected string StateName = "";

	ColorTransition transition_ = null;
	ColorGameSyncByState stateGroup_ = null;

	public override Color SourceColor
	{
		get
		{
			if( transition_ != null && playingTransitions_.Contains(transition_) )
			{
				return transition_.GetCurrentColor();
			}

#if UNITY_EDITOR
			if( UnityEditor.EditorApplication.isPlaying == false || stateGroup_ == null )
			{
				if( ColorManager.ContainsStateGroup(StateGroupName) )
				{
					stateGroup_ = ColorManager.GetStateGroup(StateGroupName);
				}
				else
				{
					stateGroup_ = null;
				}
			}
#endif

			if( stateGroup_ != null )
			{
				return GetSourceColor(stateGroup_.IsGlobal ? stateGroup_.GlobalValue : StateName);
			}
			else
			{
				return Color.clear;
			}
		}
	}


	public Color GetSourceColor(string stateName)
	{
		int index = 0;
		if( stateGroup_ != null )
		{
			index = Mathf.Max(0, stateGroup_.Process(stateName));
		}
		if( 0 <= index && index < SourceList.Count )
		{
			return SourceList[index] != null ? SourceList[index] : Color.clear;
		}
		else
		{
			return Color.clear;
		}
	}

	public void OnGlobalStateTransitionStart(string stateGroupName, string stateName, ColorTransitionBase globalTransition)
	{
		if( stateGroupName != this.StateGroupName )
		{
			return;
		}

		if( transition_ != null )
		{
			transition_.SetTarget(GetSourceColor(stateName), globalTransition);
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
		stateGroup_ = null;
		if( ColorManager.ContainsStateGroup(StateGroupName) )
		{
			stateGroup_ = ColorManager.GetStateGroup(StateGroupName);
		}

		base.Awake();
	}

	// reference
	public override IEnumerable<ColorSourceBase> GetReferenceColors()
	{
		foreach( ColorSourceBase source in SourceList )
		{
			if( source == null )
			{
				continue;
			}
			yield return source;
			foreach( ColorSourceBase sourceRefColors in source.GetReferenceColors() )
			{
				yield return sourceRefColors;
			}
		}
	}
	protected override void NotifyMyReferences()
	{
		base.NotifyMyReferences();

		if( ColorManager.ContainsStateGroup(StateGroupName) )
		{
			ColorManager.GetStateGroup(StateGroupName).BeReferencedBy(this);
		}
	}

	// state
	public override void SetState(string stateGroupName, string stateName)
	{
		base.SetState(stateGroupName, stateName);

		if( this.StateGroupName == stateGroupName )
		{
			this.StateName = stateName;
			if( transition_ != null )
			{
				transition_.SetTarget(GetSourceColor(stateName));
				if( playingTransitions_.Contains(transition_) == false )
				{
					playingTransitions_.Add(transition_);
				}
			}

			SetReferenceColorDirty();
		}
	}
	public override bool HasState(string stateGroupName)
	{
		return StateGroupName == stateGroupName || base.HasState(stateGroupName);
	}

	// transition
	protected override void InitTransitionDict()
	{
		base.InitTransitionDict();
		
		if( ColorManager.ContainsStateGroup(StateGroupName) && ColorManager.GetStateGroup(StateGroupName).NeedTransition )
		{
			transition_ = new ColorTransition();
			transition_.Init(StateGroupName, SourceColor, ColorManager.GetStateGroup(StateGroupName).TransitionTime);
		}
	}

	#endregion

	public override void ApplySourceInstance()
	{
		if( SourceInstance is SwitchColorSource == false )
		{
			return;
		}

		base.ApplySourceInstance();

		this.SourceList = new List<ColorSourceBase>((SourceInstance as SwitchColorSource).SourceList);
		this.StateGroupName = (SourceInstance as SwitchColorSource).StateGroupName;
	}
}
