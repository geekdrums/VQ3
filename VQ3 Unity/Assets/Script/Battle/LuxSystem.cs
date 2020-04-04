using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum LuxState
{
	None,
    Sun,
    Overflow,
    Overload,
    SunSet,
}

public enum LuxVersion
{
	None,			//0,シールドなし
	Shield,			//1,シールド発生
	AutoShield,		//2,シールド回復発生
	SafetyAwake,	//3,シールド破壊後に覚醒
	BreakenAwake,	//4,ブレイク後に暴走
}

public class LuxSystem : MonoBehaviour
{
	public static readonly int MaxInvertTime = 4;
	public static readonly int MaxVT = 16 * 4 * 4;
	public static readonly int WaveNum = 33;
	public static readonly float WaveHeight = 2.0f;
	public static readonly float TurnMusicalUnits = 64.0f;
	public int CurrentVP { get; private set; }
	public int CurrentTime { get; private set; }
	public int LastMaxTime { get; private set; }
	public float VPRate { get { return (float)CurrentVP / OverflowVP; } }
	public float VTRate { get { return (LastMaxTime == 0 ? 0 : (float)CurrentTime / LastMaxTime); } }

	public LuxState State { get; private set; }
	public LuxVersion Version { get; private set; }

	public int OverflowVP { get; private set; }
	public bool IsOverFlow { get { return CurrentVP >= OverflowVP; } }
	public bool GetWillEclipse(int addBP) { return CurrentVP + addBP >= OverflowVP; }
	public bool IsInverting { get { return GameContext.BattleState == BattleState.Eclipse && IsOverFlow && Music.Just.Bar >= 2; } }
	public int BreakTime { get; private set; }

	#region animation property

	//game objects
	public MidairPrimitive Sun;
	public MidairPrimitive Ring;
	public GameObject Moon;
	public CounterSprite TimeCount;
	public GaugeRenderer TimeGauge;
	public CounterSprite BreakCount;
	public GaugeRenderer BreakGauge;
	public GaugeRenderer BreakAccentGauge;
	public GameObject[] SunLights;
	public GameObject MainLight;
	public GameObject WaveOrigin;
	public GameObject LightWaveUpOrigin;
	public GameObject LightWaveBottomOrigin;
	public Material WaveLineMaterial;
	public Material LightWaveMaterial;
	public Material LightEdgeMaterial;
	public Color LightWaveTargetColor;
	public float LightRemainTime = 1.0f;
	public Material BGMaterial;
	public CutInUI CutInUI;
	public DamageGauge DamageGauge;

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
	public float LightHoleCoeff = 0.5f;
	public float LightHoleOverflowOffset = 0.2f;
	public float LightHoleDefaultOffset = 0.1f;

	//initial parameters
	Vector3 initialSunPosition;
	Vector3 initialMoonPosition;
	float initialMainLightScale;
	float[] initialLightScales;
	float[] initialLightAngles;
	float initialRingWidth;
	float initialRingRadius;
	Color initialLightWaveColor;

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
	GameObject[] lightUpWaves_;
	GameObject[] lightBottomWaves_;
	float lightHoleRemainTime_ = 0;
	List<float> waveDelta_ = new List<float>();
	float waveRemainCoeff_;
	CriAtomExPlayerOutputAnalyzer analyzer_ = new CriAtomExPlayerOutputAnalyzer(new CriAtomExPlayerOutputAnalyzer.Type[1] { CriAtomExPlayerOutputAnalyzer.Type.LevelMeter });
	#endregion

	void Awake()
	{
		GameContext.LuxSystem = this;
	}

