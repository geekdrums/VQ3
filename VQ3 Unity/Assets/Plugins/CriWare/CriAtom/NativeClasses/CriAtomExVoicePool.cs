/****************************************************************************
 *
 * CRI Middleware SDK
 *
 * Copyright (c) 2011-2012 CRI Middleware Co.,Ltd.
 *
 * Library  : CRI Atom
 * Module   : CRI Atom Native Wrapper
 * File     : CriAtomVoicePool.cs
 *
 ****************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

/*==========================================================================
 *      CRI Atom Native Wrapper
 *=========================================================================*/
/**
 * \addtogroup CRIATOM_NATIVE_WRAPPER
 * @{
 */

public class CriAtomExVoicePool : IDisposable
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct StandardConfig
	{
		public uint identifier;
		public int num_voices;
		public int max_channels;
		public int max_sampling_rate;
		public bool streaming_flag;
		public CriAtomEx.SoundRendererType sound_renderer_type;
		public int decode_latency;
	}

	/**
	 * <summary>標準ボイスプールの作成</summary>
	 * <param name="numVoices">ボイス数</param>
	 * <param name="maxChannels">最大チャンネル数</param>
	 * <param name="maxSamplingRate">最大サンプリングレート</param>
	 * <param name="streamingFlag">ストリーム再生を行うかどうか</param>
	 * <returns>CriAtomExVoicePoolオブジェクト</returns>
	 * \par 説明：
	 * 標準ボイスプールを作成します。<br/>
	 * （標準ボイスは、ADXデータとHCAデータの両方の再生に対応したボイスです。）<br/>
	 * <br/>
	 * 本関数を実行することで、ADXとHCAの再生が可能なボイスがプールされます。<br/>
	 * AtomExプレーヤでADXやHCAデータ（もしくはADXやHCAデータを含むキュー）の再生を行うと、
	 * AtomExプレーヤは作成された標準ボイスプールからボイスを取得し、再生を行います。<br/>
	 * <br/>
	 * ボイスプールの作成に成功すると、戻り値としてCriAtomExVoicePoolオブジェクトが返されます。<br/>
	 * ボイスプールの作成に失敗すると、本関数は null を返します。<br/>
	 * ボイスプールの作成に失敗した理由については、エラーコールバックのメッセージで確認可能です。<br/>
	 * \par 備考:
	 * ボイスプール作成時には、第1引数（ numVoices ）で指定した数のボイスが作成されます。<br/>
	 * 作成するボイスの数が多いほど、同時に再生可能な音声の数は増えますが、
	 * 反面、使用するメモリは増加します。<br/>
	 * <br/>
	 * ボイスプール作成時には、ボイス数の他に、再生可能な音声のチャンネル数、
	 * サンプリング周波数、ストリーム再生の有無を指定します。<br/>
	 * <br/>
	 * 最大チャンネル数（第2引数の numChannels ）は、
	 * ボイスプール内のボイスが再生できる音声データのチャンネル数になります。<br/>
	 * チャンネル数を少なくすることで、ボイスプールの作成に必要なメモリサイズは小さくなりますが、
	 * 指定されたチャンネル数を越えるデータは再生できなくなります。<br/>
	 * 例えば、ボイスプールをモノラルで作成した場合、ステレオのデータは再生できません。<br/>
	 * （ステレオデータを再生する場合、AtomExプレーヤは、
	 * ステレオが再生可能なボイスプールからのみボイスを取得します。）<br/>
	 * ただし、ステレオのボイスプールを作成した場合、モノラルデータ再生時にステレオ
	 * ボイスプールのボイスが使用される可能性はあります。<br/>
	 * <br/>
	 * 最大サンプリングレート（第3引数の maxSamplingRate ）についても、
	 * 値を下げることでもボイスプールに必要なメモリサイズは小さくすることが可能ですが、指
	 * 定されたサンプリングレートを越えるデータは再生できなくなります。<br/>
	 * （指定されたサンプリングレート以下のデータのみが再生可能です。）<br/>
	 * <br/>
	 * ストリーミング再生の有無（第4引数の streamingFlag ）についても、
	 * オンメモリ再生のみのボイスプールはストリーミング再生可能なボイスプールに比べ、
	 * サイズが小さくなります。<br/>
	 * <br/>
	 * 尚、AtomExプレーヤがデータを再生した際に、
	 * ボイスプール内のボイスが全て使用中であった場合、
	 * ボイスプライオリティによる発音制御が行われます。<br/>
	 * \attention
	 * 本関数を実行する前に、ライブラリを初期化しておく必要があります。<br/>
	 * <br/>
	 * ストリーム再生用のボイスプールは、内部的にボイスの数分だけローダ（ CriFsLoader ）
	 * を確保します。<br/>
	 * ストリーム再生用のボイスプールを作成する場合、ボイス数分のローダが確保できる設定で
	 * ライブラリを初期化する必要があります。<br/>
	 * <br/>
	 * 本関数は完了復帰型の関数です。<br/>
	 * ボイスプールの作成にかかる時間は、プラットフォームによって異なります。<br/>
	 * ゲームループ等の画面更新が必要なタイミングで本関数を実行するとミリ秒単位で
	 * 処理がブロックされ、フレーム落ちが発生する恐れがあります。<br/>
	 * ボイスプールの作成／破棄は、シーンの切り替わり等、負荷変動を許容できる
	 * タイミングで行うようお願いいたします。<br/>
	 * \sa CriAtomExVoicePool::Dispose
	 */
	public static CriAtomExVoicePool AllocateStandardVoicePool(
		int numVoices, int maxChannels, int maxSamplingRate, bool streamingFlag)
	{
		var config = new StandardConfig();
		config.identifier = 0;
		config.num_voices = numVoices;
		config.max_channels = maxChannels;
		config.max_sampling_rate = maxSamplingRate;
		config.streaming_flag = streamingFlag;
		config.sound_renderer_type = CriAtomEx.SoundRendererType.Default;
		config.decode_latency = 0;
		IntPtr handle = criAtomExVoicePool_AllocateStandardVoicePool(ref config, IntPtr.Zero, 0);
		if (handle == IntPtr.Zero) {
			return null;
		}
		return new CriAtomExVoicePool(handle);
	}

	/**
	 * <summary>ボイスプールの破棄</summary>
	 * \par 説明:
	 * 作成済みのボイスプールを破棄します。<br/>
	 * \attention
	 * 本関数は完了復帰型の関数です。<br/>
	 * 音声再生中にボイスプールを破棄した場合、本関数内で再生停止を待ってから
	 * リソースの解放が行われます。<br/>
	 * （ファイルから再生している場合は、さらに読み込み完了待ちが行われます。）<br/>
	 * そのため、本関数内で処理が長時間（数フレーム）ブロックされる可能性があります。<br/>
	 * ボイスプールの作成／破棄は、シーンの切り替わり等、負荷変動を許容できる
	 * タイミングで行うようお願いいたします。<br/>
	 * \sa CriAtomExVoicePool::AllocateStandardVoicePool
	 */
	public void Dispose()
	{
		criAtomExVoicePool_Free(this.handle);
		GC.SuppressFinalize(this);
	}

	#region Internal Members

	private CriAtomExVoicePool(IntPtr handle)
	{
		this.handle = handle;
	}

	~CriAtomExVoicePool()
	{
		this.Dispose();
	}

	private IntPtr handle;
	
	#endregion

	#region DLL Import

	[DllImport(CriWare.pluginName)]
	private static extern IntPtr criAtomExVoicePool_AllocateStandardVoicePool(
		ref StandardConfig config, IntPtr work, int work_size);

	[DllImport(CriWare.pluginName)]
	private static extern IntPtr criAtomExVoicePool_Free(IntPtr handle);
	
	#endregion
}

/**
 * @}
 */

/* --- end of file --- */
