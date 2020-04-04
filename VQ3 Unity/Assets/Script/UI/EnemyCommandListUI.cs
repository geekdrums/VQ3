using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class EnemyCommandListUI : MonoBehaviour
{
	public float OldIconScale;
	public float ActiveIconScale;
	public float FutureIconScale;
	public float Interval;
	public float CenterInterval;
	public GaugeRenderer EdgeLine;
	public MidairPrimitive CurrentHex;

	public EnemyCommandState CurrentCommandState { get; private set; }
	List<GameObject> commandIcons_ = new List<GameObject>();
	int currentIndex_;
	GameObject CurrentCommandIcon { get { return (commandIcons_.Count > 0 && 0 <= currentIndex_ && currentIndex_ < commandIcons_.Count ? commandIcons_[currentIndex_] : null); } }

	void Start()
	{
	}

	void Update()
	{
		if( GameContext.BattleState == BattleState.Intro || GameContext.BattleState == BattleState.Wait ) return;

		for( int i = 0; i < commandIcons_.Count; ++i )
		{
			if( AnimManager.IsAnimating(commandIcons_[i].gameObject) ) continue;

			commandIcons_[i].transform.localPosition = Vector3.Lerp(commandIcons_[i].transform.localPosition, GetTargetPos(i), 0.2f);
			commandIcons_[i].transform.localScale = Vector3.Lerp(commandIcons_[i].transform.localScale, GetTargetScale(i), 0.2f);
		}

		if( Music.IsJustChangedAt(EnemySkillListUI.ShowSkillCutInTiming) && CurrentCommandIcon != null )
		{
			AnimManager.AddAnim(CurrentCommandIcon, Vector3.down * 2.5f, ParamType.Position, AnimType.BounceIn, 0.3f);
			AnimManager.AddAnim(CurrentCommandIcon, Vector3.zero, ParamType.Position, AnimType.BounceOut, 0.3f, (float)Music.Meter.SecPerUnit * 8);
		}

		if( AnimManager.IsAnimating(EdgeLine.gameObject) == false && commandIcons_.Count > 0 )
		{
			EdgeLine.transform.localPosition = new Vector3(commandIcons_[0].transform.localPosition.x, 0, 10);
			EdgeLine.Length = commandIcons_[commandIcons_.Count - 1].transform.localPosition.x - commandIcons_[0].transform.localPosition.x;
		}

		if( CurrentCommandIcon != null && GameContext.BattleState != BattleState.Wait )
		{
			CurrentHex.transform.localPosition = new Vector3(CurrentCommandIcon.transform.localPosition.x, 0, -1);
			CurrentHex.SetArc((float)(-Music.MusicalTime / LuxSystem.TurnMusicalUnits));
		}
	}

	Vector3 GetTargetScale(int index)
	{
		if( index < currentIndex_ )
		{
			return Vector3.one * OldIconScale;
		}
		else if( index == currentIndex_ )
		{
			return Vector3.one * ActiveIconScale;
		}
		else// (i > currentIndex_ )
		{
			return Vector3.one * FutureIconScale;
		}
	}

	Vector3 GetTargetPos(int index)
	{
		Vector3 res = Vector3.zero;

		if( currentIndex_ < 0 )
		{
			return res;
		}

		float currentLength = ActiveIconScale * commandIcons_[currentIndex_].GetComponentInChildren<MidairPrimitive>().Radius / 3.0f;
		float selfLength = GetTargetScale(index).x * commandIcons_[index].GetComponentInChildren<MidairPrimitive>().Radius / 3.0f;
		if( index < currentIndex_ )
		{
			// 過去のコマンド
			res += Vector3.left * CenterInterval;
			for( int i = currentIndex_ - 1; i >= index; --i )
			{
				res += Vector3.left * (OldIconScale * commandIcons_[i].GetComponentInChildren<MidairPrimitive>().Radius / 3.0f * Interval);
			}
			res += Vector3.left * (currentLength - selfLength);
			return res;
		}
		else if( index == currentIndex_ )
		{
			// 現在のコマンド
			return Vector3.zero;
		}
		else// (index > currentIndex_ )
		{
			// 実行予定のコマンド
			res += Vector3.right * CenterInterval;
			for( int i = currentIndex_ + 1; i <= index; ++i )
			{
				res += Vector3.right * (FutureIconScale * commandIcons_[i].GetComponentInChildren<MidairPrimitive>().Radius / 3.0f * Interval);
			}
			res += Vector3.right * (currentLength - selfLength);
			return res;
		}
	}

	public void Set(EnemyCommandState commandState)
	{
		CurrentCommandState = commandState;
		for( int i = 0; i < CurrentCommandState.Pattern.Length; ++i )
		{
			AddCommand(CurrentCommandState.Pattern[i]);
		}
	}

	public void AddCommand(EnemyCommandSet command)
	{
		GameObject iconObj = Instantiate(command.IconPrefab);
		commandIcons_.Add(iconObj);
		iconObj.transform.parent = this.transform;
		iconObj.transform.localPosition = GetTargetPos(commandIcons_.Count - 1) + Vector3.up * 5;
	}

	public void ShowAnim()
	{
		for( int i = 0; i < commandIcons_.Count; ++i )
		{
			Vector3 targetPos = GetTargetPos(i);
			commandIcons_[i].transform.localPosition = targetPos + Vector3.up * 5;
			commandIcons_[i].transform.localScale = GetTargetScale(i);
			AnimManager.AddAnim(commandIcons_[i].gameObject, targetPos, ParamType.Position, AnimType.BounceIn, 0.2f, i * (float)Music.Meter.SecPerUnit * 4.0f / commandIcons_.Count);
		}
		if( commandIcons_.Count > 0 )
		{
			EdgeLine.transform.localPosition = commandIcons_[0].transform.localPosition;
			EdgeLine.Length = commandIcons_[commandIcons_.Count - 1].transform.localPosition.x - commandIcons_[0].transform.localPosition.x;
		}
		EdgeLine.SetRate(0);
		AnimManager.AddAnim(EdgeLine.gameObject, 1.0f, ParamType.GaugeRate, AnimType.BounceIn, 0.2f, (float)Music.Meter.SecPerUnit * 4);
	}

	public void ClearCommands()
	{
		CurrentCommandState = null;
		for( int i = 0; i < commandIcons_.Count; ++i )
		{
			Vector3 targetPos = GetTargetPos(i) + Vector3.up * 5;
			AnimManager.AddAnim(commandIcons_[i].gameObject, targetPos, ParamType.Position, AnimType.BounceOut, 0.3f, i * (float)Music.Meter.SecPerUnit * 4.0f / commandIcons_.Count, true);
		}
		AnimManager.AddAnim(EdgeLine.gameObject, 0.0f, ParamType.GaugeRate, AnimType.BounceOut, 0.2f, (float)Music.Meter.SecPerUnit * 4);
		CurrentHex.SetArc(0);
		commandIcons_.Clear();
		currentIndex_ = -1;
	}

	public void OnExecCommand()
	{
		++currentIndex_;

		if( currentIndex_ >= commandIcons_.Count )
		{
			currentIndex_ = 0;
			for( int i = 0; i < commandIcons_.Count; ++i )
			{
				MidairPrimitive mask = commandIcons_[i].GetComponentsInChildren<MidairPrimitive>().First((MidairPrimitive primitive) => primitive.name == "Mask");
				if( mask != null )
				{
					mask.SetColor(Color.clear);
				}
			}
		}
		else
		{
			CurrentHex.SetSize(commandIcons_[currentIndex_].GetComponentInChildren<MidairPrimitive>().Radius);
			CurrentHex.SetWidth(commandIcons_[currentIndex_].GetComponentInChildren<MidairPrimitive>().Radius);
			CurrentHex.transform.localScale = Vector3.one * ActiveIconScale;
			for( int i = 0; i < currentIndex_; ++i )
			{
				MidairPrimitive mask = commandIcons_[i].GetComponentsInChildren<MidairPrimitive>().First((MidairPrimitive primitive) => primitive.name == "Mask");
				if( mask != null )
				{
					mask.SetColor(ColorManager.MakeAlpha(Color.black, 0.3f));
				}
			}
		}
	}
}
