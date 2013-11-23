using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DefendBGEffect : BGEffect {

    public GameObject rectPrefab;
    public float spectrumGain;

    readonly int MaxHeight = 4;
    readonly float MaxWidth = 72;
    List<MidairPrimitive[]> rects = new List<MidairPrimitive[]>();
    int[] spectrums = new int[16];
    AudioSpectrum audioSpectrum;
	// Use this for initialization
	void Start () {
        audioSpectrum = GetComponent<AudioSpectrum>();
        for( int i = 0; i < 16; ++i )
        {
            MidairPrimitive[] primitives = new MidairPrimitive[1 + MaxHeight*2];
            primitives[0] = ((GameObject)Instantiate( rectPrefab, new Vector3( MaxWidth*(-0.5f + i/8.0f), 0, 20 ), rectPrefab.transform.rotation )).GetComponent<MidairPrimitive>();
            for( int j = 0; j < MaxHeight; ++j )
            {
                primitives[1 + 2 * j] = ((GameObject)Instantiate( rectPrefab, new Vector3( MaxWidth * (-0.5f + i / 16.0f), 5*j, 20 ), rectPrefab.transform.rotation )).GetComponent<MidairPrimitive>();
                primitives[2 + 2 * j] = ((GameObject)Instantiate( rectPrefab, new Vector3( MaxWidth * (-0.5f + i / 16.0f), -5 * j, 20 ), rectPrefab.transform.rotation )).GetComponent<MidairPrimitive>();
            }
            for( int j = 0; j < primitives.Length; ++j )
            {
                primitives[j].RecalculatePolygon();
                primitives[j].SetWidth( 0 );
                primitives[j].SetSize( 0 );
                primitives[j].gameObject.transform.parent = transform;
            }
            rects.Add(primitives);
            spectrums[i] = 0;
        }
	}

    // Update is called once per frame
    void Update()
    {
        if( GameContext.CurrentState != GameContext.GameState.Battle ) return;

        if( Music.Just.totalUnit < 4 )//is showing
        {
            for( int i = 0; i < (int)((Music.MusicalTime+1)*4); ++i )
            {
                spectrums[Mathf.Min(spectrums.Length-1,i)] = 5;
            }
        }
        else
        {
            if( Music.isNowChanged )
            {
                for( int i = 0; i < audioSpectrum.MeanLevels.Length; ++i )
                {
                    if( audioSpectrum.MeanLevels[i] * spectrumGain > spectrums[i] )
                    {
                        spectrums[i] = (int)(audioSpectrum.MeanLevels[i] * spectrumGain);
                    }
                    else if( spectrums[i] > 0 )
                    {
                        --spectrums[i];
                    }
                }
            }
        }

        for( int i = 0; i < 16; ++i )
        {
            for( int j = 0; j < MaxHeight; ++j )
            {
                if( j < spectrums[(i + Music.Just.barUnit) % 16] )
                {
                    rects[i][1 + 2 * j].SetTargetWidth( 2 );
                    rects[i][1 + 2 * j].SetTargetSize( 2 );
                    rects[i][2 + 2 * j].SetTargetWidth( 2 );
                    rects[i][2 + 2 * j].SetTargetSize( 2 );
                }
                else
                {
                    rects[i][1 + 2 * j].SetTargetWidth( 0 );
                    rects[i][1 + 2 * j].SetTargetSize( 0 );
                    rects[i][2 + 2 * j].SetTargetWidth( 0 );
                    rects[i][2 + 2 * j].SetTargetSize( 0 );
                }
            }
        }
        
	}
}
