using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerConductor : MonoBehaviour {

	public GameObject MainCamera;
	Player Player;

    public CommandGraph commandGraph;
    public int Level = 1;
    public int NumQuarter { get; private set; }

    public List<int> HPLevelList;
    public List<int> QuarterLevelList;
    public List<int> AttackLevelList;
    public List<int> DefendLevelList;
    public List<int> MagicLevelList;
    public List<int> MagicDefendLevelList;
	
    Strategy[] Strategies;

	EStrategy NextStrategy;
	EStrategy CurrentStrategy;
    Command NextCommand;
    Command CurrentCommand;

    string NextBlockName { get { return NextCommand.GetBlockName(); } }
    bool CanUseBreak { get { return Level >= 8; } }

	int RemainBreakTime;
	Timing AllowInputTime = new Timing( 3, 3, 2 );

	// Use this for initialization
	void Start () {
		GameContext.PlayerConductor = this;
		Player = MainCamera.GetComponent<Player>();
		InitializeCommand();
        OnLevelUp();
	}

	void InitializeCommand()
	{
        Strategies = commandGraph.StrategyNodes;
		NextStrategy = EStrategy.Attack;
		CurrentStrategy = NextStrategy;
		NextCommand = Strategies[(int)CurrentStrategy].Commands[0];
	}
	
	// Update is called once per frame
	void Update()
	{
		if ( Music.Just < AllowInputTime )
		{
			if ( GameContext.VoxonSystem.state == VoxonSystem.VoxonState.ShowBreak )
			{
				if ( Music.IsJustChangedAt( 3 ) )
				{
					NextStrategy = EStrategy.Break;
					NextCommand = Strategies[(int)NextStrategy].Commands[0];
				}
			}
			else if ( GameContext.VoxonSystem.state == VoxonSystem.VoxonState.Break )
			{
				if ( RemainBreakTime == 1 )
				{
					UpdateInput();
					if ( Music.IsJustChangedAt( 3, 2 ) )
					{
						GameContext.VoxonSystem.SetState( VoxonSystem.VoxonState.HideBreak );
					}
				}
			}
			else
			{
				UpdateInput();
			}
        }
		if ( Music.IsJustChangedAt( AllowInputTime.bar, AllowInputTime.beat, AllowInputTime.unit ) )
        {
			SetNextBlock();
		}
	}

	void UpdateInput()
	{
		if ( Input.GetKeyDown( KeyCode.A ) )
		{
			NextStrategy = EStrategy.Attack;
			NextCommand = Strategies[(int)NextStrategy].Commands[0];
		}
		else if ( Input.GetKeyDown( KeyCode.P ) )
		{
            NextStrategy = EStrategy.Pilgrim;
            NextCommand = Strategies[(int)NextStrategy].Commands[1];
        }
        else if( Input.GetKeyDown( KeyCode.D ) )
        {
            NextStrategy = EStrategy.Attack;
            NextCommand = Strategies[(int)NextStrategy].Commands[1];
        }
		else if ( Input.GetKeyDown( KeyCode.G ) )
		{
			NextStrategy = EStrategy.Attack;
            NextCommand = Strategies[(int)NextStrategy].Commands[2];
		}
		else if ( Input.GetKeyDown( KeyCode.M ) )
		{
			NextStrategy = EStrategy.Magic;
            NextCommand = Strategies[(int)NextStrategy].Commands[0];
		}
		else if ( Input.GetKeyDown( KeyCode.H ) )
		{
			NextStrategy = EStrategy.Magic;
            NextCommand = Strategies[(int)NextStrategy].Commands[1];
		}
		else if ( Input.GetKeyDown( KeyCode.C ) )
		{
			NextStrategy = EStrategy.Magic;
            NextCommand = Strategies[(int)NextStrategy].Commands[2];
		}
		else if ( Input.GetKeyDown( KeyCode.F ) )
		{
			NextStrategy = EStrategy.Pilgrim;
            NextCommand = Strategies[(int)NextStrategy].Commands[0];
        }
	}

	void SetNextBlock()
	{
        if( !NextCommand.IsUsable() ) return;//TEMP
		if ( Music.GetNextBlockName() == "endro" )
		{
			return;
		}

		if ( CurrentStrategy == EStrategy.Break )
		{
			--RemainBreakTime;
			if ( RemainBreakTime == 0 )
			{
				if ( NextStrategy == EStrategy.Break )
				{
					NextStrategy = EStrategy.Magic;
					NextCommand = Strategies[(int)NextStrategy].Commands[0];
				}
                Music.SetNextBlock( NextBlockName );
            }
		}
		else
		{
            bool willShowBreak = false;
            if( CanUseBreak )
            {
                willShowBreak = GameContext.VoxonSystem.DetermineWillShowBreak( NextCommand.GetWillGainVoxon() );
                if( willShowBreak )
                {
                    RemainBreakTime = 2;
                }
            }
            Music.SetNextBlock( NextBlockName + (willShowBreak ? "Trans" : "") );
		}
	}

    void OnLevelUp()
    {
        NumQuarter = QuarterLevelList[Level-1];
        Player.HitPoint         = HPLevelList[Level-1];
        Player.BasePower        = AttackLevelList[Level-1];
        Player.BaseDefend       = DefendLevelList[Level-1];
        Player.BaseMagic        = MagicLevelList[Level-1];
        Player.BaseMagicDefend  = MagicDefendLevelList[Level-1];
    }

	VoxonSystem.VoxonState GetDesiredVoxonState()
	{
        if( !CanUseBreak ) return VoxonSystem.VoxonState.Hide;
		if ( NextStrategy == EStrategy.Magic )
		{
			if ( GameContext.VoxonSystem.state != VoxonSystem.VoxonState.ShowBreak )
			{
				return VoxonSystem.VoxonState.Show;
			}
			else
			{
				return VoxonSystem.VoxonState.ShowBreak;
			}
		}
		else if ( NextStrategy == EStrategy.Attack || NextStrategy == EStrategy.Pilgrim )
		{
			return VoxonSystem.VoxonState.Hide;
        }
		else// if ( NextStrategy == EStrategy.Break )
		{
			return VoxonSystem.VoxonState.Break;
		}
	}

	public void CheckCommand()
    {
        Player.SkillInit();
        if( !NextCommand.IsUsable() ) return;//TEMP
		if ( GameContext.VoxonSystem.state != GetDesiredVoxonState() )
		{
			GameContext.VoxonSystem.SetState( GetDesiredVoxonState() );
		}

		CurrentStrategy = NextStrategy;
		CurrentCommand = NextCommand;
        commandGraph.Select( CurrentCommand );
	}
    public void CheckSkill()
    {
        if( Music.Just.bar >= NumQuarter )
        {
            if( Music.IsJustChangedBar() )
            {
                Player.SkillInit();
            }
        }
        else
        {
            GameObject playerSkill = CurrentCommand.GetCurrentSkill();
            if( playerSkill != null )
            {
                Skill objSkill = (Instantiate( playerSkill ) as GameObject).GetComponent<Skill>();
                objSkill.SetOwner( Player );
                GameContext.BattleConductor.ExecSkill( objSkill );
            }
        }
    }

	public void OnBattleStarted()
	{
		InitializeCommand();
        Player.SkillInit();
        Music.SetNextBlock( NextBlockName );
        Music.SetAisac( "IsTransition", 0 );
        Music.SetAisac( "TrackVolume1", 1 );
        Music.SetAisac( "TrackVolume2", 1 );
        commandGraph.OnBattleStart();
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
}
