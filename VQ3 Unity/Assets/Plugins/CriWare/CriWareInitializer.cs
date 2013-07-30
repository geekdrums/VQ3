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

/* CRI File System初期化パラメータ */
[System.Serializable]
public class CriFsConfig {
	public int numberOfLoaders    = 16;
	public int numberOfBinders    = 8;
	public int numberOfInstallers = 2;
	public int installBufferSize  = CriFsPlugin.defaultInstallBufferSize / 1024;
	public string userAgentString = "";
}

/* CRI Atom初期化パラメータ */
[System.Serializable]
public class CriAtomConfig {
	/* ACFファイル名 */
	/* 注意）ACFファイルをStreamingAssetsフォルダに配置しておく必要あり。 */
	public string acfFileName = "";
	
	/* 標準ボイスプール作成パラメータ */
	[System.Serializable]
	public class StandardVoicePoolConfig {
		public int memoryVoices    = 16;
		public int streamingVoices = 8;
	}
	
	/* HCA-MXボイスプール作成パラメータ */
	[System.Serializable]
	public class HcaMxVoicePoolConfig {
		public int memoryVoices    = 0;
		public int streamingVoices = 0;
	}
	public int maxVirtualVoices = 32;
	public StandardVoicePoolConfig standardVoicePoolConfig;
	public HcaMxVoicePoolConfig hcaMxVoicePoolConfig;
	public int outputSamplingRate = 0;
	public bool usesInGamePreview = false;
	public float serverFrequency  = 30.0f;
	public int iosBufferingTime   = 100;
}

/* CRI Mana初期化パラメータ */
[System.Serializable]
public class CriManaConfig {
	public int numberOfDecoders = 8;
}

/*JP
 * \brief CRIWARE初期化コンポーネント
 * \ingroup WARE_UNITY_COMPONENT
 * \par 説明:
 * CRIWAREライブラリの初期化を行うためのコンポーネントです。<br>
 */
[AddComponentMenu("CRIWARE/Library Initializer")]
public class CriWareInitializer : MonoBehaviour {
	
	/*JP< CRI File Systemライブラリを初期化するかどうか */
	public bool initializesFileSystem = true;
	
	/*JP< CRI File Systemライブラリ初期化設定 */
	public CriFsConfig fileSystemConfig = new CriFsConfig();
	
	/*JP< CRI Atomライブラリを初期化するかどうか */
	public bool initializesAtom = true;
	
	/*JP< CRI Atomライブラリ初期化設定 */
	public CriAtomConfig atomConfig = new CriAtomConfig();
	
	/*JP< CRI Manaライブラリを初期化するかどうか */
	public bool initializesMana = false;
	
	/*JP< CRI Manaライブラリ初期化設定 */
	public CriManaConfig manaConfig = new CriManaConfig();
	
	/*JP< シーンチェンジ時にライブラリを終了するかどうか */
	public bool dontDestroyOnLoad = false;
	
	/* オブジェクト作成時の処理 */
	void Awake() {
		/* 初期化カウンタの更新 */
		initializationCount++;
		if (initializationCount != 1) {
			/* 多重初期化は許可しない */
			GameObject.Destroy(this);
			return;
		}

		/* CRI File Systemライブラリの初期化 */
		if (initializesFileSystem != false) {
			CriFsPlugin.SetConfigParameters(
				fileSystemConfig.numberOfLoaders,
				fileSystemConfig.numberOfBinders,
				fileSystemConfig.numberOfInstallers,
				(fileSystemConfig.installBufferSize * 1024)
				);
			CriFsPlugin.InitializeLibrary();
			if (fileSystemConfig.userAgentString.Length != 0) {
				CriFsUtility.SetUserAgentString(fileSystemConfig.userAgentString);
			}
		}
		
		/* CRI Atomライブラリの初期化 */
		if (initializesAtom != false) {
			/* serverFrequency に対して iosBufferingTime が小さすぎる場合に補正を行う */
			uint modifiedIosBufferingTime = (uint)Math.Max(atomConfig.iosBufferingTime,
				Math.Max(50, (140 - (int)(atomConfig.serverFrequency * 4.0f / 3.0f))));
			
			/* 初期化処理の実行 */
			CriAtomPlugin.SetConfigParameters(
				atomConfig.maxVirtualVoices,
				atomConfig.standardVoicePoolConfig.memoryVoices,
				atomConfig.standardVoicePoolConfig.streamingVoices,
				atomConfig.hcaMxVoicePoolConfig.memoryVoices,
				atomConfig.hcaMxVoicePoolConfig.streamingVoices,
				atomConfig.outputSamplingRate,
				atomConfig.usesInGamePreview,
				atomConfig.serverFrequency,
				modifiedIosBufferingTime);
			CriAtomPlugin.InitializeLibrary();
			
			/* ACFファイル指定時は登録 */
			if (atomConfig.acfFileName.Length != 0) {
				CriAtomEx.RegisterAcf(null,
					CriWare.streamingAssetsPath + "/" + atomConfig.acfFileName);
			}
		}
		
		/* CRI Manaライブラリの初期化 */
		if (initializesMana != false) {
			CriManaPlugin.SetConfigParameters(manaConfig.numberOfDecoders);
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
		if (initializesMana != false) {
			CriManaPlugin.FinalizeLibrary();
		}
		
		/* CRI Atomライブラリの終了 */
		if (initializesAtom != false) {
			/* 終了処理の実行 */
			CriAtomPlugin.FinalizeLibrary();
		}
		
		/* CRI File Systemライブラリの終了 */
		if (initializesFileSystem != false) {
			CriFsPlugin.FinalizeLibrary();
		}
	}
	
	/* 初期化カウンタ */
	private static int initializationCount = 0;
	
	/* 初期化実行チェック関数 */
	public static bool IsInitialized() {
		return (initializationCount>0) ? true : false;
	}
}

/* --- end of file --- */
