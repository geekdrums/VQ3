/****************************************************************************
 *
 * CRI Middleware SDK
 *
 * Copyright (c) 2011-2012 CRI Middleware Co.,Ltd.
 *
 * Library  : CRI Atom
 * Module   : CRI Atom Native Wrapper
 * File     : CriAtomEx.cs
 *
 ****************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

public class CriStructMemory <Type> : IDisposable
{
	public byte[] bytes {
		get; private set;
	}
	public IntPtr ptr {
		get { return gch.AddrOfPinnedObject(); }
	}
	GCHandle gch;

	public CriStructMemory()
	{
		this.bytes = new byte[Marshal.SizeOf(typeof(Type))];
		this.gch = GCHandle.Alloc(this.bytes, GCHandleType.Pinned);
	}

	public void Dispose()
	{
		this.gch.Free();
	}
}

/*==========================================================================
 *      CRI Atom Native Wrapper
 *=========================================================================*/
/**
 * \addtogroup CRIATOM_NATIVE_WRAPPER
 * @{
 */

/**
 * <summary>Atomライブラリのグローバルクラスです。</summary>
 * \par 説明:
 * Atomライブラリに対する設定関数や、Atomライブラリ内で共有する変数型を含むクラスです。<br/>
 */
public static class CriAtomEx
{
	public enum SoundRendererType {
		Default = 0,
		Native = 1,
		Asr = 2,
		Hw1 = 1,
		Hw2 = 9,
	}
	
	public enum VoiceAllocationMethod {
		Once,						/**< ボイスの確保は1回限り		*/
		Retry,						/**< ボイスを繰り返し確保する	*/
	}
	
	/**
	 * <summary>バイクアッドフィルタのタイプ</summary>
	 * \par 説明:
	 * バイクアッドフィルタのタイプを指定するためのデータ型です。<br/>
	 * ::CriAtomExPlayer::SetBiquadFilterParameters 関数で利用します。
	 * \sa CriAtomExPlayer::SetBiquadFilterParameters
	 */
	public enum BiquadFilterType {
		Off,						/**<フィルタ無効			*/
		LowPass,					/**<ローパスフィルタ		*/
		HighPass,					/**<ハイパスフィルタ		*/
		Notch,						/**<ノッチフィルタ			*/
		LowShelf,					/**<ローシェルフフィルタ	*/
		HighShelf,					/**<ハイシェルフフィルタ	*/
		Peaking						/**<ピーキングフフィルタ	*/
	}

	/**
	 * <summary>ポーズ解除方法</summary>
	 * \par 説明:
	 * ポーズを解除する対象を指定するためのデータ型です。<br/>
	 * ::CriAtomExPlayer::Resume 関数、および ::CriAtomExPlayback::Resume
	 * 関数の引数として使用します。
	 * \sa CriAtomExPlayer::Resume, CriAtomExPlayback::Resume
	 */
	public enum ResumeMode {
		AllPlayback = 0,			/**< 一時停止方法に関係なく再生を再開					*/
		PausedPlayback = 1,			/**< Pause関数でポーズをかけた音声のみ再生を再開		*/
		PreparedPlayback = 2,		/**< Prepare関数で再生準備を指示した音声の再生を開始	*/
	}

	/**
	 * <summary>パンタイプ</summary>
	 * \par 説明:
	 * どのようにして定位計算を行うかを指定するためのデータ型です。<br/>
	 * ::CriAtomExPlayer::SetPanType 関数で利用します。<br/>
	 * \sa criAtomExPlayer::SetPanType
	 */
	public enum PanType {
		Pan3d = 0,					/**< パン3Dで定位を計算				*/
		Pos3d,						/**< 3Dポジショニングで定位を計算	*/
	}

	/**
	 * <summary>ボイス制御方式</summary>
	 * \par 説明:
	 * AtomExプレーヤで再生する音声の発音制御方法を指定するためのデータ型です。<br/>
	 * ::CriAtomExPlayer::SetVoiceControlMethod 関数で利用します。<br/>
	 * \sa CriAtomExPlayer::SetVoiceControlMethod
	 */
	public enum VoiceControlMethod {
		PreferLast = 0,				/**< 後着優先	*/
		PreferFirst,				/**< 先着優先	*/
	}

