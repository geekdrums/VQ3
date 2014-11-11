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
    public CounterSprite DamageCount;

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
        DamageCount.transform.localScale = Vector3.zero;
	}
	
	// Update is called once per frame
    void Update()
    {
	}


    public void OnBattleStart()
    {
        turnStartHP_ = Player.HitPoint;
        MaxHPCount.Count = Player.MaxHP;
        DamageCount.transform.localScale = Vector3.zero;
        DamageCircle.SetWidth( 0 );
        DefendCircle.SetColor( ColorManager.Base.Middle );
        CurrentArc.SetColor( ColorManager.Base.Front );
        CurrentHPCount.CounterColor = ColorManager.Base.Front;
        MaxHPCount.CounterColor = ColorManager.Base.Front;
    }
    public void OnTurnStart( PlayerCommand command )
    {
        currentCommand_ = command;
        UpdateHPText();
        turnStartHP_ = Player.HitPoint;
        CurrentArc.ArcRate = targetArcRate;
        RedArc.ArcRate = targetArcRate;

        float defend = command.GetDefend() / 100.0f;
        if( defend <= 0 )
        {
            DefendCircle.SetTargetWidth( 0.1f );
            DefendCircle.SetColor( ColorManager.Base.Middle );
        }
        else
        {
            DefendCircle.SetTargetWidth( 0.1f + defend*defend );
            DefendCircle.SetColor( ColorManager.Theme.Shade );
        }
        if( command.HealPercent > 0 )
        {
            CurrentHPCount.CounterColor = ColorManager.Theme.Light;
            MaxHPCount.CounterColor = ColorManager.Theme.Light;
            CurrentArc.SetColor( ColorManager.Theme.Light );
        }
        else
        {
            CurrentHPCount.CounterColor = ColorManager.Base.Front;
            MaxHPCount.CounterColor = ColorManager.Base.Front;
            CurrentArc.SetColor( ColorManager.Base.Front );
        }
        DamageCircle.SetWidth( 0 );
        DamageCount.transform.localScale = Vector3.zero;
    }
    public void OnDamage( int damage )
    {
        UpdateHPText();
        //CurrentBar.transform.localScale = targetScale;
        CurrentArc.ArcRate = targetArcRate;
        DamageCount.Count = turnStartHP_ - Player.HitPoint;
        DamageCircle.SetAnimationSize( DefendCircle.WholeRadius / 2, DefendCircle.WholeRadius );
        DamageCircle.SetAnimationWidth( DefendCircle.WholeRadius * 10, 0 );

        //Vector3 nextDamageTextPosition = (latestDamageText == null ? damageTextPrefab.transform.position : latestDamageText.GetComponent<DamageText>().initialPosition + Vector3.right);
        //latestDamageText = (Instantiate( damageTextPrefab, nextDamageTextPosition, Quaternion.identity ) as GameObject);
        //latestDamageText.GetComponent<DamageText>().Initialize( damage, nextDamageTextPosition );
    }
    public void OnHeal( int heal )
    {
        UpdateHPText();
        //CurrentBar.transform.localScale = targetScale;
        CurrentArc.ArcRate = targetArcRate;
        if( Player.HitPoint >= turnStartHP_ )
        {
            turnStartHP_ = Player.HitPoint;
            RedArc.ArcRate = targetArcRate;
            DamageCount.transform.localScale = Vector3.zero;
            //RedBar.transform.localScale = targetScale;
        }
        else
        {
            DamageCount.Count = turnStartHP_ - Player.HitPoint;
            DamageCount.transform.localScale = Vector3.one;
        }

        //Vector3 nextDamageTextPosition = (latestDamageText == null ? damageTextPrefab.transform.position : latestDamageText.transform.position + Vector3.right);
        //latestDamageText = (Instantiate( damageTextPrefab, nextDamageTextPosition, Quaternion.identity ) as GameObject);
        //latestDamageText.GetComponent<DamageText>().Initialize( -heal, nextDamageTextPosition );
    }
    public void OnUpdateHP()
    {
        UpdateHPText();
        CurrentArc.ArcRate = targetArcRate;
        if( Player.HitPoint >= turnStartHP_ )
        {
            turnStartHP_ = Player.HitPoint;
            RedArc.ArcRate = targetArcRate;
            DamageCount.transform.localScale = Vector3.zero;
        }
        else
        {
            DamageCount.Count = turnStartHP_ - Player.HitPoint;
            DamageCount.transform.localScale = Vector3.one;
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
