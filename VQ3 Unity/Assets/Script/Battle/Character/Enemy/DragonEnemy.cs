using UnityEngine;
using System.Collections;

public class DragonEnemy : Enemy {

	public SpriteRenderer ShadowSprite;

	int firePower_ = 0;
	Vector3 initialScale;

	// Use this for initialization
	void Start()
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
		switch( currentState.name )
		{
		case "Default":
			if( skill.name.StartsWith( "breath" ) )
			{
				firePower_ = 3;
			}
			else if( skill.name.StartsWith( "ignis" ) && skill.Actions.IndexOf(act) == 0 )
			{
				--firePower_;
				if( firePower_ <= 0 )
				{
					ShadowSprite.color = Color.clear;
				}
			}
			break;
		case "Breath":
			break;
		case "Crow":
			if( skill.name.StartsWith( "moveBack" ) )
			{
				transform.localScale = initialScale;
			}
			else if( skill.name.StartsWith( "move" ) )
			{
				transform.localScale = initialScale * 1.7f;
			}
			break;
		}
	}

	public override void OnRevert()
	{
		base.OnRevert();
		firePower_ = 0;
		ShadowSprite.color = Color.clear;
		transform.localScale = initialScale;
	}

	public override void TurnInit( CommandBase command )
	{
		base.TurnInit(command);
		if( currentState.name != "Default" )
		{
			firePower_ = 0;
			ShadowSprite.color = Color.clear;
		}
	}
}