	/**
	 * <summary>パラメータID</summary>
	 * \par 説明:
	 * パラメータを指定するためのIDです。<br/>
	 * ::CriAtomExPlayer::GetParameterFloat32 関数等で利用します。
	 * \sa CriAtomExPlayer::GetParameterFloat32, CriAtomExPlayer::GetParameterSint32,
	 * CriAtomExPlayer::GetParameterUint32
	 */
	public enum Parameter {
		Volume					=  0,	/**< ボリューム									*/
		Pitch					=  1,	/**< ピッチ										*/
		Pan3dAngle				=  2,	/**< パンニング3D角度							*/
		Pan3dDistance			=  3,	/**< パンニング3D距離							*/
		Pan3dVolume				=  4,	/**< パンニング3Dボリューム						*/
		BusSendLevel0			=  9,	/**< バスセンドレベル0							*/
		BusSendLevel1			= 10,	/**< バスセンドレベル1							*/
		BusSendLevel2			= 11,	/**< バスセンドレベル2							*/
		BusSendLevel3			= 12,	/**< バスセンドレベル3							*/
		BusSendLevel4			= 13,	/**< バスセンドレベル4							*/
		BusSendLevel5			= 14,	/**< バスセンドレベル5							*/
		BusSendLevel6			= 15,	/**< バスセンドレベル6							*/
		BusSendLevel7			= 16,	/**< バスセンドレベル7							*/
		BandPassFilterCofLow	= 17,	/**< バンドパスフィルタの低域カットオフ周波数	*/
		BandPassFilterCofHigh	= 18,	/**< バンドパスフィルタの高域カットオフ周波数	*/
		BiquadFilterType		= 19,	/**< バイクアッドフィルタのフィルタタイプ		*/
		BiquadFilterFreq		= 20,	/**< バイクアッドフィルタの周波数				*/
		BiquadFIlterQ			= 21,	/**< バイクアッドフィルタのQ値					*/
		BiquadFilterGain		= 22,	/**< バイクアッドフィルタのゲイン				*/
		EnvelopeAttackTime		= 23,	/**< エンベロープのアタックタイム				*/
		EnvelopeHoldTime		= 24,	/**< エンベロープのホールドタイム				*/
		EnvelopeDecayTime		= 25,	/**< エンベロープのディケイタイム				*/
		EnvelopeReleaseTime		= 26,	/**< エンベロープのリリースタイム				*/
		EnvelopeSustainLevel	= 27,	/**< エンベロープのサスティンレベル				*/
		StartTime				= 28,	/**< 再生開始位置								*/
		Priority				= 31,	/**< ボイスプライオリティ						*/
	}

	/**
	 * <summary>フォーマット種別</summary>
	 * \par 説明:
	 * AtomExプレーヤで再生する音声のフォーマットを指定するためのデータ型です。<br/>
	 * ::CriAtomExPlayer::SetFormat 関数で利用します。<br/>
	 * \sa CriAtomExPlayer::SetFormat
	 */
	public enum Format : uint {
		ADX			= 0x00000001,		/**< ADX				*/
		HCA			= 0x00000003,		/**< HCA				*/
		HCA_MX		= 0x00000004,		/**< HCA-MX				*/
	}
	
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct FormatInfo {
		public Format format;			/**< 波形データID			*/
		public int samplingRate;		/**< フォーマット種別		*/
		public long numSamples;			/**< サンプリング周波数		*/
		public long loopOffset;			/**< チャンネル数			*/
		public long loopLength;			/**< トータルサンプル数		*/
		public int numChannels;			/**< ストリーミングフラグ	*/
		public uint reserved;			/**< 予約領域				*/
	}

	/**
	 * <summary>AISACコントロール情報取得用構造体</summary>
	 * \par 説明:
	 * AISACコントロール情報を取得するための構造体です。<br/>
	 * ::CriAtomExAcb::GetUsableAisacControl 関数に引数として渡します。<br/>
	 * \sa CriAtomExAcb::GetUsableAisacControl
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct AisacControlInfo {
		[MarshalAs(UnmanagedType.LPStr)]
		public readonly string	name;		/**< AISACコントロール名	*/
		public uint				id;			/**< AISACコントロールID	*/
		
