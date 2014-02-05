using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {
    protected static readonly float DAMAGE_RANGE = 16.0f;
    protected static readonly float MAGIC_DAMAGE_RANGE = 8.0f;

    public int HitPoint;
    public int BasePower;
    public int BaseMagic;
    public int BaseDefend;
    public int BaseMagicDefend;

    protected int SkillDefend;
    protected int SkillMagicDefend;
    protected int SkillPower;
    protected int SkillMagic;

    public int DefendPower { get { return BaseDefend + SkillDefend; } }
    public int MagicDefendPower { get { return BaseMagicDefend + SkillMagicDefend; } }
    public int AttackPower { get { return BasePower + SkillPower; } }
    public int MagicPower { get { return BaseMagic + SkillMagic; } }

	protected float damageTime;
    protected int MaxHP;

    public GUIText debugText;

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
	public virtual void BeAttacked( AttackModule attack, Skill skill )
	{
        float ad = skill.OwnerCharacter.AttackPower * (attack.AttackPower / 100.0f);
        float damage = ad / ( (DefendPower / 100.0f) * (DefendPower / 100.0f) );
        damage *= ((100.0f - DAMAGE_RANGE) + Random.Range( 0, DAMAGE_RANGE ) + Random.Range( 0, DAMAGE_RANGE )) / 100.0f;
		BeDamaged( (int)damage );
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
    public void BeMagicAttacked( MagicModule magic, Skill skill )
    {
        float ad = skill.OwnerCharacter.MagicPower * (magic.MagicPower / 100.0f);
        float damage = ad / ((MagicDefendPower / 100.0f) * (MagicDefendPower / 100.0f));
        damage *= ((100.0f - MAGIC_DAMAGE_RANGE) + Random.Range( 0, MAGIC_DAMAGE_RANGE ) + Random.Range( 0, MAGIC_DAMAGE_RANGE )) / 100.0f;
        BeDamaged( (int)damage );
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

        if( debugText != null )
        {
            debugText.text = HitPoint.ToString() + ", " + heal.HealPoint + " healed!";
        }
	}
}
