/****************************************************************************
 *
 * CRIWARE Unity Plugin
 *
 * Copyright (c) 2012-2013 CRI Middleware Co.,Ltd.
 *
 * Library  : CRI Ware
 * Module   : CRI Ware Initializer
 * File     : CriWareInitializer.cs
 *
 ****************************************************************************/
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.IO;



/*JP
 * \brief CRI File System初期化パラメータ
 */
[System.Serializable]
public class CriFsConfig {
	/*JP デバイス性能読み込み速度のデフォルト値(bps) */
	public const int defaultAndroidDeviceReadBitrate = 50000000;

	/*JP ローダー数 */
	public int numberOfLoaders    = 16;
	/*JP バインダ数 */
	public int numberOfBinders    = 8;
	/*JP インストーラ数 */
	public int numberOfInstallers = 2;
	/*JP インストールバッファのサイズ */
	public int installBufferSize  = CriFsPlugin.defaultInstallBufferSize / 1024;
	/*JP パスの最大長 */
	public int maxPath            = 256;
	/*JP ユーザーエージェント文字列 */
	public string userAgentString = "";
	/*JP [Android] デバイス性能読み込み速度(bps) */
	public int androidDeviceReadBitrate = defaultAndroidDeviceReadBitrate;

}

/*JP
 * \brief CRI Atom初期化パラメータ
 */
[System.Serializable]
public class CriAtomConfig {
	/*JP ACFファイル名
	 *   \attention ACFファイルをStreamingAssetsフォルダに配置しておく必要あり。 */
	public string acfFileName = "";
	
	/*JP 標準ボイスプール作成パラメータ */
	[System.Serializable]
	public class StandardVoicePoolConfig {
		public int memoryVoices    = 16;
		public int streamingVoices = 8;
	}
	
	/*JP HCA-MXボイスプール作成パラメータ */
	[System.Serializable]
	public class HcaMxVoicePoolConfig {
		public int memoryVoices    = 0;
		public int streamingVoices = 0;
	}
	
	/*JP 最大バーチャルボイス数 */
	public int maxVirtualVoices = 32;
	/*JP 標準ボイスプール作成パラメータ */
	public StandardVoicePoolConfig standardVoicePoolConfig;
	/*JP HCA-MXボイスプール作成パラメータ */
	public HcaMxVoicePoolConfig hcaMxVoicePoolConfig;
	/*JP 出力サンプリングレート */
	public int outputSamplingRate = 0;
	/*JP インゲームプレビューを使用するかどうか */
	public bool usesInGamePreview = false;
	/*JP サーバ周波数 */
	public float serverFrequency  = 60.0f;
	/*JP 乱数種に時間（System.DateTime.Now.Ticks）を使用するかどうか */
	public bool useRandomSeedWithTime= false;

	/*JP [iOS] 出力バッファリング時間(ミリ秒)*/
	public int  iosBufferingTime     = 100;
	/*JP [iOS] iPodの再生を上書きするか？ */
	public bool iosOverrideIPodMusic = false;

	/*JP [Android] 出力バッファリング時間 */
	public int androidBufferingTime      = 133;
	/*JP [Android] 再生開始バッファリング時間 */
	public int androidStartBufferingTime = 100;
	
	/*JP [Android] 低遅延再生用ボイスプール作成パラメータ */
	[System.Serializable]
	public class AndroidLowLatencyStandardVoicePoolConfig {
		public int memoryVoices    = 0;
		public int streamingVoices = 0;
	}
	/*JP [Android] 低遅延再生用ボイスプール作成パラメータ */
	public AndroidLowLatencyStandardVoicePoolConfig androidLowLatencyStandardVoicePoolConfig;
}

/*JP
 * \brief CRI Mana初期化パラメータ
 */
[System.Serializable]
public class CriManaConfig {
	/*JP デコーダー数 */
	public int  numberOfDecoders   = 8;
	/*JP 連結再生エントリー数 */
	public int  numberOfMaxEntries = 4;
	/*JP キューポイント再生を可能にするかどうか */
	public bool enableCuePoint     = false;
}

/// \addtogroup CRIWARE_UNITY_COMPONENT
/// @{

/*JP
 * \brief CRIWARE初期化コンポーネント
 * \par 説明:
 * CRIWAREライブラリの初期化を行うためのコンポーネントです。<br>
 */
[AddComponentMenu("CRIWARE/Library Initializer")]
public class CriWareInitializer : MonoBehaviour {
	
	/*JP CRI File Systemライブラリを初期化するかどうか */
	public bool initializesFileSystem = true;
	
	/*JP CRI File Systemライブラリ初期化設定 */
	public CriFsConfig fileSystemConfig = new CriFsConfig();
	
	/*JP CRI Atomライブラリを初期化するかどうか */
	public bool initializesAtom = true;
	
	/*JP CRI Atomライブラリ初期化設定 */
	public CriAtomConfig atomConfig = new CriAtomConfig();
	
	/*JP CRI Manaライブラリを初期化するかどうか */
	public bool initializesMana = true;
	
	/*JP CRI Manaライブラリ初期化設定 */
	public CriManaConfig manaConfig = new CriManaConfig();
	
	/*JP シーンチェンジ時にライブラリを終了するかどうか */
	public bool dontDestroyOnLoad = false;
	
