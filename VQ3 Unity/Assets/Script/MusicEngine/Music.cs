//Copyright (c) 2014 geekdrums
//Released under the MIT license
//http://opensource.org/licenses/mit-license.php
//Feel free to use this for your lovely musical games :)

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CriAtomSource))]
public class Music : MonoBehaviour
{
	#region BlockInfo class
	[System.Serializable]
	public class BlockInfo
	{
		public BlockInfo(string BlockName, int NumBar = 4)
		{
			this.BlockName = BlockName;
			this.NumBar = NumBar;
		}
		public string BlockName;
		public int NumBar = 4;
	}
	#endregion

	#region editor params
	/// <summary>
	/// how many MusicTime in a beat. maybe 4 or 3 or 6.
	/// </summary>
	public int UnitPerBeat = 4;
	/// <summary>
	/// how many MusicTime in a bar.
	/// </summary>
	public int UnitPerBar = 16;
	/// <summary>
	/// Musical Tempo. how many beats in a minutes.
	/// </summary>
	public double Tempo = 120;

	public List<BlockInfo> BlockInfos;

	/// <summary>
	/// put your debug TextMesh to see current musical time & block info.
	/// </summary>
	public TextMesh DebugText;
	#endregion

	static Music Current_;
	static List<Music> MusicList = new List<Music>();
	static event EventHandler OnNextBlockStarted;

	#region public static properties
	public static bool IsPlaying { get { return Current_.IsPlaying_; } }
	/// <summary>
	/// means last timing.
	/// </summary>
	public static Timing Just { get { return Current_.just_; } }
	/// <summary>
	/// means nearest timing.
	/// </summary>
	public static Timing Near { get { return Current_.near_; } }
	/// <summary>
	/// is Just changed in this frame or not.
	/// </summary>
	public static bool IsJustChanged { get { return Current_.isJustChanged_; } }
	/// <summary>
	/// is Near changed in this frame or not.
	/// </summary>
	public static bool IsNearChanged { get { return Current_.isNearChanged_; } }
	/// <summary>
	/// is currently former half in a MusicTimeUnit, or last half.
	/// </summary>
	public static bool IsFormerHalf { get { return Current_.isFormerHalf_; } }
	/// <summary>
	/// how many times you repeat current music/block.
	/// </summary>
	public static int NumRepeat { get { return Current_.numRepeat_; } }

	/// <summary>
	/// returns how long from nearest Just timing with sign.
	/// </summary>
	public static double SecFromJust { get { return Current_.SecFromJust_; } }
	/// <summary>
	/// returns how long from nearest Just timing absolutely.
	/// </summary>
	public static double SecFromJustAbs { get { return Current_.SecFromJustAbs_; } }
	/// <summary>
	/// returns normalized lag.
	/// </summary>
	public static double UnitFromJust { get { return Current_.UnitFromJust_; } }

	/// <summary>
	/// current musical meter
	/// </summary>
	public static Meter Meter { get { return Current_.currentMeter_; } }
	/// <summary>
	/// current musical time in units
	/// </summary>
	public static int JustTotalUnits { get { return Current_.JustTotalUnits_; } }
	/// <summary>
	/// current musical time in units
	/// </summary>
	public static int NearTotalUnits { get { return Current_.NearTotalUnits_; } }
	/// <summary>
	/// current musical time in bars
	/// </summary>
	public static float MusicalTime { get { return Current_.MusicalTime_; } }

	/// <summary>
	/// returns musically synced cos wave.
	/// if default( MusicalCos(16,0,0,1),
	/// starts from max=1,
	/// reaches min=0 on MusicalTime = cycle/2 = 8,
	/// back to max=1 on MusicalTIme = cycle = 16.
	/// </summary>
	/// <param name="cycle">wave cycle in musical unit</param>
	/// <param name="offset">wave offset in musical unit</param>
	/// <param name="min"></param>
	/// <param name="max"></param>
	/// <returns></returns>
	public static float MusicalCos(float cycle = 16, float offset = 0, float min = 0, float max = 1)
	{
		return Mathf.Lerp(min, max, ((float)Math.Cos(Math.PI * 2 * (CurrentUnitPerBar * MusicalTime + offset) / cycle) + 1.0f) / 2.0f);
	}

