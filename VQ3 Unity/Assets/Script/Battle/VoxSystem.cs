using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum VoxState
{
	None,
    Sun,
    Overflow,
    Overload,
    BackToSun,
    SunSet,
}

public class VoxSystem : MonoBehaviour
{
	public static readonly int MaxInvertTime = 4;
	public static readonly int MaxVT = 16 * 4 * 4;
	public static readonly int VPWaveNum = 33;
	public static readonly float VTFireHeight = 2.0f;
	public int currentVP { get; private set; }
	public int currentVT { get; private set; }

	public VoxState State { get; private set; }

	public int OverflowVP { get { return Mathf.Max(GameContext.EnemyConductor.InvertVP, 1); } }
	public bool IsOverFlow { get { return currentVP >= OverflowVP; } }
	public bool GetWillEclipse(int addVP) { return currentVP + addVP >= OverflowVP; }
	public bool IsOverloading { get { return GameContext.BattleState == BattleState.Eclipse && IsOverFlow && Music.Just.Bar >= 2; } }
	public int OverloadTime { get; private set; }

	#region animation property
	//game objects
	public MidairPrimitive voxSun;
	public MidairPrimitive voxRing;
	public MidairPrimitive voxMoon;
	public CounterSprite VTCount;
	public CounterSprite VPCount;
	public GameObject[] sunLights;
	public GameObject mainLight;
	public GameObject WaveOrigin;
	public Material WaveLineMaterial;
	public Material BGMaterial;

	//animation preferences
	public Color BGColor;
	public Vector3 BGOffset;
	public Vector3 sunsetPosition;
	public float[] lightAngles;
	public float targetLightAngle;
	public float lightSpeedCoeff = 0.03f;
	public float lightMinSpeed = 0.05f;
	public Vector3 waitBGOffset;
	public float BGScaleCoeff;
	public float RMSCoeff = 3.0f;
	public float SinCoeff = 0.5f;
	public float SinSpeed = 2.0f;
	public float WaveLinearFactor = 0.8f;

	//initial parameters
	Vector3 initialSunPosition;
	Vector3 initialMoonPosition;
	float initialMainLightScale;
	float[] initialLightScales;
	float[] initialLightAngles;
	float initialRingWidth;
	float initialRingRadius;

	//target parameters
	Color targetBGColor;
	float targetMainLightScale;
	float[] targetLightAngles;
	float[] targetLightScales;
	Vector3 targetSunPosition;
	Vector3 targetSunScale;

	bool useTargetMainLightScale = true;
	bool useTargetLightAngles = true;
	bool useTargetLightScales = true;

	//etc
	Enemy currentTargetEnemy_;
	float rotTime_;
	GameObject[] vtWaves_;
	List<float> waveDelta_ = new List<float>();
	List<DamageGauge> damageGauges_ = new List<DamageGauge>();
	float waveRemainCoeff_;
	CriAtomExPlayerOutputAnalyzer analyzer_ = new CriAtomExPlayerOutputAnalyzer(new CriAtomExPlayerOutputAnalyzer.Type[1] { CriAtomExPlayerOutputAnalyzer.Type.LevelMeter });
	#endregion

	void Awake()
	{
		GameContext.VoxSystem = this;
	}

	// Use this for initialization
	void Start()
	{
		State = VoxState.None;
		ColorManager.SetBaseColor(EBaseColor.Black);

		BGColor = ColorManager.Theme.Light;
		initialSunPosition = voxSun.transform.position;
		initialMoonPosition = voxMoon.transform.position;
		initialMainLightScale = mainLight.transform.localScale.x;
		targetMainLightScale = initialMainLightScale;

		lightAngles = new float[sunLights.Length];
		initialLightAngles = new float[sunLights.Length];
		initialLightScales = new float[sunLights.Length];
		targetLightAngles = new float[sunLights.Length];
		targetLightScales = new float[sunLights.Length];
		for( int i = 0; i < lightAngles.Length; i++ )
		{
			lightAngles[i] = Quaternion.Angle(Quaternion.identity, sunLights[i].transform.rotation);
			initialLightAngles[i] = lightAngles[i];
			initialLightScales[i] = sunLights[i].transform.localScale.x;
			targetLightAngles[i] = initialLightAngles[i];
			targetLightScales[i] = initialLightScales[i];
		}

		initialRingWidth = voxRing.Width;
		initialRingRadius = voxRing.Radius;

		transform.localPosition = sunsetPosition;
		voxSun.transform.localScale = Vector3.zero;
		voxRing.transform.localScale = Vector3.zero;
		for( int i = 0; i < sunLights.Length; i++ )
		{
			sunLights[i].transform.rotation = Quaternion.identity;
			sunLights[i].transform.localScale = new Vector3(0, sunLights[i].transform.localScale.y, sunLights[i].transform.localScale.z);
		}
		mainLight.transform.localScale = new Vector3(0, mainLight.transform.localScale.y, mainLight.transform.localScale.z);

		vtWaves_ = new GameObject[VPWaveNum];
		for( int i = 0; i < VPWaveNum; i++ )
		{
			vtWaves_[i] = (Instantiate(WaveOrigin) as GameObject);
			vtWaves_[i].transform.parent = WaveOrigin.transform.parent;
			vtWaves_[i].transform.localPosition = WaveOrigin.transform.localPosition + (i % 2 == 0 ? Vector3.right : Vector3.left) * (int)((i + 1)/2);
			vtWaves_[i].transform.localScale = new Vector3(1, 0, 1);
			waveDelta_.Add(0);
		}
		Destroy(WaveOrigin.gameObject);

		WaveLineMaterial.color = ColorManager.Base.Front;
	}

