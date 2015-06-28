/****************************************************************************
 *
 * CRI Middleware SDK
 *
 * Copyright (c) 2011-2013 CRI Middleware Co.,Ltd.
 *
 * Library  : CRI Ware
 * Module   : CRI Ware Initializer for Unity Editor
 * File     : CriWareInitializerEditor.cs
 *
 ****************************************************************************/
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(CriWareInitializer))]
public class CriWareInitializerEditor : Editor 
{	
	private void GenToggleField(string label_str, string tooltip, ref bool field_value)
	{
		GUIContent content = new GUIContent(label_str, tooltip);
		field_value = EditorGUILayout.Toggle(content, field_value);
	}
	
	private void GenIntField(string label_str, string tooltip, ref int field_value, int min, int max)
	{
		GUIContent content = new GUIContent(label_str, tooltip);
		field_value = Math.Min(max, Math.Max(min, EditorGUILayout.IntField(content, field_value, GUILayout.MaxWidth(200))));
	}

	private void GenIntFieldWithUnit(string label_str, string label_unit, string tooltip, ref int field_value, int min, int max, int max_width = 200)
	{
		EditorGUILayout.BeginHorizontal();
		{
			GUIContent content = new GUIContent(label_str, tooltip);
			field_value = Math.Min(max, Math.Max(min, EditorGUILayout.IntField(content, field_value, GUILayout.MaxWidth(max_width))));
			int indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			EditorGUILayout.LabelField(label_unit);
			EditorGUI.indentLevel = indentLevel;
		}
		EditorGUILayout.EndHorizontal();
	}

	private void GenPositiveFloatField(string label_str, string tooltip, ref float field_value, float min, float max)
	{
		GUIContent content = new GUIContent(label_str, tooltip);
		field_value = Math.Min(max, Math.Max(min, EditorGUILayout.FloatField(content, field_value, GUILayout.MaxWidth(200))));
	}

	private void GenPositiveFloatField(string label_str, string label_unit, string tooltip, ref float field_value, float min, float max)
	{
		EditorGUILayout.BeginHorizontal();
		{
			GUIContent content = new GUIContent(label_str, tooltip);
			field_value = Math.Min(max, Math.Max(min, EditorGUILayout.FloatField(content, field_value, GUILayout.MaxWidth(200))));
			int indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			EditorGUILayout.LabelField(label_unit);
			EditorGUI.indentLevel = indentLevel;
		}
		EditorGUILayout.EndHorizontal();
	}

	private void GenStringField(string label_str, string tooltip, ref string field_value)
	{
		GUIContent content = new GUIContent(label_str, tooltip);
		field_value = EditorGUILayout.TextField(content, field_value);
	}

	static private bool showFileSystemAndroidConfig     = false;
	static private bool showAtomStandardVoicePoolConfig = false;
	static private bool showAtomHcaMxVoicePoolConfig    = false;
	static private bool showAtomIOSConfig               = false;
	static private bool showAtomAndroidConfig           = false;
	static private bool showAtomAndroidVoicePoolConfig  = true;

