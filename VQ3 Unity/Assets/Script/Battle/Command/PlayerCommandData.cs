using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class PlayerCommandData : CommandBase
{
	void Start()
	{
		Parse();
	}

	public PlayerCommand OwnerCommand;
	public int CommandLevel;
	public int RequireSP;
	public string MusicBlockName;
	public string DescribeText;
	public string ExplanationText;

	public float GetAtk()
	{
		int sum = 0;
		foreach( Skill skill in SkillDictionary.Values )
		{
			if( skill.Actions == null ) skill.Parse();
			foreach( ActionSet a in skill.Actions )
			{
				if( a.GetModule<AttackModule>() != null )
				{
					sum += a.GetModule<AttackModule>().Power;
				}
			}
		}
		return sum/4;
	}
	public float GetVP()
	{
		float sum = 0;
		foreach( Skill skill in SkillDictionary.Values )
		{
			if( skill.Actions == null ) skill.Parse();
			foreach( ActionSet a in skill.Actions )
			{
				if( a.GetModule<AttackModule>() != null )
				{
					sum += a.GetModule<AttackModule>().VP;
				}
			}
		}
		return sum;
	}
	public float GetVT()
	{
		int sum = 0;
		foreach( Skill skill in SkillDictionary.Values )
		{
			if( skill.Actions == null ) skill.Parse();
			foreach( ActionSet a in skill.Actions )
			{
				if( a.GetModule<AttackModule>() != null )
				{
					sum += a.GetModule<AttackModule>().VT;
				}
			}
		}
		return sum/4.0f;
	}
	public float GetHeal()
	{
		float sum = HealPercent;
		foreach( Skill skill in SkillDictionary.Values )
		{
			if( skill.Actions == null ) skill.Parse();
			foreach( ActionSet a in skill.Actions )
			{
				if( a.GetModule<HealModule>() != null )
				{
					sum += a.GetModule<HealModule>().HealPoint;
				}
			}
		}
		return sum;
	}
	public float GetDefend()
	{
		return DefendPercent;
	}
	public List<EnhanceModule> GetEnhModules()
	{
		List<EnhanceModule> enhModules = new List<EnhanceModule>();
		foreach( Skill skill in SkillDictionary.Values )
		{
			if( skill.Actions == null ) skill.Parse();
			foreach( ActionSet a in skill.Actions )
			{
				if( a.GetModule<EnhanceModule>() != null )
				{
					enhModules.Add(a.GetModule<EnhanceModule>());
				}
			}
		}
		return enhModules;
	}
}

