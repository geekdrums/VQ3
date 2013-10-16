using UnityEngine;
using System.Collections;

public class PlayerConductor : MonoBehaviour {

	public GameObject MainCamera;
	Player Player;

	public Command[] Commands;
	Strategy[] Strategies;
	Strategy CurrentStrategy;

	ECommand[] NextCommandList = new ECommand[4];
	ECommand[] CurrentCommandList = new ECommand[4];

	// Use this for initialization
	void Start () {
		GameContext.PlayerConductor = this;
		Player = MainCamera.GetComponent<Player>();

		Strategies = new Strategy[(int)EStrategy.Count];
		Strategies[(int)EStrategy.Attack] = new Strategy( ECommand.Attack );
		Strategies[(int)EStrategy.Magic] = new Strategy( ECommand.Magic );

		CurrentStrategy = Strategies[(int)EStrategy.Attack];

		NextCommandList[0] = ECommand.Attack;
		NextCommandList[1] = ECommand.Attack;
		NextCommandList[2] = ECommand.Attack;
		NextCommandList[3] = ECommand.Attack;
	}
	
	// Update is called once per frame
	void Update () {
		if( Input.GetKeyDown( KeyCode.A ) )
		{
			NextCommandList[0] = ECommand.Attack;
			NextCommandList[1] = ECommand.Attack;
			NextCommandList[2] = ECommand.Attack;
			NextCommandList[3] = ECommand.Attack;
			Music.SetNextBlock( "aaaa" );
		}
		else if ( Input.GetKeyDown( KeyCode.P ) )
		{
			NextCommandList[0] = ECommand.Power;
			NextCommandList[1] = ECommand.Power;
			NextCommandList[2] = ECommand.Attack;
			NextCommandList[3] = ECommand.Attack;
			Music.SetNextBlock( "ppaa" );
		}
		else if ( Input.GetKeyDown( KeyCode.G ) )
		{
			NextCommandList[0] = ECommand.Guard;
			NextCommandList[1] = ECommand.Guard;
			NextCommandList[2] = ECommand.Guard;
			NextCommandList[3] = ECommand.Guard;
			Music.SetNextBlock( "gggg" );
		}
		else if ( Input.GetKeyDown( KeyCode.D ) )
		{
			NextCommandList[0] = ECommand.Guard;
			NextCommandList[1] = ECommand.Guard;
			NextCommandList[2] = ECommand.Attack;
			NextCommandList[3] = ECommand.Attack;
			Music.SetNextBlock( "ggaa" );
		}
	}

	public void OnBarStarted( int CurrentIndex )
	{
		Player.OnBarStarted( CurrentIndex );

		if ( CurrentIndex == 0 )
		{
			CurrentCommandList[0] = NextCommandList[0];
			CurrentCommandList[1] = NextCommandList[1];
			CurrentCommandList[2] = NextCommandList[2];
			CurrentCommandList[3] = NextCommandList[3];
		}
		ECommand Command = CurrentCommandList[CurrentIndex];
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
