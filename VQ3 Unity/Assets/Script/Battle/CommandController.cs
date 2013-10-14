using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandController : MonoBehaviour {

    public Command[] Commands;
    Strategy[] Strategies;
	Strategy CurrentStrategy;

	ECommand[] NextCommandList = new ECommand[4];
	ECommand[] CurrentCommandList = new ECommand[4];

	// Use this for initialization
	void Start()
	{
		GameContext.CommandController = this;

		Strategies = new Strategy[(int)EStrategy.Count];
		Strategies[(int)EStrategy.Attack] = new Strategy( ECommand.Attack );
		Strategies[(int)EStrategy.Magic] = new Strategy( ECommand.Magic );
		Strategies[(int)EStrategy.Cure] = new Strategy( ECommand.Cure );

        //Commands = new Command[(int)ECommand.Count];
        //Commands[(int)ECommand.Attack] = new Command( new AttackModule( 1 ) );
        //Commands[(int)ECommand.Magic] = new Command( new MagicModule( 1 ) );
        //Commands[(int)ECommand.Cure] = new Command( new HealModule( 1 ) );

		CurrentStrategy = Strategies[(int)EStrategy.Attack];

		NextCommandList[0] = ECommand.Attack;
		NextCommandList[1] = ECommand.Attack;
		NextCommandList[2] = ECommand.Attack;
		NextCommandList[3] = ECommand.Attack;
	}

	// Update is called once per frame
	void Update()
	{
		if ( Input.GetKeyDown( KeyCode.A ) )
		{
			NextCommandList[0] = ECommand.Attack;
			NextCommandList[1] = ECommand.Attack;
			NextCommandList[2] = ECommand.Attack;
			NextCommandList[3] = ECommand.Attack;
		}
		else if ( Input.GetKeyDown( KeyCode.M ) )
		{
			NextCommandList[0] = ECommand.Magic;
			NextCommandList[1] = ECommand.Magic;
			NextCommandList[2] = ECommand.Magic;
			NextCommandList[3] = ECommand.Magic;
		}
		else if ( Input.GetKeyDown( KeyCode.B ) )
		{
			NextCommandList[0] = ECommand.Magic;
			NextCommandList[1] = ECommand.Magic;
			NextCommandList[2] = ECommand.Magic;
			NextCommandList[3] = ECommand.Break;
		}
	}

	public void OnBarStarted( int CurrentIndex )
	{
		if ( CurrentIndex == 0 )
		{
			CurrentCommandList[0] = NextCommandList[0];
			CurrentCommandList[1] = NextCommandList[1];
			CurrentCommandList[2] = NextCommandList[2];
			CurrentCommandList[3] = NextCommandList[3];
		}
		ECommand Command = CurrentCommandList[CurrentIndex];//CurrentStrategy.DefaultCommands[CurrentIndex%4];//TEMP!!!
		Command NewCommand = (Command)Instantiate( Commands[(int)Command], new Vector3(), Commands[(int)Command].transform.rotation );
		GameContext.BattleConductor.ExecCommand( NewCommand );
	}

	public void OnPlayerLose()
	{
	}
}