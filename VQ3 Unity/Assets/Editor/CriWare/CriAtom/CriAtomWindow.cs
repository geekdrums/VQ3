/****************************************************************************
 *
 * CRI Middleware SDK
 *
 * Copyright (c) 2011-2012 CRI Middleware Co.,Ltd.
 *
 * Library  : CRI Atom
 * Module   : CRI Atom for Unity Editor
 * File	    : CriAtomWindow.cs
 *
 ****************************************************************************/
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public sealed class CriAtomWindow : EditorWindow
{
	#region Variables
	private int selectedCueId = 0;
	private int selectedCueSheetId = 0;
	private int lastPreviewCueSheetId = -1;
	private Vector2 scrollPos;
	private Vector2 scrollPos_Window;
	private Rect windowRect = new Rect(10, 10, 100, 100);
	private bool scaling = true;
	// Public
	public string acfPath = "";
	public string dspBusSetting = "";
	#endregion

	#region Functions
	[MenuItem("CRI/Open CRI Atom Window ...")]
	static void OpenWindow()
	{
		EditorWindow.GetWindow<CriAtomWindow>(false, "CRI Atom");
	}

	void OnSelectionChange()
	{
		Repaint();
	}

	void OnFocus()
	{
	}

	private void OnEnable()
	{
		CriAtomAcfInfo.GetCueInfo(false);
		if (CriAtomAcfInfo.acfInfo != null)
		{
			this.acfPath = CriAtomAcfInfo.acfInfo.acfPath;
			this.dspBusSetting = CriAtomAcfInfo.acfInfo.dspBusSetting;
		}
		else
		{
			string[] files = Directory.GetFiles(Application.streamingAssetsPath);
			foreach(string file in files){
				if(System.IO.Path.GetExtension(file) == ".acf"){
					this.acfPath = file;
				}
			}
		}

        string tmpStr = FromBase64(EditorPrefs.GetString("CriAtomWindow_OutputAssetsRoot"));
        if (tmpStr != string.Empty)
        {
            atomCraftOutputAssetsRootPath = tmpStr;
        }
	}

	private void OnDisable()
	{
	}

	private void GUIACFSettings()
	{
		this.acfPath = EditorGUILayout.TextField("ACF File Path", this.acfPath, EditorStyles.label);
		this.dspBusSetting = EditorGUILayout.TextField("DSP Bus Setting", this.dspBusSetting, EditorStyles.label);
		
		EditorGUILayout.Space();
	}

	private void GUICueList()
	{
		#region CueSheet
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.PrefixLabel("Cue Sheet");

			List<string> popupCueSheetNameList = new List<string>();
			foreach (CriAtomAcfInfo.AcbInfo cueSheetInfo in CriAtomAcfInfo.acfInfo.acbInfoList)
			{
				popupCueSheetNameList.Add(cueSheetInfo.name);
			}
			lastPreviewCueSheetId = this.selectedCueSheetId;
			this.selectedCueSheetId = EditorGUILayout.Popup(this.selectedCueSheetId, popupCueSheetNameList.ToArray());
			if (this.selectedCueSheetId != lastPreviewCueSheetId)
			{
			}
		}
		EditorGUILayout.EndHorizontal();
		#endregion

		GUILayout.BeginHorizontal();
		{
			GUIStyle style = new GUIStyle(EditorStyles.miniButtonMid);
			style.alignment = TextAnchor.LowerLeft;
			if (GUILayout.Button("Cue Name", style)) {
				this.SortCueList(1);
			}
			if (GUILayout.Button("Cue ID", style, GUILayout.Width(70))) {
				this.SortCueList(0);
			}
		}
		GUILayout.EndHorizontal();
		
		if (CriAtomAcfInfo.acfInfo.acbInfoList == null || CriAtomAcfInfo.acfInfo.acbInfoList.Count <= (selectedCueSheetId)) {
			EditorGUILayout.HelpBox("Can not Get CueSheet Info!!!(CueSheetID:" + this.selectedCueSheetId.ToString() + ")", MessageType.Error);
			GUIImportAssetsFromAtomCraft();
		} else {
			var acbInfo = CriAtomAcfInfo.acfInfo.acbInfoList[this.selectedCueSheetId];
			if (acbInfo.cueInfoList.Count > 0) {
				float height = this.position.height - 304.0f;
				if (height < 100.0f) height = 100.0f;
				scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(height));
				foreach (CriAtomAcfInfo.CueInfo inf in acbInfo.cueInfoList.Values) {
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button(inf.name, EditorStyles.radioButton)) {
						this.selectedCueId = inf.id;
					}
					GUILayout.Label(inf.id.ToString(), GUILayout.Width(40));
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndScrollView();
			} else {
				EditorGUILayout.HelpBox("Can not found(CueSheetID:" + this.selectedCueSheetId.ToString() + ")", MessageType.Error);
			}
		}
	}
	
	private void GUICueInfo()
	{
		#region CueInfo.
		EditorGUILayout.BeginHorizontal("Toolbar");
		EditorGUILayout.LabelField("Selected Cue");
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.LabelField("Cue ID", this.selectedCueId.ToString());
		if (CriAtomAcfInfo.acfInfo.acbInfoList == null || CriAtomAcfInfo.acfInfo.acbInfoList.Count <= (selectedCueSheetId)) {
			EditorGUILayout.HelpBox("Can not Get CueSheet Info!!!(CueSheetID:" + this.selectedCueSheetId.ToString() + ")", MessageType.Error);
			GUIImportAssetsFromAtomCraft();
		} else {
			var acbInfo = CriAtomAcfInfo.acfInfo.acbInfoList[selectedCueSheetId];
			if (acbInfo.cueInfoList.ContainsKey(this.selectedCueId)) {
				var cueInfo = acbInfo.cueInfoList[this.selectedCueId];
				EditorGUILayout.LabelField("Cue Name", cueInfo.name);
				EditorGUILayout.LabelField("Comment", cueInfo.comment, EditorStyles.wordWrappedLabel, GUILayout.Height(28));
				EditorGUILayout.Space();
			} else {
				EditorGUILayout.HelpBox("Can not Get Cue Info!!!(CueID:" + this.selectedCueId.ToString() + ")", MessageType.Error);
			}
		}
		#endregion
	}

	private void GUIEdit()
	{
		if (this.selectedCueSheetId < CriAtomAcfInfo.acfInfo.acbInfoList.Count && CriAtomAcfInfo.acfInfo.acbInfoList[this.selectedCueSheetId].cueInfoList.ContainsKey(this.selectedCueId))
		{
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Create GameObject", EditorStyles.miniButtonLeft, GUILayout.Height(22))) {
					this.CreateAtomSourceGameObject(true);
				}

				GameObject targetObject = null;
				CriAtomSource atomSource = null;
				if (Selection.gameObjects.Length > 0) {
					targetObject = Selection.gameObjects[0];
					atomSource = targetObject.GetComponent<CriAtomSource>();
				}

				if (targetObject == null) {
					GUI.backgroundColor = Color.gray;
				}
				if (GUILayout.Button("Add Component", EditorStyles.miniButtonMid, GUILayout.Height(22))) {
					if (targetObject != null) {
						if (atomSource != null) {
							if (EditorUtility.DisplayDialog("There are already \"Cri Atom Souce\".", "Are you sure you want to add more?", "Add", "No")) {
								this.CreateAtomSourceGameObject(false);
							}
						} else {
							this.CreateAtomSourceGameObject(false);
						}
					}
				}
				
				if (atomSource == null) {
					GUI.backgroundColor = Color.gray;
				}
				if (GUILayout.Button("Update Cue Info", EditorStyles.miniButtonRight, GUILayout.Height(22))) {
					if (atomSource != null) {
						var acbInfo = CriAtomAcfInfo.acfInfo.acbInfoList[this.selectedCueSheetId];
						var cueInfo = acbInfo.cueInfoList[this.selectedCueId];
						if (atomSource.cueSheet == acbInfo.name && atomSource.cueName == cueInfo.name) {
							EditorUtility.DisplayDialog("Information", "Is the same configuration.", "OK");
						} else {
							atomSource.cueSheet = acbInfo.name;
							atomSource.cueName = cueInfo.name;
						}
						Selection.activeGameObject = targetObject;
					}
				}
				GUI.backgroundColor = Color.white;
				
			}
			EditorGUILayout.EndHorizontal();
		}
	}

	private void SortCueList(int type)
	{
		List<CriAtomAcfInfo.CueInfo> cueList = new List<CriAtomAcfInfo.CueInfo>();
		foreach (CriAtomAcfInfo.CueInfo inf in CriAtomAcfInfo.acfInfo.acbInfoList[selectedCueSheetId].cueInfoList.Values) {
			cueList.Add(inf);
		}

		switch (type) {
		case 0:
			cueList.Sort(delegate(CriAtomAcfInfo.CueInfo x, CriAtomAcfInfo.CueInfo y) {
				return x.id - y.id;
			});
			break;
		default:
			cueList.Sort(delegate(CriAtomAcfInfo.CueInfo x, CriAtomAcfInfo.CueInfo y) {
				return string.Compare(x.name, y.name);
			});
			break;
		} // end of switch

		Dictionary<int, CriAtomAcfInfo.CueInfo> sortedList = new Dictionary<int, CriAtomAcfInfo.CueInfo>();
		foreach (CriAtomAcfInfo.CueInfo inf in cueList) {
			sortedList.Add(inf.id, inf);
		}

		CriAtomAcfInfo.acfInfo.acbInfoList[this.selectedCueSheetId].cueInfoList = sortedList;
	}

	private void ScalingWindow(int windowID)
	{
		GUILayout.Box("", GUILayout.Width(20), GUILayout.Height(20));
		if (Event.current.type == EventType.MouseUp)
			this.scaling = false;
		else if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
			this.scaling = true;

		if (this.scaling)
			this.windowRect = new Rect(windowRect.x, windowRect.y, windowRect.width + Event.current.delta.x, windowRect.height + Event.current.delta.y);

	}

	private void OnGUI()
	{
		this.windowRect = GUILayout.Window(0, windowRect, ScalingWindow, "resizeable", GUILayout.MinHeight(80), GUILayout.MaxHeight(200));
		
		if (CriAtomAcfInfo.acfInfo == null) {
			EditorGUILayout.HelpBox("Can not Get CueSheet Info!!!(CueSheetID:" + this.selectedCueSheetId.ToString() + ")", MessageType.Error);
			GUIImportAssetsFromAtomCraft();
			return;
		}

		this.scrollPos_Window = GUILayout.BeginScrollView(this.scrollPos_Window);
		{
			GUIACFSettings();

			GUICueList();

			GUICueInfo();

			GUIEdit();
			
			GUIImportAssetsFromAtomCraft();
		}
		GUILayout.EndScrollView();
	}
	
	private string atomCraftOutputAssetsRootPath = "";
	private void GUIImportAssetsFromAtomCraft()
	{
			GUILayout.Space(12);
			GUILayout.BeginHorizontal();
			GUILayout.Label("\"CRI Atom Craft\" Assets Path:");
			
			if(GUILayout.Button("Select Assets Root")){
				string tmpStr = SelectFolder();	
				if(tmpStr != String.Empty){
					atomCraftOutputAssetsRootPath = tmpStr;
                    EditorPrefs.SetString("CriAtomWindow_OutputAssetsRoot", ToBase64(tmpStr));
                    Debug.Log(FromBase64(EditorPrefs.GetString("CriAtomWindow_OutputAssetsRoot")));
				}
			}
			GUILayout.EndHorizontal();
		
			atomCraftOutputAssetsRootPath = GUILayout.TextArea(atomCraftOutputAssetsRootPath);
			//GUILayout.Label(Application.dataPath);
		
			if(GUILayout.Button("Update Assets of \"CRI Atom Craft\"")){
				if (CheckPathIsAtomCraftAssetRoot(atomCraftOutputAssetsRootPath) == false) {			
					return;
				}
			
                try
                {
                    CopyDirectory(atomCraftOutputAssetsRootPath, Application.dataPath);
                    Debug.Log("Complete Update Assets of \"CRI Atom Craft\"");
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);                    
					Debug.LogError("Failed to update Assets of \"CRI Atom Craft\"");
                }
                AssetDatabase.Refresh();
			}
	}
	private string SelectFolder()
	{
		string outString = String.Empty;
		
		outString = EditorUtility.OpenFolderPanel("Select \"CRI Atom Craft\" output Assets Folder",outString,atomCraftOutputAssetsRootPath);
		if (CheckPathIsAtomCraftAssetRoot(outString) == false) {
			outString = String.Empty;
		}
		return outString;
	}
	
	private bool CheckPathIsAtomCraftAssetRoot(string outString)
	{
		if(outString != String.Empty && (System.IO.Path.GetFileName(outString) == "Assets")){
			Debug.Log(String.Format("Change \"CRI Atom Craft\" output Assets \"{0}\"",outString));
			return true;
		} else {
            Debug.LogError(String.Format("Can not Change \"CRI Atom Craft\" output Assets \"{0}\".Please select a \"Assets\" Folder.", outString));
			return false;
		}		
	}
	
	private static void CopyDirectory( string sourceDirName, string destDirName)
	{
	    if (!System.IO.Directory.Exists(destDirName))
	    {
	        System.IO.Directory.CreateDirectory(destDirName);
	        System.IO.File.SetAttributes(destDirName, 
	            System.IO.File.GetAttributes(sourceDirName));
	    }
	
	    if (destDirName[destDirName.Length - 1] !=
	            System.IO.Path.DirectorySeparatorChar)
	        destDirName = destDirName + System.IO.Path.DirectorySeparatorChar;
	
	    string[] files = System.IO.Directory.GetFiles(sourceDirName);
	    foreach (string file in files)
	        System.IO.File.Copy(file,
	            destDirName + System.IO.Path.GetFileName(file), true);
	
	    string[] dirs = System.IO.Directory.GetDirectories(sourceDirName);
	    foreach (string dir in dirs)
	        CopyDirectory(dir, destDirName + System.IO.Path.GetFileName(dir));
	}

    static string ToBase64(string s)
    {
        return System.Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(s));
    }

    static string FromBase64(string s)
    {
        return UTF8Encoding.UTF8.GetString(System.Convert.FromBase64String(s));
    }

	private void CreateAtomSourceGameObject(bool createGameObjectFlag)
	{
		if (Selection.gameObjects.Length == 0) {
			createGameObjectFlag = true;
		}

		GameObject go = null;
		if (createGameObjectFlag) {
			go = new GameObject(CriAtomAcfInfo.acfInfo.acbInfoList[this.selectedCueSheetId].cueInfoList[this.selectedCueId].name + "(CriAtomSource)");
			if (Selection.gameObjects.Length > 0) {
				go.transform.parent = Selection.gameObjects[0].transform;
			}
		} else {
			go = Selection.gameObjects[0];
		}
		
		var acbInfo = CriAtomAcfInfo.acfInfo.acbInfoList[this.selectedCueSheetId];
		CriAtom atom = GameObject.FindObjectOfType(typeof(CriAtom)) as CriAtom;
		if (atom == null) {
			atom = CriWare.managerObject.AddComponent<CriAtom>();
			atom.acfFile = CriAtomAcfInfo.acfInfo.acfPath;
		}
		CriAtomCueSheet cueSheet = atom.GetCueSheetInternal(acbInfo.name);
		if (cueSheet == null) {
			cueSheet = atom.AddCueSheetInternal(null, acbInfo.acbPath, acbInfo.awbPath, null);
		}

		CriAtomSource newCriAtomSource = go.AddComponent<CriAtomSource>();
		newCriAtomSource.cueSheet = cueSheet.name;
		newCriAtomSource.cueName = acbInfo.cueInfoList[this.selectedCueId].name;


		Selection.activeObject = go;

		//Debug.Log("Add \"CRI Atom Souce\" \"" + newCriAtomSource.AcbName + "/" + newCriAtomSource.CueName + "\"");
	}
	#endregion
} // end of class

/* end of file */
