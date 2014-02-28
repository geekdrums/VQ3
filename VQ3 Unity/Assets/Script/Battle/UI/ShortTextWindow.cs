using UnityEngine;
using System.Collections;

public class ShortTextWindow : MonoBehaviour {

    string text;
    TextMesh textMesh;
    int remainingMT;
    public void Initialize( string text, int mt = 16 )
    {
        this.text = text;
        remainingMT = mt;
    }

	// Use this for initialization
	void Start () {
        textMesh = GetComponentInChildren<TextMesh>();
	}
	
	// Update is called once per frame
	void Update () {
        if( textMesh.text == "" && animation.isPlaying )
        {
            textMesh.text = text;
        }
        if( Music.isJustChanged )
        {
            --remainingMT;
        }
        if( remainingMT <= 0 )
        {
            Destroy( gameObject );
        }
	}
}
