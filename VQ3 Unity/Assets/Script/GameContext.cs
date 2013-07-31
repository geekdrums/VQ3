using System;
using System.Collections.Generic;

public static class GameContext
{
    //Game State
    public enum GameState
    {
        Battle,
        Field
    }
    public static GameState CurrentState = GameState.Field;
    public static void ChangeState( GameState NewState )
    {
        GameState OldState = CurrentState;
        CurrentState = NewState;
        OnLeaveState( OldState );
        OnEnterState( CurrentState );
    }
    static void OnLeaveState( GameState OldState )
    {
        switch( OldState )
        {
        case GameState.Battle:
            break;
        case GameState.Field:
            break;
        }
    }
    static void OnEnterState( GameState NewState )
    {
        switch( NewState )
        {
        case GameState.Battle:
            Music.Play( "battleMusic" );
            break;
        case GameState.Field:
            Music.Play( "fieldMusic" );
            break;
        }
    }

    //Conductors
    public static BattleConductor BattleConductor;
    public static EnemyConductor EnemyConductor;
    public static PlayerConductor PlayerConductor;
    public static CommandController CommandController;
}
