using UnityEngine;
using System.Collections;

public class QuarterRing : MonoBehaviour {

    public GameObject[] Quarters;
    public Color ActiveColor;
    public Color DeactiveColor;
    public Color WeakActiveColor;
    public Color WeakDeactiveColor;

    Color[] targetColors = new Color[4];
    int NumQuarter { get { return GameContext.PlayerConductor.NumQuarter; } }
    float blinkTime;
    readonly float BlinkInterval = 0.5f;

	// Use this for initialization
	void Start () {
        for( int i = 0; i < 4; i++ )
        {
            targetColors[i] = Color.clear;
            Quarters[i].renderer.material.color = targetColors[i];
        }
	}

    // Update is called once per frame
    void Update()
    {
        switch( GameContext.CurrentState )
        {
        case GameState.Intro:
            if( Music.isJustChanged )
            {
                for( int i = 0; i < 4; i++ )
                {
                    targetColors[i] = (i < NumQuarter ? ActiveColor : WeakActiveColor);
                }
            }
            break;
        case GameState.Battle:
            if( Music.IsJustChangedBar() )
            {
                for( int i = 0; i < 4; i++ )
                {
                    if( i == Music.Just.bar )
                    {
                        targetColors[i] = (i < NumQuarter ? ActiveColor : WeakActiveColor);
                    }
                    else
                    {
                        targetColors[i] = (i < NumQuarter ? DeactiveColor : WeakDeactiveColor);
                    }
                }
            }
            break;
        case GameState.Endro:
        case GameState.Continue:
            if( Music.isJustChanged )
            {
                for( int i = 0; i < 4; i++ )
                {
                    targetColors[i] = (i < NumQuarter ? DeactiveColor : WeakDeactiveColor);
                }
            }
            break;
        case GameState.Result:
            if( blinkTime > 0 )
            {
                blinkTime -= Time.deltaTime;
            }
            for( int i = 0; i < 4; i++ )
            {
                if( i == NumQuarter - 1 && blinkTime > 0 )
                {
                    float t = ( blinkTime % (BlinkInterval*2) ) / BlinkInterval;
                    targetColors[i] = Color.Lerp( DeactiveColor, ActiveColor, ( t > 1 ? 2 - t : t ) );
                }
                else
                {
                    targetColors[i] = (i < NumQuarter ? DeactiveColor : WeakDeactiveColor);
                }
            }
            break;
        }
        UpdateAnimation();
	}

    void UpdateAnimation()
    {
        for( int i = 0; i < 4; i++ )
        {
            Quarters[i].renderer.material.color = Color.Lerp( Quarters[i].renderer.material.color, targetColors[i], 0.3f );
        }
    }

    public void SetBlinkTime( float blinkTime )
    {
        this.blinkTime = blinkTime;
    }
}
