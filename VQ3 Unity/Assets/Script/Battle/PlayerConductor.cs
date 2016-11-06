using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerConductor : MonoBehaviour {

	public CommandGraph CommandGraph;
	public MemoryPanel MemoryPanel;
	public EnemyExplanation EnemyExp;
	public MidairPrimitive WindowFrame;

	public GameObject VPMeter;

    public int Level = 1;
	public int TotalMemory;
	public int RemainMemory;
	public float PlayerDamageTimeCoeff = 1.0f;
	public float PlayerDamageTimeMin = 0.15f;
	public float EnemyDamageBGShake = 1.0f;
	
	[System.Serializable]
	public class LevelInfo
	{
		public int NeedMemory;
		public int HP;
		public int Attack;
		public int Magic;
	}

	public List<LevelInfo> LevelInfoList;

    public int PlayerHP { get { return Player.HitPoint; } }
	public int PlayerMaxHP { get { return Player.MaxHP; } }
	public bool PlayerIsDanger { get { return Player.IsDangerMode; } }
	public bool IsEclipse { get { return CommandGraph.CurrentCommand is RevertCommand; } }
	public int WaitCount { get; private set; }


	PlayerCommand CurrentCommand;
	PlayerCommand SelectedCommand;
	Player Player;

	void Awake()
	{
		GameContext.PlayerConductor = this;
	}

	// Use this for initialization
	void Start () {
		Player = GetComponentInChildren<Player>();
        SetLevelParams();
		ValidateCommands();
	}

	// Update is called once per frame
	void Update()
	{
	}

	public bool OnGainMemory(int memory)
	{
		TotalMemory += memory;
		RemainMemory += memory;
		if( TotalMemory >= LevelInfoList[Level + 1].NeedMemory )
		{
			++Level;
			SetLevelParams();
			return true;
		}
		else return false;
	}

    public void OnLevelUp()
    {
        TextWindow.SetMessage( MessageCategory.Result, "システムをレベル" + Level + "にアップグレード。" );
        SEPlayer.Play( "levelUp" );
    }

	void SetLevelParams()
	{
		Player.HitPoint = LevelInfoList[Level].HP;//500 + Level * 100;
		Player.BasePower = LevelInfoList[Level].Attack; //50 + Level * 10;
		Player.BaseMagic = LevelInfoList[Level].Magic; //50 + Level * 10;
		Player.Initialize();
	}

	public PlayerCommand CheckAcquireCommand()
	{
		return CommandGraph.CheckAcquireCommand(Level);
	}

	public bool CanStartBattle()
	{
		if( GameContext.State == GameState.Event ) return false;
		foreach( PlayerCommand command in CommandGraph.IntroCommand.LinkedCommands )
		{
			if( command.currentLevel > 0 )
			{
				return RemainMemory < TotalMemory/2;
			}
		}
		return false;
	}

	public void ValidateCommands()
	{
		PlayerCommand acquiredCommand = CommandGraph.CheckAcquireCommand(Level);
		while( acquiredCommand != null )
		{
			acquiredCommand.Acquire();
			acquiredCommand = CommandGraph.CheckAcquireCommand(Level);
		}
		PlayerCommand forgetCommand = CommandGraph.CheckForgetCommand(Level);
		while( forgetCommand != null )
		{
			forgetCommand.Forget();
			forgetCommand = CommandGraph.CheckAcquireCommand(Level);
		}
	}

	public void OnSelectedCommand(PlayerCommand command)
	{
		if( SelectedCommand != null ) SelectedCommand.Deselect();
		SelectedCommand = command;
		SelectedCommand.Select();
		switch( GameContext.State )
		{
		case GameState.Battle:
			//イントロから別のコマンドを選んだ時
			if( command != CommandGraph.IntroCommand && (CurrentCommand == CommandGraph.IntroCommand || (CurrentCommand is InvertCommand)) )
			{
				foreach( CommandEdge line in CommandGraph.IntroCommand.linkLines )
				{
					line.SetEnabled(false);
				}
				//TextWindow.SetCommand(command);
			}
			//ブレイクからイントロに戻ってきた時
			else if( command == CommandGraph.IntroCommand )
			{
				foreach( CommandEdge line in CommandGraph.IntroCommand.linkLines )
				{
					line.SetEnabled(true);
				}
			}
			else
			{
				//TextWindow.SetCommand(command);
			}
			break;
		case GameState.Setting:
			if( command == CommandGraph.IntroCommand )
			{
				MemoryPanel.Reset();
				EnemyExp.SetEnemy(GameContext.FieldConductor.CurrentEncounter.BattleSets[0].Enemies[0].GetComponent<Enemy>());
			}
			else
			{
				MemoryPanel.Set(command);
				EnemyExp.Hide();
				//TextWindow.SetCommand(command);
			}
			break;
		}
	}

	public void OnDeselectedCommand()
	{
		if( SelectedCommand != null )
		{
			SelectedCommand.Deselect();
			SelectedCommand = null;
			CommandGraph.Deselect();
			if( GameContext.State == GameState.Setting )
			{
				MemoryPanel.Reset();
				EnemyExp.SetEnemy(GameContext.FieldConductor.CurrentEncounter.BattleSets[0].Enemies[0].GetComponent<Enemy>());
			}
		}
		switch( GameContext.State )
		{
		case GameState.Battle:
			TextWindow.SetMessage(MessageCategory.Help, "次のコマンドは？");
			//TextWindow.SetCommand(null);
			break;
		case GameState.Result:
			GameContext.ResultConductor.OnPushedOKButton(this, null);
			//TextWindow.SetCommand(null);
			break;
		}
	}

	public void ExecCommand()
    {
        CommandGraph.OnExecCommand();
		CurrentCommand = CommandGraph.CurrentCommand;
		if( CurrentCommand != null )
		{
			CurrentCommand.SetCurrent();
			ColorManager.SetThemeColor(CurrentCommand.themeColor);

			if( CurrentCommand.BGAnim != null && GameContext.LuxSystem.IsOverFlow )
			{
				CurrentCommand.BGAnim.Activate();
			}
			else
			{
				BGAnimBase.DeactivateCurrentAnim();
			}
		}
		Player.TurnInit(CurrentCommand.currentData);
		TextWindow.SetMessage(MessageCategory.PlayerCommand, CurrentCommand.currentData.DescribeText);
		//TextWindow.SetCommand(null);
        WaitCount = 0;
	}

    public void ExecWaitCommand()
    {
        CurrentCommand = null;
        Player.DefaultInit();
		if( WaitCount == 0 )
		{
			TextWindow.SetMessage(MessageCategory.PlayerWait, "コマンド命令があるまで待機。");
		}
		BGAnimBase.DeactivateCurrentAnim();
        ++WaitCount;
	}

	public void CheckSkill()
	{
		if( CurrentCommand  == null ) return;
		GameObject playerSkill = CurrentCommand.GetCurrentSkill();
		if( playerSkill != null )
		{
			Skill objSkill = (Instantiate(playerSkill) as GameObject).GetComponent<Skill>();
			objSkill.SetOwner(Player);
			GameContext.BattleConductor.ExecSkill(objSkill);
		}
	}

	public bool ReceiveAction(ActionSet Action, Skill skill)
	{
		bool isSucceeded = false;
		AnimModule anim = Action.GetModule<AnimModule>();
		if( anim != null && !skill.isPlayerSkill )
		{
			isSucceeded = true;
		}
		AttackModule attack = Action.GetModule<AttackModule>();
		if( attack != null )
		{
			if( skill.isPlayerSkill == false )
			{
				Player.BeAttacked(attack, skill);
				int vpDamage = (int)(attack.VP * Player.VPCoeff);
				if( vpDamage < 0 )
				{
					GameContext.LuxSystem.AddVP(vpDamage, 0);
					Player.VPDrained(attack, skill, -vpDamage);
				}
				isSucceeded = true;
			}
		}
		HealModule heal = Action.GetModule<HealModule>();
		if( heal != null && skill.isPlayerSkill )
		{
			Player.Heal(heal);
			isSucceeded = true;
		}
		EnhanceModule enhance = Action.GetModule<EnhanceModule>();
		if( enhance != null && enhance.TargetType == TargetType.Player )
		{
			Player.Enhance(enhance);
			isSucceeded = true;
		}
		WaitModule wait = Action.GetModule<WaitModule>();
		if( wait != null && !skill.isPlayerSkill )
		{
			isSucceeded = true;
		}
		return isSucceeded;
	}

	public void UpdateHealHP()
	{
		Player.UpdateHealHP();
	}

	public void OnBattleStarted()
	{
		WindowFrame.SetTargetWidth(1);
		EnemyExp.Hide();
		MemoryPanel.Hide();
        Player.OnBattleStart();
        CommandGraph.OnBattleStart();
		CurrentCommand = CommandGraph.IntroCommand;
		CurrentCommand.SetCurrent();
        WaitCount = 0;
		ColorManager.SetBaseColor(EBaseColor.Black);
		ColorManager.SetThemeColor(EThemeColor.White);
		//TextWindow.SetCommand(null);

		VPMeter.SetActive(GameContext.FieldConductor.CurrentEncounter.Version != LuxVersion.None);
    }

	public void OnEnterSetting()
	{
		WindowFrame.SetTargetWidth(12);
		CommandGraph.OnEnterSetting();
		MemoryPanel.Reset();
		EnemyExp.SetEnemy(GameContext.FieldConductor.CurrentEncounter.BattleSets[0].Enemies[0].GetComponent<Enemy>());
	}

	public void OnEnterResult()
	{
		Player.HitPoint = Player.MaxHP;
		Player.HPPanel.OnUpdateHP();
		WindowFrame.SetTargetWidth(12);
		BGAnimBase.DeactivateCurrentAnim();
	}

	public void OnEnterEvent()
	{
		WindowFrame.SetTargetWidth(12);
		CommandGraph.OnEnterSetting();
		EnemyExp.Hide();
		MemoryPanel.Reset();
	}

	public void OnOverFlowed()
	{
		if( GameContext.State == GameState.Event ) return;

		Player.CutInUI.SetOverflow();
		if( CurrentCommand.BGAnim != null )
		{
			CurrentCommand.BGAnim.Activate();
		}
	}
	public void OnRevert()
	{
		Player.CheckDangerMode();
	}
    public void OnPlayerWin()
	{
		Player.DefaultInit();
    }
    public void OnPlayerLose()
    {
		Player.DefaultInit();
		BGAnimBase.DeactivateCurrentAnim();
		CommandGraph.OnEnterContinue();
    }
    public void OnContinue()
    {
        Player.HitPoint = Player.MaxHP;
		Player.DefaultInit();
		BGAnimBase.DeactivateCurrentAnim();
		WindowFrame.SetTargetWidth(12);
    }
	public void OnEndro()
	{
		CommandGraph.OnEndro();
	}
}
