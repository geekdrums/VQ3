using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ColorManager : MonoBehaviour
{
	// singleton
	public static ColorManager Instance
	{
		get
		{
			if( instance_ == null )
			{
				instance_ = UnityEngine.Object.FindObjectOfType<ColorManager>();
			}
			return instance_;
		}
	}
	static ColorManager instance_;

	// editor params
	[SerializeField]
	SerializableColorStateGroupDictionary StateGroups = new SerializableColorStateGroupDictionary();
	[SerializeField]
	SerializableColorParameterDictionary Parameters = new SerializableColorParameterDictionary();

	// transitions
	Dictionary<string, ColorStateTransition> stateTransitionDict_ = new Dictionary<string, ColorStateTransition>();
	Dictionary<string, ColorParameterTransition> parameterTransitionDict_ = new Dictionary<string, ColorParameterTransition>();
	List<ColorTransitionBase> playingTransitions_ = new List<ColorTransitionBase>();


	#region unity functions

	void Awake()
	{
		instance_ = this;
		InitTransitionDict();
	}

	void OnValidate()
	{
		StateGroups.OnValidate();
		Parameters.OnValidate();
		foreach( ColorGameSyncByState stateGroup in StateGroups.Values )
		{
			stateGroup.RemoveEmptyRefereces();
		}
		foreach( ColorGameSyncByParameter parameter in Parameters.Values )
		{
			parameter.RemoveEmptyRefereces();
		}
	}

	void Update()
	{
		playingTransitions_.RemoveAll(transition => transition.RemainingTime() == false);
		// 時間を進める
		foreach( ColorTransitionBase transition in playingTransitions_ )
		{
			transition.Update();
			// Dirty設定
			if( transition is ColorStateTransition )
			{
				StateGroups[transition.Name].SetReferenceColorDirty();
			}
			else if( transition is ColorParameterTransition )
			{
				Parameters[transition.Name].SetReferenceColorDirty();
			}
		}
	}

	void LateUpdate()
	{
	}

	#endregion


	#region initialize

	void InitTransitionDict()
	{
		foreach( ColorGameSyncByState stateGroup in StateGroups.Values )
		{
			if( stateGroup.NeedTransition && stateGroup.IsGlobal )
			{
				ColorStateTransition colorStateTransition = new ColorStateTransition();
				colorStateTransition.Init(stateGroup, initialValue: stateGroup.GlobalValue);
				stateTransitionDict_.Add(stateGroup.GroupName, colorStateTransition);
			}
		}
		foreach( ColorGameSyncByParameter parameter in Parameters.Values )
		{
			if( parameter.NeedTransition && parameter.IsGlobal )
			{
				ColorParameterTransition colorParameterTransition = new ColorParameterTransition();
				colorParameterTransition.Init(parameter, initialValue: parameter.GlobalValue);
				parameterTransitionDict_.Add(parameter.ParameterName, colorParameterTransition);
			}
		}
	}

	#endregion


	#region states / parameters

	// states    -------------------------------------------------------------------------------------------------

	// dictionary
	public bool ContainsStateGroup_(string name) { return StateGroups.ContainsKey(name); }
	public ColorGameSyncByState GetStateGroup_(string name) { return StateGroups[name]; }
	public ColorGameSyncByState GetStateGroup_(int index) { return StateGroups.At(index); }

	// global value
	public string GetGlobalState_(string name) { return StateGroups[name].GlobalValue; }
	public void SetGlobalState_(string stateGroupName, string stateName)
	{
		if( ContainsStateGroup_(stateGroupName) == false )
		{
			return;
		}

		ColorGameSyncByState stateGroup = GetStateGroup(stateGroupName);
		if( stateGroup.IsGlobal == false )
		{
			return;
		}

		stateGroup.GlobalValue = stateName;
		stateGroup.SetReferenceColorDirty();

		if( stateTransitionDict_.ContainsKey(stateGroupName) )
		{
			ColorStateTransition stateTransition = stateTransitionDict_[stateGroupName];
			foreach( ColorSourceBase source in stateGroup.GetReferenceColors() )
			{
				if( source is SwitchColorSource )
				{
					(source as SwitchColorSource).OnGlobalStateTransitionStart(stateGroupName, stateName, stateTransition);
				}
			}
			stateTransition.SetTarget(stateName);
			if( playingTransitions_.Contains(stateTransition) == false )
			{
				playingTransitions_.Add(stateTransition);
			}
		}
	}

	// transition
	public bool IsStateTransitioning_(string name)
	{
		return stateTransitionDict_.ContainsKey(name) && playingTransitions_.Contains(stateTransitionDict_[name]);
	}
	public ColorStateTransition GetStateTransition_(string name)
	{
		return stateTransitionDict_[name];
	}


	// parameters -------------------------------------------------------------------------------------------------

	// dictionary
	public bool ContainsParameter_(string name) { return Parameters.ContainsKey(name); }
	public ColorGameSyncByParameter GetParameter_(string name) { return Parameters[name]; }
	public ColorGameSyncByParameter GetParameter_(int index) { return Parameters.At(index); }

	// global value
	public float GetGlobalParameter_(string name) { return Parameters[name].GlobalValue; }
	public void SetGlobalParameter_(string parameterName, float value)
	{
		if( ContainsParameter_(parameterName) == false )
		{
			return;
		}

		ColorGameSyncByParameter parameter = GetParameter(parameterName);
		if( parameter.IsGlobal == false )
		{
			return;
		}

		parameter.GlobalValue = value;
		parameter.SetReferenceColorDirty();

		if( parameterTransitionDict_.ContainsKey(parameterName) )
		{
			ColorParameterTransition parameterTransition = parameterTransitionDict_[parameterName];
			foreach( ColorSourceBase source in parameter.GetReferenceColors() )
			{
				if( source is BlendColorSource )
				{
					(source as BlendColorSource).OnGlobalParameterTransitionStart(parameterName, value, parameterTransition);
				}
			}
			parameterTransition.SetTarget(value);
			if( playingTransitions_.Contains(parameterTransition) == false )
			{
				playingTransitions_.Add(parameterTransition);
			}
		}
	}
	
	// transition
	public bool IsParameterTransitioning_(string name)
	{
		return parameterTransitionDict_.ContainsKey(name) && playingTransitions_.Contains(parameterTransitionDict_[name]);
	}
	public ColorParameterTransition GetParameterTransition_(string name)
	{
		return parameterTransitionDict_[name];
	}


	// static functions

	public static bool ContainsStateGroup(string name) { return Instance.ContainsStateGroup_(name); }
	public static ColorGameSyncByState GetStateGroup(string name) { return Instance.GetStateGroup_(name); }
	public static string GetGlobalState(string name) { return Instance.GetGlobalState_(name); }
	public static void SetGlobalState(string stateGroupName, string stateName) { Instance.SetGlobalState_(stateGroupName, stateName); }
	public static bool IsStateTransitioning(string name) { return Instance.IsStateTransitioning_(name); }
	public static ColorStateTransition GetStateTransition(string name) { return Instance.GetStateTransition_(name); }

	public static bool ContainsParameter(string name) { return Instance.ContainsParameter_(name); }
	public static ColorGameSyncByParameter GetParameter(string name) { return Instance.GetParameter_(name); }
	public static float GetGlobalParameter(string name) { return Instance.GetGlobalParameter_(name); }
	public static void SetGlobalParameter(string parameterName, float value) { Instance.SetGlobalParameter_(parameterName, value); }
	public static bool IsParameterTransitioning(string name) { return Instance.IsParameterTransitioning_(name); }
	public static ColorParameterTransition GetParameterTransition(string name) { return Instance.GetParameterTransition_(name); }

	#endregion

}

[System.Serializable]
public class ColorStatePair : SerializableKeyValuePair<string, ColorGameSyncByState> { public ColorStatePair(string key, ColorGameSyncByState value) : base(key, value) { } }

[System.Serializable]
public class SerializableColorStateGroupDictionary : SerializableDictionary<string, ColorGameSyncByState, ColorStatePair> { }

[System.Serializable]
public class ColorParameterPair : SerializableKeyValuePair<string, ColorGameSyncByParameter> { public ColorParameterPair(string key, ColorGameSyncByParameter value) : base(key, value) { } }

[System.Serializable]
public class SerializableColorParameterDictionary : SerializableDictionary<string, ColorGameSyncByParameter, ColorParameterPair> { }
