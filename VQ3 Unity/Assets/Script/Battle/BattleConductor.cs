using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleConductor : MonoBehaviour {

	List<KeyValuePair<Timing,Command>> Commands;

	// Use this for initialization
	void Start ()
	{
		GameContext.BattleConductor = this;
        Commands = new List<KeyValuePair<Timing, Command>>();
	}
	
	// Update is called once per frame
    void Update()
    {
        if (Music.IsJustChangedWhen((Timing t) => t.barUnit == 0))
        {
            OnBarStarted(Music.Just.bar);
        }

        foreach (KeyValuePair<Timing, Command> cmd in Commands)
        {
            GameContext.EnemyConductor.ReceiveAction(cmd.Value.GetCurrentAction(cmd.Key), cmd.Value.isPlayerAction);
            GameContext.PlayerConductor.ReceiveAction(cmd.Value.GetCurrentAction(cmd.Key), cmd.Value.isPlayerAction);
        }
        Commands.RemoveAll((KeyValuePair<Timing, Command> cmd) => cmd.Value.IsEnd(cmd.Key));
    }

	void OnBarStarted( int CurrentIndex )
	{
		GameContext.CommandManager.OnBarStarted( CurrentIndex );
		GameContext.PlayerConductor.OnBarStarted( CurrentIndex );
	}

	public void ExecCommand( Command NewCommand )
	{
		Commands.Add( new KeyValuePair<Timing, Command>( new Timing( Music.Just ), NewCommand ) );
	}


	public void OnPlayerWin()
	{
	}
	public void OnPlayerLose()
	{
	}
}
