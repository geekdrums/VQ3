﻿using UnityEngine;
using System.Collections;

public class DragonEnemy : Enemy {

	public SpriteRenderer ShadowSprite;

	int firePower_ = 0;
	Vector3 initialScale;

	// Use this for initialization
	public override void Start()
	{
		Initialize();
		ShadowSprite.color = Color.clear;
		initialScale = transform.localScale;
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update();
		if( firePower_ > 0 )
		{
			ShadowSprite.color = Color.Lerp(Color.clear, Color.red, (Mathf.Sin(Mathf.PI * (float)Music.MusicalTime / Mathf.Pow(2, firePower_)) + 1.0f) /2.0f);
		}
	}

	public override void OnExecuted( Skill skill, ActionSet act )
	{
		base.OnExecuted(skill, act);
		//switch( currentState.name )
		//{
		//case "Default":
		//	if( skill.name.StartsWith( "breath" ) )
		//	{
		//		firePower_ = 3;
		//		SEPlayer.Play("breath");
		//	}
		//	else if( skill.name.StartsWith( "ignis" ) && skill.Actions.IndexOf(act) == 0 )
		//	{
		//		--firePower_;
		//		if( firePower_ <= 0 )
		//		{
		//			ShadowSprite.color = Color.clear;
		//		}
		//	}
		//	break;
		//case "Breath":
		//	if( skill.name.StartsWith("poisonBreath") )
		//	{
		//		SEPlayer.Play("poisonBreath");
		//	}
		//	break;
		//case "Crow":
		//	if( skill.name.StartsWith( "moveBack" ) )
		//	{
		//		transform.localScale = initialScale;
		//		SEPlayer.Play("footstep");
		//	}
		//	else if( skill.name.StartsWith( "move" ) )
		//	{
		//		transform.localScale = initialScale * 1.7f;
		//		SEPlayer.Play("footstep");
		//	}
		//	break;
		//}
	}

	public void OnRevert()
	{
		//currentState = States.Find(( BattleState state ) => state.name == "Default");
		firePower_ = 0;
		ShadowSprite.color = Color.clear;
		transform.localScale = initialScale;
	}

	public override void TurnInit( CommandBase command )
	{
		base.TurnInit(command);
		//if( currentState.name != "Default" )
		//{
		//	firePower_ = 0;
		//	ShadowSprite.color = Color.clear;
		//}
	}
}
