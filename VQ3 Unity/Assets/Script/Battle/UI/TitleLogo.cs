using UnityEngine;
using System.Collections;

public class TitleLogo : MonoBehaviour {

    public float ColorAlpha = 1.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        GetComponent<SpriteRenderer>().color = Color.white * ColorAlpha;
        foreach( TextMesh textMesh in GetComponentsInChildren<TextMesh>() )
        {
            textMesh.color = Color.white * ColorAlpha;
        }
	}
}
