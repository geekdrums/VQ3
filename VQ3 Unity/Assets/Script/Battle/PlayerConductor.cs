using UnityEngine;
using System.Collections;

public class PlayerConductor : MonoBehaviour {

	public GameObject MainCamera;
	Player Player;

	public Command[] Commands;
	public Strategy[] Strategies;

	EStrategy NextStrategy;
	EStrategy CurrentStrategy;
	ECommand[] NextCommandList = new ECommand[4];
	ECommand[] CurrentCommandList = new ECommand[4];

    public string NextCommandsName
    {
        get
        {
            string res = "";
            for( int i = 0; i < NextCommandList.Length; ++i )
            {
                res += NextCommandList[i].ToString()[0];
            }
            return res;
        }
    }
	public string NextStrategyName { get { return NextStrategy.ToString(); } }

	int RemainBreakTime;
	Timing AllowInputTime = new Timing( 3, 3, 2 );

	// Use this for initialization
	void Start () {
		GameContext.PlayerConductor = this;
		Player = MainCamera.GetComponent<Player>();
		InitializeCommand();
	}

	void InitializeCommand()
	{
		NextStrategy = EStrategy.Attack;
		CurrentStrategy = NextStrategy;
		NextCommandList = Strategies[(int)CurrentStrategy].CommandList[0];
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
					NextCommandList = Strategies[(int)NextStrategy].CommandList[0];
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
			NextCommandList = Strategies[(int)NextStrategy].CommandList[0];
		}
		else if ( Input.GetKeyDown( KeyCode.P ) )
		{
			NextStrategy = EStrategy.Attack;
			NextCommandList = Strategies[(int)NextStrategy].CommandList[1];
		}
		else if ( Input.GetKeyDown( KeyCode.G ) )
		{
			NextStrategy = EStrategy.Attack;
			NextCommandList = Strategies[(int)NextStrategy].CommandList[2];
		}
		else if ( Input.GetKeyDown( KeyCode.D ) )
		{
			NextStrategy = EStrategy.Attack;
			NextCommandList = Strategies[(int)NextStrategy].CommandList[3];
		}
		else if ( Input.GetKeyDown( KeyCode.M ) )
		{
			NextStrategy = EStrategy.Magic;
			NextCommandList = Strategies[(int)NextStrategy].CommandList[0];
		}
		else if ( Input.GetKeyDown( KeyCode.H ) )
		{
			NextStrategy = EStrategy.Magic;
			NextCommandList = Strategies[(int)NextStrategy].CommandList[1];
		}
		else if ( Input.GetKeyDown( KeyCode.C ) )
		{
			NextStrategy = EStrategy.Magic;
			NextCommandList = Strategies[(int)NextStrategy].CommandList[2];
		}
		else if ( Input.GetKeyDown( KeyCode.F ) )
		{
			NextStrategy = EStrategy.Magic;
			NextCommandList = Strategies[(int)NextStrategy].CommandList[3];
        }
	}

	void SetNextBlock()
	{
		CurrentCommandList[0] = ECommand.Wait;
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
					NextCommandList = Strategies[(int)NextStrategy].CommandList[0];
				}
                Music.SetNextBlock( NextStrategyName + NextCommandsName );
            }
		}
		else
		{
			bool willShowBreak = GameContext.VoxonSystem.DetermineWillShowBreak( GetWillGainVoxon() );
            if( willShowBreak )
            {
                RemainBreakTime = 2;
            }
            Music.SetNextBlock( NextStrategyName + NextCommandsName + ( willShowBreak ? "Trans" : "" ) );
		}
	}

	int GetWillGainVoxon()
	{
		int sum = 0;
		for ( int i=0; i<3; ++i )
		{
            if( Commands[(int)NextCommandList[i]].Actions == null )
            {
                Commands[(int)NextCommandList[i]].Parse();
            }
			ActionSet[] actions = Commands[(int)NextCommandList[i]].Actions;
			foreach ( ActionSet a in actions )
			{
				if ( a.GetModule<MagicModule>() != null )
				{
					sum += a.GetModule<MagicModule>().VoxonEnergy;
				}
			}
		}
		return sum;
	}

	VoxonSystem.VoxonState GetDesiredVoxonState()
	{
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
		else if ( NextStrategy == EStrategy.Attack )
		{
			return VoxonSystem.VoxonState.Hide;
		}
		else// if ( NextStrategy == EStrategy.Break )
		{
			return VoxonSystem.VoxonState.Break;
		}
	}

	public void On4BarStarted()
	{
		if ( GameContext.VoxonSystem.state != GetDesiredVoxonState() )
		{
			GameContext.VoxonSystem.SetState( GetDesiredVoxonState() );
		}

		CurrentStrategy = NextStrategy;
		CurrentCommandList[0] = NextCommandList[0];
		CurrentCommandList[1] = NextCommandList[1];
		CurrentCommandList[2] = NextCommandList[2];
		if ( GameContext.VoxonSystem.state == VoxonSystem.VoxonState.ShowBreak )
		{
			CurrentCommandList[3] = ECommand.Wait;
		}
		else
		{
			CurrentCommandList[3] = NextCommandList[3];
		}
	}

	public void OnBarStarted( int CurrentIndex )
	{
		Player.OnBarStarted( CurrentIndex );
		ECommand Command = CurrentCommandList[CurrentIndex%4];
		Command NewCommand = (Command)Instantiate( Commands[(int)Command], new Vector3(), Commands[(int)Command].transform.rotation );
		NewCommand.SetOwner( Player );
		GameContext.BattleConductor.ExecCommand( NewCommand );
	}

	public void OnBattleStarted()
	{
		InitializeCommand();
        Music.SetNextBlock( NextStrategyName + NextCommandsName );
        Music.SetAisac( "IsTransition", 0 );
        Music.SetAisac( "TrackVolume1", 1 );
        Music.SetAisac( "TrackVolume2", 1 );
    }

	public bool ReceiveAction( ActionSet Action, Command command )
	{
		bool isSucceeded = false;
		AttackModule attack = Action.GetModule<AttackModule>();
        if( attack != null && !command.isPlayerAction )
		{
			Player.BeAttacked( attack, command );
			isSucceeded = true;
		}
		DefendModule defend = Action.GetModule<DefendModule>();
        if( defend != null && command.isPlayerAction )
		{
			Player.Defend( defend );
			isSucceeded = true;
		}
		HealModule heal = Action.GetModule<HealModule>();
        if( heal != null && command.isPlayerAction )
		{
			Player.Heal( heal );
			isSucceeded = true;
		}
		PowerModule power = Action.GetModule<PowerModule>();
		if ( power != null && command.isPlayerAction )
		{
			Player.PowerUp( power );
			isSucceeded = true;
		}
		return isSucceeded;
	}
}
