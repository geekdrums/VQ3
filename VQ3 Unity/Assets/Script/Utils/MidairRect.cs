using UnityEngine;
using System.Collections;

public class MidairRect : MonoBehaviour {

	public float targetRectSize = 3;
	public float targetWidth = 1;
	public Color targetColor = Color.white;

	float currentRectSize;
	float currentWidth;
	Color currentColor;

	LineRenderer[] lines;

	// Use this for initialization
	void Start () {
		currentRectSize = targetRectSize;
		currentWidth = targetWidth;
		lines = GetComponentsInChildren<LineRenderer>();
	}

	// Update is called once per frame
	void Update () {
		currentRectSize = Mathf.Lerp( currentRectSize, targetRectSize, 0.3f );
		currentWidth = Mathf.Lerp( currentWidth, targetWidth, 0.3f );
		currentColor = Color.Lerp( currentColor, targetColor, 0.3f );

		UpdateAnimation();
	}

	void UpdateAnimation()
	{
		if ( lines == null )
		{
			lines = GetComponentsInChildren<LineRenderer>();
		}
		for ( int i=0; i<lines.Length; ++i )
		{
			lines[i].SetWidth( currentWidth, currentWidth );
			lines[i].SetColors( currentColor, currentColor );
		}
		lines[0].SetPosition( 0, new Vector3( -currentRectSize/2, -currentRectSize/2+currentWidth/2 ) );
		lines[0].SetPosition( 1, new Vector3( +currentRectSize/2, -currentRectSize/2+currentWidth/2 ) );
		lines[1].SetPosition( 0, new Vector3( -currentRectSize/2+currentWidth, -currentRectSize/2+currentWidth/2 ) );
		lines[1].SetPosition( 1, new Vector3( +currentRectSize/2-currentWidth, -currentRectSize/2+currentWidth/2 ) );

		lines[2].SetPosition( 0, new Vector3( currentRectSize/2-currentWidth/2, -currentRectSize/2 ) );
		lines[2].SetPosition( 1, new Vector3( currentRectSize/2-currentWidth/2, +currentRectSize/2 ) );
		lines[3].SetPosition( 0, new Vector3( currentRectSize/2-currentWidth/2, -currentRectSize/2+currentWidth ) );
		lines[3].SetPosition( 1, new Vector3( currentRectSize/2-currentWidth/2, +currentRectSize/2-currentWidth ) );

		lines[4].SetPosition( 0, new Vector3( +currentRectSize/2, currentRectSize/2-currentWidth/2 ) );
		lines[4].SetPosition( 1, new Vector3( -currentRectSize/2, currentRectSize/2-currentWidth/2 ) );
		lines[5].SetPosition( 0, new Vector3( +currentRectSize/2-currentWidth, currentRectSize/2-currentWidth/2 ) );
		lines[5].SetPosition( 1, new Vector3( -currentRectSize/2+currentWidth, currentRectSize/2-currentWidth/2 ) );

		lines[6].SetPosition( 0, new Vector3( -currentRectSize/2+currentWidth/2, +currentRectSize/2 ) );
		lines[6].SetPosition( 1, new Vector3( -currentRectSize/2+currentWidth/2, -currentRectSize/2 ) );
		lines[7].SetPosition( 0, new Vector3( -currentRectSize/2+currentWidth/2, +currentRectSize/2-currentWidth ) );
		lines[7].SetPosition( 1, new Vector3( -currentRectSize/2+currentWidth/2, -currentRectSize/2+currentWidth ) );
	}

	public void SetTargetSize( float newTargetSize )
	{
		targetRectSize = newTargetSize;
	}
	public void SetTargetWidth( float newTargetWidth )
	{
		targetWidth = newTargetWidth;
	}
	public void SetTargetColor( Color newTargetColor )
	{
		targetColor = new Color( newTargetColor.r, newTargetColor.g, newTargetColor.b, newTargetColor.a * newTargetColor.a );
	}

	public void SetSize( float newSize )
	{
		SetTargetSize( newSize );
		currentRectSize = targetRectSize;
		UpdateAnimation();
	}
	public void SetWidth( float newWidth )
	{
		SetTargetWidth( newWidth );
		currentWidth = targetWidth;
		UpdateAnimation();
	}
	public void SetColor( Color newTargetColor )
	{
		SetTargetColor( newTargetColor );
		currentColor = targetColor;
		UpdateAnimation();
	}
}
