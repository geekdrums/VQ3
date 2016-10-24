using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemyCommand : CommandBase
{
	public string ShortText;
	public string ExplanationText;
	public List<string> CommandAliases;

    public Skill GetCurrentSkill( int startBar )
    {
		int totalUnit = Music.Just.MusicalTime - startBar * Music.CurrentUnitPerBar;
        return SkillDictionary.ContainsKey( totalUnit ) ? SkillDictionary[totalUnit] : null;
    }
}
