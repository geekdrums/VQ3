using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {
    protected static readonly float DAMAGE_RANGE = 12.0f;
    protected static readonly float MAGIC_DAMAGE_RANGE = 8.0f;
    protected static readonly int LEAST_DAMAGE_RANGE = 3;
    protected static readonly int LEAST_MAGIC_DAMAGE_RANGE = 2;
    
    public int HitPoint;
    public int BasePower;
    public int BaseMagic;

    protected int PhysicDefend;
    protected int MagicDefend;
    protected int TurnDamage;

    public bool isAlive { get { return HitPoint > 0; } }
    public int MaxHP { get; protected set; }
    //TODO: add parameter coeffs
    public float PhysicDefendCoeff { get { return (100.0f - PhysicDefend) / 100.0f; } }
    public float MagicDefendCoeff { get { return (100.0f - MagicDefend) / 100.0f; } }
    //public float PowerCoeff { get { return BasePower; } }
    //public float MagicCoeff { get { return BaseMagic; } }

    protected float damageTime;
    protected Vector3 initialPosition;

    public virtual void Initialize()
    {
        MaxHP = HitPoint;
    }

	// ======================
	// Battle
	// ======================
    public virtual void TurnInit( CommandBase command )
    {
        PhysicDefend = command.PhysicDefend;
        MagicDefend = command.MagicDefend;
        TurnDamage = 0;
    }
	public virtual void BeAttacked( AttackModule attack, Skill skill )
	{
        float damage = 0;
        if( attack.isPhysic )
        {
            damage = skill.OwnerCharacter.BasePower * (attack.Power / 100.0f) * PhysicDefendCoeff;
            damage *= ((100.0f - DAMAGE_RANGE) + Random.Range( 0, DAMAGE_RANGE ) + Random.Range( 0, DAMAGE_RANGE )) / 100.0f;
        }
        else
        {
            damage = skill.OwnerCharacter.BaseMagic * (attack.Power / 100.0f) * MagicDefendCoeff;
            damage *= ((100.0f - MAGIC_DAMAGE_RANGE) + Random.Range( 0, MAGIC_DAMAGE_RANGE ) + Random.Range( 0, MAGIC_DAMAGE_RANGE )) / 100.0f;
        }
        BeDamaged( Mathf.Max( 0, (int)damage ), skill.OwnerCharacter );
        Debug.Log( this.ToString() + " was Attacked! " + damage + "Damage! HitPoint is " + HitPoint );
	}
    protected virtual void BeDamaged( int damage, Character ownerCharacter )
    {
        int d = Mathf.Max( 0, damage );
        HitPoint = Mathf.Clamp( HitPoint - d, 0, HitPoint );
        TurnDamage += d;
        int RelativeMaxHP = (MaxHP < GameContext.PlayerConductor.PlayerMaxHP ? MaxHP : GameContext.PlayerConductor.PlayerMaxHP);
        damageTime += 0.15f + ((float)d / (float)RelativeMaxHP) * 0.7f;
        damageTime = Mathf.Min( damageTime, (float)Music.mtUnit * 8 );
    }

    /*
	public void Defend( DefendModule defend )
	{
        PhysicDefend = defend.DefendPower;
	}

    public void MagicDefend( MagicDefendModule magicDefend )
    {
        MagicDefend = magicDefend.MagicDefendPower;
    }
    */

	public virtual void Heal( HealModule heal )
	{
        int h = Mathf.Min( MaxHP - HitPoint, (int)(MaxHP * ( (float)heal.HealPoint/100.0f )) );
        if( h > 0 )
        {
            HitPoint += h;
            Debug.Log( this.ToString() + " used Heal! HitPoint is " + HitPoint );
        }
	}
}
