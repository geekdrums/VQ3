/**
	SpriteStudioPlayer

	Sprite inspector
	
	Copyright(C) 2003-2013 Web Technology Corp. 
	
*/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SsSprite))]
public class SsSpriteEditor : Editor
{
	[HideInInspector]	static public	SsSprite	LastSprite;
	static GameObject databaseGo;
	
	SsAssetDatabase _database;
	string[]		_animeNames;
	int				_selectedAnimeIndex;
	SsSprite		_sprite;
	float			_animeFrame;
	int				_startAnimeFrame;
	int				_endAnimeFrame;
	bool			_hFlip;
	bool			_vFlip;
	bool			_drawBoundingBox;
	bool			_changed;

	List<SsSubAnimeController>	_subAnimations = null;
	
	void OnEnable()
	{
//		SsTimer.StartTimer();
		_sprite = target as SsSprite;

		// add shader keeper if it doesn't exist during show the sprite object substance.
		PrefabType prefabType = PrefabUtility.GetPrefabType(_sprite);
		if (_sprite != LastSprite
		&&	prefabType != PrefabType.Prefab)
		{
			//SsTimer.StartTimer();
			SsEditorWindow.AddShaderKeeper();
			//SsTimer.EndTimer("checking or add shader keeper");
		}
			
		_animeFrame = _sprite._animeFrame;
		_startAnimeFrame = _sprite._startAnimeFrame;
		_endAnimeFrame = _sprite._endAnimeFrame;
		_hFlip = _sprite.hFlip;
		_vFlip = _sprite.vFlip;
		_drawBoundingBox = _sprite.DrawBoundingBox;

		_subAnimations = null;//_sprite.subAnimations;

		// get latest animation list
//		SsTimer.StartTimer();
		if (!databaseGo)
			databaseGo = SsAssetPostProcessor.GetDatabaseGo();
//		SsTimer.EndTimer("load database asset");

		if (databaseGo)
		{
			//SsTimer.StartTimer();
			_database = databaseGo.GetComponent<SsAssetDatabase>();
			List<SsAnimation> animeList = _database.animeList;
			_animeNames = new string[animeList.Count + 1];
			for (int i = 0; i < animeList.Count; ++i)
				_animeNames[i + 1] = animeList[i].name;
			System.Array.Sort(_animeNames, 1, _animeNames.Length - 1);
			_animeNames[0] = "<none>";
			
			// get the index of this animation in the list
			if (_sprite.Animation)
			{
				string myAnimeName = _sprite.Animation.name;
				for (int i = 1; i < _animeNames.Length; ++i)
				{
					if (myAnimeName == _animeNames[i])
					{
					    _selectedAnimeIndex = i;
						break;
					}
				}
			}
			//SsTimer.EndTimer("make anime list");
		}
		else
		{
			Debug.LogError("Not found animation list: '" + SsAssetDatabase.filePath + "' needs to reimport animation data");
		}
		LastSprite = _sprite;
//		SsTimer.EndTimer("SsSpriteEditor.OnEnable()");
	}
	
	public override void OnInspectorGUI()
	{
		if (_animeNames == null) return;

		// display animation list
		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Animation");
			int newAnimeIndex = EditorGUILayout.Popup(_selectedAnimeIndex, _animeNames/*, GUILayout.ExpandHeight(true), GUILayout.Height(30)*/);
			bool pushedGotoAnime = GUILayout.Button("Edit...", GUILayout.Width(40), GUILayout.Height(15));
		EditorGUILayout.EndHorizontal();
		if (newAnimeIndex != _selectedAnimeIndex)
		{
			// change animation
			_sprite.Animation = (newAnimeIndex == 0 ? null : _database.GetAnime(_animeNames[newAnimeIndex]));
			_sprite.ResetAnimationStatus();
			_changed = true;
		}

		if (_sprite.Animation)
		{
		    if (pushedGotoAnime)
			{
				EditorGUIUtility.PingObject(_sprite.Animation);
				Selection.activeObject = _sprite.Animation;
			}
		}

		// cannot update prefab because SsSprite modifies transform and creates some transoforms as its children.
		PrefabType prefabType = PrefabUtility.GetPrefabType(_sprite);
		if (prefabType != PrefabType.Prefab)
		{
			if (_sprite.Animation)
			{
				// Show default inspector property editor
		        DrawDefaultInspector();
				
				if (_sprite.subAnimations != _subAnimations)
				{
					_subAnimations = _sprite.subAnimations;
					_changed = true;
				}

				// clamp values
				if (_sprite.PlayCount < 0)
					_sprite.PlayCount = 0;
 				
				_sprite._startAnimeFrame = Mathf.Clamp(_sprite._startAnimeFrame, 0, _sprite._endAnimeFrame);
				_sprite._endAnimeFrame = Mathf.Clamp(_sprite._endAnimeFrame, _sprite._startAnimeFrame, _sprite.Animation.EndFrame);
				if (_sprite._endAnimeFrame < 0)
					_sprite._endAnimeFrame = 0;
				_sprite._animeFrame = Mathf.Clamp(_sprite._animeFrame, _sprite._startAnimeFrame, _sprite._endAnimeFrame);

				// change play direction
				if (_sprite.PlayDirection != _sprite._prevPlayDirection)
				{
					_sprite._prevPlayDirection = _sprite.PlayDirection;
					// reset animation status
					_sprite.ResetAnimationStatus();
					_changed = true;
				}
				
				if (_sprite._startAnimeFrame != _startAnimeFrame)
				{
					_startAnimeFrame = _sprite._startAnimeFrame;
					_sprite._animeFrame = _startAnimeFrame;	// show animation at the selected frame
					_changed = true;
				}
				
				if (_sprite._endAnimeFrame != _endAnimeFrame)
				{
					_endAnimeFrame = _sprite._endAnimeFrame;
					_sprite._animeFrame = _endAnimeFrame;	// show animation at the selected frame
					_changed = true;
				}

				if (!Application.isPlaying
				&&	_sprite._animeFrame != _animeFrame)
				{
					_animeFrame = _sprite._animeFrame;
					_changed = true;
				}
				
				if (_sprite.hFlip != _hFlip)
				{
					_hFlip = _sprite.hFlip;
					_changed = true;
				}
	
				if (_sprite.vFlip != _vFlip)
				{
					_vFlip = _sprite.vFlip;
					_changed = true;
				}
				
				if (_sprite.DrawBoundingBox != _drawBoundingBox)
				{
					_drawBoundingBox = _sprite.DrawBoundingBox;
					_changed = true;
				}
			}
		}
		if (_changed)
		{
			if (_sprite._animeFrame < 0)
				_sprite._animeFrame = 0;
			_sprite.UpdateVertex();
			_sprite.UpdateAlways();
			_changed = false;
		}
	}
}
