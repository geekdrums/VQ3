using UnityEngine;
using System.Collections;

public enum ActionResult
{
    PlayerDefend,
    PlayerPhysicDamage,
    PlayerMagicDamage,
    PhysicDamage,
    PhysicGoodDamage,
    PhysicBadDamage,
	PhysicShieldDamage,
	MagicDamage,
    MagicGoodDamage,
    MagicBadDamage,
	MagicShieldDamage,
	PlayerHeal,
    EnemyHeal,
	VPDrain,
	NoDamage,
}

public class SEPlayer : MonoBehaviour {

    static SEPlayer instance;
    CriAtomSource atomSource;

	// Use this for initialization
	void Start () {
	    instance = this;
        atomSource = GetComponent<CriAtomSource>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public static void Play( ActionResult act, Character target, int damage, string aisac = "", float aisacValue = 0 )
    {
		instance.Play_(act, target, damage, aisac, aisacValue);
    }

    public static void Play( string cueName )
    {
        instance.Play_( cueName );
    }

    void Play_( string cueName )
    {
        atomSource.Play( cueName );
    }

	void Play_(ActionResult act, Character target, int damage, string aisac = "", float aisacValue = 0)
    {
		if( aisac != "" )
		{
			atomSource.SetAisac(aisac, aisacValue);
		}
        Play_( act.ToString() );
    }
}
