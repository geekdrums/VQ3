//#define ADX

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ���y�̏����擾�o���܂��B
/// ���y�̃L���[��Component�Ɋ܂񂾃I�u�W�F�N�g�ɃA�^�b�`���Ă��������B
/// �������ɍĐ��J�n���A���̌㉹�y��̂ǂ̃^�C�~���O���A�ǂ̃u���b�N�ɂ��邩�A�Ȃǂ��擾�\�ł��B
/// �N�I���^�C�Y���čĐ��A�u���b�N�̐ݒ�A�Ȃǂ��\�ł��B
/// Known issues...
/// �����̋Ȃ̐؂�ւ��͂܂������ƃT�|�[�g���Ă܂���B
/// ���삪�d���Ȃ������͔�����ԉ\��������܂��B
/// �u���b�N�̃��[�v���ɖ���GetNumPlayedSamples��1,2�t���[���قǎ��s���邱�Ƃ��킩���Ă��܂��B
/// IMusicListener�͕K�v�ɉ����Ċg���\��ł��B
/// </summary>
public class Music : MonoBehaviour {
	
	public class SoundCue
	{
#if ADX
		public SoundCue( CriAtomSource source ) { this.source = source; }
		public CriAtomSource source { get; private set; }
		public CriAtomExPlayer Player { get { return source.Player; } }
#else
		public SoundCue( AudioSource source ) { this.source = source; }
		public AudioSource source { get; private set; }
#endif

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
#if ADX
			source.Stop();
#else
			source.Pause();
#endif
		}
		public bool IsPlaying()
		{
#if ADX
			return source.status == CriAtomSource.Status.Playing;
#else
			return source.isPlaying;
#endif
		}


	}

	/// <summary>
	/// Get a currently playing music.
	/// Be suer to play only one Music Cue at once.
	/// </summary>
	private static Music Current;
	private static List<IMusicListener> Listeners = new List<IMusicListener>();
	public interface IMusicListener
	{
		void OnMusicStarted();
		void OnBlockChanged();
	}

	//static properties
	public static int mtBar { get { return Current.mtBar_; } }
	public static int mtBeat { get { return Current.mtBeat_; } }
	public static double mtUnit { get { return Current.MusicTimeUnit; } }
	public static Timing Now { get { return Current.Now_; } }
	public static Timing Just { get { return Current.Just_; } }
	public static bool isJustChanged { get { return Current.isJustChanged_; } }
	public static bool isNowChanged { get { return Current.isNowChanged_; } }
	public static bool IsPlaying() { return Current.MusicSource.IsPlaying(); }
	public static void Pause() { Current.MusicSource.Pause(); }
	public static void Resume() { Current.MusicSource.Play(); }
	public static void Stop() { Current.MusicSource.Stop(); }
	/// <summary>
	/// ��ԋ߂�Just���玞�Ԃ��ǂꂾ������Ă��邩�𕄍��t���ŕԂ��B
	/// </summary>
	public static double lag
	{
		get
		{
			if ( Current.isFormerHalf_ )
				return Current.dtFromJust_;
			else
				return Current.dtFromJust_ - Current.MusicTimeUnit;
		}
	}
	/// <summary>
	/// ��ԋ߂�dmtUnit����ǂꂾ�����O�����邩���Βl�ŕԂ��B
	/// </summary>
	public static double lagAbs
	{
		get
		{
			if ( Current.isFormerHalf_ )
				return Current.dtFromJust_;
			else
				return Current.MusicTimeUnit - Current.dtFromJust_;
		}
	}
	/// <summary>
	/// lag��-1�`0�`1�̊ԂŕԂ��B
	/// </summary>
	public static double lagUnit { get { return lag / Current.MusicTimeUnit; } }

	//static predicates
	public static bool IsNowChangedWhen( System.Predicate<Timing> pred )
	{
		return Current.isNowChanged_ && pred( Current.Now_ );
	}
	public static bool IsNowChangedAt( int bar, int beat = 0, int unit = 0 )
	{
		return Current.isNowChanged_ &&
                Current.Now_.totalUnit == Current.mtBar_ * bar + Current.mtBeat_ * beat + unit;
	}
	public static bool IsJustChangedWhen( System.Predicate<Timing> pred )
	{
		return Current.isJustChanged_ && pred( Current.Just_ );
	}
	public static bool IsJustChangedAt( int bar = 0, int beat = 0, int unit = 0 )
	{
		return Current.isJustChanged_ &&
                Current.Just_.totalUnit == Current.mtBar_ * bar + Current.mtBeat_ * beat + unit;
	}

	//static funcs
	public static void QuantizePlay( SoundCue source ) { Current.QuantizedCue.Add( source ); }
	public static void AddListener( IMusicListener listener ) { Listeners.Add( listener ); }

