using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class QuarterCommand : Command
{
    public int AcqureLevel = 1;

    public override string GetBlockName()
    {
        return MusicBlockName + GameContext.PlayerConductor.NumQuarter.ToString();
    }
    public override bool IsUsable()
    {
        return GameContext.PlayerConductor.Level >= AcqureLevel && base.IsUsable();
    }
}