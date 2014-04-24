using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ResultState
{
    Status1,
    Status2,
    Quarter,
    Command,
    Moon1,
    Moon2,
    End
}
public class FieldConductor : MonoBehaviour {

    [System.Serializable]
    public class LevelEncounter
    {
        public List<Encounter> Encounters = new List<Encounter>();
    }

    LevelEncounter[] LevelEncounters;
	int encounterCount;

    public ResultState RState { get; private set; }
    LevelEncounter CurrentLevel { get { return LevelEncounters[GameContext.PlayerConductor.Level - 1]; } }

	// Use this for initialization
    void Start()
    {
        GameContext.FieldConductor = this;

        LevelEncounters = new LevelEncounter[50];
        for( int i = 0; i < 50; i++ )
        {
            LevelEncounters[i] = new LevelEncounter();
        }
        foreach( Encounter encounter in GetComponentsInChildren<Encounter>() )
        {
            LevelEncounters[encounter.Level-1].Encounters.Add( encounter );
        }
	}
	
	// Update is called once per frame
	void Update () {
        switch( GameContext.CurrentState )
        {
        case GameState.Field:
            if( encounterCount >= CurrentLevel.Encounters.Count )
            {
                if( GameContext.PlayerConductor.Level >= LevelEncounters.Length ) return;
                else
                {
                    GameContext.PlayerConductor.Level++;
                    GameContext.PlayerConductor.OnLevelUp();
                    encounterCount = 0;
                    //RState = ResultState.Status1;
                    //GameContext.ChangeState( GameState.Result );
                }
            }
            else
            {
                CheckEncount();
            }
            break;
        //case GameState.Result:
        //    GameContext.PlayerConductor.UpdateResult();
        //    break;
        default:
            break;
        }
        //if( Input.GetMouseButtonDown( 0 ) )
        //{
        //}
	}

    void CheckEncount()
    {
        Music.Stop();
        TextWindow.SetNextCursor( false );
        GameContext.EnemyConductor.SetEncounter( CurrentLevel.Encounters[encounterCount] );
        GameContext.ChangeState( GameState.Intro );
        ++encounterCount;
    }

    public void MoveNextResult()
    {
        ++RState;
        if( RState == ResultState.End )
        {
            GameContext.ChangeState( GameState.Field );
        }
    }
}
