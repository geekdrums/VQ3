/**
	SpriteStudioPlayer

	Database inspector
	
	Copyright(C) 2003-2013 Web Technology Corp. 
	
*/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SsAssetDatabase))]
public class SsAssetDatabaseEditor : Editor
{
	SsAssetDatabase		_target;

	void OnEnable()
	{
		_target = target as SsAssetDatabase;
	}

	public override void OnInspectorGUI()
	{
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.LabelField("AnimeList", (string)null, GUILayout.MaxWidth(60));
			if (GUILayout.Button("Pickup"))
			{
				Undo.RegisterUndo(_target, "Pickup");
				// trim
				_target.CleanupAnimeList();
				// find all anims attached to sprites in scene
				var sprites = FindObjectsOfType(typeof(SsSprite)) as SsSprite[];
				foreach (var e in sprites)
				{
					SsAnimation anm = e.Animation;
					if (!anm) continue;
					_target.AddAnime(anm);
				}
			}
			if (GUILayout.Button("Add"))
			{
				Undo.RegisterUndo(_target, "Add");
				_target.animeList.Add(null);
			}
			if (GUILayout.Button("Clear"))
			{
				Undo.RegisterUndo(_target, "Clear");
				_target.animeList.Clear();
			}
			if (PrefabUtility.GetPrefabType(_target) == PrefabType.Prefab
			||	PrefabUtility.GetPrefabType(_target) == PrefabType.PrefabInstance)
			{
				if (GUILayout.Button("Instantiate"))
				{
					var obj = Instantiate(_target);
					Undo.RegisterCreatedObjectUndo(obj, "Instantiate");
					obj.name = SsAssetDatabase.fileName;
				}
			}
		}
		EditorGUILayout.EndHorizontal();

		DrawDefaultInspector();
	}
}