	/* オブジェクト作成時の処理 */
	void Awake() {
		/* 現在のランタイムのバージョンが正しいかチェック */
		CriWare.CheckBinaryVersionCompatibility();

		/* 初期化カウンタの更新 */
		initializationCount++;
		if (initializationCount != 1) {
			/* CriWareInitializer自身による多重初期化は許可しない */
			GameObject.Destroy(this);
			return;
		}	

		/* CRI File Systemライブラリの初期化 */
		if (initializesFileSystem) {
			CriFsPlugin.SetConfigParameters(
				fileSystemConfig.numberOfLoaders,
				fileSystemConfig.numberOfBinders,
				fileSystemConfig.numberOfInstallers,
				(fileSystemConfig.installBufferSize * 1024),
				fileSystemConfig.maxPath
				);
			{
				/* Ver.2.03.03 以前は 0 がデフォルト値だったことの互換性維持のための処理 */
				if (fileSystemConfig.androidDeviceReadBitrate == 0) {
					fileSystemConfig.androidDeviceReadBitrate = CriFsConfig.defaultAndroidDeviceReadBitrate;
				}
			}
			CriFsPlugin.SetConfigAdditionalParameters_ANDROID(fileSystemConfig.androidDeviceReadBitrate);
			CriFsPlugin.InitializeLibrary();
			if (fileSystemConfig.userAgentString.Length != 0) {
				CriFsUtility.SetUserAgentString(fileSystemConfig.userAgentString);
			}
		}
		
		/* CRI Atomライブラリの初期化 */
		if (initializesAtom) {
			/* 初期化処理の実行 */
			CriAtomPlugin.SetConfigParameters(
				(int)Math.Max(atomConfig.maxVirtualVoices, CriAtomPlugin.GetRequiredMaxVirtualVoices(atomConfig)),
				atomConfig.standardVoicePoolConfig.memoryVoices,
				atomConfig.standardVoicePoolConfig.streamingVoices,
				atomConfig.hcaMxVoicePoolConfig.memoryVoices,
				atomConfig.hcaMxVoicePoolConfig.streamingVoices,
				atomConfig.outputSamplingRate,
				atomConfig.usesInGamePreview,
				atomConfig.serverFrequency);

			CriAtomPlugin.SetConfigAdditionalParameters_IOS(
				(uint)Math.Max(atomConfig.iosBufferingTime, 16),
				atomConfig.iosOverrideIPodMusic
				);
			
			{
				/* Ver.2.03.03 以前は 0 がデフォルト値だったことの互換性維持のための処理 */
				if (atomConfig.androidBufferingTime == 0) {
					atomConfig.androidBufferingTime = (int)(4 * 1000.0 / atomConfig.serverFrequency);
				}
				if (atomConfig.androidStartBufferingTime == 0) {
					atomConfig.androidStartBufferingTime = (int)(3 * 1000.0 / atomConfig.serverFrequency);
				}
			}
			CriAtomPlugin.SetConfigAdditionalParameters_ANDROID(
				atomConfig.androidLowLatencyStandardVoicePoolConfig.memoryVoices,
				atomConfig.androidLowLatencyStandardVoicePoolConfig.streamingVoices,
				atomConfig.androidBufferingTime,
				atomConfig.androidStartBufferingTime);

			CriAtomPlugin.InitializeLibrary();

			if (atomConfig.useRandomSeedWithTime == true) {
				/* 時刻を乱数種に設定 */
				CriAtomEx.SetRandomSeed((uint)System.DateTime.Now.Ticks);
			}

			/* ACFファイル指定時は登録 */
			if (atomConfig.acfFileName.Length != 0) {
				CriAtomEx.RegisterAcf(null,
					Path.Combine(CriWare.streamingAssetsPath, atomConfig.acfFileName));
			}
		}
		
		/* CRI Manaライブラリの初期化 */
		if (initializesMana) {
			CriManaPlugin.SetConfigParameters(manaConfig.numberOfDecoders, manaConfig.numberOfMaxEntries, manaConfig.enableCuePoint);
			CriManaPlugin.InitializeLibrary();
		}
		
		/* シーンチェンジ後もオブジェクトを維持するかどうかの設定 */
		if (dontDestroyOnLoad) {
			DontDestroyOnLoad(transform.gameObject);
			DontDestroyOnLoad(CriWare.managerObject);
		}
	}
	
	/* Execution Order の設定を確実に有効にするために OnEnable をオーバーライド */
	void OnEnable() {
	}
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	void OnDestroy() {
		/* 初期化カウンタの更新 */
		initializationCount--;
		if (initializationCount != 0) {
			return;
		}
		
		/* CRI Manaライブラリの終了 */
		if (initializesMana) {
			CriManaPlugin.FinalizeLibrary();
		}
		
		/* CRI Atomライブラリの終了 */
		if (initializesAtom) {
			/* 終了処理の実行 */
			CriAtomPlugin.FinalizeLibrary();
		}
		
		/* CRI File Systemライブラリの終了 */
		if (initializesFileSystem) {
			CriFsPlugin.FinalizeLibrary();
		}
	}
	
	/* 初期化カウンタ */
	private static int initializationCount = 0;
	
	/* 初期化実行チェック関数 */
	public static bool IsInitialized() {
		if (initializationCount > 0) {
			return true;
		} else {
			/* 現在のランタイムのバージョンが正しいかチェック */
			CriWare.CheckBinaryVersionCompatibility();
			return false;
		}
	}
} // end of class

/// @}

/* --- end of file --- */
