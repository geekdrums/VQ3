using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JewelEnemy : Enemy
{
    public override void BeMagicDamaged( int damage, Character ownerCharacter )
    {
        SEPlayer.Play( "MagicNoDamage" );
        Debug.Log( this.ToString() + "was magic attacked but no damage." );
        //base.BeMagicDamaged( damage, ownerCharacter );
    }
}
