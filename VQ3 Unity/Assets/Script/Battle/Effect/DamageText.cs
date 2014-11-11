using UnityEngine;
using System.Collections;

public class DamageText : CounterSprite {

    public Vector3 initialPosition { get; private set; }

    float time = 0;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
#if UNITY_EDITOR
        if( UnityEditor.EditorApplication.isPlaying == false )
        {
            return;
        }
#endif
        time += Time.deltaTime;
        float theta = time * (Mathf.PI * 2) * 8.0f;
        if( theta <= Mathf.PI * 2 )
        {
            float AnimY = Mathf.Sin( theta ) * 0.4f;
            transform.position = initialPosition + Vector3.up * AnimY;
        }
        if( time >= 2.0f )
        {
            Destroy( gameObject );
        }
	}


    public void Initialize( int damage, ActionResult actResult, Vector3 initialPos )
    {
        Count = Mathf.Abs( damage );
        switch( actResult )
        {
        case ActionResult.MagicDamage:
        case ActionResult.PhysicDamage:
            CounterColor = ColorManager.Accent.Damage;
            transform.localScale = Vector3.one;
            break;
        case ActionResult.MagicBadDamage:
        case ActionResult.PhysicBadDamage:
            CounterColor = ColorManager.Base.MiddleBack;
            transform.localScale = Vector3.one * 0.7f;
            break;
        case ActionResult.MagicGoodDamage:
        case ActionResult.PhysicGoodDamage:
            CounterColor = ColorManager.Accent.Critical;
            transform.localScale = Vector3.one * 1.3f;
            break;
        case ActionResult.EnemyHeal:
            CounterColor = ColorManager.Accent.Heal;
            transform.localScale = Vector3.one;
            break;
        default:
            CounterColor = Color.black;
            transform.localScale = Vector3.one;
            break;
        }
        initialPosition = initialPos;
    }
}