	// Update is called once per frame
	void Update()
	{
		if( GameContext.State != GameState.Battle ) return;

		UpdateVT();

		if( GameContext.BattleState == BattleState.Eclipse )
		{
			EclipseUpdate();
			useTargetLightAngles = false;
			useTargetLightScales = false;
			UpdateAnimation();
			return;
		}

		switch( State )
		{
		case VoxState.Sun:
		case VoxState.Overflow:
			UpdateLightAngles();
			UpdateBGOffset();
			break;
		case VoxState.Overload:
			if( Music.IsNearChangedAt(0) )
			{
				--OverloadTime;
			}
			if( Music.IsJustChangedAt(3, 2) && OverloadTime == 1 )
			{
				SetState(VoxState.BackToSun);
				GameContext.EnemyConductor.OnRevert();
				GameContext.PlayerConductor.OnRevert();
				ColorManager.SetBaseColor(EBaseColor.Black);
				WaveLineMaterial.color = ColorManager.Base.Front;
			}
			break;
		case VoxState.BackToSun:
			BGColor = Color.Lerp(BGColor, targetBGColor, 0.1f);
			BGMaterial.color = BGColor;
			break;
		case VoxState.SunSet:
			break;
		}

		if( Music.IsPlaying )
		{
			UpdateAnimation();
		}
	}

	void UpdateVT()
	{
		if( Music.IsJustChanged && currentVT > 0 )
		{
			if( (State == VoxState.Sun || State == VoxState.Overflow) && IsOverloading == false )
			{
				--currentVT;
				VTCount.Count = currentVT / 64.0f;
				if( currentVT <= 0 )
				{
					currentVP = 0;
					VPCount.Count = currentVP;
					Music.SetAisac("TrackVolumeEnergy", 0);
					WaveLineMaterial.color = ColorManager.Base.Front;
					State = VoxState.Sun;
				}
			}
			else if( State == VoxState.Overload )
			{
				VTCount.Count = (float)(OverloadTime - Music.MusicalTime/64.0f);
			}
		}

		float vtRate = (float)currentVT / MaxVT;
		float maxWaveScale = Mathf.Min(1.0f, (vtRate <= 0.5f ? 0.75f * (float)vtRate / 0.5f :  0.75f + 0.25f * (vtRate - 0.5f) / 0.5f) + waveRemainCoeff_ * 0.5f);
		float targetWaveScale = maxWaveScale;
		float linearFactor = Mathf.Lerp(WaveLinearFactor, 0.99f, vtRate);
		float targetRemainScale = waveRemainCoeff_;
		float remainLinearFactor = (0.9f - 0.9f * (float)currentVP/OverflowVP);
		waveRemainCoeff_ *= 0.97f;
		damageGauges_.RemoveAll((DamageGauge dg) => dg == null);
		for( int i = 0; i<VPWaveNum; ++i )
		{
			float waveScale = 0;
			if( i < ((float)currentVP / OverflowVP) * VPWaveNum )
			{
				waveScale = Mathf.Clamp(targetWaveScale + waveDelta_[VPWaveNum - 1 - i], 0.0f, 1.0f);
				targetWaveScale *= linearFactor;
			}
			else
			{
				waveScale = Mathf.Min(targetWaveScale * 2, targetRemainScale);
				targetRemainScale *= remainLinearFactor;
			}
			vtWaves_[i].transform.localScale = new Vector3(1, Mathf.Lerp(vtWaves_[i].transform.localScale.y, waveScale, 0.2f), 1);
			foreach( DamageGauge gauge in damageGauges_ )
			{
				gauge.SetVTWave(i, vtWaves_[i].transform.localScale.y * 0.7f + 0.15f);
			}
		}
		if( Music.IsJustChanged )
		{
			waveDelta_.RemoveAt(0);
			float sin = Mathf.Sin(SinSpeed * (float)Music.MusicalTime);
			float rms = analyzer_.GetRms(0) * RMSCoeff * vtRate + sin * SinCoeff;
			waveDelta_.Add(rms);
		}
	}

