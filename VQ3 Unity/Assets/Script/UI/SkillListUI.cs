using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillListUI : MonoBehaviour {

	bool isExecuting_;
	bool isTurnStarted_;

	GaugeRenderer baseLine_;
	PlayerCommandData commandData_;
	List<SkillUI> skillData_ = new List<SkillUI>();
	List<EnhanceUI> enhanceData_ = new List<EnhanceUI>();



	// Use this for initialization
	void Awake () {
		baseLine_ = GetComponentInChildren<GaugeRenderer>();
		skillData_.AddRange(GetComponentsInChildren<SkillUI>());
		enhanceData_.AddRange(GetComponentsInChildren<EnhanceUI>());
	}
	
	// Update is called once per frame
	void Update () {
		if( isExecuting_ )
		{
			if( isTurnStarted_ == false )
			{
				if( Music.IsJustChangedAt(0) )
				{
					isTurnStarted_ = true;
				}
				else
				{
					return;
				}
			}

			if( Music.IsJustChangedAt(CommandGraph.AllowInputEnd) )
			{
				Destroy(this.gameObject);
				return;
			}

			if( Music.IsJustChangedBar() )
			{
				int bar = Music.Just.Bar;
				if( skillData_[bar].length > 0 )
				{
					skillData_[bar].Execute();
				}
			}

			float mtRate = (float)(Music.MusicalTime / LuxSystem.TurnMusicalUnits);
			baseLine_.SetRate(1.0f - mtRate);

			int targetIndex = 0;
			for( int i = 0; i < skillData_.Count; ++i )
			{
				if( skillData_[i].WillBeExecuted )
				{
					Vector3 targetPos = Vector3.right * (5 * targetIndex /*- 19.5f * mtRate*/) + Vector3.up * skillData_[i].transform.localPosition.y;
					skillData_[i].transform.localPosition = Vector3.Lerp(skillData_[i].transform.localPosition, targetPos, 0.2f);
					targetIndex += skillData_[i].length;
				}
			}
		}
	}

	public void Execute(GameObject enhIconParent)
	{
		isExecuting_ = true;
		for( int i = 0; i < enhanceData_.Count; ++i )
		{
			if( enhanceData_[i].paramType != EnhanceParamType.Count )
			{
				enhanceData_[i].transform.parent = enhIconParent.transform;
				AnimManager.AddAnim(enhanceData_[i].gameObject, Vector3.right * 2.4f * i, ParamType.Position, AnimType.Linear, 0.2f);
				enhanceData_[i].Execute();
			}
		}
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
