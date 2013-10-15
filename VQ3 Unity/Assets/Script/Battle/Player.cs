using UnityEngine;
using System.Collections;

public class Player : Character {
	Vector3 initialPosition;
	GUILayer guiLayer;

	// Use this for initialization
	void Start()
	{
		HitPoint = 10;
		guiLayer = GetComponent<GUILayer>();
		initialPosition = guiLayer.transform.position;
		DefendPower = 0;
		AttackPower = 0;
	}
	
	// Update is called once per frame
	void Update () {
		if ( damageTime > 0 )
		{
			if ( (int)( damageTime/0.05f ) != (int)( (damageTime+Time.deltaTime)/0.05f ) )
			{
				guiLayer.transform.position = initialPosition + Random.insideUnitSphere;
			}
			damageTime -= Time.deltaTime;
			if ( damageTime <= 0 )
			{
				guiLayer.transform.position = initialPosition;
			}
		}
	}



	public override string ToString()
	{
		return "Player";
	}

	public void OnBarStarted( int CurrentIndex )
	{
		DefendPower = 0;
		if ( CurrentIndex == 0 )
		{
			AttackPower = 0;
		}
	}

	public override void BeAttacked( AttackModule attack, Command command )
	{
		base.BeAttacked( attack, command );
		if ( HitPoint <= 0 )
		{
			GameContext.BattleConductor.OnPlayerLose();
		}
	}
}
