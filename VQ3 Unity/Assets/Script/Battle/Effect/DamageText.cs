using UnityEngine;
using System.Collections;

public class DamageText : MonoBehaviour {

    public float AnimY;
    Vector3 initialPosition;
    float time = 0;
	// Use this for initialization
	void Start () {
        //animation.Play();
	}
	
	// Update is called once per frame
	void Update () {
        time += Time.deltaTime;
        float theta = time * (Mathf.PI * 2) * 8.0f;
        if( theta <= Mathf.PI * 2 )
        {
            AnimY = Mathf.Sin( theta ) * 0.4f;
            transform.position = initialPosition + Vector3.up * AnimY;
        }
        if( time >= 2.0f )
        {
            Destroy( gameObject );
        }
	}


    public void Initialize( int damage, Vector3 initialPos )
    {
        foreach( TextMesh textMesh in GetComponentsInChildren<TextMesh>() ) textMesh.text = Mathf.Abs( damage ).ToString();
        if( damage < 0 ) GetComponentsInChildren<TextMesh>()[1].color = Color.green;
        initialPosition = initialPos;
        transform.position = initialPosition;
    }
}
