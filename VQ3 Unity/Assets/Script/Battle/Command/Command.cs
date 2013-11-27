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
	Heal,
	Fire1,
	Fire2,
	BreakAttack,
	Wait,
	Count
}

public class Command : MonoBehaviour
{
    public bool isPlayerAction;
    public string[] ActionStr;
    public string RhythmStr;
    public GameObject bgEffefctPrefab;

	public ActionSet[] Actions { get; protected set; }
	protected Rhythm ActionRhythm;
	protected Animation CommandAnim;
	protected bool isEnd;

	public Character OwnerCharacter { get; protected set; }

	public void SetOwner( Character chara ) { OwnerCharacter = chara; }

    void Awake()
    {
        Parse();
    }

    public void Parse()
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
        if( isPlayerAction )
        {
            GameContext.BattleConductor.SetBGEffect( bgEffefctPrefab );
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isEnd && (CommandAnim == null || !CommandAnim.isPlaying)) Destroy(this.gameObject);
    }

	public void OnExecuted( ActionSet act )
    {
        AnimModule anim = act.GetModule<AnimModule>();
        if( anim != null )
        {
            string AnimName = anim.AnimName == "" ? name.Replace( "Command(Clone)", "Anim" ) : anim.AnimName;
            CommandAnim = GetComponentInChildren<Animation>();
            if( CommandAnim.GetClip( AnimName ) != null )
            {
                CommandAnim[AnimName].speed = 1 / (float)(Music.mtBeat * Music.mtUnit);
                CommandAnim.Play( AnimName );
            }
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
			int noteIndex = ActionRhythm.GetNoteIndex( mt );
			int toneIndex = ActionRhythm.GetToneIndex( noteIndex );
			if ( 0 <= toneIndex && toneIndex < Actions.Length )
			{
				return Actions[toneIndex];
			}
			else return null;
        }
        else return null;
	}
    public bool CheckIsEnd(Timing startedTiming)
    {
		isEnd = Music.Just - startedTiming >= ActionRhythm.MTLength(); 
        return isEnd;
    }
}
