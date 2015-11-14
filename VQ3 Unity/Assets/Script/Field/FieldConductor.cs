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

public class FieldConductor : MonoBehaviour {

	public bool RefreshWatchFlags = true;
	public List<StageData> StageData;
	public int StageIndex;
	public int EncountIndex;
	public GameObject Setting;

	public Encounter CurrentEncounter { get { return (StageIndex >= StageData.Count || EncountIndex >= StageData[StageIndex].Encounters.Count ? null : StageData[StageIndex].Encounters[EncountIndex]); } }
	public EventData CurrentEvent { get; private set; }

    void Awake()
    {
		GameContext.FieldConductor = this;
		if( RefreshWatchFlags )
		{
			foreach( StageData stage in StageData )
			{
				foreach( EventData eventData in stage.EventData )
				{
					eventData.Watched = false;
				}
			}
		}
	}

	// Use this for initialization
	void Start()
	{
		for( int i=0; i<EncountIndex; ++i )
		{
			GameContext.PlayerConductor.OnGainMemory(StageData[0].Encounters[i].AcquireMemory);
		}

		PlayerCommand acquiredCommand = GameContext.PlayerConductor.CheckAcquireCommand();
		while( acquiredCommand != null )
		{
			acquiredCommand.Acquire();
			acquiredCommand = GameContext.PlayerConductor.CheckAcquireCommand();
		}
	}

	// Update is called once per frame
	void Update()
    {
	}

	public void OnEnterResult()
	{
		EncountIndex++;
	}

    public void OnEnterSetting()
	{
		Setting.SetActive(true);
		if( Music.IsPlaying == false || Music.CurrentMusicName != "ambient" )
		{
			Music.Play("ambient");
		}
    }

	public void OnEnterBattle()
	{
		Setting.SetActive(false);
	}

	public void OnEnterEvent()
	{
		Setting.SetActive(true);
	}

	public GameState CheckEvent(GameState nextState)
	{
		if( StageIndex >= StageData.Count ) return nextState;

		if( CurrentEncounter == null )
		{
			CurrentEvent = StageData[StageIndex].EventData[StageData[StageIndex].EventData.Count-1];
			return GameState.Event;
		}

		int currentEncountIndex = StageData[StageIndex].Encounters.IndexOf(CurrentEncounter);
		EventData eventData = StageData[StageIndex].EventData.Find((EventData e) => e.EncounterIndex ==  currentEncountIndex && e.NextState == nextState);
		if( eventData != null && eventData.Watched == false )
		{
			CurrentEvent = eventData;
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
	}
}
