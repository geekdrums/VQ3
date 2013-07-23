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
		isPlayerAction = isPlayer;
	}
	public Command( ActionSet act, bool isPlayer = true )
		: this( Rhythm.ONE_NOTE_RHYTHM, isPlayer, act ) { }
	public Command( IActionModule Module )
		: this( new ActionSet( Module ), true ) { }

	public ActionSet GetCurrentAction( Timing startedTiming )
	{
        int mt = Music.Just - startedTiming;
		Note n = ActionRhythm.GetNote(mt);
        if( n != null && n.hasNote )
        {
            return Actions[ActionRhythm.GetToneIndex(ActionRhythm.GetNoteIndex(mt))];
        }
        else return null;
	}
    public bool IsEnd(Timing startedTiming)
    {
        return Music.Just - startedTiming >= ActionRhythm.MTLength();
    }
}
