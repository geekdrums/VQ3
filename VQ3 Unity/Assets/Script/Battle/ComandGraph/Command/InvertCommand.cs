using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class InvertCommand : PlayerCommand
{

    public override void SetLink( bool linked )
    {
        base.SetLink( linked && ( GameContext.VoxSystem.state == VoxState.Eclipse || GameContext.VoxSystem.state == VoxState.Invert ) );
    }
}