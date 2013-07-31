using UnityEngine;
using System.Collections;

public class FieldConductor : MonoBehaviour {

    public Encounter[] Encounters;

	// Use this for initialization
    void Start()
    {
        Music.Play( "fieldMusic" );
	}
	
	// Update is called once per frame
	void Update () {
        if( GameContext.CurrentState != GameContext.GameState.Field ) return;

        CheckEncount();
	}

    void CheckEncount()
    {
        foreach( Encounter e in Encounters )
        {
            if( e.IsEncountered() )
            {
                Music.Stop();
                GameContext.EnemyConductor.SetEnemy( e.Enemies );
                GameContext.ChangeState( GameContext.GameState.Battle );
                break;
            }
        }
    }
}
