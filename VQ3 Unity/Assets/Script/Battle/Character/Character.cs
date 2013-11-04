using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {

	public int HitPoint;
	public int DefendPower { get; protected set; }
	public int AttackPower { get; protected set; }

	protected float damageTime;

	// ======================
	// Battle
	// ======================
	public virtual void BeAttacked( AttackModule attack, Command command )
	{
		int damage = Mathf.Max( 0, attack.AttackPower + command.OwnerCharacter.AttackPower - DefendPower );
		BeDamaged( damage );
		if ( damage <= 0 )
		{
			SEPlayer.Play( ActionResult.Guarded, this is Player );
		}
		else
		{
			SEPlayer.Play( ActionResult.Damaged, this is Player );
		}
		//Debug.Log( this.ToString() + " was Attacked! " + damage + "Damage! HitPoint is " + HitPoint );
	}
	public void BeMagicAttacked( MagicModule magic, Command command )
	{
		BeDamaged( magic.MagicPower );
		SEPlayer.Play( ActionResult.MagicDamaged, this is Player );
		//Debug.Log( this.ToString() + " was MagicAttacked! " + magic.MagicPower + "Damage! HitPoint is " + HitPoint );
	}

	void BeDamaged( int damage )
	{
		HitPoint -= damage;
		damageTime = 0.15f + damage*0.15f;
	}

	public void Defend( DefendModule defend )
	{
		DefendPower = defend.DefendPower;
	}
	public void PowerUp( PowerModule power )
	{
		AttackPower += power.AttackPower;
	}
	public void Heal( HealModule heal )
	{
		HitPoint += heal.HealPoint;
		SEPlayer.Play( ActionResult.Healed );
		//Debug.Log( this.ToString() + " used Heal! HitPoint is " + HitPoint );
	}
}
