/**
	SpriteStudioPlayer
	
	Asset post processor and the database management
	
	Copyright(C) 2003-2013 Web Technology Corp. 
	
*/

//#define _BUILD_UNIFIED_SHADERS
//#define _ADD_SHADER_KEEPER
//#define _GENERATE_SHADERS

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class SsAssetPostProcessor : AssetPostprocessor
{
#if _GENERATE_SHADERS
	static private string _shaderTemplateFilename = "SsShaderTemplate.txt";
#endif
	static private bool	_s_rejectReenter = false;
	static System.StringComparison _pathComparison = System.StringComparison.CurrentCultureIgnoreCase;
	static private	SsAssetDatabase _database;
	
	private uint _version = 0;
	public override uint GetVersion() {return _version;}
		
	static SsAssetPostProcessor()
	{
		var go = GetDatabaseGo();
		if (!go)
		{
			go = CreateDatabaseGo();
			// get replaced database go.
			go = SaveDatabase(go);
		}
		_database = go.GetComponent<SsAssetDatabase>();
	}
	
	static public void
	OnPostprocessAllAssets(
		string[]	importedAssets,
		string[]	deletedAssets,
		string[]	movedAssets,
		string[]	movedFromAssetPaths)
	{
		// reject re-entering for files unnecessary to import during an another file is created internally.
		if (_s_rejectReenter)
		{
//			Debug.LogError("REJECT ->" + importedAssets[0]);
			return;
		}
		
		string dbFilename = Path.GetFileName(SsAssetDatabase.filePath);

		// ignore database import
		if (importedAssets.Length == 1
		&&	importedAssets[0].EndsWith(dbFilename, _pathComparison))
		{
#if _RELINK_DB_OBJECT_TO_PREFAB
			UpdateDatabaseObjectInHierarchy();
#endif
//			Debug.LogWarning("IGNORE Database");
			return;
		}

//		if (importedAssets.Length > 0)
//			Debug.Log("ENTER importing " + importedAssets[0] + " count: " + importedAssets.Length);

		// reject reenter this function while importing new generated files.
		_s_rejectReenter = true;

		try
		{
			AssetDatabase.Refresh();

			// get game object contains database
			GameObject databaseGo = GetDatabaseGo();

			bool createDatabase = false;
			// make database first to assure the existing.
			if (databaseGo == null)
				createDatabase = true;
			else
			{
				// ignore database delete and re-create it
				if (deletedAssets.Length > 0
				&&	deletedAssets[0].EndsWith(dbFilename, _pathComparison))
					createDatabase = true;
			}
			
			if (createDatabase)
			{
				// create database
//				Debug.LogWarning("Create Database: " + databaseGo);
				var go = CreateDatabaseGo();
				RebuildDatabase(go);
				databaseGo = SaveDatabase(go);
			}
			
			_database = databaseGo.GetComponent<SsAssetDatabase>();
			
			int prevAnimeListCount = _database.animeList.Count;
	
			// filter imported files and create assets as needed
			foreach	(var name in importedAssets)
			{
				// ignore database file in filename list
				if (name.EndsWith(dbFilename, _pathComparison) )
					continue;
					
#if _GENERATE_SHADERS
				if (name.EndsWith(_shaderTemplateFilename, _pathComparison))
				{
					BuildShaders(name);
				}
#endif
	
				if (name.EndsWith(".ssax", _pathComparison))
				{
					SsaxImporter v = SsaxImporter.Create(name);
					if (v != null)
					{
						var anm = v.AnimeRes;
						CreateSpritePrefab(anm);
						_database.AddAnime(anm);
					}
				}
			}
			
	 		// assets will be deleted
			foreach	(var name in deletedAssets)
			{
				if (!name.EndsWith(".asset", _pathComparison)) continue;
				Object asset = AssetDatabase.LoadAssetAtPath(name, typeof(SsAnimation));
				// remove animation element from list
				SsAnimation anim = asset as SsAnimation;
				if (anim == null) continue;
				_database.animeList.Remove(anim);
			}
			// cleanup null elements in the database
			_database.CleanupAnimeList();
	
			if (_database.animeList.Count != prevAnimeListCount)
			{
				// apply modification of database to actual file
				EditorUtility.SetDirty(databaseGo);
			}
			AssetDatabase.SaveAssets();
	
#if _RELINK_DB_OBJECT_TO_PREFAB
			// update game object refers to the database prefab
			UpdateDatabaseObjectInHierarchy();
#endif
		}
		catch
		{
//			Debug.LogWarning("ABORT importing");
			throw;
		}
		finally
		{
			_s_rejectReenter = false;
//			Debug.Log("LEAVE importing");
		}
	}
	
	//--------- shader builder
	static	private	SsShaderContainer	_shaderContainer;
		
	static	private string _shaderPath;

	// alpha blend command
	// followings are inserted in shader template to create derived shader
	private class _AlphaBlend
	{
		public	string	name;
		public	string	blendOp;
		public	string	blendMode;
		public	string	additive;
		public	string	fragAdditive;
		public _AlphaBlend(string n, string op, string b, string ad, string fad) {
			name = n; blendOp = op; blendMode = b; additive = ad; fragAdditive = fad;
		}
	}
	static private _AlphaBlend[]	_alphaBlends = new _AlphaBlend[]
	{
		// NONE
		new _AlphaBlend("NonAlpha",	null, "Off", null, ""),
		
		// src.rgb * src.a + dst.rgb * (1 - src.a) [FIX]
		new _AlphaBlend("MixAlpha",	null, "SrcAlpha OneMinusSrcAlpha", null, ""),
		
		// src.rgb * src.a * dst.rgb
#if true		
		// SrcColor must be multiplied by src.a before blending.
		new _AlphaBlend("MulAlpha",	null, "DstColor OneMinusSrcAlpha\n		AlphaTest NotEqual 0",
			"SetTexture [_MainTex] {combine previous * primary alpha, previous}",
			"col.rgb *= col.a;"),
#else
		// following is usable when BlendOp supports multiplication, besides doesn't need an extra SetTexture.
		//new _AlphaBlend("MulAlpha",	null, "SrcAlpha OneMinusSrcAlpha\n	AlphaTest NotEqual 0", null),
#endif

		// src.rgb * src.a + dst.rgb [possibly FIX]
		new _AlphaBlend("AddAlpha",	null, "SrcAlpha One", null, ""),
		
		// dst.rgb - src.rgb * src.a
 		new _AlphaBlend("SubAlpha",	"RevSub", "DstColor SrcAlpha\n		AlphaTest NotEqual 0", null, ""),
	};

	// vertex color blend command
	// followings are inserted in shader template to create derived shader
	private class _ColorBlend
	{
		public	string	name;
		public	string	setTexture_0;
		public	string	setTexture_1;
		public	string	property;
		public	string	cgCode;
		public _ColorBlend(string n, string st0, string st1, string p, string cg)
		{
			name = n;
			setTexture_0 = st0;
			setTexture_1 = st1;
			property = p;
			cgCode = cg;
		}
	}
	static private _ColorBlend[]	_colorBlends = new _ColorBlend[]
	{
		// use texture color only
		new _ColorBlend("NonColor",	"combine texture, texture * primary", "", "",
		                "col.rgb = tex.rgb;"),

#if false
		// not supported on OpenGL ES 1.x
		// vColor.rgb *= vColor.a, vColor.a = 1 - vColor.a
		// texture.rgb * vColor.a + vColor.rgb
		new _ColorBlend("MixColor",
						"combine primary * primary alpha, one - primary", 
						"combine texture * previous + previous, texture", ""),
#else
		// lerp vColor.rgb to texture.rgb along with vColor.a
		new _ColorBlend("MixColor",
		                "combine primary lerp(primary) texture, texture",
		                "", "",
		                "col.rgb = col.rgb * rate + tex.rgb * (1 - rate);"),
#endif
		
		// lerp vColor.rgb to 1,1,1 along with vColor.a
		new _ColorBlend("MulColor",
			"ConstantColor [_OneColor]\n	combine primary lerp (primary) constant, texture",
			"combine previous * texture, texture",
			"_OneColor (\"Constant Color(1,1,1,1)\", Color) = (1,1,1,1)",
		                "col.rgb = lerp(ONE_COLOR, col.rgb, rate) * tex.rgb;\n" +
		                "col.rgb *= col.a;"),
		
		// (vcol.rgb * vcol.a) + tex.rgb
		new _ColorBlend("AddColor",	"combine primary * primary alpha", "combine previous + texture, texture", "",
			"col.rgb = (col.rgb * rate) + tex.rgb;"),
		
		// tex.rgb - (vcol.rgb * vcol.a)
		new _ColorBlend("SubColor",	"combine primary * primary alpha", "combine texture - previous, texture", "",
			"col.rgb = tex.rgb - (col.rgb * rate);"),
	};
	
	// material color blend addition
	// followings are inserted in shader template to create derived shader
	private class _MaterialColorBlends
	{
		public	string	name;
		public	string	property;
		public	string	category;
		public	string	combiner;
		public	string	cgCode;
		public _MaterialColorBlends(string n, string p, string c, string co, string cg)
		{
			name = n;
			property = p;
			category = c;
			combiner = co;
			cgCode = cg;
		}
	}
	static private _MaterialColorBlends[]	_materialColorBlends = new _MaterialColorBlends[]
	{
		// no blending
		new _MaterialColorBlends("", "", "", "", ""),
		// multiply
		new _MaterialColorBlends(
			"MulMatCol",
			"_Color (\"Main Color\", Color) = (0.5,0.5,0.5,1)",
			"		Material {\n" +
			"			Diffuse [_Color]\n" +
			"			Ambient [_Color]\n" +
			"		}",
			"SetTexture [_MainTex] {constantColor [_Color] combine previous * constant DOUBLE, previous * constant}",
			"col.rgb *= _Color.rgb * 2;\n" +
			"col.a *= _Color.a;"
		)
	};

	/// build derived shaders from the template text of shader.
	static private void
	BuildShaders(string srcPath)
	{
		_shaderPath = Path.GetDirectoryName(srcPath);
		TextAsset ta = AssetDatabase.LoadAssetAtPath(srcPath, typeof(TextAsset)) as TextAsset;

		TextAsset cgShaderTemplateText = AssetDatabase.LoadAssetAtPath(_shaderPath + "/SsCgShaderTemplate.txt", typeof(TextAsset)) as TextAsset;

		// create gameObject as keeper of generated shaders
		var shaderPrefab = new PrefabricatableObject("SpriteStudio/Shaders", "__SsShaderKeeper_DoNotDeleteMe");
		_shaderContainer = shaderPrefab.GetOrAddComponent<SsShaderContainer>();
		if (!_shaderContainer)
		{
			Debug.LogError("Fatal error!!: cannot create shader keeper. try to reimport all.");
			return;
		}
		
		// to be invisible
		_shaderContainer.gameObject.hideFlags = HideFlags.NotEditable;//|HideFlags.HideInHierarchy;
		// clear shader list
		_shaderContainer._shaders.Clear();
			
		// create all the variations
		foreach (var ce in _colorBlends)
			foreach (var ae in _alphaBlends)
				foreach (var mce in _materialColorBlends)
					BuildShader(ta.text, ce.name == "NonColor" ? "" : cgShaderTemplateText.text, ce, ae, mce);
		
#if _BUILD_UNIFIED_SHADERS
		// create color blend unified shaders
		{
			string unifiedSrcPath = srcPath;
			unifiedSrcPath = unifiedSrcPath.Replace(_shaderTemplateFilename, "SsUnifiedShaderTemplate.txt");
			_shaderPath = Path.GetDirectoryName(unifiedSrcPath);
			ta = AssetDatabase.LoadAssetAtPath(unifiedSrcPath, typeof(TextAsset)) as TextAsset;
			
			foreach (var ae in _alphaBlends)
				BuildUnifiedShader(ta.text, ae);
		}
#endif
		shaderPrefab.Close(true);
	}

	/// build derived shader with its name and blend mode specified.
	static private void
	BuildShader(
		string 					srcShaderText,
		string					betterShader,
		_ColorBlend				colorBlend,
		_AlphaBlend				alphaBlend,
		_MaterialColorBlends	matColorBlend)
	{
		string newShaderText = string.Copy(srcShaderText);
		
		// make shader name by combination of each blend method
		string newShaderName = colorBlend.name + alphaBlend.name + matColorBlend.name;
		newShaderText = newShaderText.Replace("%SHADER_NAME%", newShaderName);

		// add properties
		newShaderText = newShaderText.Replace("%COLOR_PROPERTY%", matColorBlend.property);
		newShaderText = newShaderText.Replace("%PROPERTY%", colorBlend.property);
		
		// add material color setting in Category area
		newShaderText = newShaderText.Replace("%MATERIAL%", matColorBlend.category);

		// add alpha blend setting in Category area
		newShaderText = newShaderText.Replace("%BLEND_ARGUMENTS%", alphaBlend.blendMode);
		newShaderText = newShaderText.Replace("%BLEND_OP%",
			alphaBlend.blendOp == null ? "" : "BlendOp " + alphaBlend.blendOp);

		// add better shader written in Cg as SubShader
		newShaderText = newShaderText.Replace("%BETTER_SHADER%", betterShader);
		
		// add extra SetTexture depending on alpha/color blend method
		newShaderText = newShaderText.Replace("%SET_TEXTURE_0%", "SetTexture [_MainTex] {" + colorBlend.setTexture_0 + "}");
		string replaced = colorBlend.setTexture_1;
		if (replaced.Length > 0)
			replaced = "SetTexture [_MainTex] {" + replaced + "}";
		newShaderText = newShaderText.Replace("%SET_TEXTURE_1%", replaced);
		
		// add material color combiner
		newShaderText = newShaderText.Replace("%SET_TEXTURE_2%", matColorBlend.combiner);

		// add extra combiner for multiple alpha blending
		string additive = colorBlend.name == "NonColor" ? alphaBlend.additive : "";
		newShaderText = newShaderText.Replace("%ADDITIVE%", additive);

		// add something needed in fragment shader
		newShaderText = newShaderText.Replace("%COLOR_BLEND_FUNC%", colorBlend.cgCode);
		newShaderText = newShaderText.Replace("%FRAG_MAT_COLOR%", matColorBlend.cgCode);
		newShaderText = newShaderText.Replace("%FRAG_ADDITIVE%", alphaBlend.fragAdditive);

		// create file as .shader text file
		string shaderFilePath = _shaderPath + "/_Ss" + newShaderName + ".shader";
		File.WriteAllText(shaderFilePath, newShaderText);//, SsEncoding.SJIS);
		AssetDatabase.ImportAsset(shaderFilePath);
		
		// add to container.
		_shaderContainer._shaders.Add(AssetDatabase.LoadAssetAtPath(shaderFilePath, typeof(Shader)) as Shader);
	}

#if _BUILD_UNIFIED_SHADERS
	/// build derived shaders from the template text of shader.
	static private void
	BuildUnifiedShader(
		string 		srcShaderText,
		_AlphaBlend	alphaBlend)
	{
		string newShaderText = string.Copy(srcShaderText);
		string newShaderName = "UniColor" + alphaBlend.name;
		newShaderText = newShaderText.Replace("%SHADER_NAME%", newShaderName);
		newShaderText = newShaderText.Replace("%BLEND_ARGUMENTS%", alphaBlend.blendMode);
		
		// create file as .shader text file
		string shaderFilePath = _shaderPath + "/_Ss" + newShaderName + ".shader";
		File.WriteAllText(shaderFilePath, newShaderText);//, SsEncoding.SJIS);
		AssetDatabase.ImportAsset(shaderFilePath);
		
		// add to container.
		_shaderContainer._shaders.Add(AssetDatabase.LoadAssetAtPath(shaderFilePath, typeof(Shader)) as Shader);
	}
#endif
	
	static public void
	AddShaderKeeperToCurrentScene()
	{
#if _ADD_SHADER_KEEPER
		// we don't check exsiting because it needs that we create prefab and exit this function once then import it and get instance of it!!
//		if (PrefabricatableObject.Exists("SpriteStudioPrefabs", "__SsShaderKeeper_DoNotDeleteMe"))
//			BuildShaders();
		//Object obj = FindObjectOfType(typeof(SsShaderContainer));
		
		//SsTimer.StartTimer();
		GameObject obj = GameObject.Find("__SsShaderKeeper_DoNotDeleteMe");
		//SsTimer.EndTimer("finding shader keeper");
		if (obj)
		{
			if (PrefabUtility.GetPrefabType(obj) == PrefabType.MissingPrefabInstance)
			{
				// missing, so delete it then restore
				GameObject.DestroyImmediate(obj);
			}
			else
			{
				return;
			}
		}

		//SsTimer.StartTimer();
		GameObject go = PrefabricatableObject.LoadPrefab("SpriteStudio/Shaders", "__SsShaderKeeper_DoNotDeleteMe");
		EditorUtility.InstantiatePrefab(go);
		//SsTimer.EndTimer("instantiating shader keeper");
#endif
	}
	
	//--------- asset database for sprite studio resources. (save as prefab)
	static public GameObject
	GetDatabaseGo()
	{
		var go = (GameObject)AssetDatabase.LoadAssetAtPath(SsAssetDatabase.filePath, typeof(GameObject));
		return go;
	}

	static public GameObject
	CreateDatabaseGo()
	{
		GameObject go = new GameObject();
		go.AddComponent<SsAssetDatabase>();
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
		go.SetActive(false);
#else
		go.active = false;
#endif
		return go;
	}
	
	static public SsAssetDatabase
	GetDatabase()
	{
#if false
		// ensure the database is present
		if (!_database)
		{
			// get or create game object of database
			GameObject databaseGo = GetDatabaseGo();
			if (databaseGo == null)
			{
				databaseGo = CreateDatabaseGo();
				RebuildDatabase(databaseGo);
				SaveDatabase(databaseGo);
			}
			_database = databaseGo.GetComponent<SsAssetDatabase>();
		}
#endif
		return _database;
	}
	
	static public void
	RebuildDatabase(GameObject go)
	{
		// list up all assets from root so far...
		List<string> allAssets = new List<string>();
		Stack<string> paths = new Stack<string>();
		paths.Push(Application.dataPath);
		while (paths.Count != 0)
		{
			string path = paths.Pop();
			string[] files = Directory.GetFiles(path, "*.asset");
			foreach (var e in files)
				allAssets.Add(e.Substring(Application.dataPath.Length - 6));
			foreach (string e in Directory.GetDirectories(path)) 
				paths.Push(e);
		}	

		var database = go.GetComponent<SsAssetDatabase>();
		foreach (var path in allAssets)
		{
			// add only SsAnimation to the list
			SsAnimation e = AssetDatabase.LoadAssetAtPath(path, typeof(SsAnimation)) as SsAnimation;
			if (e != null)
			{
				database.AddAnime(e);
				CreateSpritePrefab(e);
			}
		}
	}
	
	static public GameObject
	SaveDatabase(GameObject go)
	{
		// save prefab into file
		Object p = PrefabUtility.CreateEmptyPrefab(SsAssetDatabase.filePath);
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
		var createdGo = PrefabUtility.ReplacePrefab(go, p, ReplacePrefabOptions.Default);
#else
		var createdGo = EditorUtility.ReplacePrefab(go, p, ReplacePrefabOptions.Default);
#endif
		GameObject.DestroyImmediate(go);
		return createdGo;
	}
	
	static void
	CreateSpritePrefab(SsAnimation anime)
	{
		var po = new PrefabricatableObject("SpriteStudioPrefabs", anime.name);
		SsSprite spr = po.GetOrAddComponent<SsSprite>();
		spr.Animation = null;	// force to update when the same animation attached
		spr.Animation = anime;
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
		spr.gameObject.SetActive(true);
#endif
		po.Close(true);
	}

	static public void
	CleanupSpritePrefabs()
	{
		// list up all assets from root so far...
		List<string> allAssets = new List<string>();
		Stack<string> paths = new Stack<string>();
		paths.Push(Application.dataPath + "/SpriteStudioPrefabs/");
		while (paths.Count != 0)
		{
			string path = paths.Pop();
			string[] files = Directory.GetFiles(path, "*_ssa.prefab");
			foreach (var e in files)
				allAssets.Add(e.Substring(Application.dataPath.Length - 6));
			foreach (string e in Directory.GetDirectories(path)) 
				paths.Push(e);
		}	

		foreach (var path in allAssets)
		{
			// delete spite prefab which has no longer animation.
			SsSprite e = AssetDatabase.LoadAssetAtPath(path, typeof(SsSprite)) as SsSprite;
			if (e.Animation == null)
			{
				Debug.Log(e + " was removed");
				AssetDatabase.DeleteAsset(path);
			}
		}
	}
}