	void UpdateLightAngles()
	{
		rotTime_ += Time.deltaTime / (float)Music.MusicalTimeUnit;
		for( int i = 0; i < lightAngles.Length; i++ )
		{
			float diffToMainLight = sunLights[i].transform.eulerAngles.z - mainLight.transform.eulerAngles.z;
			if( diffToMainLight > 180 ) diffToMainLight -= 360.0f;
			float leaveAwayFactor = 1.0f / Mathf.Max(0.2f, (Mathf.Abs(diffToMainLight) % 180.0f) / 90.0f);
			float d = 0;
			if( leaveAwayFactor > 1.0f )
			{
				d = (Mathf.Abs(diffToMainLight) > 0.1f ? diffToMainLight/Mathf.Abs(diffToMainLight) : 1.0f) * lightSpeedCoeff * leaveAwayFactor;
			}
			else
			{
				d = (i % 2 == 0 ? 1 : -1) * Mathf.Sin((float)(rotTime_ / 512) * Mathf.PI * 2 * (3 - i*0.5f)) * lightSpeedCoeff;
			}
			lightAngles[i] += (Mathf.Abs(d) < lightMinSpeed ? 0 : d);
		}
	}

	void UpdateBGOffset()
	{
		string blockName = Music.CurrentBlockName;
		BGOffset = Vector3.Lerp(BGOffset, (blockName == "intro" || blockName == "wait") ? waitBGOffset : Vector3.zero, 0.1f);
	}

	void EclipseUpdate()
	{
		if( Music.Just.Bar < 3 || !IsOverFlow )
		{
			float t = (float)Music.MusicalTime / (Music.CurrentUnitPerBar * 3);
			if( !IsOverFlow && Music.Just.Bar >= 2 )
			{
				t = (2.0f / 3.0f) * (Mathf.Max(0, 1.0f - (float)(Music.MusicalTime - Music.CurrentUnitPerBar * 2) / Music.CurrentUnitPerBar));
			}
			voxMoon.transform.position = Vector3.Lerp(initialMoonPosition, voxSun.transform.position + Vector3.back * 0.2f, (-Mathf.Cos(t * Mathf.PI) + 1) / 2);
			BGColor = Color.Lerp(BGColor, Color.Lerp(ColorManager.Theme.Light, Color.black, 1.0f / (1.0f + (voxMoon.transform.position - voxSun.transform.position).magnitude)), 0.3f);
			BGOffset = Vector3.Lerp(Vector3.zero, Vector3.forward * 10, t * t);
			for( int i = 0; i < lightAngles.Length; i++ )
			{
				lightAngles[i] += (targetLightAngle - lightAngles[i] > 0 ? -1 : 1) * 0.1f * (i+2);
			}
			voxRing.SetWidth(initialRingWidth * (1.1f - t));
			voxRing.SetSize(initialRingRadius + t);
			voxSun.GrowSize = t;
			voxSun.SetTargetColor(Color.Lerp(Color.white, Color.black, t*0.2f));
		}

		if( Music.Near.Bar < 2 && GameContext.PlayerConductor.PlayerIsDanger )
		{
			Music.SetAisac(8, (float)Music.MusicalTime/32);
		}
		if( Music.IsNearChangedAt(2) )
		{
			//TODO:ゲーム変数で置き換え？
			Music.SetAisac("TrackVolumeTransition", IsOverFlow ? 1 : 0);
			Music.SetAisac("TrackVolumeLoop", IsOverFlow ? 0 : 1);
			if( IsOverFlow )
			{
				TextWindow.SetMessage(MessageCategory.Invert, "くろいつきが　せかいを　はんてんさせる");
				OverloadTime = Mathf.Clamp((int)(currentVT / 64.0f), 2, MaxInvertTime);
			}
			else
			{
				TextWindow.SetMessage(MessageCategory.Invert, "しかし　VPが　たりなかったようだ");
			}
			Music.SetAisac(8, GameContext.PlayerConductor.PlayerIsDanger ? 0 : 1);
		}
		if( IsOverFlow )
		{
			if( GetComponent<Animation>().isPlaying )
			{
				for( int i = 0; i < lightAngles.Length; i++ )
				{
					lightAngles[i] = (targetLightAngle + 90) * (1.0f + i * 0.3f) - 90;
				}
			}
			if( Music.IsJustChangedAt(3) )
			{
				voxMoon.transform.position = voxSun.transform.position + Vector3.back * 0.1f + Vector3.down * 0.1f;
				BGColor = Color.black;
				GetComponent<Animation>()["EclipseAnim"].speed = 1 / (float)(Music.CurrentUnitPerBeat * Music.MusicalTimeUnit);
				GetComponent<Animation>().Play();
				GameContext.EnemyConductor.OnInvert();
				GameContext.PlayerConductor.CommandGraph.SelectInitialInvertCommand();
			}
			else if( Music.IsJustChangedAt(3, 2) )
			{
				GetComponent<Animation>().Stop();
				GameContext.EnemyConductor.baseColor = Color.white;
				ColorManager.SetBaseColor(EBaseColor.White);
				targetBGColor = ColorManager.Base.Front;
				BGOffset = Vector3.zero;
				voxRing.SetColor(Color.clear);
				voxRing.SetWidth(initialRingWidth);
				voxRing.SetSize(initialRingRadius);
				voxSun.transform.localScale = Vector3.zero;

				Enemy refleshTarget = currentTargetEnemy_;
				currentTargetEnemy_ = null;
				SetTargetEnemy(refleshTarget);
				for( int i = 0; i < sunLights.Length; i++ )
				{
					sunLights[i].transform.localPosition = Vector3.right * (i < 2 ? i - 5 : i + 2) * 0.55f;
					sunLights[i].transform.localScale = new Vector3(0.1f, sunLights[i].transform.localScale.y, sunLights[i].transform.localScale.z);
					sunLights[i].transform.rotation = Quaternion.AngleAxis(lightAngles[i], Vector3.forward);
				}
				mainLight.transform.rotation = Quaternion.AngleAxis(targetLightAngle, Vector3.forward);
			}
			else if( Music.IsNearChangedAt(3, 3) )
			{
				if( GameContext.PlayerConductor.CommandGraph.NextCommand == null )
				{
					Music.Pause();
				}
			}
		}

		BGColor = Color.Lerp(BGColor, targetBGColor, 0.1f);
		BGMaterial.color = BGColor;
	}

