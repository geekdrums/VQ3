using UnityEngine;
using System.Collections;

public class VoxParticleSystem : MonoBehaviour {

	public GameObject cubePrefab;

	public float BG_DEPTH = 24.0f;
	public float ANGLE_INTERVAL = 15.0f;
	public float WIDTH_INTERVAL = 2.5f;
	public float HEIGHT_INTERVAL = 2.5f;
	Vector3 DEFAULT_CENTER;
	Vector3 MAGIC_CENTER;

	readonly static int WIDTH =33;
	readonly static int CENTER_WIDTH = WIDTH/2;
	readonly static int HEIGHT = 17;
	readonly static int CENTER_HEIGHT = 8;
	readonly static int DEPTH = 8;
	readonly static int ANGLE = 9;
	readonly static int CENTER_ANGLE = ANGLE/2;


	GameObject[][] bgMap;
	GameObject[][] floorMap;

	Vector3[][] bgTargetPositions;

	// Use this for initialization
	void Start () {
		DEFAULT_CENTER = new Vector3( 0, CENTER_HEIGHT*HEIGHT_INTERVAL, BG_DEPTH );
		MAGIC_CENTER = new Vector3( 0, 3*HEIGHT_INTERVAL, BG_DEPTH );

		bgMap = new GameObject[WIDTH][];
		bgTargetPositions = new Vector3[WIDTH][];
		for ( int x=0; x<WIDTH; x++ )
		{
			bgMap[x] = new GameObject[HEIGHT];
			bgTargetPositions[x] = new Vector3[HEIGHT];
			for ( int y=0; y<HEIGHT; y++ )
			{
				bgTargetPositions[x][y] = GetDefaultBGPosition( x, y );
				bgMap[x][y] = (GameObject)Instantiate( cubePrefab, bgTargetPositions[x][y], cubePrefab.transform.rotation );
				bgMap[x][y].transform.parent = this.transform;
			}
		}
		floorMap = new GameObject[ANGLE][];
		for ( int a=0; a<ANGLE; a++ )
		{
			floorMap[a] = new GameObject[DEPTH];
			for ( int d=0; d<DEPTH; d++ )
			{
				floorMap[a][d] = (GameObject)Instantiate( cubePrefab, GetFloorPosition( d, a ), cubePrefab.transform.rotation );
				floorMap[a][d].transform.parent = this.transform;
				floorMap[a][d].transform.localScale = new Vector3();
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		if ( Music.IsNowChangedBar() )
		{
			for ( int x=0; x<WIDTH; x++ )
			{
				for ( int y=0; y<HEIGHT; y++ )
				{
					bgTargetPositions[x][y] = Music.Now.bar % 2 == 0 ? GetMagicBGPosition( x, y ) : GetDefaultBGPosition( x, y );
				}
			}
		}

		for ( int x=0; x<WIDTH; x++ )
		{
			for ( int y=0; y<HEIGHT; y++ )
			{
				Vector3 d = bgTargetPositions[x][y] - bgMap[x][y].transform.position;
				if ( d.sqrMagnitude > 0.5f )
				{
					bgMap[x][y].transform.position += d * 0.15f;
				}
				else
				{
					bgMap[x][y].transform.position = bgTargetPositions[x][y];
				}
			}
		}
	}

	Vector3 GetFloorPosition( int d, int a )
	{
		float distance = (d+1) * ( BG_DEPTH/(float)DEPTH );
		float theta = ( ( a - CENTER_ANGLE ) * ANGLE_INTERVAL ) * Mathf.PI / 180;
		return new Vector3( Mathf.Sin( theta )*distance, 0, Mathf.Cos( theta )*distance );
	}

	Vector3 GetDefaultBGPosition( int x, int y )
	{
		return new Vector3( ( x - CENTER_WIDTH )*WIDTH_INTERVAL, y*HEIGHT_INTERVAL, BG_DEPTH );
	}

	Vector3 GetMagicBGPosition( int x, int y )
	{
		//float theta = Mathf.PI * 2 * (float)y/(float)HEIGHT - Mathf.PI;
		//return new Vector3( Mathf.Cos( theta )*x, Mathf.Sin( theta )*x + 5, BG_DEPTH );
		Vector3 v = GetDefaultBGPosition( x, y );
		return ( v - DEFAULT_CENTER ).normalized *
			Mathf.Max( Mathf.Abs( v.x - DEFAULT_CENTER.x ), Mathf.Abs( v.y - DEFAULT_CENTER.y ) ) + MAGIC_CENTER;
	}
}
