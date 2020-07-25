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
	public ResonantRoll Roll1;
	public ResonantRoll Roll2;
	public string UniqueRollName;

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
	public Color GetAtkColor()
	{
		float atk = GetAtk();
		ThemeColor theme = ColorManagerObsolete.GetThemeColor(OwnerCommand.themeColor);
		BaseColor baseColor = ColorManagerObsolete.Base;
		if( atk <= 0 )
		{
			return baseColor.MiddleBack;
		}
		else if( atk <= 15 )
		{
			return theme.Shade;
		}
		else if( atk <= 30 )
		{
			return theme.Light;
		}
		else
		{
			return theme.Bright;
		}
	}
	public Color GetHealColor()
	{
		float heal = GetHeal();
		ThemeColor theme = ColorManagerObsolete.GetThemeColor(OwnerCommand.themeColor);
		BaseColor baseColor = ColorManagerObsolete.Base;
		if( heal <= 0 )
		{
			return baseColor.MiddleBack;
		}
		else if( heal <= 5 )
		{
			return theme.Shade;
		}
		else if( heal <= 30 )
		{
			return theme.Light;
		}
		else
		{
			return theme.Bright;
		}
	}
	public Color GetDefColor()
	{
		float def = GetDefend();
		ThemeColor theme = ColorManagerObsolete.GetThemeColor(OwnerCommand.themeColor);
		BaseColor baseColor = ColorManagerObsolete.Base;
		if( def <= 0 )
		{
			return baseColor.MiddleBack;
		}
		else if( def <= 20 )
		{
			return theme.Shade;
		}
		else if( def <= 50 )
		{
			return theme.Light;
		}
		else
		{
			return theme.Bright;
		}
	}
	public Color GetVPColor()
	{
		float vp = GetVP();
		Color color = ColorManagerObsolete.Accent.Break;
		BaseColor baseColor = ColorManagerObsolete.Base;
		if( vp <= 0 )
		{
			return baseColor.MiddleBack;
		}
		else if( vp <= 15 )
		{
			return ColorManagerObsolete.MakeAlpha(color, 0.2f);
		}
		else if( vp <= 30 )
		{
			return ColorManagerObsolete.MakeAlpha(color, 0.5f);
		}
		else
		{
			return color;
		}
	}
	public Color GetVTColor()
	{
		float vt = GetVT();
		Color color = ColorManagerObsolete.Accent.Time;
		BaseColor baseColor = ColorManagerObsolete.Base;
		if( vt <= 0 )
		{
			return baseColor.MiddleBack;
		}
		else if( vt <= 15 )
		{
			return ColorManagerObsolete.MakeAlpha(color, 0.2f);
		}
		else if( vt <= 30 )
		{
			return ColorManagerObsolete.MakeAlpha(color, 0.5f);
		}
		else
		{
			return color;
		}
	}
	public Color GetEnhColor()
	{
		List<EnhanceModule> enh = GetEnhModules();
		ThemeColor theme = ColorManagerObsolete.GetThemeColor(OwnerCommand.themeColor);
		BaseColor baseColor = ColorManagerObsolete.Base;
		if( enh == null || enh.Count == 0 )
		{
			return baseColor.Back;
		}
		else if( enh.Count == 1 && enh[0].phase == 1 )
		{
			return theme.Light;
		}
		else
		{
			return theme.Bright;
		}
	}
}

