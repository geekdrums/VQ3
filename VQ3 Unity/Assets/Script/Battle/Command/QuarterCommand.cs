﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class QuarterCommand : Command
{
    public override string GetBlockName()
    {
        return MusicBlockName + GameContext.PlayerConductor.NumQuarter.ToString();
    }
}