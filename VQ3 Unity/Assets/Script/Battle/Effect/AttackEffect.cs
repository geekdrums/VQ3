using UnityEngine;
using System.Collections;

public class AttackEffect : MonoBehaviour {

    public Color Color;
    public float radius;
    public float width;
    public float size;

    MidairPrimitive[] rects;

	// Use this for initialization
	void Start () {
        rects = GetComponentsInChildren<MidairPrimitive>();
        for( int i = 0; i < rects.Length; i++ )
        {
            rects[i].transform.rotation = Quaternion.AngleAxis( (i * 360.0f / rects.Length), Vector3.forward );
        }
	}
	
	// Update is called once per frame
	void Update () {
        UpdateAnimation();
	}

    public void UpdateAnimation()
    {
        for( int i = 0; i < rects.Length; i++ )
        {
            rects[i].SetColor( Color );
            rects[i].SetWidth( width );
            rects[i].SetSize( size );
            rects[i].transform.position = this.transform.position + Quaternion.AngleAxis( (i * 360.0f / rects.Length), Vector3.forward ) * (Vector3.down * radius);
        }
    }
}