	public void UpdateAnimation()
	{
		GameContext.BattleConductor.transform.position = BGOffset;
		GameContext.BattleConductor.transform.localScale = Vector3.one * BGScaleCoeff / (BGScaleCoeff + BGOffset.magnitude);

		if( useTargetMainLightScale )
		{
			if( (mainLight.transform.localScale.x - targetMainLightScale) < 0.1f )
			{
				mainLight.transform.localScale = new Vector3(targetMainLightScale, mainLight.transform.localScale.y, mainLight.transform.localScale.z);
			}
			else
			{
				mainLight.transform.localScale = new Vector3(Mathf.Lerp(mainLight.transform.localScale.x, targetMainLightScale, 0.1f), mainLight.transform.localScale.y, mainLight.transform.localScale.z);
			}
		}
		mainLight.transform.rotation = Quaternion.Lerp(mainLight.transform.rotation, Quaternion.AngleAxis(targetLightAngle, Vector3.forward), 0.2f);
		for( int i = 0; i < sunLights.Length; i++ )
		{
			if( useTargetLightAngles )
			{
				lightAngles[i] = Mathf.Lerp(lightAngles[i], targetLightAngles[i], 0.1f);
			}
			sunLights[i].transform.rotation = Quaternion.Lerp(sunLights[i].transform.rotation, Quaternion.AngleAxis(lightAngles[i], Vector3.forward), 0.2f);
			if( useTargetLightScales )
			{
				if( (sunLights[i].transform.localScale.x - targetLightScales[i]) < 0.1f )
				{
					sunLights[i].transform.localScale = new Vector3(targetLightScales[i], sunLights[i].transform.localScale.y, sunLights[i].transform.localScale.z);
				}
				else
				{
					sunLights[i].transform.localScale = new Vector3(Mathf.Lerp(sunLights[i].transform.localScale.x, targetLightScales[i], 0.1f), sunLights[i].transform.localScale.y, sunLights[i].transform.localScale.z);
				}
			}
		}

		transform.localPosition = Vector3.Lerp(transform.localPosition, targetSunPosition, 0.1f);
		voxSun.transform.localScale = Vector3.Lerp(voxSun.transform.localScale, targetSunScale, 0.05f);
		voxRing.transform.localScale = Vector3.Lerp(voxRing.transform.localScale, targetSunScale, 0.05f);
	}

