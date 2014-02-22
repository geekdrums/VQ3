using UnityEngine;
using System.Collections;

public enum VoxState
{
    Sun,
    Eclipse,
    Invert,
    Revert,
    Dark,
}

public class VoxSystem : MonoBehaviour{
	readonly float InvertVP = 100;
	float deltaVP = 8;
	float currentVP = 0;

    public MidairPrimitive voxSun;
    public MidairPrimitive voxRing;
    public MidairPrimitive voxMoon;
    public TextMesh VPText;
    public GameObject[] sunLights;
    public GameObject targetLight;

    public Color BGColor;
    public Vector3 BGOffset;
    public float[] lightAngles;
    public float targetLightAngle;
    public float lightSpeedCoeff = 0.03f;
    public float lightMinSpeed = 0.05f;

    public VoxState state { get; private set; }
    public bool WillEclipse { get { return currentVP >= InvertVP; } }

    Camera mainCamera;

    Color initialBGColor;
    Color initialMoonColor;
    Vector3 initialMoonPosition;
    float initialTargetLightScale;
    float[] initialLightScales;
    float[] initialLightAngles;

    Color targetTextColor;
    Color targetMoonColor;
    Enemy currentTargetEnemy;

    float rotTime;

	// Use this for initialization
	void Start () {
        GameContext.VoxSystem = this;
        state = VoxState.Dark;
		mainCamera = GameObject.Find( "Main Camera" ).camera;

        initialBGColor = mainCamera.backgroundColor;
        BGColor = initialBGColor;
        initialMoonPosition = voxMoon.transform.position;
        initialMoonColor = voxMoon.Color;
        voxMoon.transform.localScale = Vector3.zero;

        lightAngles = new float[sunLights.Length];
        initialLightScales = new float[sunLights.Length];
        initialLightAngles = new float[sunLights.Length];
        for( int i = 0; i < lightAngles.Length; i++ )
        {
            lightAngles[i] = Quaternion.Angle( Quaternion.identity, sunLights[i].transform.rotation );
            initialLightAngles[i] = lightAngles[i];
            initialLightScales[i] = sunLights[i].transform.localScale.x;
        }
        initialTargetLightScale = targetLight.transform.localScale.x;
	}

    // Update is called once per frame
    void Update()
	{
        if( GameContext.CurrentState == GameContext.GameState.Field ) return;

		switch ( state )
		{
        case VoxState.Sun:
            if( WillEclipse )
            {
                if( Music.IsJustChangedAt( 0 ) )
                {
                    //if don't use eclipse, reset.
                    DecreaseDeltaVP();
                    targetTextColor = Color.black;
                    targetMoonColor = initialMoonColor;
                }
                else if( Music.isJustChanged )
                {
                    targetTextColor = (Music.Just.unit == 2 ? Color.clear : Color.white);
                    VPText.text = "VP:" + ((int)InvertVP).ToString() + "/" + ((int)InvertVP).ToString();
                }
            }
            else
            {
                if( Music.isJustChanged )
                {
                    DecreaseDeltaVP();
                    VPText.text = "VP:" + ((int)currentVP).ToString() + "/" + ((int)InvertVP).ToString();
                }
            }
            UpdateLightAngles();
            break;
        case VoxState.Eclipse:
            EclipseUpdate();
            break;
        case VoxState.Invert:
            break;
        case VoxState.Revert:
            for( int i = 0; i < lightAngles.Length; i++ )
            {
                lightAngles[i] = Mathf.Lerp( lightAngles[i], initialLightAngles[i], 0.1f );
                sunLights[i].transform.localScale = new Vector3( Mathf.Lerp( sunLights[i].transform.localScale.x, initialLightScales[i], 0.1f ), sunLights[i].transform.localScale.y, sunLights[i].transform.localScale.z );
                sunLights[i].transform.localPosition = Vector3.Lerp( sunLights[i].transform.localPosition, Vector3.zero, 0.1f );
            }
            BGColor = Color.Lerp( BGColor, initialBGColor, 0.1f );
            break;
        case VoxState.Dark:
            break;
        }

		UpdateAnimation();
	}

    void DecreaseDeltaVP()
    {
        currentVP -= deltaVP / ( Music.mtBar * 4 );
        currentVP = Mathf.Clamp( currentVP, 0, InvertVP );
        Music.SetAisac( "TrackVolumeEnergy", Mathf.Sqrt( (float)currentVP / InvertVP ) );
    }

