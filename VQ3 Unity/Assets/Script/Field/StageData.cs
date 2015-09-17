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
		public EnemyConductor.StateSet[] StateSets;
	}

	public BattleSet[] BattleSets;

	public int AcquireMemory = 10;
	public int InvertVP = 100;
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
}