#if ADX

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

	public static void SetNextBlock( string blockName )
	{
		int index = Current.BlockInfos.FindIndex( ( BlockInfo info ) => info.BlockName==blockName );
		if ( index >= 0 )
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
		if ( index < Current.CueInfo.numBlocks )
		{
			Current.NextBlockIndex = index;
			Current.playback.SetNextBlockIndex( index );
		}
		else
		{
			Debug.LogError( "Error!! Music.SetNextBlock index is out of range: " + index );
		}
	}
	public static int GetNextBlock() { return Current.NextBlockIndex; }
	public static string GetNextBlockName() { return Current.BlockInfos[Current.NextBlockIndex].BlockName; }
	public static int GetCurrentBlock() { return Current.CurrentBlockIndex; }
	public static string GetCurrentBlockName() { return Current.BlockInfos[Current.CurrentBlockIndex].BlockName; }

	public static void SetFirstBlock( int index )
	{
		if ( index < Current.CueInfo.numBlocks )
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
	public static void SetFirstBlock( string blockName )
	{
		int index = Current.BlockInfos.FindIndex( ( BlockInfo info ) => info.BlockName==blockName );
		if ( index >= 0 )
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
#endif

	//static readonlies
	private static readonly int SamplingRate = 44100;

	//music editor params
	/// <summary>
	/// �ꔏ��MusicTime�����ɋ�؂��Ă��邩�B4or3���Ǝv���B
	/// </summary>
	public int mtBeat_ = 4;
	/// <summary>
	/// �ꏬ�߂�MusicTime�������B
	/// </summary>
	public int mtBar_ = 16;
	/// <summary>
	/// �ʏ�̈Ӗ��ł̉��y�̃e���|�B
	/// ���m�ɂ́AmtBeat����MusicTime��������܂ł̎��Ԃ�1���ɂ������邩�B
	/// </summary>
	public double Tempo_ = 128;

	#region private params
	//music current params
	/// <summary>
	/// ��ԋ߂��^�C�~���O�ɍ��킹�Đ؂�ւ��B
	/// </summary>
	Timing Now_;
	/// <summary>
	/// �W���X�g�ɂȂ��Ă���؂�ւ��B
	/// </summary>
	Timing Just_;
	/// <summary>
	/// ���̃t���[����Timing���ω�������
	/// </summary>
	bool isJustChanged_;
	/// <summary>
	/// ���̃t���[����isFormerHalf���ω�������
	/// </summary>
	bool isNowChanged_;
	/// <summary>
	/// ���Ɣ��̊Ԃ̑O���������i�㔼�������j
	/// </summary>
	bool isFormerHalf_;
	/// <summary>
	/// mtUnit�ŏ��%����B�Ō�ɉ��y��̃^�C�~���O�����Ă���̎����ԁB
	/// </summary>
	double dtFromJust_;
	/// <summary>
	/// ���݂̃u���b�N�����s�[�g������
	/// </summary>
	int numRepeat;

	SoundCue MusicSource;
	List<SoundCue> QuantizedCue;
#if ADX
	CriAtomExPlayback playback;
	CriAtomExAcb ACBData;
	CriAtomEx.CueInfo CueInfo;
	
	/// <summary>
	/// ADX��ł̌��ݍĐ����̃u���b�N
	/// </summary>
	int CurrentBlockIndex;
	/// <summary>
	/// ADX��ł̎��ɍĐ�����\��̃u���b�N
	/// (ADX��ŏ���ɑJ�ڂ���ꍇ�͎擾�ł��Ȃ�)
	/// </summary>
	int NextBlockIndex;
	public List<BlockInfo> BlockInfos;
	int NumBlockBar { get { return BlockInfos[CurrentBlockIndex].NumBar; } }
	long SamplesInBlock { get { return BlockInfos[CurrentBlockIndex].NumBar * SamplesPerBar; } }
#else
    readonly int NumBlockBar = 0;
#endif

	//readonly params
	double MusicTimeUnit;
	long SamplesPerUnit;
	long SamplesPerBeat;
	long SamplesPerBar;

	//others
	/// <summary>
	/// Now�̒��O�̏��
	/// </summary>
	Timing Old, OldJust;
	int OldBlockIndex;
	#endregion

	#region Unity Interfaces
	void Awake()
	{
		Current = this;
#if ADX
		MusicSource = new SoundCue( GetComponent<CriAtomSource>() );
		ACBData = CriAtom.GetAcb( MusicSource.source.cueSheet );
		ACBData.GetCueInfo( MusicSource.source.cueName, out CueInfo );
#else
		MusicSource = new SoundCue( GetComponent<AudioSource>() );
#endif
		QuantizedCue = new List<SoundCue>();

		SamplesPerUnit = (long)( SamplingRate * ( 60.0 / ( Tempo_ * mtBeat_ ) ) );
		SamplesPerBeat = SamplesPerUnit*mtBeat_;
		SamplesPerBar = SamplesPerUnit*mtBar_;

		MusicTimeUnit = (double)SamplesPerUnit / (double)SamplingRate;

		Now_ = new Timing( 0, 0, -1 );
		Just_ = new Timing( Now_ );
		Old = new Timing( Now_ );
		OldJust = new Timing( Just_ );
	}

	// Use this for initialization
	void Start()
	{
		WillBlockChange();
#if ADX
		playback = MusicSource.source.Play();
#else
		MusicSource.Play();
#endif
		foreach ( IMusicListener listener in Listeners )
		{
			listener.OnMusicStarted();
		}
		OnBlockChanged();
	}
	
	// Update is called once per frame
	void Update () {
		long numSamples;
#if ADX
		CurrentBlockIndex = playback.GetCurrentBlockIndex();
		int tempOut;
		if ( !playback.GetNumPlayedSamples( out numSamples, out tempOut ) )
		{
			numSamples = -1;
		}
#else
		numSamples = MusicSource.source.timeSamples;
#endif
		if( numSamples >= 0 )
		{
			Just_.bar = (int)( numSamples / SamplesPerBar );
			if ( NumBlockBar != 0 ) Just_.bar %= NumBlockBar;
			Just_.beat = (int)( ( numSamples % SamplesPerBar ) / SamplesPerBeat );
			Just_.unit = (int)( ( numSamples % SamplesPerBeat ) / SamplesPerUnit );
			isFormerHalf_ = ( numSamples % SamplesPerUnit ) < SamplesPerUnit / 2;
			dtFromJust_ = (double)( numSamples % SamplesPerUnit ) / (double)SamplingRate;

			Now_.Copy( Just_ );
			if ( !isFormerHalf_ ) Now_.Increment();
#if ADX
			if ( numSamples + SamplesPerUnit/2 >= SamplesInBlock )
			{
				Now_.Init();
			}
#endif

			isNowChanged_ = Now_.totalUnit != Old.totalUnit;
			isJustChanged_ = Just_.totalUnit != OldJust.totalUnit;

			CallEvents();

			Old.Copy( Now_ );
			OldJust.Copy( Just_ );
		}
		else
		{
			Debug.LogWarning( "Warning!! Failed to GetNumPlayedSamples" );
		}
	}

	void CallEvents()
	{
		if ( isNowChanged_ ) OnNowChanged();
		if ( isNowChanged_ && Old > Now_ )
		{
#if ADX
			if ( NextBlockIndex == CurrentBlockIndex )
			{
#endif
				WillBlockRepeat();
#if ADX
			}
			else
			{
				WillBlockChange();
			}
#endif
        }
		if ( isJustChanged_ ) OnJustChanged();
		if ( isJustChanged_ && Just_.unit == 0 ) OnBeat();
		if ( isJustChanged_ && Just_.barUnit == 0 ) OnBar();
		if ( isJustChanged_ && OldJust > Just_ )
		{
#if ADX
			if ( OldBlockIndex == CurrentBlockIndex )
			{
#endif
				OnBlockRepeated();
#if ADX
			}
			else
			{
				OnBlockChanged();
			}
            OldBlockIndex = CurrentBlockIndex;
#endif
        }

		if ( isJustChanged_ && Just_.totalUnit > 0 )
		{
			Timing tempOld = new Timing( OldJust );
			tempOld.Increment();
			if ( tempOld.totalUnit != Just_.totalUnit )
			{
				Debug.LogWarning( "Warning!! OldJust = " + OldJust.ToString() + ", Just = " + Just_.ToString() );
			}
		}
	}
	#endregion

	//On events (when isJustChanged)
	void OnNowChanged()
	{
		//foreach ( CriAtomSource cue in QuantizedCue )
		//{
		//    cue.SetAisac( 2, (float)(MusicTimeUnit - dtFromJust_) );
		//    cue.Play();
		//}
		//QuantizedCue.Clear();
	}

	void OnJustChanged()
	{
		foreach ( SoundCue cue in QuantizedCue )
		{
			cue.Play();
		}
		QuantizedCue.Clear();
		//Debug.Log( "OnJust " + Just.ToString() );
	}

	void OnBeat()
	{
		//Debug.Log( "OnBeat " + Just.ToString() );
	}

	void OnBar()
	{
		//Debug.Log( "OnBar " + Just.ToString() );
	}

	void OnBlockRepeated()
	{
		++numRepeat;
		//Debug.Log( "NumRepeat = " + numRepeat );
	}

	void OnBlockChanged()
	{
		numRepeat = 0;
		foreach ( IMusicListener listener in Listeners )
		{
			listener.OnBlockChanged();
		}
	}

	//Will events (when isNowChanged)
	void WillBlockRepeat()
	{
	}

	void WillBlockChange()
	{
	}
}
