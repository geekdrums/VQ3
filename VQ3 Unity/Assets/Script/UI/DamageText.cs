using UnityEngine;
using System.Collections;

public class DamageText : MonoBehaviour
{
	public CounterSprite DamageCount;

	public float MaxCounterScale;
	public float MidCounterScale;
	public float MinCounterScale;

	float time_ = 0;
	bool isEnd_ = false;
	bool isPlayerDamage_ = false;

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

		if( isEnd_ )
		{
			return;
		}

		bool isEnd = false;
		time_ += Time.deltaTime;
		isEnd |= time_ >= 2.0f;
		isEnd |= GameContext.LuxSystem.State != LuxState.Overload && Music.IsJustChangedAt(3, 3, 0);
		if( isEnd )
		{
			isEnd_ = true;
			Destroy(this.gameObject);
		}
	}

	public void AddDamage(int damage)
	{
		DamageCount.Count += damage;
		time_ = 0;
		DamageCount.Shake(0.3f, 1.0f);
	}

	public void InitializeDamage(int damage, ActionResult actionResult, bool isPlayerDamage = false)
	{
		Color color;
		DamageCount.Count = Mathf.Abs(damage);
		DamageCount.Shake(0.4f, 1.0f);
		isPlayerDamage_ = isPlayerDamage;
		switch( actionResult )
		{
		case ActionResult.MagicDamage:
		case ActionResult.PhysicDamage:
			color = ColorManagerObsolete.Accent.Damage;
			transform.localScale = Vector3.one;
			break;
		case ActionResult.MagicBadDamage:
		case ActionResult.PhysicBadDamage:
			color = ColorManagerObsolete.Base.MiddleBack;
			transform.localScale = Vector3.one * 0.7f;
			break;
		case ActionResult.MagicGoodDamage:
		case ActionResult.PhysicGoodDamage:
			color = ColorManagerObsolete.Accent.Critical;
			if( GameContext.LuxSystem.State == LuxState.Overload )
			{
				transform.localScale = Vector3.one * 1.4f;
			}
			else if( GameContext.LuxSystem.State == LuxState.Break )
			{
				transform.localScale = Vector3.one * 1.2f;
			}
			else
			{
				transform.localScale = Vector3.one * 1.0f;
			}
			break;
		case ActionResult.EnemyHeal:
			color = ColorManagerObsolete.Accent.Heal;
			transform.localScale = Vector3.one;
			foreach( TextMesh text in GetComponentsInChildren<TextMesh>() )
			{
				text.text = "HEAL";
			}
			break;
		case ActionResult.PlayerPhysicDamage:
		case ActionResult.PlayerMagicDamage:
			color = ColorManagerObsolete.Accent.PlayerDamage;
			transform.localScale = Vector3.one;
			break;
		case ActionResult.VPDrain:
			color = ColorManagerObsolete.Accent.Drain;
			transform.localScale = Vector3.one;
			foreach( TextMesh text in GetComponentsInChildren<TextMesh>() )
			{
				text.text = "VP DRAIN";
			}
			break;
		default:
			color = ColorManagerObsolete.Accent.Drain;
			transform.localScale = Vector3.one;
			break;
		}
		DamageCount.GetComponentInChildren<TextMesh>().color = color;
		DamageCount.CounterColor = color;
	}

}
