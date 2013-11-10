using UnityEngine;
using System.Collections;

public class DefendEffect : CommandEffect{

	public float centerSize;
	public float secondSize;
	public float secondWidth;
	public float thirdSize;
	public float thirdWidth;
	public Color initialColor;
	public MidairRect[] rects;

	public Vector3 targetPosition;
	public Vector3 initialPosition;
	Timing startTiming;

	// Use this for initialization
	void Start()
	{
		Initialize();
	}

	void Initialize()
	{
		transform.position = initialPosition;
		rects[0].SetSize( centerSize );
		rects[0].SetWidth( centerSize/2 );
		rects[1].SetSize( secondSize );
		rects[1].SetWidth( secondWidth );
		rects[2].SetSize( thirdSize );
		rects[2].SetWidth( thirdWidth );
		rects[0].SetColor( initialColor );
		rects[1].SetColor( initialColor );
		rects[2].SetColor( initialColor );
	}
	
	// Update is called once per frame
	void Update () {
		if ( startTiming != null )
		{
			float mt = (float)( Music.MusicalTime - startTiming.totalUnit );
			if ( mt < 2 )
			{
				transform.position = Vector3.Lerp( transform.position, targetPosition+Vector3.up, 0.2f );
			}
			else if ( mt < 3 )
			{
				transform.position = Vector3.Lerp( transform.position, targetPosition+Vector3.down*2, 0.5f );
			}
			else if ( mt < 4 )
			{
				transform.position = Vector3.Lerp( transform.position, targetPosition, 0.5f );
			}
			else
			{
				rects[1].transform.rotation = Quaternion.AngleAxis( 180 * ( mt - 4 )/4, Vector3.forward );
				rects[2].transform.rotation = Quaternion.AngleAxis( -180 * ( mt - 4 )/4, Vector3.forward );
				Color currentColor = ( ( mt - 4 ) < 2 ? Color.Lerp( initialColor, Color.white, ( mt-4 )/2 ) : Color.Lerp( Color.white, Color.clear, ( mt-6 )/2 ) );
				rects[0].SetTargetColor( currentColor );
				rects[1].SetTargetColor( currentColor );
				rects[2].SetTargetColor( currentColor );
			}
		}
	}

	public override void Play()
	{
		startTiming = new Timing( Music.Just );
		Debug.Log( startTiming.ToString() );
		Initialize();
	}
}
