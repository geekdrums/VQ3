using UnityEngine;
using System.Collections;

public class MemoryResult : MonoBehaviour {

	public enum Phase
	{
		None,
		Anim,
		Gauge,
		Wait
	}

	public CounterSprite AcquiredMemory;
	public GaugeRenderer Gauge;
	public CounterSprite StartMemory;
	public CounterSprite EndMemory;

	public Phase CurrentPhase { get; private set; }
	public bool IsLevelUp { get; private set; }

	float targetRate_;
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
				CurrentPhase = Phase.Gauge;
				Gauge.SetRate(targetRate_, 0.3f);
			}
			break;
		case Phase.Gauge:
			if( Gauge.Rate >= targetRate_ )
			{
				CurrentPhase = Phase.Wait;
				animTime_ = 0;
				TextWindow.SetNextCursor(true);
			}
			break;
		case Phase.Wait:
			if( IsLevelUp )
			{
				animTime_ += Time.deltaTime;
				Gauge.SetColor(Color.Lerp(ColorManager.Base.Bright, ColorManager.Base.Light, (Mathf.Cos(animTime_*8) + 1)/2));
			}
			break;
		}
	}


	public void Show(int memory)
	{
		gameObject.SetActive(true);
		AcquiredMemory.Count = memory;
		StartMemory.Count = GameContext.PlayerConductor.LevelInfoList[GameContext.PlayerConductor.Level].NeedMemory;
		EndMemory.Count = GameContext.PlayerConductor.LevelInfoList[GameContext.PlayerConductor.Level + 1].NeedMemory;
		targetRate_ = Mathf.Min(1.0f, (float)(GameContext.PlayerConductor.TotalMemory - StartMemory.Count)/(float)(EndMemory.Count - StartMemory.Count));
		Gauge.Rate = (float)(GameContext.PlayerConductor.TotalMemory - StartMemory.Count - memory)/(float)(EndMemory.Count - StartMemory.Count);
		IsLevelUp = GameContext.PlayerConductor.TotalMemory >= EndMemory.Count;
		CurrentPhase = MemoryResult.Phase.Anim;
		GetComponent<Animation>().Play("ResultMemoryAnim");
		TextWindow.SetMessage(MessageCategory.Result, "メモリーを獲得。");
		TextWindow.SetNextCursor(false);
	}

	public void Hide()
	{
		gameObject.SetActive(false);
		CurrentPhase = Phase.None;
	}
}
