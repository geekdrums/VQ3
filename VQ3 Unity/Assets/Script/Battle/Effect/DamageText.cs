using UnityEngine;
using System.Collections;

public class DamageText : CounterSprite {

	static Vector3 PositionOffeset = new Vector3(-2, 3, 0);
    public Vector3 initialPosition { get; private set; }

    float time = 0;
	Color initialColor;

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
		//if( Music.isJustChanged || Music.isNowChanged )
		//{
		//	if( actionResult.ToString().EndsWith("GoodDamage") )
		//	{
		//		CounterColor = Music.isJustChanged ? initialColor : Color.white;
		//		GetComponentInChildren<TextMesh>().color = CounterColor;
		//	}
		//}
	}

	public void AddDamage( int damage )
	{
		Count += damage;
		time = 0;
	}

	public void Initialize(int damage, ActionResult actionResult, Vector3 initialPos)
    {
        Count = Mathf.Abs( damage );
		switch( actionResult )
        {
        case ActionResult.MagicDamage:
        case ActionResult.PhysicDamage:
			initialColor = ColorManager.Accent.Damage;
            transform.localScale = Vector3.one;
            break;
        case ActionResult.MagicBadDamage:
        case ActionResult.PhysicBadDamage:
			initialColor = ColorManager.Base.MiddleBack;
            transform.localScale = Vector3.one * 0.7f;
            break;
        case ActionResult.MagicGoodDamage:
        case ActionResult.PhysicGoodDamage:
			initialColor = ColorManager.Accent.Critical;
			if( GameContext.VoxSystem.State == VoxState.Overload )
			{
				transform.localScale = Vector3.one * 1.3f;
			}
			else
			{
				transform.localScale = Vector3.one * 1.0f;
			}
            break;
        case ActionResult.EnemyHeal:
			initialColor = ColorManager.Accent.Heal;
            transform.localScale = Vector3.one;
			foreach( TextMesh text in GetComponentsInChildren<TextMesh>() )
			{
				text.text = "HEAL";
			}
			break;
		case ActionResult.PlayerPhysicDamage:
		case ActionResult.PlayerMagicDamage:
			initialColor = ColorManager.Accent.PlayerDamage;
			transform.localScale = Vector3.one;
			break;
		case ActionResult.VPDrain:
			initialColor = ColorManager.Accent.Drain;
			transform.localScale = Vector3.one;
			foreach( TextMesh text in GetComponentsInChildren<TextMesh>() )
			{
				text.text = "VP DRAIN";
			}
			break;
        default:
			initialColor = ColorManager.Accent.Drain;
            transform.localScale = Vector3.one;
            break;
        }
		initialPosition = initialPos + PositionOffeset;
		CounterColor = initialColor;
		GetComponentInChildren<TextMesh>().color = CounterColor;
    }
}
