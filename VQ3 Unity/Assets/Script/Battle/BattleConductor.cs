using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleConductor : MonoBehaviour {

	List<Command> Commands;

	// Use this for initialization
	void Start ()
	{
		GameContext.BattleConductor = this;
		Commands = new List<Command>();
	}
	
	// Update is called once per frame
	void Update () {
		if ( Music.IsJustChangedWhen( (Timing t)=>t.barUnit==0 ) )
		{
            OnBarStarted(Music.Just.bar);
		}
	}

	void OnBarStarted( int CurrentIndex )
	{
		GameContext.CommandManager.OnBarStarted( CurrentIndex );
		GameContext.PlayerConductor.OnBarStarted( CurrentIndex );

		//TEMP!!
		foreach ( Command cmd in Commands )
		{
			GameContext.EnemyConductor.ReceiveAction( cmd.GetCurrentAction(), cmd.isPlayerAction );
			GameContext.PlayerConductor.ReceiveAction( cmd.GetCurrentAction(), cmd.isPlayerAction );
		}
		Commands.Clear();
	}

	public void ExecCommand( Command NewCommand )
	{
		Commands.Add( NewCommand );
	}


	public void OnPlayerWin()
	{
	}
	public void OnPlayerLose()
	{
	}
}
