using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EnhanceParamType
{
    Time,
    Break,
    Heal,
	Defend,
	Esna,
    Despell,
	Count
}

public class EnhanceParameter
{
    readonly int[] goodPhaseParams;
    readonly int[] badPhaseParams;

    public EnhanceParamType type { get; private set; }
    public int phase { get; private set; }
    public int remainTurn { get; private set; }
    public int currentParam { get { return (phase == 0 ? 0 : (phase > 0 ? goodPhaseParams[phase - 1] : badPhaseParams[-phase - 1])); } }

    public EnhanceParameter( EnhanceParamType type, params int[] badAndGoodParams )
    {
        this.type = type;
        int numBadParam = 0;
        for( int i = 0; i < badAndGoodParams.Length; i++ )
        {
            if( badAndGoodParams[i] > 0 ) { numBadParam = i; break; }
        }
        badPhaseParams = new int[numBadParam];
        goodPhaseParams = new int[badAndGoodParams.Length - numBadParam];
        for( int i = 0; i < badAndGoodParams.Length; i++ )
        {
            if( i < numBadParam ) badPhaseParams[numBadParam-i-1] = badAndGoodParams[i];
            else goodPhaseParams[i - numBadParam] = badAndGoodParams[i];
        }
    }

    public void SetPhase( int phase, int turn )
    {
        this.phase = Mathf.Clamp( phase, -badPhaseParams.Length, goodPhaseParams.Length );
        this.remainTurn = (phase == 0 ? 0 : turn);
    }
    public void OnTurnStart()
    {
        --remainTurn;
        if( remainTurn <= 0 ) phase = 0;
    }
    public void Init()
    {
        phase = 0;
        remainTurn = 0;
    }
}

public class Character : MonoBehaviour
{
	protected static readonly int LEAST_DAMAGE_RANGE = 3;
	protected static readonly int LEAST_MAGIC_DAMAGE_RANGE = 2;

	public int HitPoint;
	public int BasePower;
	public int BaseMagic;

	protected int DefendPercent;
	protected int HealPercent;
	protected int TurnDamage;

	public bool isAlive { get { return HitPoint > 0; } }
	public int MaxHP { get; protected set; }
	public float DefendCoeff { get { return (100.0f - DefendPercent - DefendEnhance.currentParam) / 100.0f; } }
	public float VTCoeff { get { return (100 + PhysicAttackEnhance.currentParam) / 100.0f; } }
	public float VPCoeff { get { return (100 + MagicAttackEnhance.currentParam) / 100.0f; } }
	public float DefendEnhParam { get { return DefendEnhance.currentParam; } }
	public float PhysicAttack { get { return BasePower * (100 + PhysicAttackEnhance.currentParam) / 100.0f; } }
	public float MagicAttack { get { return BaseMagic * (100 + MagicAttackEnhance.currentParam) / 100.0f; } }
	public EnhanceParameter GetActiveEnhance(EnhanceParamType type) { return ActiveEnhanceParams.Find((EnhanceParameter enhance) => enhance.type == type); }

	protected EnhanceParameter PhysicAttackEnhance = new EnhanceParameter(EnhanceParamType.Time, -50, -30, 30, 50);
	protected EnhanceParameter MagicAttackEnhance = new EnhanceParameter(EnhanceParamType.Break, -50, -30, 30, 50);
	protected EnhanceParameter DefendEnhance = new EnhanceParameter(EnhanceParamType.Defend, -45, -30, 30, 45);
	protected EnhanceParameter HitPointEnhance = new EnhanceParameter(EnhanceParamType.Heal, -10, -5, 5, 10);
	protected List<EnhanceParameter> ActiveEnhanceParams = new List<EnhanceParameter>();

	protected float damageTime;
	protected Vector3 initialPosition;

	public virtual void Initialize()
	{
		MaxHP = HitPoint;
	}

