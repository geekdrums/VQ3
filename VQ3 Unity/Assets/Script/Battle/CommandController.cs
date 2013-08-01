using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandController : MonoBehaviour {

    public Command[] Commands;
    Strategy[] Strategies;
	Strategy CurrentStrategy;

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
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void OnBarStarted( int CurrentIndex )
	{
		ECommand Command = CurrentStrategy.DefaultCommands[CurrentIndex%4];//TEMP!!!
        Command NewCommand = (Command)Instantiate( Commands[(int)Command], new Vector3(), Commands[(int)Command].transform.rotation );
        GameContext.BattleConductor.ExecCommand( NewCommand );
	}

	public void OnPlayerLose()
	{
	}
}