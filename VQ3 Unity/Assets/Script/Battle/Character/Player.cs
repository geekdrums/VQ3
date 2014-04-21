using UnityEngine;
using System.Collections;

public class Player : Character {
    GameObject UIParent;
    HPBar HPBar;
    EnhanceIcons EnhanceIcons;
    CommandGraph commandGraph;
    //public GameObject DefendAnimPrefab;

	// Use this for initialization
	void Start()
    {
        Initialize();
        UIParent = GameObject.Find( "UI" );
        HPBar = UIParent.GetComponentInChildren<HPBar>();
        EnhanceIcons = UIParent.GetComponentInChildren<EnhanceIcons>();
        commandGraph = GameObject.Find( "CommandGraph" ).GetComponent<CommandGraph>();
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

    public override void TurnInit( CommandBase command )
    {
        base.TurnInit( command );
        if( HPBar != null ) HPBar.OnTurnStart();
    }
    public override void BeAttacked(AttackModule attack, Skill skill)
    {
        int oldHP = HitPoint;
        base.BeAttacked( attack, skill );
        int damage = oldHP - HitPoint;

        if( attack.isPhysic )
        {
            SEPlayer.Play( ActionResult.PlayerPhysicDamage, skill.OwnerCharacter, damage );
        }
        else
        {
            SEPlayer.Play( ActionResult.PlayerMagicDamage, skill.OwnerCharacter, damage );
        }
    }
    protected override void BeDamaged( int damage, Character ownerCharacter )
    {
        base.BeDamaged( damage, ownerCharacter );

        HPBar.OnDamage( damage );
        commandGraph.OnReactEvent( IconReactType.OnDamage );
        TextWindow.AddMessage( "オクスは " + damage + " のダメージを　うけた" );
        //if( damage <= 0 )
        //{
        //    (Instantiate( DefendAnimPrefab, ownerCharacter.transform.position + new Vector3( 0, 0, -0.1f ), DefendAnimPrefab.transform.rotation ) as GameObject).transform.parent = transform;
        //    TextWindow.AddMessage( "オクスは　ぼうぎょで　うけきった" );
        //}
        //else
        //{
        //    HPBar.OnDamage( damage );
        //    TextWindow.AddMessage( "オクスは " + damage + " のダメージを　うけた" );
        //}

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
            SEPlayer.Play( ActionResult.PlayerHeal, this, HitPoint - oldHitPoint );
        }
    }
    public override void Enhance( EnhanceModule enhance )
    {
        base.Enhance( enhance );
        switch( enhance.type )
        {
        case EnhanceParamType.Brave: commandGraph.OnReactEvent( IconReactType.OnBrave ); break;
        case EnhanceParamType.Faith: commandGraph.OnReactEvent( IconReactType.OnFaith ); break;
        case EnhanceParamType.Shield: commandGraph.OnReactEvent( IconReactType.OnShield ); break;
        case EnhanceParamType.Regene: commandGraph.OnReactEvent( IconReactType.OnRegene ); break;
        case EnhanceParamType.Esna: commandGraph.OnReactEvent( IconReactType.OnEsna ); break;
        }
        
        EnhanceIcons.OnUpdateParam( GetActiveEnhance( enhance.type ) );
    }
    public override void UpdateHealHP()
    {
        base.UpdateHealHP();
        HPBar.OnUpdateHP();
        if( HitPoint <= 0 )
        {
            GameContext.BattleConductor.OnPlayerLose();
        }
    }

    public void OnBattleStart()
    {
        HitPoint = MaxHP;
        PhysicDefend = 0;
        MagicDefend = 0;
        HealPercent = 0;
        TurnDamage = 0;
    }
}
