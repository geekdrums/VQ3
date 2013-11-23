using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DefendBGEffect : BGEffect {

    public GameObject rectPrefab;
    public float spectrumGain;
    public float spectrumDelta;
    public float HeightInterval = 3.0f;
    public float BaseYPosition = 0;
    public float ColorFactor = 0.5f;

    readonly float MaxWidth = 75;
    readonly int MaxHeight = 5;

    List<GameObject[]> rects = new List<GameObject[]>();
    float[] spectrums = new float[16];

	// Use this for initialization
    void Start()
    {
        CriAtom.SetBusAnalyzer( true );
        for( int i = 0; i < 16; ++i )
        {
            GameObject[] primitives = new GameObject[1 + MaxHeight * 2];
            primitives[0] = ((GameObject)Instantiate( rectPrefab, GetRectPosition( i, 0 ), rectPrefab.transform.rotation ));
            for( int j = 0; j < MaxHeight; ++j )
            {
                primitives[1 + 2 * j] = ((GameObject)Instantiate( rectPrefab, GetRectPosition( i, 1 + 2 * j ), rectPrefab.transform.rotation ));
                primitives[2 + 2 * j] = ((GameObject)Instantiate( rectPrefab, GetRectPosition( i, 2 + 2 * j ), rectPrefab.transform.rotation ));
            }
            for( int j = 0; j < primitives.Length; ++j )
            {
                primitives[j].transform.parent = transform;
                primitives[j].transform.localScale = Vector3.zero;
            }
            rects.Add(primitives);
            spectrums[i] = 0;
        }
	}

    Vector3 GetRectPosition( int i, int j )
    {
        if( j == 0 ) return new Vector3( MaxWidth * (-0.5f + (i + 0.5f) / 16.0f), BaseYPosition, 20 );
        else
        {
            return (j % 2 == 1 ? new Vector3( MaxWidth * (-0.5f + (i + 0.5f) / 16.0f), BaseYPosition + HeightInterval * ((j + 1) / 2), 20 )
                               : new Vector3( MaxWidth * (-0.5f + (i + 0.5f) / 16.0f), BaseYPosition - HeightInterval * ((j + 1) / 2), 20 ));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if( GameContext.CurrentState != GameContext.GameState.Battle ) return;
        UpdateFade();
        
        if( Music.isJustChanged )
        {
            for( int i = 0; i < spectrums.Length; ++i )
            {
                if( spectrums[i] > 0 )
                {
                    spectrums[i] -= spectrumDelta;
                }
            }
            if( Music.UseADX )
            {
                CriAtomExAsr.BusAnalyzerInfo lBusInfo = CriAtom.GetBusAnalyzerInfo( 0 );
                spectrums[Music.Just.barUnit] = (int)(Mathf.Sqrt( lBusInfo.peakLevels[0] ) * spectrumGain);
            }
            else
            {
                spectrums[Music.Just.barUnit] = (int)(Mathf.Sqrt( AudioListener.GetSpectrumData(64,0,FFTWindow.Rectangular)[1] * 1.5f ) * spectrumGain);
            }
        }

        for( int i = 0; i < 16; ++i )
        {
            if( spectrums[i] > 0.0f )
            {
                rects[i][0].transform.localScale = Vector3.Lerp( rects[i][0].transform.localScale, rectPrefab.transform.localScale * fade, 0.3f );
            }
            Color c = rects[i][0].renderer.material.GetColor( "_TintColor" );
            rects[i][0].renderer.material.SetColor( "_TintColor", new Color( c.r, c.g, c.b, 0.2f + spectrums[i] * ColorFactor / (float)MaxHeight ) );
            for( int j = 0; j < MaxHeight; ++j )
            {
                if( j < spectrums[i] )
                {
                    rects[i][1 + 2 * j].transform.localScale = Vector3.Lerp( rects[i][1 + 2 * j].transform.localScale, rectPrefab.transform.localScale * fade, 0.3f );
                    rects[i][2 + 2 * j].transform.localScale = Vector3.Lerp( rects[i][2 + 2 * j].transform.localScale, rectPrefab.transform.localScale * fade, 0.3f );
                }
                else
                {
                    rects[i][1 + 2 * j].transform.localScale = Vector3.Lerp( rects[i][1 + 2 * j].transform.localScale, Vector3.zero, 0.15f );
                    rects[i][2 + 2 * j].transform.localScale = Vector3.Lerp( rects[i][2 + 2 * j].transform.localScale, Vector3.zero, 0.15f );
                }
            }
        }
	}

    void OnGUI()
    {
        for( int i = 0; i < 16; ++i )
        {
            for( int j = 0; j < rects[i].Length; ++j )
            {
                rects[i][j].transform.position = GetRectPosition( i, j );
            }
        }
    }
}
