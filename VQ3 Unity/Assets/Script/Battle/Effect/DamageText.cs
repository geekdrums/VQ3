using UnityEngine;
using System.Collections;

public class DamageText : CounterSprite {

    public Vector3 initialPosition { get; private set; }
    public Color damageColor;
    public Color healColor;

    float time = 0;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        time += Time.deltaTime;
        float theta = time * (Mathf.PI * 2) * 8.0f;
        if( theta <= Mathf.PI * 2 )
        {
            float AnimY = Mathf.Sin( theta ) * 0.4f;
            transform.position = initialPosition + Vector3.up * AnimY;
        }
        if( time >= 2.0f )
        {
            Destroy( gameObject );
        }
	}


    public void Initialize( int damage, Vector3 initialPos )
    {
        count = Mathf.Abs( damage );
        foreach( SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>() )
        {
            //sprite.gameObject.renderer.material.SetColor( "Tint", (damage < 0 ? healColor : damageColor) );
            sprite.renderer.material.color = (damage < 0 ? healColor : damageColor);
            //sprite.color = (damage < 0 ? healColor : damageColor);
        }
        initialPosition = initialPos;
    }
}
