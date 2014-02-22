using UnityEngine;
using System.Collections;

public enum VoxState
{
    Sun,
    Eclipse,
    Invert,
    Revert,
    SunSet,
}

public class VoxSystem : MonoBehaviour{
	readonly float InvertVP = 100;
	float deltaVP = 8;
    float currentVP = 0;

    public VoxState state { get; private set; }
    public bool WillEclipse { get { return currentVP >= InvertVP; } }

    //game objects
    public MidairPrimitive voxSun;
    public MidairPrimitive voxRing;
    public MidairPrimitive voxMoon;
    public TextMesh VPText;
    public GameObject[] sunLights;
    public GameObject mainLight;

    //animation preferences
    public Color BGColor;
    public Vector3 BGOffset;
    public Vector3 sunsetPosition;
    public float[] lightAngles;
    public float targetLightAngle;
    public float lightSpeedCoeff = 0.03f;
    public float lightMinSpeed = 0.05f;

    //initial parameters
    Color initialBGColor;
    Color initialMoonColor;
    Vector3 initialSunPosition;
    Vector3 initialMoonPosition;
    float initialMainLightScale;
    float[] initialLightScales;
    float[] initialLightAngles;

    //target parameters
    Color targetTextColor;
    Color targetMoonColor;
    Color targetBGColor;
    float targetMainLightScale;
    float[] targetLightAngles;
    float[] targetLightScales;
    Vector3 targetSunPosition;
    Vector3 targetSunScale;

    bool useTargetBGColor = true;
    bool useTargetMainLightScale = true;
    bool useTargetLightAngles = true;
    bool useTargetLightScales = true;

    //etc
    Camera mainCamera;
    Enemy currentTargetEnemy;
    float rotTime;

