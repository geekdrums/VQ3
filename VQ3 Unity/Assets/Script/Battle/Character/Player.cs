using UnityEngine;
using System.Collections;

public class Player : Character {

    public float DamageShake;

    public GameObject UIParent;
    public CommandGraph commandGraph;
    public BattlePanel[] BattlePanels;
    public HPPanel HPPanel;
	public EnhanceCutIn EnhanceCutIn;
	public float DangerPercentage;
    //HPBar HPBar;
    //EnhanceIcons EnhanceIcons;
    //public GameObject DefendAnimPrefab;
	DamageText lastDamageText;
	public bool IsDangerMode { get; protected set; }

	// Use this for initialization
	void Start()
    {
        Initialize();
        //HPBar = UIParent.GetComponentInChildren<HPBar>();
        //EnhanceIcons = UIParent.GetComponentInChildren<EnhanceIcons>();
        initialPosition = UIParent.transform.position;
        //HPBar.OnTurnStart();
        HPPanel.OnBattleStart();
	}
	
	// Update is called once per frame
	void Update () {
		if ( damageTime > 0 )
		{
			if ( (int)( damageTime/0.05f ) != (int)( (damageTime+Time.deltaTime)/0.05f ) )
			{
                UIParent.transform.position = initialPosition + Random.insideUnitSphere * Mathf.Clamp( damageTime, 0.1f, 2.0f ) * DamageShake;
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
        foreach( BattlePanel battlePannel in BattlePanels )
        {
            battlePannel.Set( command as PlayerCommand );
        }
        foreach( EnhanceParameter enhanceParam in ActiveEnhanceParams )
        {
            switch( enhanceParam.type )
            {
            case EnhanceParamType.Brave:
                BattlePanels[(int)EBattlePanelType.VT].SetEnhance( enhanceParam );
                break;
            case EnhanceParamType.Faith:
                BattlePanels[(int)EBattlePanelType.VP].SetEnhance( enhanceParam );
                break;
            case EnhanceParamType.Shield:
                BattlePanels[(int)EBattlePanelType.DF].SetEnhance( enhanceParam );
                break;
            case EnhanceParamType.Regene:
                BattlePanels[(int)EBattlePanelType.HL].SetEnhance( enhanceParam );
                break;
            }
        }
        HPPanel.OnTurnStart( command as PlayerCommand );
    }
    public override void BeAttacked(AttackModule attack, Skill skill)
    {
        int oldHP = HitPoint;
        base.BeAttacked( attack, skill );
        int damage = oldHP - HitPoint;

        if( attack.isPhysic )
        {
			SEPlayer.Play(ActionResult.PlayerPhysicDamage, skill.OwnerCharacter, damage);
        }
        else
        {
			SEPlayer.Play(ActionResult.PlayerMagicDamage, skill.OwnerCharacter, damage);
		}
		if( lastDamageText != null )
		{
			lastDamageText.AddDamage(damage);
		}
		else
		{
			GameObject damageText = (Instantiate(GameContext.EnemyConductor.damageTextPrefab) as GameObject);
			lastDamageText = damageText.GetComponent<DamageText>();
			lastDamageText.Initialize(damage, (attack.isPhysic ? ActionResult.PlayerPhysicDamage : ActionResult.PlayerMagicDamage), skill.GetComponentInChildren<Animation>().transform.position + Vector3.back);
		}
    }
    protected override void BeDamaged( int damage, Character ownerCharacter )
    {
        base.BeDamaged( damage, ownerCharacter );

		HPPanel.OnDamage(damage);
		CheckDangerMode();
        //HPBar.OnDamage( damage );
        commandGraph.OnReactEvent( IconReactType.OnDamage );
        //TextWindow.ChangeMessage( BattleMessageType.Damage, "オクスは <color=red>" + damage + "</color> のダメージを　うけた" );
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
			HPPanel.OnHeal(HitPoint - oldHitPoint);
			CheckDangerMode();
            //HPBar.OnHeal( HitPoint - oldHitPoint );
            SEPlayer.Play( ActionResult.PlayerHeal, this, HitPoint - oldHitPoint );
        }
    }
    public override void Enhance( EnhanceModule enhance )
    {
        base.Enhance( enhance );
        if( enhance.phase >= 0 )
        {
            SEPlayer.Play( "enhance" );
        }
        else
        {
            SEPlayer.Play( "jam" );
        }
        switch( enhance.type )
        {
        case EnhanceParamType.Brave:
            commandGraph.OnReactEvent( IconReactType.OnBrave );
            BattlePanels[(int)EBattlePanelType.VT].SetEnhance( PhysicAttackEnhance );
			EnhanceCutIn.Set("VT", enhance.phase, PhysicAttackEnhance.currentParam);
            break;
        case EnhanceParamType.Faith:
            commandGraph.OnReactEvent( IconReactType.OnFaith );
            BattlePanels[(int)EBattlePanelType.VP].SetEnhance( MagicAttackEnhance );
			EnhanceCutIn.Set("VP", enhance.phase, MagicAttackEnhance.currentParam);
            break;
        case EnhanceParamType.Shield:
            commandGraph.OnReactEvent( IconReactType.OnShield );
            BattlePanels[(int)EBattlePanelType.DF].SetEnhance( DefendEnhance );
			EnhanceCutIn.Set("DEF", enhance.phase, DefendEnhance.currentParam);
            break;
        case EnhanceParamType.Regene:
            commandGraph.OnReactEvent( IconReactType.OnRegene );
            BattlePanels[(int)EBattlePanelType.HL].SetEnhance( HitPointEnhance );
			EnhanceCutIn.Set("HEAL", enhance.phase, HitPointEnhance.currentParam);
            break;
        case EnhanceParamType.Esna:
            commandGraph.OnReactEvent( IconReactType.OnEsna );
            break;
        }
        
        //EnhanceIcons.OnUpdateParam( GetActiveEnhance( enhance.type ) );
    }
    public override void UpdateHealHP()
    {
        base.UpdateHealHP();
        HPPanel.OnUpdateHP();
		CheckDangerMode();
        if( HitPoint <= 0 )
        {
            GameContext.BattleConductor.OnPlayerLose();
		}
	}

	public void CheckDangerMode()
	{
		if( GameContext.VoxSystem.state != VoxState.Invert )
		{
			if( IsDangerMode )
			{
				if( HitPoint > MaxHP * (DangerPercentage + 10) / 100.0f )
				{
					IsDangerMode = false;
					ColorManager.SetBaseColor(EBaseColor.Black);
					Music.SetAisac(8, 1);
				}
			}
			else
			{
				if( HitPoint <= MaxHP * DangerPercentage / 100.0f )
				{
					IsDangerMode = true;
					ColorManager.SetBaseColor(EBaseColor.Red);
					EnhanceCutIn.SetDanger();
					Music.SetAisac(8, 0);
				}
			}
		}
	}

    public void OnBattleStart()
    {
        HitPoint = MaxHP;
        PhysicDefend = 0;
        MagicDefend = 0;
        HealPercent = 0;
        TurnDamage = 0;
        HPPanel.OnBattleStart();
        foreach( EnhanceParameter enhanceParam in ActiveEnhanceParams )
        {
            enhanceParam.Init();
            //EnhanceIcons.OnUpdateParam( enhanceParam );
        }
        foreach( BattlePanel battlePanel in BattlePanels )
        {
            battlePanel.Init();
        }
        ActiveEnhanceParams.Clear();
    }
}
