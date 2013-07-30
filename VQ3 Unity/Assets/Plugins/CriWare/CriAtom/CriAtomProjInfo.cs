/****************************************************************************
 *
 * CRI Middleware SDK
 *
 * Copyright (c) 2011-2012 CRI Middleware Co.,Ltd.
 *
 * Library  : CRI Atom
 * Module   : CRI Atom for Unity
 * File     : CriAtomAcfInfo.cs
 *
 ****************************************************************************/
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class CriAtomAcfInfo
{
	#region Variables
	public static AcfInfo acfInfo = null;
	#endregion

    #region InfoBase
	public class InfoBase
	{
		public string name = "dummyCueSheet";
		public int id = 0;
		public string comment = "";
	} /* end of class */
    #endregion

    #region AcfInfo
	public class AcfInfo : InfoBase
	{
		public string acfPath = "dummyCueSheet.acf";
		public List<AcbInfo> acbInfoList = new List<AcbInfo>();
		public string atomGuid = "";
		public string dspBusSetting = "DspBusSetting_0";
		public List<string> aisacControlNameList = new List<string>();
		
		public AcfInfo(string n, int inId, string com, string inAcfPath, string _guid, string _dspBusSetting)
		{
			this.name = n;
			this.id = inId;
			this.comment = com;
			this.acfPath = inAcfPath;
			this.atomGuid = _guid;
			this.dspBusSetting = _dspBusSetting;
		}
	} /* end of class */
    #endregion

    #region AcbInfo
	public class AcbInfo : InfoBase
	{
		public string acbPath = "dummyCueSheet.acb";
		public string awbPath = "dummyCueSheet_streamfiles.awb";
		public string atomGuid = "";
		public Dictionary<int, CueInfo> cueInfoList = new Dictionary<int, CueInfo>();

		public AcbInfo(string n, int inId, string com, string inAcbPath, string inAwbPath, string _guid)
		{
			this.name = n;
			this.id = inId;
			this.comment = com;
			this.acbPath = inAcbPath;
			this.awbPath = inAwbPath;
			this.atomGuid = _guid;
		}
	} /* end of class */
    #endregion

    #region CueInfo
	public class CueInfo : InfoBase
	{
		public CueInfo(string n, int inId, string com)
		{
			this.name = n;
			this.id = inId;
			this.comment = com;
		}
	} /* end of class */
    #endregion
	
	public static bool GetCueInfo(bool forceReload)
	{
		if (CriAtomAcfInfo.acfInfo == null || forceReload) {
			GetCueInfoInternal();
		}		
		
		/* もしACFInfoが無い場合、acfがあるか検索 */
		if (CriAtomAcfInfo.acfInfo == null) {
			//Debug.LogWarning("ADX2 need \"CriAtomProjInfo_Unity.cs\"");	
		
			string[] files = System.IO.Directory.GetFiles(Application.streamingAssetsPath);
			int acbIndex = 0;
			foreach (string file in files) {
				if (System.IO.Path.GetExtension(file.Replace("\\","/")) == ".acf") {
					CriAtomAcfInfo.acfInfo = new AcfInfo(System.IO.Path.GetFileNameWithoutExtension(file),
						0,"",System.IO.Path.GetFileName(file),"","");
				}
			}
			if(CriAtomAcfInfo.acfInfo != null){
				foreach (string file in files) {
					if (System.IO.Path.GetExtension(file.Replace("\\","/")) == ".acb") {
						
						AcbInfo acbInfo = new AcbInfo(System.IO.Path.GetFileNameWithoutExtension(file),
							acbIndex,"",System.IO.Path.GetFileName(file),"","");
						
						/* 指定したACBファイル名(キューシート名)を指定してキュー情報を取得 */
						//CriAtomExAcb acb = CriAtomExAcb.LoadAcbFile(null, file.Replace("\\","/"), "");
						
						/* キュー名リストの作成 */
						//int cueIndex = 0;
						//CriAtomEx.CueInfo[] cueInfoList = acb.GetCueInfoList();
						//foreach(CriAtomEx.CueInfo cueInfo in cueInfoList){
						//	CueInfo tmpCueInfo = new CueInfo(cueInfo.name,cueInfo.id,"");
						//	acbInfo.cueInfoList.Add(cueIndex,tmpCueInfo);
						//}
						CueInfo tmpCueInfo = new CueInfo("DummyCue",0,"");
						acbInfo.cueInfoList.Add(0,tmpCueInfo);
						
						CriAtomAcfInfo.acfInfo.acbInfoList.Add(acbInfo);
						acbIndex++;
					}
				}
			}
		}
		
		return (CriAtomAcfInfo.acfInfo != null);
	}
    
	static partial void GetCueInfoInternal();
	
} // end of class

/* end of file */

