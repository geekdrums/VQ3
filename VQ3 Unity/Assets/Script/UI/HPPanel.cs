using UnityEngine;
using System.Collections;

public class HPPanel : MonoBehaviour {
    public Player Player;

    public GaugeRenderer CurrentBar;
    public GaugeRenderer DamageBar;
    public CounterSprite CurrentHPCount;
    public CounterSprite MaxHPCount;

    int turnStartHP_;
    PlayerCommand currentCommand_;

    float targetRate { get { return (float)Player.HitPoint / (float)Player.MaxHP; } }

	// Use this for initialization
    void Start()
    {
        turnStartHP_ = Player.HitPoint;
        MaxHPCount.Count = Player.MaxHP;
	}
	
	// Update is called once per frame
    void Update()
    {
		if( Music.IsJustChanged ) DamageBar.gameObject.SetActive(Music.JustTotalUnits % 3 <= 1);
	}


	public void OnBattleStart()
	{
		turnStartHP_ = Player.HitPoint;
		MaxHPCount.Count = Player.MaxHP;
		CurrentBar.SetColor(ColorManagerObsolete.Base.Front);
		CurrentBar.SetRate(1);
		UpdateHPText();

		CurrentBar.transform.parent.localScale = new Vector3(0, 1, 1);
		float mtu = (float)Music.Meter.SecPerUnit;
		CurrentBar.transform.parent.AnimateScaleX(1.0f, time: 0.1f, delay: mtu * 16);// AnimType.Linear
	}
    public void OnTurnStart( PlayerCommand command )
    {
        currentCommand_ = command;
        UpdateHPText();
        turnStartHP_ = Player.HitPoint;
		CurrentBar.SetRate(targetRate);
		DamageBar.SetRate(targetRate);
		
		if( currentCommand_.currentData.HealPercent > 0 )
        {
            MaxHPCount.CounterColor = ColorManagerObsolete.Theme.Light;
            CurrentBar.SetColor( ColorManagerObsolete.Theme.Light );
        }
        else
        {
            MaxHPCount.CounterColor = ColorManagerObsolete.Base.Front;
            CurrentBar.SetColor( ColorManagerObsolete.Base.Front );
        }
    }
    public void OnDamage( int damage )
    {
        UpdateHPText();
		CurrentBar.AnimateRate(targetRate, time: 0.1f);
    }
    public void OnHeal( int heal )
    {
        UpdateHPText();
		CurrentBar.AnimateRate(targetRate, time: 0.1f);
        if( Player.HitPoint >= turnStartHP_ )
        {
            turnStartHP_ = Player.HitPoint;
			DamageBar.SetRate(targetRate);
        }
    }
    public void OnUpdateHP()
    {
        UpdateHPText();
		CurrentBar.SetRate(targetRate);
        if( Player.HitPoint >= turnStartHP_ )
        {
            turnStartHP_ = Player.HitPoint;
			DamageBar.SetRate(targetRate);
        }
    }

    void UpdateHPText()
    {
        CurrentHPCount.Count = Player.HitPoint;
    }
}
