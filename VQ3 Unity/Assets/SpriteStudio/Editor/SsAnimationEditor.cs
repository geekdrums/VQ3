/**
	SpriteStudioPlayer
	
	Animation inspector
	
	Copyright(C) 2003-2013 Web Technology Corp. 
	
*/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SsAnimation))]
public class SsAnimationEditor : Editor
{
	SsAnimation		_anime;
	SsAssetDatabase	_database;
	void OnEnable()
	{
		_anime = target as SsAnimation;
//		_database = SsAssetPostProcessor.GetDatabase();
//		if (!_database)
//		{
//			Debug.LogWarning("SpriteStudioDatabase not found");
//		}
	}
	
	public override void OnInspectorGUI()
	{
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Reimport"))
		{
			AssetDatabase.ImportAsset(_anime.OriginalPath);
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		{
			_anime.UseScaleFactor = EditorGUILayout.BeginToggleGroup("ScaleFactor", _anime.UseScaleFactor);
			if (_anime.UseScaleFactor)
				_anime.ScaleFactor = EditorGUILayout.FloatField(_anime.ScaleFactor);
			else
			{
				if (!_database)
					_database = SsAssetPostProcessor.GetDatabase();
				EditorGUILayout.FloatField(_database.ScaleFactor);
			}
			EditorGUILayout.EndToggleGroup();
		}
		EditorGUILayout.EndHorizontal();

		DrawDefaultInspector();
	}
}
