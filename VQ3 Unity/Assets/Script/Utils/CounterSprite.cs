using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CounterAlign
{
    Right,
    Center,
    Left
}
public class CounterSprite : MonoBehaviour {

    public Sprite[] NumberSprites;
    public GameObject RendererPrefab;
    public float Interval;
    public CounterAlign align = CounterAlign.Center;
    public float counterScale = 1.0f;

    GameObject NumberParent;
    int count_;
    public int count
    {
        get { return count_; }
        set
        {
            if( NumberParent == null )
            {
                NumberParent = new GameObject();
                NumberParent.transform.parent = transform;
                NumberParent.transform.localPosition = Vector3.zero;
            }

            count_ = value;
            int numDigits = (int)Mathf.Log10( (float)count ) + 1;
            for( int i = digits.Count; i < numDigits; i++ )
            {
                SpriteRenderer sprite = (Instantiate( RendererPrefab ) as GameObject).GetComponent<SpriteRenderer>();
                sprite.transform.parent = NumberParent.transform;
                sprite.transform.localScale *= counterScale;
                sprite.transform.localPosition = new Vector3( -Interval * counterScale * i, 0, 0 );
                digits.Add( sprite );
            }
            int restCount = count_;
            for( int i = 0; i < digits.Count; i++ )
            {
                if( i < numDigits )
                {
                    digits[i].enabled = true;
                    digits[i].sprite = NumberSprites[restCount % 10];
                    restCount -= (restCount % 10);
                    restCount /= 10;
                }
                else
                {
                    digits[i].enabled = false;
                }
            }

            switch( align )
            {
            case CounterAlign.Right:
                NumberParent.transform.localPosition = Vector3.zero;
                break;
            case CounterAlign.Center:
                NumberParent.transform.localPosition = Vector3.right * Interval * counterScale * numDigits * 0.5f;
                break;
            case CounterAlign.Left:
                NumberParent.transform.localPosition = Vector3.right * Interval * counterScale * numDigits;
                break;
            }
        }
    }

    List<SpriteRenderer> digits = new List<SpriteRenderer>();

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
