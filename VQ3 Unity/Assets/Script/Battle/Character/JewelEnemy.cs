using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JewelEnemy : Enemy
{
    public override void BeAttacked( AttackModule attack, Skill skill )
    {
        if( attack.isPhysic )
        {
            base.BeAttacked( attack, skill );
        }
        else
        {
            SEPlayer.Play( "MagicNoDamage" );
            Debug.Log( this.ToString() + "was magic attacked but no damage." );
        }
    }
}
