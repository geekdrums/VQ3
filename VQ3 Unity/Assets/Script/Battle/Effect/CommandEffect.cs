using UnityEngine;
using System.Collections;

public class CommandEffect : MonoBehaviour {

    public Color Color;
    public float width;
    public float size;
    public bool useColorAnim, useWidthAnim, useSizeAnim;

    protected MidairPrimitive[] primitives;

	// Use this for initialization
    void Start(){
        Initialize();
	}

    protected virtual void Initialize()
    {
        primitives = GetComponentsInChildren<MidairPrimitive>();
    }
	
	// Update is called once per frame
	void Update () {
        UpdateAnimation();
	}

    public virtual void UpdateAnimation()
    {
        for( int i = 0; i < primitives.Length; i++ )
        {
            if( useColorAnim ) primitives[i].SetColor( Color );
            if( useWidthAnim ) primitives[i].SetWidth( width );
            if( useSizeAnim )  primitives[i].SetSize( size );
        }
    }
}