	// Use this for initialization
	void Start()
	{
		State = LuxState.None;
		ColorManager.SetBaseColor(EBaseColor.Black);
		ColorManager.OnBaseColorChanged += this.OnBaseColorChanged;

		BGColor = ColorManager.Theme.Light;
		initialSunPosition = Sun.transform.position;
		initialMoonPosition = Moon.transform.position;
		initialMainLightScale = MainLight.transform.localScale.x;
		targetMainLightScale = initialMainLightScale;

		lightAngles = new float[SunLights.Length];
		initialLightAngles = new float[SunLights.Length];
		initialLightScales = new float[SunLights.Length];
		targetLightAngles = new float[SunLights.Length];
		targetLightScales = new float[SunLights.Length];
		for( int i = 0; i < lightAngles.Length; i++ )
		{
			lightAngles[i] = Quaternion.Angle(Quaternion.identity, SunLights[i].transform.rotation);
			initialLightAngles[i] = lightAngles[i];
			initialLightScales[i] = SunLights[i].transform.localScale.x;
			targetLightAngles[i] = initialLightAngles[i];
			targetLightScales[i] = initialLightScales[i];
		}

		initialRingWidth = Ring.Width;
		initialRingRadius = Ring.Radius;

		transform.localPosition = sunsetPosition;
		Sun.transform.localScale = Vector3.zero;
		//Ring.transform.localScale = Vector3.zero;
		for( int i = 0; i < SunLights.Length; i++ )
		{
			SunLights[i].transform.rotation = Quaternion.identity;
			SunLights[i].transform.localScale = new Vector3(0, SunLights[i].transform.localScale.y, SunLights[i].transform.localScale.z);
		}
		MainLight.transform.localScale = new Vector3(0, MainLight.transform.localScale.y, MainLight.transform.localScale.z);

		vtWaves_ = new GameObject[WaveNum];
		lightUpWaves_ = new GameObject[WaveNum];
		lightBottomWaves_ = new GameObject[WaveNum];
		for( int i = 0; i < WaveNum; i++ )
		{
			vtWaves_[i] = (Instantiate(WaveOrigin) as GameObject);
			vtWaves_[i].transform.parent = WaveOrigin.transform.parent;
			vtWaves_[i].transform.localPosition = WaveOrigin.transform.localPosition + (i % 2 == 0 ? Vector3.right : Vector3.left) * (int)((i + 1)/2);
			vtWaves_[i].transform.localScale = new Vector3(1, 0, 1);
			waveDelta_.Add(0);

			lightUpWaves_[i] = (Instantiate(LightWaveUpOrigin) as GameObject);
			lightUpWaves_[i].transform.parent = LightWaveUpOrigin.transform.parent;
			lightUpWaves_[i].transform.localPosition = LightWaveUpOrigin.transform.localPosition + (i % 2 == 0 ? Vector3.right : Vector3.left) * (int)((i + 1)/2);
			lightUpWaves_[i].transform.localScale = new Vector3(1, 1, 1);

			lightBottomWaves_[i] = (Instantiate(LightWaveBottomOrigin) as GameObject);
			lightBottomWaves_[i].transform.parent = LightWaveBottomOrigin.transform.parent;
			lightBottomWaves_[i].transform.localPosition = LightWaveBottomOrigin.transform.localPosition + (i % 2 == 0 ? Vector3.right : Vector3.left) * (int)((i + 1)/2);
			lightBottomWaves_[i].transform.localScale = new Vector3(1, 1, 1);
		}
		Destroy(WaveOrigin.gameObject);
		Destroy(LightWaveUpOrigin.gameObject);
		Destroy(LightWaveBottomOrigin.gameObject);

		WaveLineMaterial.color = ColorManager.Base.Front;
		initialLightWaveColor = ColorManager.MakeAlpha(Color.white, 0.333f);
		LightWaveMaterial.color = initialLightWaveColor;

		Version = LuxVersion.None;
		OverflowVP = 1;
	}

