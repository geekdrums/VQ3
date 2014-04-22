using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum VoxState
{
    Sun,
    Eclipse,
    Invert,
    Revert,
    SunSet,
}

public class VoxSystem : MonoBehaviour{
    public static readonly int InvertVP = 100;
    public static readonly int MaxInvertTime = 5;
    public static readonly int MaxVT = 16*4*4;
    public static readonly int VTFireNum = 19;
    public static readonly int VTPerLine = MaxVT / VTFireNum;
    public static readonly float VTFireHeight = 23.0f;
	int currentVP = 0;
    int currentVT = 0;

    public VoxState state { get; private set; }
    public bool IsReadyEclipse { get { return currentVP >= InvertVP; } }
    public int InvertTime { get; private set; }

    #region animation property
    //game objects
    public MidairPrimitive voxSun;
    public MidairPrimitive voxRing;
    public MidairPrimitive voxMoon;
    public TextMesh VPText;
    public TextMesh VTText;
    public GameObject[] sunLights;
    public GameObject mainLight;
    public GameObject FireOrigin;
    public Color FireColor;
    public Color InvertFireColor;

    //animation preferences
    public Color BGColor;
    public Vector3 BGOffset;
    public Vector3 sunsetPosition;
    public float[] lightAngles;
    public float targetLightAngle;
    public float lightSpeedCoeff = 0.03f;
    public float lightMinSpeed = 0.05f;
    public float fireMinSpeed;
    public float fireMaxSpeed;
    public float fireMinHeight;
    public float fireMaxHeight;
    public float fireLinearFactor;

    //initial parameters
    Color initialBGColor;
    Color initialMoonColor;
    Color initialNextLightColor;
    Color initialFireColor;
    Vector3 initialSunPosition;
    Vector3 initialMoonPosition;
    float initialMainLightScale;
    float[] initialLightScales;
    float[] initialLightAngles;
    float initialRingWidth;
    float initialRingRadius;

    //target parameters
    //Color targetTextColor;
    //Color targetMoonColor;
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
    GameObject[] VTFires;
    Queue<float> FireDeltaQueue = new Queue<float>();
    #endregion

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
        //voxMoon.transform.localScale = Vector3.zero;

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

        initialRingWidth = voxRing.Width;
        initialRingRadius = voxRing.Radius;

        //initial blacck
        mainCamera.backgroundColor = Color.black;
        BGColor = Color.black;
        transform.position = sunsetPosition;
        voxSun.transform.localScale = Vector3.zero;
        voxRing.transform.localScale = Vector3.zero;
        for( int i = 0; i < sunLights.Length; i++ )
        {
            sunLights[i].transform.rotation = Quaternion.identity;
            sunLights[i].transform.localScale = new Vector3( 0, sunLights[i].transform.localScale.y, sunLights[i].transform.localScale.z );
        }
        mainLight.transform.localScale = new Vector3( 0, mainLight.transform.localScale.y, mainLight.transform.localScale.z );

