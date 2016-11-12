//Copyright (c) 2014 geekdrums
//Released under the MIT license
//http://opensource.org/licenses/mit-license.php
//Feel free to use this for your lovely musical games :)

/* Note1: You must do this
Add this code in Plugins/CriWare/CriWare/CriAtomSource.cs
so that you can use SetFirstBlock function.
public CriAtomExPlayer Player
{
	get { return this.player; }
}
http://www53.atwiki.jp/soundtasukeai/pages/22.html#id_6c095b2d
*/

/* Note2: If you use many blocks in ADX2, you should do this
Auto update of block info.

Add this function in Editor/CriWare/CriAtom/CriAtomWindow.cs
...
using System.Xml;
...

	//(((((((((((((((geekdrums MusicEngine(((((((((((((((
	private void UpdateBlockInfo()
	{
		string sourceDirName = atomCraftOutputAssetsRootPath.Replace( "/Assets", "" );
		string[] files = System.IO.Directory.GetFiles( sourceDirName );
		foreach( string file in files )
		{
			if( file.EndsWith( "_acb_info.xml" ) )
			{
				string fileName = System.IO.Path.GetFileName( file );
				GameObject musicObj = GameObject.Find( fileName.Replace( "_acb_info.xml", "" ) );
				Music adxMusic = null;
				if( musicObj != null )
				{
					adxMusic = musicObj.GetComponent<Music>();
				}
				if( adxMusic != null )
				{
					adxMusic.BlockInfos.Clear();

					XmlReaderSettings settings = new XmlReaderSettings();
					settings.IgnoreWhitespace = true;
					settings.IgnoreComments = true;
					using( XmlReader reader = XmlReader.Create( System.IO.File.OpenText( file ), settings ) )
					{
						while( reader.Read() )
						{
							if( reader.GetAttribute( "Bpm" ) != null )
							{
								adxMusic.Tempo = double.Parse( reader.GetAttribute( "Bpm" ) );
							}
							if( adxMusic.Tempo > 0 && reader.GetAttribute( "BlockEndPositionMs" ) != null )
							{
								string blockName = reader.GetAttribute( "OrcaName" );
								int msec = int.Parse( reader.GetAttribute( "BlockEndPositionMs" ) );
								Music.BlockInfo blockInfo = new Music.BlockInfo( blockName, Mathf.RoundToInt( (msec / 1000.0f) / (4 * 60.0f / (float)adxMusic.Tempo) ) );
								adxMusic.BlockInfos.Add( blockInfo );
							}
						}
						reader.Close();
					}
				}
				//else there are no Music component for this AtomSource.
			}
			//else this file is not _acb_info.
		}
	}
	//(((((((((((((((geekdrums MusicEngine(((((((((((((((

and call this function from GUIImportAssetsFromAtomCraft() in same source code,
just after  CopyDirectory(atomCraftOutputAssetsRootPath, Application.dataPath); executed.
like this
				try
				{
					CopyDirectory(atomCraftOutputAssetsRootPath, Application.dataPath);
					Debug.Log("Complete Update Assets of \"CRI Atom Craft\"");

					//geekdrums MusicEngine
					UpdateBlockInfo();
				}

Preparation:
- Create a CriAtomSource object with Music component,
- specify UnitPerBeat, UnitPerBar and Tempo,
- name it with same name of CueName,
- then open CRI Atom window and push "Update Assets of CRI Atom Craft" button,
- your BlockInfo will be automatically updated.

*/

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
	static readonly int SamplingRate = 44100;
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
	/// delta time from JustChanged.
	/// </summary>
	public static double TimeSecFromJust { get { return Current_.timeSecFromJust_; } }
	/// <summary>
	/// how many times you repeat current music/block.
	/// </summary>
	public static int NumRepeat { get { return Current_.numRepeat_; } }
	/// <summary>
	/// returns how long from nearest Just timing with sign.
	/// </summary>
	public static double lLg { get { return Current_.Lag_; } }
	/// <summary>
	/// returns how long from nearest Just timing absolutely.
	/// </summary>
	public static double LagAbs { get { return Current_.LagAbs_; } }
	/// <summary>
	/// returns normalized lag.
	/// </summary>
	public static double LagUnit { get { return Current_.LagUnit_; } }
	/// <summary>
	/// delta musical time from last update.
	/// only positive when IsJustChanged == true.
	/// </summary>
	public static int DeltaMT { get { return Current_.deltaMusicalTime_; } }
	/// <summary>
	/// sec / musicalUnit
	/// </summary>
	public static double MusicalTimeUnit { get { return Current_.musicalTimeUnit_; } }
	/// <summary>
	/// current musical time based on MusicalTimeUnit
	/// </summary>
	public static float MusicalTime { get { return Current_.MusicalTime_; } }
	/// <summary>
	/// current musical time based on MusicalTimeUnit
	/// </summary>
	public static float MusicalTimeBar { get { return Current_.MusicalTimeBar_; } }
	/// <summary>
	/// dif from timing to now on musical time unit.
	/// </summary>
	/// <param name="timing"></param>
	/// <returns></returns>
	public static float MusicalTimeFrom(Timing timing)
	{
		return MusicalTime - timing.MusicalTime;
	}
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
		return Mathf.Lerp(min, max, ((float)Math.Cos(Math.PI * 2 * (MusicalTime + offset) / cycle) + 1.0f)/2.0f);
	}

	public static int CurrentUnitPerBar { get { return Current_.UnitPerBar; } }
	public static int CurrentUnitPerBeat { get { return Current_.UnitPerBeat; } }
	public static double CurrentTempo { get { return Current_.Tempo; } }
	public static string CurrentMusicName { get { return Current_.name; } }
	public static string CurrentBlockName { get { return Current_.CurrentBlock_.BlockName; } }
	public static string NextBlockName { get { return Current_.NextBlock_.BlockName; } }
	public static CriAtomSource CurrentSource { get { return Current_.musicSource_; } }
	public static BlockInfo CurrentBlock { get { return Current_.CurrentBlock_; } }
	public static BlockInfo NextBlock { get { return Current_.NextBlock_; } }
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
		if( IsFormerHalf && LagUnit < allowRange )
		{
			source.Play();
		}
		else
		{
			Current_.quantizedCue_.Add(source);
		}
	}
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
	private Timing just_;
	private Timing near_;
	private bool isJustChanged_;
	private bool isNearChanged_;
	private bool isJustLooped_;
	private bool isNearLooped_;
	private bool isFormerHalf_;
	private double timeSecFromJust_;
	private int numRepeat_;
	private double musicalTimeUnit_;
	private int deltaMusicalTime_;

	private CriAtomSource musicSource_;
	private CriAtomExPlayback playback_;
	private CriAtomExAcb acbData_;
	private CriAtomEx.CueInfo cueInfo_;
	private List<CriAtomSource> quantizedCue_ = new List<CriAtomSource>();
	private int currentBlockIndex_;
	private int currentSample_;
	private int nextBlockIndex_;

	private int samplesPerUnit_;
	private int samplesPerBeat_;
	private int samplesPerBar_;

	private Timing oldNear_, oldJust_;
	private int oldBlockIndex_;
	private int numBlockBar_;
	private bool playOnStart_ = false;
	private static readonly float PITCH_UNIT = Mathf.Pow(2.0f, 1.0f / 12.0f);
	#endregion

	#region private properties
	private double Lag_
	{
		get
		{
			if( isFormerHalf_ )
				return timeSecFromJust_;
			else
				return timeSecFromJust_ - musicalTimeUnit_;
		}
	}
	private double LagAbs_
	{
		get
		{
			if( isFormerHalf_ )
				return timeSecFromJust_;
			else
				return musicalTimeUnit_ - timeSecFromJust_;
		}
	}
	private double LagUnit_ { get { return Lag_ / musicalTimeUnit_; } }
	private float MusicalTime_ { get { return (float)(just_.MusicalTime + timeSecFromJust_ / musicalTimeUnit_); } }
	private float MusicalTimeBar_ { get { return MusicalTime_/UnitPerBar; } }
	private bool IsPlaying_ { get { return musicSource_ != null && musicSource_.status == CriAtomSource.Status.Playing; } }

	private BlockInfo CurrentBlock_ { get { return BlockInfos[currentBlockIndex_]; } }
	private BlockInfo NextBlock_ { get { return BlockInfos[nextBlockIndex_]; } }
	private int SamplesInLoop_ { get { return numBlockBar_ * samplesPerBar_; } }
	#endregion

	#region private predicates
	private bool IsNearChangedWhen_(Predicate<Timing> pred)
	{
		if( isNearChanged_ )
		{
			Timing t = new Timing(oldNear_);
			t.Add(0, 0, 1);
			int numBar = BlockInfos[oldBlockIndex_].NumBar;
			t.LoopBack(numBar);
			for( ; t <= near_; t.Add(0, 0, 1), t.LoopBack(numBar) )
			{
				if( pred(t) )
				{
					return true;
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
			t.Add(0, 0, 1);
			int numBar = BlockInfos[oldBlockIndex_].NumBar;
			t.LoopBack(numBar);
			for( ; t <= just_; t.Add(0, 0, 1), t.LoopBack(numBar) )
			{
				if( pred(t) )
				{
					return true;
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

		double beatSec = (60.0 / Tempo);
		samplesPerUnit_ = (int)(SamplingRate * (beatSec/UnitPerBeat));
		samplesPerBeat_ =(int)(SamplingRate * beatSec);
		samplesPerBar_ = (int)(SamplingRate * UnitPerBar * (beatSec/UnitPerBeat));
		musicalTimeUnit_ = (double)samplesPerUnit_ / (double)SamplingRate;

		Initialize();
	}

	void Initialize()
	{
		isJustChanged_ = false;
		isNearChanged_ = false;
		near_ = new Timing(0, 0, -1);
		just_ = new Timing(near_);
		oldNear_ = new Timing(near_);
		oldJust_ = new Timing(just_);
		timeSecFromJust_ = 0;
		isFormerHalf_ = true;
		numRepeat_ = 0;
		currentBlockIndex_ = 0;
		oldBlockIndex_ = 0;
		nextBlockIndex_ = 0;
		currentSample_ = 0;
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
		UnityEditor.EditorApplication.playmodeStateChanged = OnPlaymodeStateChanged;
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
		}
	}

#if UNITY_EDITOR
	void OnPlaymodeStateChanged()
	{
		if( Current_.musicSource_.Player != null )
		{
			if( UnityEditor.EditorApplication.isPaused )
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
		oldNear_.Copy(near_);
		oldJust_.Copy(just_);
		long numSamples;
		int tempOut;
		if( !playback_.GetNumPlayedSamples(out numSamples, out tempOut) )
		{
			numSamples = -1;
		}
		isNearChanged_ = false;
		isJustChanged_ = false;
		isJustLooped_ = false;
		isNearLooped_ = false;
		currentSample_ = (int)numSamples;
		if( currentSample_ >= 0 )
		{
			just_.Bar = (int)(currentSample_ / samplesPerBar_);
			just_.Beat = (int)((currentSample_ - just_.Bar * samplesPerBar_) / samplesPerBeat_);
			just_.Unit = (int)((currentSample_ - just_.Bar * samplesPerBar_ - just_.Beat * samplesPerBeat_) / samplesPerUnit_);
			just_.Fix();
			if( numBlockBar_ > 0 )
			{
				while( just_.Bar >= numBlockBar_ )
				{
					just_--;
				}
			}

			timeSecFromJust_ = (double)(currentSample_ - just_.Bar * samplesPerBar_ - just_.Beat * samplesPerBeat_ - just_.Unit * samplesPerUnit_) / (double)SamplingRate;
			isFormerHalf_ = (timeSecFromJust_ * SamplingRate) < samplesPerUnit_ / 2;

			near_.Copy(just_);
			if( !isFormerHalf_ )
			{
				near_++;
				near_.LoopBack(numBlockBar_);
			}

			isJustChanged_ = (just_.Equals(oldJust_) == false);
			isNearChanged_ = (near_.Equals(oldNear_) == false);

			if( isJustChanged_ )
			{
				foreach( CriAtomSource cue in quantizedCue_ )
				{
					cue.Play();
				}
				quantizedCue_.Clear();
			}

			isJustLooped_ = isJustChanged_ && just_ < oldJust_;
			isNearLooped_ = isNearChanged_ && near_ < oldNear_;
			deltaMusicalTime_ = just_.MusicalTime - oldJust_.MusicalTime;
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
					oldJust_.Subtract(0, 0, 1);
					deltaMusicalTime_ += BlockInfos[oldBlockIndex_].NumBar * UnitPerBar;
					deltaMusicalTime_ %= UnitPerBar;//小節合わせでブロック遷移する前提
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

	#endregion

	#region Events

	void OnBlockRepeated()
	{
		++numRepeat_;
	}

	void OnBlockChanged()
	{
		numRepeat_ = 0;
		//Debug.Log("Music::OnBlockChanged " + CurrentBlockName);
		if( OnNextBlockStarted != null )
		{
			OnNextBlockStarted(null, null);
			OnNextBlockStarted = null;
		}
	}

	#endregion
}

[Serializable]
public class Timing : IComparable<Timing>, IEquatable<Timing>
{
	public Timing(int bar = 0, int beat = 0, int unit = 0)
	{
		Bar = bar;
		Beat = beat;
		Unit = unit;
	}

	public Timing(Timing copy)
	{
		Copy(copy);
	}
	public Timing() { this.Init(); }
	public void Init() { Bar = 0; Beat = 0; Unit = 0; }
	public void Copy(Timing copy)
	{
		Bar = copy.Bar;
		Beat = copy.Beat;
		Unit = copy.Unit;
	}

	public int Bar, Beat, Unit;

	public int MusicalTime { get { return Bar * Music.CurrentUnitPerBar + Beat * Music.CurrentUnitPerBeat + Unit; } }
	public void Fix()
	{
		int totalUnit = Bar * Music.CurrentUnitPerBar + Beat * Music.CurrentUnitPerBeat + Unit;
		Bar = totalUnit / Music.CurrentUnitPerBar;
		Beat = (totalUnit - Bar*Music.CurrentUnitPerBar) / Music.CurrentUnitPerBeat;
		Unit = (totalUnit - Bar*Music.CurrentUnitPerBar - Beat * Music.CurrentUnitPerBeat);
	}
	public void Add(int bar, int beat = 0, int unit = 0)
	{
		Bar += bar;
		Beat += beat;
		Unit += unit;
		Fix();
	}
	public void Set(int bar, int beat = 0, int unit = 0)
	{
		Bar = bar;
		Beat = beat;
		Unit = unit;
	}
	public void Add(Timing t)
	{
		Bar += t.Bar;
		Beat += t.Beat;
		Unit += t.Unit;
		Fix();
	}
	public void Subtract(int bar, int beat = 0, int unit = 0)
	{
		Bar -= bar;
		Beat -= beat;
		Unit -= unit;
		Fix();
	}
	public void Subtract(Timing t)
	{
		Bar -= t.Bar;
		Beat -= t.Beat;
		Unit -= t.Unit;
		Fix();
	}
	public void LoopBack(int loopBar)
	{
		if( loopBar > 0 )
		{
			Bar += loopBar;
			Fix();
			Bar %= loopBar;
		}
	}

	public static bool operator >(Timing t, Timing t2) { return t.Bar > t2.Bar || (t.Bar == t2.Bar && t.Beat > t2.Beat) || (t.Bar == t2.Bar && t.Beat == t2.Beat && t.Unit > t2.Unit); }
	public static bool operator <(Timing t, Timing t2) { return !(t > t2) && !(t.Equals(t2)); }
	public static bool operator <=(Timing t, Timing t2) { return !(t > t2); }
	public static bool operator >=(Timing t, Timing t2) { return !(t < t2); }
	public static Timing operator ++(Timing t) { t.Unit++; t.Fix(); return t; }
	public static Timing operator --(Timing t) { t.Unit--; t.Fix(); return t; }

	public override bool Equals(object obj)
	{
		if( object.ReferenceEquals(obj, null) )
		{
			return false;
		}
		if( object.ReferenceEquals(obj, this) )
		{
			return true;
		}
		if( this.GetType() != obj.GetType() )
		{
			return false;
		}
		return this.Equals(obj as Timing);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public bool Equals(Timing other)
	{
		return (this.Bar == other.Bar && this.Beat == other.Beat && this.Unit == other.Unit);
	}

	public int CompareTo(Timing tother)
	{
		if( this.Equals(tother) ) return 0;
		else if( this > tother ) return 1;
		else return -1;
	}

	public override string ToString()
	{
		return Bar + " " + Beat + " " + Unit;
	}
}