	// Update is called once per frame
	void Update()
	{
		if( GameContext.State != GameState.Battle ) return;

		if( Version >= LuxVersion.AutoShield )
		{
			UpdateTime();
		}
		UpdateWaves();

		if( GameContext.BattleState == BattleState.Eclipse )
		{
			if( Music.Just.Bar >= 3 ) lightHoleRemainTime_ = -1;
			EclipseUpdate();
			useTargetLightAngles = false;
			useTargetLightScales = false;
			UpdateAnimation();
			return;
		}

		switch( State )
		{
		case LuxState.Sun:
			if( lightHoleRemainTime_ > 0 )
			{
				lightHoleRemainTime_ -= Time.deltaTime;
			}
			UpdateLightAngles();
			UpdateBGOffset();
			BGColor = Color.Lerp(BGColor, ColorManager.Theme.Light, 0.1f);
			BGMaterial.color = BGColor;
			break;
		case LuxState.Overflow:
			UpdateLightAngles();
			UpdateBGOffset();
			BGColor = Color.Lerp(BGColor, ColorManager.Theme.Light, 0.1f);
			BGMaterial.color = BGColor;
			BreakAccentGauge.SetColor(Color.Lerp(ColorManager.Accent.Break, Color.clear, Music.MusicalCos(8) * 0.3f));
			break;
		case LuxState.Overload:
			lightHoleRemainTime_ = -1;
			if( Music.IsNearChangedAt(0) )
			{
				--BreakTime;
			}
			if( Music.IsJustChangedAt(3, 2) && BreakTime == 1 && GameContext.BattleState != BattleState.Win )
			{
				ColorManager.SetBaseColor(EBaseColor.Black);
				ColorManager.SetThemeColor(EThemeColor.White);
				GameContext.EnemyConductor.OnRevert();
				GameContext.PlayerConductor.OnRevert();
				SetState(LuxState.Sun);
				WaveLineMaterial.color = ColorManager.Base.Front;
			}
			break;
		case LuxState.SunSet:
			break;
		}

		if( Music.IsPlaying )
		{
			UpdateAnimation();
		}
	}

	void UpdateTime()
	{
		if( Music.IsJustChanged && CurrentTime > 0 )
		{
			if( (State == LuxState.Sun || State == LuxState.Overflow) && IsInverting == false )
			{
				CurrentTime -= (Music.IsJustChanged ? 1 : 0); //Music.DeltaMT;
				TimeCount.Count = CurrentTime / TurnMusicalUnits;
				if( CurrentTime <= 0 )
				{
					if( CurrentVP >= 20 )
					{
						SEPlayer.Play("vtEmpty");
						CutInUI.SetShieldRecover();
						DamageGauge.OnShieldRecover();
					}
					CurrentTime = 0;
					LastMaxTime = 0;
					CurrentVP = 0;
					BreakCount.Count = CurrentVP;
					Music.SetAisac("TrackVolumeOver", 0);
					WaveLineMaterial.color = ColorManager.Base.Front;
					SetState(LuxState.Sun);
					GameContext.PlayerConductor.CommandGraph.OnShieldRecover();
				}
			}
			else if( State == LuxState.Overload )
			{
				TimeCount.Count = (float)(BreakTime - Music.MusicalTime/ TurnMusicalUnits);
			}
		}
	}

