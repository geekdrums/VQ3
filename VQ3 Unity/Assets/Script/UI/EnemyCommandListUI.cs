using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class EnemyCommandListUI : MonoBehaviour
{
	public float OldIconScale = 0.12f;
	public float ActiveIconScale = 0.22f;
	public float FutureIconScale = 0.17f;
	public float Interval = 2.2f;
	//public float IconLength = 4.0f;
	public GaugeRenderer EdgeLine;

	List<GameObject> commandIcons_ = new List<GameObject>();
	int currentIndex_;

	void Start()
	{
	}

	void Update()
	{
		for( int i = 0; i < commandIcons_.Count; ++i )
		{
			if( i < currentIndex_ )
			{
				// 過去のコマンド
				Vector3 targetPos = Vector3.left * (currentIndex_ - i) * (OldIconScale * Interval);
				commandIcons_[i].transform.localPosition = Vector3.Lerp(commandIcons_[i].transform.localPosition, targetPos, 0.2f);
				commandIcons_[i].transform.localScale = Vector3.Lerp(commandIcons_[i].transform.localScale, Vector3.one * OldIconScale, 0.2f);
			}
			else if( i == currentIndex_ )
			{
				// 現在のコマンド
				commandIcons_[i].transform.localPosition = Vector3.Lerp(commandIcons_[i].transform.localPosition, Vector3.zero, 0.2f);
				commandIcons_[i].transform.localScale = Vector3.Lerp(commandIcons_[i].transform.localScale, Vector3.one * ActiveIconScale, 0.2f);
				//CurrentGauge.SetRate((float)(1.0f - Music.MusicalTime / LuxSystem.TurnMusicalUnits));
			}
			else// (i > currentIndex_ )
			{
				// 実行予定のコマンド
				Vector3 targetPos = Vector3.right * (i - currentIndex_) * (FutureIconScale * Interval);
				commandIcons_[i].transform.localPosition = Vector3.Lerp(commandIcons_[i].transform.localPosition, targetPos, 0.2f);
				commandIcons_[i].transform.localScale = Vector3.Lerp(commandIcons_[i].transform.localScale, Vector3.one * FutureIconScale, 0.2f);
			}
		}
		EdgeLine.transform.localPosition = commandIcons_[0].transform.localPosition;
		EdgeLine.Length = commandIcons_[commandIcons_.Count - 1].transform.localPosition.x - commandIcons_[0].transform.localPosition.x;
	}


	public void AddCommand(EnemyCommandSet command)
	{
		GameObject iconObj = Instantiate(command.IconPrefab);
		iconObj.transform.parent = this.transform;
		iconObj.transform.localPosition = Vector3.zero;
		commandIcons_.Add(iconObj);
	}

	public void ClearCommands()
	{
		foreach(GameObject command in commandIcons_)
		{
			Destroy(command.gameObject);
		}
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
