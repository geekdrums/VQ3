using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public enum FieldState
{
	None,
	EnemyInfo,
	CommandInfo
}

[ExecuteInEditMode]
public class FieldConductor : MonoBehaviour {

	public List<StageData> StageData;
	public int StageIndex;
	public int EncountIndex;
	public MidairPrimitive WindowFrame;
	public GameObject Setting;

	public Encounter CurrentEncounter { get { return StageData[StageIndex].Encounters[EncountIndex]; } }
	public EventData CurrentEvent { get; private set; }

	// Use this for initialization
    void Awake()
    {
		GameContext.FieldConductor = this;
	}

	// Update is called once per frame
    void Update()
    {
        switch( GameContext.State )
        {
		//case GameState.Title:
		//	if( Input.GetMouseButtonUp(0) )
		//	{
		//		foreach( PlayerCommand command in GameContext.PlayerConductor.CommandGraph.CommandNodes )
		//		{
		//			command.ValidateState();
		//			if( command.state != CommandState.DontKnow )
		//			{
		//				for( int i=command.currentLevel; i<command.commandData.Count; ++i )
		//				{
		//					int needSP = command.commandData[command.currentLevel].RequireSP - (command.currentLevel == 0 ? 0 : command.commandData[command.currentLevel-1].RequireSP);
		//					if( needSP <= GameContext.PlayerConductor.RemainSP )
		//					{
		//						command.LevelUp();
		//					}
		//				}
		//			}
		//		}
		//		GameContext.PlayerConductor.CommandGraph.CheckLinkedFromIntro();
		//		GameContext.SetState(GameState.Setting);
		//	}
		//	break;
        case GameState.Setting:
            break;
        case GameState.Result:
            break;
        default:
            break;
        }
	}

	public void OnEnterResult()
	{
		WindowFrame.SetTargetWidth(12);
		EncountIndex++;
	}

    public void OnEnterSetting()
	{
		WindowFrame.SetTargetWidth(12);
		Setting.SetActive(true);
		if( Music.IsPlaying == false || Music.CurrentMusicName != "ambient" )
		{
			Music.Play("ambient");
		}
    }

	public void OnEnterBattle()
	{
		Setting.SetActive(false);
		WindowFrame.SetTargetWidth(1);
	}

	public GameState CheckEvent(GameState nextState)
	{
		if( StageIndex >= StageData.Count ) return nextState;
		int currentEncountIndex = StageData[StageIndex].Encounters.IndexOf(CurrentEncounter);
		CurrentEvent = StageData[StageIndex].EventData.Find((EventData e) => e.EncounterIndex ==  currentEncountIndex && e.NextState == nextState);
		if( CurrentEvent != null )
		{
			return GameState.Event;
		}
		else
		{
			return nextState;
		}
	}

	public void OnContinue()
	{
	}

	public void OnPlayerLose()
	{
		//EncountIndex--;
		//CommandExp.SetEnemy(CurrentLevel.Encounters[encounterCount].BattleSets[0].Enemies[0].GetComponent<Enemy>());
	}
}