	void UpdateWaves()
	{
		float vtRate = (Version >= LuxVersion.AutoShield ? (float)CurrentTime / MaxVT : 0.3f + 0.3f * ((float)CurrentVP / OverflowVP));
		float maxWaveScale = Mathf.Min(1.0f, (vtRate <= 0.5f ? 0.6f * (float)vtRate / 0.5f :  0.6f + 0.4f * (vtRate - 0.5f) / 0.5f) + waveRemainCoeff_ * 0.5f);
		float targetWaveScale = maxWaveScale;
		float linearFactor = Mathf.Lerp(WaveLinearFactor, 0.99f, vtRate);
		float targetRemainScale = waveRemainCoeff_;
		float remainLinearFactor = (0.9f - 0.9f * (float)CurrentVP/OverflowVP);
		waveRemainCoeff_ *= 0.97f;
		for( int i = 0; i<WaveNum; ++i )
		{
			float waveScale = 0;
			if( i < ((float)CurrentVP / OverflowVP) * WaveNum )
			{
				waveScale = Mathf.Clamp(targetWaveScale + waveDelta_[WaveNum - 1 - i], 0.0f, 1.0f);
				targetWaveScale *= linearFactor;
			}
			else
			{
				waveScale = Mathf.Min(targetWaveScale * 2, targetRemainScale);
				targetRemainScale *= remainLinearFactor;
			}
			vtWaves_[i].transform.localScale = new Vector3(1, Mathf.Lerp(vtWaves_[i].transform.localScale.y, waveScale, 0.2f), 1);
		}
		TimeGauge.SetRate(maxWaveScale, 0.1f);
		BreakGauge.SetRate((float)CurrentVP / OverflowVP, 0.1f);
		if( lightHoleRemainTime_ > 0 )
		{
			for( int i = 0; i<WaveNum; ++i )
			{
				float offset = (State == LuxState.Overflow ? LightHoleOverflowOffset * (GameContext.BattleState == BattleState.Eclipse ? 1.0f - Music.MusicalTime/3.0f : 1.0f) : LightHoleDefaultOffset);
				float targetScale = Mathf.Max(0, 1.0f - vtWaves_[i].transform.localScale.y * LightHoleCoeff - offset);
				lightUpWaves_[i].transform.localScale = new Vector3(1, Mathf.Lerp(lightUpWaves_[i].transform.localScale.y, targetScale, 0.2f), 1);
				lightBottomWaves_[i].transform.localScale = new Vector3(1, Mathf.Lerp(lightBottomWaves_[i].transform.localScale.y, targetScale, 0.2f), 1);
			}
		}
		else
		{
			for( int i = 0; i<WaveNum; ++i )
			{
				lightUpWaves_[i].transform.localScale = new Vector3(1, Mathf.Lerp(lightUpWaves_[i].transform.localScale.y, 1, 0.01f), 1);
				lightBottomWaves_[i].transform.localScale = new Vector3(1, Mathf.Lerp(lightBottomWaves_[i].transform.localScale.y, 1, 0.01f), 1);
			}
		}
		if( Music.IsJustChanged )
		{
			waveDelta_.RemoveAt(0);
			float sin = Mathf.Sin(SinSpeed * (float)Music.MusicalTime);
			float rms = analyzer_.GetRms(0) * RMSCoeff * vtRate + sin * SinCoeff;
			waveDelta_.Add(rms);
		}
		if( ( Version < LuxVersion.AutoShield && CurrentVP > 0 ) || CurrentTime >= 16 )
		{
			LightWaveMaterial.color = Color.Lerp(LightWaveMaterial.color, initialLightWaveColor, 0.05f);
			LightEdgeMaterial.color = Color.Lerp(LightEdgeMaterial.color, ColorManager.MakeAlpha(Color.white, 0), 0.05f);
		}
		else
		{
			LightWaveMaterial.color = Color.Lerp(LightWaveMaterial.color, ColorManager.MakeAlpha(Color.white, 0.8f), 0.05f);
			LightEdgeMaterial.color = Color.Lerp(LightEdgeMaterial.color, Color.white, 0.05f);
		}
	}

