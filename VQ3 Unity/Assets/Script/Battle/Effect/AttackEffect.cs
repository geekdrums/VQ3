using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackEffect : MonoBehaviour {

    public Color Color;
    //public float radius;
    public float width;
    public float size;
    public float interval;

    MidairPrimitive[] triangles;

	// Use this for initialization
	void Start () {
        triangles = GetComponentsInChildren<MidairPrimitive>();
	}
	
	// Update is called once per frame
	void Update () {
        UpdateAnimation();
	}

    public void UpdateAnimation()
    {
        for( int i = 0; i < triangles.Length; i++ )
        {
            triangles[i].SetColor( Color );
            triangles[i].SetWidth( width );
            triangles[i].SetSize( size );
            triangles[i].transform.localPosition = new Vector3( triangles[i].transform.localPosition.x, interval * (i - 1.5f), triangles[i].transform.localPosition.z );
        }
    }
}
