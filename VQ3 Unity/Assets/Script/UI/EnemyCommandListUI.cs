using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class EnemyCommandListUI : MonoBehaviour
{
	public float CenterInterval;
	public float Interval;
	public float PastIconScale;
	public float FutureIconScale;
	public int MaxPastCommands = 2;
	public int MaxFutureCommands = 2;
	
	List<GameObject> futureCommandIcons_ = new List<GameObject>();
	List<GameObject> pastCommandIcons_ = new List<GameObject>();
	GameObject currentCommandIcon_;

	void Start()
	{
	}

	void Update()
	{
	}

	Vector3 GetTargetPos(bool isFutureCommand, int index)
	{
		List<GameObject> targetList = isFutureCommand ? futureCommandIcons_ : pastCommandIcons_;
		Vector3 direction = isFutureCommand ? Vector3.right : Vector3.left;
		float iconScale = isFutureCommand ? FutureIconScale : PastIconScale;
		
		Vector3 res = Vector3.zero;
		res += direction * CenterInterval;
		for( int i = 0; i < index; ++i )
		{
			res += direction * (iconScale * targetList[i].GetComponentInChildren<MidairPrimitive>().Radius / 3.0f * Interval);
		}
		float selfLength = targetList[index].GetComponentInChildren<MidairPrimitive>().Radius / 3.0f;
		res -= direction * selfLength;

		return res;
	}

	/// <summary>
	/// 実行予定のリストにコマンドを追加
	/// </summary>
	/// <param name="command"></param>
	public void AddFutureCommand(EnemyCommandSet command)
	{
		GameObject iconObj = Instantiate(command.IconPrefab);
		futureCommandIcons_.Add(iconObj);
		iconObj.transform.parent = this.transform;
		iconObj.transform.localPosition = GetTargetPos(isFutureCommand: true, futureCommandIcons_.Count - 1);
		iconObj.transform.localScale = FutureIconScale * Vector3.one;
	}

	public void Show()
	{
		for( int i = 0; i < futureCommandIcons_.Count; ++i )
		{
			Vector3 targetPos = GetTargetPos(isFutureCommand: true, i);
			futureCommandIcons_[i].transform.AnimatePosition(
				targetPos,
				InterpType.BackOut,
				time: 0.2f,
				delay: i * (float)Music.Meter.SecPerUnit * 4.0f / futureCommandIcons_.Count)
				.From(targetPos + Vector3.up * 5);
		}
	}

	public void ClearFutureCommands()
	{
		for( int i = 0; i < futureCommandIcons_.Count; ++i )
		{
			Vector3 targetPos = GetTargetPos(isFutureCommand: true, i) + Vector3.up * 5;
			futureCommandIcons_[i].transform.AnimatePosition(
				targetPos,
				InterpType.BackOut,
				time: 0.3f, 
				delay: i * (float)Music.Meter.SecPerUnit * 4.0f / futureCommandIcons_.Count,
				endOption: AnimEndOption.Destroy);
		}
		futureCommandIcons_.Clear();
	}

	public void OnExecCommand()
	{
		if( currentCommandIcon_ != null )
		{
			currentCommandIcon_.SetActive(true);
			currentCommandIcon_.transform.localPosition = CenterInterval * Vector3.left;
			currentCommandIcon_.transform.localScale = PastIconScale * Vector3.one;
			pastCommandIcons_.Insert(0, currentCommandIcon_);
			if( pastCommandIcons_.Count > MaxPastCommands )
			{
				Destroy(pastCommandIcons_[MaxPastCommands]);
				pastCommandIcons_.RemoveAt(MaxPastCommands);
			}
		}

		if( futureCommandIcons_.Count == 0 )
		{
			return;
		}

		currentCommandIcon_ = futureCommandIcons_[0];
		currentCommandIcon_.SetActive(false);
		futureCommandIcons_.RemoveAt(0);

		for( int i = 0; i < futureCommandIcons_.Count; ++i )
		{
			futureCommandIcons_[i].transform.AnimatePosition(GetTargetPos(isFutureCommand: true, i), InterpType.QuadOut, time: 0.2f);
		}
		for( int i = 0; i < pastCommandIcons_.Count; ++i )
		{
			pastCommandIcons_[i].transform.AnimatePosition(GetTargetPos(isFutureCommand: false, i), InterpType.QuadOut, time: 0.2f);
		}
		//TODO: カラー設定
		//currentCommandIcon_
		// todo add future
	}
}
