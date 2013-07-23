using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum ECommand
{
	Attack,
	Magic,
	Cure,
	Count
}

public class Command
{
	public bool isPlayerAction { get; private set; }
	protected ActionSet[] Actions;
	protected Rhythm ActionRhythm;

	public Command( Rhythm rhythm, bool isPlayer = true, params ActionSet[] inActions )
	{
		Actions = inActions;
		ActionRhythm = rhythm;
	}
	public Command( ActionSet act, bool isPlayer = true )
		: this( Rhythm.ONE_NOTE_RHYTHM, isPlayer, act ) { }
	public Command( bool isPlayer, params IActionModule[] Modules )
		: this( new ActionSet( Modules ), isPlayer ) { }
	public Command( IActionModule Module )
		: this( new ActionSet( Module ), true ) { }

	//TEMP!!!
	public ActionSet GetCurrentAction()
	{
		return Actions[0];
	}
}
