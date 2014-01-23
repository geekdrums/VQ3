using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class QuarterCommand : Command
{
    public bool IsWait() { return Music.Just.bar >= Level; }

    public override string GetBlockName()
    {
        return MusicBlockName + Level.ToString();
        //MusicBlockName.Substring(0,Level) + new string( 'W', 4-Level );
    }
}