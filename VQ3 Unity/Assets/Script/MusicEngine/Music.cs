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

/* Note2: You should do this
Auto update of block info.

Add this function in Editor/CriWare/CriAtom/CriAtomWindow.cs

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
                                adxMusic.Tempo_ = double.Parse( reader.GetAttribute( "Bpm" ) );
                            }
                            if( adxMusic.Tempo_ > 0 && reader.GetAttribute( "BlockEndPositionMs" ) != null )
                            {
                                string blockName = reader.GetAttribute( "OrcaName" );
                                int msec = int.Parse( reader.GetAttribute( "BlockEndPositionMs" ) );
                                Music.BlockInfo blockInfo = new Music.BlockInfo( blockName, Mathf.RoundToInt( (msec / 1000.0f) / (4 * 60.0f / (float)adxMusic.Tempo_) ) );
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
- specify mtBeat, mtBar and Tempo,
- name it with same name of CueName,
- then open CRI Atom window and push "Update Assets of CRI Atom Craft" button,
- your BlockInfo will be automatically updated.

*/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Music : MonoBehaviour
{
    //editor params
    /// <summary>
    /// how many MusicTime in a beat. maybe 4 or 3 or 6.
    /// </summary>
    public int mtBeat_ = 4;
    /// <summary>
    /// how many MusicTime in a bar.
    /// </summary>
    public int mtBar_ = 16;
    /// <summary>
    /// Musical Tempo. how many beats in a minutes.
    /// </summary>
    public double Tempo_ = 120;

    public List<BlockInfo> BlockInfos;

    /// <summary>
    /// put your debug GUIText to see current musical time & block info.
    /// </summary>
    public GUIText debugText;

    [System.Serializable]
    public class BlockInfo
    {
        public BlockInfo( string BlockName, int NumBar = 4 )
        {
            this.BlockName = BlockName;
            this.NumBar = NumBar;
        }
        public string BlockName;
        public int NumBar = 4;
    }

    public class SoundCue
    {
        public SoundCue( CriAtomSource source ) { this.source = source; }
        public CriAtomSource source { get; private set; }
        public CriAtomExPlayer Player { get { return source.Player; } }

        public void Play()
        {
            source.Play();
        }
        public void Stop()
        {
            source.Stop();
        }
        public void Pause()
        {
            source.Stop();
        }
        public bool IsPlaying()
        {
            return source.status == CriAtomSource.Status.Playing;
        }
        public void SetVolume( float volume )
        {
            source.volume = volume;
        }
    }

    static Music Current;
    static List<Music> MusicList = new List<Music>();
    static readonly int SamplingRate = 44100;

    #region public static properties
	/// <summary>
	/// means last timing.
	/// </summary>
	public static Timing Just { get { return Current.Just_; } }
	/// <summary>
	/// means nearest timing.
	/// </summary>
	public static Timing Now { get { return Current.Now_; } }
	/// <summary>
	/// is Just changed in this frame or not.
	/// </summary>
	public static bool isJustChanged { get { return Current.isJustChanged_; } }
	/// <summary>
	/// is Now changed in this frame or not.
	/// </summary>
	public static bool isNowChanged { get { return Current.isNowChanged_; } }
	/// <summary>
    /// is currently former half in a MusicTimeUnit, or last half.
	/// </summary>
	public static bool isFormerHalf { get { return Current.isFormerHalf_; } }
	/// <summary>
	/// delta time from JustChanged.
	/// </summary>
	public static double dtFromJust { get { return Current.dtFromJust_; } }
	/// <summary>
	/// how many times you repeat current music/block.
	/// </summary>
	public static int numRepeat { get { return Current.numRepeat_; } }
	/// <summary>
	/// returns how long from nearest Just timing with sign.
	/// </summary>
	public static double lag{ get{ return Current.lag_; } }
	/// <summary>
	/// returns how long from nearest Just timing absolutely.
	/// </summary>
	public static double lagAbs{ get{ return Current.lagAbs_; } }
	/// <summary>
	/// returns normalized lag.
	/// </summary>
	public static double lagUnit{ get{ return Current.lagUnit_; } }

    public static double MusicalTime { get { return Current.MusicalTime_; } }       //sec per MusicTimeUnit
    public static double MusicTimeUnit { get { return Current.MusicTimeUnit_; } }   //sec
    public static int mtBar { get { return Current.mtBar_; } }
	public static int mtBeat { get { return Current.mtBeat_; } }

    public static string CurrentMusicName { get { return Current.name; } }
    public static SoundCue CurrentSource { get { return Current.MusicSource; } }

    public static string CurrentBlockName { get { return Current.CurrentBlock_.BlockName; } }
    public static string NextBlockName { get { return Current.NextBlock_.BlockName; } }
    public static BlockInfo CurrentBlock { get { return Current.CurrentBlock_; } }
    public static BlockInfo NextBlock { get { return Current.NextBlock_; } }
    #endregion

    #region public static predicates
    public static bool IsJustChangedWhen( Predicate<Timing> pred )
    {
        return Current.IsJustChangedWhen_( pred );
    }
    public static bool IsJustChangedBar()
    {
        return Current.IsJustChangedBar_();
    }
    public static bool IsJustChangedBeat()
    {
        return Current.IsJustChangedBeat_();
    }
    public static bool IsJustChangedAt( int bar = 0, int beat = 0, int unit = 0 )
    {
        return Current.IsJustChangedAt_( bar, beat, unit );
    }
    public static bool IsJustChangedAt( Timing t )
    {
        return Current.IsJustChangedAt_( t.bar, t.beat, t.unit );
    }

    public static bool IsNowChangedWhen( Predicate<Timing> pred )
    {
        return Current.IsNowChangedWhen_( pred );
    }
    public static bool IsNowChangedBar()
    {
        return Current.IsNowChangedBar_();
    }
    public static bool IsNowChangedBeat()
    {
        return Current.IsNowChangedBeat_();
    }
    public static bool IsNowChangedAt( int bar, int beat = 0, int unit = 0 )
    {
        return Current.IsNowChangedAt_( bar, beat, unit );
    }
    public static bool IsNowChangedAt( Timing t )
    {
        return Current.IsNowChangedAt_( t.bar, t.beat, t.unit );
    }

    public static float MusicalTimeFrom( Timing timing )
    {
        return (Now - timing) + (float)lagUnit;
    }
    #endregion

    #region public static functions
    /// <summary>
	/// Change Current Music.
	/// </summary>
	/// <param name="MusicName">name of the GameObject that include Music</param>
	public static void Play( string MusicName, string firstBlockName = "" ) { MusicList.Find( ( Music m ) => m.name == MusicName ).PlayStart( firstBlockName ); }
	/// <summary>
	/// Quantize to musical time( default is 16 beat ).
	/// </summary>
	/// <param name="source">your sound source( Unity AudioSource or ADX CriAtomSource )</param>
    public static void QuantizePlay( CriAtomSource source ) { Current.QuantizedCue.Add( source ); }
    public static bool IsPlaying() { return Current != null && Current.MusicSource.IsPlaying(); }
	public static void Pause() { Current.MusicSource.Pause(); }
	public static void Resume() { Current.MusicSource.Play(); }
	public static void Stop() { Current.MusicSource.Stop(); }

    public static void SetNextBlock( string blockName )
    {
        if( blockName == CurrentBlockName ) return;
        Debug.Log( "SetNextBlock : " + blockName );
        int index = Current.BlockInfos.FindIndex( ( BlockInfo info ) => info.BlockName == blockName );
        if( index >= 0 )
        {
            Current.NextBlockIndex = index;
            Current.playback.SetNextBlockIndex( index );
        }
        else
        {
            Debug.LogError( "Error!! Music.SetNextBlock Can't find block name: " + blockName );
        }
    }
    public static void SetNextBlock( int index )
    {
        if( index == Current.CurrentBlockIndex ) return;
        if( index < Current.CueInfo.numBlocks )
        {
            Current.NextBlockIndex = index;
            Current.playback.SetNextBlockIndex( index );
        }
        else
        {
            Debug.LogError( "Error!! Music.SetNextBlock index is out of range: " + index );
        }
    }
    
    public static void SetFirstBlock( string blockName )
    {
        int index = Current.BlockInfos.FindIndex( ( BlockInfo info ) => info.BlockName == blockName );
        if( index >= 0 )
        {
            Current.NextBlockIndex = index;
            Current.CurrentBlockIndex = index;
            Current.MusicSource.Player.SetFirstBlockIndex( index );
        }
        else
        {
            Debug.LogError( "Error!! Music.SetFirstBlock Can't find block name: " + blockName );
        }
    }
    public static void SetFirstBlock( int index )
    {
        if( index < Current.CueInfo.numBlocks )
        {
            Current.NextBlockIndex = index;
            Current.CurrentBlockIndex = index;
            Current.MusicSource.Player.SetFirstBlockIndex( index );
        }
        else
        {
            Debug.LogError( "Error!! Music.SetFirstBlock index is out of range: " + index );
        }
    }

    public static void SetAisac( uint index, float value )
    {
        Current.MusicSource.source.SetAisac( index, value );
    }
    public static void SetAisac( string controlName, float value )
    {
        Current.MusicSource.source.SetAisac( controlName, value );
    }
    #endregion

    #region private properties
    private Timing Now_;
    private Timing Just_;
    private bool isJustChanged_;
    private bool isNowChanged_;
    private bool isFormerHalf_;
    private double dtFromJust_;
    private int numRepeat_;
    private double MusicTimeUnit_;

    private double lag_
    {
        get
        {
            if( isFormerHalf_ )
                return dtFromJust_;
            else
                return dtFromJust_ - MusicTimeUnit_;
        }
    }
    private double lagAbs_
    {
        get
        {
            if( isFormerHalf_ )
                return dtFromJust_;
            else
                return MusicTimeUnit_ - dtFromJust_;
        }
    }
    private double lagUnit_ { get { return lag / MusicTimeUnit_; } }
    private double MusicalTime_ { get { return Just.totalUnit + dtFromJust / MusicTimeUnit; } }
    private bool isPlaying { get { return MusicSource != null && MusicSource.IsPlaying(); } }
    #endregion

    #region private predicates
    private bool IsNowChangedWhen_( Predicate<Timing> pred )
    {
        return isNowChanged_ && pred( Now_ );
    }
    private bool IsNowChangedBar_()
    {
        return isNowChanged_ && Now_.barUnit == 0;
    }
    private bool IsNowChangedBeat_()
    {
        return isNowChanged_ && Now_.unit == 0;
    }
    private bool IsNowChangedAt_( int bar, int beat = 0, int unit = 0 )
    {
        return isNowChanged_ &&
            Now_.bar == bar && Now_.beat == beat && Now_.unit == unit;
    }
    private bool IsJustChangedWhen_( Predicate<Timing> pred )
    {
        return isJustChanged_ && pred( Just_ );
    }
    private bool IsJustChangedBar_()
    {
        return isJustChanged_ && Just_.barUnit == 0;
    }
    private bool IsJustChangedBeat_()
    {
        return isJustChanged_ && Just_.unit == 0;
    }
    private bool IsJustChangedAt_( int bar = 0, int beat = 0, int unit = 0 )
    {
        return isJustChanged_ &&
            Just_.bar == bar && Just_.beat == beat && Just_.unit == unit;
    }
    #endregion

	#region private params
	//music current params
    SoundCue MusicSource;
    CriAtomExPlayback playback;
    CriAtomExAcb ACBData;
    CriAtomEx.CueInfo CueInfo;
    List<CriAtomSource> QuantizedCue = new List<CriAtomSource>();
    int CurrentBlockIndex;
    /// <summary>
    /// you can't catch NextBlockIndex completely, if ADX automatically change next block.
    /// </summary>
    int NextBlockIndex;
    BlockInfo CurrentBlock_ { get { return BlockInfos[CurrentBlockIndex]; } }
    BlockInfo NextBlock_ { get { return BlockInfos[NextBlockIndex]; } }

	//readonly params
	long SamplesPerUnit;
	long SamplesPerBeat;
	long SamplesPerBar;

	//others
	Timing OldNow, OldJust;
    int OldBlockIndex;
    long OldNumSamples;
	int NumBlockBar;
    long SamplesInLoop { get { return NumBlockBar * SamplesPerBar; } }
	#endregion

	#region Initialize & Update
	void Awake()
    {
        MusicList.Add( this );
        MusicSource = new SoundCue( GetComponent<CriAtomSource>() );
        if( Current == null || MusicSource.source.playOnStart )
        {
            Current = this;
        }
		ACBData = CriAtom.GetAcb( MusicSource.source.cueSheet );
		ACBData.GetCueInfo( MusicSource.source.cueName, out CueInfo );

		SamplesPerUnit = (long)( SamplingRate * ( 60.0 / ( Tempo_ * mtBeat_ ) ) );
		SamplesPerBeat = SamplesPerUnit*mtBeat_;
		SamplesPerBar = SamplesPerUnit*mtBar_;

		MusicTimeUnit_ = (double)SamplesPerUnit / (double)SamplingRate;

		Initialize();
	}

	void Initialize()
	{
		if ( Current != null && isJustChanged && Just.totalUnit == 0 )
		{
			Now_ = new Timing( 0, 0, 0 );
			isJustChanged_ = true;
		}
		else
		{
			Now_ = new Timing( 0, 0, -1 );
			isJustChanged_ = false;
		}
		Just_ = new Timing( Now_ );
		OldNow = new Timing( Now_ );
		OldJust = new Timing( Just_ );
		dtFromJust_ = 0;
		isFormerHalf_ = true;
		numRepeat_ = 0;
        CurrentBlockIndex = 0;
        OldBlockIndex = 0;
        NextBlockIndex = 0;
        OldNumSamples = 0;
	}

	void PlayStart( string firstBlockName = "" )
	{
		if ( Current != null && IsPlaying() )
		{
			Stop();
		}

		Initialize();
		Current = this;

		if ( firstBlockName != "" )
		{
			SetFirstBlock( firstBlockName );
		}
		playback = MusicSource.source.Play();
        NumBlockBar = BlockInfos[CurrentBlockIndex].NumBar;
	}

	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
    void Update()
    {
        if( isPlaying )
        {
            UpdateTiming();
        }
    }

	void UpdateTiming()
	{
		long numSamples;
		isNowChanged_ = false;
		isJustChanged_ = false;
        if( playback.GetStatus() != CriAtomExPlayback.Status.Playing ) return;
		int tempOut;
		if ( !playback.GetNumPlayedSamples( out numSamples, out tempOut ) )
		{
			numSamples = -1;
		}
		if ( numSamples >= 0 )
		{
			Just_.bar = (int)( numSamples / SamplesPerBar );
            UpdateNumBlockBar( numSamples );
            Just_.beat = (int)((numSamples % SamplesPerBar) / SamplesPerBeat);
            Just_.unit = (int)(((numSamples % SamplesPerBar) % SamplesPerBeat) / SamplesPerUnit);
            if( Just_.unit >= mtBeat_ )
            {
                Just_.beat += (int)(Just_.unit / mtBeat_);
                Just_.unit %= mtBeat_;
            }
            int barUnit = Just_.beat * mtBeat_ + Just_.unit;
            if( barUnit >= mtBar_ )
            {
                Just_.bar += (int)(barUnit / mtBar_);
                Just_.beat = 0;
                Just_.unit = (barUnit % mtBar_);
                if( Just_.unit >= mtBeat_ )
                {
                    Just_.beat += (int)(Just_.unit / mtBeat_);
                    Just_.unit %= mtBeat_;
                }
            }
			
			if( NumBlockBar != 0 ) Just_.bar %= NumBlockBar;

			isFormerHalf_ = ( numSamples % SamplesPerUnit ) < SamplesPerUnit / 2;
			dtFromJust_ = (double)( numSamples % SamplesPerUnit ) / (double)SamplingRate;

			Now_.Copy( Just_ );
			if ( !isFormerHalf_ ) Now_.Increment();
			if ( SamplesInLoop != 0 && numSamples + SamplesPerUnit/2 >= SamplesInLoop )
			{
				Now_.Init();
			}

			isNowChanged_ = Now_.totalUnit != OldNow.totalUnit;
			isJustChanged_ = Just_.totalUnit != OldJust.totalUnit;

			CallEvents();

			OldNow.Copy( Now_ );
			OldJust.Copy( Just_ );
		}
		else
		{
			//Debug.LogWarning( "Warning!! Failed to GetNumPlayedSamples" );
        }

        DebugUpdateText();
	}

	void UpdateNumBlockBar( long numSamples )
	{
		//BlockChanged
		if ( OldNumSamples > numSamples )
		{
			NumBlockBar = BlockInfos[playback.GetCurrentBlockIndex()].NumBar;
        }
		//BlockChanged during this block
		else if ( playback.GetCurrentBlockIndex() != CurrentBlockIndex && Just_.bar < BlockInfos[CurrentBlockIndex].NumBar - 1 )
		{
			NumBlockBar = Just_.bar + 1;
		}
        OldNumSamples = numSamples;
	}

    void DebugUpdateText()
    {
        if( debugText != null )
        {
            debugText.text = "Just = " + Just_.ToString() + ", MusicalTime = " + MusicalTime_;
            if( BlockInfos.Count > 0 )
            {
                debugText.text += System.Environment.NewLine + "block[" + CurrentBlockIndex + "] = " + CurrentBlock_.BlockName + "(" + NumBlockBar + "bar)";
            }
        }
    }
	#endregion

    #region Events
    void CallEvents()
    {
        if( isNowChanged_ ) OnNowChanged();
        if( isNowChanged_ && OldNow > Now_ )
        {
            if( NextBlockIndex == CurrentBlockIndex )
            {
                WillBlockRepeat();
            }
            else
            {
                WillBlockChange();
            }
        }
        if( isJustChanged_ ) OnJustChanged();
        if( isJustChanged_ && Just_.unit == 0 ) OnBeat();
        if( isJustChanged_ && Just_.barUnit == 0 ) OnBar();
        if( isJustChanged_ && OldJust > Just_ )
        {
            CurrentBlockIndex = playback.GetCurrentBlockIndex();
            if( OldBlockIndex == CurrentBlockIndex )
            {
                OnBlockRepeated();
            }
            else
            {
                OnBlockChanged();
            }
            OldBlockIndex = CurrentBlockIndex;
        }

        /*
		if ( isJustChanged_ && Just_.totalUnit > 0 )
		{
			Timing tempOld = new Timing( OldJust );
			tempOld.Increment();
			if ( tempOld.totalUnit != Just_.totalUnit )
			{
				//This often happens when the frame rate is slow.
				Debug.LogWarning( "Skipped some timing: OldJust = " + OldJust.ToString() + ", Just = " + Just_.ToString() );
			}
		}
        */
    }

	//On events (when isJustChanged)
	void OnNowChanged()
	{
	}

	void OnJustChanged()
	{
		foreach ( CriAtomSource cue in QuantizedCue )
		{
			cue.Play();
		}
		QuantizedCue.Clear();
	}

	void OnBeat()
	{
	}

	void OnBar()
	{
	}

    void OnBlockRepeated()
	{
		++numRepeat_;
	}

	void OnBlockChanged()
	{
		numRepeat_ = 0;
	}

	//Will events (when isNowChanged)
    void WillBlockRepeat()
	{
	}

	void WillBlockChange()
	{
	}
	#endregion
}

