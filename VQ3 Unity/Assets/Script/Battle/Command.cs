using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum ECommand
{
	Attack,
	Power,
	Guard,
	Magic,
	Cure,
	Break,
	BreakAttack,
	Count
}

public class Command : MonoBehaviour
{
    public bool isLocal;
    public bool isPlayerAction;
    public string[] ActionStr;
    public string RhythmStr;

    protected ActionSet[] Actions;
	protected Rhythm ActionRhythm;
	protected Animation CommandAnim;
	protected bool isEnd;

	public Character OwnerCharacter { get; protected set; }

	public void SetOwner( Character chara ) { OwnerCharacter = chara; }

    void Awake()
    {
        if( RhythmStr != "" )
        {
            ActionRhythm = new Rhythm( 4, RhythmStr );
        }
        else
        {
            ActionRhythm = Rhythm.ONE_NOTE_RHYTHM;
        }
        Actions = new ActionSet[ActionStr.Length];
        for( int i = 0; i < ActionStr.Length; ++i )
        {
            Actions[i] = ActionSet.Parse( ActionStr[i] );
        }
    }

    // Use this for initialization
    void Start()
    {
		CommandAnim = GetComponentInChildren<Animation>();
		//GetComponentInChildren<Animation>()["attackAnim"].speed = 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (isEnd && (CommandAnim == null || !CommandAnim.isPlaying)) Destroy(this.gameObject);
    }

	public void OnExecuted()
	{
		if ( CommandAnim != null && !CommandAnim.isPlaying )
		{
			CommandAnim.Play();
		}
	}

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
    public bool CheckIsEnd(Timing startedTiming)
    {
		isEnd = Music.Just - startedTiming >= ActionRhythm.MTLength(); 
        return isEnd;
    }
}
