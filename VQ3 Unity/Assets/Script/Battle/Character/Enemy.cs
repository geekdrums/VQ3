using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : Character {

    public Command[] Commands;

    SpriteRenderer HPCircle;
    Vector3 baseHPCircleScale;

    //todo: multiply EnemyConductor's coeff
    float targetHPCircleSize;// { get { return Mathf.Sqrt( HitPoint ); } }
    Color targetHPCircleColor;// { get { return Color.Lerp( Color.clear, GameContext.EnemyConductor.baseColor, Mathf.Sqrt( (float)HitPoint / (float)MaxHP ) ); } }

	// Use this for initialization
	void Start()
	{
        base.Initialize();
        HPCircle = GetComponentsInChildren<SpriteRenderer>()[1];
        if( HPCircle != null )
        {
            baseHPCircleScale = HPCircle.transform.localScale / Mathf.Sqrt( GameContext.EnemyConductor.baseHP );
            targetHPCircleSize = Mathf.Sqrt( MaxHP );
            HPCircle.transform.localScale = baseHPCircleScale * targetHPCircleSize;
            targetHPCircleColor = GameContext.EnemyConductor.baseColor;
            HPCircle.color = targetHPCircleColor;
        }
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
        UpdateAnimation();
	}

    void UpdateAnimation()
    {
        HPCircle.transform.localScale = Vector3.Lerp( HPCircle.transform.localScale, baseHPCircleScale * targetHPCircleSize, 0.1f );
        HPCircle.color = Color.Lerp( targetHPCircleColor, HPCircle.color, 0.1f );
    }

    public Command GetExecCommand()
    {
        return Commands[0];
    }

    protected override void BeDamaged( int damage )
    {
        base.BeDamaged( damage );
        if( HPCircle != null )
        {
            targetHPCircleSize = Mathf.Sqrt( HitPoint );
            targetHPCircleColor = Color.Lerp( Color.clear, GameContext.EnemyConductor.baseColor, Mathf.Sqrt( (float)HitPoint / (float)MaxHP ) );
        }
    }
    public void OnBaseColorChanged( Color newColor )
    {
        renderer.material.color = newColor;
        targetHPCircleColor = Color.Lerp( Color.clear, newColor, Mathf.Sqrt( (float)HitPoint / (float)MaxHP ) );
        HPCircle.color = targetHPCircleColor;
    }


	// ======================
	// Utils
	// ======================
	public override string ToString()
	{
        return name;
	}
}
