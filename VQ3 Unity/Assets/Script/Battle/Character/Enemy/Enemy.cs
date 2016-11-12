using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EnemySpecies
{
    Spirit,
    Fairy,
    Human,
    Thing,
    Beast,
    Dragon,
    Jewel,
    Weather,
	Boss,
}

public class Enemy : Character
{
	protected static readonly float DamageTrembleTime = 0.025f;

	public string DisplayName;
	public EnemySpecies Speceis;
	public string ExplanationText;

	public EnemyCommand currentCommand { get; protected set; }
	public int commandExecBar { get; protected set; }
	public Vector3 targetLocalPosition { get; protected set; }

	protected ActionResult lastDamageResult;
	protected DamageText lastDamageText;
	protected SpriteRenderer spriteRenderer;
	protected List<EnemyCommandIcon> commandIcons;
	protected ShortTextWindow shortText;


	// Use this for initialization
	public virtual void Start()
	{
		Initialize();
	}

	public override void Initialize()
	{
		base.Initialize();
		foreach( EnemyCommand c in GetComponentsInChildren<EnemyCommand>() )
		{
			c.Parse();
		}
		initialPosition = transform.localPosition;
		targetLocalPosition = initialPosition;

		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	// Update is called once per frame
	public virtual void Update()
	{
		UpdateAnimation();

		/*
		if( GameContext.State == GameState.Battle )
		{
			if( Music.IsJustChangedAt(commandExecBar) && currentCommand != null && currentCommand.ShortText != "" )
			{
				if( shortText != null )
				{
					Destroy(shortText.gameObject);
					shortText = null;
				}
				if( !GameContext.LuxSystem.IsInverting )
				{
					shortText = (Instantiate(GameContext.EnemyConductor.shortTextWindowPrefab) as GameObject).GetComponent<ShortTextWindow>();
					shortText.Initialize(currentCommand.ShortText);
					shortText.transform.position = new Vector3(transform.position.x, shortText.transform.position.y, shortText.transform.position.z);
				}
			}
		}
		*/
	}
	protected virtual void UpdateAnimation()
	{
		if( damageTime > 0 )
		{
			if( (int)(damageTime / DamageTrembleTime) != (int)((damageTime + Time.deltaTime) / DamageTrembleTime) )
			{
				transform.localPosition = initialPosition + Random.insideUnitSphere * Mathf.Clamp(damageTime - GameContext.EnemyConductor.EnemyDamageTimeMin, 0.2f, 2.0f) * GameContext.EnemyConductor.EnemyDamageShake;
			}
			spriteRenderer.color = (damageTime % (DamageTrembleTime * 2) > DamageTrembleTime ? (GameContext.LuxSystem.IsOverFlow ? ColorManager.Theme.Bright : Color.clear) : GameContext.EnemyConductor.baseColor);

			damageTime -= Time.deltaTime;
			if( damageTime <= 0 )
			{
				transform.localPosition = initialPosition;
				
				if( HitPoint <= 0 )
				{
					//overkill
					if( GameContext.LuxState == LuxState.Overload && Music.Just.Bar < 3 ) return;

					spriteRenderer.color = Color.clear;
					//SEPlayer.Play("Defeat");
					Destroy(this.gameObject);
				}
				else
				{
					spriteRenderer.color = GameContext.EnemyConductor.baseColor;
				}
			}
		}
	}

	protected void CreateDamageText(int damage, ActionResult actResult, GameObject parent = null)
	{
		if( (GameContext.LuxSystem.Version < LuxVersion.Shield || GameContext.LuxState == LuxState.Overflow) && damage == 0 ) return;

		if( GameContext.LuxSystem.IsOverFlow )
		{
			Vector3 damageTextPos = parent != null ? parent.transform.position : transform.position + Vector3.left * 5;
			if( GameContext.EnemyConductor.damageGauge.Enemy == this )
			{
				GameContext.EnemyConductor.damageGauge.AddDamage(damage);
			}
			else
			{
				GameContext.EnemyConductor.damageGauge.InitializeDamage(this, damage, damageTextPos + Vector3.down + Vector3.right);
			}

			if( lastDamageText != null )
			{
				lastDamageText.AddDamage(damage);
			}
			else
			{
				GameObject damageText = (Instantiate(GameContext.EnemyConductor.damageTextPrefab, damageTextPos, Quaternion.identity) as GameObject);
				damageText.transform.parent = GameContext.BattleConductor.DamageTextParent.transform;
				damageText.transform.localPosition = new Vector3(damageText.transform.localPosition.x, damageText.transform.localPosition.y, 0);
				lastDamageText = damageText.GetComponent<DamageText>();
				lastDamageText.InitializeDamage(damage, actResult);
			}
		}
	}


	public void TellCommand(string commandName)
	{
		currentCommand = null;
		foreach( EnemyCommand c in GetComponentsInChildren<EnemyCommand>() )
		{
			if( c.name == commandName || c.CommandAliases.Contains(commandName) )
			{
				currentCommand = c;
				break;
			}
		}
		if( currentCommand != null )
		{
			TurnInit(currentCommand);
		}

		if( GameContext.LuxSystem.State != LuxState.Overload )
		{
			lastDamageText = null;
		}
	}
	public virtual void InvertInit()
	{
		DefendPercent = 0;
		HealPercent = 0;
		TurnDamage = 0;
		currentCommand = null;
	}
	public void SetWaitCommand(EnemyCommand WaitCommand)
	{
		DefaultInit();
		currentCommand = WaitCommand;
	}
	public void SetExecBar(int bar)
	{
		commandExecBar = bar;
	}
	public void CheckSkill()
	{
		Skill skill = (currentCommand != null ? currentCommand.GetCurrentSkill(commandExecBar) : null);
		if( skill != null )
		{
			Skill objSkill = (Skill)Instantiate(skill);
			objSkill.SetOwner(this);
			GameContext.BattleConductor.ExecSkill(objSkill);
		}
	}

	public override void BeAttacked(AttackModule attack, Skill skill)
	{
		float typeCoeff = 1.0f;
		float shieldCoeff = 1.0f;

		if( attack.type == AttackType.Vox )
		{
			lastDamageResult = ActionResult.PhysicGoodDamage;
			typeCoeff = 2.0f;
		}
		else
		{
			if( GameContext.LuxSystem.IsOverFlow )
			{
				if( attack.type == AttackType.Dain ) lastDamageResult = ActionResult.PhysicGoodDamage;
				else lastDamageResult = ActionResult.MagicGoodDamage;
			}
			else
			{
				if( attack.type == AttackType.Dain ) lastDamageResult = ActionResult.PhysicShieldDamage;
				else lastDamageResult = ActionResult.MagicShieldDamage;

				/*
				switch( Speceis )
				{
				case EnemySpecies.Fairy:
					if( attack.type == AttackType.Dain ) lastDamageResult = ActionResult.PhysicDamage;
					else lastDamageResult = ActionResult.MagicGoodDamage;
					break;
				case EnemySpecies.Spirit:
					if( attack.type == AttackType.Dain ) lastDamageResult = ActionResult.PhysicGoodDamage;
					else lastDamageResult = ActionResult.MagicDamage;
					break;
				default:
					if( attack.type == AttackType.Dain ) lastDamageResult = ActionResult.PhysicDamage;
					else lastDamageResult = ActionResult.MagicDamage;
					break;
				}
				if( lastDamageResult.ToString().EndsWith("GoodDamage") ) typeCoeff = 2.0f;
				*/

				if( GameContext.LuxSystem.Version >= LuxVersion.Shield ) shieldCoeff = 0.0f;
			}
		}


		float overFlowPower = 0.0f;
		if( GameContext.LuxSystem.IsOverFlow && GameContext.LuxSystem.State != LuxState.Overload )
		{
			overFlowPower = attack.VP * 8.0f;
		}

		float damage = skill.OwnerCharacter.PhysicAttack * ((attack.Power + overFlowPower) / 100.0f) * typeCoeff * shieldCoeff * DefendCoeff;
		BeDamaged(Mathf.Max(0, (int)damage), skill);
		Debug.Log(this.ToString() + " was Attacked! " + damage + "Damage! HitPoint is " + HitPoint);

		if( lastDamageResult != ActionResult.NoDamage )
		{
			SEPlayer.Play(lastDamageResult, skill.OwnerCharacter, (int)damage);
		}
	}
	protected override void BeDamaged(int damage, Skill skill)
	{
		base.BeDamaged(damage, skill);
		CreateDamageText(damage, lastDamageResult, skill.damageParent);
		if( HitPoint <= 0 )
		{
			OnDead();
			if( shortText != null )
			{
				Destroy(shortText.gameObject);
				shortText = null;
			}
		}
	}
	public override void Heal(HealModule heal)
	{
		int oldHitPoint = HitPoint;
		base.Heal(heal);
		CreateDamageText(-(HitPoint - oldHitPoint), ActionResult.EnemyHeal);
		SEPlayer.Play(ActionResult.EnemyHeal, this, HitPoint - oldHitPoint);
	}
	public override void Drain(DrainModule drain, int drainDamage)
	{
		int oldHitPoint = HitPoint;
		base.Drain(drain, drainDamage);
		CreateDamageText(-(HitPoint - oldHitPoint), ActionResult.EnemyHeal);
		SEPlayer.Play(ActionResult.EnemyHeal, this, HitPoint - oldHitPoint);
	}
	public override void UpdateHealHP()
	{
		base.UpdateHealHP();
	}

	public void SetTargetPosition(Vector3 target)
	{
		targetLocalPosition = target;
		initialPosition = target;
	}

	public void OnBaseColorChanged(Color newColor)
	{
		if( spriteRenderer == null ) spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.color = newColor;
	}
	public void OnPlayerLose()
	{
	}

	public override void OnExecuted(Skill skill, ActionSet act)
	{
		base.OnExecuted(skill, act);

		if( skill.characterAnimName != "" && GetComponent<Animation>() != null && GetComponent<Animation>().GetClip(skill.characterAnimName) != null )
		{
			GetComponent<Animation>()[skill.characterAnimName].speed = (float)(Music.CurrentTempo / 60.0);
			GetComponent<Animation>().Play(skill.characterAnimName);
		}
	}
	public virtual void OnDead()
	{
	}

	public override string ToString()
	{
		return DisplayName;
	}
}
