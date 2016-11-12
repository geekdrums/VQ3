using UnityEngine;
using System.Collections;

public class DamageText : MonoBehaviour
{
	public CounterSprite DamageCount;
	public CounterSprite VPCount;
	public CounterSprite VTCount;

	public float MaxCounterScale;
	public float MidCounterScale;
	public float MinCounterScale;

	float time_ = 0;
	bool isEnd_ = false;
	bool isPlayerDamage_ = false;
	DamageGauge.Mode mode_;
	Vector3 destination_;

	// Use this for initialization
	void Start()
	{
		destination_ = transform.parent.FindChild(isPlayerDamage_ ? "PlayerDestination" : "EnemyDestination").transform.localPosition;
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
		isEnd |= mode_ == DamageGauge.Mode.Break && GameContext.LuxSystem.IsOverFlow;
		isEnd |= GameContext.LuxSystem.State != LuxState.Overload && Music.IsJustChangedAt(3, 3, 0);
		if( isEnd )
		{
			isEnd_ = true;
			AnimManager.AddAnim(gameObject, destination_, ParamType.Position, AnimType.Time, 0.2f);
			AnimManager.AddAnim(gameObject, destination_ + ( isPlayerDamage_ ? Vector3.down : Vector3.up ) * 5, ParamType.Position, AnimType.Time, 0.2f, 0.3f, true);
		}
	}

	public void AddDamage(int damage)
	{
		DamageCount.Count += damage;
		time_ = 0;
		DamageCount.Shake(0.3f, 1.0f);
	}

	public void AddVP(int VP, int Time)
	{
		VTCount.Count += Time / LuxSystem.TurnMusicalUnits;
		VTCount.gameObject.SetActive(VTCount.Count > 0);
		VPCount.Count += VP;
		time_ = 0;
		UpdateVPColors(VP, Time / LuxSystem.TurnMusicalUnits);

		VPCount.Shake(0.3f, 1.0f);
		VTCount.Shake(0.3f, 1.0f);
	}


	void ModeInit(DamageGauge.Mode mode)
	{
		mode_ = mode;
		DamageCount.gameObject.SetActive(mode_ == DamageGauge.Mode.Damage || mode_ == DamageGauge.Mode.DamageAndTime);
		VPCount.gameObject.SetActive(mode_ == DamageGauge.Mode.Break);
		VTCount.gameObject.SetActive(mode_ == DamageGauge.Mode.Break);
	}

	public void InitializeVP(int VP, int Time)
	{
		ModeInit(DamageGauge.Mode.Break);
		VTCount.Count = Time / LuxSystem.TurnMusicalUnits;
		VTCount.gameObject.SetActive(VTCount.Count > 0);
		VPCount.Count = VP;

		VPCount.Shake(0.4f, 1.0f);
		VTCount.Shake(0.4f, 1.0f);

		UpdateVPColors(VP, Time / LuxSystem.TurnMusicalUnits);
		isPlayerDamage_ = false;
	}

	void UpdateVPColors(int VP, float Time)
	{
		Color vpColor = Color.white;
		if( VP >= 10 )
		{
			vpColor = ColorManager.Accent.Break;
			VPCount.CounterScale = MaxCounterScale;
		}
		else if( VP >= 4 )
		{
			vpColor = Color.Lerp(ColorManager.Accent.Break, ColorManager.Base.Bright, 0.3f);
			VPCount.CounterScale = MidCounterScale;
		}
		else
		{
			vpColor = ColorManager.Base.MiddleBack;
			VPCount.CounterScale = MinCounterScale;
		}
		VPCount.CounterColor = vpColor;
		VPCount.GetComponentInChildren<TextMesh>().color = vpColor;

		Color timeColor = Color.white;
		if( Time >= 0.5f )
		{
			timeColor = ColorManager.Accent.Time;
			VTCount.CounterScale = MaxCounterScale;
		}
		else if( Time >= 0.2f )
		{
			timeColor = Color.Lerp(ColorManager.Accent.Time, ColorManager.Base.Bright, 0.3f);
			VTCount.CounterScale = MidCounterScale;
		}
		else
		{
			timeColor = ColorManager.Base.MiddleBack;
			VTCount.CounterScale = MinCounterScale;
		}
		VTCount.CounterColor = timeColor;
		VTCount.GetComponentInChildren<TextMesh>().color = timeColor;
	}

	public void InitializeDamage(int damage, ActionResult actionResult, bool isPlayerDamage = false)
	{
		ModeInit(DamageGauge.Mode.Damage);
		Color color;
		DamageCount.Count = Mathf.Abs(damage);
		DamageCount.Shake(0.4f, 1.0f);
		isPlayerDamage_ = isPlayerDamage;
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
		DamageCount.GetComponentInChildren<TextMesh>().color = color;
		DamageCount.CounterColor = color;
	}

}
