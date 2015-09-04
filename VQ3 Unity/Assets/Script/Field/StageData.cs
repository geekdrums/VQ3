using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class CreateStageData : MonoBehaviour
{
	[MenuItem("Assets/Create/CreateStageData")]
	public static void CreateAsset()
	{
		StageData item = ScriptableObject.CreateInstance<StageData>();

		string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/StageData/" + typeof(StageData) + ".asset");

		AssetDatabase.CreateAsset(item, path);
		AssetDatabase.SaveAssets();

		EditorUtility.FocusProjectWindow();
		Selection.activeObject = item;
	}
}

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

	public int AcquireStars = 10;
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
}