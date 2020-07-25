using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CommandListUI : MonoBehaviour
{
	public int MaxCommands;
	public float OldIconScale;
	public float ActiveIconScale;
	public float FutureIconScale;
	public float Interval;
	public float IconLength;
	public float GaugeHeight;
	List<GameObject> commandIcons_ = new List<GameObject>();

	//public GaugeRenderer CurrentGauge;
	//public GaugeRenderer CurrentGaugeBase;
	public GaugeRenderer EdgeLine;

	int currentIndex_;

	void Start()
	{
	}

	void Update()
	{
		Vector3 targetPos = Vector3.zero;
		for( int i = 0; i < commandIcons_.Count; ++i )
		{
			if( i < currentIndex_ )
			{
				// 実行予定のコマンド
				commandIcons_[i].transform.localPosition = Vector3.Lerp(commandIcons_[i].transform.localPosition, targetPos, 0.2f);
				commandIcons_[i].transform.localScale = Vector3.Lerp(commandIcons_[i].transform.localScale, Vector3.one * FutureIconScale, 0.2f);
				targetPos += Vector3.left * (FutureIconScale * IconLength + Interval);
			}
			else if( i == currentIndex_ )
			{
				// 現在のコマンド
				commandIcons_[i].transform.localPosition = Vector3.Lerp(commandIcons_[i].transform.localPosition, targetPos, 0.2f);
				commandIcons_[i].transform.localScale = Vector3.Lerp(commandIcons_[i].transform.localScale, Vector3.one * ActiveIconScale, 0.2f);
				//CurrentGauge.transform.localPosition = Vector3.Lerp(CurrentGauge.transform.localPosition, targetPos + Vector3.up * GaugeHeight + Vector3.left * CurrentGaugeBase.Length/2, 0.2f);
				//CurrentGauge.SetRate((float)(1.0f - Music.MusicalTime / LuxSystem.TurnMusicalUnits));
				targetPos += Vector3.left * (ActiveIconScale * IconLength + Interval);
			}
			else// (i > currentIndex_ )
			{
				// 過去のコマンド
				commandIcons_[i].transform.localPosition = Vector3.Lerp(commandIcons_[i].transform.localPosition, targetPos, 0.2f);
				commandIcons_[i].transform.localScale = Vector3.Lerp(commandIcons_[i].transform.localScale, Vector3.one * OldIconScale, 0.2f);
				targetPos += Vector3.left * (OldIconScale * IconLength + Interval);
			}
		}
		EdgeLine.transform.localPosition = commandIcons_[commandIcons_.Count - 1].transform.localPosition;
		EdgeLine.Length = commandIcons_[0].transform.localPosition.x - commandIcons_[commandIcons_.Count - 1].transform.localPosition.x;
	}


	public void AddCommand(PlayerCommand command)
	{
		GameObject iconObj = command.InstantiateIconObj(this.gameObject);
		commandIcons_.Insert(0, iconObj);
		//commandIcons_[0].GetComponent<PlayerCommand>().maskPlane.SetActive(false);
		//commandIcons_[0].GetComponent<PlayerCommand>().enabled = false;
		while( commandIcons_.Count > MaxCommands )
		{
			Destroy(commandIcons_[commandIcons_.Count - 1].gameObject);
			commandIcons_.RemoveAt(commandIcons_.Count - 1);
		}
		++currentIndex_;
	}

	public void DeleteCommand()
	{
		Destroy(commandIcons_[0].gameObject);
		commandIcons_.RemoveAt(0);
		--currentIndex_;
	}

	public void OnExecCommand()
	{
		--currentIndex_;
		//CurrentGauge.SetColor(ColorManager.GetThemeColor(commandIcons_[currentIndex_].GetComponent<PlayerCommand>().themeColor).Bright);
		//CurrentGaugeBase.SetColor(ColorManager.Base.Shade);
		EdgeLine.SetColor(ColorManagerObsolete.Base.Bright);

		for( int i = currentIndex_ + 1; i < commandIcons_.Count; ++i )
		{
			//commandIcons_[i].GetComponent<PlayerCommand>().maskPlane.SetActive(true);
			//commandIcons_[i].GetComponent<PlayerCommand>().maskPlane.GetComponent<Renderer>().material.color = ColorManagerObsolete.MakeAlpha(Color.black, 0.6f);
		}
	}
}