        //initialFireColor = FireOrigin.GetComponent<SpriteRenderer>().color;
        VTFires = new GameObject[VTFireNum];
        for( int i = 0; i < VTFireNum; i++ )
        {
            VTFires[i] = (Instantiate( FireOrigin ) as GameObject);
            VTFires[i].transform.parent = FireOrigin.transform.parent;
            VTFires[i].transform.localPosition = new Vector3( i, FireOrigin.transform.localPosition.y, FireOrigin.transform.localPosition.z );
            VTFires[i].transform.localScale = new Vector3( 1, 0, 1 );
            FireDeltaQueue.Enqueue( 0 );
        }
        FireOrigin.transform.localScale = new Vector3( 1, 0, 1 );
    }

    // Update is called once per frame
    void Update()
	{
        if( GameContext.CurrentState == GameState.Field ) return;

        if( GameContext.PlayerConductor.CanUseInvert )
        {
            UpdateVT();
        }
		switch ( state )
		{
        case VoxState.Sun:
            UpdateLightAngles();
            break;
        case VoxState.Eclipse:
            EclipseUpdate();
            break;
        case VoxState.Invert:
            if( Music.IsNowChangedAt( 0 ) )
            {
                --InvertTime;
            }
            if( Music.IsJustChangedAt( 3, 2 ) && InvertTime == 1 )
            {
                SetState( VoxState.Revert );
            }
            break;
        case VoxState.Revert:
            break;
        case VoxState.SunSet:
            break;
        }

		UpdateAnimation();
	}

    void UpdateVT()
    {
        if( Music.isJustChanged && currentVT > 0 )
        {
            if( state == VoxState.Sun ||
              ( state == VoxState.Eclipse && ( Music.Just.bar < 2 || !IsReadyEclipse ) ) )
            {
                --currentVT;
                VTText.text = "VT:" + currentVT.ToString();
                if( currentVT <= 0 )
                {
                    currentVP = 0;
                    Music.SetAisac( "TrackVolumeEnergy", 0 );
                    VPText.text = "VP:" + currentVP.ToString() + "%";
                    foreach( GameObject fireline in VTFires )
                    {
                        fireline.GetComponent<LineRenderer>().SetColors( Color.white, FireColor );
                    }
                }
            }
            else if( state == VoxState.Invert )
            {
                VTText.text = "VT: " + InvertTime + "TURN";
            }
        }

        float targetFireScale = ((float)currentVT / MaxVT);
        if( state == VoxState.Invert || state == VoxState.Revert )
        {
            FireOrigin.transform.localScale = new Vector3( 1, Mathf.Lerp( FireOrigin.transform.localScale.y, (float)InvertTime / MaxInvertTime, 0.1f ), 1 );
            for( int i = 0; i < VTFireNum; i++ )
            {
                VTFires[i].transform.localScale = new Vector3( 1, Mathf.Lerp( VTFires[i].transform.localScale.y, (float)InvertTime / MaxInvertTime, 0.1f ), 1 );
            }
        }
        else if( IsReadyEclipse )
        {
            float delta = FireDeltaQueue.Peek();
            FireOrigin.transform.localScale = new Vector3( 1, Mathf.Lerp( FireOrigin.transform.localScale.y, Mathf.Min( 1.0f, targetFireScale + delta ), 0.1f ), 1 );
            for( int i = 0; i < VTFireNum; i++ )
            {
                VTFires[i].transform.localScale = new Vector3( 1, Mathf.Lerp( VTFires[i].transform.localScale.y, Mathf.Min( 1.0f, targetFireScale + delta ), 0.1f ), 1 );
            }
        }
        else
        {
            FireOrigin.transform.localScale = new Vector3( 1, Mathf.Lerp( FireOrigin.transform.localScale.y, targetFireScale, 0.2f ), 1 );
            int fIndex = 0;
            foreach( float delta in FireDeltaQueue )
            {
                if( fIndex < ((float)currentVP / InvertVP) * VTFireNum )
                {
                    VTFires[fIndex].transform.localScale = new Vector3( 1, Mathf.Lerp( VTFires[fIndex].transform.localScale.y, Mathf.Min( 1.0f, targetFireScale + delta ), 0.1f ), 1 );
                    targetFireScale *= fireLinearFactor;
                }
                else if( fIndex - 1 < ((float)currentVP / InvertVP) * VTFireNum )
                {
                    targetFireScale *= Mathf.Max( 0.0f, ((float)currentVP / InvertVP) * VTFireNum - (float)(fIndex - 1) );
                    VTFires[fIndex].transform.localScale = new Vector3( 1, Mathf.Lerp( VTFires[fIndex].transform.localScale.y, targetFireScale + delta, 0.1f ), 1 );
                }
                else
                {
                    VTFires[fIndex].transform.localScale = new Vector3( 1, Mathf.Lerp( VTFires[fIndex].transform.localScale.y, 0, 0.1f ), 1 );
                }
                fIndex++;
            }
        }
        if( Music.isJustChanged )
        {
            FireDeltaQueue.Dequeue();
            FireDeltaQueue.Enqueue( Mathf.Sin( Mathf.Lerp( fireMinSpeed, fireMaxSpeed, ((float)currentVT / MaxVT) ) * (float)Music.MusicalTime )
                * Mathf.Lerp( fireMinHeight, fireMaxHeight, ((float)currentVT / MaxVT) ) + fireMinHeight );
        }
    }

    void UpdateLightAngles()
    {
        rotTime += Time.deltaTime / (float)Music.mtUnit;
        for( int i = 0; i < lightAngles.Length; i++ )
        {
            float diffToMainLight = sunLights[i].transform.eulerAngles.z - mainLight.transform.eulerAngles.z;
            if( diffToMainLight > 180 ) diffToMainLight -= 360.0f;
            float leaveAwayFactor = 1.0f / Mathf.Max( 0.2f, (Mathf.Abs( diffToMainLight ) % 180.0f) / 90.0f );
            float d = 0;
            if( leaveAwayFactor > 1.0f )
            {
                d = ( Mathf.Abs( diffToMainLight ) > 0.1f ? diffToMainLight/Mathf.Abs(diffToMainLight) : 1.0f ) * lightSpeedCoeff * leaveAwayFactor;
            }
            else
            {
                d = (i % 2 == 0 ? 1 : -1) * Mathf.Sin( (float)(rotTime / 512) * Mathf.PI * 2 * (3 - i*0.5f) ) * lightSpeedCoeff;
            }
            lightAngles[i] += (Mathf.Abs( d ) < lightMinSpeed ? 0 : d);
        }
    }
    void EclipseUpdate()
    {
        if( Music.Just.bar < 3 || !IsReadyEclipse )
        {
            float t = (float)Music.MusicalTime / (Music.mtBar * 3);
            if( !IsReadyEclipse && Music.Just.bar >= 2 )
            {
                t = (2.0f / 3.0f) * ( Mathf.Max( 0, 1.0f - (float)(Music.MusicalTime - Music.mtBar * 2) / Music.mtBar ) );
            }
            voxMoon.transform.position = Vector3.Lerp( initialMoonPosition, voxSun.transform.position + Vector3.back * 0.2f, (-Mathf.Cos( t * Mathf.PI ) + 1) / 2 );
            BGColor = Color.Lerp( BGColor, Color.Lerp( initialBGColor, Color.black, 1.0f / (1.0f + (voxMoon.transform.position - voxSun.transform.position).magnitude) ), 0.3f );
            BGOffset = Vector3.Lerp( Vector3.zero, Vector3.forward * 10, t * t );
            for( int i = 0; i < lightAngles.Length; i++ )
            {
                lightAngles[i] += (targetLightAngle - lightAngles[i] > 0 ? -1 : 1) * 0.1f * (i+2);
            }
            voxRing.SetWidth( initialRingWidth * (1.1f - t) );
            voxRing.SetSize( initialRingRadius + t * 2.0f );
            voxSun.GrowSize = t * 0.4f;
            voxSun.SetTargetColor( Color.white * ( 1.0f - t*0.2f ) );
        }

        if( Music.IsNowChangedAt( 2 ) )
        {
            //CriAtomExAcb ACBData;
            //CriAtomEx.CueInfo CueInfo;
            //ACBData = CriAtom.GetAcb( Music.CurrentMusicName );
            //ACBData.GetCueInfo( "Invert", out CueInfo );
            //print( CueInfo.gameVariableInfo.name );
            //CueInfo.gameVariableInfo.gameValue = IsReadyEclipse ? 1 : 0;
            Music.SetAisac( "TrackVolumeTransition", IsReadyEclipse ? 1 : 0 );
            Music.SetAisac( "TrackVolumeLoop", IsReadyEclipse ? 0 : 1 );
            if( IsReadyEclipse )
            {
                TextWindow.ChangeMessage( "くろいつきが　せかいを　はんてんさせる" );
                InvertTime = Mathf.Clamp( (int)(MaxInvertTime * ((float)currentVT / MaxVT)), 1, MaxInvertTime - 1 ) + 1;
                GameContext.PlayerConductor.commandGraph.OnReactEvent( IconReactType.OnInvert );
            }
            else
            {
                TextWindow.ChangeMessage( "しかし　VPが　たりなかったようだ" );
            }
        }
        if( IsReadyEclipse )
        {
            if( animation.isPlaying )
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
                /*
                FireOrigin.transform.localPosition = Vector3.zero;
                FireOrigin.GetComponent<LineRenderer>().SetColors( Color.white, initialFireColor );
                for( int i = 0; i < VTFireNum; i++ )
                {
                    VTFires[i].transform.localPosition = new Vector3( VTFires[i].transform.localPosition.x, 0, 0 );
                    VTFires[i].GetComponent<LineRenderer>().SetColors( Color.white, initialFireColor );
                }
                */
            }
            else if( Music.IsJustChangedAt( 3, 2 ) )
            {
                animation.Stop();
                GameContext.EnemyConductor.baseColor = Color.white;
                BGOffset = Vector3.zero;
                voxRing.SetColor( Color.clear );
                voxRing.SetWidth( initialRingWidth );
                voxRing.SetSize( initialRingRadius );
                voxSun.transform.localScale = Vector3.zero;

                Enemy refleshTarget = currentTargetEnemy;
                currentTargetEnemy = null;
                SetTargetEnemy( refleshTarget );
                for( int i = 0; i < sunLights.Length; i++ )
                {
                    sunLights[i].transform.localPosition = Vector3.right * (i < 2 ? i - 5 : i + 2) * 0.55f;
                    sunLights[i].transform.localScale = new Vector3( 0.1f, sunLights[i].transform.localScale.y, sunLights[i].transform.localScale.z );
                    sunLights[i].transform.rotation = Quaternion.AngleAxis( lightAngles[i], Vector3.forward );
                }
                mainLight.transform.rotation = Quaternion.AngleAxis( targetLightAngle, Vector3.forward );
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

        //VPText.color = Color.Lerp( VPText.color, targetTextColor, 0.05f );
        //voxMoon.SetColor( Color.Lerp( voxMoon.Color, targetMoonColor, 0.1f ) );
        //voxMoon.transform.localScale = Vector3.Lerp( voxMoon.transform.localScale, Vector3.one * ((float)currentVP / InvertVP), 0.2f );

        transform.localPosition = Vector3.Lerp( transform.localPosition, targetSunPosition, 0.1f );
        voxSun.transform.localScale = Vector3.Lerp( voxSun.transform.localScale, targetSunScale, 0.05f );
        voxRing.transform.localScale = Vector3.Lerp( voxRing.transform.localScale, targetSunScale, 0.05f );
 	}
    

	public void SetState( VoxState newState )
	{
        if( state != newState )
        {
            VoxState oldState = state;
            state = newState;
            switch( state )
            {
            case VoxState.Sun:
            case VoxState.Revert:
                if( oldState != VoxState.Eclipse ) AddVPVT( -currentVP, -currentVT );

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
                GameContext.EnemyConductor.baseColor = Color.black;
                voxRing.SetTargetColor( Color.white );
                voxSun.transform.localScale = Vector3.one;
                voxSun.SetTargetColor( Color.white );
                //voxMoon.transform.localScale = Vector3.zero;
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

                voxRing.SetTargetWidth( initialRingWidth );
                voxRing.SetTargetSize( initialRingRadius );
                break;
            case VoxState.Eclipse:
                useTargetBGColor = false;
                useTargetLightAngles = false;
                useTargetLightScales = false;
                //targetTextColor = Color.clear;
                //VPText.color = Color.clear;
                break;
            case VoxState.Invert:
                BGColor = Color.black;
                //targetMoonColor = Color.black;
                break;
            case VoxState.SunSet:
                useTargetBGColor = true;
                useTargetLightAngles = true;
                useTargetLightScales = true;
                AddVPVT( -currentVP, -currentVT );
                //targetTextColor = Color.clear;
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

	public void AddVPVT( int VP, int VT )
    {
        if( GameContext.PlayerConductor.CanUseInvert )
        {
            currentVP = Mathf.Clamp( currentVP + (int)(VP * (100.0f - GameContext.EnemyConductor.VPtolerance) / 100.0f), 0, InvertVP );
            currentVT = Mathf.Clamp( currentVT + VT, 0, MaxVT );
            if( currentVT <= 0 )
            {
                currentVP = 0;
            }
            Music.SetAisac( "TrackVolumeEnergy", Mathf.Sqrt( (float)currentVP / InvertVP ) );
            VPText.text = "VP:" + currentVP.ToString() + "%";
            VTText.text = "VT:" + currentVT.ToString();

            if( IsReadyEclipse )
            {
                foreach( GameObject fireline in VTFires )
                {
                    fireline.GetComponent<LineRenderer>().SetColors( Color.white, InvertFireColor );
                }
            }
            else
            {
                foreach( GameObject fireline in VTFires )
                {
                    fireline.GetComponent<LineRenderer>().SetColors( Color.white, FireColor );
                }
            }
        }
	}

    public void SetTargetEnemy( Enemy targetEnemy )
    {
        if( currentTargetEnemy != targetEnemy )
        {
            currentTargetEnemy = targetEnemy;
            Vector3 direction = targetEnemy.transform.position + Vector3.down * 1.5f - initialSunPosition;
            targetLightAngle = Quaternion.LookRotation( Vector3.forward, -direction ).eulerAngles.z;
            if( state == VoxState.Invert || state == VoxState.Eclipse )
            {
                for( int i = 0; i < lightAngles.Length; i++ )
                {
                    lightAngles[i] = targetLightAngle;
                }
            }
        }
    }

    public void SetBlinkMoonColor( Color color )
    {
        //targetMoonColor = color;
        //targetTextColor = Color.black;
        //VPText.text = "VP:0/" + InvertVP;
    }
}