	public static int CurrentUnitPerBar { get { return Current_.UnitPerBar; } }
	public static int CurrentUnitPerBeat { get { return Current_.UnitPerBeat; } }
	public static double CurrentTempo { get { return Current_.Tempo; } }
	public static string CurrentMusicName { get { return Current_.name; } }
	public static string CurrentBlockName { get { return Current_.CurrentBlock_.BlockName; } }
	public static string NextBlockName { get { return (Current_.nextBlockIndex_ >= 0 ? Current_.NextBlock_.BlockName : ""); } }
	public static CriAtomSource CurrentSource { get { return Current_.musicSource_; } }
	public static BlockInfo CurrentBlock { get { return Current_.CurrentBlock_; } }
	public static BlockInfo NextBlock { get { return (Current_.nextBlockIndex_ >= 0 ? Current_.NextBlock_ : null); } }
	#endregion

	#region public static predicates
	public static bool IsJustChangedWhen(Predicate<Timing> pred)
	{
		return Current_.IsJustChangedWhen_(pred);
	}
	public static bool IsJustChangedBar()
	{
		return Current_.IsJustChangedBar_();
	}
	public static bool IsJustChangedBeat()
	{
		return Current_.IsJustChangedBeat_();
	}
	public static bool IsJustChangedAt(int bar = 0, int beat = 0, int unit = 0)
	{
		return Current_.IsJustChangedAt_(bar, beat, unit);
	}
	public static bool IsJustChangedAt(Timing t)
	{
		return Current_.IsJustChangedAt_(t.Bar, t.Beat, t.Unit);
	}

	public static bool IsNearChangedWhen(Predicate<Timing> pred)
	{
		return Current_.IsNearChangedWhen_(pred);
	}
	public static bool IsNearChangedBar()
	{
		return Current_.IsNearChangedBar_();
	}
	public static bool IsNearChangedBeat()
	{
		return Current_.IsNearChangedBeat_();
	}
	public static bool IsNearChangedAt(int bar, int beat = 0, int unit = 0)
	{
		return Current_.IsNearChangedAt_(bar, beat, unit);
	}
	public static bool IsNearChangedAt(Timing t)
	{
		return Current_.IsNearChangedAt_(t.Bar, t.Beat, t.Unit);
	}
	#endregion

	#region public static functions
	/// <summary>
	/// Change Current Music.
	/// </summary>
	/// <param name="MusicName">name of the GameObject that include Music</param>
	public static void Play(string MusicName, string firstBlockName = "")
	{
		Music music = MusicList.Find((Music m) => m != null && m.name == MusicName);
		if( music != null )
		{
			music.PlayStart(firstBlockName);
		}
		else
		{
			Debug.Log("Can't find music: " + MusicName);
		}
	}
	/// <summary>
	/// Quantize to musical time( default is 16 beat ).
	/// </summary>
	/// <param name="source">your sound source( Unity AudioSource or ADX CriAtomSource )</param>
	public static void QuantizePlay(CriAtomSource source, int transpose = 0, float allowRange = 0.3f)
	{
		source.pitch = Mathf.Pow(PITCH_UNIT, transpose);
		if( IsFormerHalf && UnitFromJust < allowRange )
		{
			source.Play();
		}
		else
		{
			Current_.quantizedCue_.Add(source);
		}
	}
	public static readonly float PITCH_UNIT = Mathf.Pow(2.0f, 1.0f / 12.0f);
	public static void Pause() { Current_.musicSource_.Pause(true); }
	public static void Resume() { Current_.musicSource_.Pause(false); }
	public static void Stop() { Current_.musicSource_.Stop(); }
	public static void SetVolume(float volume)
	{
		Current_.musicSource_.volume = volume;
	}

	//adx2 functions
	public static void SetNextBlock(string blockName, EventHandler onNextBlockStarted = null)
	{
		if( blockName == CurrentBlockName ) return;
		//Debug.Log("SetNextBlock : " + blockName);
		int index = Current_.BlockInfos.FindIndex((BlockInfo info) => info.BlockName == blockName);
		if( index >= 0 )
		{
			Current_.nextBlockIndex_ = index;
			Current_.playback_.SetNextBlockIndex(index);
			OnNextBlockStarted = null;
			if( onNextBlockStarted != null )
				OnNextBlockStarted += onNextBlockStarted;
		}
		else
		{
			Debug.LogError("Error!! Music.SetNextBlock Can't find block name: " + blockName);
		}
	}
	public static void SetNextBlock(int index, EventHandler onNextBlockStarted = null)
	{
		if( index == Current_.currentBlockIndex_ ) return;
		if( index < Current_.cueInfo_.numBlocks )
		{
			Current_.nextBlockIndex_ = index;
			Current_.playback_.SetNextBlockIndex(index);
			if( onNextBlockStarted != null )
				OnNextBlockStarted += onNextBlockStarted;
		}
		else
		{
			Debug.LogError("Error!! Music.SetNextBlock index is out of range: " + index);
		}
	}
	public static void SetFirstBlock(string blockName)
	{
		int index = Current_.BlockInfos.FindIndex((BlockInfo info) => info.BlockName == blockName);
		if( index >= 0 )
		{
			Current_.nextBlockIndex_ = index;
			Current_.currentBlockIndex_ = index;
			Current_.musicSource_.Player.SetFirstBlockIndex(index);
		}
		else
		{
			Debug.LogError("Error!! Music.SetFirstBlock Can't find block name: " + blockName);
		}
	}
	public static void SetFirstBlock(int index)
	{
		if( index < Current_.cueInfo_.numBlocks )
		{
			Current_.nextBlockIndex_ = index;
			Current_.currentBlockIndex_ = index;
			Current_.musicSource_.Player.SetFirstBlockIndex(index);
		}
		else
		{
			Debug.LogError("Error!! Music.SetFirstBlock index is out of range: " + index);
		}
	}
	public static void SetAisac(uint index, float value)
	{
		Current_.musicSource_.SetAisac(index, value);
	}
	public static void SetAisac(string controlName, float value)
	{
		Current_.musicSource_.SetAisac(controlName, value);
	}
	public static void SetGameVariable(string variableName, float value)
	{
		CriAtomEx.SetGameVariable(variableName, value);
	}
	#endregion

