using UnityEngine;
using System.Collections;

public class VoxonSystem : BGEffect{

	public enum VoxonState
	{
		Hide,
		Show,
		ShowBreak,
		Break,
		HideBreak,
	}
	public VoxonState state { get; private set; }
	readonly int BREAK_VOXON = 6;
	int deltaVoxon = 1;
	int currentVoxon = 0;


	public float MAX_SCALE;
	public float LINEAR_FACTOR;
	public GameObject linePrefab;
	public GameObject voxonCircle;

	public Color initialBGColor;
	public Color maxBGColor;
	public Color breakBGColor;

	public Vector3 hidePosition;
	public Vector3 showPosition;

	LineRenderer threasholdLine;
	Camera mainCamera;

	readonly float lineScale = 1.2f;
	Vector3 targetCircleScale = Vector3.zero;
	Vector3 targetLineScale = Vector3.zero;
	Color targetCircleColor = Color.white;
	Color targetLineColor = Color.white;
	Vector3 targetPosition;
	
	// Use this for initialization
	void Start () {
		GameContext.VoxonSystem = this;

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

		targetPosition = showPosition;
		transform.position = showPosition;
		state = VoxonState.Show;
	}
	
	// Update is called once per frame
	void Update()
	{
		if ( GameContext.CurrentState == GameContext.GameState.Field ) return;
		if ( GameContext.CurrentState == GameContext.GameState.Intro ) return;

		switch ( state )
		{
		case VoxonState.Hide:
		case VoxonState.Show:
			if ( Music.Just.bar < 3 )
			{
				float mt3 = (float)Music.MusicalTime/( Music.mtBar*3 );
				targetLineScale = Vector3.one * ( 1.0f + ( lineScale - 1.0f )*( 1.0f - mt3 ) );
				targetCircleScale = new Vector3( 1, 0, 1 ) * ( (float)Mathf.Max( 0, currentVoxon - deltaVoxon*mt3 )/BREAK_VOXON ) * MAX_SCALE;
			}
			else
			{
				if ( Music.IsJustChangedAt( 3 ) )
				{
					AddVoxon( -deltaVoxon );
					targetLineScale = Vector3.one * lineScale;
					threasholdLine.renderer.material.color = breakBGColor;
				}
				targetCircleScale = new Vector3( 1, 0, 1 ) * ( (float)Mathf.Max( 0, currentVoxon )/BREAK_VOXON ) * MAX_SCALE;
			}
			break;
		case VoxonState.ShowBreak:
			ShowBreakUpdate();
			break;
		case VoxonState.Break:
			break;
		case VoxonState.HideBreak:
			break;
		}
		UpdateAnimation();
	}

	void ShowBreakUpdate()
	{
		if ( Music.Just.bar < 3 && targetCircleColor != breakBGColor )
		{
			targetLineScale = Vector3.one * ( 1.0f + ( lineScale - 1.0f )*( 1.0f - (float)Music.MusicalTime/( Music.mtBar*3 ) ) );
			targetCircleScale = new Vector3( 1, 0, 1 ) * ( (float)currentVoxon/BREAK_VOXON ) * MAX_SCALE;
		}
		else if ( Music.IsJustChangedAt( 3 ) )
		{
			targetCircleScale = new Vector3( 1, 0, 1 ) * MAX_SCALE * lineScale;
			targetCircleColor = breakBGColor;

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
			threasholdLine.transform.localPosition += Vector3.back * 2;
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
		threasholdLine.renderer.material.SetColor( "_TintColor", Color.Lerp( threasholdLine.renderer.material.GetColor("_TintColor"), targetLineColor, LINEAR_FACTOR ) );
		transform.position = Vector3.Lerp( transform.position, targetPosition, LINEAR_FACTOR );
		
		float f = (MAX_SCALE - voxonCircle.transform.localScale.x) / MAX_SCALE;
		mainCamera.backgroundColor = Color.Lerp( maxBGColor, initialBGColor, f );
 	}
    

	public void SetState( VoxonState newState )
	{
		state = newState;
		switch ( state )
		{
		case VoxonState.HideBreak:
			targetCircleScale = Vector3.zero;
			targetLineScale = Vector3.zero;
			targetCircleColor = Color.white;
			targetLineColor = Color.white;
			threasholdLine.transform.localPosition = Vector3.zero;
			threasholdLine.SetWidth( 0.1f, 0.1f );
			AddVoxon( -currentVoxon );
			GameContext.EnemyConductor.baseColor = Color.black;
			break;
		case VoxonState.Hide:
			targetPosition = hidePosition;
			break;
        case VoxonState.Show:
        case VoxonState.ShowBreak:
			targetPosition = showPosition;
            Music.SetAisac( "TrackVolumeEnergy", Mathf.Sqrt( (float)currentVoxon / BREAK_VOXON ) );
			break;
		}
	}

	public bool DetermineWillShowBreak( int willGainVoxon )
	{
        if( currentVoxon >= BREAK_VOXON )
        {
            SetState( VoxonState.Break );
        }
		else if ( currentVoxon + willGainVoxon >= BREAK_VOXON )
		{
			SetState( VoxonState.ShowBreak );
		}
		return state == VoxonState.ShowBreak;
	}

	public void AddVoxon( int value )
	{
		currentVoxon = Mathf.Clamp( currentVoxon + value, 0, BREAK_VOXON );
        Music.SetAisac( "TrackVolumeEnergy", Mathf.Sqrt( (float)currentVoxon / BREAK_VOXON ) );
	}
}
