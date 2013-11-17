using UnityEngine;
using System.Collections;

public class DefendEffect : CommandEffect{

    public Color Color;
    MidairPrimitive[] rects;

	// Use this for initialization
	void Start()
    {
        rects = GetComponentsInChildren<MidairPrimitive>();
        animation["DefendAnim"].speed = 1 / (float)(Music.mtBeat * Music.mtUnit);
	}
	
	// Update is called once per frame
	void Update () {
        rects[0].SetColor( Color );
        rects[1].SetColor( Color );
        rects[2].SetColor( Color );

        if( !animation.isPlaying )
        {
            Destroy( gameObject );
        }
	}
}
