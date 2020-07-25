using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillListUI : MonoBehaviour {

	public bool IsExecutingCommand;
	public GaugeRenderer TimeLine;
	PlayerCommandData commandData_;
	//List<SkillUI> skillData_ = new List<SkillUI>();
	
	// Use this for initialization
	void Awake () {
		//skillData_.AddRange(GetComponentsInChildren<SkillUI>());
	}
	
	// Update is called once per frame
	void Update () {
		if( IsExecutingCommand )
		{
			/*
			if( Music.IsJustChangedBar() )
			{
				int bar = Music.Just.Bar;
				if( skillData_[bar].WillBeExecuted )
				{
					skillData_[bar].Execute();
				}
			}
			*/
			
			TimeLine.SetRate(1.0f - (float)(Music.MusicalTime / LuxSystem.TurnMusicalBars));

			/*
			int targetIndex = 0;
			for( int i = 0; i < skillData_.Count; ++i )
			{
				if( skillData_[i].WillBeExecuted )
				{
					Vector3 targetPos = Vector3.right * (5 * targetIndex) + Vector3.up * skillData_[i].transform.localPosition.y;
					skillData_[i].transform.localPosition = Vector3.Lerp(skillData_[i].transform.localPosition, targetPos, 0.2f);
					targetIndex += skillData_[i].length;
				}
			}
			*/
		}
	}

	public void Set(PlayerCommandData commandData)
	{
		commandData_ = commandData;
		/*
		foreach( SkillUI skill in skillData_ )
		{
			skill.Reset();
		}
		
		if( commandData_ != null )
		{
			int number = 0;

			ThemeColor themeColor = ColorManagerObsolete.GetThemeColor(commandData_.OwnerCommand.themeColor);
			baseColor = themeColor.Bright;
			foreach( KeyValuePair<Timing, Skill> pair in commandData_.SkillDictionary )
			{
				Skill skill = pair.Value;
				number = Mathf.Min(4 - skill.length, Mathf.Max(pair.Key.Bar, number));

				if( skill.length <= 0 ) continue;

				skillData_[number].Set(skill.shortName, skill.length, baseColor, ColorManagerObsolete.Base.Bright);
				++number;
			}

			if( commandData_.OwnerCommand is RevertCommand )
			{
				skillData_[number].Set("OVERLOAD", 2, ColorManagerObsolete.Base.Back, ColorManagerObsolete.Base.Bright);
			}
		}

		foreach( SkillUI skill in skillData_ )
		{
			skill.Show();
		}
		*/


	}
}