		public AisacControlInfo(byte[] data)
		{
			this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, 0)));
			this.id = BitConverter.ToUInt32(data, 4);
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct CuePos3dInfo
	{
		public float coneInsideAngle;		/**< コーン内部角度				*/
		public float coneOutsideAngle;		/**< コーン外部角度				*/
		public float minDistance;			/**< 最小減衰距離				*/
		public float maxDistance;			/**< 最大減衰距離				*/
		public float dopplerFactor;			/**< ドップラー係数				*/
		public ushort distanceAisacControl;	/**< 距離減衰AISACコントロール	*/
		public ushort angleAisacControl;	/**< 角度AISACコントロール		*/
	}
	
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct GameVariableInfo
	{	
		[MarshalAs(UnmanagedType.LPStr)]
		public readonly string	name;		/**< ゲーム変数名	*/
		public uint 			id;			/**< ゲーム変数ID	*/
		public float			gameValue;	/**< ゲーム変数値	*/
		public uint 			reserved;	/**< 予約領域		*/
		public GameVariableInfo(string name, uint id, float gameValue)
		{
			this.name		= name;
			this.id			= id;
			this.gameValue	= gameValue;
			reserved = 0;
		}
	}
	
	/**
	 * <summary>キュータイプ</summary>
	 * \sa CriAtomEx::CueInfo
	 */
	public enum CueType
	{
		Polyphonic,				/**< ポリフォニック											*/	
		Sequential,				/**< シーケンシャル											*/
		Shuffle,				/**< シャッフル再生											*/
		Random,					/**< ランダム												*/
		RandomNoRepeat,			/**< ランダム非連続（前回再生した音以外をランダムに鳴らす）	*/
		Switch,					/**< スイッチ再生（ゲーム変数を参照して再生トラックの切り替える）	*/
		ComboSequential,		/**< コンボシーケンシャル（「コンボ時間」内に連続コンボが決まるとシーケンシャル、最後までいくと「コンボループバック」地点に戻る）*/
	}

	/**
	 * <summary>キュー情報</summary>
	 * \par 説明:
	 * キューの詳細情報です。<br/>
	 * \sa CriAtomExAcb::GetCueInfo
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct CueInfo
	{
		public int				id;					/**< キューID				*/
		public CueType			type;				/**< タイプ					*/
		[MarshalAs(UnmanagedType.LPStr)]
		public readonly string	name;				/**< キュー名				*/
		[MarshalAs(UnmanagedType.LPStr)]
		public readonly string	userData;			/**< ユーザーデータ			*/
		public long				length;				/**< 長さ(msec)				*/
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public ushort[]			categories;			/**< カテゴリインデックス	*/
		public ushort			numLimits;			/**< キューリミット			*/
		public ushort			numBlocks;			/**< ブロック数				*/
		public byte				priority;			/**< プライオリティ			*/
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public byte[]			reserved;			/**< 予約領域				*/
		public CuePos3dInfo		pos3dInfo;			/**< 3D情報					*/
		public GameVariableInfo gameVariableInfo;	/**< ゲーム変数				*/
		
		public CueInfo(byte[] data)
		{
			this.id = BitConverter.ToInt32(data, 0);
			this.type = (CueType)BitConverter.ToInt32(data, 4);
			this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, 8)));
			this.userData = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, 12)));
			this.length = BitConverter.ToInt64(data, 16);
			this.categories = new ushort[4];
			this.categories[0] = BitConverter.ToUInt16(data, 24);
			this.categories[1] = BitConverter.ToUInt16(data, 26);
			this.categories[2] = BitConverter.ToUInt16(data, 28);
			this.categories[3] = BitConverter.ToUInt16(data, 30);
			this.numLimits = BitConverter.ToUInt16(data, 32);
			this.numBlocks = BitConverter.ToUInt16(data, 34);
			this.priority = data[36];
			this.reserved = new byte[3];
			this.reserved[0] = 0;
			this.reserved[1] = 0;
			this.reserved[2] = 0;
			this.pos3dInfo = new CuePos3dInfo();
			this.pos3dInfo.coneInsideAngle = BitConverter.ToSingle(data, 40);
			this.pos3dInfo.coneOutsideAngle = BitConverter.ToSingle(data, 44);
			this.pos3dInfo.minDistance = BitConverter.ToSingle(data, 48);
			this.pos3dInfo.maxDistance = BitConverter.ToSingle(data, 52);
			this.pos3dInfo.dopplerFactor = BitConverter.ToSingle(data, 56);
			this.pos3dInfo.distanceAisacControl = BitConverter.ToUInt16(data, 60);
			this.pos3dInfo.angleAisacControl = BitConverter.ToUInt16(data, 62);
			this.gameVariableInfo = new GameVariableInfo(
				Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, 64))),
				BitConverter.ToUInt32(data, 68),
				BitConverter.ToSingle(data, 72)
				);
		}
	}
	
	/**
	 * <summary>音声波形情報</summary>
	 * \par 説明:
	 * 波形情報は、各キューから再生される音声波形の詳細情報です。<br/>
	 * \sa CriAtomExAcb::GetWaveformInfo
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct WaveformInfo
	{
		public int		waveId;				/**< 波形データID			*/
		public uint		format;				/**< フォーマット種別		*/
		public int		samplingRate;		/**< サンプリング周波数		*/
		public int		numChannels;		/**< チャンネル数			*/
		public long		numSamples;			/**< トータルサンプル数		*/
		public bool		streamingFlag;		/**< ストリーミングフラグ	*/
		public uint		reserved;			/**< 予約領域				*/
		
		public WaveformInfo(byte[] data)
		{
			this.waveId = BitConverter.ToInt32(data, 0);
			this.format = BitConverter.ToUInt32(data, 4);
			this.samplingRate = BitConverter.ToInt32(data, 8);
			this.numChannels = BitConverter.ToInt32(data, 12);
			this.numSamples = BitConverter.ToInt64(data, 16);
			this.streamingFlag = BitConverter.ToInt32(data, 24) != 0;
			this.reserved = 0;
		}
	}
	
	/**
	 * <summary>パフォーマンス情報</summary>
	 * \par 説明:
	 * パフォーマンス情報を取得するための構造体です。<br/>
	 * ::CriAtomEx::GetPerformanceInfo 関数で利用します。
	 * \sa ::CriAtomEx::GetPerformanceInfo
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct PerformanceInfo
	{
		public uint serverProcessCount;		/**<サーバ処理実行回数									*/
		public uint lastServerTime;			/**<サーバ処理時間の最終計測値（マイクロ秒単位）		*/
		public uint maxServerTime;			/**<サーバ処理時間の最大値（マイクロ秒単位）			*/
		public uint averageServerTime;		/**<サーバ処理時間の平均値（マイクロ秒単位）			*/
		public uint lastServerInterval;		/**<サーバ処理実行間隔の最終計測値（マイクロ秒単位）	*/
		public uint maxServerInterval;		/**<サーバ処理実行間隔の最大値（マイクロ秒単位）		*/
		public uint averageServerInterval;	/**<サーバ処理実行間隔の平均値（マイクロ秒単位）		*/
		
		public PerformanceInfo(byte[] data)
		{
			this.serverProcessCount = BitConverter.ToUInt32(data, 0);
			this.lastServerTime = BitConverter.ToUInt32(data, 4);
			this.maxServerTime = BitConverter.ToUInt32(data, 8);
			this.averageServerTime = BitConverter.ToUInt32(data, 12);
			this.lastServerInterval = BitConverter.ToUInt32(data, 16);
			this.maxServerInterval = BitConverter.ToUInt32(data, 20);
			this.averageServerInterval = BitConverter.ToUInt32(data, 24);
		}
	}

	/**
	 * <summary>ACFファイルの登録</summary>
	 * <param name="binder">バインダ</param>
	 * <param name="acfPath">ACFファイルのファイルパス</param>
	 * \par 説明:
	 * ACFファイルをロードし、ライブラリに取り込みます。<br/>
	 * \sa CriAtomEx::UnregisterAcf
	 */
	public static void RegisterAcf(CriFsBinder binder, string acfPath)
	{
		IntPtr binderHandle = (binder != null) ? binder.nativeHandle : IntPtr.Zero;
		criAtomEx_RegisterAcfFile(binderHandle, acfPath, IntPtr.Zero, 0);
	}

	/**
	 * <summary>ACFファイルの登録解除</summary>
	 * \par 説明:
	 * ACFファイルの登録を解除します。<br/>
	 * \sa CriAtomEx::RegisterAcf
	 */
	public static void UnregisterAcf()
	{
		criAtomEx_UnregisterAcf();
	}
	
	/**
	 * <summary>DSPバス設定のアタッチ</summary>
	 * <param name="settingName">DSPバス設定の名前</param>
	 * \par 説明:
	 * DSPバス設定からDSPバスを構築してサウンドレンダラにアタッチします。<br/>
	 * 本関数を実行するには、あらかじめ CriAtomEx::RegisterAcf 
	 * 関数でACF情報を登録しておく必要があります<br/>
	 * \code
	 *		：
	 * 	// ACFファイルの読み込みと登録
	 * 	CriAtomEx.RegisterAcf("Sample.acf");
	 * 	
	 * 	// DSPバス設定の適用
	 * 	CriAtomEx.AttachDspBusSetting("DspBusSetting_0");
	 * 		：
	 * \endcode
	 * \attention
	 * 本関数は完了復帰型の関数です。<br/>
	 * 本関数を実行すると、しばらくの間Atomライブラリのサーバ処理がブロックされます。<br/>
	 * 音声再生中に本関数を実行すると、音途切れ等の不具合が発生する可能性があるため、
	 * 本関数の呼び出しはシーンの切り替わり等、負荷変動を許容できるタイミングで行ってください。<br/>
	 * \sa CriAtomEx::DetachDspBusSetting, CriAtomEx::RegisterAcf
	 */
	public static void AttachDspBusSetting(string settingName)
	{
		criAtomEx_AttachDspBusSetting(settingName, IntPtr.Zero, 0);
	}

	/**
	 * <summary>DSPバス設定のデタッチ</summary>
	 * \par 説明:
	 * DSPバス設定をデタッチします。<br/>
	 * \attention
	 * 本関数は完了復帰型の関数です。<br/>
	 * 本関数を実行すると、しばらくの間Atomライブラリのサーバ処理がブロックされます。<br/>
	 * 音声再生中に本関数を実行すると、音途切れ等の不具合が発生する可能性があるため、
	 * 本関数の呼び出しはシーンの切り替わり等、負荷変動を許容できるタイミングで行ってください。
	 * \sa CriAtomEx::AttachDspBusSetting
	 */
	public static void DetachDspBusSetting()
	{
		criAtomEx_DetachDspBusSetting();
	}

	/**
	 * <summary>パフォーマンスモニタのリセット</summary>
	 * \par 説明:
	 * 現在までの計測結果を破棄します。<br/>
	 * パフォーマンスモニタは、ライブラリ初期化直後からパフォーマンス情報の取得を開始し、
	 * 計測結果を累積します。<br/>
	 * 以前の計測結果を今後の計測に含めたくない場合には、
	 * 本関数を実行し、累積された計測内容を一旦破棄する必要があります。
	 * \sa CriAtomEx::GetPerformanceInfo
	 */
	public static void ResetPerformanceMonitor()
	{
		criAtom_ResetPerformanceMonitor();
	}
	
	/**
	 * <summary>パフォーマンス情報の取得</summary>
	 * \par 説明:
	 * パフォーマンス情報を取得します。<br/>
	 * \sa CriAtomEx::PerformanceInfo, CriAtomEx::ResetPerformanceMonitor
	 */
	public static void GetPerformanceInfo(out PerformanceInfo info)
	{
		using (var mem = new CriStructMemory<PerformanceInfo>()) {
			criAtom_GetPerformanceInfo(mem.ptr);
			info = new PerformanceInfo(mem.bytes);
		}
	}
	
	#region DLL Import

	[DllImport(CriWare.pluginName)]
	private static extern bool criAtomEx_RegisterAcfFile(
		IntPtr binder, string path, IntPtr work, int workSize);

	[DllImport(CriWare.pluginName)]
	private static extern void criAtomEx_UnregisterAcf();

	[DllImport(CriWare.pluginName)]
	private static extern void criAtomEx_AttachDspBusSetting(
		string settingName, IntPtr work, int workSize);

	[DllImport(CriWare.pluginName)]
	private static extern void criAtomEx_DetachDspBusSetting();

	[DllImport(CriWare.pluginName)]
	private static extern void criAtom_ResetPerformanceMonitor();

	[DllImport(CriWare.pluginName)]
	private static extern void criAtom_GetPerformanceInfo(IntPtr info);

	#endregion
}

