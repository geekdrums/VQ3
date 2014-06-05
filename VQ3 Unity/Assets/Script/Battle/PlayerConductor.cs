using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerConductor : MonoBehaviour {
    public CommandGraph commandGraph;
    //public QuarterRing quarterRing;
    public Color MoonGetColor;
    //public CounterSprite LevelCounter;
    public int Level = 1;

    //public List<int> HPLevelList;
    //public List<int> QuarterLevelList;
    //public List<int> AttackLevelList;
    //public List<int> MagicLevelList;
	
    PlayerCommand CurrentCommand;

    Player Player;
    //public int NumQuarter { get; private set; }
    public int PlayerHP { get { return Player.HitPoint; } }
    public int PlayerMaxHP { get { return Player.MaxHP; } }
    public bool CanUseInvert { get { return Level >= 6; } }
    public int WaitCount { get; private set; }

    float resultRemainTime;
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

    public void UpdateResult()
    {
        resultRemainTime -= Time.deltaTime;
        if( resultRemainTime <= 0 )
        {
            TextWindow.SetNextCursor( true );
        }
        if( resultRemainTime <= 0 && Input.GetMouseButtonUp( 0 ) && commandGraph.CurrentButton != VoxButton.None )
        {
            TextWindow.SetNextCursor( false );
            resultRemainTime = DefaultResultTime;
            print( GameContext.FieldConductor.RState );
            if( GameContext.FieldConductor.RState == ResultState.Command )
            {
                PlayerCommand acquiredCommand = commandGraph.CheckAcquireCommand( Level );
                if( acquiredCommand != null )
                {
                    TextWindow.ChangeMessage( BattleMessageType.AcquireCommand, acquiredCommand.AcquireText );
                    acquiredCommand.Acquire();
                    commandGraph.Select( acquiredCommand );
                    SEPlayer.Play( "newCommand" );
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
        //NumQuarter = QuarterLevelList[Level - 1];
        Player.HitPoint = 500 + Level * 100; //HPLevelList[Level - 1];
        Player.BasePower = 50 + Level * 10; //AttackLevelList[Level - 1];
        Player.BaseMagic = 50 + Level * 10; //MagicLevelList[Level - 1];
        Player.Initialize();
        //LevelCounter.count = Level;
    }

    public void OnLevelUp()
    {
        SetLevelParams();
        if( Level > 1 )
        {
            TextWindow.ChangeMessage( BattleMessageType.Result, "オクスは　レベル" + Level + "に　あがった！" );
            resultRemainTime = DefaultResultTime;
            SEPlayer.Play( "levelUp" );
            //PlayerCommand acquiredCommand = commandGraph.CheckAcquireCommand( Level );
            //while( acquiredCommand != null )//&& acquiredCommand != commandGraph.InvertStrategy.Commands[0] )
            //{
            //    acquiredCommand.Acquire();
            //    acquiredCommand = commandGraph.CheckAcquireCommand( Level );
            //}
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
        Player.TurnInit( CurrentCommand );
        TextWindow.ChangeMessage( BattleMessageType.PlayerCommand, CurrentCommand.DescribeText );
        WaitCount = 0;
	}
    public void CheckWaitCommand()
    {
        commandGraph.CheckCommand();
        CurrentCommand = null;
        Player.DefaultInit();
        TextWindow.ChangeMessage( BattleMessageType.PlayerWait, "オクスは　じっと　まっている" );
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
    }

	public bool ReceiveAction( ActionSet Action, Skill skill )
	{
		bool isSucceeded = false;
		AttackModule attack = Action.GetModule<AttackModule>();
        if( attack != null )
		{
            if( skill.isPlayerSkill )
            {
                commandGraph.OnReactEvent( attack.isPhysic ? IconReactType.OnAttack : IconReactType.OnMagic );
            }
            else
            {
                Player.BeAttacked( attack, skill );
                GameContext.VoxSystem.AddVPVT( attack.VP, attack.VT );
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
		return isSucceeded;
	}

    public void UpdateHealHP()
    {
        Player.UpdateHealHP();
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
