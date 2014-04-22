using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerConductor : MonoBehaviour {
    public CommandGraph commandGraph;
    public QuarterRing quarterRing;
    public Color MoonGetColor;
    public CounterSprite LevelCounter;
    public int Level = 1;

    public List<int> HPLevelList;
    public List<int> QuarterLevelList;
    public List<int> AttackLevelList;
    public List<int> MagicLevelList;
	
    PlayerCommand CurrentCommand;

    Player Player;
    public int NumQuarter { get; private set; }
    public int PlayerHP { get { return Player.HitPoint; } }
    public int PlayerMaxHP { get { return Player.MaxHP; } }
    public bool CanUseInvert { get { return Level >= 8; } }
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
        if( GameContext.FieldConductor.RState == ResultState.Moon1 || GameContext.FieldConductor.RState == ResultState.Moon2 )
        {
            float t = (Mathf.Abs( resultRemainTime ) % 1.0f) / 0.5f;
            GameContext.VoxSystem.SetBlinkMoonColor( Color.Lerp( MoonGetColor, MoonGetColor * 0.3f, (t > 1 ? 2 - t : t) ) );
        }
        if( resultRemainTime <= 0 && Input.GetMouseButtonUp( 0 ) && commandGraph.CurrentButton != VoxButton.None )
        {
            TextWindow.SetNextCursor( false );
            resultRemainTime = DefaultResultTime;
            bool isProceeded = false;
            while( !isProceeded )
            {
                print( GameContext.FieldConductor.RState );
                isProceeded = true;
                if( GameContext.FieldConductor.RState == ResultState.Command )
                {
                    PlayerCommand acquiredCommand = commandGraph.CheckAcquireCommand( Level );
                    if( acquiredCommand != null )//&& acquiredCommand != commandGraph.InvertStrategy.Commands[0] )
                    {
                        TextWindow.ChangeMessage( acquiredCommand.AcquireTexts );
                        acquiredCommand.Acquire();
                        commandGraph.Select( acquiredCommand );
                        SEPlayer.Play( "newCommand" );
                        break;
                    }
                    else
                    {
                        isProceeded = false;
                        GameContext.FieldConductor.MoveNextResult();
                    }
                }
                else
                {
                    GameContext.FieldConductor.MoveNextResult();
                }

                quarterRing.SetBlinkTime( 0.0f );
                switch( GameContext.FieldConductor.RState )
                {
                case ResultState.Status2:
                    TextWindow.ChangeMessage( "こうげき：  " + Player.BasePower + " ( +" + (AttackLevelList[Level - 1] - AttackLevelList[Level - 2]) + " )" );
                    TextWindow.AddMessage( "まほう：  " + Player.BaseMagic + " ( +" + (MagicLevelList[Level - 1] - MagicLevelList[Level - 2]) + " )" );
                    break;
                case ResultState.Quarter:
                    if( QuarterLevelList[Level - 1] > QuarterLevelList[Level - 2] )
                    {
                        TextWindow.ChangeMessage( "オクスは　" + NumQuarter + "つめの　クオーターを　てにいれた！" );
                        if( NumQuarter < 4 )
                        {
                            TextWindow.AddMessage( "4ぶんの" + NumQuarter + "のちからが　ときはなたれた。" );
                            TextWindow.AddMessage( "あと" + (4 - NumQuarter) + "つ！" );
                        }
                        else
                        {
                            TextWindow.AddMessage( "しかしまだ　かくされたちからが　あるようだ。" );
                        }
                        quarterRing.SetBlinkTime( 10.0f );
                        resultRemainTime += resultRemainTime;
                        SEPlayer.Play( "quarter" + NumQuarter );
                    }
                    else
                    {
                        isProceeded = false;
                    }
                    break;
                case ResultState.Command:
                    isProceeded = false;
                    break;
                case ResultState.Moon1:
                    if( CanUseInvert )
                    {
                        TextWindow.ChangeMessage( "かくされた　つきのちからを　てにいれた。" );
                        //commandGraph.InvertStrategy.Commands[0].Acquire();
                        //commandGraph.Select( commandGraph.InvertStrategy.Commands[0] );
                        isProceeded = true;
                        resultRemainTime += resultRemainTime * 3;
                        GameContext.VoxSystem.AddVPVT( (int)VoxSystem.InvertVP, 0 );
                        SEPlayer.Play( "moon" );
                    }
                    else
                    {
                        isProceeded = false;
                    }
                    break;
                case ResultState.Moon2:
                    if( CanUseInvert )
                    {
                        int startIndex = "<color=red>まほう</color>".Length;
                        TextWindow.AddMessage( new GUIMessage( "<color=red>まほう</color>により　つきがみちたとき、", null, null, startIndex ) );
                        TextWindow.AddMessage( "てんちの　すべてを　みかたに　できるだろう。" );
                        resultRemainTime += resultRemainTime * 2;
                    }
                    else
                    {
                        isProceeded = false;
                    }
                    break;
                default://ResultState.End
                    break;
                }
            }

        }
    }

    void SetLevelParams()
    {
        NumQuarter = QuarterLevelList[Level - 1];
        Player.HitPoint = HPLevelList[Level - 1];
        Player.BasePower = AttackLevelList[Level - 1];
        Player.BaseMagic = MagicLevelList[Level - 1];
        Player.Initialize();
        LevelCounter.count = Level;
    }

    public void OnLevelUp()
    {
        SetLevelParams();
        if( Level > 1 )
        {
            TextWindow.ChangeMessage( "オクスは　レベル" + Level + "に　あがった！" );
            TextWindow.AddMessage( "さいだいHP：" + Player.HitPoint + " ( +" + (HPLevelList[Level - 1] - HPLevelList[Level - 2]) + " )" );
            resultRemainTime = DefaultResultTime;
            SEPlayer.Play( "levelUp" );
        }
    }

	public void CheckCommand()
    {
        commandGraph.CheckCommand();
        CurrentCommand = commandGraph.CurrentCommand;
        Player.TurnInit( CurrentCommand );
        TextWindow.ChangeMessage( CurrentCommand.DescribeTexts );
        WaitCount = 0;
	}
    public void CheckWaitCommand()
    {
        commandGraph.CheckCommand();
        CurrentCommand = null;
        Player.DefaultInit();
        TextWindow.ChangeMessage( "オクスは　じっと　まっている" );
        ++WaitCount;
	}
    
    public void CheckSkill()
    {
        if( Music.Just.bar >= NumQuarter || CurrentCommand  == null ) return;
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