	// ======================
	// Battle
	// ======================
	public virtual void TurnInit(CommandBase command)
	{
		DefendPercent = command.DefendPercent;
		HealPercent = command.HealPercent;
		TurnDamage = 0;
		foreach( EnhanceParameter enhanceParam in ActiveEnhanceParams )
		{
			enhanceParam.OnTurnStart();
		}
		ActiveEnhanceParams.RemoveAll((EnhanceParameter enhanceParam) => enhanceParam.remainTurn <= 0);
	}
	public virtual void DefaultInit()
	{
		DefendPercent = 0;
		HealPercent = 0;
		TurnDamage = 0;
	}
	public virtual void BeAttacked(AttackModule attack, Skill skill)
	{
		float damage = 0;
		float basePower = (attack.type == AttackType.Attack || attack.type == AttackType.Vox) ? skill.OwnerCharacter.PhysicAttack : skill.OwnerCharacter.MagicAttack;
		damage = basePower * (attack.Power / 100.0f) * DefendCoeff;
		int damageResult = Mathf.Max(0, (int)damage);
		BeDamaged(damageResult, skill);
		attack.SetDamageResult(damageResult);
		//Debug.Log(this.ToString() + " was Attacked! " + damage + "Damage! HitPoint is " + HitPoint);
	}
	protected virtual void BeDamaged(int damage, Skill skill)
	{
		int d = Mathf.Max(0, damage);
		HitPoint = Mathf.Clamp(HitPoint - d, 0, HitPoint);
		TurnDamage += d;
		int RelativeMaxHP = (MaxHP < GameContext.PlayerConductor.PlayerMaxHP ? MaxHP : GameContext.PlayerConductor.PlayerMaxHP);
		if( this is Player )
		{
			damageTime += GameContext.PlayerConductor.PlayerDamageTimeMin + ((float)d / (float)RelativeMaxHP) * GameContext.PlayerConductor.PlayerDamageTimeCoeff;
		}
		else
		{
			damageTime += GameContext.EnemyConductor.EnemyDamageTimeMin + ((float)d / (float)RelativeMaxHP) * GameContext.EnemyConductor.EnemyDamageTimeCoeff;
			if( BGAnimBase.CurrentAnim != null )
			{
				BGAnimBase.CurrentAnim.OnDamage(damageTime);
			}
		}
		damageTime = Mathf.Min(damageTime, (float)Music.MusicalTimeUnit * 8);
	}
	public virtual void Heal(HealModule heal)
	{
		int h = Mathf.Min(MaxHP - HitPoint, (int)(MaxHP * ((float)heal.HealPoint/100.0f)));
		if( h > 0 )
		{
			HitPoint += h;
			//Debug.Log(this.ToString() + " used Heal! HitPoint is " + HitPoint);
		}
	}
	public virtual void Drain(DrainModule drain, int drainDamage)
	{
		int h = Mathf.Min(MaxHP - HitPoint, (int)(drain.Rate * drainDamage));
		if( h > 0 )
		{
			HitPoint += h;
			//Debug.Log(this.ToString() + " used Drain! HitPoint is " + HitPoint);
		}
	}
	public virtual void Enhance(EnhanceModule enhance)
	{
		EnhanceParameter TargetParameter = null;
		int dPhase = enhance.phase;
		switch( enhance.type )
		{
		case EnhanceParamType.Time: TargetParameter = PhysicAttackEnhance; break;
		case EnhanceParamType.Break: TargetParameter = MagicAttackEnhance; break;
		case EnhanceParamType.Defend: TargetParameter = DefendEnhance; break;
		case EnhanceParamType.Heal: TargetParameter = HitPointEnhance; break;
		case EnhanceParamType.Esna:
			foreach( EnhanceParameter activeEnhance in ActiveEnhanceParams )
			{
				if( enhance.phase < 0 )
				{
					TargetParameter = activeEnhance;
					dPhase = -activeEnhance.phase;
					break;
				}
			}
			break;
		case EnhanceParamType.Despell:
			foreach( EnhanceParameter activeEnhance in ActiveEnhanceParams )
			{
				if( enhance.phase > 0 )
				{
					TargetParameter = activeEnhance;
					dPhase = -activeEnhance.phase;
					break;
				}
			}
			break;
		}
		if( TargetParameter != null )
		{
			TargetParameter.SetPhase(TargetParameter.phase + dPhase, enhance.turn);
			if( TargetParameter.phase == 0 && ActiveEnhanceParams.Contains(TargetParameter) ) ActiveEnhanceParams.Remove(TargetParameter);
			else if( TargetParameter.phase != 0 && !ActiveEnhanceParams.Contains(TargetParameter) ) ActiveEnhanceParams.Add(TargetParameter);
		}
	}
	public virtual void UpdateHealHP()
	{
		int mt = Music.Just.MusicalTime;
		if( mt <= 0 ) return;
		else
		{
			int HealHP = (int)(MaxHP * (HealPercent + HitPointEnhance.currentParam) / 100.0f);
			int previousHealHP = HealHP * (mt-1) / 64;// 4 bars
			int currentHealHP  = HealHP * mt / 64;
			HitPoint += (currentHealHP - previousHealHP);
			HitPoint = Mathf.Clamp(HitPoint, 0, MaxHP);
		}
	}

	public virtual void OnExecuted(Skill skill, ActionSet act)
	{
	}

	public virtual void OnSkillEnd(Skill skill)
	{
	}
}
