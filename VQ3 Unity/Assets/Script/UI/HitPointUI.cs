using UnityEngine;
using System.Collections;

public class HitPointUI : MonoBehaviour {
    public Player Player;

    public GaugeRenderer CurrentBar;
    public GaugeRenderer DamageBar;
    public CounterSprite CurrentHPCount;

    int turnStartHP_;
    PlayerCommand currentCommand_;

    float targetRate { get { return (float)Player.HitPoint / (float)Player.MaxHP; } }

	// Use this for initialization
    void Start()
    {
        turnStartHP_ = Player.HitPoint;
	}
	
	// Update is called once per frame
    void Update()
    {
		if( Music.IsJustChanged ) DamageBar.gameObject.SetActive(Music.JustTotalUnits % 3 <= 1);
	}


	public void OnBattleStart()
	{
		turnStartHP_ = Player.HitPoint;
		CurrentBar.AnimateRate(1.0f, time: 0.1f, interpType: InterpType.QuadOut).From(0.0f);
		UpdateHPText();
	}
    public void OnTurnStart( PlayerCommand command )
    {
        currentCommand_ = command;
        UpdateHPText();
        turnStartHP_ = Player.HitPoint;
		CurrentBar.SetRate(targetRate);
		DamageBar.SetRate(targetRate);
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
