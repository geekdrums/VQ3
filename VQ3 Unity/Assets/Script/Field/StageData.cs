using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
public class CreateStageData : MonoBehaviour
{
	[UnityEditor.MenuItem("Assets/Create/CreateStageData")]
	public static void CreateAsset()
	{
		StageData item = ScriptableObject.CreateInstance<StageData>();

		string path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/StageData/" + typeof(StageData) + ".asset");

		UnityEditor.AssetDatabase.CreateAsset(item, path);
		UnityEditor.AssetDatabase.SaveAssets();

		UnityEditor.EditorUtility.FocusProjectWindow();
		UnityEditor.Selection.activeObject = item;
	}
}
#endif

[System.Serializable]
public class StageData : ScriptableObject
{
	public List<Encounter> Encounters;
	public List<EventData> EventData;
}

[System.Serializable]
public class Encounter
{
	[System.Serializable]
	public class BattleSet
	{
		public GameObject[] Enemies;
		public List<EnemyCommandState> States;
		public StateChangeCondition[] Conditions;
	}

	public BattleSet[] BattleSets;

	public int AcquireMemory = 10;
	public int BreakPoint = 100;
	public LuxVersion Version= 0;
}


[System.Serializable]
public class EnemyCommandSet
{
	public string Command;
	public GameObject RingPrefab;
	public float Threat;
}

[System.Serializable]
public class EnemyCommandState
{
	public string Name;
	public EnemyCommandSet[] Pattern;
	public string NextState;
}

public enum ConditionType
{
	PlayerHP,
	TurnCount,
	EnemyCount,
	Count
}
public struct ConditionParts
{
	public ConditionType conditionType;
	public int MaxValue;
	public int MinValue;
}

[System.Serializable]
public class StateChangeCondition : IEnumerable<ConditionParts>
{
	public List<string> _conditions;
	public string FromState;
	public string ToState;
	public bool ViceVersa;

	List<ConditionParts> conditionParts = new List<ConditionParts>();

	public void Parse()
	{
		foreach( string str in _conditions )
		{
			string[] conditionParams = str.Split(' ');
			if( conditionParams.Length != 3 ) Debug.LogError("condition param must be TYPE MIN MAX format. ->" + str);
			else
			{
				conditionParts.Add(new ConditionParts()
				{
					conditionType = (ConditionType)System.Enum.Parse(typeof(ConditionType), conditionParams[0]),
					MinValue = conditionParams[1] == "-" ? -9999999 : int.Parse(conditionParams[1]),
					MaxValue = conditionParams[2] == "-" ? +9999999 : int.Parse(conditionParams[2]),
				});
			}
		}
	}

	public IEnumerator<ConditionParts> GetEnumerator()
	{
		foreach( ConditionParts parts in conditionParts ) yield return parts;
	}
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}


[System.Serializable]
public class EventData
{
	[System.Serializable]
	public struct Message
	{
		public string Text;
		public string Sender;
		public string Macro;
	}

	public Message[] Messages;
	public int EncounterIndex;
	public GameState NextState = GameState.Setting;
	public string MusicName;
	public bool Watched = false;
}