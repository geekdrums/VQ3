using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemyCommand : CommandBase
{
    public string nextState;
    public bool isPassive;

    public string DescribeText;
    public string ShortText;

    public Skill GetCurrentSkill( int startBar )
    {
        int totalUnit = Music.Just.totalUnit - startBar * Music.mtBar;
        return SkillDictionary.ContainsKey( totalUnit ) ? SkillDictionary[totalUnit] : null;
    }
}