using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleConductor : MonoBehaviour {

	List<KeyValuePair<Timing,Command>> Commands;

	// Use this for initialization
	void Start ()
	{
		GameContext.BattleConductor = this;
        Commands = new List<KeyValuePair<Timing, Command>>();
	}
	
	// Update is called once per frame
    void Update()
    {
        if( GameContext.CurrentState != GameContext.GameState.Battle ) return;
        switch( Music.GetCurrentBlockName() )
        {
        case "Attack"://TEMP!!!
            UpdateBattle();
            break;
        case "Endro":
            if( !Music.IsPlaying() )
            {
                GameContext.ChangeState( GameContext.GameState.Field );
            }
            break;
        }
    }

    void UpdateBattle()
    {
        if( Music.IsJustChangedWhen( ( Timing t ) => t.barUnit == 0 ) )
        {
            Debug.Log( "OnBarStarted " + Music.Just.ToString() );
            OnBarStarted( Music.Just.bar );
        }

        if( Music.isJustChanged )
        {
            foreach( KeyValuePair<Timing, Command> cmd in Commands )
            {
                ActionSet act = cmd.Value.GetCurrentAction( cmd.Key );
                if( act != null )
                {
                    GameContext.EnemyConductor.ReceiveAction( act, cmd.Value.isPlayerAction );
                    GameContext.PlayerConductor.ReceiveAction( act, cmd.Value.isPlayerAction );
                }
            }
            Commands.RemoveAll( ( KeyValuePair<Timing, Command> cmd ) => cmd.Value.IsEnd( cmd.Key ) );
        }
    }

	void OnBarStarted( int CurrentIndex )
	{
		GameContext.CommandController.OnBarStarted( CurrentIndex );
		GameContext.PlayerConductor.OnBarStarted( CurrentIndex );
	}

	public void ExecCommand( Command NewCommand )
	{
		Commands.Add( new KeyValuePair<Timing, Command>( new Timing( Music.Just ), NewCommand ) );
	}


	public void OnPlayerWin()
	{
        Music.SetNextBlock("Endro");
	}
	public void OnPlayerLose()
	{
	}
}
