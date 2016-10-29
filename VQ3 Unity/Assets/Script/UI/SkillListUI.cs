using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillListUI : MonoBehaviour {

	GaugeRenderer baseLine_;
	PlayerCommandData commandData_;
	List<SkillUI> skillData_ = new List<SkillUI>();



	// Use this for initialization
	void Start () {
		baseLine_ = GetComponentInChildren<GaugeRenderer>();
		skillData_.AddRange(GetComponentsInChildren<SkillUI>());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Set(PlayerCommandData commandData)
	{
		commandData_ = commandData;
		foreach(SkillUI skill in skillData_)
		{
			skill.Reset();
		}

		int number = 0;
		ThemeColor themeColor = ColorManager.GetThemeColor(commandData_.OwnerCommand.themeColor);
		Color baseColor = themeColor.Bright;

		if( commandData_.DefendPercent > 40 )
		{
			skillData_[number].Set("GAND", true, 2, baseColor, themeColor.Shade);
			++number;
		}
		else if( commandData_.DefendPercent > 0 )
		{
			skillData_[number].Set("GAD", true, 1, baseColor, themeColor.Shade);
			++number;
		}
		if( commandData_.HealPercent > 30 )
		{
			skillData_[number].Set("LIMM", true, 2, baseColor, themeColor.Light);
			++number;
		}
		else if( commandData_.DefendPercent > 0 )
		{
			skillData_[number].Set("LIL", true, 1, baseColor, themeColor.Light);
			++number;
		}

		foreach( KeyValuePair<int,Skill> pair in commandData_.SkillDictionary )
		{
			Skill skill = pair.Value;
			int bar = (int)(pair.Key / 16);
			number = Mathf.Min(4 - skill.length, Mathf.Max(bar, number));

			if( skill.length <= 0 ) continue;

			skillData_[number].Set(skill.shortName, false, skill.length, baseColor, ColorManager.Base.Bright);
			++number;
		}

		foreach( SkillUI skill in skillData_ )
		{
			skill.Show();
		}

		baseLine_.SetColor(baseColor);
	}
}
