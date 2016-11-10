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

	List<GameObject> commandIcons_ = new List<GameObject>();
	int currentIndex_;

	void Start()
	{
	}

	void Update()
	{
		for( int i = 0; i < commandIcons_.Count; ++i )
		{
			if( AnimManager.IsAnimating(commandIcons_[i].gameObject) ) continue;

			commandIcons_[i].transform.localPosition = Vector3.Lerp(commandIcons_[i].transform.localPosition, GetTargetPos(i), 0.2f);
			commandIcons_[i].transform.localScale = Vector3.Lerp(commandIcons_[i].transform.localScale, GetTargetScale(i), 0.2f);
		}

		if( AnimManager.IsAnimating(EdgeLine.gameObject) == false && commandIcons_.Count > 0 )
		{
			EdgeLine.transform.localPosition = commandIcons_[0].transform.localPosition + Vector3.forward * 10;
			EdgeLine.Length = commandIcons_[commandIcons_.Count - 1].transform.localPosition.x - commandIcons_[0].transform.localPosition.x;
		}

		if( currentIndex_ >= 0 && commandIcons_.Count > 0 && GameContext.BattleState != BattleState.Wait )
		{
			CurrentHex.transform.position = commandIcons_[currentIndex_].transform.position + Vector3.back * 10;
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
		if( index < currentIndex_ )
		{
			// 過去のコマンド
			return Vector3.left * ((currentIndex_ - index) * (OldIconScale * Interval) + CenterInterval);
		}
		else if( index == currentIndex_ )
		{
			// 現在のコマンド
			return Vector3.zero;
		}
		else// (i > currentIndex_ )
		{
			// 実行予定のコマンド
			return Vector3.right * ((index - currentIndex_) * (FutureIconScale * Interval) + CenterInterval);
		}
	}


	public void AddCommand(EnemyCommandSet command)
	{
		GameObject iconObj = Instantiate(command.IconPrefab);
		iconObj.transform.parent = this.transform;
		iconObj.transform.localPosition = Vector3.zero;
		commandIcons_.Add(iconObj);
	}

	public void ShowAnim()
	{
		for( int i = 0; i < commandIcons_.Count; ++i )
		{
			Vector3 targetPos = GetTargetPos(i);
			commandIcons_[i].transform.localPosition = targetPos + Vector3.up * 5;
			commandIcons_[i].transform.localScale = GetTargetScale(i);
			AnimManager.AddAnim(commandIcons_[i].gameObject, targetPos, ParamType.Position, AnimType.BounceIn, 0.2f, i * (float)Music.MusicalTimeUnit * 4.0f / commandIcons_.Count);
		}
		EdgeLine.transform.localPosition = commandIcons_[0].transform.localPosition;
		EdgeLine.Length = commandIcons_[commandIcons_.Count - 1].transform.localPosition.x - commandIcons_[0].transform.localPosition.x;
		EdgeLine.SetRate(0);
		AnimManager.AddAnim(EdgeLine.gameObject, 1.0f, ParamType.GaugeRate, AnimType.BounceIn, 0.2f, (float)Music.MusicalTimeUnit * 4);
	}

	public void ClearCommands()
	{
		for( int i = 0; i < commandIcons_.Count; ++i )
		{
			Vector3 targetPos = GetTargetPos(i) + Vector3.up * 5;
			AnimManager.AddAnim(commandIcons_[i].gameObject, targetPos, ParamType.Position, AnimType.BounceOut, 0.3f, i * (float)Music.MusicalTimeUnit * 4.0f / commandIcons_.Count, true);
		}
		AnimManager.AddAnim(EdgeLine.gameObject, 0.0f, ParamType.GaugeRate, AnimType.BounceOut, 0.2f, (float)Music.MusicalTimeUnit * 4);
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
			CurrentHex.SetSize(commandIcons_[currentIndex_].GetComponentInChildren<MidairPrimitive>().Radius + 0.5f);
			CurrentHex.SetWidth(commandIcons_[currentIndex_].GetComponentInChildren<MidairPrimitive>().Radius + 0.5f);
			CurrentHex.transform.localScale = Vector3.one * ActiveIconScale;
			for( int i = 0; i < currentIndex_; ++i )
			{
				MidairPrimitive mask = commandIcons_[i].GetComponentsInChildren<MidairPrimitive>().First((MidairPrimitive primitive) => primitive.name == "Mask");
				if( mask != null )
				{
					mask.SetColor(ColorManager.MakeAlpha(Color.black, 0.6f));
				}
			}
		}
	}
}
