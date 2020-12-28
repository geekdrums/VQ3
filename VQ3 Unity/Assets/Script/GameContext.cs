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

public class GameContext : MonoBehaviour
{
	public GameState State_;
	public BattleConductor BattleConductor_;
	public EnemyConductor EnemyConductor_;
	public PlayerConductor PlayerConductor_;
	public LuxSystem LuxSystem_;
	public FieldConductor FieldConductor_;
	public ResultConductor ResultConductor_;
	public EventConductor EventConductor_;
	public Camera MainCamera_;

	public int EncounterIndex = 0;
	public int ScreenWidth = 1366;
	public int ScreenHeight = 768;

	void Awake()
	{
		if( Application.platform == RuntimePlatform.WindowsPlayer ||
		Application.platform == RuntimePlatform.OSXPlayer ||
		Application.platform == RuntimePlatform.LinuxPlayer )
		{
			Screen.SetResolution(ScreenWidth, ScreenHeight, false);
			Screen.fullScreen = true;
		}
		GameContext.FieldConductor.EncounterIndex = EncounterIndex;
		Application.targetFrameRate = 60;
	}

	void Start()
	{
		SetStateInternal(State_);
	}

	void OnValidate()
	{
		Instance_ = this;
	}

	public static void SetState(GameState nextState)
	{
		Instance.SetStateInternal(nextState);
	}
	public void SetStateInternal(GameState nextState)
	{
		GameState oldState = State_;
		if( oldState != GameState.Event )
		{
			State_ = FieldConductor_.CheckEvent(nextState);
		}
		else
		{
			State_ = nextState;
		}
		OnLeaveState(oldState);
		OnEnterState(State_);

		//Debug.Log("Enter GameState: " + State.ToString());
	}
	void OnLeaveState(GameState OldState)
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
	void OnEnterState(GameState NewState)
	{
		switch( NewState )
		{
		case GameState.Battle:
			LuxSystem.OnBattleStarted(FieldConductor.CurrentEncounter);
#if UNITY_EDITOR
			Music.Play("BattleMusic", PlayerConductor.CommandGraph.DebugIntroCommand.GetBlockName());
#else
			Music.Play("BattleMusic", PlayerConductor.CommandGraph.IntroCommand.GetBlockName());
#endif
			ColorManagerObsolete.SetBaseColor(EBaseColor.Black);
			ColorManager.SetGlobalState("Base", "Black");
			FieldConductor.OnEnterBattle();
			BattleConductor.SetState(BattleState.Intro);
			LuxSystem.SetState(LuxState.Shield);
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
			ColorManagerObsolete.SetBaseColor(EBaseColor.Black);
			ColorManager.SetGlobalState("Base", "Black");
			FieldConductor.OnEnterSetting();
			PlayerConductor.OnEnterSetting();
			//ResultConductor.OKButton.SetMode(ButtonMode.Hide, true);
			break;
		case GameState.Title:
			Music.Play("ambient");
			break;
		}
	}


	// accessors
	public static GameContext Instance_;
	public static GameContext Instance
	{
		get
		{
			if( Instance_ == null )
			{
				Instance_ = FindObjectOfType<GameContext>();
			}
			return Instance_;
		}
	}
	public static GameState State { get { return Instance.State_; } set { Instance.State_ = value; } }
	public static BattleConductor BattleConductor { get { return Instance.BattleConductor_; } }
    public static EnemyConductor EnemyConductor { get { return Instance.EnemyConductor_; } }
	public static PlayerConductor PlayerConductor { get { return Instance.PlayerConductor_; } }
	public static LuxSystem LuxSystem { get { return Instance.LuxSystem_; } }
	public static FieldConductor FieldConductor { get { return Instance.FieldConductor_; } }
	public static ResultConductor ResultConductor { get { return Instance.ResultConductor_; } }
	public static EventConductor EventConductor { get { return Instance.EventConductor_; } }
	public static Camera MainCamera { get { return Instance.MainCamera_; } }

	//States
	public static BattleState BattleState { get { return BattleConductor.State; } }
	public static LuxState LuxState { get { return LuxSystem.State; } }
	public static ResultState ResultState { get { return ResultConductor.State; } }
}
