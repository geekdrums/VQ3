using UnityEngine;
using System.Collections;

public class VPGauge : MonoBehaviour
{
	public GaugeRenderer BreakGauge;
	public GaugeRenderer TimeGauge;
	public CounterSprite VPCount;
	public CounterSprite MaxVPCount;
	public CounterSprite VTCount;
	public AnimComponent InitAnimation;
	public AnimComponent RecoverAnimation;
	public GameObject CounterParent;
	
	bool isInitialized_ = false;

	public bool GetIsInitialized() { return isInitialized_; }

	// Use this for initialization
	void Start()
	{
		gameObject.SetActive(false);
	}

	// Update is called once per frame
	void Update()
	{
		UpdateGaugeAndCount();
	}

	public void InitializeVPVT(Vector3 position)
	{
		if( isInitialized_ == false )
		{
			gameObject.SetActive(true);
			CounterParent.SetActive(false);
			InitAnimation.transform.position = position;
			InitAnimation.Play();
			isInitialized_ = true;
		}

		UpdateGaugeAndCount();
	}

	void UpdateGaugeAndCount()
	{
		float BreakRate = GameContext.LuxSystem.VPRate;
		BreakGauge.SetRate(Mathf.Clamp01(1.0f - BreakRate));
		TimeGauge.SetRate(Mathf.Clamp01(BreakRate * GameContext.LuxSystem.VTRate));
		VPCount.Count = GameContext.LuxSystem.OverflowVP - GameContext.LuxSystem.CurrentVP;
		VTCount.Count = GameContext.LuxSystem.CurrentTime / LuxSystem.TurnMusicalUnits;
	}

	public void OnBattleStarted()
	{
		MaxVPCount.Count = GameContext.LuxSystem.OverflowVP;
	}

	public void OnShieldRecover()
	{
		isInitialized_ = false;
		if( InitAnimation.State == AnimComponent.AnimState.Playing )
		{
			gameObject.SetActive(false);
		}
		else
		{
			RecoverAnimation.Play();
		}
	}
}
