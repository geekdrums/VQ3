using UnityEngine;
//using UnityEditor;
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

public class CounterSprite : MonoBehaviour, IColoredObject {

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
        0.9f,//Minus,
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
		PercentAndSign = Percent | Sign,
        Numerator = 4,
        Denominator = 8,
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
			if( counterColor_ != value )
			{
				counterColor_ = value;
				foreach( SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>() )
				{
					sprite.color = CounterColor;
				}
			}
        }
    }
    public Color counterColor_;

	public void SetColor(Color color) { CounterColor = color; }
	public Color GetColor() { return CounterColor; }

	public Options optionFlags_ = Options.None;
    bool HasFlag( Options opt ) { return (optionFlags_ & opt) == opt; }

    public GameObject NumberParent;
    public List<SpriteRenderer> digits_ = new List<SpriteRenderer>();
    public SpriteRenderer dotSprite_ = null;
    public SpriteRenderer prefixSprit_ = null;
    public SpriteRenderer suffixSprite_ = null;
	private Mark prefixMark_ = Mark.Count;
	private Mark suffixMark_ = Mark.Count;
	public float Width { get; private set; }
	public float Height { get; private set; }

	float maxShakeAnimTime_;
	float shakeAnimTime_;
	float shakeAnimMagnitude_;

	// Use this for initialization
    void Start()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {
		if( shakeAnimTime_ > 0 )
		{
			shakeAnimTime_ -= Time.deltaTime;
			if( shakeAnimTime_ <= 0 )
			{
				shakeAnimTime_ = 0;
				shakeAnimMagnitude_ = 0;
				maxShakeAnimTime_ = 0;
			}
			UpdateSprite();
		}
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
        //digits
        int numDigits = Mathf.Max( 1, Mathf.CeilToInt( Mathf.Log10( (int)Mathf.Abs( Count ) + 0.5f ) ) ) + SignificantDigits;
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
            if( count_ >= 0 )
            {
				prefixMark_ = Mark.Plus;
            }
            else
            {
				prefixMark_ = Mark.Minus;
			}
        }
		else if( HasFlag(Options.Denominator) )
		{
			prefixMark_ = Mark.Slash;
		}
		else
		{
			prefixMark_ = Mark.Count;
			if( prefixSprit_ != null )
			{
				prefixSprit_.enabled = false;
			}
		}
		if( prefixMark_ != Mark.Count )
		{
			if( prefixSprit_ == null )
			{
				prefixSprit_ = (Instantiate(RendererPrefab) as GameObject).GetComponent<SpriteRenderer>();
				prefixSprit_.transform.parent = NumberParent.transform;
				prefixSprit_.name = "prefix";
			}
			prefixSprit_.transform.localScale = Vector3.one * CounterScale;
			prefixSprit_.sprite = MarkSprites[(int)prefixMark_];
			prefixSprit_.enabled = true;
		}

        if( HasFlag( Options.Percent ) )
        {
			suffixMark_ = Mark.Percent;
		}
		else if( HasFlag(Options.Numerator) )
		{
			suffixMark_ = Mark.Slash;
		}
        else
		{
			suffixMark_ = Mark.Count;
            if( suffixSprite_ != null )
            {
                suffixSprite_.enabled = false;
            }
		}
		if( suffixMark_ != Mark.Count )
		{
			if( suffixSprite_ == null )
			{
				suffixSprite_ = (Instantiate(RendererPrefab) as GameObject).GetComponent<SpriteRenderer>();
				suffixSprite_.transform.parent = NumberParent.transform;
				suffixSprite_.name = "suffix";
			}
			suffixSprite_.transform.localScale = Vector3.one * CounterScale;
			suffixSprite_.sprite = MarkSprites[(int)suffixMark_];
			suffixSprite_.enabled = true;
		}

        //update numbers
		int restCount = (int)Mathf.Abs((count_ * Mathf.Pow(10, SignificantDigits)));
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
        if( suffixMark_ != Mark.Count )
        {
			currentPos += vInterval * MarksIntervals[(int)suffixMark_] / 2;
            suffixSprite_.transform.localPosition = currentPos;
			currentPos += vInterval * MarksIntervals[(int)suffixMark_] / 2;
        }
        for( int i = 0; i < numDigits; i++ )
        {
            currentPos += vInterval * CounterIntervals[numbers[i]] / 2;
            digits_[i].transform.localPosition = currentPos + Vector3.up * GetShakeOffset(i);
            currentPos += vInterval * CounterIntervals[numbers[i]] / 2;
            if( i == SignificantDigits - 1 )
            {
                currentPos += vInterval * MarksIntervals[(int)Mark.Dot] / 2;
                dotSprite_.transform.localPosition = currentPos + Vector3.up * GetShakeOffset(i);
                currentPos += vInterval * MarksIntervals[(int)Mark.Dot] / 2;
            }
        }
        //currentPos -= vInterval/2;
        if( prefixMark_ != Mark.Count )
        {
			currentPos += vInterval * MarksIntervals[(int)prefixMark_] / 2;
            prefixSprit_.transform.localPosition = currentPos;
			currentPos += vInterval * MarksIntervals[(int)prefixMark_] / 2;
        }

        //align
		float alignX = 0;
		Width = Mathf.Abs( currentPos.x );
        switch( align )
        {
        case CounterAlign.Right:
            alignX = 0;
            break;
        case CounterAlign.Center:
			alignX = Width * 0.5f;
            break;
        case CounterAlign.Left:
			alignX = Width;
            break;
        }
        float alignY = 0;
		Height = Mathf.Abs( Interval * CounterScale * 1.2f );
        switch( vAlign )
        {
        case CounterVerticalAlign.Top:
			alignY = -Height / 2;
            break;
        case CounterVerticalAlign.Center:
            alignY = 0;
            break;
        case CounterVerticalAlign.Bottom:
			alignY = Height / 2;
            break;
        }
        NumberParent.transform.localPosition = new Vector3( alignX, alignY, 0 );

        //color
        foreach( SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>() )
        {
            sprite.color = CounterColor;
        }
    }

	public void Shake(float time, float magnitude)
	{
		maxShakeAnimTime_ = shakeAnimTime_ = time;
		shakeAnimMagnitude_ = magnitude;
	}

	const float waveSpeed = 0.3f;
	const float waveDecreaseRate = 0.2f;
	const float timeWaveRate = 30.0f;
	const float timeDecreaseRate = 1.0f;
	const float timeDecreaseMin = 0.0f;
	float GetShakeOffset(int index)
	{
		if( shakeAnimTime_ <= 0 ) return 0;
		else return shakeAnimMagnitude_ * -Mathf.Cos(timeWaveRate * shakeAnimTime_ + index * waveSpeed) * (1.0f - index * waveDecreaseRate) * Mathf.Min(1.0f, (timeDecreaseMin + (shakeAnimTime_ / maxShakeAnimTime_) * timeDecreaseRate));
	}
}
