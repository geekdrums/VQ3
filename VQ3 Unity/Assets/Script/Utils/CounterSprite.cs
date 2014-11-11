using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public enum CounterAlign
{
    Right,
    Center,
    Left
}

public enum CounterVerticalAlign
{
    Top,
    Center,
    Bottom
}


[ExecuteInEditMode]
public class CounterSprite : MonoBehaviour {

    enum Mark
    {
        Dot,
        Percent,
        Slash,
        Plus,
        Minus,
        Count
    }
    static float[] MarksIntervals = new float[(int)Mark.Count]
    {
        0.35f,//Dot,
        1.2f,//Percent,
        0.5f,//Slash,
        0.9f,//Plus,
        0.9f //Minus,
    };
    static float[] CounterIntervals = new float[10]
    {
        1.0f,//0
        0.6f,//1
        1.0f,//2
        1.0f,//3
        1.0f,//4
        1.0f,//5
        1.0f,//6
        1.0f,//7
        1.0f,//8
        1.0f,//9
    };
    [System.Flags]
    public enum Options
    {
        None = 0,
        Percent = 1,
        Sign = 2,
        //Numerator
        //Denominator
    }

    public Sprite[] NumberSprites;
    public Sprite[] MarkSprites;
    public GameObject RendererPrefab;
    public float Interval;
    public CounterAlign align = CounterAlign.Center;
    public CounterVerticalAlign vAlign = CounterVerticalAlign.Center;
    public float CounterScale = 1.0f;
    public int SignificantDigits = 0;

    public float count_;
    public float Count
    {
        get { return count_; }
        set
        {
            count_ = value;
            UpdateSprite();
        }
    }
    public Color CounterColor
    {
        get { return counterColor_; }
        set
        {
            counterColor_ = value;
            foreach( SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>() )
            {
                sprite.color = CounterColor;
            }
        }
    }
    public Color counterColor_;

    public Options optionFlags_ = Options.None;
    bool HasFlag( Options opt ) { return (optionFlags_ & opt) == opt; }

    public GameObject NumberParent;
    public List<SpriteRenderer> digits_ = new List<SpriteRenderer>();
    public SpriteRenderer dotSprite_ = null;
    public SpriteRenderer prefixSprit_ = null;
    public SpriteRenderer suffixSprite_ = null;

	// Use this for initialization
    void Start()
    {
        //if( NumberParent != null )
        //{
        //    int index = 0;
        //    for( int i = 0; i < NumberParent.transform.childCount; ++i )
        //    {
        //        SpriteRenderer child = NumberParent.transform.GetChild( index ).GetComponent<SpriteRenderer>();
        //        if( digits_.Contains( child ) == false && child != dotSprite_ && child != prefixSprit_ && child != suffixSprite_ )
        //        {
        //            DestroyImmediate( NumberParent.transform.GetChild( index ).gameObject );
        //        }
        //        else
        //        {
        //            ++index;
        //        }
        //    }
        //}
        //else
        //{
        //    CreateNumberParent();
        //}
        UpdateSprite();
	}
	
	// Update is called once per frame
	void Update ()
    {
#if UNITY_EDITOR
        //if( !UnityEditor.EditorApplication.isPlaying )
        //{
        //    if( NumberParent == null )
        //    {
        //        for( int i = 0; i < transform.childCount; ++i )
        //        {
        //            DestroyImmediate( transform.GetChild( 0 ).gameObject );
        //        }
        //        CreateNumberParent();
        //        UpdateSprite();
        //    }
        //    return;
        //}
#endif
    }

    void CreateNumberParent()
    {
        NumberParent = new GameObject();
        NumberParent.transform.parent = transform;
        NumberParent.transform.localPosition = Vector3.zero;
        NumberParent.name = "NumberHead";
        digits_.Clear();
        dotSprite_ = null;
        suffixSprite_ = null;
        prefixSprit_ = null;
    }

    void OnValidate()
    {
        UpdateSprite();
    }