    void UpdateLightAngles()
    {
        rotTime += Time.deltaTime / (float)Music.mtUnit;
        for( int i = 0; i < lightAngles.Length; i++ )
        {
            float leaveAwayFactor = ( currentTargetEnemy != null ? 0.5f / Mathf.Max( 0.1f, Mathf.Abs( (targetLightAngle - lightAngles[i]) % 180.0f ) / 90.0f ) : 0.0f );
            float d = (i % 2 == 0 ? 1 : -1) * Mathf.Sin( (float)(rotTime / 1024) * Mathf.PI * 2 * (5 - i) ) * lightSpeedCoeff * Mathf.Max( 1.0f, leaveAwayFactor );
            lightAngles[i] += (Mathf.Abs( d ) < lightMinSpeed ? 0 : d);
        }
    }
    void EclipseUpdate()
    {
        if( Music.Just.bar < 3 )
        {
            float t = (float)Music.MusicalTime / (Music.mtBar * 3);
            voxMoon.transform.position = Vector3.Slerp( initialMoonPosition, voxSun.transform.position, (-Mathf.Cos( t*Mathf.PI ) + 1)/2 );
            BGColor = Color.Lerp( BGColor, Color.Lerp( initialBGColor, Color.black, 1.0f / (1.0f + (voxMoon.transform.position - voxSun.transform.position).magnitude) ), 0.3f );
            BGOffset = Vector3.Lerp( Vector3.zero, Vector3.forward * 10, t * t );
            for( int i = 0; i < lightAngles.Length; i++ )
            {
                lightAngles[i] += (targetLightAngle - lightAngles[i] > 0 ? -1 : 1) * 0.1f * (i+2);
            }
        }
        else if( animation.isPlaying )
        {
            for( int i = 0; i < lightAngles.Length; i++ )
            {
                lightAngles[i] = (targetLightAngle + 90) * (1.0f + i * 0.3f) - 90;
            }
        }

        if( Music.IsJustChangedAt( 3 ) )
        {
            voxMoon.transform.position = voxSun.transform.position + Vector3.back * 0.1f;
            BGColor = Color.black;
            animation["EclipseAnim"].speed = 1 / (float)(Music.mtBeat * Music.mtUnit);
            animation.Play();
            GameContext.EnemyConductor.OnInvert();
        }
        else if ( Music.IsJustChangedAt( 3, 2 ) )
        {
            animation.Stop();
            GameContext.EnemyConductor.baseColor = Color.white;
            BGOffset = Vector3.zero;
            voxRing.SetColor( Color.clear );
            voxSun.transform.localScale = Vector3.zero;

            Enemy refleshTarget = currentTargetEnemy;
            currentTargetEnemy = null;
            SetTargetEnemy( refleshTarget );
            targetLight.transform.localScale = new Vector3( initialTargetLightScale, targetLight.transform.localScale.y, targetLight.transform.localScale.z );
            for( int i = 0; i < sunLights.Length; i++ )
            {
                sunLights[i].transform.localPosition = Vector3.right * (i < 2 ? i - 5 : i + 2) * 0.55f;
                sunLights[i].transform.localScale = new Vector3( 0.1f, sunLights[i].transform.localScale.y, sunLights[i].transform.localScale.z );
            }
        }
	}

	void UpdateAnimation()
    {
        mainCamera.backgroundColor = BGColor;
        GameContext.BattleConductor.transform.position = BGOffset;
        VPText.color = Color.Lerp( VPText.color, targetTextColor, 0.05f );
        voxMoon.SetColor( Color.Lerp( voxMoon.Color, targetMoonColor, 0.1f ) );
        voxMoon.transform.localScale = Vector3.Lerp( voxMoon.transform.localScale, Vector3.one * ((float)currentVP / InvertVP), 0.2f );
        for( int i = 0; i < sunLights.Length; i++ )
        {
            sunLights[i].transform.rotation = Quaternion.Lerp( sunLights[i].transform.rotation, Quaternion.AngleAxis( lightAngles[i], Vector3.forward ), 0.2f );
        }
        targetLight.transform.rotation = Quaternion.Lerp( targetLight.transform.rotation, Quaternion.AngleAxis( targetLightAngle, Vector3.forward ), 0.2f );
 	}
    

	public void SetState( VoxState newState )
	{
        if( state != newState )
        {
            state = newState;
            switch( state )
            {
            case VoxState.Sun:
                targetTextColor = Color.black;
                targetMoonColor = initialMoonColor;
                voxMoon.transform.position = initialMoonPosition;
                voxMoon.transform.localScale = Vector3.zero;
                break;
            case VoxState.Eclipse:
                targetTextColor = Color.clear;
                VPText.color = Color.clear;
                break;
            case VoxState.Invert:
                BGColor = Color.black;
                targetMoonColor = Color.black;
                break;
            case VoxState.Revert:
                targetTextColor = Color.black;
                targetMoonColor = initialMoonColor;
                AddVP( -(int)currentVP - 1 );
                GameContext.EnemyConductor.baseColor = Color.black;
                voxRing.SetTargetColor( Color.white );
                voxSun.transform.localScale = Vector3.one;
                break;
            case VoxState.Dark:
                break;
            }
        }
	}

	public void AddVP( int value )
	{
        currentVP = Mathf.Clamp( currentVP + value, 0, InvertVP );
        Music.SetAisac( "TrackVolumeEnergy", Mathf.Sqrt( (float)currentVP / InvertVP ) );

        if( state == VoxState.Sun && WillEclipse )
        {
            targetTextColor = Color.white;
            targetMoonColor = Color.black;
        }
	}

    public void SetTargetEnemy( Enemy targetEnemy )
    {
        if( currentTargetEnemy != targetEnemy )
        {
            currentTargetEnemy = targetEnemy;
            Vector3 direction = targetEnemy.transform.position + Vector3.down * 1.5f - targetLight.transform.position;
            targetLightAngle = Quaternion.LookRotation( Vector3.forward, -direction ).eulerAngles.z;
            if( state == VoxState.Sun )
            {
                for( int i = 0; i < lightAngles.Length; i++ )
                {
                    lightAngles[i] += Random.Range( -10.0f, 10.0f );
                }
            }
            else if( state == VoxState.Invert || state == VoxState.Eclipse )
            {
                for( int i = 0; i < lightAngles.Length; i++ )
                {
                    lightAngles[i] = targetLightAngle;
                }
            }
        }
    }
}
