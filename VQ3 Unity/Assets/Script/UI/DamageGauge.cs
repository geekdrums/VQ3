using UnityEngine;
using System.Collections;

public class DamageGauge : MonoBehaviour
{
	public enum Mode
	{
		Damage,
		Break,
		DamageAndTime,
	}

	public GaugeRenderer BreakGauge;
	public GaugeRenderer BreakBaseGauge;
	public GaugeRenderer TimeGauge;
	public CounterSprite VPCount;
	public CounterSprite MaxVPCount;
	public CounterSprite VTCount;
	public GaugeRenderer TextBase;
	public TextMesh ShieldText;

	public GaugeRenderer HPGauge;
	public GaugeRenderer RedGauge;
	public GaugeRenderer HPBaseGauge;
	public GaugeRenderer TimeGauge2;
	public CounterSprite VTCount2;
	public GaugeRenderer TextBase2;
	public TextMesh EnemyText;

	public GaugeRenderer[] Splits;

	public Enemy Enemy { get; private set; }
	public Mode CurrentMode { get; private set; }

	int damage_;

	public static Mode GetDesiredMode()
	{
		if( GameContext.LuxSystem.Version >= LuxVersion.AutoShield && GameContext.LuxSystem.IsOverFlow )
		{
			return Mode.DamageAndTime;
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
		HPGauge.transform.parent.gameObject.SetActive(CurrentMode == Mode.Damage || CurrentMode == Mode.DamageAndTime);
		BreakGauge.transform.parent.gameObject.SetActive(CurrentMode == Mode.Break);
		TimeGauge2.transform.parent.gameObject.SetActive(CurrentMode == Mode.DamageAndTime);
		switch( CurrentMode )
		{
		case Mode.Damage:
			{
				RedGauge.SetRate((float)(Enemy.HitPoint + damage_) / Enemy.MaxHP);
				HPGauge.SetRate(RedGauge.Rate);
				HPGauge.SetRate((float)Enemy.HitPoint / Enemy.MaxHP, 0.1f);
			}
			break;
		case Mode.Break:
			break;
		case Mode.DamageAndTime:
			break;
		}
	}


	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		Mode newMode = GetDesiredMode();
		if( CurrentMode != newMode )
		{
			CurrentMode = newMode;
			ModeInit();
		}

		switch( CurrentMode )
		{
		case Mode.Damage:
			{
				if( Music.IsJustChanged ) RedGauge.GetComponentInChildren<Renderer>().enabled = Music.Just.MusicalTime % 3 <= 1;
			}
			break;
		case Mode.Break:
			{
				float BreakRate = GameContext.LuxSystem.VPRate;
				BreakGauge.SetRate(1.0f - BreakRate);
				TimeGauge.SetRate(BreakRate * GameContext.LuxSystem.VTRate);
				VPCount.Count = GameContext.LuxSystem.OverflowVP - GameContext.LuxSystem.CurrentVP;
				VTCount.Count = GameContext.LuxSystem.CurrentTime / LuxSystem.TurnMusicalUnits;
				BreakGauge.SetColor(Color.Lerp(BreakGauge.LineColor, BreakRate > 0.0f ? Color.white : ColorManager.Accent.Time, 0.2f));
				Color timeColor = GetTimeColor();
				TimeGauge.SetColor(timeColor);
				VTCount.CounterColor = timeColor;
			}
			break;
		case Mode.DamageAndTime:
			{
				TimeGauge2.SetRate(GameContext.LuxSystem.VTRate);
				VTCount2.Count = GameContext.LuxSystem.CurrentTime / LuxSystem.TurnMusicalUnits;
				Color timeColor = GetTimeColor();
				TimeGauge2.SetColor(timeColor);
				VTCount2.CounterColor = timeColor;

				if( Music.IsJustChanged ) RedGauge.GetComponentInChildren<Renderer>().enabled = Music.Just.MusicalTime % 3 <= 1;
			}
			break;
		}
	}

