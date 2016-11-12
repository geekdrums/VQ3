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
		foreach( KeyValuePair<Timing, Skill> pair in SkillDictionary )
		{
			if( Music.IsJustChangedAt(startBar + pair.Key.Bar, pair.Key.Beat, pair.Key.Unit) ) return pair.Value;
		}
		return null;
	}
}
