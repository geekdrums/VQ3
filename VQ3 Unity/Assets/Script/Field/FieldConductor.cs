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
		string message = "";
		foreach ( GameObject e in Encounters[encounterCount].Enemies )
		{
			message += e.GetComponent<Enemy>().ToString() + " Ç™Ç†ÇÁÇÌÇÍÇΩÅI\n";
		}
		TextWindow.AddMessage( new GUIMessage( message ) );
		++encounterCount;
		encounterCount %= Encounters.Length;
    }
}
