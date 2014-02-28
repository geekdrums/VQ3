using UnityEngine;
using System.Collections;

public class FieldConductor : MonoBehaviour {

    [System.Serializable]
    public class LevelEncounter
    {
        public Encounter[] Encounters;
    }

    public LevelEncounter[] LevelEncounters;
	public int encounterCount;

    LevelEncounter CurrentLevel { get { return LevelEncounters[GameContext.PlayerConductor.Level - 1]; } }

	// Use this for initialization
    void Start()
    {
        GameContext.FieldConductor = this;
	}
	
	// Update is called once per frame
	void Update () {
        if( GameContext.CurrentState != GameContext.GameState.Field ) return;

        CheckEncount();
	}

    void CheckEncount()
    {
        if( encounterCount >= CurrentLevel.Encounters.Length )
        {
            if( GameContext.PlayerConductor.Level >= LevelEncounters.Length ) return;
            else
            {
                GameContext.PlayerConductor.Level++;
                GameContext.PlayerConductor.OnLevelUp();
                encounterCount = 0;
            }
        }
        Music.Stop();
		GameContext.EnemyConductor.SetEncounter( CurrentLevel.Encounters[encounterCount] );
        GameContext.ChangeState( GameContext.GameState.Intro );
		++encounterCount;
    }
}
