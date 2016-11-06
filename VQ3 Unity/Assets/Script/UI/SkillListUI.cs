using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillListUI : MonoBehaviour {

	GaugeRenderer baseLine_;
	PlayerCommandData commandData_;
	List<SkillUI> skillData_ = new List<SkillUI>();
	List<EnhanceUI> enhanceData_ = new List<EnhanceUI>();



	// Use this for initialization
	void Start () {
		baseLine_ = GetComponentInChildren<GaugeRenderer>();
		skillData_.AddRange(GetComponentsInChildren<SkillUI>());
		enhanceData_.AddRange(GetComponentsInChildren<EnhanceUI>());
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
		foreach( EnhanceUI enhance in enhanceData_ )
		{
			enhance.Reset();
		}

		int number = 0;

		if( commandData_.DefendPercent > 0 )
		{
			enhanceData_[number].Set(EnhanceParamType.Defend);
			++number;
		}
		if( commandData_.HealPercent > 0 )
		{
			enhanceData_[number].Set(EnhanceParamType.Heal);
			++number;
		}

		ThemeColor themeColor = ColorManager.GetThemeColor(commandData_.OwnerCommand.themeColor);
		Color baseColor = themeColor.Bright;
		foreach( KeyValuePair<int,Skill> pair in commandData_.SkillDictionary )
		{
			Skill skill = pair.Value;
			int bar = (int)(pair.Key / 16);
			number = Mathf.Min(4 - skill.length, Mathf.Max(bar, number));

			if( skill.length <= 0 ) continue;

			skillData_[number].Set(skill.shortName, skill.length, baseColor, ColorManager.Base.Bright);
			++number;
		}

		foreach( SkillUI skill in skillData_ )
		{
			skill.Show();
		}

		baseLine_.SetColor(baseColor);
	}
}
