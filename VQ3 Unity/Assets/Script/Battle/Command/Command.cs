using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[ExecuteInEditMode]
public class Command : MonoNode
{
    static readonly float radius = 7.5f;

    public string MusicBlockName;
    public List<Skill> _skillList;
    public string _timingStr = "0,1,2,3";
    public float latitude;
    public float longitude;
    public int AcquireLevel = 1;
    public string[] DescribeTexts;
    public string[] AcquireTexts;

    public Strategy ParentStrategy { get; protected set; }
    public bool IsLinked { get; protected set; }
    public bool IsCurrent { get; protected set; }
    public bool IsSelected { get; protected set; }
    public bool IsAcquired { get; protected set; }
    public bool IsTargetSelectable { get { return _skillList.Find( ( Skill s ) => s.IsTargetSelectable ) != null; } }

    protected Dictionary<int, Skill> SkillDictionary = new Dictionary<int, Skill>();

    void Start()
    {
        Parse();
        IsLinked = false;
        IsCurrent = false;
        IsSelected = false;
    }

    void Update()
    {
#if UNITY_EDITOR
        if( UnityEditor.EditorApplication.isPlaying ) return;
        transform.localPosition = Quaternion.AngleAxis( latitude, Vector3.right ) * Quaternion.AngleAxis( longitude, Vector3.down ) * Vector3.back * radius;
        transform.localRotation = Quaternion.LookRotation( -transform.localPosition );
#endif
    }

    void Parse()
    {
#if UNITY_EDITOR
        if( !UnityEditor.EditorApplication.isPlaying ) return;
#endif
        string[] timingStrs = _timingStr.Split( ",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries );
        if( timingStrs.Length != _skillList.Count )
        {
            Debug.LogError("invalid skill list! _skillList.Count = " + _skillList.Count + ", timingStrs.Length = " + timingStrs.Length);
            return;
        }
        for( int i=0; i<timingStrs.Length; i++ )
        {
            string[] barBeatUnitStr = timingStrs[i].Split( " ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries );
            int bar = int.Parse( barBeatUnitStr[0] );
            int beat = barBeatUnitStr.Length > 1 ? int.Parse( barBeatUnitStr[1] ) : 0;
            int unit = barBeatUnitStr.Length > 2 ? int.Parse( barBeatUnitStr[2] ) : 0;
            SkillDictionary.Add( new Timing( bar, beat, unit ).totalUnit, _skillList[i] );
            _skillList[i].Parse();
        }
        IsLinked = true;
        IsAcquired = AcquireLevel <= GameContext.PlayerConductor.Level;
        if( !IsAcquired )
        {
            GetComponent<TextMesh>().color = Color.clear;
        }
    }

    public virtual GameObject GetCurrentSkill()
    {
        if( GameContext.VoxSystem.state == VoxState.Eclipse && Music.Just.bar >= 2 ) return null;
        return SkillDictionary.ContainsKey( Music.Just.totalUnit ) ? SkillDictionary[Music.Just.totalUnit].gameObject : null;
    }
    public void SetParent( Strategy parent )
    {
        ParentStrategy = parent;
    }

    public int GetWillGainVP()
    {
        int sum = 0;
        for( int i = 0; i < SkillDictionary.Count; ++i )
        {
            if( SkillDictionary.Keys.ElementAt( i ) < 3 * Music.mtBar )
            {
                Skill skill = SkillDictionary.Values.ElementAt( i );
                if( skill.Actions == null ) skill.Parse();
                foreach( ActionSet a in skill.Actions )
                {
                    if( a.GetModule<MagicModule>() != null )
                    {
                        sum += a.GetModule<MagicModule>().VoxPoint;
                    }
                }
            }
            else break;
        }
        return sum;
    }

    public virtual string GetBlockName()
    {
        return MusicBlockName;
    }
    public bool IsUsable()
    {
        return IsAcquired && IsLinked;
    }


    public IEnumerable<Command> LinkedCommands
    {
        get
        {
            if( ParentStrategy != null )
            {
                foreach( Command c in ParentStrategy.LinkedCommands )
                {
                    yield return c;
                }
            }
            foreach( MonoNode link in links )
            {
                Strategy linkedStrategy = link as Strategy;
                Command linkedCommand = link as Command;
                if( linkedStrategy != null )
                {
                    foreach( Command c in linkedStrategy.Commands )
                    {
                        yield return c;
                    }
                }
                else if( linkedCommand != null )
                {
                    yield return linkedCommand;
                }
            }
        }
    }

    public virtual void SetLink( bool linked )
    {
        IsCurrent = false;
        IsSelected = false;
        IsLinked = linked;
        if( IsUsable() )
        {
            GetComponent<TextMesh>().color = Color.black;
        }
        else
        {
            if( IsAcquired )
            {
                GetComponent<TextMesh>().color = Color.gray;
            }
        }
    }
    public void SetCurrent()
    {
        if( IsAcquired )
        {
            IsCurrent = true;
            GetComponent<TextMesh>().color = Color.red;
        }
    }
    public void Select()
    {
        if( IsAcquired )
        {
            IsSelected = true;
            if( !IsCurrent )
            {
                GetComponent<TextMesh>().color = Color.magenta;
            }
        }
    }
    public void Deselect()
    {
        if( IsAcquired )
        {
            IsSelected = false;
            if( !IsCurrent )
            {
                GetComponent<TextMesh>().color = Color.black;
            }
        }
    }
    public void Acquire()
    {
        IsAcquired = true;
        GetComponent<TextMesh>().color = Color.gray;
    }
}