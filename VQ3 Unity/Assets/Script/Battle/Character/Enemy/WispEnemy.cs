using UnityEngine;
using System.Collections;

public class WispEnemy : Enemy {

	public GameObject SunSpotPrefab;
	public Vector3 flareBGOffset;

	MidairPrimitive sunSpot_;
	bool isFlarePlaying = false;
	Vector3 initialSunspotPos;


	// Use this for initialization
	public override void Start()
	{
		Initialize();
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update();
		if( isFlarePlaying )
		{
			GameContext.LuxSystem.BGOffset = Vector3.Lerp(GameContext.LuxSystem.BGOffset, flareBGOffset, 0.1f);
			if( sunSpot_ != null )
			{
				sunSpot_.transform.localPosition = Vector3.Lerp(sunSpot_.transform.localPosition, initialSunspotPos, 0.1f);
				sunSpot_.transform.localScale = Vector3.Lerp(sunSpot_.transform.localScale, Vector3.one, 0.1f);
				if( (sunSpot_.transform.localPosition - initialSunspotPos).magnitude < 0.1f )
				{
					Destroy(sunSpot_.gameObject);
				}
			}
		}
		else
		{
			GameContext.LuxSystem.BGOffset = Vector3.Lerp(GameContext.LuxSystem.BGOffset, Vector3.zero, 0.1f);
			if( sunSpot_ != null )
			{
				Vector3 targetOffset = Quaternion.AngleAxis(360.0f * (float)Music.MusicalTime/16.0f,Vector3.up) * Vector3.right;
				sunSpot_.transform.localPosition = Vector3.Lerp(sunSpot_.transform.localPosition, initialSunspotPos + targetOffset, 0.1f);
				sunSpot_.transform.localScale = Vector3.Lerp(sunSpot_.transform.localScale, Vector3.one * ( 1.0f + targetOffset.z * 0.3f ), 0.1f);
			}
		}
	}

	/*
	public override void OnSkillEnd( Skill skill )
	{
		base.OnSkillEnd(skill);
		switch( currentState.name )
		{
		case "Flare":
			if( skill.name.StartsWith( "sunspot" ) )
			{
				sunSpot_ = (Instantiate(SunSpotPrefab) as GameObject).GetComponent<MidairPrimitive>();
				sunSpot_.transform.parent = transform;
				initialSunspotPos = sunSpot_.transform.localPosition;
			}
			else if( skill.name.StartsWith("blackflare") )
			{
				isFlarePlaying = false;
			}
			break;
		}
	}

	public override void OnExecuted( Skill skill, ActionSet act )
	{
		base.OnExecuted(skill, act);
		switch( currentState.name )
		{
		case "Flare":
			if( skill.name.StartsWith("blackflare") )
			{
				isFlarePlaying = true;
				SEPlayer.Play("flare");
			}
			break;
		}
	}
	*/

	public override void OnDead()
	{
		base.OnDead();

		isFlarePlaying = false;
		if( sunSpot_ != null )
		{
			Destroy(sunSpot_.gameObject);
		}
	}

	public override void InvertInit()
	{
		base.InvertInit();

		isFlarePlaying = false;
		if( sunSpot_ != null )
		{
			Destroy(sunSpot_.gameObject);
		}
	}
}