	public void OnBattleStarted()
	{
		WaveLineMaterial.color = ColorManager.Base.Front;
		analyzer_.AttachExPlayer(Music.CurrentSource.Player);//再生開始前じゃないと失敗するらしい
	}

	public void SetState(VoxState newState)
	{
		if( State != newState )
		{
			VoxState oldState = State;
			State = newState;
			switch( State )
			{
			case VoxState.Sun:
			case VoxState.BackToSun:
				AddVPVT(-currentVP, -currentVT);
				WaveLineMaterial.color = ColorManager.Base.Front;

				useTargetLightAngles = true;
				useTargetLightScales = true;
				for( int i = 0; i < lightAngles.Length; i++ )
				{
					targetLightAngles[i] = initialLightAngles[i];
					targetLightScales[i] = initialLightScales[i];
					sunLights[i].transform.localPosition = Vector3.zero;
				}
				GameContext.EnemyConductor.baseColor = Color.black;
				voxRing.SetTargetColor(Color.white);
				voxSun.transform.localScale = Vector3.one;
				voxSun.SetTargetColor(Color.white);
				voxMoon.transform.position = initialMoonPosition;

				targetBGColor = ColorManager.Theme.Light;
				targetSunScale = Vector3.one;
				targetSunPosition = initialSunPosition;
				for( int i = 0; i < lightAngles.Length; i++ )
				{
					targetLightAngles[i] = initialLightAngles[i];
					targetLightScales[i] = initialLightScales[i];
				}
				targetMainLightScale = initialMainLightScale;

				voxRing.SetWidth(initialRingWidth);
				voxRing.SetTargetSize(initialRingRadius);

				break;
			case VoxState.Overload:
				BGColor = Color.black;
				break;
			case VoxState.SunSet:
				BGOffset = Vector3.zero;
				UpdateAnimation();
				useTargetLightAngles = true;
				useTargetLightScales = true;
				AddVPVT(-currentVP, -currentVT);
				targetBGColor = Color.black;
				targetSunScale = Vector3.zero;
				targetSunPosition = sunsetPosition;
				for( int i = 0; i < lightAngles.Length; i++ )
				{
					targetLightAngles[i] = targetLightAngle;
					targetLightScales[i] = 0;
				}
				targetMainLightScale = 0;
				voxMoon.transform.position = initialMoonPosition;
				break;
			}
		}
	}

	public void AddVPVT(int VP, int VT)
	{
		bool oldIsOverFlow = IsOverFlow;
		currentVP = Mathf.Clamp(currentVP + VP, 0, OverflowVP);
		currentVT = Mathf.Clamp(currentVT + VT, 0, MaxVT);
		if( currentVT <= 0 )
		{
			currentVP = 0;
		}
		VPCount.Count = 100.0f * ((float)currentVP / OverflowVP);
		VTCount.Count = currentVT / 64.0f;
		waveRemainCoeff_ = (float)currentVT / MaxVT;

		if( IsOverFlow && !oldIsOverFlow && GameContext.PlayerConductor.CanUseInvert )
		{
			SetState(VoxState.Overflow);
			WaveLineMaterial.color = ColorManager.Accent.Break;
			SEPlayer.Play("invert");
			GameContext.PlayerConductor.OnOverFlowed();
			Music.SetAisac("TrackVolumeEnergy", 1);
		}
		else if( oldIsOverFlow && !IsOverFlow )
		{
			SetState(VoxState.Sun);
			WaveLineMaterial.color = ColorManager.Base.Front;
			Music.SetAisac("TrackVolumeEnergy", 0);
		}
	}

	public void SetTargetEnemy(Enemy targetEnemy)
	{
		currentTargetEnemy_ = targetEnemy;
		Vector3 direction = targetEnemy.targetLocalPosition + Vector3.down * 1.5f - initialSunPosition;
		targetLightAngle = Quaternion.LookRotation(Vector3.forward, -direction).eulerAngles.z;
		if( State == VoxState.Overload || GameContext.BattleState == BattleState.Eclipse )
		{
			for( int i = 0; i < lightAngles.Length; i++ )
			{
				lightAngles[i] = targetLightAngle;
			}
		}
	}

	public void AddDamageGauge(DamageGauge gauge)
	{
		damageGauges_.Add(gauge);
	}
}
