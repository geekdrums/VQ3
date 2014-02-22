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
    public int PlayerMaxHP { get { return Player.HitPoint; } }

	// Use this for initialization
	void Start () {
		GameContext.PlayerConductor = this;
		Player = GameObject.Find( "Main Camera" ).GetComponent<Player>();
        OnLevelUp();
	}

	// Update is called once per frame
	void Update()
	{
	}

    public void OnLevelUp()
    {
        NumQuarter = QuarterLevelList[Level-1];
        Player.HitPoint         = HPLevelList[Level-1];
        Player.BasePower        = AttackLevelList[Level-1];
        Player.BaseDefend       = DefendLevelList[Level-1];
        Player.BaseMagic        = MagicLevelList[Level-1];
        Player.BaseMagicDefend  = MagicDefendLevelList[Level-1];
        Player.Initialize();
        Player.TurnInit();
    }

	public void CheckCommand()
    {
        commandGraph.CheckCommand();
        CurrentCommand = commandGraph.CurrentCommand;
        Player.TurnInit();
        TextWindow.AddMessage( "オクスは" + CurrentCommand.name + "をはなった！" );
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
}
