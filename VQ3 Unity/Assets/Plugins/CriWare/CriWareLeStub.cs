/****************************************************************************
 *
 * CRI Middleware SDK
 *
 * Copyright (c) 2013 CRI Middleware Co.,Ltd.
 *
 * Library  : CRI Ware
 * Module   : CRI Ware Stub for LE 
 * File     : CriWareLeStub.cs
 *
 ****************************************************************************/
using System;
using UnityEngine;
using System.Runtime.InteropServices;

public static class CriFsPlugin
{
	public static int defaultInstallBufferSize = 0;
	
	/* 初期化カウンタ */
	private static int initializationCount = 0;
	
	public static bool isInitialized { get { return initializationCount > 0; } }
	
	public static void SetConfigParameters(
		int num_loaders, int num_binders, int num_installers, int install_buffer_size)
	{
		CriFsPlugin.criFsUnity_SetConfigParameters(
			num_loaders, num_binders, num_installers);
	}
	
	public static void InitializeLibrary()
	{
		/* 初期化カウンタの更新 */
		CriFsPlugin.initializationCount++;
		if (CriFsPlugin.initializationCount != 1) {
			return;
		}
		
		/* CriWareInitializerが実行済みかどうかを確認 */
		bool initializerWorking = CriWareInitializer.IsInitialized();
		if (initializerWorking == false) {
			Debug.Log("[CRIWARE] CriWareInitializer is not working. "
				+ "Initializes FileSystem by default parameters.");
		}

		/* ライブラリの初期化 */
		CriFsPlugin.criFsUnity_Initialize();
	}

	public static void FinalizeLibrary()
	{
		/* 初期化カウンタの更新 */
		CriFsPlugin.initializationCount--;
		if (CriFsPlugin.initializationCount < 0) {
			Debug.LogError("[CRIWARE] ERROR: FileSystem library is already finalized.");
			return;
		}
		if (CriFsPlugin.initializationCount != 0) {
			return;
		}
		
		/* ライブラリの終了 */
		CriFsPlugin.criFsUnity_Finalize();
	}
	
    #region Native API Definition (DLL)
	// CRI File System Unity
	[DllImport(CriWare.pluginName)]
	private static extern void criFsUnity_SetConfigParameters(
		int num_loaders, int num_binders, int num_installers);

	[DllImport(CriWare.pluginName)]
	private static extern void criFsUnity_Initialize();

	[DllImport(CriWare.pluginName)]
	private static extern void criFsUnity_Finalize();
	
	[DllImport(CriWare.pluginName)]
	public static extern uint criFsUnity_GetAllocatedHeapSize();
	
	[DllImport(CriWare.pluginName)]
	public static extern void criFsUnity_SetUserAgentString(string userAgentString);
    #endregion
}

public class CriFsBinder 
{
	public IntPtr nativeHandle { get { return IntPtr.Zero; } }
};

public static class CriFsUtility
{
	public static void SetUserAgentString(string userAgentString) {}
};

public static class CriManaPlugin
{
	public static uint criManaUnity_GetAllocatedHeapSize() { return 0; }
	public static void SetConfigParameters(int num_decoders) {}
	public static void InitializeLibrary() {}
	public static void FinalizeLibrary() {}

};


/* --- end of file --- */
