using UnityEngine;
using System.Collections;

public class Player : Character
{

	public float DamageShake;
	public GameObject damageTextPrefab;

	public GameObject UIParent;
	public CommandGraph commandGraph;
	public HPPanel HPPanel;
	public CutInUI EnhanceCutIn;
	public float DangerPercentage;
	public int DangerHysteresis = 15;

	DamageText lastDamageText;
	public bool IsDangerMode { get; protected set; }

	void Start()
	{
		Initialize();
		initialPosition = UIParent.transform.position;
		HPPanel.OnBattleStart();
	}

	// Update is called once per frame
	void Update()
	{
		if( damageTime > 0 )
		{
			if( (int)(damageTime/0.05f) != (int)((damageTime+Time.deltaTime)/0.05f) )
			{
				UIParent.transform.position = initialPosition + Random.insideUnitSphere * Mathf.Clamp(damageTime - GameContext.PlayerConductor.PlayerDamageTimeMin, 0.2f, 2.0f) * DamageShake;
			}
			damageTime -= Time.deltaTime;
			if( damageTime <= 0 )
			{
				UIParent.transform.position = initialPosition;
			}
		}
		if( GameContext.LuxSystem.State != LuxState.Overload )
		{
			if( IsDangerMode )
			{
				if( HitPoint > MaxHP * (DangerPercentage + DangerHysteresis) / 100.0f )
				{
					float rate = (((float)HitPoint / MaxHP) - DangerPercentage)/DangerHysteresis;
					Music.SetAisac("Danger", rate);
				}
			}
		}
	}

	public override string ToString()
	{
		return "Player";
	}