	void UpdateLightAngles()
	{
		if( Version < LuxVersion.Shield ) return;

		rotTime_ += Time.deltaTime / (float)Music.Meter.SecPerUnit;
		for( int i = 0; i < lightAngles.Length; i++ )
		{
			float diffToMainLight = SunLights[i].transform.eulerAngles.z - MainLight.transform.eulerAngles.z;
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
		BGOffset = Vector3.Lerp(BGOffset, (blockName == "Intro" || blockName == "wait") ? waitBGOffset : Vector3.zero, 0.1f);
	}

	void EclipseUpdate()
	{
		if( IsOverFlow )
		{
			Ring.transform.localScale = Vector3.Lerp(Ring.transform.localScale, targetSunScale, 0.05f);
			if( Music.Just.Bar < 3 )
			{
				BreakAccentGauge.SetColor(Color.Lerp(ColorManager.Accent.Break, Color.clear, Music.MusicalCos(8) * 0.3f));
			}
		}
		else
		{
			Ring.transform.localScale = Vector3.Lerp(Ring.transform.localScale, Vector3.one, 0.05f);
		}

		if( Music.Just.Bar < 3 || !IsOverFlow )
		{
			float t = (float)Music.MusicalTime / (Music.CurrentUnitPerBar * 3);
			if( !IsOverFlow && Music.Just.Bar >= 2 )
			{
				t = (2.0f / 3.0f) * (Mathf.Max(0, 1.0f - (float)(Music.MusicalTime - Music.CurrentUnitPerBar * 2) / Music.CurrentUnitPerBar));
			}
			Moon.transform.position = Vector3.Lerp(initialMoonPosition, Sun.transform.position + Vector3.back * 75, (-Mathf.Cos(t * Mathf.PI) + 1) / 2);
			Vector3 distance = Moon.transform.position - Sun.transform.position;
			distance.z = 0;
			BGColor = Color.Lerp(BGColor, Color.Lerp(ColorManager.Theme.Light, Color.black, 1.0f / (1.0f + distance.magnitude)), 0.3f);
			BGOffset = Vector3.Lerp(Vector3.zero, Vector3.forward * 10, t * t);
			for( int i = 0; i < lightAngles.Length; i++ )
			{
				lightAngles[i] += (targetLightAngle - lightAngles[i] > 0 ? -1 : 1) * 0.1f * (i+2);
			}
			Ring.SetWidth(initialRingWidth * (1.1f - t));
			Ring.SetSize(initialRingRadius + t);
			Sun.GrowSize = t;
			Sun.SetTargetColor(Color.Lerp(Color.white, Color.black, t*0.2f));
		}

		if( Music.Near.Bar < 2 && GameContext.PlayerConductor.PlayerIsDanger )
		{
			Music.SetAisac("Danger", 1.0f - (float)Music.MusicalTime/32);
		}
		if( Music.IsJustChanged && Music.Just.Bar < 2 )
		{
			Music.SetGameVariable("IsOverflow", IsOverFlow ? 1 : 0);
		}
		if( Music.IsNearChangedAt(2) )
		{
			if( IsOverFlow )
			{
				TextWindow.SetMessage(MessageCategory.Invert, "オーバーロード完了。");
				BreakTime = Mathf.Clamp((int)(CurrentTime / TurnMusicalUnits), 2, MaxInvertTime);
				Music.SetAisac("Danger", 0);
			}
			else
			{
				TextWindow.SetMessage(MessageCategory.Invert, "オーバーロード失敗。");
				Music.SetAisac("Danger", GameContext.PlayerConductor.PlayerIsDanger ? 1 : 0);
			}
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
				Moon.transform.position = Sun.transform.position + Vector3.back * 75.0f;
				BGColor = ColorManager.Base.Back;
				GetComponent<Animation>()["EclipseAnim"].speed = (float)(Music.CurrentTempo / 60.0);
				GetComponent<Animation>().Play();
				GameContext.EnemyConductor.OnInvert();
				GameContext.PlayerConductor.CommandGraph.SelectInitialInvertCommand();
				BGAnimBase.DeactivateCurrentAnim();
			}
			else if( Music.IsJustChangedAt(3, 2) )
			{
				SetState(LuxState.Overload);
				GetComponent<Animation>().Stop();
				GameContext.EnemyConductor.baseColor = Color.white;
				ColorManager.SetBaseColor(EBaseColor.White);
				targetBGColor = ColorManager.Base.Front;
				BGOffset = Vector3.zero;
				Ring.SetColor(Color.clear);
				Ring.SetWidth(initialRingWidth);
				Ring.SetSize(initialRingRadius);
				Sun.transform.localScale = Vector3.zero;

				Enemy refleshTarget = currentTargetEnemy_;
				currentTargetEnemy_ = null;
				SetTargetEnemy(refleshTarget);
				for( int i = 0; i < SunLights.Length; i++ )
				{
					SunLights[i].transform.localPosition = Vector3.right * (i < 2 ? i - 5 : i + 2) * 0.55f;
					SunLights[i].transform.localScale = new Vector3(0.1f, SunLights[i].transform.localScale.y, SunLights[i].transform.localScale.z);
					SunLights[i].transform.rotation = Quaternion.AngleAxis(lightAngles[i], Vector3.forward);
				}
				MainLight.transform.rotation = Quaternion.AngleAxis(targetLightAngle, Vector3.forward);
			}
		}

		BGColor = Color.Lerp(BGColor, targetBGColor, 0.1f);
		BGMaterial.color = BGColor;
	}

