using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemyCommand : MonoBehaviour
{
    public List<Skill> _skillList;
    public string _timingStr = "0 2 0";
    public int numBar = 1;
    public List<int> probabilityList;
    public string currentState;
    public string nextState;
    public EnemyCommand nextCommand;

    protected Dictionary<int, Skill> SkillDictionary = new Dictionary<int, Skill>();

    public void Parse()
    {
        string[] timingStrs = _timingStr.Split( ",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries );
        if( timingStrs.Length != _skillList.Count )
        {
            Debug.LogError( "invalid skill list! _skillList.Count = " + _skillList.Count + ", timingStrs.Length = " + timingStrs.Length );
            return;
        }
        for( int i = 0; i < timingStrs.Length; i++ )
        {
            string[] barBeatUnitStr = timingStrs[i].Split( " ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries );
            int bar = int.Parse( barBeatUnitStr[0] );
            int beat = barBeatUnitStr.Length > 1 ? int.Parse( barBeatUnitStr[1] ) : 0;
            int unit = barBeatUnitStr.Length > 2 ? int.Parse( barBeatUnitStr[2] ) : 0;
            SkillDictionary.Add( new Timing( bar, beat, unit ).totalUnit, _skillList[i] );
        }
    }

    public Skill GetCurrentSkill( int startBar )
    {
        int totalUnit = Music.Just.totalUnit - startBar * Music.mtBar;
        return SkillDictionary.ContainsKey( totalUnit ) ? SkillDictionary[totalUnit] : null;
    }
    public int GetProbability( int enemyState )
    {
        if( 0 <= enemyState && enemyState < probabilityList.Count )
        {
            return probabilityList[enemyState];
        }
        else
        {
            //Debug.LogError( "enemyState : " + enemyState + " is out of range! probabilityList.Count = " + probabilityList.Count  );
            return 0;
        }
    }
}