/**
 * <summary>カテゴリ単位のパラメータ制御を行うためのクラスです。</summary>
 * \par 説明:
 * カテゴリ単位のパラメータ制御を行うためのクラスです。<br/>
 */
public static class CriAtomExCategory
{
	/**
	 * <summary>名前指定によるカテゴリに対するボリューム設定</summary>
	 * <param name="name">カテゴリ名</param>
	 * <param name="volume">ボリューム値</param>
	 * \par 説明:
	 * 名前指定でカテゴリに対してボリュームを設定します。
	 */
	public static void SetVolume(string name, float volume)
	{
		criAtomExCategory_SetVolumeByName(name, volume);
	}

	/**
	 * <summary>ID指定によるカテゴリに対するボリューム設定</summary>
	 * <param name="id">カテゴリID</param>
	 * <param name="volume">ボリューム値</param>
	 * \par説明:
	 * ID指定でカテゴリに対してボリュームを設定します。
	 */
	public static void SetVolume(int id, float volume)
	{
		criAtomExCategory_SetVolumeById(id, volume);
	}

	/**
	 * <summary>名前指定によるカテゴリボリューム取得</summary>
	 * <param name="name">カテゴリ名</param>
	 * <returns>カテゴリボリューム</returns>
	 * \par説明:
	 * 名前指定でカテゴリで適用されるのボリューム値を取得します。
	 */
	public static float GetVolume(string name)
	{
		return criAtomExCategory_GetVolumeByName(name);
	}