	public void UpdateAnimation()
	{
		GameContext.BattleConductor.transform.position = new Vector3(BGOffset.x, BGOffset.y, 0);
		float bgScale = BGScaleCoeff / (BGScaleCoeff + BGOffset.magnitude);
		GameContext.BattleConductor.transform.localScale = new Vector3(bgScale, bgScale, 1);

		if( useTargetMainLightScale )
		{
			if( (MainLight.transform.localScale.x - targetMainLightScale) < 0.1f )
			{
				MainLight.transform.localScale = new Vector3(targetMainLightScale, MainLight.transform.localScale.y, MainLight.transform.localScale.z);
			}
			else
			{
				MainLight.transform.localScale = new Vector3(Mathf.Lerp(MainLight.transform.localScale.x, targetMainLightScale, 0.1f), MainLight.transform.localScale.y, MainLight.transform.localScale.z);
			}
		}
		MainLight.transform.rotation = Quaternion.Lerp(MainLight.transform.rotation, Quaternion.AngleAxis(targetLightAngle, Vector3.forward), 0.2f);
		for( int i = 0; i < SunLights.Length; i++ )
		{
			if( useTargetLightAngles )
			{
				lightAngles[i] = Mathf.Lerp(lightAngles[i], targetLightAngles[i], 0.1f);
			}
			SunLights[i].transform.rotation = Quaternion.Lerp(SunLights[i].transform.rotation, Quaternion.AngleAxis(lightAngles[i], Vector3.forward), 0.2f);
			if( useTargetLightScales )
			{
				if( (SunLights[i].transform.localScale.x - targetLightScales[i]) < 0.1f )
				{
					SunLights[i].transform.localScale = new Vector3(targetLightScales[i], SunLights[i].transform.localScale.y, SunLights[i].transform.localScale.z);
				}
				else
				{
					SunLights[i].transform.localScale = new Vector3(Mathf.Lerp(SunLights[i].transform.localScale.x, targetLightScales[i], 0.1f), SunLights[i].transform.localScale.y, SunLights[i].transform.localScale.z);
				}
			}
		}

		transform.localPosition = Vector3.Lerp(transform.localPosition, targetSunPosition, 0.1f);
		Sun.transform.localScale = Vector3.Lerp(Sun.transform.localScale, targetSunScale, 0.05f);
	}

	public void OnBattleStarted(Encounter encounter)
	{
		WaveLineMaterial.color = ColorManager.Base.Front;
		analyzer_.AttachExPlayer(Music.CurrentSource.Player);//再生開始前じゃないと失敗するらしい

		OverflowVP = encounter.BreakPoint;
		Version = encounter.Version;
		TimeGauge.SetRate(0);
		TimeGauge.transform.parent.gameObject.SetActive(Version >= LuxVersion.AutoShield);
		TimeCount.transform.parent.gameObject.SetActive(Version >= LuxVersion.AutoShield);
		BreakGauge.SetRate(0);
		DamageGauge.OnBattleStarted();

		MainLight.SetActive(Version >= LuxVersion.Shield);
		foreach( GameObject light in SunLights )
		{
			light.SetActive(Version >= LuxVersion.Shield);
		}
	}

	public void OnEclipseEnd()
	{
		if( IsOverFlow == false )
		{
			rotTime_ = 0;
			for( int i = 0; i < lightAngles.Length; i++ )
			{
				targetLightAngles[i] = lightAngles[i] = initialLightAngles[i];
				SunLights[i].transform.rotation = Quaternion.AngleAxis(initialLightAngles[i], Vector3.forward);
			}
			targetLightAngle = 0;
			MainLight.transform.rotation = Quaternion.identity;
		}
	}

	public void OnBaseColorChanged( BaseColor Base )
	{
		WaveLineMaterial.color = ColorManager.Base.Front;
	}

	public void SetState(LuxState newState)
	{
		if( State != newState )
		{
			State = newState;
			switch( State )
			{
			case LuxState.Sun:
				ResetBreak();
				WaveLineMaterial.color = ColorManager.Base.Front;

				useTargetLightAngles = true;
				useTargetLightScales = true;
				for( int i = 0; i < lightAngles.Length; i++ )
				{
					targetLightAngles[i] = initialLightAngles[i];
					targetLightScales[i] = initialLightScales[i];
					SunLights[i].transform.localPosition = Vector3.zero;
				}
				GameContext.EnemyConductor.baseColor = Color.black;
				Ring.SetTargetColor(Color.white);
				Sun.transform.localScale = Vector3.one;
				Sun.SetTargetColor(Color.white);
				Moon.transform.position = initialMoonPosition;

				targetBGColor = ColorManager.Theme.Light;
				targetSunScale = Vector3.one;
				targetSunPosition = initialSunPosition;
				targetMainLightScale = initialMainLightScale;

				Ring.SetWidth(initialRingWidth);
				Ring.SetTargetSize(initialRingRadius);
				BreakAccentGauge.SetColor(Color.clear);
				
				Music.SetAisac("TrackVolumeOver", 0);
				BGAnimBase.DeactivateCurrentAnim();
				break;
			case LuxState.Overload:
				BGColor = Color.black;
				BreakAccentGauge.SetColor(Color.clear);
				break;
			case LuxState.SunSet:
				BGOffset = Vector3.zero;
				UpdateAnimation();
				useTargetLightAngles = true;
				useTargetLightScales = true;
				ResetBreak();
				targetBGColor = Color.black;
				targetSunScale = Vector3.zero;
				targetSunPosition = sunsetPosition;
				for( int i = 0; i < lightAngles.Length; i++ )
				{
					targetLightAngles[i] = targetLightAngle;
					targetLightScales[i] = 0;
				}
				targetMainLightScale = 0;
				Moon.transform.position = initialMoonPosition;
				WaveLineMaterial.color = ColorManager.Base.Front;
				Music.SetAisac("TrackVolumeOver", 0);
				BreakAccentGauge.SetColor(Color.clear);
				break;
			case LuxState.Overflow:
				WaveLineMaterial.color = ColorManager.Accent.Break;
				SEPlayer.Play("invert");
				GameContext.PlayerConductor.OnOverFlowed();
				Music.SetAisac("TrackVolumeOver", 1);
				break;
			}
		}
	}

