using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// You can get musical timing from Music.SomethingYouWant.
/// Attach this as a component to a GameObject that include Music.
/// Comment out "#define ADX" line when you don't use ADX2LE.
/// </summary>
public class Music : MonoBehaviour
{
	public bool UseADX_;
	GUIText debugText;
	void Awake()
	{
		UseADX = UseADX_;
		Debug.Log( "UseADX_ = " + UseADX_.ToString() );
		debugText = GetComponentInChildren<GUIText>();
	}
	void Update()
	{
		if ( debugText!= null )
		{
			debugText.text = Just.ToString();
		}
	}

	public static bool UseADX = true;

	//static properties
	/// <summary>
	/// means last timing.
	/// </summary>
	public static Timing Just { get { return UseADX ? ADXMusic.Just : UnityMusic.Just; } }
	/// <summary>
	/// means nearest timing.
	/// </summary>
	public static Timing Now { get { return UseADX ? ADXMusic.Now : UnityMusic.Now; } }
	/// <summary>
	/// is Just changed in this frame or not.
	/// </summary>
	public static bool isJustChanged { get { return UseADX ? ADXMusic.isJustChanged : UnityMusic.isJustChanged; } }
	/// <summary>
	/// is Now changed in this frame or not.
	/// </summary>
	public static bool isNowChanged { get { return UseADX ? ADXMusic.isNowChanged : UnityMusic.isNowChanged; } }
	/// <summary>
	/// is currently former half in a MusicalTime, or last half.
	/// </summary>
	public static bool isFormerHalf { get { return UseADX ? ADXMusic.isFormerHalf : UnityMusic.isFormerHalf; } }
	/// <summary>
	/// delta time from JustChanged.
	/// </summary>
	public static double dtFromJust { get { return UseADX ? ADXMusic.dtFromJust : UnityMusic.dtFromJust; } }
	/// <summary>
	/// how many times you repeat current music/block.
	/// </summary>
	public static int numRepeat { get { return UseADX ? ADXMusic.numRepeat : UnityMusic.numRepeat; } }
	/// <summary>
	/// returns how long from nearest Just timing with sign.
	/// </summary>
	public static double lag { get { return UseADX ? ADXMusic.lag : UnityMusic.lag; } }
	
	/// <summary>
	/// returns how long from nearest Just timing absolutely.
	/// </summary>
	public static double lagAbs { get { return UseADX ? ADXMusic.lagAbs : UnityMusic.lagAbs; } }
	/// <summary>
	/// returns normalized lag.
	/// </summary>
	public static double lagUnit { get { return UseADX ? ADXMusic.lagUnit : UnityMusic.lagUnit; } }
	/// <summary>
	/// returns time based in beat.
	/// </summary>
	public static double MusicalTime { get { return UseADX ? ADXMusic.MusicalTime : UnityMusic.MusicalTime; } }
	public static int mtBar { get { return UseADX ? ADXMusic.mtBar : UnityMusic.mtBar; } }
	public static int mtBeat { get { return UseADX ? ADXMusic.mtBeat : UnityMusic.mtBeat; } }
	public static double mtUnit { get { return UseADX ? ADXMusic.mtUnit : UnityMusic.mtUnit; } }
	public static string CurrentMusicName { get { return UseADX ? ADXMusic.CurrentMusicName : UnityMusic.CurrentMusicName; } }

