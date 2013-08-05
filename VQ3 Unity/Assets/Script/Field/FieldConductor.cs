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
        foreach( Encounter encounter in Encounters )
        {
            if( encounter.IsEncountered() )
            {
                Music.Stop();
                GameContext.EnemyConductor.SetEnemy( encounter.Enemies );
                GameContext.ChangeState( GameContext.GameState.Battle );
				string message = "";
				foreach ( GameObject e in encounter.Enemies )
				{
					message += e.GetComponent<Enemy>().ToString() + " Ç™Ç†ÇÁÇÌÇÍÇΩÅI\n";
				}
				TextWindow.AddMessage( new GUIMessage( message ) );
                break;
            }
        }
    }
}
