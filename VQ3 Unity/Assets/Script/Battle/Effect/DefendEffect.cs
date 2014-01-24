using UnityEngine;
using System.Collections;

public class DefendEffect : CommandEffect{

	// Use this for initialization
	void Start()
    {
        animation["DefendAnim"].speed = 1 / (float)(Music.mtBeat * Music.mtUnit);
        Initialize();
	}
	
	// Update is called once per frame
    
	void Update () {
        UpdateAnimation();

        if( !animation.isPlaying )
        {
            Destroy( gameObject );
        }
	}
}
