using UnityEngine;
using System.Collections;

public class BGAnimBase : MonoBehaviour {

	public static BGAnimBase CurrentAnim { get; private set; }

	public bool IsActive = false;
	public int Cycle = 2;

	protected MidairPrimitive[] primitives_;

	// Use this for initialization
	void Start () {
		primitives_ = GetComponentsInChildren<MidairPrimitive>();
		transform.localScale = Vector3.zero;
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

		if( GameContext.State == GameState.Battle )
		{
			if( GameContext.BattleState == BattleState.Battle || GameContext.BattleState == BattleState.Eclipse || GameContext.BattleState == BattleState.Win )
			{
				transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, 0.2f);
				for( int i=0; i<primitives_.Length; ++i )
				{
					SetParams(primitives_[i], ((Music.MusicalTimeBar + Cycle * (float)i/primitives_.Length)%Cycle)/Cycle, i % 4 == 0);
				}
			}
		}
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