    public void UpdateSprite()
    {
        if( PrefabUtility.GetPrefabType( this ) == PrefabType.Prefab || NumberParent == null ) return;

        //digits
        int numDigits = Mathf.Max( 1, Mathf.CeilToInt( Mathf.Log10( (int)Count + 0.5f ) ) ) + SignificantDigits;
        for( int i = 0; i < numDigits; i++ )
        {
            if( i >= digits_.Count )
            {
                SpriteRenderer sprite = (Instantiate( RendererPrefab ) as GameObject).GetComponent<SpriteRenderer>();
                sprite.transform.parent = NumberParent.transform;
                digits_.Add( sprite );
            }
            if( digits_[i] == null ) return;//???
            digits_[i].transform.localScale = Vector3.one * CounterScale;
        }

        //dots
        if( SignificantDigits > 0 )
        {
            if( dotSprite_ == null )
            {
                dotSprite_ = (Instantiate( RendererPrefab ) as GameObject).GetComponent<SpriteRenderer>();
                dotSprite_.transform.parent = NumberParent.transform;
                dotSprite_.name = "dot";
            }
            dotSprite_.transform.localScale = Vector3.one * CounterScale;
            dotSprite_.sprite = MarkSprites[(int)Mark.Dot];
            dotSprite_.enabled = true;
        }
        else
        {
            if( dotSprite_ != null )
            {
                dotSprite_.enabled = false;
            }
        }

        //options
        if( HasFlag( Options.Sign ) )
        {
            if( prefixSprit_ == null )
            {
                prefixSprit_ = (Instantiate( RendererPrefab ) as GameObject).GetComponent<SpriteRenderer>();
                prefixSprit_.transform.parent = NumberParent.transform;
                prefixSprit_.name = "prefix";
            }
            prefixSprit_.transform.localScale = Vector3.one * CounterScale;
            if( count_ >= 0 )
            {
                prefixSprit_.sprite = MarkSprites[(int)Mark.Plus];
            }
            else
            {
                prefixSprit_.sprite = MarkSprites[(int)Mark.Minus];
            }
            prefixSprit_.enabled = true;
        }
        else
        {
            if( prefixSprit_ != null )
            {
                prefixSprit_.enabled = false;
            }
        }

        if( HasFlag( Options.Percent ) )
        {
            if( suffixSprite_ == null )
            {
                suffixSprite_ = (Instantiate( RendererPrefab ) as GameObject).GetComponent<SpriteRenderer>();
                suffixSprite_.transform.parent = NumberParent.transform;
                suffixSprite_.name = "suffix";
            }
            suffixSprite_.transform.localScale = Vector3.one * CounterScale;
            suffixSprite_.sprite = MarkSprites[(int)Mark.Percent];
            suffixSprite_.enabled = true;
        }
        else
        {
            if( suffixSprite_ != null )
            {
                suffixSprite_.enabled = false;
            }
        }

        //update numbers
        int restCount = (int)(count_ * Mathf.Pow(10,SignificantDigits));
        int[] numbers = new int[numDigits];
        for( int i = 0; i < digits_.Count; i++ )
        {
            if( i < numDigits )
            {
                digits_[i].enabled = true;
                numbers[i] = restCount % 10;
                digits_[i].sprite = NumberSprites[numbers[i]];
                restCount -= (restCount % 10);
                restCount /= 10;
            }
            else
            {
                digits_[i].enabled = false;
            }
        }

        //positions
        Vector3 vInterval = Vector3.left * Interval * CounterScale;
        Vector3 currentPos = Vector3.zero;
        if( HasFlag( Options.Percent ) )
        {
            currentPos += vInterval * MarksIntervals[(int)Mark.Percent] / 2;
            suffixSprite_.transform.localPosition = currentPos;
            currentPos += vInterval * MarksIntervals[(int)Mark.Percent] / 2;
        }
        for( int i = 0; i < numDigits; i++ )
        {
            currentPos += vInterval * CounterIntervals[numbers[i]] / 2;
            digits_[i].transform.localPosition = currentPos;
            currentPos += vInterval * CounterIntervals[numbers[i]] / 2;
            if( i == SignificantDigits - 1 )
            {
                currentPos += vInterval * MarksIntervals[(int)Mark.Dot] / 2;
                dotSprite_.transform.localPosition = currentPos;
                currentPos += vInterval * MarksIntervals[(int)Mark.Dot] / 2;
            }
        }
        //currentPos -= vInterval/2;
        if( HasFlag( Options.Sign ) )
        {
            currentPos += vInterval * MarksIntervals[(int)Mark.Plus] / 2;
            prefixSprit_.transform.localPosition = currentPos;
            currentPos += vInterval * MarksIntervals[(int)Mark.Plus] / 2;
        }

        //align
        float alignX = 0;
        switch( align )
        {
        case CounterAlign.Right:
            alignX = 0;
            break;
        case CounterAlign.Center:
            alignX = -currentPos.x * 0.5f;
            break;
        case CounterAlign.Left:
            alignX = -currentPos.x;
            break;
        }
        float alignY = 0;
        switch( vAlign )
        {
        case CounterVerticalAlign.Top:
            alignY = -Interval * CounterScale * 1.2f / 2;
            break;
        case CounterVerticalAlign.Center:
            alignY = 0;
            break;
        case CounterVerticalAlign.Bottom:
            alignY = Interval * CounterScale * 1.2f / 2;
            break;
        }
        NumberParent.transform.localPosition = new Vector3( alignX, alignY, 0 );

        //color
        foreach( SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>() )
        {
            sprite.color = CounterColor;
        }
    }
}