	public override void OnInspectorGUI()
	{
		CriWareInitializer initializer = target as CriWareInitializer;
		
		// FileSystem Config
		initializer.initializesFileSystem = 
			EditorGUILayout.BeginToggleGroup("Initialize FileSystem", initializer.initializesFileSystem);
		EditorGUI.indentLevel += 1;
		{	
			GenIntField("Number of Loaders",        "The maximum number of CriFsLoader objects used at a time. " +
				"NOTE: Count the number of CriFsLoadFileRequest objects and the total number of Streaming Voices in CRI Atom settings and the number of CriManaPlayer component,", 
				ref initializer.fileSystemConfig.numberOfLoaders,    0,  128);
			GenIntField("Number of Binders",        "", ref initializer.fileSystemConfig.numberOfBinders,    0,  128);
			GenIntField("Number of Installers",     "", ref initializer.fileSystemConfig.numberOfInstallers, 0,  128);
			GenIntFieldWithUnit("Install Buffer Size", "[KiB]", "Internal buffer size to install data. A larger buffer size result in better performance.", ref initializer.fileSystemConfig.installBufferSize,  32, int.MaxValue);
			GenIntField("Max Length of Path",       "The maximum length of path (file path or url path) that can be passed.", ref initializer.fileSystemConfig.maxPath, 64,  2048);
			GenStringField("User Agent String",     "", ref initializer.fileSystemConfig.userAgentString);

			showFileSystemAndroidConfig = EditorGUILayout.Foldout(showFileSystemAndroidConfig, "Android Config");
			if (showFileSystemAndroidConfig) {
				EditorGUI.indentLevel += 1;
				{
					/* Ver.2.03.03 以前は 0 がデフォルト値だったことの互換性維持のための処理 */
					if (initializer.fileSystemConfig.androidDeviceReadBitrate == 0) {
						initializer.fileSystemConfig.androidDeviceReadBitrate = CriFsConfig.defaultAndroidDeviceReadBitrate;
					}
				}
				GenIntFieldWithUnit("Device Read Bitrate", "[bps]", "Expected minimum device read bitrate to be used for multi-streaming management." + CriFsConfig.defaultAndroidDeviceReadBitrate + " bps", 
					ref initializer.fileSystemConfig.androidDeviceReadBitrate, 0, int.MaxValue, 225);
				EditorGUI.indentLevel -= 1;
			}
		}
		EditorGUI.indentLevel -= 1;
		EditorGUILayout.EndToggleGroup();
		
		// Atom Config
		initializer.initializesAtom = 
			EditorGUILayout.BeginToggleGroup("Initialize Atom", initializer.initializesAtom);
		EditorGUI.indentLevel += 1;
		{
			GenStringField("ACF File Name",           "", ref initializer.atomConfig.acfFileName);
			GenIntField("Max Virtual Voices",         "", ref initializer.atomConfig.maxVirtualVoices,      CriAtomPlugin.GetRequiredMaxVirtualVoices(initializer.atomConfig),    1024);
			GenIntFieldWithUnit("Sampling Rate", "[Hz]",
				"Sound output sampling rate. "
				+ "HCA-MX needs to set the sampling rate of HCA-MX data. "
				+ "A value of 0 (the default value) means that the internal value will be applied.",
				ref initializer.atomConfig.outputSamplingRate,    0,    192000);
			GenPositiveFloatField("Server Frequency", "[Hz]", "", ref initializer.atomConfig.serverFrequency, 15.0f, 120.0f);
			GenToggleField("Uses Time For Seed",    "", ref initializer.atomConfig.useRandomSeedWithTime);
			GenToggleField("Uses In Game Preview",    "", ref initializer.atomConfig.usesInGamePreview);
			
			showAtomStandardVoicePoolConfig = EditorGUILayout.Foldout(showAtomStandardVoicePoolConfig, "Standard Voice Pool Config");
			if (showAtomStandardVoicePoolConfig) {
				EditorGUI.indentLevel += 1;
				GenIntField("Memoy Voices", "", ref initializer.atomConfig.standardVoicePoolConfig.memoryVoices,        0, 1024);
				GenIntField("Streaming Voices", "", ref initializer.atomConfig.standardVoicePoolConfig.streamingVoices, 0, 1024);
				EditorGUI.indentLevel -= 1;
			}
			
			showAtomHcaMxVoicePoolConfig = EditorGUILayout.Foldout(showAtomHcaMxVoicePoolConfig, "HCA-MX Voice Pool Config");
			if (showAtomHcaMxVoicePoolConfig) {
				EditorGUI.indentLevel += 1;
				GenIntField("Memoy Voices", "", ref initializer.atomConfig.hcaMxVoicePoolConfig.memoryVoices,        0, 1024);
				GenIntField("Streaming Voices", "", ref initializer.atomConfig.hcaMxVoicePoolConfig.streamingVoices, 0, 1024);
				EditorGUI.indentLevel -= 1;
			}
			
			showAtomIOSConfig = EditorGUILayout.Foldout(showAtomIOSConfig, "iOS Config");
			if (showAtomIOSConfig) {
				EditorGUI.indentLevel += 1;
				GenIntFieldWithUnit("Buffering Time", "[msec]", "Sound buffering time in msec.", ref initializer.atomConfig.iosBufferingTime, 16, 200);
				GenToggleField("Override iPod Music", "", ref initializer.atomConfig.iosOverrideIPodMusic);
				EditorGUI.indentLevel -= 1;
			}

			showAtomAndroidConfig = EditorGUILayout.Foldout(showAtomAndroidConfig, "Android Config");
			if (showAtomAndroidConfig) {
				EditorGUI.indentLevel += 1;
				{
					/* Ver.2.03.03 以前は 0 がデフォルト値だったことの互換性維持のための処理 */
					if (initializer.atomConfig.androidBufferingTime == 0) {
						initializer.atomConfig.androidBufferingTime = (int)(4 * 1000.0 / initializer.atomConfig.serverFrequency);
					}
					if (initializer.atomConfig.androidStartBufferingTime == 0) {
						initializer.atomConfig.androidStartBufferingTime = (int)(3 * 1000.0 / initializer.atomConfig.serverFrequency);
					}
				}
				GenIntFieldWithUnit("Buffering TimeWithUnit", "[msec]", "Sound buffering time in msec.", ref initializer.atomConfig.androidBufferingTime, 50, 500);
				GenIntFieldWithUnit("Start Buf TimeWithUnit", "[msec]", "Sound buffering time to start playing. This value will be applied when using the low latency voice pool.", ref initializer.atomConfig.androidStartBufferingTime, 50, 500);
				showAtomAndroidVoicePoolConfig = EditorGUILayout.Foldout(showAtomAndroidVoicePoolConfig, "Low Latency Standard Voice Pool Config");
				if (showAtomAndroidVoicePoolConfig) {
					EditorGUI.indentLevel += 1;
					GenIntField("Memory Voices", "", ref initializer.atomConfig.androidLowLatencyStandardVoicePoolConfig.memoryVoices,        0, 32);
					GenIntField("Streaming Voices", "", ref initializer.atomConfig.androidLowLatencyStandardVoicePoolConfig.streamingVoices, 0, 32);
					EditorGUI.indentLevel -= 1;
				}
				EditorGUI.indentLevel -= 1;
			}

		}
		EditorGUI.indentLevel -= 1;
		EditorGUILayout.EndToggleGroup();
		
		// Mana Config
		initializer.initializesMana = 
			EditorGUILayout.BeginToggleGroup("Initialize Mana", initializer.initializesMana);
		EditorGUI.indentLevel += 1;
		{
			GenIntField("Number Of Decoders", "", ref initializer.manaConfig.numberOfDecoders, 0, 128);
			GenIntField("Number Of Max Entries", "", ref initializer.manaConfig.numberOfMaxEntries, 0, 1024);
			GenToggleField("Enable CuePoint", "", ref initializer.manaConfig.enableCuePoint);
		}

		EditorGUI.indentLevel -= 1;
		EditorGUILayout.EndToggleGroup();

		GenToggleField("Dont Destroy On Load",    "", ref initializer.dontDestroyOnLoad);

	}
} // end of class

/* end of file */
