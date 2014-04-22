using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class InvertCommand : PlayerCommand
{
    public bool isLast;

    public override void SetLink( bool linked )
    {
        base.SetLink( linked && (GameContext.VoxSystem.state == VoxState.Invert || GameContext.VoxSystem.IsReadyEclipse) );
    }
}