using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleConductor : MonoBehaviour {

	List<Pair<Timing, Command>> Commands;

	public enum VoxonState
	{
		Hide,
		Show,
		ShowBreak,
		Break,
		HideBreak,
	}
	public VoxonState state { get; private set; }
	public VoxonSystem voxonSystem;

	readonly int BREAK_VOXON = 6;
	int deltaVoxon = 1;
	int currentVoxon = 0;

	// Use this for initialization
	void Start ()
	{
		GameContext.BattleConductor = this;
        Commands = new List<Pair<Timing, Command>>();
	}
	
	// Update is called once per frame
    void Update()
    {
		switch ( GameContext.CurrentState )
		{
		case GameContext.GameState.Intro:
			if ( Music.IsJustChangedAt( 0 ) && ( Music.UseADX ? Music.GetCurrentBlockName() == "battle" : Music.CurrentMusicName != "intro" ) )
			{
				GameContext.ChangeState( GameContext.GameState.Battle );
				UpdateBattle();
			}
			break;
		case GameContext.GameState.Battle:
            UpdateBattle();
			if ( Music.IsJustChangedAt( 0 ) && Music.GetCurrentBlockName() == "GotoEndro" )
			{
				SetState( VoxonState.HideBreak );
				AddVoxon( -currentVoxon );
				GameContext.ChangeState( GameContext.GameState.Endro );
				TextWindow.AddMessage( "てきを　やっつけた！", "３のけいけんちを　えた！" );
			}
            break;
		case GameContext.GameState.Endro:
			if ( Music.IsJustChangedAt(2) && Music.GetCurrentBlockName() == "endro" )
			{
				GameContext.ChangeState( GameContext.GameState.Field );
			}
            break;
		}
    }

    void UpdateBattle()
    {
        if( Music.IsJustChangedBar() && Music.GetCurrentBlockName() != "GotoEndro" )
        {
            OnBarStarted( Music.Just.bar );
        }

        if( Music.isJustChanged )
        {
            List<Pair<ActionSet, Command>> CurrentActions = new List<Pair<ActionSet, Command>>();
            foreach( Pair<Timing, Command> cmd in Commands )
            {
                ActionSet act = cmd.Get<Command>().GetCurrentAction( cmd.Get<Timing>() );
                if( act != null )
                {
                    CurrentActions.Add( new Pair<ActionSet, Command>( act, cmd.Get<Command>() ) );
                }
            }
            //Add setoff logic if needed.
            foreach( Pair<ActionSet, Command> act in CurrentActions )
            {
				bool isSucceeded = false;
                isSucceeded |= GameContext.EnemyConductor.ReceiveAction( act.Get<ActionSet>(), act.Get<Command>() );
				isSucceeded |=GameContext.PlayerConductor.ReceiveAction( act.Get<ActionSet>(), act.Get<Command>() );
				if ( isSucceeded )
				{
					act.Get<Command>().OnExecuted();
				}
            }

			Commands.RemoveAll( ( Pair<Timing, Command> cmd ) => cmd.Get<Command>().CheckIsEnd( cmd.Get<Timing>() ) );
        }
    }

	void OnBarStarted( int CurrentIndex )
	{
		if ( CurrentIndex == 0 )
		{
			GameContext.PlayerConductor.On4BarStarted();
		}
		else if ( CurrentIndex == 3 && state != VoxonState.ShowBreak && state != VoxonState.Break )
		{
			AddVoxon( -deltaVoxon );
		}
        GameContext.PlayerConductor.OnBarStarted(CurrentIndex);
        GameContext.EnemyConductor.OnBarStarted(CurrentIndex);
	}

	public void ExecCommand( Command NewCommand )
	{
		Commands.Add( new Pair<Timing, Command>( new Timing( Music.Just ), NewCommand ) );
	}


	public void OnPlayerWin()
	{
        Music.SetNextBlock("GotoEndro");
	}
	public void OnPlayerLose()
	{
	}

	public void AddVoxon( int value )
	{
		currentVoxon = Mathf.Clamp( currentVoxon + value, 0, BREAK_VOXON );
		voxonSystem.SetCurrentVoxon( (float)currentVoxon/BREAK_VOXON );
		Music.SetAisac( 2, Mathf.Sqrt( (float)currentVoxon/BREAK_VOXON ) );
	}
	public bool DetermineWillShowBreak( int willGainVoxon )
	{
		if ( currentVoxon + willGainVoxon >= BREAK_VOXON )
		{
			SetState( VoxonState.ShowBreak );
		}
		return state == VoxonState.ShowBreak;
	}

	public void SetState( VoxonState newState )
	{
		Debug.Log( "SetState: "+newState );
		switch( newState )
		{
		case VoxonState.Hide:
			voxonSystem.Hide();
			break;
		case VoxonState.HideBreak:
			AddVoxon( -currentVoxon );
			voxonSystem.HideBreak();
			GameContext.EnemyConductor.baseColor = Color.black;
			break;
		case VoxonState.Show:
			voxonSystem.SetCurrentVoxon( (float)currentVoxon/BREAK_VOXON );
			break;
		}
		state = newState;
	}
}