	#region private params

	// 現在再生中の箇所のメーター情報。
	private Meter currentMeter_;
	// 最新のJustタイミング。(タイミングちょうどになってから切り替わる）
	private Timing just_;
	// 最新のNearタイミング。（最も近いタイミングが変わった地点、つまり2つのタイミングの中間で切り替わる）
	private Timing near_;
	// 1フレーム前のJustタイミング。
	private Timing oldJust_;
	// 1フレーム前のNearタイミング。
	private Timing oldNear_;

	// 今のフレームでjust_が変化したフラグ。
	private bool isJustChanged_ = false;
	// 今のフレームでnear_が変化したフラグ。
	private bool isNearChanged_ = false;
	// 今のフレームでjust_がループして戻ったフラグ。
	private bool isJustLooped_ = false;
	// 今のフレームでnear_がループして戻ったフラグ。
	private bool isNearLooped_ = false;
	// 今がunit内の前半かどうか。 true なら just_ == near_, false なら ++just == near。
	private bool isFormerHalf_;

	// 現在の曲のサンプルレート。
	private int sampleRate_ = 44100;
	// 現在の再生サンプル数。
	private int currentSample_;
	// Justのタイミングから何サンプル過ぎているか。
	private int samplesFromJust_;
	// 現在のループカウント。
	private int numRepeat_;

	// musicSource_にPlayOnStart属性があるかどうか。
	private bool playOnStart_ = false;


	// ADX関連
	private CriAtomSource musicSource_;
	private CriAtomExPlayback playback_;
	private CriAtomExAcb acbData_;
	private int currentBlockIndex_;
	private int nextBlockIndex_;

	private CriAtomEx.CueInfo cueInfo_;
	private List<CriAtomSource> quantizedCue_ = new List<CriAtomSource>();

	private int oldBlockIndex_;
	private int numBlockBar_;

	#endregion

	#region private properties

	private double SecFromJust_
	{
		get
		{
			if( isFormerHalf_ )
				return samplesFromJust_ / sampleRate_;
			else
				return samplesFromJust_ / sampleRate_ - currentMeter_.SecPerUnit;
		}
	}
	private double SecFromJustAbs_ { get { return Math.Abs(SecFromJust_); } }
	private double UnitFromJust_ { get { return SecFromJust_ / currentMeter_.SecPerUnit; } }
	private double UnitFromJustAbs_ { get { return Math.Abs(UnitFromJust_); } }

	private int JustTotalUnits_ { get { return just_.GetTotalUnits(currentMeter_); } }
	private int NearTotalUnits_ { get { return near_.GetTotalUnits(currentMeter_); } }
	private float MusicalTime_ { get { return currentMeter_.GetMusicalTime(just_, samplesFromJust_); } }

	// ADX関連
	private bool IsPlaying_ { get { return musicSource_ != null && musicSource_.status == CriAtomSource.Status.Playing; } }
	private BlockInfo CurrentBlock_ { get { return BlockInfos[currentBlockIndex_]; } }
	private BlockInfo NextBlock_ { get { return BlockInfos[nextBlockIndex_]; } }
	private int SamplesInLoop_ { get { return numBlockBar_ * currentMeter_.SamplesPerBar; } }
	#endregion

