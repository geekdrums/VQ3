using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JewelEnemy : Enemy
{

	public override void Update()
	{
		base.Update();
		if( currentState.name == "Drain" )
		{
			spriteRenderer.color = Color.Lerp(Color.black, Color.yellow, Music.MusicalCos(4, 0)*1.0f);
		}
		else
		{
			spriteRenderer.color = Color.black;
		}
	}
    public override void BeAttacked( AttackModule attack, Skill skill )
    {
		if( currentState.name == "Drain" )
		{
			if( attack.type == AttackType.Dain )
			{
				lastDamageResult = ActionResult.PhysicGoodDamage;
				float damage = skill.OwnerCharacter.PhysicAttack * (attack.Power / 100.0f) * DefendCoeff;
				BeDamaged(Mathf.Max(0, (int)damage), skill.OwnerCharacter);
				Debug.Log(this.ToString() + " was Attacked! " + damage + "Damage! HitPoint is " + HitPoint);
				SEPlayer.Play(lastDamageResult, skill.OwnerCharacter, (int)damage);
			}
			else
			{
				lastDamageResult = ActionResult.EnemyHeal;
				int oldHitPoint = HitPoint;
				int damage = (int)(skill.OwnerCharacter.PhysicAttack * (attack.Power / 100.0f) * DefendCoeff);
				int h = Mathf.Min(MaxHP - HitPoint, damage);
				if( h > 0 )
				{
					HitPoint += h;
				}
				CreateDamageText(-(HitPoint - oldHitPoint), ActionResult.EnemyHeal);
				HPCircle.OnHeal(HitPoint - oldHitPoint);
				SEPlayer.Play(ActionResult.EnemyHeal, this, HitPoint - oldHitPoint);
				Debug.Log(this.ToString() + "was magic attacked but drained.");
			}
		}
		else
		{
			if( attack.type == AttackType.Dain )
			{
				lastDamageResult = ActionResult.PhysicBadDamage;
				float damage = skill.OwnerCharacter.PhysicAttack * (attack.Power / 100.0f) * 0.1f * DefendCoeff;
				BeDamaged(Mathf.Max(0, (int)damage), skill.OwnerCharacter);
				Debug.Log(this.ToString() + " was Attacked! " + damage + "Damage! HitPoint is " + HitPoint);
				SEPlayer.Play(lastDamageResult, skill.OwnerCharacter, (int)damage);
			}
			else
			{
				SEPlayer.Play("MagicNoDamage");
				Debug.Log(this.ToString() + "was magic attacked but no damage.");
			}
		}
    }
}
