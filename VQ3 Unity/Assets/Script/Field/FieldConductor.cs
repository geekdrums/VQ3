using UnityEngine;
using System.Collections;

public class FieldConductor : MonoBehaviour {

    public Encounter[] Encounters;

	int encounterCount;

	// Use this for initialization
    void Start()
    {
        //Music.Play( "fieldMusic" );
	}
	
	// Update is called once per frame
	void Update () {
        if( GameContext.CurrentState != GameContext.GameState.Field ) return;

		//if ( Music.IsJustChangedAt( 0 ) && Music.numRepeat > 0 )
		//{
			CheckEncount();
		//}
	}

    void CheckEncount()
    {
        Music.Stop();
		GameContext.EnemyConductor.SetEnemy( Encounters[encounterCount].Enemies );
        GameContext.ChangeState( GameContext.GameState.Intro );
		foreach ( GameObject e in Encounters[encounterCount].Enemies )
        {
            TextWindow.AddMessage( new GUIMessage( e.GetComponent<Enemy>().DisplayName + " �������ꂽ�I" ) );
		}
		++encounterCount;
		encounterCount %= Encounters.Length;
    }
}
