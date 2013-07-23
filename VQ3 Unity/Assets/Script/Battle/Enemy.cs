using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour {

	public AudioClip DamageSound;
	public AudioClip MagicDamageSound;
	public AudioSource AudioSource;

	public string CharacterName { get; protected set; }
	public int HitPoint { get; protected set; }

	float damageTime;

	// ======================
	// Initilaize/
	// ======================
	public void Initialize( EnemyProperty ep )
	{
		HitPoint = ep.HP;
		CharacterName = ep.Name;
		renderer.material.mainTexture = ep.Texture;
		transform.localScale *= ep.Transform.Scale;
		transform.position += ep.Transform.Position * 10;
	}

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
		Debug.Log( this.ToString() + " was Attacked! " + attack.AttackPower + "Damage! HitPoint is " + HitPoint );
	}
	public void BeMagicAttacked( MagicModule magic )
	{
		BeDamaged( magic.MagicPower );
		Debug.Log( this.ToString() + " was MagicAttacked! " + magic.MagicPower + "Damage! HitPoint is " + HitPoint );
	}

	void BeDamaged( int damage )
	{
		HitPoint -= damage;
		damageTime = 0.2f + damage*0.2f;
		AudioSource.clip = DamageSound;
		AudioSource.Play();
	}


	// ======================
	// Utils
	// ======================
	public override string ToString()
	{
		return CharacterName+GetInstanceID();
	}
}
