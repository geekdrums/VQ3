using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Skill : MonoBehaviour
{
	public string shortName;
	public int length;
	public bool isPlayerSkill;
    public string[] _actionStr;
    public string _rhythmStr;
    public int _rhythmBaseTime = 4;
    public List<GameObject> _prefabs;
	public Vector2 animRandomRange = Vector2.zero;
	public Vector2 animRandomExcludeRange = Vector2.zero;
	public string characterAnimName;
	public GameObject damageParent;

	public List<ActionSet> Actions { get; protected set; }
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
		Actions = new List<ActionSet>();// ActionSet[_actionStr.Length];
        IsTargetSelectable = false;
        for( int i = 0; i < _actionStr.Length; ++i )
        {
            Actions.Add( ActionSet.Parse( _actionStr[i], this ) );
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
    }

    // Update is called once per frame
    void Update()
    {
		if( isEnd && (SkillAnim == null || !SkillAnim.isPlaying) )
		{
			OwnerCharacter.OnSkillEnd(this);
			Destroy(this.gameObject);
		}
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
				SkillAnim[AnimName].speed = (float)(Music.CurrentTempo / 60.0);
				SkillAnim.Play(AnimName);
				Vector2 randomUnit = UnityEngine.Random.insideUnitCircle;
				transform.localPosition +=
					new Vector3(Mathf.Sign(randomUnit.x) * animRandomExcludeRange.x + randomUnit.x * (animRandomRange.x - animRandomExcludeRange.x),
					Mathf.Sign(randomUnit.y) * animRandomExcludeRange.y + randomUnit.y * (animRandomRange.y - animRandomExcludeRange.y), 0);
            }
        }
		OwnerCharacter.OnExecuted(this, act);
	}

	public Skill( Rhythm rhythm, bool isPlayer = true, params ActionSet[] inActions )
	{
		Actions = new List<ActionSet>();
		Actions.AddRange( inActions );
		ActionRhythm = rhythm;
		isPlayerSkill = isPlayer;
	}
	public Skill( ActionSet act, bool isPlayer = true )
		: this( Rhythm.ONE_NOTE_RHYTHM, isPlayer, act ) { }
	public Skill( IActionModule Module )
		: this( new ActionSet( Module ), true ) { }

	public ActionSet GetCurrentAction( Timing startedTiming )
	{
        int mt = Music.JustTotalUnits - startedTiming.GetTotalUnits(Music.Meter);
		Note n = ActionRhythm.GetNote(mt);
        if( n != null && n.hasNote )
        {
			int noteIndex = ActionRhythm.GetNoteIndex( mt );
			int toneIndex = ActionRhythm.GetToneIndex( noteIndex );
			if ( 0 <= toneIndex && toneIndex < Actions.Count )
			{
				return Actions[toneIndex];
			}
			else return null;
        }
        else return null;
	}
    public bool CheckIsEnd(Timing startedTiming)
    {
		isEnd = OwnerCharacter.isAlive == false || Music.JustTotalUnits - startedTiming.GetTotalUnits(Music.Meter) >= ActionRhythm.MTLength();
        return isEnd;
    }
}
