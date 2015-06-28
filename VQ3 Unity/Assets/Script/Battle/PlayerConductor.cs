using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerConductor : MonoBehaviour {

	public CommandGraph commandGraph;
	public SPPanel SPPanel;
    public int Level = 1;
	public int TotalSP;
	public int RemainSP;
    public int PlayerHP { get { return Player.HitPoint; } }
	public int PlayerMaxHP { get { return Player.MaxHP; } }
	public bool PlayerIsDanger { get { return Player.IsDangerMode; } }
    public bool CanUseInvert { get { return Level >= 6; } }
    public int WaitCount { get; private set; }

	PlayerCommand CurrentCommand;
	Player Player;
    float resultRemainTime;
	int resultGainSP;
    readonly float DefaultResultTime = 0.4f;

	// Use this for initialization
	void Start () {
		GameContext.PlayerConductor = this;
		Player = GameObject.Find( "Main Camera" ).GetComponent<Player>();
        SetLevelParams();
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void CheckResult( int sp )
	{
		resultGainSP = sp;
		SPPanel.ShowSPPanel();
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
			print(GameContext.FieldConductor.RState);
			if( GameContext.FieldConductor.RState == ResultState.StarPoint )
			{
				RemainSP += resultGainSP;
				TotalSP += resultGainSP;
				SPPanel.UpdateSP();
				TextWindow.ChangeMessage(MessageCategory.Result, resultGainSP + "SPを　てにいれた！");
				SEPlayer.Play("newCommand");
				GameContext.FieldConductor.MoveNextResult();
			}
			else if( GameContext.FieldConductor.RState == ResultState.Command )
			{
				PlayerCommand acquiredCommand = commandGraph.CheckAcquireCommand(Level);
				if( acquiredCommand != null )
				{
					TextWindow.ChangeMessage(MessageCategory.AcquireCommand, acquiredCommand.name + "が　習得可能になった！");
					acquiredCommand.Acquire();
					commandGraph.ShowAcquireCommand(acquiredCommand);
					SEPlayer.Play("newCommand");
				}
				else
				{
					GameContext.FieldConductor.MoveNextResult();
				}
			}
			else
			{
				GameContext.FieldConductor.MoveNextResult();
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
            TextWindow.ChangeMessage( MessageCategory.Result, "オクスは　レベル" + Level + "に　あがった！" );
            resultRemainTime = DefaultResultTime;
            SEPlayer.Play( "levelUp" );
        }
    }

    public void CheckAcquireCommands()
    {
        PlayerCommand acquiredCommand = commandGraph.CheckAcquireCommand( Level );
        while( acquiredCommand != null )
        {
            acquiredCommand.Acquire();
            acquiredCommand = commandGraph.CheckAcquireCommand( Level );
        }
        PlayerCommand forgetCommand = commandGraph.CheckForgetCommand( Level );
        while( forgetCommand != null )
        {
            forgetCommand.Forget();
            forgetCommand = commandGraph.CheckAcquireCommand( Level );
        }
    }

	public void CheckCommand()
    {
        commandGraph.CheckCommand();
        CurrentCommand = commandGraph.CurrentCommand;
        Player.TurnInit( CurrentCommand.currentData );
		TextWindow.ChangeMessage(MessageCategory.PlayerCommand, CurrentCommand.currentData.DescribeText);
        WaitCount = 0;
	}
    public void CheckWaitCommand()
    {
        //commandGraph.CheckCommand();
        CurrentCommand = null;
        Player.DefaultInit();
		if( WaitCount == 0 )
		{
			TextWindow.ChangeMessage(MessageCategory.PlayerWait, "オクスは　つぎの　いってを　かんがえている");
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
        Player.OnBattleStart();
        commandGraph.OnBattleStart();
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
