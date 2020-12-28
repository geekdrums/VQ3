using UnityEngine;
using System.Collections;

public class DamageGauge : MonoBehaviour
{
	public GaugeRenderer HPGauge;
	public GaugeRenderer RedGauge;
	public GaugeRenderer HPBaseGauge;
	public TextMesh EnemyText;

	public Enemy Enemy { get; private set; }

	int damage_;
	bool isInitialized_ = false;

	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		if( Music.IsJustChanged )
		{
			RedGauge.GetComponentInChildren<Renderer>().enabled = Music.JustTotalUnits % 3 <= 1;
		}
	}

	public void AddDamage(int damage)
	{
		damage_ += damage;
		HPGauge.AnimateRate((float)Enemy.HitPoint / Enemy.MaxHP, time: 0.1f);
	}

	public void TurnInit()
	{
		if( Enemy != null )
		{
			damage_ = 0;
			HPGauge.SetRate((float)Enemy.HitPoint / Enemy.MaxHP);
			RedGauge.SetRate(HPGauge.Rate);
		}
	}

	public void InitializeDamage(Enemy enemy, int damage, Vector3 position)
	{
		isInitialized_ = true;
		Enemy = enemy;
		damage_ = damage;

		//HPBaseGauge.SetColor(ColorManagerObsolete.Base.Light);
		//HPGauge.SetColor(ColorManagerObsolete.Base.Bright);
		RedGauge.SetRate((float)(Enemy.HitPoint + damage_) / Enemy.MaxHP);
		HPGauge.SetRate(RedGauge.Rate);
		HPGauge.AnimateRate((float)Enemy.HitPoint / Enemy.MaxHP, time: 0.1f);
		
		float delay = (float)Music.Meter.SecPerUnit * 24;
		float animTime = 0.2f;

		Vector3 initialScale = EnemyText.transform.localScale;
		EnemyText.text = Enemy.DisplayName;
		EnemyText.transform.localScale = Vector3.zero;
		EnemyText.transform.AnimateScale(initialScale.x, time: 0.0f, delay: delay + animTime);
	}
}
