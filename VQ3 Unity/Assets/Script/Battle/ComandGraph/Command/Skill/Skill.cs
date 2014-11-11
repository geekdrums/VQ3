using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Skill : MonoBehaviour
{
    public bool isPlayerSkill;
    public string[] _actionStr;
    public string _rhythmStr;
    public int _rhythmBaseTime = 4;
    public List<GameObject> _prefabs;
    //public GameObject bgEffefctPrefab;
    //public string DescribeText;

	public ActionSet[] Actions { get; protected set; }
    public bool IsTargetSelectable { get; protected set; }
	protected Rhythm ActionRhythm;
	protected Animation SkillAnim;
	protected bool isEnd;

	public Character OwnerCharacter { get; protected set; }

    public void SetOwner( Character chara )
    {
        OwnerCharacter = chara;
    }

    void Awake()
    {
        Parse();
    }

    public void Parse()
    {
        if( _rhythmStr != "" && _rhythmBaseTime > 0 )
        {
            ActionRhythm = new Rhythm( _rhythmBaseTime, _rhythmStr );
        }
        else
        {
            ActionRhythm = Rhythm.ONE_NOTE_RHYTHM;
        }
        Actions = new ActionSet[_actionStr.Length];
        IsTargetSelectable = false;
        for( int i = 0; i < _actionStr.Length; ++i )
        {
            Actions[i] = ActionSet.Parse( _actionStr[i], this );
            if( !IsTargetSelectable && Actions[i].GetModule<TargetModule>() != null
                && Actions[i].GetModule<TargetModule>().TargetType == TargetType.Select )
            {
                IsTargetSelectable = true;
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        //if( isPlayerSkill )
        //{
        //    GameContext.BattleConductor.SetBGEffect( bgEffefctPrefab );
        //}
    }

    // Update is called once per frame
    void Update()
    {
        if (isEnd && (SkillAnim == null || !SkillAnim.isPlaying)) Destroy(this.gameObject);
    }

	public void OnExecuted( ActionSet act )
    {
        AnimModule anim = act.GetModule<AnimModule>();
        if( anim != null )
        {
            string AnimName = anim.AnimName == "" ? name.Replace( "Command(Clone)", "Anim" ) : anim.AnimName;
            SkillAnim = GetComponentInChildren<Animation>();
            if( SkillAnim.GetClip( AnimName ) != null )
            {
                SkillAnim[AnimName].speed = 1 / (float)(Music.mtBeat * Music.MusicTimeUnit);
                SkillAnim.Play( AnimName );
            }
        }
	}

	public Skill( Rhythm rhythm, bool isPlayer = true, params ActionSet[] inActions )
	{
		Actions = inActions;
		ActionRhythm = rhythm;
		isPlayerSkill = isPlayer;
	}
	public Skill( ActionSet act, bool isPlayer = true )
		: this( Rhythm.ONE_NOTE_RHYTHM, isPlayer, act ) { }
	public Skill( IActionModule Module )
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
