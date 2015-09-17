using UnityEngine;
using System.Collections;

public class HPPanel : MonoBehaviour {

    //public GameObject CurrentBar;
    //public GameObject RedBar;
    //public GameObject damageTextPrefab;
    public Player Player;

    public MidairPrimitive CurrentArc;
    public MidairPrimitive RedArc;
    public MidairPrimitive DefendCircle;
    public MidairPrimitive DamageCircle;
    public CounterSprite CurrentHPCount;
    public CounterSprite MaxHPCount;

    int turnStartHP_;
    PlayerCommand currentCommand_;

    //TextMesh hpText;
    //Vector3 initialScale;
    //Vector3 targetScale { get { return new Vector3( initialScale.x * ((float)Player.HitPoint / (float)Player.MaxHP), initialScale.y, initialScale.z ); } }
    //GameObject latestDamageText;
    float targetArcRate { get { return (float)Player.HitPoint / (float)Player.MaxHP; } }

	// Use this for initialization
    void Start()
    {
        //hpText = GetComponent<TextMesh>();
        //initialScale = CurrentBar.transform.localScale;
        turnStartHP_ = Player.HitPoint;
        MaxHPCount.Count = Player.MaxHP;
	}
	
	// Update is called once per frame
    void Update()
    {
		if( Music.IsJustChanged ) RedArc.GetComponent<Renderer>().enabled = Music.Just.MusicalTime % 3 <= 1;
	}


    public void OnBattleStart()
    {
        turnStartHP_ = Player.HitPoint;
        MaxHPCount.Count = Player.MaxHP;
		DamageCircle.SetWidth(0);
		DefendCircle.SetSize(0);
		CurrentArc.SetColor(ColorManager.Base.Front);
		CurrentArc.SetArc(1);
		UpdateHPText();
        CurrentHPCount.CounterColor = ColorManager.Base.Front;
        MaxHPCount.CounterColor = ColorManager.Base.Front;
    }
    public void OnTurnStart( PlayerCommand command )
    {
        currentCommand_ = command;
        UpdateHPText();
        turnStartHP_ = Player.HitPoint;
		CurrentArc.SetTargetArc( targetArcRate );
		RedArc.SetTargetArc( targetArcRate );

		float defend = (Player.DefendEnhParam + currentCommand_.GetDefend()) / 100.0f;
        if( defend <= 0 )
        {
            DefendCircle.SetTargetSize( 0.0f );
            DefendCircle.SetColor( ColorManager.Base.Middle );
        }
        else
        {
			DefendCircle.SetTargetSize(CurrentArc.Radius * defend);
            DefendCircle.SetColor( ColorManager.Theme.Shade );
        }
		if( currentCommand_.currentData.HealPercent > 0 )
        {
            //CurrentHPCount.CounterColor = ColorManager.Theme.Light;
            MaxHPCount.CounterColor = ColorManager.Theme.Light;
            CurrentArc.SetColor( ColorManager.Theme.Light );
        }
        else
        {
            //CurrentHPCount.CounterColor = ColorManager.Base.Front;
            MaxHPCount.CounterColor = ColorManager.Base.Front;
            CurrentArc.SetColor( ColorManager.Base.Front );
        }
        DamageCircle.SetWidth( 0 );
    }
    public void OnDamage( int damage )
    {
        UpdateHPText();
        //CurrentBar.transform.localScale = targetScale;
        CurrentArc.SetTargetArc( targetArcRate );
		DamageCircle.SetAnimationSize(DefendCircle.Radius / 2, CurrentArc.Radius);
		DamageCircle.SetAnimationWidth(CurrentArc.Radius, 0);

        //Vector3 nextDamageTextPosition = (latestDamageText == null ? damageTextPrefab.transform.position : latestDamageText.GetComponent<DamageText>().initialPosition + Vector3.right);
        //latestDamageText = (Instantiate( damageTextPrefab, nextDamageTextPosition, Quaternion.identity ) as GameObject);
        //latestDamageText.GetComponent<DamageText>().Initialize( damage, nextDamageTextPosition );
    }
    public void OnHeal( int heal )
    {
        UpdateHPText();
        //CurrentBar.transform.localScale = targetScale;
		CurrentArc.SetTargetArc( targetArcRate );
        if( Player.HitPoint >= turnStartHP_ )
        {
            turnStartHP_ = Player.HitPoint;
			RedArc.SetTargetArc( targetArcRate );
        }

        //Vector3 nextDamageTextPosition = (latestDamageText == null ? damageTextPrefab.transform.position : latestDamageText.transform.position + Vector3.right);
        //latestDamageText = (Instantiate( damageTextPrefab, nextDamageTextPosition, Quaternion.identity ) as GameObject);
        //latestDamageText.GetComponent<DamageText>().Initialize( -heal, nextDamageTextPosition );
    }
    public void OnUpdateHP()
    {
        UpdateHPText();
		CurrentArc.SetTargetArc( targetArcRate );
        if( Player.HitPoint >= turnStartHP_ )
        {
            turnStartHP_ = Player.HitPoint;
			RedArc.SetTargetArc(targetArcRate);
        }
    }

    void UpdateHPText()
    {
        CurrentHPCount.Count = Player.HitPoint;
        //hpText.text = "HP:" + Player.HitPoint + "/" + Player.MaxHP;
        //if( (float)Player.HitPoint / Player.MaxHP <= 0.25f )
        //{
        //    hpText.color = Color.gray;
        //}
        //else
        //{
        //    hpText.color = Color.white;
        //}
    }
}
