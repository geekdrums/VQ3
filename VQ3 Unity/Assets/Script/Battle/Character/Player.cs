using UnityEngine;
using System.Collections;

public class Player : Character {
    GameObject UIParent;
    HPBar HPBar;

    public GameObject DefendAnimPrefab;

	// Use this for initialization
	void Start()
    {
        Initialize();
        UIParent = GameObject.Find( "UI" );
        HPBar = UIParent.GetComponentInChildren<HPBar>();
        initialPosition = UIParent.transform.position;
        HPBar.OnTurnStart();
	}
	
	// Update is called once per frame
	void Update () {
		if ( damageTime > 0 )
		{
			if ( (int)( damageTime/0.05f ) != (int)( (damageTime+Time.deltaTime)/0.05f ) )
			{
                UIParent.transform.position = initialPosition + Random.insideUnitSphere * Mathf.Clamp( damageTime, 0.1f, 2.0f );
			}
			damageTime -= Time.deltaTime;
			if ( damageTime <= 0 )
			{
                UIParent.transform.position = initialPosition;
			}
		}
	}

	public override string ToString()
	{
		return "Player";
	}

    public override void TurnInit()
    {
        base.TurnInit();
        if( HPBar != null ) HPBar.OnTurnStart();
    }
    protected override void BeDamaged( int damage, Character ownerCharacter )
    {
        base.BeDamaged( damage, ownerCharacter );

        if( damage <= 0 )
        {
            (Instantiate( DefendAnimPrefab, ownerCharacter.transform.position + new Vector3( 0, 0, -0.1f ), DefendAnimPrefab.transform.rotation ) as GameObject).transform.parent = transform;
        }
        else
        {
            HPBar.OnDamage( damage );
        }

        if( HitPoint <= 0 )
        {
            GameContext.BattleConductor.OnPlayerLose();
        }
    }
    public override void Heal( HealModule heal )
    {
        int oldHitPoint = HitPoint;
        base.Heal( heal );
        if( HitPoint - oldHitPoint > 0 )
        {
            HPBar.OnHeal( HitPoint - oldHitPoint );
        }
    }

    public void OnBattleStart()
    {
        TurnInit();
        HitPoint = MaxHP;
    }
}