	/**
	 * <summary>ID指定によるカテゴリボリューム取得</summary>
	 * <param name="id">カテゴリID</param>
	 * <returns>カテゴリボリューム</returns>
	 * \par説明:
	 * ID指定でカテゴリで適用されるのボリューム値を取得します。
	 */
	public static float GetVolume(int id)
	{
		return criAtomExCategory_GetVolumeById(id);
	}

	/**
	 * <summary>名前指定によるカテゴリミュート状態設定</summary>
	 * <param name="name">カテゴリ名</param>
	 * <param name="mute">ミュート状態（false = ミュート解除、true = ミュート）</param>
	 * \par説明:
	 * 名前指定でカテゴリのミュート状態を設定します。
	 */
	public static void Mute(string name, bool mute)
	{
		criAtomExCategory_MuteByName(name, mute);
	}

	/**
	 * <summary>ID指定によるカテゴリミュート状態設定</summary>
	 * <param name="id">カテゴリID</param>
	 * <param name="mute">ミュート状態（false = ミュート解除、true = ミュート）</param>
	 * \par説明:
	 * ID指定でカテゴリのミュート状態を設定します。
	 */
	public static void Mute(int id, bool mute)
	{
		criAtomExCategory_MuteById(id, mute);
	}

	/**
	 * <summary>名前指定によるカテゴリミュート状態取得</summary>
	 * <param name="name">カテゴリ名</param>
	 * <returns>ミュート状態（false = ミュート中ではない、true = ミュート中）</returns>
	 * \par説明:
	 * 名前指定でカテゴリのミュート状態を取得します。
	 */
	public static bool IsMuted(string name)
	{
		return criAtomExCategory_IsMutedByName(name);
	}

	/**
	 * <summary>ID指定によるカテゴリミュート状態取得</summary>
	 * <param name="id">カテゴリID</param>
	 * <returns>ミュート状態（false = ミュート中ではない、true = ミュート中）</returns>
	 * \par説明:
	 * ID指定でカテゴリのミュート状態を取得します。
	 */
	public static bool IsMuted(int id)
	{
		return criAtomExCategory_IsMutedById(id);
	}

	/**
	 * <summary>名前指定によるカテゴリソロ状態設定</summary>
	 * <param name="name">カテゴリ名</param>
	 * <param name="solo">ソロ状態（false = ソロ解除、true = ソロ）</param>
	 * <param name="muteVolume">他のカテゴリに適用するミュートボリューム値</param>
	 * \par説明:
	 * 名前指定でカテゴリのソロ状態を設定します。<br/>
	 * muteVolume で指定したボリュームは、
	 * 同一カテゴリグループに所属するカテゴリに対して適用されます。
	 */
	public static void Solo(string name, bool solo, float muteVolume)
	{
		criAtomExCategory_SoloByName(name, solo, muteVolume);
	}

