using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeatherEnemy : Enemy
{
    public int DensityPoint;
    public string WeatherName;
    public ParticleSystem weatherParticle;

    public bool IsSubstance { get { return currentState != WeatherState; } }

    string WeatherState;
    string SubstanceState;
    int MaxDensityPoint;
    float currentAlpha;

    public override void Initialize()
    {
        base.Initialize();
        WeatherState = StateNames[0];
        SubstanceState = StateNames[1];
        MaxDensityPoint = DensityPoint;

        Color c = renderer.material.color;
        currentAlpha = IsSubstance ? 1 : 0;
        c.a = currentAlpha;
        renderer.material.color = c;
        HPCircle.SetActive( false );
        weatherParticle.enableEmission = !IsSubstance;
    }

    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();
        float targetAlpha = (IsSubstance ? 1 : 0);
        float d = targetAlpha - currentAlpha;
        if( Mathf.Abs( d ) > 0.1f )
        {
            currentAlpha += d * 0.1f;
            d = targetAlpha - currentAlpha;
            if( Mathf.Abs( d ) <= 0.1f ) currentAlpha = targetAlpha;

            Color c = renderer.material.color;
            c.a = currentAlpha;
            renderer.material.color = c;
        }
    }

    public void ReceiveWeatherModule( WeatherModule wm )
    {
        if( this.WeatherName == wm.WeatherName )
        {
            if( IsSubstance && wm.Point > 0 )
            {
                DensityPoint += wm.Point;
                print( DensityPoint );
                if( DensityPoint >= MaxDensityPoint )
                {
                    DensityPoint = MaxDensityPoint;
                    currentState = WeatherState;
                    weatherParticle.enableEmission = true;
                    HPCircle.SetActive( false );
                }
            }
            else if( !IsSubstance && wm.Point < 0 )
            {
                DensityPoint += wm.Point;
                if( DensityPoint <= 0 )
                {
                    DensityPoint = 0;
                    currentState = SubstanceState;
                    weatherParticle.enableEmission = false;
                    HPCircle.SetActive( true );
                    renderer.material.color = GameContext.EnemyConductor.baseColor;
                }
            }
            print( "DensityPoint is " + DensityPoint + "! (wm.Point = " + wm.Point + ")" ); 
        }
    }
}