	public override void TurnInit(CommandBase command)
	{
		base.TurnInit(command);
		//foreach( EnhanceParameter enhanceParam in ActiveEnhanceParams )
		//{
		//	switch( enhanceParam.type )
		//	{
		//	case EnhanceParamType.Brave:
		//		BattlePanels[(int)EBattlePanelType.VT].SetEnhance(enhanceParam);
		//		break;
		//	case EnhanceParamType.Faith:
		//		BattlePanels[(int)EBattlePanelType.VP].SetEnhance(enhanceParam);
		//		break;
		//	case EnhanceParamType.Shield:
		//		BattlePanels[(int)EBattlePanelType.DF].SetEnhance(enhanceParam);
		//		break;
		//	case EnhanceParamType.Regene:
		//		BattlePanels[(int)EBattlePanelType.HL].SetEnhance(enhanceParam);
		//		break;
		//	}
		//}
		HPPanel.OnTurnStart((command as PlayerCommandData).OwnerCommand);
	}
	public override void BeAttacked(AttackModule attack, Skill skill)
	{
		int oldHP = HitPoint;
		base.BeAttacked(attack, skill);
		int damage = oldHP - HitPoint;

		if( attack.type == AttackType.Attack )
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
			GameObject damageText = (Instantiate(damageTextPrefab) as GameObject);
			lastDamageText = damageText.GetComponent<DamageText>();
			GameObject textPos = skill.damageParent;// transform.FindChild("DamageTextPos");
			if( textPos == null ) textPos = skill.GetComponentInChildren<Animation>().gameObject;
			lastDamageText.Initialize(damage, (attack.type == AttackType.Attack ? ActionResult.PlayerPhysicDamage : ActionResult.PlayerMagicDamage));
			lastDamageText.transform.position = textPos.transform.position + Vector3.back;
		}
	}
	public void VPDrained(AttackModule attack, Skill skill, int drainVP)
	{
		GameObject damageText = (Instantiate(damageTextPrefab) as GameObject);
		Transform textPos = skill.transform.FindChild("DamageTextPos");
		if( textPos == null ) textPos = skill.GetComponentInChildren<Animation>().transform;
		damageText.GetComponent<DamageText>().Initialize(drainVP, ActionResult.VPDrain);
		damageText.transform.position = textPos.position + Vector3.right * 5 + Vector3.down * 2;
	}
	protected override void BeDamaged(int damage, Skill skill)
	{
		base.BeDamaged(damage, skill);

		HPPanel.OnDamage(damage);
		CheckDangerMode();

		if( HitPoint <= 0 )
		{
			GameContext.BattleConductor.SetState(BattleState.Continue);
		}
	}
	public override void Heal(HealModule heal)
	{
		int oldHitPoint = HitPoint;
		base.Heal(heal);
		if( HitPoint - oldHitPoint > 0 )
		{
			HPPanel.OnHeal(HitPoint - oldHitPoint);
			CheckDangerMode();
			//HPBar.OnHeal( HitPoint - oldHitPoint );
			SEPlayer.Play(ActionResult.PlayerHeal, this, HitPoint - oldHitPoint);
		}
	}
	public override void Enhance(EnhanceModule enhance)
	{
		base.Enhance(enhance);
		if( enhance.phase >= 0 )
		{
			SEPlayer.Play("enhance");
		}
		else
		{
			SEPlayer.Play("jam");
		}
		//switch( enhance.type )
		//{
		//case EnhanceParamType.Brave:
		//	BattlePanels[(int)EBattlePanelType.VT].SetEnhance(PhysicAttackEnhance);
		//	EnhanceCutIn.Set("持続", enhance.phase, PhysicAttackEnhance.currentParam);
		//	break;
		//case EnhanceParamType.Faith:
		//	BattlePanels[(int)EBattlePanelType.VP].SetEnhance(MagicAttackEnhance);
		//	EnhanceCutIn.Set("充填", enhance.phase, MagicAttackEnhance.currentParam);
		//	break;
		//case EnhanceParamType.Shield:
		//	BattlePanels[(int)EBattlePanelType.DF].SetEnhance(DefendEnhance);
		//	EnhanceCutIn.Set("防御", enhance.phase, DefendEnhance.currentParam);
		//	break;
		//case EnhanceParamType.Regene:
		//	BattlePanels[(int)EBattlePanelType.HL].SetEnhance(HitPointEnhance);
		//	EnhanceCutIn.Set("回復", enhance.phase, HitPointEnhance.currentParam);
		//	break;
		//case EnhanceParamType.Esna:
		//	break;
		//}

		//EnhanceIcons.OnUpdateParam( GetActiveEnhance( enhance.type ) );
	}
	public override void UpdateHealHP()
	{
		base.UpdateHealHP();
		HPPanel.OnUpdateHP();
		CheckDangerMode();
		if( HitPoint <= 0 )
		{
			GameContext.BattleConductor.SetState(BattleState.Continue);
		}
	}

	public void CheckDangerMode()
	{
		if( GameContext.LuxSystem.State != LuxState.Overload )
		{
			if( IsDangerMode )
			{
				if( HitPoint > MaxHP * (DangerPercentage + DangerHysteresis) / 100.0f )
				{
					IsDangerMode = false;
					ColorManager.SetBaseColor(EBaseColor.Black);
					Music.SetAisac("Danger", 1);
				}
			}
			else
			{
				if( HitPoint <= MaxHP * DangerPercentage / 100.0f )
				{
					IsDangerMode = true;
					ColorManager.SetBaseColor(EBaseColor.Red);
					EnhanceCutIn.SetDanger();
					Music.SetAisac("Danger", 0);
				}
			}
		}
	}

	public void OnBattleStart()
	{
		HitPoint = MaxHP;
		DefendPercent = 0;
		HealPercent = 0;
		TurnDamage = 0;
		HPPanel.OnBattleStart();
		foreach( EnhanceParameter enhanceParam in ActiveEnhanceParams )
		{
			enhanceParam.Init();
			//EnhanceIcons.OnUpdateParam( enhanceParam );
		}
		ActiveEnhanceParams.Clear();
	}
}