public class PrefabricatableObject
{
	string	prefabPath;
	public	GameObject	gameObject;

	static public GameObject
	LoadPrefab(string aPath, string name)
	{
		string pefabDir = "Assets/" + aPath;
		string path = pefabDir + "/" + name + ".prefab";
		if (!Directory.Exists(pefabDir)) return null;
		return (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
	}

	static public bool
	Exists(string path, string name)
	{
		return LoadPrefab(path, name) != null;
	}
	
	public PrefabricatableObject(string path, GameObject go)
	{
		gameObject = go;
		prefabPath = "Assets/" + path + "/" + go.name + ".prefab";
	}

	public PrefabricatableObject(string path, string name)
	{
//		Debug.Log("PrefabricatableObject() " + path + "/" + name);
		string pefabDir = "Assets/" + path;
		prefabPath = pefabDir + "/" + name + ".prefab";
		if (!Directory.Exists(pefabDir))
		{
			AssetDatabase.CreateFolder("Assets", path);
		}
		// try to get the exsiting prefab
		gameObject = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
		if (!gameObject)
		{
			gameObject = new GameObject(name);
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
			gameObject.SetActive(false);
#else
			gameObject.active = false;
#endif
//			Debug.LogError("Create new GameObject: " + name);
		}
	}
	public T
	GetOrAddComponent<T>() where T: Component
	{
		T c = gameObject.GetComponent<T>();
		if (!c)
			c = gameObject.AddComponent<T>();
		return c;
	}
	
	public void
	Close(bool applyFileWhenUpdate)
	{
//		Debug.LogWarning(gameObject.name + "'s prefabType: " + PrefabUtility.GetPrefabType(gameObject));
		if (PrefabUtility.GetPrefabType(gameObject) == PrefabType.None)
		{
//			Debug.LogError("prefabalize " + prefabPath);
			// prefabalize
			Object tmp = PrefabUtility.CreateEmptyPrefab(prefabPath);
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
			PrefabUtility.ReplacePrefab(gameObject, tmp, ReplacePrefabOptions.Default);
#else
			EditorUtility.ReplacePrefab(gameObject, tmp, ReplacePrefabOptions.Default);
#endif
			GameObject.DestroyImmediate(gameObject);
		}
		else if (applyFileWhenUpdate)
		{
//			Debug.Log("apply to file " + prefabPath);
			// apply to actual file
			EditorUtility.SetDirty(gameObject);
			AssetDatabase.SaveAssets();
		}

		prefabPath = null;
		gameObject = null;
	}
}
