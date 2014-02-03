/**
	SpriteStudioPlayer
	
	Sprite Studio menu
	
	Copyright(C) 2003-2013 Web Technology Corp. 
	
*/

using UnityEngine;
using UnityEditor;

public class SsEditor : Editor
{
	// Add main menu newly
    [MenuItem ("SpriteStudio/Create Sprite")]
    static void
	SSMenu_CreateSprite()
	{
		CreateSprite();
    }

	// Add Database under main menu
	[MenuItem ("SpriteStudio/Create Database")]
    static void
	SSMenu_CreateDatabase(MenuCommand command)
	{
		SsAssetDatabase.CreateNewObject();
    }

	// Add this under main menu
	[MenuItem ("SpriteStudio/Cleanup Prefabs")]
    static void
	SSMenu_CleanupPrafabs(MenuCommand command)
	{
		SsAssetPostProcessor.CleanupSpritePrefabs();
	}

	// Add GamreObject menu
	[MenuItem ("GameObject/Create Other/SpriteStudio/Sprite")]
    static void
	GameObject_CreateSprite(MenuCommand command)
	{
		CreateSprite();
    }
	
	// Add GamreObject menu
	[MenuItem ("GameObject/Create Other/SpriteStudio/Database")]
    static void
	GameObject_CreateDatabase(MenuCommand command)
	{
		SsAssetDatabase.CreateNewObject();
    }
	
	// create sprite
	static public GameObject
	CreateSprite()
	{
		var go = new GameObject("New Sprite");
		go.AddComponent<SsSprite>();
		// add shader keeper to current scene if it doesn't exist.
		SsAssetPostProcessor.AddShaderKeeperToCurrentScene();
		return go;
	}
}
