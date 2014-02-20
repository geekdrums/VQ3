using UnityEngine;
using System.Collections;

public enum ActionResult
{
    Damaged,
    MagicDamaged,
    Healed,
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

    public static void Play( ActionResult act, Character target, int damage )
    {
        instance.Play_( act, target, damage );
    }

    public static void Play( string cueName )
    {
        instance.Play_( cueName );
    }

    void Play_( string cueName )
    {
        atomSource.Play( cueName );
    }

    void Play_( ActionResult act, Character target, int damage )
    {
        Player targetPlayer = target as Player;
        Enemy targetEnemy = target as Enemy;
        switch( act )
        {
        case ActionResult.Damaged:
            if( targetPlayer != null )
            {
                if( damage <= 0 )
                {
                    Play_( "PlayerDefend" );
                }
                else
                {
                    Play_( "PlayerPhysicDamage" );
                }
            }
            else if( targetEnemy != null )
            {
                switch( targetEnemy.Speceis )
                {
                case EnemySpecies.Human:
                case EnemySpecies.Thing:
                case EnemySpecies.Fairy:
                case EnemySpecies.Jewel:
                    Play_( "PhysicDamage" );
                    break;
                case EnemySpecies.Spirit:
                case EnemySpecies.Dragon:
                    Play_( "PhysicGoodDamage" );
                    break;
                case EnemySpecies.Beast:
                    Play_( "PhysicBadDamage" );
                    break;
                case EnemySpecies.Weather:
                    break;
                }
            }
            else Debug.LogError( "target is invalid! -> " + target );
            break;
        case ActionResult.MagicDamaged:
            if( targetPlayer != null )
            {
                if( damage <= 0 )
                {
                    Play_( "PlayerDefend" );
                }
                else
                {
                    Play_( "PlayerMagicDamage" );
                }
            }
            else if( targetEnemy != null )
            {
                switch( targetEnemy.Speceis )
                {
                case EnemySpecies.Human:
                case EnemySpecies.Thing:
                case EnemySpecies.Spirit:
                case EnemySpecies.Weather:
                    Play_( "MagicDamage" );
                    break;
                case EnemySpecies.Fairy:
                case EnemySpecies.Beast:
                    Play_( "MagicGoodDamage" );
                    break;
                case EnemySpecies.Dragon:
                    Play_( "MagicBadDamage" );
                    break;
                case EnemySpecies.Jewel:
                    break;
                }
            }
            else Debug.LogError( "magic target is invalid! -> " + target );
            break;
        case ActionResult.Healed:
            Play_( (targetPlayer != null ? "Player" : "Enemy") + "Heal" );
            break;
        }
    }
}
