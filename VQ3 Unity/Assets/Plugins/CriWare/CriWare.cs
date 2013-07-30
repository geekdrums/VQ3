/****************************************************************************
 *
 * CRI Middleware SDK
 *
 * Copyright (c) 2011-2012 CRI Middleware Co.,Ltd.
 *
 * Library  : CRI Ware (CRI File System / CRI Atom / CRI Mana)
 * Module   : CRI Ware for Unity
 * File     : CriWare.cs
 *
 ****************************************************************************/
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// CRI Ware
/// </summary>
public class CriWare
{
	/* スクリプトバージョン */
	private const string scriptVersionString = "1.11.14";
	private const int scriptVersionNumber = 0x01111400;

	/* ネイティブライブラリ */
	#if UNITY_EDITOR
		public const string pluginName = "cri_ware_unity";
	#elif UNITY_IOS
		public const string pluginName = "__Internal";
	#else
		public const string pluginName = "cri_ware_unity";
	#endif

	/**
	 * <summary>StreamingAssetsフォルダのパスです。</summary>
	 */
	public static string streamingAssetsPath
	{
		get {
			if (Application.platform == RuntimePlatform.Android) {
				return "";
			} else {
				return Application.streamingAssetsPath;
			}
		}
	}

	/**
	 * <summary>データフォルダのパスです。</summary>
	 * \attention
	 * iOS環境の場合、本フォルダへファイルの書き込みは、
	 * AppStoreの審査で問題になる可能性があります。<br/>
	 */
	public static string installTargetPath
	{
		get {
			if (Application.platform == RuntimePlatform.IPhonePlayer) {
				return Application.temporaryCachePath;
			} else {
				return Application.persistentDataPath;
			}
		}
	}

	private static GameObject _managerObject = null;
	public static GameObject managerObject
	{
		get {
			if (_managerObject == null) {
				_managerObject = GameObject.Find("/CRIWARE");
				if (_managerObject == null) {
					_managerObject = new GameObject("CRIWARE");
				}
			}
			return _managerObject;
		}
	}

	[DllImport(pluginName)]
	public static extern int criWareUnity_GetVersionNumber();

	/* バージョン文字列の番号 */
	public static int GetVersionNumber() {
		int version = criWareUnity_GetVersionNumber();
		return (version);
	}

	/* スクリプトバージョン文字列の取得 */
	public static string GetScriptVersionString() {
		return (scriptVersionString);
	}

	/* スクリプトバージョン番号の取得 */
	public static int GetScriptVersionNumber() {
		return (scriptVersionNumber);
	}

	public static uint GetFsMemoryUsage()
	{
		return CriFsPlugin.criFsUnity_GetAllocatedHeapSize();
	}

	public static uint GetAtomMemoryUsage()
	{
		return CriAtomPlugin.criAtomUnity_GetAllocatedHeapSize();
	}

	public static uint GetManaMemoryUsage()
	{
		return CriManaPlugin.criManaUnity_GetAllocatedHeapSize();
	}

	public struct CpuUsage
	{
		public float last;
		public float average;
		public float peak;
	}

	public static CpuUsage GetAtomCpuUsage()
	{
		return CriAtomPlugin.GetCpuUsage();
	}
}

/* --- end of file --- */
