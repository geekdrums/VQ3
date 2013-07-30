/****************************************************************************
 *
 * CRI Middleware SDK
 *
 * Copyright (c) 2011-2012 CRI Middleware Co.,Ltd.
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
	private void GenToggleField(string label_str, ref bool field_value)
	{
		field_value = EditorGUILayout.Toggle(label_str, field_value);
	}
	
	private void GenIntField(string label_str, ref int field_value, int min, int max)
	{
		field_value = Math.Min(max, Math.Max(min, EditorGUILayout.IntField(label_str, field_value, GUILayout.MaxWidth(200))));
	}
	
	private void GenPositiveFloatField(string label_str, ref float field_value, float min, float max)
	{
		field_value = Math.Min(max, Math.Max(min, EditorGUILayout.FloatField(label_str, field_value, GUILayout.MaxWidth(200))));
	}
	
	private void GenStringField(string label_str, ref string field_value)
	{
		field_value = EditorGUILayout.TextField(label_str, field_value);
	}

	static private bool showAtomStandardVoicePoolConfig = false;
	static private bool showAtomHcaMxVoicePoolConfig    = false;
	static private bool showAtomIOSConfig               = false;

	public override void OnInspectorGUI()
	{
		CriWareInitializer initializer = target as CriWareInitializer;
		
		// FileSystem Config
		initializer.initializesFileSystem = 
			EditorGUILayout.BeginToggleGroup("Initialize FileSystem", initializer.initializesFileSystem);
		EditorGUI.indentLevel += 1;
		{	
			GenIntField("Number of Loaders",        ref initializer.fileSystemConfig.numberOfLoaders,    0,  128);
			GenIntField("Number of Binders",        ref initializer.fileSystemConfig.numberOfBinders,    0,  128);
			GenIntField("Number of Installers",     ref initializer.fileSystemConfig.numberOfInstallers, 0,  128);
			GenIntField("Install Buffer Size [kB]", ref initializer.fileSystemConfig.installBufferSize,  32, int.MaxValue);
			GenStringField("User Agent String",     ref initializer.fileSystemConfig.userAgentString);
		}
		EditorGUI.indentLevel -= 1;
		EditorGUILayout.EndToggleGroup();
		
		// Atom Config
		initializer.initializesAtom = 
			EditorGUILayout.BeginToggleGroup("Initialize Atom", initializer.initializesAtom);
		EditorGUI.indentLevel += 1;
		{
			GenStringField("ACF File Name",           ref initializer.atomConfig.acfFileName);
			GenIntField("Max Virtual Voices",         ref initializer.atomConfig.maxVirtualVoices,      0,    1024);
			GenIntField("Sampling Rate [Hz]",         ref initializer.atomConfig.outputSamplingRate,    0,    192000);
			GenPositiveFloatField("Server Frequency [FPS]", ref initializer.atomConfig.serverFrequency, 15.0f, 120.0f);
			GenToggleField("Uses In Game Preview",    ref initializer.atomConfig.usesInGamePreview);
			
			showAtomStandardVoicePoolConfig = EditorGUILayout.Foldout(showAtomStandardVoicePoolConfig, "Standard Voice Pool Config");
			if (showAtomStandardVoicePoolConfig) {
				EditorGUI.indentLevel += 1;
				GenIntField("Memoy Voices", ref initializer.atomConfig.standardVoicePoolConfig.memoryVoices,        0, 1024);
				GenIntField("Streaming Voices", ref initializer.atomConfig.standardVoicePoolConfig.streamingVoices, 0, 1024);
				EditorGUI.indentLevel -= 1;
			}
			
			showAtomHcaMxVoicePoolConfig = EditorGUILayout.Foldout(showAtomHcaMxVoicePoolConfig, "HCA-MX Voice Pool Config");
			if (showAtomHcaMxVoicePoolConfig) {
				EditorGUI.indentLevel += 1;
				GenIntField("Memoy Voices", ref initializer.atomConfig.hcaMxVoicePoolConfig.memoryVoices,        0, 1024);
				GenIntField("Streaming Voices", ref initializer.atomConfig.hcaMxVoicePoolConfig.streamingVoices, 0, 1024);
				EditorGUI.indentLevel -= 1;
			}
			
			showAtomIOSConfig = EditorGUILayout.Foldout(showAtomIOSConfig, "iOS Config");
			if (showAtomIOSConfig) {
				EditorGUI.indentLevel += 1;
				GenIntField("Buffering Time [ms]", ref initializer.atomConfig.iosBufferingTime, 50, 200);
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
			GenIntField("Number Of Decoders", ref initializer.manaConfig.numberOfDecoders, 0, 128);
		}
		EditorGUI.indentLevel -= 1;
		EditorGUILayout.EndToggleGroup();

		GenToggleField("Dont Destroy On Load",    ref initializer.dontDestroyOnLoad);

	}
} // end of class

/* end of file */