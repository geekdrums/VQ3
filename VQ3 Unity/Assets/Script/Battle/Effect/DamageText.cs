using UnityEngine;
using System.Collections;

public class DamageText : CounterSprite
{
	float time = 0;

	// Use this for initialization
	void Start()
	{

	}


	// Update is called once per frame
	void Update()
	{
#if UNITY_EDITOR
		if( UnityEditor.EditorApplication.isPlaying == false )
		{
			return;
		}
#endif
		time += Time.deltaTime;
		if( time >= 2.0f )
		{
			Destroy(gameObject);
		}
	}

	public void AddDamage(int damage)
	{
		Count += damage;
		time = 0;
	}

	public void Initialize(int damage, ActionResult actionResult)
	{
		Color color;
		Count = Mathf.Abs(damage);
		switch( actionResult )
		{
		case ActionResult.MagicDamage:
		case ActionResult.PhysicDamage:
			color = ColorManager.Accent.Damage;
			transform.localScale = Vector3.one;
			break;
		case ActionResult.MagicBadDamage:
		case ActionResult.PhysicBadDamage:
			color = ColorManager.Base.MiddleBack;
			transform.localScale = Vector3.one * 0.7f;
			break;
		case ActionResult.MagicGoodDamage:
		case ActionResult.PhysicGoodDamage:
			color = ColorManager.Accent.Critical;
			if( GameContext.LuxSystem.State == LuxState.Overload )
			{
				transform.localScale = Vector3.one * 1.4f;
			}
			else if( GameContext.LuxSystem.State == LuxState.Overflow )
			{
				transform.localScale = Vector3.one * 1.2f;
			}
			else
			{
				transform.localScale = Vector3.one * 1.0f;
			}
			break;
		case ActionResult.EnemyHeal:
			color = ColorManager.Accent.Heal;
			transform.localScale = Vector3.one;
			foreach( TextMesh text in GetComponentsInChildren<TextMesh>() )
			{
				text.text = "HEAL";
			}
			break;
		case ActionResult.PlayerPhysicDamage:
		case ActionResult.PlayerMagicDamage:
			color = ColorManager.Accent.PlayerDamage;
			transform.localScale = Vector3.one;
			break;
		case ActionResult.VPDrain:
			color = ColorManager.Accent.Drain;
			transform.localScale = Vector3.one;
			foreach( TextMesh text in GetComponentsInChildren<TextMesh>() )
			{
				text.text = "VP DRAIN";
			}
			break;
		default:
			color = ColorManager.Accent.Drain;
			transform.localScale = Vector3.one;
			break;
		}
		GetComponentInChildren<TextMesh>().color = color;
		CounterColor = color;
	}

}