	#region private predicates
	private bool IsNearChangedWhen_(Predicate<Timing> pred)
	{
		if( isNearChanged_ )
		{
			Timing t = new Timing(oldNear_);
			t.Increment(currentMeter_);
			if( isNearLooped_ )
			{
				int numBar = BlockInfos[oldBlockIndex_].NumBar;
				t.LoopBack(numBar, currentMeter_);
				Timing end = new Timing(near_);
				end.Add(numBar, 0, 0, currentMeter_);
				for( ; t <= end; t.Increment(currentMeter_) )
				{
					if( t.Bar >= numBar )
					{
						t.LoopBack(numBar, currentMeter_);
						end.LoopBack(numBar, currentMeter_);
					}
					if( pred(t) ) return true;
				}
			}
			else
			{
				for( ; t <= near_; t.Increment(currentMeter_) )
				{
					if( pred(t) ) return true;
				}
			}
		}
		return false;
	}
	private bool IsNearChangedBar_()
	{
		return isNearChanged_ && (oldNear_.Bar != near_.Bar);
	}
	private bool IsNearChangedBeat_()
	{
		return isNearChanged_ && (oldNear_.Beat != near_.Beat);
	}
	private bool IsNearChangedAt_(int bar = 0, int beat = 0, int unit = 0)
	{
		return IsNearChangedAt_(new Timing(bar, beat, unit));
	}
	private bool IsNearChangedAt_(Timing t)
	{
		return (isNearChanged_ && (oldNear_ < t && t <= near_)) || (isNearLooped_ && (oldNear_ < t || t <= near_));
	}
	private bool IsJustChangedWhen_(Predicate<Timing> pred)
	{
		if( isJustChanged_ )
		{
			Timing t = new Timing(oldJust_);
			t.Increment(currentMeter_);
			if( isJustLooped_ )
			{
				int numBar = BlockInfos[oldBlockIndex_].NumBar;
				t.LoopBack(numBar, currentMeter_);
				Timing end = new Timing(just_);
				end.Add(numBar, 0, 0, currentMeter_);
				for( ; t <= end; t.Increment(currentMeter_) )
				{
					if( t.Bar >= numBar )
					{
						t.LoopBack(numBar, currentMeter_);
						end.LoopBack(numBar, currentMeter_);
					}
					if( pred(t) ) return true;
				}
			}
			else
			{
				for( ; t <= just_; t.Increment(currentMeter_) )
				{
					if( pred(t) ) return true;
				}
			}
		}
		return false;
	}
	private bool IsJustChangedBar_()
	{
		return isJustChanged_ && (oldJust_.Bar != just_.Bar);
	}
	private bool IsJustChangedBeat_()
	{
		return isJustChanged_ && (oldJust_.Beat != just_.Beat);
	}
	private bool IsJustChangedAt_(int bar = 0, int beat = 0, int unit = 0)
	{
		return IsJustChangedAt_(new Timing(bar, beat, unit));
	}
	private bool IsJustChangedAt_(Timing t)
	{
		return (isJustChanged_ && (oldJust_ < t && t <= just_)) || (isJustLooped_ && (oldJust_ < t || t <= just_));
	}
	#endregion

	#region Initialize & Update
	void Awake()
	{
		MusicList.Add(this);
		musicSource_ = GetComponent<CriAtomSource>();
		if( Current_ == null || musicSource_.playOnStart )
		{
			if( musicSource_.playOnStart )
			{
				musicSource_.playOnStart = false;
				playOnStart_ = true;
			}
			Current_ = this;
		}
		acbData_ = CriAtom.GetAcb(musicSource_.cueSheet);
		acbData_.GetCueInfo(musicSource_.cueName, out cueInfo_);

		currentMeter_ = new Meter(0, UnitPerBeat, UnitPerBar, Tempo);
		currentMeter_.OnValidate(sampleRate_);

		Initialize();
	}

	void Initialize()
	{
		currentSample_ = 0;
		isJustChanged_ = false;
		isNearChanged_ = false;
		isJustLooped_ = false;
		isNearLooped_ = false;
		near_ = new Timing(0, 0, -1);
		just_ = new Timing(near_);
		oldNear_ = new Timing(near_);
		oldJust_ = new Timing(just_);
		samplesFromJust_ = 0;
		isFormerHalf_ = true;
		numRepeat_ = 0;

		currentBlockIndex_ = 0;
		oldBlockIndex_ = 0;
		nextBlockIndex_ = 0;
	}

	void PlayStart(string firstBlockName = "")
	{
		if( Current_ != null && IsPlaying )
		{
			Stop();
		}

		Current_ = this;
		Initialize();

		if( firstBlockName != "" )
		{
			SetFirstBlock(firstBlockName);
		}
		playback_ = musicSource_.Play();
		numBlockBar_ = BlockInfos[currentBlockIndex_].NumBar;
	}

