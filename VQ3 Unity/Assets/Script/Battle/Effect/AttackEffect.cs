using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackEffect : MonoBehaviour {

    public Color Color;
    //public float radius;
    public float width;
    public float size;

    List<MidairPrimitive> triangles = new List<MidairPrimitive>();

	// Use this for initialization
	void Start () {
        triangles.AddRange( GetComponentsInChildren<MidairPrimitive>() );
        triangles.RemoveAt( triangles.Count - 1 );
        //for( int i = 0; i < rects.Length; i++ )
        //{
        //    rects[i].transform.rotation = Quaternion.AngleAxis( (i * 360.0f / rects.Length), Vector3.forward );
        //}
	}
	
	// Update is called once per frame
	void Update () {
        UpdateAnimation();
	}

    public void UpdateAnimation()
    {
        for( int i = 0; i < triangles.Count; i++ )
        {
            triangles[i].SetColor( Color );
            triangles[i].SetWidth( width );
            triangles[i].SetSize( size );
            //rects[i].transform.position = this.transform.position + Quaternion.AngleAxis( (i * 360.0f / rects.Length), Vector3.forward ) * (Vector3.down * radius);
        }
    }
}
