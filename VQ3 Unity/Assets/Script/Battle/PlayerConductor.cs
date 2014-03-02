using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerConductor : MonoBehaviour {
    public CommandGraph commandGraph;
    public int Level = 1;

    public List<int> HPLevelList;
    public List<int> QuarterLevelList;
    public List<int> AttackLevelList;
    public List<int> DefendLevelList;
    public List<int> MagicLevelList;
    public List<int> MagicDefendLevelList;
	
    Command CurrentCommand;

    Player Player;
    public int NumQuarter { get; private set; }
    public int PlayerHP { get { return Player.HitPoint; } }
    public int PlayerMaxHP { get { return Player.MaxHP; } }
    public bool CanUseInvert { get { return Level >= 8; } }

    float resultTime;

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
        resultTime += Time.deltaTime;
        if( resultTime >= 0.4f && Input.GetMouseButtonUp( 0 ) && commandGraph.CurrentButton != VoxButton.None )
        {
            resultTime = 0;
            bool isProceeded = false;
            while( !isProceeded )
            {
                print( GameContext.FieldConductor.RState );
                isProceeded = true;
                if( GameContext.FieldConductor.RState == ResultState.Command )
                {
                    Command acquiredCommand = commandGraph.CheckAcquireCommand( Level );
                    if( acquiredCommand != null && acquiredCommand != commandGraph.InvertStrategy.Commands[0] )
                    {
                        TextWindow.ChangeMessage( acquiredCommand.AcquireTexts );
                        acquiredCommand.Acquire();
                        commandGraph.Select( acquiredCommand );
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

                switch( GameContext.FieldConductor.RState )
                {
                case ResultState.Status2:
                    TextWindow.ChangeMessage( "ぼうぎょ：" + Player.BaseDefend + " ( +" + (DefendLevelList[Level - 1] - DefendLevelList[Level - 2]) + " )" );
                    TextWindow.AddMessage( "まほう：  " + Player.BaseMagic + " ( +" + (MagicLevelList[Level - 1] - MagicLevelList[Level - 2]) + " )" );
                    TextWindow.AddMessage( "まぼう：  " + Player.BaseMagicDefend + " ( +" + (MagicDefendLevelList[Level - 1] - MagicDefendLevelList[Level - 2]) + " )" );
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
                        commandGraph.InvertStrategy.Commands[0].Acquire();
                        commandGraph.Select( commandGraph.InvertStrategy.Commands[0] );
                        isProceeded = true;
                    }
                    else
                    {
                        isProceeded = false;
                    }
                    break;
                case ResultState.Moon2:
                    if( CanUseInvert )
                    {
                        int startIndex = "<color=red>まほうにより</color>".Length;
                        TextWindow.AddMessage( new GUIMessage( "<color=red>まほうにより</color>　つきがみちたとき、", null, null, startIndex ) );
                        TextWindow.AddMessage( "てんちの　すべてを　みかたに　できるだろう。" );
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
        Player.BaseDefend = DefendLevelList[Level - 1];
        Player.BaseMagic = MagicLevelList[Level - 1];
        Player.BaseMagicDefend = MagicDefendLevelList[Level - 1];
        Player.Initialize();
        Player.TurnInit();
    }

    public void OnLevelUp()
    {
        SetLevelParams();
        if( Level > 1 )
        {
            TextWindow.ChangeMessage( "オクスは　レベル" + Level + "に　あがった！" );
            TextWindow.AddMessage( "さいだいHP：" + Player.HitPoint + " ( +" + (HPLevelList[Level - 1] - HPLevelList[Level - 2]) + " )" );
            TextWindow.AddMessage( "こうげき：  " + Player.BasePower + " ( +" + (AttackLevelList[Level - 1] - AttackLevelList[Level - 2]) + " )" );
            resultTime = 0;
        }
    }

	public void CheckCommand()
    {
        commandGraph.CheckCommand();
        CurrentCommand = commandGraph.CurrentCommand;
        Player.TurnInit();
        TextWindow.ChangeMessage( CurrentCommand.DescribeTexts );
	}
    public void CheckSkill()
    {
        if( Music.Just.bar >= NumQuarter ) return;
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
        Music.SetAisac( "IsTransition", 0 );
        Music.SetAisac( "TrackVolume1", 1 );
        Music.SetAisac( "TrackVolume2", 1 );
    }

	public bool ReceiveAction( ActionSet Action, Skill skill )
	{
		bool isSucceeded = false;
		AttackModule attack = Action.GetModule<AttackModule>();
        if( attack != null && !skill.isPlayerSkill )
		{
			Player.BeAttacked( attack, skill );
			isSucceeded = true;
        }
        MagicModule magic = Action.GetModule<MagicModule>();
        if( magic != null && !skill.isPlayerSkill )
        {
            Player.BeMagicAttacked( magic, skill );
            isSucceeded = true;
            GameContext.VoxSystem.AddVP( magic.VoxPoint );
        }
		DefendModule defend = Action.GetModule<DefendModule>();
        if( defend != null && skill.isPlayerSkill )
		{
			Player.Defend( defend );
			isSucceeded = true;
        }
        MagicDefendModule magicDefend = Action.GetModule<MagicDefendModule>();
        if( magicDefend != null && skill.isPlayerSkill )
        {
            Player.MagicDefend( magicDefend );
            isSucceeded = true;
        }
		HealModule heal = Action.GetModule<HealModule>();
        if( heal != null && skill.isPlayerSkill )
		{
			Player.Heal( heal );
			isSucceeded = true;
        }
		return isSucceeded;
	}


    public void OnPlayerWin()
    {
        Player.TurnInit();
    }
    public void OnPlayerLose()
    {
        Player.TurnInit();
    }
    public void OnContinue()
    {
        Player.HitPoint = Player.MaxHP;
        Player.TurnInit();
    }
}