	public void AddVP(int VP, int Time)
	{
		bool oldIsOverFlow = IsOverFlow;
		CurrentVP = Mathf.Clamp(CurrentVP + VP, 0, OverflowVP);
		if( Version >= LuxVersion.AutoShield )
		{
			CurrentTime = Mathf.Clamp(CurrentTime + Time, 0, MaxVT);
			LastMaxTime = CurrentTime;
			if( CurrentTime <= 0 )
			{
				CurrentVP = 0;
				CurrentTime = 0;
				LastMaxTime = 0;
				DamageGauge.OnShieldRecover();
			}
		}
		BreakCount.Count = 100.0f * ((float)CurrentVP / OverflowVP);
		TimeCount.Count = CurrentTime / TurnMusicalUnits;
		waveRemainCoeff_ = (Version >= LuxVersion.AutoShield ? (float)CurrentTime / MaxVT : 0.1f + 0.3f * ((float)CurrentVP / OverflowVP));
		if( CurrentVP > 0 )
		{
			lightHoleRemainTime_ = LightRemainTime;
			if( IsOverFlow == false )
			{
				LightWaveMaterial.color = LightWaveTargetColor;
			}
		}

		if( IsOverFlow && !oldIsOverFlow )
		{
			SetState(LuxState.Overflow);
			GameContext.PlayerConductor.CommandGraph.OnShieldBreak();
		}
		else if( oldIsOverFlow && !IsOverFlow && State != LuxState.SunSet )
		{
			SetState(LuxState.Sun);
			GameContext.PlayerConductor.CommandGraph.OnShieldRecover();
		}
	}

	public void ResetBreak()
	{
		CurrentVP = 0;
		CurrentTime = 0;
		LastMaxTime = 0;
		BreakCount.Count = 0;
		TimeCount.Count = 0;
		waveRemainCoeff_ = 0;
		WaveLineMaterial.color = ColorManager.Base.Front;
		BreakAccentGauge.SetColor(Color.clear);
		Music.SetAisac("TrackVolumeOver", 0);
	}

	public void SetTargetEnemy(Enemy targetEnemy)
	{
		currentTargetEnemy_ = targetEnemy;
		Vector3 direction = targetEnemy.targetLocalPosition + Vector3.down * 1.5f - initialSunPosition;
		targetLightAngle = Quaternion.LookRotation(Vector3.forward, -direction).eulerAngles.z;
		if( State == LuxState.Overload || GameContext.BattleState == BattleState.Eclipse )
		{
			for( int i = 0; i < lightAngles.Length; i++ )
			{
				lightAngles[i] = targetLightAngle;
			}
		}
	}

	public void Event_ShowBPMeter(bool init = false)
	{
		if( init )
		{
			CurrentVP = 0;
			OverflowVP = 100;
			BreakAccentGauge.SetColor(Color.clear);
			TimeCount.transform.parent.gameObject.SetActive(false);
			TimeGauge.transform.parent.gameObject.SetActive(false);
		}
		UpdateWaves();
		if( IsOverFlow )
		{
			BreakAccentGauge.SetColor(Color.Lerp(ColorManager.Accent.Break, Color.clear, Music.MusicalCos(8) * 0.3f));
		}
	}
}