	/**
	 * <summary>ID指定によるカテゴリソロ状態設定</summary>
	 * <param name="id">カテゴリID</param>
	 * <param name="solo">ソロ状態（false = ソロ解除、true = ソロ）</param>
	 * <param name="muteVolume">他のカテゴリに適用するミュートボリューム値</param>
	 * \par説明:
	 * ID指定でカテゴリのソロ状態を設定します。<br/>
	 * muteVolume で指定したボリュームは、
	 * 同一カテゴリグループに所属するカテゴリに対して適用されます。
	 */
	public static void Solo(int id, bool solo, float muteVolume)
	{
		criAtomExCategory_SoloById(id, solo, muteVolume);
	}

	/**
	 * <summary>名前指定によるカテゴリソロ状態取得</summary>
	 * <param name="name">カテゴリ名</param>
	 * <returns>ソロ状態（false = ソロ中ではない、true = ソロ中）</returns>
	 * \par説明:
	 * 名前指定でカテゴリのソロ状態を取得します。
	 */
	public static bool IsSoloed(string name)
	{
		return criAtomExCategory_IsSoloedByName(name);
	}

	/**
	 * <summary>ID指定によるカテゴリソロ状態取得</summary>
	 * <param name="id">カテゴリID</param>
	 * <returns>ソロ状態（false = ソロ中ではない、true = ソロ中）</returns>
	 * \par説明:
	 * ID指定でカテゴリのソロ状態を取得します。
	 */
	public static bool IsSoloed(int id)
	{
		return criAtomExCategory_IsSoloedById(id);
	}

	/**
	 * <summary>名前指定によるカテゴリのポーズ／ポーズ解除</summary>
	 * <param name="name">カテゴリ名</param>
	 * <param name="pause">スイッチ（false = ポーズ解除、true = ポーズ）</param>
	 * \par説明:
	 * 名前指定でカテゴリのポーズ／ポーズ解除を行います。<br/>
	 * カテゴリを名前で指定する以外は、
	 * ::CriAtomExCategory::Pause(int, bool)  関数と仕様は同じです。<br/>
	 * \sa CriAtomExCategory::Pause(int, bool)
	 */
	public static void Pause(string name, bool pause)
	{
		criAtomExCategory_PauseByName(name, pause);
	}

	/**
	 * <summary>ID指定によるカテゴリのポーズ／ポーズ解除</summary>
	 * <param name="id">カテゴリID</param>
	 * <param name="pause">スイッチ（false = ポーズ解除、true = ポーズ）</param>
	 * \par説明:
	 * ID指定でカテゴリのポーズ／ポーズ解除を行います。<br/>
	 * \par 備考:
	 * カテゴリのポーズは、AtomExプレーヤ／再生音のポーズ
	 * （ ::CriAtomExPlayer::Pause 関数や ::CriAtomExPlayback::Pause 関数でのポーズ）とは独立して扱われ、
	 * 音声の最終的なポーズ状態は、それぞれのポーズ状態を考慮して決まります。<br/>
	 * すなわち、どちらかがポーズ状態ならポーズ、どちらもポーズ解除状態ならポーズ解除、となります。
	 * \sa CriAtomExCategory::Pause(string, bool)
	 */
	public static void Pause(int id, bool pause)
	{
		criAtomExCategory_PauseById(id, pause);
	}

	/**
	 * <summary>ID指定によるカテゴリのポーズ状態取得</summary>
	 * <param name="id">カテゴリID</param>
	 * <returns>ポーズ状態（false = ポーズ中ではない、true = ポーズ中）</returns>
	 * \par説明:
	 * ID指定でカテゴリのポーズ状態を取得します。
	 */
	public static bool IsPaused(string name)
	{
		return criAtomExCategory_IsPausedByName(name);
	}

	/**
	 * <summary>名前指定によるカテゴリのポーズ／ポーズ解除</summary>
	 * <param name="name">カテゴリ名</param>
	 * <returns>ポーズ状態（false = ポーズ中ではない、true = ポーズ中）</returns>
	 * \par説明:
	 * 名前指定でカテゴリのポーズ状態を取得します。
	 */
	public static bool IsPaused(int id)
	{
		return criAtomExCategory_IsPausedById(id);
	}

	/**
	 * <summary>名前指定によるカテゴリに対するAISACコントロール値設定</summary>
	 * <param name="name">カテゴリ名</param>
	 * <param name="controlName">AISACコントロール名</param>
	 * <param name="value">AISACコントロール値</param>
	 * \par説明:
	 * 名前指定でカテゴリに対してAISACコントロール値を設定します。<br/>
	 * カテゴリおよびAISACコントロールを名前で指定する以外は、
	 * ::criAtomExCategory::SetAisacById 関数と仕様は同じです。<br/>
	 * \par 備考:
	 * カテゴリを名前、AISACコントロールをIDで指定したい場合、
	 * ::criAtomExAcf::GetAisacControlNameById 関数にて変換を行ってください。
	 * \sa
	 * CriAtomExCategory::SetAisac(int, int, float), CriAtomExCategory::AttachAisac
	 */
	public static void SetAisac(string name, string controlName, float value)
	{
		criAtomExCategory_SetAisacByName(name, controlName, value);
	}

