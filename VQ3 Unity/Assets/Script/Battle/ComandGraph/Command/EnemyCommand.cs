﻿using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemyCommand : CommandBase
{
    public string nextState;

    //public string DescribeText;
    public string ShortText;
    public EStatusIcon Icon;

    public Skill GetCurrentSkill( int startBar )
    {
        int totalUnit = Music.Just.totalUnit - startBar * Music.mtBar;
        return SkillDictionary.ContainsKey( totalUnit ) ? SkillDictionary[totalUnit] : null;
    }
}
