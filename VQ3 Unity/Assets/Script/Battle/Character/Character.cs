using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {

    public int HitPoint;
    public int BaseDefend;
    public int BaseMagicDefend;
    protected int SkillDefend;
    protected int SkillMagicDefend;
    protected int DefendPower { get { return BaseDefend + SkillDefend; } }
    protected int MagicDefendPower { get { return BaseMagicDefend + SkillMagicDefend; } }

	protected float damageTime;
    protected int MaxHP;

    protected virtual void Initialize()
    {
        MaxHP = HitPoint;
    }

	// ======================
	// Battle
	// ======================
    public virtual void SkillInit()
    {
        SkillDefend = 0;
        SkillMagicDefend = 0;
    }
	public virtual void BeAttacked( AttackModule attack, Skill command )
	{
		int damage = Mathf.Max( 0, attack.AttackPower - DefendPower );
		BeDamaged( damage );
		if ( damage <= 0 )
		{
			SEPlayer.Play( ActionResult.Guarded, this is Player );
		}
		else
		{
			SEPlayer.Play( ActionResult.Damaged, this is Player );
		}
		Debug.Log( this.ToString() + " was Attacked! " + damage + "Damage! HitPoint is " + HitPoint );
	}
	public void BeMagicAttacked( MagicModule magic, Skill command )
    {
        int damage = Mathf.Max( 0, magic.MagicPower - MagicDefendPower );
        BeDamaged( damage );
        if( damage <= 0 )
        {
            SEPlayer.Play( ActionResult.Guarded, this is Player );
        }
        else
        {
            SEPlayer.Play( ActionResult.MagicDamaged, this is Player );
        }
        Debug.Log( this.ToString() + " was MagicAttacked! " + damage + "Damage! HitPoint is " + HitPoint );
	}

	protected virtual void BeDamaged( int damage )
	{
		HitPoint = Mathf.Max( 0, HitPoint - damage );
		damageTime = 0.15f + damage*0.015f;
	}

	public void Defend( DefendModule defend )
	{
        SkillDefend = defend.DefendPower;
	}

    public void MagicDefend( MagicDefendModule magicDefend )
    {
        SkillMagicDefend = magicDefend.MagicDefendPower;
    }
	public void Heal( HealModule heal )
	{
		HitPoint += heal.HealPoint;
		SEPlayer.Play( ActionResult.Healed );
		Debug.Log( this.ToString() + " used Heal! HitPoint is " + HitPoint );
	}
}
