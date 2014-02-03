/**
	SpriteStudioPlayer
	
	An animation importer
	
	Copyright(C) 2003-2013 Web Technology Corp. 
	
*/

//#define _BUILD_UNIFIED_SHADERS
//#define _FORCE_BOUND_PART_TO_MOST_TOP
//#define _FORCE_BOUND_PART_TO_BE_TRANSPARENT

/// for debug print
//#define _DEBUG

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class SsaxImporter
{
	public const int RequiredVersion = 0x40001;
	public const int CurrentVersion = 0x40002;

	// constants
	private const float PI = 3.14159265f;
	private const int	SSIO_DEF_DENOMINATOR = 1000;
	private const float	SSIO_SUCCEED_DENOMINATOR = 10000f;

	// settings
	private bool	_precalcAttrValues = true;
#if NEED_EDIT_MODE
	private int 	_editMode;	// [NOT USED] 0:key frame base. 1: full key.
#endif
	// temps
	public	SsAnimation	AnimeRes {get {return _anmRes;}}
	SsAnimation	_anmRes;
	string	_text;
	string	_relPath;	///< the relative path of this .ssax file under "Assets/..."
	XmlNamespaceManager _nsMgr;
	SsAssetDatabase	_database;

	// to just fend off the warning about Unused variable created by new.
	static public	SsaxImporter
	Create(string srcPath)
	{
		var inst = new SsaxImporter();
		if (!inst.Load(srcPath))
			return null;
		return inst;
	}
	
	bool
	Load(string srcPath)
	{
		_database = SsAssetPostProcessor.GetDatabase();
		string path = Path.GetDirectoryName(srcPath);

		// new path name of ss anime asset
		string newPath = path + "/assets/" + Path.GetFileNameWithoutExtension(srcPath) + "_ssa.asset";

		// identify the encoding which is Shift-JIS or UTF-8
		Encoding encode = SsEncoding.SJIS;
		{
			StreamReader sr = File.OpenText(srcPath);
			string xmlHeaderText = sr.ReadLine();
			// XmlDocument requires one node at least and also needs to get "encoding" attribute.
			string dummy = System.String.Copy(xmlHeaderText);
			dummy = dummy.Replace("<?xml", "<Dummy");
			dummy = dummy.Replace("?>", "/>");
			xmlHeaderText += dummy;
			sr.Close();
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xmlHeaderText);
			XmlNode node = doc.FirstChild.NextSibling;
			string encStr = node.Attributes["encoding"].Value;
			if (encStr == "shift_jis")
				encode = SsEncoding.SJIS;
			else if (encStr == "utf-8")
				encode = SsEncoding.UTF8;
		}

		// to read SJIS encoded text correctly.
		string textAll = File.ReadAllText(srcPath, encode);
		
		// create directory if it doesn't exist.
		bool newAsset = false;
		if (!Directory.Exists(path + "/assets"))
		{
			AssetDatabase.CreateFolder(path, "assets");
		}
		else
		{
			// try to open existing asset
			_anmRes = AssetDatabase.LoadAssetAtPath(newPath, typeof(SsAnimation)) as SsAnimation;
		}
		
		if (_anmRes == null)
		{
			// create new asset
			_anmRes = ScriptableObject.CreateInstance<SsAnimation>();
			newAsset = true;
		}
		else
		{
			// update existing asset
			ClearPreviousMaterials();
			_anmRes.ImageList = null;
			_anmRes.PartList = null;
			System.GC.Collect();
		}
		
		// update imported time
		_anmRes.UpdateImportedTime();

		// save original resource path
		_anmRes.OriginalPath = srcPath;
		
		// save scale factor at this import.
		if (!_anmRes.UseScaleFactor)
			_anmRes.ScaleFactor = _database.ScaleFactor;
		
#if _BUILD_UNIFIED_SHADERS
		if (_anmRes._UseUnifiedShader == UseUnifiedShaderEnum.Default)
		{
			// use global setting
			_anmRes.UseUnifiedShader = _database.UseUnifiedShader;
		}
		else
		{
			// use own setting
			_anmRes.UseUnifiedShader = (_anmRes._UseUnifiedShader == UseUnifiedShaderEnum.Yes);
		}
#endif

		if (!LoadXml(path, textAll))
		{
			Debug.LogError("Failed to import animation data: " + srcPath);
			return false;
		}
		
		if (newAsset)
			AssetDatabase.CreateAsset(_anmRes, newPath);
		else
		{
			// modification done internally is not applied actual file, so must make it dirty before.
			EditorUtility.SetDirty(_anmRes);
			AssetDatabase.SaveAssets();	//same as EditorApplication.SaveAssets();
		}
		
#if false
		// AddObjectToAsset() a_mat.mat -> hoge.ssax.asset
		// ↓
		// hoge.ssax.mat
		//   hoge.ssax.asset
		// umm...
		foreach (var e in _anmRes.ImageList)
		{
			if (e.material != null)
			{
				e.material.name = e.texture.name + "_Mat";
				AssetDatabase.AddObjectToAsset(e.material, _anmRes);

				// Reimport the asset after adding an object.
				// Otherwise the change only shows up when saving the project
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(e.material));
			}
		}
