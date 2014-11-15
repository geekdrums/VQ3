using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[ExecuteInEditMode]
public class InvertCommand : PlayerCommand
{
    public int DepthLevel;
    public bool IsLast { get { return DepthLevel >= 2; } }

    public override void SetLink( bool linked )
    {
        base.SetLink( linked && (GameContext.VoxSystem.state == VoxState.Invert || GameContext.VoxSystem.IsReadyEclipse) );
    }

    void Start()
    {
        Parse();
        IsLinked = false;
        IsCurrent = false;
        IsSelected = false;
        if( linkLines == null ) linkLines = new List<LineRenderer>();
        ValidatePosition();
        ValidateIcons();
        ValidateColor();
    }
    public override void ValidateIcons()
    {

    }
    void Update()
    {
#if UNITY_EDITOR
        if( !UnityEditor.EditorApplication.isPlaying ) return;
#endif
        UpdateIcon();
    }
}