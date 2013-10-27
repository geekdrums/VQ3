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

	// Use this for initialization
	void Start () {
		GameContext.PlayerConductor = this;
		Player = MainCamera.GetComponent<Player>();
		NextCommandList = Strategies[(int)CurrentStrategy].CommandList[0];
		NextBlockName = "aaaa";
	}
	
	// Update is called once per frame
	void Update () {
        if( Music.Just.totalUnit < Music.mtBar * 4 - 1 )
        {
            if( Input.GetKeyDown( KeyCode.A ) )
            {
				NextStrategy = EStrategy.Attack;
				NextCommandList = Strategies[(int)NextStrategy].CommandList[0];
                NextBlockName = "aaaa";
            }
            else if( Input.GetKeyDown( KeyCode.P ) )
			{
				NextStrategy = EStrategy.Attack;
				NextCommandList = Strategies[(int)NextStrategy].CommandList[1];
                NextBlockName = "ppaa";
            }
            else if( Input.GetKeyDown( KeyCode.G ) )
			{
				NextStrategy = EStrategy.Attack;
				NextCommandList = Strategies[(int)NextStrategy].CommandList[2];
                NextBlockName = "gggg";
            }
            else if( Input.GetKeyDown( KeyCode.D ) )
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
        if( Music.IsJustChangedAt( 3, 3, 3 ) && Music.GetNextBlockName() != "GotoEndro" )
        {
			if ( CurrentStrategy != NextStrategy )
			{
				Music.SetNextBlock( "Goto" + NextStrategy.ToString() );
			}
			else
			{
				Music.SetNextBlock( NextBlockName );
			}
        }

		if ( Music.IsJustChangedAt(0) )
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
			}
		}
	}

	public void On4BarStarted()
	{
		CurrentStrategy = NextStrategy;
		CurrentCommandList[0] = NextCommandList[0];
		CurrentCommandList[1] = NextCommandList[1];
		CurrentCommandList[2] = NextCommandList[2];
		CurrentCommandList[3] = NextCommandList[3];
	}

	public void OnBarStarted( int CurrentIndex )
	{
		Player.OnBarStarted( CurrentIndex );
		ECommand Command = CurrentCommandList[CurrentIndex%4];
		Command NewCommand = (Command)Instantiate( Commands[(int)Command], new Vector3(), Commands[(int)Command].transform.rotation );
		NewCommand.SetOwner( Player );
		GameContext.BattleConductor.ExecCommand( NewCommand );
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
