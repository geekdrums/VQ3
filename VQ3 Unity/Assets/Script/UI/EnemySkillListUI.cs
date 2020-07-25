using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySkillListUI : MonoBehaviour {
	
	bool isExecuting_;

	public GaugeRenderer TimeLine;
	Dictionary<int, EnemyCommand> commandData_;
	//List<SkillUI> skillData_ = new List<SkillUI>();

	// Use this for initialization
	void Awake () {
		//skillData_.AddRange(GetComponentsInChildren<SkillUI>());
	}
	
	// Update is called once per frame
	void Update () {
		if( isExecuting_ )
		{
			if( Music.IsJustChangedAt(CommandGraph.AllowInputEnd) )
			{
				isExecuting_ = false;
				return;
			}

			if( GameContext.LuxSystem.IsInverting )
			{
				return;
			}

			if( AnimManager.IsAnimating(TimeLine.gameObject) == false )
			{
				TimeLine.SetRate(1.0f - (float)(Music.MusicalTime / LuxSystem.TurnMusicalBars));
			}

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

	public void Execute()
	{
		isExecuting_ = true;
		transform.localScale = Vector3.one;
	}

	public void Set(Dictionary<int, EnemyCommand> commandData)
	{
		commandData_ = commandData;
		/*
		foreach( SkillUI skill in skillData_ )
		{
			skill.Reset();
		}

		foreach( KeyValuePair<int, EnemyCommand> pair in commandData_ )
		{
			skillData_[pair.Key].Set(pair.Value.ShortText, 2, ColorManagerObsolete.Base.Dark, ColorManagerObsolete.Base.Bright, isEnemySkill: true);
		}

		foreach( SkillUI skill in skillData_ )
		{
			skill.Show();
		}
		*/
	}

	public void OnInvert()
	{
		transform.localScale = Vector3.zero;
		/*
		foreach( SkillUI skill in skillData_ )
		{
			skill.Reset();
		}
		*/
	}

	public void OnPlayerWin()
	{
		transform.localScale = Vector3.zero;
		isExecuting_ = false;
	}
}