	//static predicates
	public static bool IsNowChangedWhen( System.Predicate<Timing> pred )
	{
		return UseADX ? ADXMusic.IsNowChangedWhen( pred ) : UnityMusic.IsNowChangedWhen( pred );
	}
	public static bool IsNowChangedBar()
	{
		return UseADX ? ADXMusic.IsNowChangedBar() : UnityMusic.IsNowChangedBar();
	}
	public static bool IsNowChangedBeat()
	{
		return UseADX ? ADXMusic.IsNowChangedBeat() : UnityMusic.IsNowChangedBeat();
	}
	public static bool IsNowChangedAt( int bar, int beat = 0, int unit = 0 )
	{
		return UseADX ? ADXMusic.IsNowChangedAt( bar, beat, unit ) : UnityMusic.IsNowChangedAt( bar, beat, unit );
	}
    public static bool IsNowChangedAt( Timing timing )
    {
        return UseADX ? ADXMusic.IsNowChangedAt( timing.bar, timing.beat, timing.unit ) : UnityMusic.IsNowChangedAt( timing.bar, timing.beat, timing.unit );
    }
    public static bool IsJustChangedWhen( System.Predicate<Timing> pred )
	{
		return UseADX ? ADXMusic.IsJustChangedWhen( pred ) : UnityMusic.IsJustChangedWhen( pred );
	}
	public static bool IsJustChangedBar()
	{
		return UseADX ? ADXMusic.IsJustChangedBar() : UnityMusic.IsJustChangedBar();
	}
	public static bool IsJustChangedBeat()
	{
		return UseADX ? ADXMusic.IsJustChangedBeat() : UnityMusic.IsJustChangedBeat();
	}
	public static bool IsJustChangedAt( int bar = 0, int beat = 0, int unit = 0 )
	{
		return UseADX ? ADXMusic.IsJustChangedAt( bar, beat, unit ) : UnityMusic.IsJustChangedAt( bar, beat, unit );
	}
    public static bool IsJustChangedAt( Timing timing )
    {
        return UseADX ? ADXMusic.IsJustChangedAt( timing.bar, timing.beat, timing.unit ) : UnityMusic.IsJustChangedAt( timing.bar, timing.beat, timing.unit );
    }

	//static functions
	/// <summary>
	/// Change Current Music.
	/// </summary>
	/// <param name="MusicName">name of the GameObject that include Music</param>
	public static void Play( string MusicName, string firstBlockName = "" )
	{
		if ( UseADX )
		{
			ADXMusic.Play( MusicName, firstBlockName );
		}
		else
		{
			UnityMusic.Play( ( firstBlockName != "" ? firstBlockName : MusicName ) );
		}
	}
	public static bool IsPlaying()
	{
		return UseADX ? ADXMusic.IsPlaying() : UnityMusic.IsPlaying();
	}
	public static void Pause()
	{
		if ( UseADX )
		{
			ADXMusic.Pause();
		}
		else
		{
			UnityMusic.Pause();
		}
	}
	public static void Resume()
	{
		if ( UseADX )
		{
			ADXMusic.Resume();
		}
		else
		{
			UnityMusic.Resume();
		}
	}
	public static void Stop()
	{
		if ( UseADX )
		{
			ADXMusic.Stop();
		}
		else
		{
			UnityMusic.Stop();
		}
	}


	public static void SetNextBlock( string blockName )
	{
		if ( UseADX )
		{
			ADXMusic.SetNextBlock( blockName );
		}
		else
		{
			UnityMusic.SetNextBlock( blockName );
		}
	}
	public static void SetNextBlock( int index )
	{
		if ( UseADX )
		{
			ADXMusic.SetNextBlock( index );
		}
		else
		{
			Debug.LogError( "Error!! You can't use SetNextBlock(index) in UnityMusic" );
		}
	}
	public static int GetNextBlock() { return UseADX ? ADXMusic.GetNextBlock() : 0; }
	public static string GetNextBlockName() { return UseADX ? ADXMusic.GetNextBlockName() : UnityMusic.GetNextBlockName(); }
	public static int GetCurrentBlock() { return UseADX ? ADXMusic.GetCurrentBlock() : 0; }
	public static string GetCurrentBlockName() { return UseADX ? ADXMusic.GetCurrentBlockName() : UnityMusic.GetCurrentBlockName(); }

	public static void SetFirstBlock( int index )
	{
		if ( UseADX )
		{
			ADXMusic.SetFirstBlock( index );
		}
		else
		{
			Debug.LogError( "Error!! You can't use SetFirstBlock(index) in UnityMusic" );
		}
	}
	public static void SetFirstBlock( string blockName )
	{
		if ( UseADX )
		{
			ADXMusic.SetFirstBlock( blockName );
		}
		else
		{
			Debug.LogError( "Error!! You can't use SetFirstBlock(blockName) in UnityMusic" );
		}
	}
	public static void SetAisac( uint index, float value )
	{
		if ( UseADX )
		{
			ADXMusic.CurrentSource.source.SetAisac( index, value );
		}
		else
		{
			//Debug.LogError( "Error!! You can't use SetAisac(index,value) in UnityMusic" );
		}
	}
    public static void SetAisac( string controlName, float value )
    {
        if( UseADX )
        {
            ADXMusic.CurrentSource.source.SetAisac( controlName, value );
        }
        else
        {
            //Debug.LogError( "Error!! You can't use SetAisac(index,value) in UnityMusic" );
        }
    }
}
