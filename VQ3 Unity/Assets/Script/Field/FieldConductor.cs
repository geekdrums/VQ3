using UnityEngine;
using System.Collections;

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
        public Encounter[] Encounters;
    }

    public LevelEncounter[] LevelEncounters;
	public int encounterCount;

    public ResultState RState { get; private set; }
    LevelEncounter CurrentLevel { get { return LevelEncounters[GameContext.PlayerConductor.Level - 1]; } }

	// Use this for initialization
    void Start()
    {
        GameContext.FieldConductor = this;
	}
	
	// Update is called once per frame
	void Update () {
        switch( GameContext.CurrentState )
        {
        case GameState.Field:
            if( encounterCount >= CurrentLevel.Encounters.Length )
            {
                if( GameContext.PlayerConductor.Level >= LevelEncounters.Length ) return;
                else
                {
                    GameContext.PlayerConductor.Level++;
                    GameContext.PlayerConductor.OnLevelUp();
                    encounterCount = 0;
                    RState = ResultState.Status1;
                    GameContext.ChangeState( GameState.Result );
                }
            }
            else
            {
                CheckEncount();
            }
            break;
        case GameState.Result:
            GameContext.PlayerConductor.UpdateResult();
            break;
        default:
            break;
        }
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
