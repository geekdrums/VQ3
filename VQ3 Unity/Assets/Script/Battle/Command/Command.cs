using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Command : MonoNode
{
    public string MusicBlockName;
    public List<int> ExpThreasholds;
    public List<Skill> _skillList;
    public string _timingStr = "0,1,2,3";

    protected int Level;
    protected Dictionary<int, Skill> SkillDictionary = new Dictionary<int,Skill>();

    public Strategy ParentStrategy { get; protected set; }
    public int NextExp { get { return (ExpThreasholds.Count > Level ? ExpThreasholds[Level] : -1); } }
    public bool isExecutable { get { return Level > 0; } }
    
    public void Parse(){
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
        }
    }

    public virtual GameObject GetCurrentSkill()
    {
        if( GameContext.VoxonSystem.state == VoxonSystem.VoxonState.ShowBreak && Music.Just.bar >= 3 ) return null;
        return SkillDictionary.ContainsKey( Music.Just.totalUnit ) ? SkillDictionary[Music.Just.totalUnit].gameObject : null;
    }
    public void SetParent( Strategy parent )
    {
        ParentStrategy = parent;
        Level = ExpThreasholds.Count;
        for( int i = 0; i < ExpThreasholds.Count; i++ )
        {
            if( parent.Exp < ExpThreasholds[i] )
            {
                Level = i;
                break;
            }
        }
    }

    public int GetWillGainVoxon()
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
                        sum += a.GetModule<MagicModule>().VoxonPoint;
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

    public void SetLink( bool linked )
    {
        if( linked )
        {
            GetComponent<TextMesh>().color = Color.black;
        }
        else
        {
            GetComponent<TextMesh>().color = Color.gray;
        }
    }
    public void SetCurrent()
    {
        GetComponent<TextMesh>().color = Color.red;
    }
    public void SetNext()
    {
        GetComponent<TextMesh>().color = Color.magenta;
    }
}