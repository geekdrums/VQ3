using UnityEngine;
using System;
using System.Collections.Generic;

//Game State
public enum GameState
{
	Init,
	Field,
	SetMenu,
	//AskMenu,
	Intro,
	Battle,
	Continue,
    Endro,
    Result
}
public static class GameContext
{
	public static GameState CurrentState = GameState.Init;
	public static bool IsBattleState { get { return GameState.Intro <= CurrentState && CurrentState <= GameState.Endro; } }
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
			FieldConductor.OnContinue();
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
			VoxSystem.SetState(VoxState.Sun);
			ColorManager.SetBaseColor(EBaseColor.Black);
            PlayerConductor.OnBattleStarted();
			VoxSystem.OnBattleStarted();
			break;
		case GameState.Endro:
			break;
		case GameState.Battle:
            break;
        case GameState.Continue:
            break;
        case GameState.Field:
            break;
        case GameState.Result:
			FieldConductor.CheckResult();
            break;
		case GameState.SetMenu:
			Music.Play("ambient");
			PlayerConductor.SPPanel.ShowSPPanel();
			PlayerConductor.SPPanel.ShowBattleButton();
			PlayerConductor.commandGraph.CheckLinkedFromIntro();
			TextWindow.ChangeMessage(MessageCategory.Help, "SPを割り振って　戦闘で使用するコマンドを　選ぶことができます。");
			ColorManager.SetBaseColor(EBaseColor.Black);
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
