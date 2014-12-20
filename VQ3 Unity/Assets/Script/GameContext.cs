﻿using UnityEngine;
using System;
using System.Collections.Generic;

//Game State
public enum GameState
{
    Intro,
    Endro,
    Battle,
    Continue,
    Field,
    Result
}
public static class GameContext
{
    public static GameState CurrentState = GameState.Field;
    public static void ChangeState( GameState NewState )
    {
        GameState OldState = CurrentState;
        CurrentState = NewState;
        OnLeaveState( OldState );
        OnEnterState( CurrentState );

		Debug.Log( "EnterState:"+CurrentState.ToString() );
    }
    static void OnLeaveState( GameState OldState )
    {
        switch( OldState )
		{
		case GameState.Intro:
			break;
		case GameState.Endro:
			break;
        case GameState.Battle:
            break;
        case GameState.Continue:
            EnemyConductor.OnContinue();
            PlayerConductor.OnContinue();
            VoxSystem.SetState( VoxState.Sun );
            break;
        case GameState.Field:
            break;
        case GameState.Result:
            break;
        }
    }
    static void OnEnterState( GameState NewState )
    {
        switch( NewState )
		{
        case GameState.Intro:
			Music.Play( "BattleMusic", "intro" );
			ColorManager.SetBaseColor(EBaseColor.Black);
            PlayerConductor.OnBattleStarted();
			break;
		case GameState.Endro:
			//Music.Play( "IntroEndro", "endro" );
			break;
		case GameState.Battle:
            //if ( Music.UseADX )
            //{
            //    Music.Play( PlayerConductor.NextStrategyName, PlayerConductor.NextBlockName );
            //}
            break;
        case GameState.Continue:
            break;
        case GameState.Field:
            //Music.Play( "fieldMusic" );
            break;
        case GameState.Result:
            break;
        }
    }

    //Conductors
    public static FieldConductor FieldConductor;
	public static BattleConductor BattleConductor;
    public static EnemyConductor EnemyConductor;
    public static PlayerConductor PlayerConductor;
    public static VoxSystem VoxSystem;
    public static Camera MainCamera = GameObject.Find( "Main Camera" ).GetComponent<Camera>();
}