	// Use this for initialization
	void Start () {
        GameContext.VoxSystem = this;
        state = VoxState.SunSet;
		mainCamera = GameObject.Find( "Main Camera" ).camera;

        initialBGColor = mainCamera.backgroundColor;
        BGColor = initialBGColor;
        initialSunPosition = voxSun.transform.position;
        initialMoonPosition = voxMoon.transform.position;
        initialMoonColor = voxMoon.Color;
        initialMainLightScale = mainLight.transform.localScale.x;
        targetMainLightScale = initialMainLightScale;
        voxMoon.transform.localScale = Vector3.zero;

        lightAngles = new float[sunLights.Length];
        initialLightAngles = new float[sunLights.Length];
        initialLightScales = new float[sunLights.Length];
        targetLightAngles = new float[sunLights.Length];
        targetLightScales = new float[sunLights.Length];
        for( int i = 0; i < lightAngles.Length; i++ )
        {
            lightAngles[i] = Quaternion.Angle( Quaternion.identity, sunLights[i].transform.rotation );
            initialLightAngles[i] = lightAngles[i];
            initialLightScales[i] = sunLights[i].transform.localScale.x;
            targetLightAngles[i] = initialLightAngles[i];
            targetLightScales[i] = initialLightScales[i];
        }
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
            break;
        case VoxState.SunSet:
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
            voxMoon.transform.position = Vector3.Slerp( initialMoonPosition, voxSun.transform.position + Vector3.back * 0.2f, (-Mathf.Cos( t * Mathf.PI ) + 1) / 2 );
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
            voxMoon.transform.position = voxSun.transform.position + Vector3.back * 0.1f + Vector3.down * 0.1f;
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
            for( int i = 0; i < sunLights.Length; i++ )
            {
                sunLights[i].transform.localPosition = Vector3.right * (i < 2 ? i - 5 : i + 2) * 0.55f;
                sunLights[i].transform.localScale = new Vector3( 0.1f, sunLights[i].transform.localScale.y, sunLights[i].transform.localScale.z );
            }
        }
	}

	void UpdateAnimation()
    {
        if( useTargetBGColor ) BGColor = Color.Lerp( BGColor, targetBGColor, 0.1f );
        mainCamera.backgroundColor = BGColor;
        GameContext.BattleConductor.transform.position = BGOffset;

        if( useTargetMainLightScale )
        {
            mainLight.transform.localScale = new Vector3( Mathf.Lerp( mainLight.transform.localScale.x, targetMainLightScale, 0.1f ), mainLight.transform.localScale.y, mainLight.transform.localScale.z );
        }
        mainLight.transform.rotation = Quaternion.Lerp( mainLight.transform.rotation, Quaternion.AngleAxis( targetLightAngle, Vector3.forward ), 0.2f );
        for( int i = 0; i < sunLights.Length; i++ )
        {
            if( useTargetLightAngles )
            {
                lightAngles[i] = Mathf.Lerp( lightAngles[i], targetLightAngles[i], 0.1f );
            }
            sunLights[i].transform.rotation = Quaternion.Lerp( sunLights[i].transform.rotation, Quaternion.AngleAxis( lightAngles[i], Vector3.forward ), 0.2f );
            if( useTargetLightScales )
            {
                sunLights[i].transform.localScale = new Vector3( Mathf.Lerp( sunLights[i].transform.localScale.x, targetLightScales[i], 0.1f ), sunLights[i].transform.localScale.y, sunLights[i].transform.localScale.z );
            }
        }

        VPText.color = Color.Lerp( VPText.color, targetTextColor, 0.05f );
        voxMoon.SetColor( Color.Lerp( voxMoon.Color, targetMoonColor, 0.1f ) );
        voxMoon.transform.localScale = Vector3.Lerp( voxMoon.transform.localScale, Vector3.one * ((float)currentVP / InvertVP), 0.2f );

        transform.localPosition = Vector3.Lerp( transform.localPosition, targetSunPosition, 0.1f );
        voxSun.transform.localScale = Vector3.Lerp( voxSun.transform.localScale, targetSunScale, 0.05f );
        voxRing.transform.localScale = Vector3.Lerp( voxRing.transform.localScale, targetSunScale, 0.05f );
 	}
    

	public void SetState( VoxState newState )
	{
        if( state != newState )
        {
            state = newState;
            switch( state )
            {
            case VoxState.Sun:
            case VoxState.Revert:
                useTargetBGColor = true;
                useTargetLightAngles = true;
                useTargetLightScales = true;
                targetBGColor = initialBGColor;
                for( int i = 0; i < lightAngles.Length; i++ )
                {
                    targetLightAngles[i] = initialLightAngles[i];
                    targetLightScales[i] = initialLightScales[i];
                    sunLights[i].transform.localPosition = Vector3.zero;
                }

                targetTextColor = Color.black;
                targetMoonColor = initialMoonColor;
                AddVP( -(int)currentVP - 1 );
                GameContext.EnemyConductor.baseColor = Color.black;
                voxRing.SetTargetColor( Color.white );
                voxSun.transform.localScale = Vector3.one;
                voxMoon.transform.localScale = Vector3.zero;
                voxMoon.transform.position = initialMoonPosition;
                
                targetBGColor = initialBGColor;
                targetSunScale = Vector3.one;
                targetSunPosition = initialSunPosition;
                for( int i = 0; i < lightAngles.Length; i++ )
                {
                    targetLightAngles[i] = initialLightAngles[i];
                    targetLightScales[i] = initialLightScales[i];
                }
                targetMainLightScale = initialMainLightScale;
                break;
            case VoxState.Eclipse:
                useTargetBGColor = false;
                useTargetLightAngles = false;
                useTargetLightScales = false;
                targetTextColor = Color.clear;
                VPText.color = Color.clear;
                break;
            case VoxState.Invert:
                BGColor = Color.black;
                targetMoonColor = Color.black;
                break;
            case VoxState.SunSet:
                useTargetBGColor = true;
                useTargetLightAngles = true;
                useTargetLightScales = true;
                AddVP( -(int)currentVP - 1 );
                targetTextColor = Color.clear;
                targetBGColor = Color.black;
                targetSunScale = Vector3.zero;
                targetSunPosition = sunsetPosition;
                for( int i = 0; i < lightAngles.Length; i++ )
                {
                    targetLightAngles[i] = targetLightAngle;
                    targetLightScales[i] = 0;
                }
                targetMainLightScale = 0;
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
            Vector3 direction = targetEnemy.transform.position + Vector3.down * 1.5f - mainLight.transform.position;
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
