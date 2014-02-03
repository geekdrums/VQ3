/**
	SpriteStudioPlayer
	
	Settings / About window and about shader keeper addition...
	
	Copyright(C) 2003-2013 Web Technology Corp. 
	
*/

//#define _BUILD_UNIFIED_SHADERS

using UnityEngine;
using UnityEditor;

// this class exists to catch creating SsSprite
public class SsEditorWindow : EditorWindow
{
	static SsAssetDatabase	_database;
	static SsSprite			_lastSprite;

	[MenuItem("SpriteStudio/Settings...")]
	static  public void Init()
	{
		EditorWindow.GetWindowWithRect<SsEditorWindow>(new Rect(0,0,400,100), true, "SpriteStudio Settings");
		// get current settings from database.
		_database = SsAssetPostProcessor.GetDatabase();
	}

	public void OnGUI()
	{
		if (!_database)
		{
			_database = SsAssetPostProcessor.GetDatabase();
		}
			
		GUILayout.Label("Import Settings", EditorStyles.boldLabel);
		{
#if _BUILD_UNIFIED_SHADERS
			_database.UseUnifiedShader = EditorGUILayout.Toggle("Use UnifiedShader", _database.UseUnifiedShader);
#endif
			_database.ScaleFactor = EditorGUILayout.FloatField("Scale Factor", _database.ScaleFactor);
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Interpret angle curve parameter as radian", GUILayout.Width(250));
				_database.AngleCurveParamAsRadian = EditorGUILayout.Toggle(_database.AngleCurveParamAsRadian);
			EditorGUILayout.EndHorizontal();
		}
		GUILayout.Space(12);
		if (GUILayout.Button("Close"))
		{
			Close();
		}
    }
	
	public void OnDestroy()
	{
		EditorUtility.SetDirty(_database.gameObject);
	}

//	public void OnHierarchyChange()
//	{
//		Debug.LogWarning("OnHierarchyChange()");
//		AddShaderKeeper();
//	}

//	public void OnSelectionChange()
//	{
//		Debug.LogWarning("OnSelectionChange()");
//		AddShaderKeeper();
//	}
	
	static public void AddShaderKeeper()
	{
		// if null or not changed or prefab is ignored.
		if (!SsSpriteEditor.LastSprite) return;
		if (SsSpriteEditor.LastSprite == _lastSprite) return;
		if (PrefabUtility.GetPrefabType(SsSpriteEditor.LastSprite.gameObject) == PrefabType.Prefab) return;
		
		// it seems to be added new sprite possibly...
		_lastSprite = SsSpriteEditor.LastSprite;
		//Debug.Log("sprite added to this scene!!" + _lastSprite);
		
		// add shader keeper to current scene if it doesn't exist.
		SsAssetPostProcessor.AddShaderKeeperToCurrentScene();
	}

	[MenuItem("SpriteStudio/About")]
	static public void AboutSpriteStudio()
	{
		EditorUtility.DisplayDialog("About SpriteStudioPlayer",
		                            "SpriteStudioPlayer Version 1.27f1\n" +
		                            "Ssax File Version " + SsVersion.ToString(SsaxImporter.CurrentVersion) + "\n" +
		                            "Copyright(C) 2003-2013 Web Technology Corp.",
		                            "Ok");
	}
}
