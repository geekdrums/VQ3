using UnityEngine;
using System.Collections;

public class EnemyCommandIcon : MonoBehaviour {

    static readonly float RotInterval = 30.0f;

    SpriteRenderer sprite;
    Quaternion targetRotation;
    Color targeColor;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        transform.rotation = Quaternion.Lerp( transform.rotation, targetRotation, 0.2f );
        sprite.transform.localRotation = Quaternion.Inverse( transform.rotation );
        sprite.color = Color.Lerp( sprite.color, targeColor, 0.2f );
	}

    public void SetIndex( int index, int length )
    {
        if( index < length )
        {
            targetRotation = Quaternion.AngleAxis( RotInterval * (-(length - 1) / 2.0f + index), Vector3.forward );
            targeColor = Color.black;
        }
        else if( index == length )
        {
            targetRotation = Quaternion.AngleAxis( 179.9f, Vector3.forward );
        }
        else
        {
            targeColor = Color.clear;
        }
    }
    public void SetIcon( Sprite icon )
    {
        if( icon != null )
        {
            sprite.sprite = icon;
        }
        if( sprite == null )
        {
            sprite = GetComponentInChildren<SpriteRenderer>();
        }
        sprite.color = targeColor = Color.clear;
        transform.rotation = targetRotation = Quaternion.AngleAxis( -90, Vector3.forward );
    }
}
