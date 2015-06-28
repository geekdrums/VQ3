using UnityEngine;
using System.Collections;

public class DefendEffect : CommandEffect{

	// Use this for initialization
	void Start()
    {
		GetComponent<Animation>()["DefendAnim"].speed = 1 / (float)(Music.CurrentUnitPerBeat * Music.MusicalTimeUnit);
        Initialize();
	}
	
	// Update is called once per frame
    
	void Update () {
        UpdateAnimation();

        if( !GetComponent<Animation>().isPlaying )
        {
            Destroy( gameObject );
        }
	}
}