[Serializable]
public class Timing : IComparable<Timing>, IEquatable<Timing>
{
	public Timing( int b = 0, int be = 0, int u = 0 )
	{
		bar = b;
		beat = be;
		unit = u;
	}

	public Timing( Timing copy )
	{
		Copy( copy );
	}
	public Timing() { this.Init(); }
	public void Init() { bar = 0; beat = 0; unit = 0; }
	public void Copy( Timing copy )
	{
		bar = copy.bar;
		beat = copy.beat;
		unit = copy.unit;
	}

	public int bar, beat, unit;

	public int totalUnit { get { return Music.mtBar * bar + Music.mtBeat * beat + unit; } }
	public int totalBeat { get { return totalUnit/Music.mtBeat; } }
	public int barUnit { get { return ( unit < 0 ? Music.mtBar - unit : Music.mtBeat * beat + unit )%Music.mtBar; } }
	public void Increment()
	{
		unit++;
		if ( Music.mtBeat * beat + unit >= Music.mtBar )
		{
			unit = 0;
			beat = 0;
			bar += 1;
		}
		else if ( unit >= Music.mtBeat )
		{
			unit = 0;
			beat += 1;
		}
	}

	public void IncrementBeat()
	{
		beat++;
		if ( Music.mtBeat * beat + unit >= Music.mtBar )
		{
			beat = 0;
			bar += 1;
		}
	}

