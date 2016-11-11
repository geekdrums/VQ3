using UnityEngine;
using System;
using System.Collections.Generic;

//Game State
public enum GameState
{
	Title,
	Stage,
	Event,
	Setting,
	Battle,
    Result
}

public static class GameContext
{
	public static GameState State = GameState.Setting;

	public static void SetState(GameState nextState)
	{
		GameState OldState = State;
		if( OldState != GameState.Event )
		{
			State = FieldConductor.CheckEvent(nextState);
		}
		else
		{
			State = nextState;
		}
		OnLeaveState(OldState);
		OnEnterState(State);

		//Debug.Log("Enter GameState: " + State.ToString());
	}
	static void OnLeaveState(GameState OldState)
	{
		switch( OldState )
		{
		case GameState.Battle:
			break;
		case GameState.Event:
			break;
		case GameState.Result:
			break;
		}
	}
	static void OnEnterState(GameState NewState)
	{
		switch( NewState )
		{
		case GameState.Battle:
			LuxSystem.OnBattleStarted(FieldConductor.CurrentEncounter);
			Music.Play("BattleMusic", "Intro");
			ColorManager.SetBaseColor(EBaseColor.Black);
			FieldConductor.OnEnterBattle();
			BattleConductor.SetState(BattleState.Intro);
			LuxSystem.SetState(LuxState.Sun);
			EnemyConductor.SetEncounter(FieldConductor.CurrentEncounter);
			PlayerConductor.OnBattleStarted();
			break;
		case GameState.Event:
			EventConductor.OnEnterEvent(FieldConductor.CurrentEvent);
			FieldConductor.OnEnterEvent();
			PlayerConductor.OnEnterEvent();
			break;
		case GameState.Result:
			ResultConductor.OnEnterResult(FieldConductor.CurrentEncounter.AcquireMemory);
			FieldConductor.OnEnterResult();
			PlayerConductor.OnEnterResult();
			break;
		case GameState.Setting:
			ColorManager.SetBaseColor(EBaseColor.Black);
			FieldConductor.OnEnterSetting();
			PlayerConductor.OnEnterSetting();
			ResultConductor.OKButton.SetMode(ButtonMode.Hide, true);
			break;
		}
	}

	//Conductors
	public static BattleConductor BattleConductor;
    public static EnemyConductor EnemyConductor;
	public static PlayerConductor PlayerConductor;
	public static LuxSystem LuxSystem;
	public static FieldConductor FieldConductor;
	public static ResultConductor ResultConductor;
	public static EventConductor EventConductor;
	public static Camera MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

	//States
	public static BattleState BattleState { get { return BattleConductor.State; } }
	public static LuxState LuxState { get { return LuxSystem.State; } }
	public static ResultState ResultState { get { return ResultConductor.State; } }
}