	/**
	 * <summary>ID指定によるカテゴリに対するAISACコントロール値設定</summary>
	 * <param name="id">カテゴリID</param>
	 * <param name="controlId">AISACコントロールID</param>
	 * <param name="value">AISACコントロール値</param>
	 * \par説明:
	 * ID指定でカテゴリに対してAISACコントロール値を設定します。<br/>
	 * \attention
	 * キューやトラックに設定されているAISACに関しては、
	 * プレーヤでのAISACコントロール値設定よりも、
	 * <b>カテゴリのAISACコントロール値を優先して</b>参照します。<br/>
	 * カテゴリにアタッチしたAISACについては、
	 * 常に<b>カテゴリに設定したAISACコントロール値のみ</b>、参照されます。
	 * \sa
	 * CriAtomExCategory::SetAisac(string, string, float), CriAtomExCategory::AttachAisac
	 */
	public static void SetAisac(int id, int controlId, float value)
	{
		criAtomExCategory_SetAisacById(id, (ushort)controlId, value);
	}

	#region DLL Import

	[DllImport(CriWare.pluginName)]
	private static extern void criAtomExCategory_SetVolumeByName(string name, float volume);
	
	[DllImport(CriWare.pluginName)]
	private static extern float criAtomExCategory_GetVolumeByName(string name);

	[DllImport(CriWare.pluginName)]
	private static extern void criAtomExCategory_SetVolumeById(int id, float volume);

	[DllImport(CriWare.pluginName)]
	private static extern float criAtomExCategory_GetVolumeById(int id);

	[DllImport(CriWare.pluginName)]
	private static extern void criAtomExCategory_MuteById(int id, bool mute);

	[DllImport(CriWare.pluginName)]
	private static extern bool criAtomExCategory_IsMutedById(int id);

	[DllImport(CriWare.pluginName)]
	private static extern void criAtomExCategory_MuteByName(string name, bool mute);

	[DllImport(CriWare.pluginName)]
	private static extern bool criAtomExCategory_IsMutedByName(string name);

	[DllImport(CriWare.pluginName)]
	private static extern void criAtomExCategory_SoloById(int id, bool solo, float volume);

	[DllImport(CriWare.pluginName)]
	private static extern bool criAtomExCategory_IsSoloedById(int id);

	[DllImport(CriWare.pluginName)]
	private static extern void criAtomExCategory_SoloByName(string name, bool solo, float volume);

	[DllImport(CriWare.pluginName)]
	private static extern bool criAtomExCategory_IsSoloedByName(string name);

	[DllImport(CriWare.pluginName)]
	private static extern void criAtomExCategory_PauseById(int id, bool pause);

	[DllImport(CriWare.pluginName)]
	private static extern bool criAtomExCategory_IsPausedById(int id);

	[DllImport(CriWare.pluginName)]
	private static extern void criAtomExCategory_PauseByName(string name, bool pause);

	[DllImport(CriWare.pluginName)]
	private static extern bool criAtomExCategory_IsPausedByName(string name);

	[DllImport(CriWare.pluginName)]
	private static extern void criAtomExCategory_SetAisacById(int id, ushort controlId, float value);

	[DllImport(CriWare.pluginName)]
	private static extern void criAtomExCategory_SetAisacByName(string name, string controlName, float value);

	#endregion
}

public class CriAtomExAsr
{
	[StructLayout(LayoutKind.Sequential)]
	private struct BusAnalyzerConfig
	{
		public int interval;
		public int peakHoldTime;
	}
	
	/**
	 * <summary>レベル測定情報</summary>
	 * DSPバスのレベル測定情報を取得するための構造体です。<br/>
	 * CriAtomExAsr::GetBusAnalyzerInfo 関数で利用します。
	 * \par 備考:
	 * 各レベル値は音声データの振幅に対する倍率です（単位はデシベルではありません）。<br/>
	 * 以下のコードでデシベル表記に変換することができます。<br/>
	 * dB = 10.0f * log10f(level);
	 * \sa CriAtomExAsr::GetBusAnalyzerInfo
	 */
	[StructLayout(LayoutKind.Sequential)]
	public struct BusAnalyzerInfo
	{
		public int numChannels;					/**< 有効チャンネル数		*/
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public float[] rmsLevels;				/**< RMSレベル				*/
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public float[] peakLevels;				/**< ピークレベル			*/
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public float[] peakHoldLevels;			/**< ピークホールドレベル	*/

		public BusAnalyzerInfo(byte[] data)
		{
			this.numChannels = BitConverter.ToInt32(data, 0);
			this.rmsLevels = new float[8];
			for (int i = 0; i < 8; i++) {
				this.rmsLevels[i] = BitConverter.ToSingle(data, 4 + i * 4);
			}
			this.peakLevels = new float[8];
			for (int i = 0; i < 8; i++) {
				this.peakLevels[i] = BitConverter.ToSingle(data, 36 + i * 4);
			}
			this.peakHoldLevels = new float[8];
			for (int i = 0; i < 8; i++) {
				this.peakHoldLevels[i] = BitConverter.ToSingle(data, 68 + i * 4);
			}
		}
	}
	
