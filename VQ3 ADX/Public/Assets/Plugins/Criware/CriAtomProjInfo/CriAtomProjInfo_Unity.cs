﻿/****************************************************************************
*
* CRI Middleware SDK
*
* Copyright (c) 2011-2012 CRI Middleware Co.,Ltd.
*
* Library  : CRI Atom
* Module   : CRI Atom for Unity
* File     : CriAtomProjInfo_Unity.cs
* Tool Ver.          : CRI Atom Craft LE Ver.1.32.00
* Date Time          : 2014/03/07 9:42
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
        newAcbInfo.cueInfoList.Add(2, new CueInfo("continue", 2, ""));
        newAcbInfo = new AcbInfo("SE", 1, "", "SE.acb", "SE.awb","61f6c484-c862-4d1d-b6ad-5f194ae823c8");
        acfInfo.acbInfoList.Add(newAcbInfo);
        newAcbInfo.cueInfoList.Add(2, new CueInfo("PhysicGoodDamage", 2, ""));
        newAcbInfo.cueInfoList.Add(5, new CueInfo("PhysicDamage", 5, ""));
        newAcbInfo.cueInfoList.Add(6, new CueInfo("PhysicBadDamage", 6, ""));
        newAcbInfo.cueInfoList.Add(7, new CueInfo("PhysicNoDamage", 7, ""));
        newAcbInfo.cueInfoList.Add(9, new CueInfo("MagicGoodDamage", 9, ""));
        newAcbInfo.cueInfoList.Add(8, new CueInfo("MagicDamage", 8, ""));
        newAcbInfo.cueInfoList.Add(10, new CueInfo("MagicBadDamage", 10, ""));
        newAcbInfo.cueInfoList.Add(11, new CueInfo("MagicNoDamage", 11, ""));
        newAcbInfo.cueInfoList.Add(4, new CueInfo("EnemyHeal", 4, ""));
        newAcbInfo.cueInfoList.Add(0, new CueInfo("PlayerHeal", 0, ""));
        newAcbInfo.cueInfoList.Add(3, new CueInfo("PlayerDefend", 3, ""));
        newAcbInfo.cueInfoList.Add(1, new CueInfo("PlayerPhysicDamage", 1, ""));
        newAcbInfo.cueInfoList.Add(12, new CueInfo("PlayerMagicDamage", 12, ""));
        newAcbInfo.cueInfoList.Add(13, new CueInfo("select", 13, ""));
        newAcbInfo.cueInfoList.Add(14, new CueInfo("tick", 14, ""));
        newAcbInfo.cueInfoList.Add(15, new CueInfo("tickback", 15, ""));
        newAcbInfo.cueInfoList.Add(16, new CueInfo("runaway", 16, ""));
        newAcbInfo.cueInfoList.Add(17, new CueInfo("newCommand", 17, ""));
        newAcbInfo.cueInfoList.Add(18, new CueInfo("quarter2", 18, ""));
        newAcbInfo.cueInfoList.Add(19, new CueInfo("quarter3", 19, ""));
        newAcbInfo.cueInfoList.Add(20, new CueInfo("quarter4", 20, ""));
        newAcbInfo.cueInfoList.Add(21, new CueInfo("moon", 21, ""));
        newAcbInfo.cueInfoList.Add(22, new CueInfo("levelUp", 22, ""));
    }
}
