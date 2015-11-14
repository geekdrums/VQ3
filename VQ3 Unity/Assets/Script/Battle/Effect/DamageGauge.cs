using UnityEngine;
using System.Collections;

public class DamageGauge : MonoBehaviour {

	public enum Mode
	{
		Damage,
		Break,
		Time
	}

	public GaugeRenderer HPGauge;
	public GaugeRenderer RedGauge;
	public DamageText DamageText;
	public GaugeRenderer BaseGauge;

	public GaugeRenderer BreakGauge;
	public GaugeRenderer TimeGauge;

	int damage_;
	ActionResult actionResult_;
	float time_;
	Enemy enemy_;
	Mode mode_;

	public static Mode GetDesiredMode()
	{
		if( GameContext.LuxSystem.Version >= LuxVersion.AutoShield && GameContext.LuxSystem.CurrentTime <= 16 * 4 && GameContext.LuxSystem.BreakCount.Count <= 70 )
		{
			return Mode.Time;
		}
		else if( GameContext.LuxSystem.Version >= LuxVersion.Shield && GameContext.LuxSystem.IsOverFlow == false )
		{
			return Mode.Break;
		}
		else
		{
			return Mode.Damage;
		}
	}

	void ModeInit()
	{
		HPGauge.transform.parent.gameObject.SetActive(mode_ == Mode.Damage);
		BreakGauge.transform.parent.gameObject.SetActive(mode_ == Mode.Break || mode_ == Mode.Time);
		TimeGauge.transform.parent.gameObject.SetActive(mode_ == Mode.Time || (mode_ == Mode.Break && GameContext.LuxSystem.Version >= LuxVersion.AutoShield));
		switch( mode_ )
		{
		case Mode.Damage:
			{
				DamageText.Initialize(damage_, actionResult_);
				DamageText.SignificantDigits = 0;
				DamageText.optionFlags_ = CounterSprite.Options.None;
				DamageText.GetComponentInChildren<TextMesh>().color = ColorManager.Accent.Damage;
				DamageText.GetComponentInChildren<TextMesh>().text = "";
				RedGauge.SetRate((float)(enemy_.HitPoint + damage_) / enemy_.MaxHP);
				HPGauge.SetRate(RedGauge.Rate);
				HPGauge.SetRate((float)enemy_.HitPoint / enemy_.MaxHP, 0.1f);
			}
			break;
		case Mode.Break:
			{
				DamageText.Count = GameContext.LuxSystem.BreakCount.Count;
				DamageText.SignificantDigits = 0;
				DamageText.optionFlags_ = CounterSprite.Options.Percent;
				DamageText.CounterColor = ColorManager.Accent.Break;
				DamageText.GetComponentInChildren<TextMesh>().color = ColorManager.Accent.Break;
				DamageText.GetComponentInChildren<TextMesh>().text = "BREAK";
				BreakGauge.SetRate(GameContext.LuxSystem.BreakGauge.Rate);
				TimeGauge.SetRate(GameContext.LuxSystem.TimeGauge.Rate);
			}
			break;
		case Mode.Time:
			{
				DamageText.Count = GameContext.LuxSystem.TimeCount.Count;
				DamageText.SignificantDigits = 2;
				DamageText.optionFlags_ = CounterSprite.Options.None;
				DamageText.CounterColor = ColorManager.Accent.Time;
				DamageText.GetComponentInChildren<TextMesh>().color = ColorManager.Accent.Time;
				DamageText.GetComponentInChildren<TextMesh>().text = "TIME";
				BreakGauge.SetRate(GameContext.LuxSystem.BreakGauge.Rate);
				TimeGauge.SetRate(GameContext.LuxSystem.TimeGauge.Rate);
			}
			break;
		}
	}


	// Use this for initialization
	void Start()
	{
		ModeInit();
		BaseGauge.SetColor(ColorManager.Base.Light);
		HPGauge.SetColor(ColorManager.Base.Back);
	}
	
	// Update is called once per frame
	void Update () {
		time_ += Time.deltaTime;
		if( time_ >= 2.0f || Music.IsJustChangedAt(0) )
        {
            Destroy( transform.parent != null ? transform.parent.gameObject : gameObject );
		}

		Mode newMode = GetDesiredMode();
		if( mode_ != newMode )
		{
			mode_ = newMode;
			ModeInit();
		}

		switch( mode_ )
		{
		case Mode.Damage:
			{
				if( Music.IsJustChanged ) RedGauge.GetComponentInChildren<Renderer>().enabled = Music.Just.MusicalTime % 3 <= 1;
			}
			break;
		case Mode.Break:
			{
				BreakGauge.SetRate(GameContext.LuxSystem.BreakGauge.Rate);
				TimeGauge.SetRate(GameContext.LuxSystem.TimeGauge.Rate);
				DamageText.Count = GameContext.LuxSystem.BreakCount.Count;
			}
			break;
		case Mode.Time:
			{
				BreakGauge.SetRate(GameContext.LuxSystem.BreakGauge.Rate);
				TimeGauge.SetRate(GameContext.LuxSystem.TimeGauge.Rate);
				DamageText.Count = GameContext.LuxSystem.TimeCount.Count;
			}
			break;
		}
	}

	public void AddDamage(int damage)
	{
		time_ = 0;

		Mode newMode = GetDesiredMode();
		if( mode_ != newMode )
		{
			mode_ = newMode;
			ModeInit();
		}

		if( mode_ == Mode.Damage )
		{
			damage_ += damage;
			DamageText.AddDamage(damage);
			HPGauge.SetRate((float)enemy_.HitPoint / enemy_.MaxHP, 0.1f);
		}
		else
		{
			DamageText.AddDamage(0);
		}
	}

	public void Initialize(Enemy enemy, int damage, ActionResult actionResult, GameObject parent)
	{
		time_ = 0;
		enemy_ = enemy;
		damage_ = damage;
		actionResult_ = actionResult;
		mode_ = GetDesiredMode();

		if( parent != null )
		{
			Vector3 scale = parent.transform.localScale;
			parent.transform.parent = null;
			parent.transform.localScale = scale;
			parent.transform.position = new Vector3(parent.transform.position.x, parent.transform.position.y, 5);
			transform.parent = parent.transform;
			transform.localPosition = Vector3.zero;
			transform.localScale = Vector3.one;
			transform.localRotation = Quaternion.identity;
		}
		else
		{
			transform.position = enemy.transform.position + Vector3.down * 6;
		}
    }
}