	// Use this for initialization
	void Start()
	{
#if UNITY_EDITOR
		 UnityEditor.EditorApplication.pauseStateChanged += OnPlaymodeStateChanged;
#endif
		if( playOnStart_ )
		{
			PlayStart();
		}
	}

	// Update is called once per frame
	void Update()
	{
		if( IsPlaying )
		{
			UpdateTiming();
			UpdateQuantizedCue();
		}
	}

#if UNITY_EDITOR
	void OnPlaymodeStateChanged(UnityEditor.PauseState state)
	{
		if( Current_.musicSource_.Player != null )
		{
			if ( state == UnityEditor.PauseState.Paused )
			{
				Pause();
			}
			else
			{
				Resume();
			}
		}
	}
#endif

	void UpdateTiming()
	{
		int oldSample = currentSample_;
		oldNear_.Set(near_);
		oldJust_.Set(just_);
		isNearChanged_ = false;
		isJustChanged_ = false;
		isJustLooped_ = false;
		isNearLooped_ = false;

		long numSamples;
		int sampleRate;
		if( !playback_.GetNumPlayedSamples(out numSamples, out sampleRate) )
		{
			numSamples = -1;
		}
		currentSample_ = (int)numSamples;
		if( currentSample_ >= 0 )
		{
			int meterSample = currentSample_ - currentMeter_.StartSamples;
			int bar = (int)(meterSample / currentMeter_.SamplesPerBar);
			int beat = (int)((meterSample - bar * currentMeter_.SamplesPerBar) / currentMeter_.SamplesPerBeat);
			int unit = (int)(((meterSample - bar * currentMeter_.SamplesPerBar) - beat * currentMeter_.SamplesPerBeat) / currentMeter_.SamplesPerUnit);
			just_.Set(bar + currentMeter_.StartBar, beat, unit);
			just_.Fix(currentMeter_);

			samplesFromJust_ = currentSample_ - currentMeter_.GetSampleFromTiming(just_);
			isFormerHalf_ = samplesFromJust_ < currentMeter_.SamplesPerUnit / 2;

			near_.Set(just_);
			if( !isFormerHalf_ )
			{
				near_.Increment(currentMeter_);
			}

			if( numBlockBar_ > 0 )
			{
				while( just_.Bar >= numBlockBar_ )
				{
					just_.Decrement(currentMeter_);
				}
				if( near_.Bar >= numBlockBar_ )
				{
					near_.LoopBack(numBlockBar_, currentMeter_);
				}
			}

			isJustChanged_ = (just_.Equals(oldJust_) == false);
			isNearChanged_ = (near_.Equals(oldNear_) == false);
			isJustLooped_ = isJustChanged_ && just_ < oldJust_;
			isNearLooped_ = isNearChanged_ && near_ < oldNear_;

			if( isJustLooped_ )
			{
				oldBlockIndex_ = currentBlockIndex_;
				currentBlockIndex_ = playback_.GetCurrentBlockIndex();
				numBlockBar_ = BlockInfos[currentBlockIndex_].NumBar;
				if( oldBlockIndex_ == currentBlockIndex_ )
				{
					OnBlockRepeated();
				}
				else
				{
					OnBlockChanged();
					oldJust_.Set(BlockInfos[oldBlockIndex_].NumBar);
					oldJust_.Decrement(currentMeter_);
				}
			}
		}

		if( DebugText != null )
		{
			DebugText.text = String.Format("Just = {0}, old = {1}, MusicalTime = {2}", Just.ToString(), oldJust_.ToString(), MusicalTime_);
			if( BlockInfos.Count > 0 )
			{
				DebugText.text += String.Format("{0}block[{1}] = {2}({3}bar)", System.Environment.NewLine, currentBlockIndex_, CurrentBlock_.BlockName, numBlockBar_);
			}
		}
	}

	void UpdateQuantizedCue()
	{
		if( isJustChanged_ )
		{
			foreach( CriAtomSource cue in quantizedCue_ )
			{
				cue.Play();
			}
			quantizedCue_.Clear();
		}
	}

	#endregion

	#region Events

	void OnBlockRepeated()
	{
		nextBlockIndex_ = -1;
		++numRepeat_;
	}

	void OnBlockChanged()
	{
		numRepeat_ = 0;
		nextBlockIndex_ = -1;
		//Debug.Log("Music::OnBlockChanged " + CurrentBlockName);
		if( OnNextBlockStarted != null )
		{
			OnNextBlockStarted(null, null);
			OnNextBlockStarted = null;
		}
	}

	#endregion
}
