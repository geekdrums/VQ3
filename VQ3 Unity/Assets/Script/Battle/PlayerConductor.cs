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

	public string NextBlockName { get; protected set; }
	public string NextStrategyName { get { return NextStrategy.ToString(); } }

	int RemainBreakTime;
	Timing AllowInputTime = new Timing( 3, 3, 3 );

	// Use this for initialization
	void Start () {
		GameContext.PlayerConductor = this;
		Player = MainCamera.GetComponent<Player>();
		InitializeCommand();
	}

	void InitializeCommand()
	{
		NextStrategy = EStrategy.Magic;
		CurrentStrategy = NextStrategy;
		NextCommandList = Strategies[(int)CurrentStrategy].CommandList[0];
		NextBlockName = "mmmm";
	}
	
	// Update is called once per frame
	void Update()
	{
		if ( Music.IsJustChangedAt( 0 ) )
		{
			PlayNextMusic();
		}
		if ( Music.Just < AllowInputTime )
		{
			if ( GameContext.BattleConductor.state == BattleConductor.VoxonState.ShowBreak )
			{
				if ( Music.IsJustChangedAt( 3 ) )
				{
					NextStrategy = EStrategy.Break;
					NextCommandList = Strategies[(int)NextStrategy].CommandList[0];
					NextBlockName = "bbbb";
				}
			}
			else if ( GameContext.BattleConductor.state == BattleConductor.VoxonState.Break )
			{
				if ( RemainBreakTime == 1 )
				{
					UpdateInput();
					if ( Music.IsJustChangedAt( 3, 2 ) )
					{
						GameContext.BattleConductor.SetState( BattleConductor.VoxonState.HideBreak );
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

	void PlayNextMusic()
	{
		if ( Music.GetCurrentBlockName() == "GotoMagic" )
		{
			Music.Play( "Magic", NextBlockName );
		}
		else if ( Music.GetCurrentBlockName() == "GotoAttack" )
		{
			Music.Play( "Attack", NextBlockName );
		}
		else if ( Music.GetCurrentBlockName() == "GotoBreak" )
		{
			Music.Play( "Break", NextBlockName );
			RemainBreakTime = 2;
		}
	}

	void UpdateInput()
	{
		if ( Input.GetKeyDown( KeyCode.A ) )
		{
			NextStrategy = EStrategy.Attack;
			NextCommandList = Strategies[(int)NextStrategy].CommandList[0];
			NextBlockName = "aaaa";
		}
		else if ( Input.GetKeyDown( KeyCode.P ) )
		{
			NextStrategy = EStrategy.Attack;
			NextCommandList = Strategies[(int)NextStrategy].CommandList[1];
			NextBlockName = "ppaa";
		}
		else if ( Input.GetKeyDown( KeyCode.G ) )
		{
			NextStrategy = EStrategy.Attack;
			NextCommandList = Strategies[(int)NextStrategy].CommandList[2];
			NextBlockName = "gggg";
		}
		else if ( Input.GetKeyDown( KeyCode.D ) )
		{
			NextStrategy = EStrategy.Attack;
			NextCommandList = Strategies[(int)NextStrategy].CommandList[3];
			NextBlockName = "ggaa";
		}
		else if ( Input.GetKeyDown( KeyCode.M ) )
		{
			NextStrategy = EStrategy.Magic;
			NextCommandList = Strategies[(int)NextStrategy].CommandList[0];
			NextBlockName = "mmmm";
		}
		else if ( Input.GetKeyDown( KeyCode.H ) )
		{
			NextStrategy = EStrategy.Magic;
			NextCommandList = Strategies[(int)NextStrategy].CommandList[1];
			NextBlockName = "hhhh";
		}
		else if ( Input.GetKeyDown( KeyCode.C ) )
		{
			NextStrategy = EStrategy.Magic;
			NextCommandList = Strategies[(int)NextStrategy].CommandList[2];
			NextBlockName = "hhmm";
		}
		else if ( Input.GetKeyDown( KeyCode.F ) )
		{
			NextStrategy = EStrategy.Magic;
			NextCommandList = Strategies[(int)NextStrategy].CommandList[3];
			NextBlockName = "ffff";
		}
	}

	void SetNextBlock()
	{
		CurrentCommandList[0] = ECommand.Wait;
		if ( Music.GetNextBlockName() == "GotoEndro" )
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
					NextBlockName = "mmmm";
				}
				Music.SetNextBlock( "Goto" + NextStrategy.ToString() );
			}
		}
		else if ( CurrentStrategy != NextStrategy )
		{
			Music.SetNextBlock( "Goto" + NextStrategy.ToString() );
		}
		else // CurrentStrategy == NextStrategy != EStrategy.Break
		{
			bool willShowBreak = GameContext.BattleConductor.DetermineWillShowBreak( GetWillGainVoxon() );
			Music.SetNextBlock( NextBlockName + ( willShowBreak ? "Trans" : "" ) );
		}
	}

	int GetWillGainVoxon()
	{
		int sum = 0;
		for ( int i=0; i<3; ++i )
		{
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

	BattleConductor.VoxonState GetDesiredVoxonState()
	{
		if ( NextStrategy == EStrategy.Magic )
		{
			if ( GameContext.BattleConductor.state != BattleConductor.VoxonState.ShowBreak )
			{
				return BattleConductor.VoxonState.Show;
			}
			else
			{
				return BattleConductor.VoxonState.ShowBreak;
			}
		}
		else if ( NextStrategy == EStrategy.Attack )
		{
			return BattleConductor.VoxonState.Hide;
		}
		else// if ( NextStrategy == EStrategy.Break )
		{
			return BattleConductor.VoxonState.Break;
		}
	}

	public void On4BarStarted()
	{
		if ( GameContext.BattleConductor.state != GetDesiredVoxonState() )
		{
			GameContext.BattleConductor.SetState( GetDesiredVoxonState() );
		}

		CurrentStrategy = NextStrategy;
		CurrentCommandList[0] = NextCommandList[0];
		CurrentCommandList[1] = NextCommandList[1];
		CurrentCommandList[2] = NextCommandList[2];
		if ( GameContext.BattleConductor.state == BattleConductor.VoxonState.ShowBreak )
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
