/****************************************************************************
*
* CRI Middleware SDK
*
* Copyright (c) 2011-2012 CRI Middleware Co., Ltd.
*
* Library  : CRI Atom
* Module   : CRI Atom for Unity
* File     : CriAtomProjInfo_Unity.cs
* Tool Ver.          : CRI Atom Craft LE Ver.2.13.00
* Date Time          : 2015/06/28 20:16
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
        acfInfo.aisacControlNameList.Add("AisacControl00");
        acfInfo.aisacControlNameList.Add("AisacControl01");
        acfInfo.aisacControlNameList.Add("TrackVolume1");
        acfInfo.aisacControlNameList.Add("TrackVolume2");
        acfInfo.aisacControlNameList.Add("TrackVolumeEnergy");
        acfInfo.aisacControlNameList.Add("TrackVolumeTransition");
        acfInfo.aisacControlNameList.Add("TrackVolumeLoop");
        acfInfo.aisacControlNameList.Add("IsTransition");
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
        newAcbInfo.cueInfoList.Add(3, new CueInfo("Invert", 3, ""));
        newAcbInfo.cueInfoList.Add(4, new CueInfo("ambient", 4, ""));
        newAcbInfo = new AcbInfo("SE", 1, "", "SE.acb", "SE.awb","61f6c484-c862-4d1d-b6ad-5f194ae823c8");
        acfInfo.acbInfoList.Add(newAcbInfo);
        newAcbInfo.cueInfoList.Add(2, new CueInfo("PhysicGoodDamage", 2, ""));
        newAcbInfo.cueInfoList.Add(5, new CueInfo("PhysicDamage", 5, ""));
        newAcbInfo.cueInfoList.Add(6, new CueInfo("PhysicBadDamage", 6, ""));
        newAcbInfo.cueInfoList.Add(7, new CueInfo("PhysicNoDamage", 7, ""));
        newAcbInfo.cueInfoList.Add(26, new CueInfo("MagicGoodDamage", 26, ""));
        newAcbInfo.cueInfoList.Add(9, new CueInfo("MagicDamage", 9, ""));
        newAcbInfo.cueInfoList.Add(8, new CueInfo("MagicBadDamage", 8, ""));
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
        newAcbInfo.cueInfoList.Add(23, new CueInfo("enhance", 23, ""));
        newAcbInfo.cueInfoList.Add(24, new CueInfo("invert", 24, ""));
        newAcbInfo.cueInfoList.Add(25, new CueInfo("jam", 25, ""));
        newAcbInfo.cueInfoList.Add(27, new CueInfo("footstep", 27, ""));
        newAcbInfo.cueInfoList.Add(28, new CueInfo("flare", 28, ""));
        newAcbInfo.cueInfoList.Add(29, new CueInfo("poisonBreath", 29, ""));
        newAcbInfo.cueInfoList.Add(30, new CueInfo("breath", 30, ""));
        newAcbInfo.cueInfoList.Add(31, new CueInfo("commandLevelDown", 31, ""));
        newAcbInfo.cueInfoList.Add(32, new CueInfo("commandLevelUp", 32, ""));
        newAcbInfo.cueInfoList.Add(33, new CueInfo("spawn", 33, ""));
    }
}