	public void AddDamage(int damage, ActionResult actionResult)
	{
		Mode newMode = GetDesiredMode();
		if( CurrentMode != newMode )
		{
			CurrentMode = newMode;
			ModeInit();
		}

		if( CurrentMode == Mode.Damage || CurrentMode == Mode.DamageAndTime )
		{
			damage_ += damage;
			HPGauge.SetRate((float)Enemy.HitPoint / Enemy.MaxHP, 0.1f);
		}
	}

	public void TurnInit()
	{
		if( Enemy != null )
		{
			RedGauge.SetRate((float)(Enemy.HitPoint + damage_) / Enemy.MaxHP);
			HPGauge.SetRate(RedGauge.Rate);
			HPGauge.SetRate((float)Enemy.HitPoint / Enemy.MaxHP, 0.1f);
		}
	}

	public void Initialize(Enemy enemy, int damage, ActionResult actionResult, Vector3 position)
	{
		Enemy = enemy;
		damage_ = damage;
		CurrentMode = GetDesiredMode();
		ModeInit();

		HPBaseGauge.SetColor(ColorManager.Base.Light);
		HPGauge.SetColor(ColorManager.Base.Bright);
		RedGauge.SetRate((float)(Enemy.HitPoint + damage_) / Enemy.MaxHP);
		HPGauge.SetRate(RedGauge.Rate);
		HPGauge.SetRate((float)Enemy.HitPoint / Enemy.MaxHP, 0.1f);

		Vector3 initialPosition_ = transform.position;
		transform.position = position;
		transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 5.0f);

		float delay = 2.0f;
		float animTime = 0.2f;
		float animTime2 = 0.4f;
		AnimManager.AddAnim(gameObject, initialPosition_, ParamType.Position, AnimType.Time, animTime, delay);
		TextBase2.SetRate(0);
		AnimManager.AddAnim(TextBase2.gameObject, 1.0f, ParamType.GaugeRate, AnimType.BounceIn, animTime2, delay + animTime);
		foreach( GaugeRenderer split in Splits )
		{
			split.SetRate(0);
			AnimManager.AddAnim(split.gameObject, 1.0f, ParamType.GaugeRate, AnimType.BounceIn, animTime2, delay + animTime);
		}
		TimeGauge2.transform.parent.localScale = Vector3.zero;
		AnimManager.AddAnim(TimeGauge2.transform.parent.gameObject, Vector3.one, ParamType.Scale, AnimType.Time, 0.0f, delay + animTime);
		Vector3 initialScale = EnemyText.transform.localScale;
		EnemyText.text = Enemy.DisplayName;
		EnemyText.transform.localScale = Vector3.zero;
		AnimManager.AddAnim(EnemyText.gameObject, initialScale, ParamType.Scale, AnimType.Time, 0.0f, delay + animTime);
	}

	public void OnBattleStarted()
	{
		MaxVPCount.Count = GameContext.LuxSystem.OverflowVP;
		CurrentMode = GetDesiredMode();
		ModeInit();
	}

	private Color GetTimeColor()
	{
		Color timeColor = Color.white;
		float time = GameContext.LuxSystem.LastMaxTime / LuxSystem.TurnMusicalUnits;
		if( GameContext.LuxSystem.VPRate <= 0.0f )
		{
			timeColor = Color.white;
		}
		else if( time < 1.0f )
		{
			timeColor = Color.Lerp(ColorManager.GetThemeColor(EThemeColor.Red).Bright, ColorManager.GetThemeColor(EThemeColor.Yellow).Bright, time);
		}
		else if( time < 2.0f )
		{
			timeColor = Color.Lerp(ColorManager.GetThemeColor(EThemeColor.Yellow).Bright, ColorManager.GetThemeColor(EThemeColor.Green).Bright, time - 1.0f);
		}
		else if( time < 3.0f )
		{
			timeColor = Color.Lerp(ColorManager.GetThemeColor(EThemeColor.Green).Bright, ColorManager.GetThemeColor(EThemeColor.Blue).Bright, time - 2.0f);
		}
		else
		{
			timeColor = ColorManager.GetThemeColor(EThemeColor.Blue).Bright;
		}
		return timeColor;
	}
}