#endif
		return true;
	}

	private bool
	LoadXml(string path, string text)
	{
		_text = text;

		// make relative path 
		_relPath = System.String.Copy(path);
		//_relPath = _relPath.Replace("Assets/SsRes/", "");
		
		if (!Parse(true)) return false;
		return true;
	}

	private bool
	Parse(bool isAnime)// = true)
	{
		if (_text == null) return false;

		XmlDocument doc = new XmlDocument(); // doc is the new xml document.
		doc.LoadXml(_text); // load the file.

		string rootTagStr, rootNSStr;
		if (isAnime)
		{
			// this is .ssax
			rootTagStr = "SpriteStudioMotion";
			rootNSStr = "http://www.webtech.co.jp/SpriteStudio/XML/Motion";
		}
		else
		{
			// this is .sssx
			rootTagStr = "SpriteStudioScene";
			rootNSStr = "http://www.webtech.co.jp/SpriteStudio/XML/Scene";
		}

		// check the root tag name validity
		// root is ?xml, so skip to the next
		XmlNode rootNode = doc.FirstChild;
		rootNode = rootNode.NextSibling;
		if (rootNode.Name != rootTagStr)
		{
			Debug.LogError("Invalid root tag name: " + rootNode.Name);
			return false;
		}

		// check the root tag namespace validity
		XmlAttributeCollection rootNodeAttrs = rootNode.Attributes;
		XmlNode nodeXMLNS = rootNodeAttrs["xmlns"];
		if (nodeXMLNS.Value != rootNSStr)
		{
			Debug.LogError("\"xmlns\" doesn't match: " + rootNode.Name);
			return false;
		}

		// check file format version validity
		XmlNode versionNode = rootNodeAttrs["version"];
		int readVersion = SsVersion.ToInt(versionNode.Value);
	
		if (readVersion < RequiredVersion)
		{
			Debug.LogError("Version under " + SsVersion.ToString(RequiredVersion) + " is not supported -> " + SsVersion.ToString(readVersion));
			return false;
		}
		if (readVersion > CurrentVersion)
		{
			Debug.LogError("This version " + SsVersion.ToString(readVersion) + " is not supported yet. supports up to " + SsVersion.ToString(CurrentVersion));
			return false;
		}

#if false
		// made sure the rootNode has Header, ImageList, Parts
		XmlNodeList children = rootNode.ChildNodes;
		foreach(XmlNode n in children)
			Debug.Log(n.Name);
#endif
		// Create an XmlNamespaceManager to resolve namespaces.
		NameTable nt = new NameTable();
		_nsMgr = new XmlNamespaceManager(nt);
		_nsMgr.AddNamespace("cur", nodeXMLNS.Value);
			
		// read header
		XmlNode headerNode = _SelectSingleNode(rootNode, "cur:Header");
		if (headerNode == null)
		{
			Debug.LogError("Header node is not found");
			return false;
		}
		XmlNode endFrameNode = _SelectSingleNode(headerNode, "cur:EndFrame");
		if (endFrameNode == null)
		{
			Debug.LogError("EndFrame node is not found");
			return false;
		}
		_anmRes.EndFrame = _ToInt(endFrameNode.InnerText);
#if NEED_EDIT_MODE
		_editMode = _ToInt(_SelectSingleNode(headerNode, "cur:EditMode").InnerText);
#endif
		_anmRes.FPS = _ToInt(_SelectSingleNode(headerNode, "cur:BaseTickTime").InnerText);
			
		// read option settings
		XmlNode optionNode = _SelectSingleNode(headerNode, "cur:OptionState");
		if (optionNode != null)
		{
			XmlNode n = _SelectSingleNode(optionNode, "cur:hvFlipForImageOnly");
			if (n == null)
				_anmRes.hvFlipForImageOnly = true;
			else
				_anmRes.hvFlipForImageOnly = _ToBool(n.InnerText);
		}
		
		// create image manager singleton
		//var imgMgr = SsImageManager.Singleton;
		
		// enumerate image paths
		// it is possible to be nothing in .ssax
		XmlNodeList imageNodeList = _SelectNodes(rootNode, "./cur:ImageList/cur:Image");
		if (imageNodeList == null)
		{
			Debug.LogError("ImageList node is not found");
			return false;
		}
		if (imageNodeList.Count <= 0)
		{
			Debug.LogError("ImageList has no contents");
			return false;
		}
		_anmRes.ImageList = new SsImageFile[imageNodeList.Count];
			
		//Debug.Log("imageNodeList.Count: " + imageNodeList.Count);
		foreach (XmlNode e in imageNodeList)
		{
			string idStr = e.Attributes["Id"].Value;
			int index = _ToInt(idStr) - 1;	// because it starts from 1.
			string path = e.Attributes["Path"].Value;
			// remove the useless string "./cur:" on head
			//Debug.Log("Id=" + idStr + " Path=" + path);
			if (path.StartsWith(@".\"))
				path = path.Remove(0, 2);
			path = path.Replace(@"\", "/"); // for Mac
			//path = _relPath + "/" + path; // doesn't care about ../
			string baseFullPath = Path.GetFullPath("./");
			path = Path.GetFullPath(Path.Combine(_relPath, path));	// combine path considering ../ 
			path = path.Substring(baseFullPath.Length);
			path = path.Replace(@"\", "/"); // just in case
			//Debug.Log("Id=" + index + " Path=" + path);
			
			// get image file info which contains a Texture Asset.
			SsImageFile imgFile = GetImage(path);
			if (imgFile == null) return false;

			XmlNode widthNode = e.Attributes["Width"];
			if (widthNode != null)
			{
				imgFile.width = _ToInt(widthNode.Value);
				if (!_IsPowerOfTwo(imgFile.width))
					Debug.LogWarning("Image width is not power of 2, it will be scaled.");
			}
			else
				imgFile.width = imgFile.texture.width;
			
			XmlNode heightNode = e.Attributes["Height"];
			if (heightNode != null)
			{
				imgFile.height = _ToInt(heightNode.Value);
				if (!_IsPowerOfTwo(imgFile.height))
					Debug.LogWarning("Image height is not power of 2, it will be scaled.");
			}
			else
				imgFile.height = imgFile.texture.height;
			
			// For now, bpp is not used anywhere.
			XmlNode bppNode = e.Attributes["Bpp"];
			if (bppNode != null)
			{
				imgFile.bpp = _ToInt(bppNode.Value);
				if (imgFile.bpp <= 8)
				{
					Debug.LogWarning(path + " seems index color image, so it has to be converted to direct color or compressed type");
				}
			}
			else
			{
				switch (imgFile.texture.format)
				{
				default:
					imgFile.bpp = 0;	// Zero means something compressed type
					break;
				case TextureFormat.ARGB4444:
				case TextureFormat.RGB565:
					imgFile.bpp = 16;
					break;
				case TextureFormat.RGB24:
					imgFile.bpp = 24;
					break;
				case TextureFormat.ARGB32:
				case TextureFormat.RGBA32:
					imgFile.bpp = 32;
					break;
				}
			}

			// register image info into the list
			_anmRes.ImageList[index] = imgFile;
		}
		
		// enumerate parts
		XmlNodeList partList = _SelectNodes(rootNode, "./cur:Parts/cur:Part");
		if (partList == null)
		{
			Debug.LogError("Parts node is not found");
			return false;
		}
		//Debug.Log("Parts Num: " + partList.Count);
		if(partList.Count <= 0)
		{
			Debug.LogError("No existence of Parts");
			return false;
		}
		
		_anmRes.PartList = new SsPartRes[partList.Count];

		foreach (XmlNode part in partList)
		{
			// create a part
			var partBase = new SsPartRes();
			partBase.AnimeRes = _anmRes;
			
			XmlAttribute rootAttr = part.Attributes["Root"];

			partBase.InheritState.Initialize(4, _anmRes.hvFlipForImageOnly);
			string partName = _GetNodeValue(part, "./cur:Name");
			partBase.Name = System.String.Copy(partName);
			if (rootAttr != null)
			{
				// this is root parts
				partBase.Type = SsPartType.Root;
				partBase.MyId = 0;
				partBase.ParentId = -1;
			}
			else
			{
				// general parts
				partBase.Type				= (SsPartType)_ToInt(_GetNodeValue(part, "./cur:Type"));
				partBase.PicArea.Top		= _ToInt(_GetNodeValue(part, "./cur:PictArea/cur:Top"));
				partBase.PicArea.Left		= _ToInt(_GetNodeValue(part, "./cur:PictArea/cur:Left"));
				partBase.PicArea.Bottom		= _ToInt(_GetNodeValue(part, "./cur:PictArea/cur:Bottom"));
				partBase.PicArea.Right		= _ToInt(_GetNodeValue(part, "./cur:PictArea/cur:Right"));
				partBase.OriginX			= _ToInt(_GetNodeValue(part, "./cur:OriginX"));
				partBase.OriginY			= _ToInt(_GetNodeValue(part, "./cur:OriginY"));
				partBase.MyId				= 1 + _ToInt(_GetNodeValue(part, "./cur:ID"));
				partBase.ParentId			= 1 + _ToInt(_GetNodeValue(part, "./cur:ParentID"));
				partBase.SrcObjId			= _ToInt(_GetNodeValue(part, "./cur:PicID"));
				partBase.AlphaBlendType		= (SsAlphaBlendOperation)(1 + _ToInt(_GetNodeValue(part, "./cur:TransBlendType")));
				partBase.InheritState.Type	= (SsInheritanceType)_ToInt(_GetNodeValue(part, "./cur:InheritType"));
			
				if (partBase.SrcObjId >= _anmRes.ImageList.Length)
				{
					/*
					 * // supply lack of image
					int count = 1 + partBase.SrcObjId - _anmRes.ImageList.Length;
					_anmRes.ImageList.
					for (int i = 0; i < count; ++i)
					{
						_anmRes.ImageList[partBase.SrcObjId + i] = SsImageFile.invalidInstance;
					}
					*/
					// clamp id
					Debug.LogWarning("Picture ID is out of image list. Part: " + partName + " PicId: " + partBase.SrcObjId);
					partBase.SrcObjId = 0;
				}
				
				partBase.imageFile = _anmRes.ImageList[partBase.SrcObjId];
				if (partBase.imageFile != SsImageFile.invalidInstance)
				{
					// precalc UV coordinates
					partBase.CalcUVs(0,0,0,0);
	
					// add basic material with basic shader
					AddMaterials(partBase, SsColorBlendOperation.Non);
				}

				if (partBase.InheritState.Type == SsInheritanceType.Parent)
				{
					// copy parent's value and rate statically. dynamic reference is certain implement but it costs much more.
					for (int i = 0; i < (int)SsKeyAttr.Num; ++i)
					{
						SsKeyAttr attr = (SsKeyAttr)i;
						var param = _anmRes.PartList[partBase.ParentId].InheritParam(attr);
						partBase.InheritState.Values[i] = param;
						//Debug.Log(partBase.Name +" inherits parent's attr: " + attr + " use: " + param.Use + " rate:" + param.Rate);
					}
				}
			}

			// make original 4 vertices will be not modified. it consists of OriginX/Y and PicArea.WH.
			partBase.OrgVertices = partBase.GetVertices(partBase.PicArea.WH());

#if false
			// parent part
			SsPartRes parentPart = null;
			if (partBase.ParentId >= 0)
				parentPart = _anmRes.PartList[partBase.ParentId];
#endif

			if (!isAnime)
			{
				// scene only has this element
				partBase.SrcObjType = (SsSourceObjectType)_ToInt(_GetNodeValue(part, "./cur:ObjectType") );
			}

			// parse attributes
			XmlNodeList attrList = _SelectNodes(part, "./cur:Attributes/cur:Attribute");
			string attrName;
			foreach (XmlNode attrNode in attrList)
			{
				// recognize the tag
				attrName = attrNode.Attributes["Tag"].Value;
				if (attrName.Length != 4)
				{
					Debug.LogWarning("invalid attribute tag!!: " + attrName);
					continue;
				}
				SsKeyAttrDesc attrDesc = SsKeyAttrDescManager.Get(attrName);
				if (attrDesc == null)
				{
					Debug.LogWarning("Unknown or Unsupported attribute: " + attrName);
					continue;
				}
				
				switch (attrDesc.Attr)
				{
				case SsKeyAttr.PartsPal:
					if (attrNode.HasChildNodes)
						Debug.LogWarning("Unsupported attribute: " + attrName);
					continue;
				}

				// set inheritance parameter to part instance.
				if (partBase.Type != SsPartType.Root)
				{
					if (partBase.InheritState.Type == SsInheritanceType.Self)
					{
						// has its own value.
						var InheritParam = new SsInheritanceParam();
						XmlNode InheritNode = attrNode.Attributes.GetNamedItem("Inherit");
						if (InheritNode != null)
						{
							// mix my value and parent's value, but actually user 100% parent's...
							InheritParam.Use = true;
							InheritParam.Rate = (100 * _ToInt(InheritNode.Value)) / SSIO_SUCCEED_DENOMINATOR;
						}
						else
						{
							// absolutely not refer the parent's value.
							InheritParam.Use = false;
							InheritParam.Rate = 0;
						}
						// apply to part
						partBase.InheritState.Values[(int)attrDesc.Attr] = InheritParam;
						//Debug.LogError(partBase.Name +" has own value! attr:" + attrDesc.Attr + " use:" + InheritParam.Use);
					}
				}
				
				// enumerate keys
				XmlNodeList keyList = _SelectNodes(attrNode, "./cur:Key");
				if (keyList == null
				||	keyList.Count < 1)
				{
					//Debug.Log(attrName + " has no keys");
				}
				else
				{
					foreach (XmlNode key in keyList)
					{
						//dynamic keyBase;	cannot use dynamic at this time
						SsKeyFrameInterface keyBase;
						bool bNeedCurve = true;
						switch (attrDesc.ValueType)
						{
						case SsKeyValueType.Data:
							{
								// if value type is Data, string format of node "Value" depends on attribute type.
								string strValue = _GetNodeValue(key, "./cur:Value");
								switch (attrDesc.CastType)
								{
								default:
								case SsKeyCastType.Bool:	// not supported currently
								case SsKeyCastType.Other:	// not supported currently
								case SsKeyCastType.Int:
								case SsKeyCastType.Hex:
									// these types are used as integer.
									var	intKey = new SsIntKeyFrame();	// to make intellisence working, really??
									if (attrDesc.CastType == SsKeyCastType.Hex)
										intKey.Value = _HexToInt(strValue);
									else
										intKey.Value = _ToInt(strValue);
									
									switch (attrDesc.Attr)
									{
									case SsKeyAttr.PosX:
									case SsKeyAttr.PosY:
										// apply scale factor
										intKey.Value = (int)((float)intKey.Value * _anmRes.ScaleFactor);
										break;
#if _FORCE_BOUND_PART_TO_MOST_TOP
									// force bound part to most top to draw surely if wanted
									case SsKeyAttr.Prio:
										if (partBase.Type == SsPartType.Bound)
											intKey.Value = short.MaxValue;	// int.MaxValue is failed to unbox
										break;
#endif
									}
#if _DEBUG
									Debug.LogWarning("Is it OK? -> " + strValue + " = " + intKey.Value);
#endif
									keyBase = intKey;
									break;
								case SsKeyCastType.Float:
								case SsKeyCastType.Degree:
									var	floatKey = new SsFloatKeyFrame();	// to make intellisence working, really??
#if _FORCE_BOUND_PART_TO_BE_TRANSPARENT
									// force bound part to transparent to draw if wanted
									if (attrDesc.Attr == SsKeyAttr.Trans
									&&	partBase.Type == SsPartType.Bound)
										floatKey.Value = 0.5f;
									else
#endif
										floatKey.Value = (float)_ToDouble(strValue);
									// unnecessary to convert to radian. because Unity requires degree unit.
									//if (attrDesc.CastType == SsKeyCastType.Degree)
										//floatKey.Value = (float)_DegToRad( _ToDouble(strValue) );
									keyBase = floatKey;
									break;
								}
							}
							break;

						case SsKeyValueType.Param:
							{
								var boolKey = new SsBoolKeyFrame();
								keyBase = boolKey;
								boolKey.Value = _ToUInt(_GetNodeValue(key, "./cur:Value")) != 0 ? true : false;
								bNeedCurve = false;
							}
							break;
	
						case SsKeyValueType.Palette:
							{
								var paletteKey = new SsPaletteKeyFrame();
								keyBase = paletteKey;
								var v = new SsPaletteKeyValue();
								v.Use	= _ToUInt(_GetNodeValue(key, "./cur:Use")) != 0 ? true : false;
								v.Page	= _ToInt(_GetNodeValue(key, "./cur:Page"));
								v.Block	= (byte)_ToInt(_GetNodeValue(key, "./cur:Block"));
							 	paletteKey.Value = v;
							}
							break;
	
						case SsKeyValueType.Color:
							{
								var colorBlendKey = new SsColorBlendKeyFrame();
								keyBase = colorBlendKey;
								var v = new SsColorBlendKeyValue(4);
								v.Target 	= (SsColorBlendTarget)_ToUInt(_GetNodeValue(key, "./cur:Type"));
								v.Operation	= (SsColorBlendOperation)(1 + _ToUInt(_GetNodeValue(key, "./cur:Blend")));
								AddMaterials(partBase, v.Operation);
								switch (v.Target)
								{
								case SsColorBlendTarget.None:
									// no color, but do care about interpolation to/from 4 vertex colors
									for (int i = 0; i < 4; ++i)
									{
										v.Colors[i].R = v.Colors[i].G = v.Colors[i].B = 255;
										// 0 alpha value means no effect.
										v.Colors[i].A = 0;
									}
									break;
	
								case SsColorBlendTarget.Overall:
									// one color, but do care about interpolation to/from 4 vertex colors
									_GetColorRef(v.Colors[0], key, "./cur:Value");
									for (int i = 1; i < 4; ++i)
										v.Colors[i] = v.Colors[0];
									break;

								case SsColorBlendTarget.Vertex:
									// 4 vertex colors. the order is clockwise.
									_GetColorRef(v.Colors[0], key, "./cur:TopLeft");
									_GetColorRef(v.Colors[1], key, "./cur:TopRight");
									_GetColorRef(v.Colors[2], key, "./cur:BottomRight");
									_GetColorRef(v.Colors[3], key, "./cur:BottomLeft");
									break;
								}
								colorBlendKey.Value = v;
							}
							break;
	
						case SsKeyValueType.Vertex:
							{
								var vertexKey = new SsVertexKeyFrame();
								keyBase = vertexKey;
								var v = new SsVertexKeyValue(4);
								_GetPoint(v.Vertices[0], key, "./cur:TopLeft");
								_GetPoint(v.Vertices[1], key, "./cur:TopRight");
	
								if (readVersion <= (uint)SsAnimeFormatVersion.V320)
								{// for obsolete version
									_GetPoint(v.Vertices[2], key, "./cur:BottomLeft");
									_GetPoint(v.Vertices[3], key, "./cur:BottomRight");
								}
								else
								{
									_GetPoint(v.Vertices[3], key, "./cur:BottomLeft");
									_GetPoint(v.Vertices[2], key, "./cur:BottomRight");
								}
								// apply scale factor
								for (int i = 0; i < v.Vertices.Length; ++i)
									v.Vertices[i].Scale(_anmRes.ScaleFactor);
								vertexKey.Value = v;
							}
							break;
	
						case SsKeyValueType.User:
							{
								var userKey = new SsUserDataKeyFrame();
								keyBase = userKey;
								var	v = new SsUserDataKeyValue();
								XmlNode numberNode = _SelectSingleNode(key, "./cur:Number");
								if (numberNode != null)
								{
									v.IsNum = true;
									v.Num = _ToInt(numberNode.InnerText);
								}
								XmlNode rectNode = _SelectSingleNode(key, "./cur:Rect");
								if (rectNode != null)
								{
									v.IsRect = true;
									v.Rect = new SsRect();
									_GetRect(v.Rect, rectNode);
								}
								XmlNode pointNode = _SelectSingleNode(key, "./cur:Point");
								if (pointNode != null)
								{
									v.IsPoint = true;
									v.Point = new SsPoint();
									_GetPoint(v.Point, pointNode);
								}
								XmlNode stringNode = _SelectSingleNode(key, "./cur:String");
								if (stringNode != null)
								{
									v.IsString = true;
									v.String = System.String.Copy(stringNode.InnerText);
								}
								// writeback
								userKey.Value = v;

								bNeedCurve = false;
							} // case KEYTYPE_USERDATA
							break;
	
						case SsKeyValueType.Sound:
							{
								var soundKey = new SsSoundKeyFrame();
								keyBase = soundKey;
								var v = new SsSoundKeyValue();
								v.Flags	= (SsSoundKeyFlags)_ToUInt(_SelectSingleNode(key, "./cur:Use").InnerText);
								v.NoteOn	= _ToByte(_SelectSingleNode(key, "./cur:Note").InnerText);
								v.SoundId	= _ToByte(_SelectSingleNode(key, "./cur:ID").InnerText);
								v.LoopNum	= _ToByte(_SelectSingleNode(key, "./cur:Loop").InnerText);
								v.Volume	= _ToByte(_SelectSingleNode(key, "./cur:Volume").InnerText);
								v.UserData	= _ToUInt(_SelectSingleNode(key, "./cur:Data").InnerText);
								// writeback
								soundKey.Value = v;
	
								bNeedCurve = false;
							}
							break;
	
						default:
							Debug.LogError("Fatal error!! not implemented cast type: " + _ToInt(attrNode.Value) );
							continue;
						} // switch
						
						//keyBase = InitKeyFrameData();
						keyBase.ValueType = attrDesc.ValueType;
						keyBase.Time = _ToInt(key.Attributes["Time"].Value);
	
						if (bNeedCurve)
						{
							// make values to interpolate
							var v = new SsCurveParams();
							
							XmlNode curveTypeNode = key.Attributes["CurveType"];
							if (curveTypeNode == null)
							{
								v.Type = SsInterpolationType.None;
							}
							else
							{
								v.Type = (SsInterpolationType)_ToUInt(curveTypeNode.Value);
								if (v.Type == SsInterpolationType.Hermite
								||	v.Type == SsInterpolationType.Bezier)
								{
									// must exist
									if (key.Attributes["CurveStartT"] == null)
									{
										Debug.LogError("CurveStartT param not found!!");
										return false;
									}
								
									v.StartT	= _ToFloat(key.Attributes["CurveStartT"].Value);
									v.StartV	= _ToFloat(key.Attributes["CurveStartV"].Value);
									v.EndT		= _ToFloat(key.Attributes["CurveEndT"].Value);
									v.EndV		= _ToFloat(key.Attributes["CurveEndV"].Value);
								
									switch (attrDesc.Attr)
									{
									case SsKeyAttr.PosX:
									case SsKeyAttr.PosY:
									case SsKeyAttr.Vertex:
										// apply scale factor
										v.StartV *= _anmRes.ScaleFactor;
										v.EndV *= _anmRes.ScaleFactor;
										break;
									case SsKeyAttr.Angle:
										if (_database.AngleCurveParamAsRadian)
										{
											// degree to radian
											v.StartV = (float)(v.StartV * 180 / System.Math.PI);
											v.EndV = (float)(v.EndV * 180 / System.Math.PI);
										}
										break;
									}
								}
							}
							keyBase.Curve = v;
						}
	
#if false//_DEBUG
						// dump debug info
						SsDebugLog.Print("Attr: " + attrDesc.Attr + " " + keyBase.ToString());
#endif					
						
						// add keyFrame info to part instance.
						partBase.AddKeyFrame(attrDesc.Attr, keyBase);
					}
				}
			}
#if _DEBUG
			// dump debug info
			SsDebugLog.Print(partBase.ToString());
#endif					
			if (_precalcAttrValues)
			{
				// precalculate attribute values each frame
				partBase.CreateAttrValues(_anmRes.EndFrame + 1);
			}

			// add to list
			// if the container is Dictionary, allow non-sequential ID order of the stored parts, but it occurs to search by ID.
			_anmRes.PartList[partBase.MyId] = partBase;
			//_anmRes.PartList.Add(partBase);
		}
//		Debug.Log("_anmRes.PartList.Count: " + _anmRes.PartList.Count);

#if false
		// precalculate values to inherit parent's one.
		if (PrecalcAttrValues)
		{
			//foreach (var e in _anmRes.PartList.Values) // for Dictionary version
			foreach (var e in _anmRes.PartList)
				if (!e.IsRoot && e.HasParent)
					e.CalcInheritedValues(_anmRes.PartList[e.ParentId]);
		}
#endif

		return true;
	}
	
	private SsImageFile
	GetImage(string path)
	{
		//Debug.Log("Textures " + Resources.FindObjectsOfTypeAll(typeof(Texture)).Length);
		SsImageFile img;
		try
		{
			// try get the existing
			img = _imageFileList[path];
		}
		catch (KeyNotFoundException)
		{
			// not found, so create
			img = new SsImageFile();
			img.path = path;
			img.materials = new Material[(int)SsShaderType.Num];
			
#if false
			// reimport texture with settings for SpriteStudio
			SsAssetPostProcessor.HookTextureImport = true;
			AssetDatabase.ImportAsset(path, ImportAssetOptions.Default);
#endif
			
#if false
			// omit extension to load Texture Asset successfully
			//string pathExtOmitted = Path.ChangeExtension(path, null);
			
			// load image newly
			//Object obj = Resources.Load(pathExtOmitted, typeof(Texture2D));
#else
			Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
#endif

			img.texture = obj as Texture2D;
			//Debug.Log(img.texture);
			if (img.texture == null)
			{
				Debug.LogError("can't load image: " + path);
				return null;
			}
			
			// change default texture wrap and filtering medthod to avoid unexpected edge colors
			img.texture.wrapMode = TextureWrapMode.Clamp;
//			img.texture.filterMode = FilterMode.Point;	// unwanted... 2012.04.04

			// add to list
			_imageFileList[path] = img;
		}
		
		return img;
	}
	
	void
	AddMaterials(SsPartRes part, SsColorBlendOperation colorBlendType)
	{
		SsImageFile img = part.imageFile;
		SsShaderType shaderType = SsShaderManager.EnumToType(colorBlendType, part.AlphaBlendType, SsMaterialColorBlendOperation.Non);
		Material material = img.GetMaterial(shaderType);
		if (material)
			return;	// already added

		// create material
#if _BUILD_UNIFIED_SHADERS
		img.useUnifiedShader = _anmRes.UseCgProgram;
		Shader shader = SsShaderManager.Get(shaderType, _anmRes.UseCgProgram);
#else
		Shader shader = SsShaderManager.Get(shaderType, false);
#endif
		// get material asset path
		string assetPath = Path.GetDirectoryName( AssetDatabase.GetAssetPath(img.texture) ) + "/assets/";
		string shaderName = shader.name;
		shaderName = shaderName.Replace("Ss/", "");
		string assetName = assetPath + img.texture.name + "_Mat_" + shaderName + ".asset";

		// try to load the exiting
		Material existedMat = (Material)AssetDatabase.LoadAssetAtPath(assetName, typeof(Material));
		Material mat = (existedMat ?? new Material(shader));

		mat.SetTexture("_MainTex", img.texture);

		if (existedMat == null)
		{
			// if none, create material as asset file newly
//			Debug.Log("create material: " + img.path + " shader: " + shaderType);
			if (!Directory.Exists(assetPath))
			{
				string parentFolder = Path.GetDirectoryName(assetPath.TrimEnd('\\', '/'));
				AssetDatabase.CreateFolder(parentFolder, "assets");
			}
			AssetDatabase.CreateAsset(mat, assetName);
		}
		else
		{
			// update the existing content
			mat.shader = shader;
			EditorUtility.SetDirty(mat);
			AssetDatabase.SaveAssets();	//same as EditorApplication.SaveAssets();
		}

		// add to material list
		img.materials[SsShaderManager.ToSerial(shaderType)] = mat;
	}

	void
	ClearPreviousMaterials()
	{
#if false
		foreach (var img in _anmRes.ImageList)
		{
			// save material as asset file to serialize this SsAnime instance entirely.
			string assetPath = Path.GetDirectoryName( AssetDatabase.GetAssetPath(img.texture) ) + "/assets/";
			for (int i = 0; i < img.materials.Length; ++i)
			{
				Material m = img.materials[i];
				if (!m) continue;
				string shaderName = m.shader.name;
				shaderName = shaderName.Replace("Ss/", "");
				string assetName = assetPath + img.texture.name + "_Mat_" + shaderName + ".asset";
		
				// try to load the exiting
				Material existedMat = (Material)AssetDatabase.LoadAssetAtPath(assetName, typeof(Material));
				if (existedMat == null) continue;
				// detach this material from this animation
				img.materials[i] = null;
				//string[] names = {assetName};
				//string[] deps = AssetDatabase.GetDependencies(names); returns the list of all assets the material depends on.
				if (deps.Length == 0)
				{
					// delete this material
					AssetDatabase.DeleteAsset(assetName);
				}
			}
		}
#endif
	}

	private byte
	_ToByte<T>(T src)
	{
		return System.Convert.ToByte(src);
	}

	private bool
	_ToBool(string src)
	{
		bool ret = false;
		try {
			ret = System.Convert.ToBoolean(src);
		}catch(System.FormatException){
			int i = System.Convert.ToInt32(src);
			ret = (i == 0 ? false : true);
		}
		return ret;
	}

	private bool
	_ToBool<T>(T src)
	{
		return System.Convert.ToBoolean(src);
	}

	private int
	_ToInt<T>(T src)
	{
		return System.Convert.ToInt32(src);
	}

	private int
	_HexToInt(string src)
	{
		return System.Convert.ToInt32(src, 16);
	}

	private uint
	_ToUInt<T>(T src)
	{
		return System.Convert.ToUInt32(src);
	}

	private uint
	_HexToUInt(string src)
	{
		return System.Convert.ToUInt32(src, 16);
	}

	private float
	_ToFloat<T>(T src)
	{
		return System.Convert.ToSingle(src);
	}

	private double
	_ToDouble<T>(T src)
	{
		return System.Convert.ToDouble(src);
	}
	
	private XmlNode
	_SelectSingleNode(XmlNode n, string path)
	{
		XmlNode ret = n.SelectSingleNode(path, _nsMgr);
#if _DEBUG
		if (ret == null)
		{
			Debug.LogError("Node not found"+ path);
		}
#endif
		return ret;
	}

	private XmlNodeList
	_SelectNodes(XmlNode n, string path)
	{
		return n.SelectNodes(path, _nsMgr);
	}

	private string
	_GetNodeValue(XmlNode n, string path)
	{
		return _SelectSingleNode(n, path).InnerText;
	}

	private void
	_GetColorRef(
		SsColorRef o,
		XmlNode n,
		string path)
	{
		if (path != null) n = _SelectSingleNode(n, path);
		o.R = _ToByte(_GetNodeValue(n, "./cur:Red"));
		o.G = _ToByte(_GetNodeValue(n, "./cur:Green"));
		o.B = _ToByte(_GetNodeValue(n, "./cur:Blue"));
		o.A = _ToByte(_GetNodeValue(n, "./cur:Alpha"));
	}
	private void
	_GetColorRef(SsColorRef o, XmlNode n)
	{
		_GetColorRef(o, n, null);
	}

	private void
	_GetPoint(
		SsPoint o,
		XmlNode n,
		string path)
	{
		if (path != null) n = _SelectSingleNode(n, path);
		o.X = _ToInt(_GetNodeValue(n, "./cur:X"));
		o.Y = _ToInt(_GetNodeValue(n, "./cur:Y"));
	}
	private void
	_GetPoint(SsPoint o, XmlNode n)
	{
		_GetPoint(o, n, null);
	}

	private void
	_GetRect(
		SsRect o,
		XmlNode n,
		string path)
	{
		if (path != null) n = _SelectSingleNode(n, path);
		o.Top		= _ToInt(_GetNodeValue(n, "./cur:Top"));
		o.Left		= _ToInt(_GetNodeValue(n, "./cur:Left"));
		o.Bottom	= _ToInt(_GetNodeValue(n, "./cur:Bottom"));
		o.Right		= _ToInt(_GetNodeValue(n, "./cur:Right"));
	}
	private void
	_GetRect(SsRect o, XmlNode n)
	{
		_GetRect(o, n, null);
	}

	private	Dictionary<string, SsImageFile>	_imageFileList = new Dictionary<string, SsImageFile>();

	private bool
	_IsPowerOfTwo(int n)
	{
		switch (n)
		{
		case 2:case 4:case 8:case 16:case 32:case 64:case 128:case 256:case 512:
		case 1024:case 2048:case 4096:case 8192:case 16384:case 32768:case 65536:
			return true;
		}
		return false;
	}
}
