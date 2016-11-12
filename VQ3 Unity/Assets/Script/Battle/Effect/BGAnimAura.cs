using UnityEngine;
using System.Collections;

public class BGAnimAura : BGAnimBase {

	public float RateAnimTime = 0.1f;
	public float WidthAnimTime = 0.2f;
	public float RandomRange = 6.0f;
	public float AccentWidth = 0.3f;
	public float DefaultWidth = 0.1f;

	private GaugeRenderer[] gauges_;
	private Vector3 invisibleScale_ = new Vector3(0, 1, 1);
	private int animIndex_ = 0;

	// Use this for initialization
	protected override void Start()
	{
		gauges_ = GetComponentsInChildren<GaugeRenderer>();
		transform.localScale = invisibleScale_;
	}
	
	// Update is called once per frame
	protected override void Update()
	{
		if( IsActive == false )
		{
			transform.localScale = Vector3.Lerp(transform.localScale, invisibleScale_, 0.2f);
			if( transform.localScale.x < 0.1f )
			{
				transform.localScale = invisibleScale_;
				gameObject.SetActive(false);
			}
			return;
		}
		
		UpdateDamageAnim();

		if( GameContext.BattleState == BattleState.Battle )
		{
			transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, 0.2f);
			if( Music.IsJustChanged )
			{
				int mt = Music.Just.MusicalTime % 5;
				gauges_[animIndex_].SetRate(0);
				switch( mt )
				{
				case 0:
					gauges_[animIndex_].SetColor(ColorManager.Theme.Bright);
					gauges_[animIndex_].SetWidth(AccentWidth);
					break;
				case 2:
					gauges_[animIndex_].SetColor(ColorManager.Theme.Shade);
					gauges_[animIndex_].SetWidth(DefaultWidth);
					break;
				case 3:
					gauges_[animIndex_].SetColor(ColorManager.Theme.Shade);
					gauges_[animIndex_].SetWidth(DefaultWidth);
					break;
				default:
					return;
				}
				gauges_[animIndex_].transform.localPosition = new Vector3(-40, Random.Range(-RandomRange, RandomRange), 0);
				gauges_[animIndex_].SetRate(1, RateAnimTime);
				gauges_[animIndex_].SetWidth(0, WidthAnimTime);
				animIndex_ = (animIndex_ + 1) % gauges_.Length;
			}
		}
	}
}