	public static bool operator >( Timing t, Timing t2 ) { return t.totalUnit > t2.totalUnit; }
	public static bool operator <( Timing t, Timing t2 ) { return !( t > t2 ) && !( t.Equals( t2 ) ); }
	public static bool operator <=( Timing t, Timing t2 ) { return !( t > t2 ); }
	public static bool operator >=( Timing t, Timing t2 ) { return t > t2 || t.Equals( t2 ); }
	public static int operator -( Timing t, Timing t2 ) { return t.totalUnit - t2.totalUnit; }

	public bool IsEqualTo( int bar, int beat = 0, int unit = 0 )
	{
		return this.totalUnit == Music.mtBar * bar + Music.mtBeat * beat + unit;
	}

	public override bool Equals( object obj )
	{
		if ( object.ReferenceEquals( obj, null ) )
		{
			return false;
		}
		if ( object.ReferenceEquals( obj, this ) )
		{
			return true;
		}
		if ( this.GetType() != obj.GetType() )
		{
			return false;
		}
		return this.Equals( obj as Timing );
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public bool Equals( Timing other )
	{
		return this.totalUnit == other.totalUnit;
	}

	public int CompareTo( Timing tother )
	{
		if ( this.Equals( tother ) ) return 0;
		else if ( this > tother ) return 1;
		else return -1;
	}

	public override string ToString()
	{
		return bar + " " + beat + " " + unit;
	}
}
