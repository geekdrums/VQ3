using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandManager : MonoBehaviour {

	Strategy[] Strategies;
	Command[] Commands;
	Strategy CurrentStrategy;

	// Use this for initialization
	void Start()
	{
		GameContext.CommandManager = this;

		Strategies = new Strategy[(int)EStrategy.Count];
		Strategies[(int)EStrategy.Default] = new Strategy( ECommand.Attack );
		Strategies[(int)EStrategy.Magic] = new Strategy( ECommand.Magic );
		Strategies[(int)EStrategy.Cure] = new Strategy( ECommand.Cure );

		Commands = new Command[(int)ECommand.Count];
		Commands[(int)ECommand.Attack] = new Command( new AttackModule( 1 ) );
		Commands[(int)ECommand.Magic] = new Command( new MagicModule( 1 ) );
		Commands[(int)ECommand.Cure] = new Command( new HealModule( 1 ) );

		CurrentStrategy = Strategies[(int)EStrategy.Default];
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void OnBarStarted( int CurrentIndex )
	{
		ECommand Command = CurrentStrategy.DefaultCommands[CurrentIndex];
		GameContext.BattleConductor.ExecCommand( Commands[(int)Command] );
	}

	public void OnPlayerLose()
	{
	}
}