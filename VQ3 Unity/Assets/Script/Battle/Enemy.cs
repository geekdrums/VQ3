using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour {

    public int HitPoint;
    float damageTime;

	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update ()
	{
		if ( damageTime > 0 )
		{
			renderer.material.color = ( damageTime % 0.1f > 0.05f ? Color.clear : Color.black );
			damageTime -= Time.deltaTime;
			if ( damageTime <= 0 )
			{
				if ( HitPoint <= 0 )
				{
                    renderer.material.color = Color.clear;
                    Destroy( this.gameObject );
				}
				else
				{
                    renderer.material.color = Color.black;
				}
			}
		}
	}


	// ======================
	// Battle
	// ======================
	public void BeAttacked( AttackModule attack )
	{
		BeDamaged( attack.AttackPower );
        SEPlayer.Play( ActionResult.Damaged, false );
		Debug.Log( this.ToString() + " was Attacked! " + attack.AttackPower + "Damage! HitPoint is " + HitPoint );
	}
	public void BeMagicAttacked( MagicModule magic )
	{
        BeDamaged( magic.MagicPower );
        SEPlayer.Play( ActionResult.MagicDamaged, false );
		Debug.Log( this.ToString() + " was MagicAttacked! " + magic.MagicPower + "Damage! HitPoint is " + HitPoint );
	}

	void BeDamaged( int damage )
	{
		HitPoint -= damage;
		damageTime = 0.2f + damage*0.2f;
	}


	// ======================
	// Utils
	// ======================
	public override string ToString()
	{
        return name;
	}
}