	/**
	 * <summary>レベル測定機能の追加</summary>
	 * <param name="interval">測定間隔（ミリ秒）</param>
	 * <param name="peakHoldTime">ピークホールドレベルのホールド時間（ミリ秒）</param>
	 * \par 説明:
	 * DSPバスにレベル測定機能を追加し、レベル測定処理を開始します。<br/>
	 * 本関数を実行後、 CriAtomExAsr::GetBusAnalyzerInfo 関数を実行することで、
	 * RMSレベル（音圧）、ピークレベル（最大振幅）、ピークホールドレベルを
	 * 取得することが可能です。
	 * 複数DSPバスのレベルを計測するには、DSPバスごとに本関数を呼び出す必要があります。
	 * \sa CriAtomExAsr::GetBusAnalyzerInfo, CriAtomExAsr::DetachBusAnalyzer
	 */
	public static void AttachBusAnalyzer(int interval, int peakHoldTime)
	{
		BusAnalyzerConfig config;
		config.interval = 50;
		config.peakHoldTime = 1000;
		for (int i = 0; i < 8; i++) {
			criAtomExAsr_AttachBusAnalyzer(i, ref config);
		}
	}
	
	/**
	 * <summary>レベル測定機能の削除</summary>
	 * \par 説明:
	 * DSPバスからレベル測定機能を削除します。
	 * \sa CriAtomExAsr::AttachBusAnalyzer
	 */
	public static void DetachBusAnalyzer()
	{
		for (int i = 0; i < 8; i++) {
			criAtomExAsr_DetachBusAnalyzer(i);
		}
	}
	
	/**
	 * <summary>レベル測定結果の取得</summary>
	 * <param name="bus">DSPバス番号</param>
	 * <param name="info">レベル測定結果</param>
	 * \par 説明:
	 * DSPバスからレベル測定機能の結果を取得します。
	 * \sa CriAtomExAsr::AttachBusAnalyzer
	 */
	public static void GetBusAnalyzerInfo(int bus, out BusAnalyzerInfo info)
	{
		using (var mem = new CriStructMemory<BusAnalyzerInfo>()) {
			criAtomExAsr_GetBusAnalyzerInfo(bus, mem.ptr);
			info = new BusAnalyzerInfo(mem.bytes);
		}
	}
	
	/**
	 * <summary>DSPバスのボリュームの設定</summary>
	 * <param name="bus">DSPバス番号</param>
	 * <param name="volume">ボリューム値</param>
	 * \par 説明:
	 * DSPバスのボリュームを設定します。<br/>
	 * センドタイプがポストボリューム、ポストパンのセンド先に有効です。<br/>
	 * <br/>
	 * ボリューム値には、0.0f～1.0fの範囲で実数値を指定します。<br/>
	 * ボリューム値は音声データの振幅に対する倍率です（単位はデシベルではありません）。<br/>
	 * 例えば、1.0fを指定した場合、原音はそのままのボリュームで出力されます。<br/>
	 * 0.5fを指定した場合、原音波形の振幅を半分にしたデータと同じ音量（-6dB）で
	 * 音声が出力されます。<br/>
	 * 0.0fを指定した場合、音声はミュートされます（無音になります）。<br/>
	 * ボリュームのデフォルト値はCRI Atom Craftで設定した値です。<br/>
	 */
	public static void SetBusVolume(int bus, float volume)
	{
		criAtomExAsr_SetBusVolume(bus, volume);
	}
	
	/**
	 * <summary>DSPバスのセンドレベルの設定</summary>
	 * <param name="bus">DSPバス番号</param>
	 * <param name="sendTo">センド先DSPバス番号</param>
	 * <param name="level">レベル値</param>
	 * \par 説明:
	 * センド先DSPバスに音声データを送る際のレベルを設定します。<br/>
	 * <br/>
	 * レベル値には、0.0f～1.0fの範囲で実数値を指定します。<br/>
	 * レベル値は音声データの振幅に対する倍率です（単位はデシベルではありません）。<br/>
	 * 例えば、1.0fを指定した場合、原音はそのままのレベルで出力されます。<br/>
	 * 0.5fを指定した場合、原音波形の振幅を半分にしたデータと同じ音量（-6dB）で
	 * 音声が出力されます。<br/>
	 * 0.0fを指定した場合、音声はミュートされます（無音になります）。<br/>
	 * レベルのデフォルト値はCRI Atom Craftで設定した値です。<br/>
	 */
	public static void SetBusSendLevel(int bus, int sendTo, float level)
	{
		criAtomExAsr_SetBusSendLevel(bus, sendTo, level);
	}

	#region DLL Import

	[DllImport(CriWare.pluginName)]
	private static extern void criAtomExAsr_AttachBusAnalyzer(
		int bus_no, ref BusAnalyzerConfig config);

	[DllImport(CriWare.pluginName)]
	private static extern void criAtomExAsr_DetachBusAnalyzer(int bus_no);
	
	[DllImport(CriWare.pluginName)]
	private static extern void criAtomExAsr_GetBusAnalyzerInfo(
		int bus_no, IntPtr info);
	
	[DllImport(CriWare.pluginName)]
	private static extern void criAtomExAsr_SetBusVolume(
		int bus_no, float volume);
	
	[DllImport(CriWare.pluginName)]
	private static extern void criAtomExAsr_SetBusSendLevel(
		int bus_no, int sendto_no, float level);
	
	#endregion
}

/**
 * @}
 */

/* --- end of file --- */
