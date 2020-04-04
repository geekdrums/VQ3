using UnityEngine;
using System.Collections;

public class BGAnimBase : MonoBehaviour {

	public static BGAnimBase CurrentAnim { get; private set; }

	protected static readonly float DamageTrembleTime = 0.025f;

	public bool IsActive = false;
	public int Cycle = 2;

	protected MidairPrimitive[] primitives_;
	protected float damageTime_;
	protected Vector3 initialPosition_;

	// Use this for initialization
	protected virtual void Start () {
		primitives_ = GetComponentsInChildren<MidairPrimitive>();
		transform.localScale = Vector3.zero;
		initialPosition_ = transform.localPosition;
	}

	// Update is called once per frame
	protected virtual void Update()
	{
		if( IsActive == false )
		{
			transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
			if( transform.localScale.magnitude < 0.1f )
			{
				transform.localScale = Vector3.zero;
				gameObject.SetActive(false);
			}
			return;
		}

		UpdateDamageAnim();
		if( GameContext.State == GameState.Battle )
		{
			if( GameContext.BattleState == BattleState.Battle || GameContext.BattleState == BattleState.Eclipse || GameContext.BattleState == BattleState.Win )
			{
				transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, 0.2f);
				for( int i=0; i<primitives_.Length; ++i )
				{
					SetParams(primitives_[i], ((Music.MusicalTime + Cycle * (float)i/primitives_.Length)%Cycle)/Cycle, i % 4 == 0);
				}
			}
		}
	}

	protected void UpdateDamageAnim()
	{
		if( damageTime_ > 0 )
		{
			if( (int)(damageTime_ / DamageTrembleTime) != (int)((damageTime_ + Time.deltaTime) / DamageTrembleTime) )
			{
				transform.localPosition = initialPosition_ + Random.insideUnitSphere * Mathf.Clamp(damageTime_ -  GameContext.PlayerConductor.PlayerDamageTimeMin, 0.2f, 2.0f) * GameContext.PlayerConductor.EnemyDamageBGShake;
			}

			damageTime_ -= Time.deltaTime;
			if( damageTime_ <= 0 )
			{
				transform.localPosition = initialPosition_;
			}
		}
	}

	public void OnDamage(float damageTime)
	{
		damageTime_ = damageTime;
	}

	public void Activate()
	{
		if( CurrentAnim == this )
		{
			return;
		}
		if( CurrentAnim != null )
		{
			CurrentAnim.transform.localScale = Vector3.zero;
			CurrentAnim.gameObject.SetActive(false);
			CurrentAnim.Deactivate();
		}
		gameObject.SetActive(true);
		IsActive = true;
		CurrentAnim = this;
	}

	public void Deactivate()
	{
		IsActive = false;
		CurrentAnim = null;
	}

	protected virtual void SetParams(MidairPrimitive primitive, float t, bool accent)
	{
	}

	public static void DeactivateCurrentAnim()
	{
		if( CurrentAnim != null )
		{
			CurrentAnim.Deactivate();
		}
	}
}
