using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CommandBase : MonoBehaviour
{
    public List<Skill> _skillList;
    public string _timingStr = "0 2 0";
	public int DefendPercent;
    public int HealPercent;

    public Dictionary<int, Skill> SkillDictionary = new Dictionary<int, Skill>();

    public virtual void Parse()
    {
#if UNITY_EDITOR
        if( !UnityEditor.EditorApplication.isPlaying ) return;
#endif
        string[] timingStrs = _timingStr.Split( ",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries );
        if( timingStrs.Length != _skillList.Count )
        {
            Debug.LogError( this.name + " has invalid skill list! _skillList.Count = " + _skillList.Count + ", timingStrs.Length = " + timingStrs.Length );
            return;
        }
        for( int i = 0; i < timingStrs.Length; i++ )
        {
            string[] barBeatUnitStr = timingStrs[i].Split( " ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries );
            int bar = int.Parse( barBeatUnitStr[0] );
            int beat = barBeatUnitStr.Length > 1 ? int.Parse( barBeatUnitStr[1] ) : 0;
            int unit = barBeatUnitStr.Length > 2 ? int.Parse( barBeatUnitStr[2] ) : 0;
            SkillDictionary.Add( new Timing( bar, beat, unit ).totalUnit, _skillList[i] );
            _skillList[i].Parse();
        }
    }
}
