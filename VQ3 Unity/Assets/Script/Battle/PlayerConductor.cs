using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerConductor : MonoBehaviour {

	public CommandGraph CommandGraph;
	public MemoryPanel MemoryPanel;

    public int Level = 1;
	public int TotalSP;
	public int RemainSP;
    public int PlayerHP { get { return Player.HitPoint; } }
	public int PlayerMaxHP { get { return Player.MaxHP; } }
	public bool PlayerIsDanger { get { return Player.IsDangerMode; } }
    public bool CanUseInvert { get { return Level >= 6; } }
	public bool IsEclipse { get { return CommandGraph.CurrentCommand is RevertCommand; } }
    public int WaitCount { get; private set; }

	PlayerCommand CurrentCommand;
	PlayerCommand SelectedCommand;
	Player Player;
    float resultRemainTime;
	int resultGainMemory;
    readonly float DefaultResultTime = 0.4f;

	void Awake()
	{
		GameContext.PlayerConductor = this;
	}

	// Use this for initialization
	void Start () {
		Player = GameObject.Find( "Main Camera" ).GetComponent<Player>();
        SetLevelParams();
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void CheckResult( int sp )
	{
		resultGainMemory = sp;
	}

    public void UpdateResult()
    {
        resultRemainTime -= Time.deltaTime;
        if( resultRemainTime <= 0 )
        {
            TextWindow.SetNextCursor( true );
        }
    }

	public void ProceedResult()
	{
		if( resultRemainTime <= 0 )
		{
			TextWindow.SetNextCursor(false);
			resultRemainTime = DefaultResultTime;
			if( GameContext.ResultState == ResultState.Memory )
			{
				RemainSP += resultGainMemory;
				TotalSP += resultGainMemory;
				SEPlayer.Play("newCommand");
				GameContext.ResultConductor.MoveNextResult();
			}
			else if( GameContext.ResultState == ResultState.Command )
			{
				PlayerCommand acquiredCommand = CommandGraph.CheckAcquireCommand(Level);
				if( acquiredCommand != null )
				{
					acquiredCommand.Acquire();
					CommandGraph.ShowAcquireCommand(acquiredCommand);
					SEPlayer.Play("newCommand");
				}
				else
				{
					GameContext.ResultConductor.MoveNextResult();
				}
			}
			else
			{
				GameContext.ResultConductor.MoveNextResult();
			}
		}
	}

    void SetLevelParams()
    {
        Player.HitPoint = 500 + Level * 100;
        Player.BasePower = 50 + Level * 10;
        Player.BaseMagic = 50 + Level * 10;
        Player.Initialize();
    }

    public void OnLevelUp()
    {
        SetLevelParams();
        if( Level > 1 )
        {
            TextWindow.SetMessage( MessageCategory.Result, "オクスは　レベル" + Level + "に　あがった！" );
            resultRemainTime = DefaultResultTime;
            SEPlayer.Play( "levelUp" );
        }
    }

    public void CheckAcquireCommands()
    {
        PlayerCommand acquiredCommand = CommandGraph.CheckAcquireCommand( Level );
        while( acquiredCommand != null )
        {
            acquiredCommand.Acquire();
            acquiredCommand = CommandGraph.CheckAcquireCommand( Level );
        }
        PlayerCommand forgetCommand = CommandGraph.CheckForgetCommand( Level );
        while( forgetCommand != null )
        {
            forgetCommand.Forget();
            forgetCommand = CommandGraph.CheckAcquireCommand( Level );
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
				TextWindow.SetCommand(command);
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
				TextWindow.SetCommand(command);
			}
			break;
		case GameState.Setting:
			if( command == CommandGraph.IntroCommand )
			{
				MemoryPanel.Reset();
				TextWindow.SetMessage(MessageCategory.Help, "コマンドを選択して\nメモリーを分配できます");
			}
			else
			{
				MemoryPanel.Set(command);
				TextWindow.SetCommand(command);
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
		}
		switch( GameContext.State )
		{
		case GameState.Battle:
			TextWindow.SetMessage(MessageCategory.Help, "次のコマンドは　なに？");
			break;
		case GameState.Setting:
			MemoryPanel.Reset();
			TextWindow.SetMessage(MessageCategory.Help, "コマンドを選択して\nメモリーを分配できます");
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
		}
		Player.TurnInit(CurrentCommand.currentData);
		TextWindow.SetMessage(MessageCategory.PlayerCommand, CurrentCommand.currentData.DescribeText);
        WaitCount = 0;
	}

    public void ExecWaitCommand()
    {
        CurrentCommand = null;
        Player.DefaultInit();
		if( WaitCount == 0 )
		{
			TextWindow.SetMessage(MessageCategory.PlayerWait, "オクスは　つぎの　いってを　かんがえている");
		}
        ++WaitCount;
	}
    
    public void CheckSkill()
    {
        if( CurrentCommand  == null ) return;
        GameObject playerSkill = CurrentCommand.GetCurrentSkill();
        if( playerSkill != null )
        {
            Skill objSkill = (Instantiate( playerSkill ) as GameObject).GetComponent<Skill>();
            objSkill.SetOwner( Player );
            GameContext.BattleConductor.ExecSkill( objSkill );
        }
    }

	public void OnBattleStarted()
    {
		MemoryPanel.Hide();
        Player.OnBattleStart();
        CommandGraph.OnBattleStart();
		CurrentCommand = CommandGraph.IntroCommand;
		CurrentCommand.SetCurrent();
        WaitCount = 0;
		ColorManager.SetBaseColor(EBaseColor.Black);
		ColorManager.SetThemeColor(EThemeColor.White);
    }

	public bool ReceiveAction( ActionSet Action, Skill skill )
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
                Player.BeAttacked( attack, skill );
				int vpDamage = (int)(attack.VP * Player.VPCoeff);
				if( vpDamage < 0 )
				{
					GameContext.VoxSystem.AddVPVT(vpDamage, 0);
					Player.VPDrained(attack, skill, -vpDamage);
				}
                isSucceeded = true;
            }
        }
		HealModule heal = Action.GetModule<HealModule>();
        if( heal != null && skill.isPlayerSkill )
		{
			Player.Heal( heal );
			isSucceeded = true;
        }
		EnhanceModule enhance = Action.GetModule<EnhanceModule>();
        if( enhance != null && enhance.TargetType == TargetType.Player )
		{
            Player.Enhance( enhance );
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

	public void OnOverFlowed()
	{
		Player.EnhanceCutIn.SetReadyEclipse();
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
    }
    public void OnContinue()
    {
        Player.HitPoint = Player.MaxHP;
        Player.DefaultInit();
    }
}
