using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySkillListUI : MonoBehaviour {

	public GameObject SkillCutIn;

	public static readonly Timing ShowSkillCutInTiming = new Timing(0, 3, 0);

	bool isExecuting_;

	GaugeRenderer baseLine_;
	Dictionary<int, EnemyCommand> commandData_;
	List<SkillUI> skillData_ = new List<SkillUI>();

	// Use this for initialization
	void Awake () {
		baseLine_ = GetComponentInChildren<GaugeRenderer>();
		skillData_.AddRange(GetComponentsInChildren<SkillUI>());
		transform.localScale = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
		if( isExecuting_ )
		{
			if( Music.Just < ShowSkillCutInTiming )
			{
				transform.localScale = Vector3.zero;
				return;
			}

			if( Music.IsJustChangedAt(ShowSkillCutInTiming) )
			{
				transform.localScale = Vector3.one;
				SkillCutIn.GetComponent<Animation>().Play();
			}

			if( Music.IsJustChangedAt(CommandGraph.AllowInputEnd) )
			{
				isExecuting_ = false;
				return;
			}

			if( GameContext.LuxSystem.IsInverting )
			{
				return;
			}

			if( Music.IsJustChangedBar() )
			{
				int bar = Music.Just.Bar;
				if( skillData_[bar].WillBeExecuted )
				{
					skillData_[bar].Execute();
				}
			}

			if( AnimManager.IsAnimating(baseLine_.gameObject) == false )
			{
				float mtRate = (float)(Music.MusicalTime / LuxSystem.TurnMusicalUnits);
				baseLine_.SetRate(1.0f - mtRate);
			}

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
		}
	}

	public void Execute()
	{
		isExecuting_ = true;
	}

	public void Set(Dictionary<int, EnemyCommand> commandData)
	{
		commandData_ = commandData;
		foreach( SkillUI skill in skillData_ )
		{
			skill.Reset();
		}

		foreach( KeyValuePair<int, EnemyCommand> pair in commandData_ )
		{
			skillData_[pair.Key].Set(pair.Value.ShortText, 2/*temp*/, ColorManager.Base.Dark, ColorManager.Base.Bright, isEnemySkill: true);
		}

		foreach( SkillUI skill in skillData_ )
		{
			skill.Show();
		}

		baseLine_.SetColor(ColorManager.Base.Dark);
	}

	public void OnInvert()
	{
		transform.localScale = Vector3.zero;
		foreach( SkillUI skill in skillData_ )
		{
			skill.Reset();
		}
	}

	public void OnPlayerWin()
	{
		transform.localScale = Vector3.zero;
		isExecuting_ = false;
	}
}
