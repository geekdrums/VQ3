using UnityEngine;
using System.Collections;

public class VoxonSystem : MonoBehaviour {

	public float MAX_SCALE;
	public float LINEAR_FACTOR;
	public GameObject linePrefab;
	public GameObject voxonCircle;

	public Color initialBGColor;
	public Color maxBGColor;
	public Color breakBGColor;

	LineRenderer threasholdLine;
	Camera mainCamera;

	readonly float lineScale = 1.2f;
	Vector3 targetCircleScale = Vector3.zero;
	Vector3 targetLineScale = Vector3.zero;
	Color targetCircleColor = Color.white;
	Color targetLineColor = Color.white;
	
	// Use this for initialization
	void Start () {
		//make circle
		int linePositions = 65;
		threasholdLine = ( (GameObject)Instantiate( linePrefab, transform.position, linePrefab.transform.rotation ) ).GetComponent<LineRenderer>();
		threasholdLine.SetVertexCount( linePositions+1 );
		for ( int i=0; i<=linePositions; ++i )
		{
			threasholdLine.SetPosition( i, new Vector3(
				Mathf.Cos( Mathf.PI * 2 * i/64.0f ) * MAX_SCALE/2,
				Mathf.Sin( Mathf.PI * 2 * i/64.0f ) * MAX_SCALE/2, 0 ) );
		}
		threasholdLine.transform.parent = transform;
		
		//init scale
		threasholdLine.transform.localScale = targetLineScale;
		voxonCircle.transform.localScale = targetCircleScale;

		mainCamera = GameObject.Find( "Main Camera" ).camera;
		mainCamera.backgroundColor = initialBGColor;
	}
	
	// Update is called once per frame
	void Update()
	{
		if ( GameContext.CurrentState == GameContext.GameState.Field ) return;
		if ( GameContext.CurrentState == GameContext.GameState.Intro ) return;

		//if ( Music.IsJustChangedBar() ) Debug.Log( GameContext.BattleConductor.state );

		switch ( GameContext.BattleConductor.state )
		{
		case BattleConductor.VoxonState.Hide:
			break;
		case BattleConductor.VoxonState.Show:
			ShowUpdate();
			break;
		case BattleConductor.VoxonState.ShowBreak:
			ShowBreakUpdate();
			break;
		case BattleConductor.VoxonState.Break:
			break;
		case BattleConductor.VoxonState.HideBreak:
			break;
		}
		UpdateAnimation();
	}

	void ShowUpdate()
	{
		if ( Music.Just.bar < 3 )
		{
			targetLineScale = Vector3.one * ( 1.0f + ( lineScale - 1.0f )*( 1.0f - (float)Music.MusicalTime/( Music.mtBar*3 ) ) );
		}
		else if ( Music.IsJustChangedAt( 3 ) )
		{
			targetLineScale = Vector3.one * lineScale;
			threasholdLine.renderer.material.color = breakBGColor;//Color.black;
		}
	}

	void ShowBreakUpdate()
	{
		if ( Music.Just.bar < 3 && targetCircleColor != breakBGColor )//Color.black )
		{
			targetLineScale = Vector3.one * ( 1.0f + ( lineScale - 1.0f )*( 1.0f - (float)Music.MusicalTime/( Music.mtBar*3 ) ) );
		}
		else if ( Music.IsJustChangedAt( 3 ) )
		{
			targetCircleScale = new Vector3( 1, 0, 1 ) * MAX_SCALE * lineScale;
			targetCircleColor = breakBGColor;// Color.black;

			targetLineScale = Vector3.zero;
			threasholdLine.transform.localScale = Vector3.zero;
		}
		else if ( Music.IsJustChangedAt( 3, 0, 2 ) )
		{
			targetCircleScale = Vector3.zero;
		}
		else if ( Music.IsJustChangedAt( 3, 1 ) )
		{
			targetCircleScale = new Vector3( 1, 0, 1 ) * MAX_SCALE * 8;
		}
		else if ( Music.IsJustChangedAt( 3, 1, 1 ) )
		{
			targetLineScale = Vector3.one * 3.5f;
			threasholdLine.transform.position += Vector3.back * 2;
			threasholdLine.SetWidth( 30, 30 );
			GameContext.EnemyConductor.baseColor = Color.white;
		}
	}

	//linear fade
	void UpdateAnimation()
	{
		voxonCircle.transform.localScale = Vector3.Lerp( voxonCircle.transform.localScale, targetCircleScale, LINEAR_FACTOR );
		voxonCircle.renderer.material.color = Color.Lerp( voxonCircle.renderer.material.color, targetCircleColor, LINEAR_FACTOR );
		threasholdLine.transform.localScale = Vector3.Lerp( threasholdLine.transform.localScale, targetLineScale, LINEAR_FACTOR );
		threasholdLine.renderer.material.color = Color.Lerp( threasholdLine.renderer.material.color, targetLineColor, LINEAR_FACTOR );

		float f = (MAX_SCALE - voxonCircle.transform.localScale.x) / MAX_SCALE;
		mainCamera.backgroundColor = Color.Lerp( maxBGColor, initialBGColor, f );
 	}

	public void SetCurrentVoxon( float value )
	{
		targetCircleScale = new Vector3( 1, 0, 1 ) * value * MAX_SCALE;
	}

	public void HideBreak()
	{
		targetCircleScale = Vector3.zero;
		targetLineScale = Vector3.zero;
		targetCircleColor = Color.white;
		targetLineColor = Color.white;
		threasholdLine.transform.position -= Vector3.back * 2;
		threasholdLine.SetWidth( 0.1f, 0.1f );
	}

	public void Hide()
	{
		targetLineScale = Vector3.zero;
		targetCircleScale = Vector3.zero;
	}
}
