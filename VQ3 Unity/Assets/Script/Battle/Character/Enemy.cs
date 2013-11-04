using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : Character {

    public Command[] Commands;

	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update ()
	{
		if ( damageTime > 0 )
		{
			renderer.material.color = ( damageTime % 0.1f > 0.05f ? Color.clear : GameContext.EnemyConductor.baseColor );
			damageTime -= Time.deltaTime;
			if ( damageTime <= 0 )
			{
				if ( HitPoint <= 0 )
				{
                    renderer.material.color = Color.clear;
                    Destroy( this.gameObject );
				}
				else
				{
					renderer.material.color = GameContext.EnemyConductor.baseColor;
				}
			}
		}
	}

    public Command GetExecCommand()
    {
        return Commands[0];
    }


	// ======================
	// Utils
	// ======================
	public override string ToString()
	{
        return name;
	}
}
