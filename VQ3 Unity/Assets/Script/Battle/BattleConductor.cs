using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleConductor : MonoBehaviour {

	List<Pair<Timing,Command>> Commands;

	// Use this for initialization
	void Start ()
	{
		GameContext.BattleConductor = this;
        Commands = new List<Pair<Timing, Command>>();
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
            OnBarStarted( Music.Just.bar );
        }

        if( Music.isJustChanged )
        {
            List<Pair<ActionSet, bool>> CurrentActions = new List<Pair<ActionSet, bool>>();
            foreach( Pair<Timing, Command> cmd in Commands )
            {
                ActionSet act = cmd.Get<Command>().GetCurrentAction( cmd.Get<Timing>() );
                if( act != null )
                {
                    CurrentActions.Add( new Pair<ActionSet, bool>( act, cmd.Get<Command>().isPlayerAction ) );
                }
            }
            //Add setoff logic if needed.
            foreach( Pair<ActionSet, bool> act in CurrentActions )
            {
                GameContext.EnemyConductor.ReceiveAction( act.Get<ActionSet>(), act.second );
                GameContext.PlayerConductor.ReceiveAction( act.Get<ActionSet>(), act.second );
            }

            Commands.RemoveAll( ( Pair<Timing, Command> cmd ) => cmd.Get<Command>().IsEnd( cmd.Get<Timing>() ) );
        }
    }

	void OnBarStarted( int CurrentIndex )
	{
		GameContext.CommandController.OnBarStarted( CurrentIndex );
		GameContext.PlayerConductor.OnBarStarted( CurrentIndex );
	}

	public void ExecCommand( Command NewCommand )
	{
		Commands.Add( new Pair<Timing, Command>( new Timing( Music.Just ), NewCommand ) );
	}


	public void OnPlayerWin()
	{
        Music.SetNextBlock("Endro");
	}
	public void OnPlayerLose()
	{
	}
}
