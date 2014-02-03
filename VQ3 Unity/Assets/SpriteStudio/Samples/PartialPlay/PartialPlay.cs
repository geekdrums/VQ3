using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PartialPlay : MonoBehaviour
{
	public struct StartEndFrame
	{
		public	int	start;	// 開始フレーム
		public	int	end;	// 終了フレーム
	};

	// ユーザーデータに格納されているはずのラベルのリスト
	string[]	labels = new string[] {"first", "second", "third"};

	Dictionary<string, StartEndFrame>	labelToFrame = new Dictionary<string, StartEndFrame>();
	SsSprite sprite;

	// Use this for initialization
	void Start ()
	{
		sprite = GetComponent<SsSprite>();

		// ０番＝ルートパーツを参照する
		SsPart part = sprite.GetPart(0);
		
		// ルートパーツが持つ全ユーザーデータキーの文字列とフレーム位置のペアで辞書を作る
		SsPartRes res = part._res;
		int endFrame = res.FrameNum - 1;
		
		// 末尾キーから先頭キーに向かって遡る
		for (int i = res.UserKeys.Count - 1; i >= 0; --i)
		{
			SsUserDataKeyFrame userDataKey = (SsUserDataKeyFrame)res.GetKey(SsKeyAttr.User, i);
			
			// 文字列が格納されていない場合は無視する
			if (!userDataKey.Value.IsString) continue;
			
			var se = new StartEndFrame();
				
			// １つ右のキーフレーム、または全フレーム数－１を終了フレームとする
			se.end = endFrame;

			// "文字列"領域に書かれた文字列をキーにして開始・終了位置フレームを登録する
			se.start = userDataKey.Time;
			labelToFrame[userDataKey.Value.String] = se;

			// このキーフレームの位置－１が１つ左の区間の終了フレームになる
			endFrame = userDataKey.Time - 1;
		}
		
		// 登録したラベルと区間を列挙する
		Debug.Log("User data keys...");
		foreach (var e in labelToFrame)
		{
			Debug.Log("[" + e.Key + "] start:" + e.Value.start + " end:" + e.Value.end);
		}
	}
	
	int index = 0;

	// Update is called once per frame
	void Update ()
	{
		if (Input.GetButtonDown("Fire1"))
		{
			// 左クリックで次の区間に移る
			Debug.Log("PLAY " + labels[index]);
			StartEndFrame se = labelToFrame[labels[index]];
			sprite.SetStartEndFrame(se.start, se.end);
			sprite.AnimFrame = se.start;
			index = (index + 1) % labels.Length;
		}
	}
}
