using UnityEngine;
using System.Collections;

public class LevelResult : MonoBehaviour {

	[System.Serializable]
	public class LevelParamCounts
	{
		public CounterSprite Level;
		public CounterSprite HP;
		public CounterSprite ATK;
		public CounterSprite MAG;
	}

	public LevelParamCounts Before;
	public LevelParamCounts After;

	public enum Phase
	{
		None,
		Anim,
		Wait
	}

	public Phase CurrentPhase { get; private set; }

	float animTime_;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		switch( CurrentPhase )
		{
		case Phase.Anim:
			if( GetComponent<Animation>().isPlaying == false )
			{
				CurrentPhase = Phase.Wait;
				animTime_ = 0;
			}
			break;
		case Phase.Wait:
			animTime_ += Time.deltaTime;
			After.Level.CounterColor = (Color.Lerp(ColorManager.Base.Bright, ColorManager.Base.Light, (Mathf.Cos(animTime_*8) + 1)/2));
			break;
		}
	}

	public void Show()
	{
		gameObject.SetActive(true);
		PlayerConductor.LevelInfo before = GameContext.PlayerConductor.LevelInfoList[GameContext.PlayerConductor.Level-1];
		PlayerConductor.LevelInfo after = GameContext.PlayerConductor.LevelInfoList[GameContext.PlayerConductor.Level];
		Before.Level.Count = GameContext.PlayerConductor.Level-1;
		After.Level.Count = GameContext.PlayerConductor.Level;
		Before.HP.Count = before.HP;
		After.HP.Count = after.HP;
		Before.ATK.Count = before.Attack;
		After.ATK.Count = after.Attack;
		Before.MAG.Count = before.Magic;
		After.MAG.Count = after.Magic;
		CurrentPhase = Phase.Anim;
		GetComponent<Animation>().Play("ResultLevelAnim");
	}

	public void Hide()
	{
		gameObject.SetActive(false);
		CurrentPhase = Phase.None;
	}
}
