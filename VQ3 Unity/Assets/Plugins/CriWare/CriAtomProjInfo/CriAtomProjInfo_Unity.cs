/****************************************************************************
*
* CRI Middleware SDK
*
* Copyright (c) 2011-2012 CRI Middleware Co.,Ltd.
*
* Library  : CRI Atom
* Module   : CRI Atom for Unity
* File     : CriAtomProjInfo_Unity.cs
* Tool Ver.          : CRI Atom Craft LE Ver.1.32.00
* Date Time          : 2014/02/05 20:50
* Project Name       : VQ3ADX
* Project Comment    : 
*
****************************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class CriAtomAcfInfo
{
    static partial void GetCueInfoInternal()
    {
        acfInfo = new AcfInfo("ACF", 0, "", "VQ3ADX.acf","4675cac8-829d-4d86-8dae-b0ebac5cf3ee","DspBusSetting_0");
        acfInfo.aisacControlNameList.Add("Level");
        acfInfo.aisacControlNameList.Add("IsTransition");
        acfInfo.aisacControlNameList.Add("TrackVolume1");
        acfInfo.aisacControlNameList.Add("TrackVolume2");
        acfInfo.aisacControlNameList.Add("TrackVolumeEnergy");
        acfInfo.aisacControlNameList.Add("TrackVolumeTransition");
        acfInfo.aisacControlNameList.Add("AisacControl06");
        acfInfo.aisacControlNameList.Add("AisacControl07");
        acfInfo.aisacControlNameList.Add("AisacControl08");
        acfInfo.aisacControlNameList.Add("AisacControl09");
        acfInfo.aisacControlNameList.Add("AisacControl10");
        acfInfo.aisacControlNameList.Add("AisacControl11");
        acfInfo.aisacControlNameList.Add("AisacControl12");
        acfInfo.aisacControlNameList.Add("AisacControl13");
        acfInfo.aisacControlNameList.Add("AisacControl14");
        acfInfo.aisacControlNameList.Add("AisacControl15");
        acfInfo.acbInfoList.Clear();
        AcbInfo newAcbInfo = null;
        newAcbInfo = new AcbInfo("BattleMusic", 0, "", "BattleMusic.acb", "BattleMusic.awb","092c99ee-743c-46a9-be50-4dc8c020b14e");
        acfInfo.acbInfoList.Add(newAcbInfo);
        newAcbInfo.cueInfoList.Add(1, new CueInfo("BattleMusic", 1, ""));
    }
}
