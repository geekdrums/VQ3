using UnityEngine;
using System.Collections;

public enum ActionResult
{
    Damaged,
    MagicDamaged,
    Guarded,
    Healed,
    //Setoff,
    //Reflected,
}

public class SEPlayer : MonoBehaviour {

    public static SEPlayer instance;
    public AudioSource AudioSource;
    public AudioClip PlayerDamageSound, GuardSound, HealSound, EnemyDamageSound, MagicDamageSound;

	// Use this for initialization
	void Start () {
	    instance = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}


    public static void Play( ActionResult act, bool isPlayer = true )
    {
        instance.Play_( act, isPlayer );
    }

    void Play_( ActionResult act, bool isPlayer )
    {
        switch( act )
        {
        case ActionResult.Damaged:
            AudioSource.PlayOneShot( isPlayer ? PlayerDamageSound : EnemyDamageSound );
            break;
        case ActionResult.Guarded:
            AudioSource.PlayOneShot( GuardSound );
            break;
        case ActionResult.MagicDamaged:
            AudioSource.PlayOneShot( MagicDamageSound );
            break;
        case ActionResult.Healed:
            AudioSource.PlayOneShot( HealSound );
            break;
        }
    }
}
