using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[ExecuteInEditMode]
public class RevertCommand : PlayerCommand
{
    public MidairPrimitive GrowEdge { get { return plane1.GetComponent<MidairPrimitive>(); } }
    public GameObject CenterCircle { get { return centerPlane; } }
    public GameObject CenterEye { get { return centerIcon; } }

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
        ValidatePosition();

        string iconStr = "";
        foreach( EStatusIcon ic in icons ) iconStr += ic.ToString();

        themeColor = EThemeColor.White;
        if( iconStr.Contains( 'A' ) )
        {
            themeColor = EThemeColor.Blue;
        }
        else if( iconStr.Contains( 'W' ) )
        {
            themeColor = EThemeColor.Green;
        }
        else if( iconStr.Contains( 'F' ) )
        {
            themeColor = EThemeColor.Red;
        }
        else if( iconStr.Contains( 'L' ) )
        {
            themeColor = EThemeColor.Yellow;
        }

        GrowEdge.SetColor( ColorManager.GetThemeColor( themeColor ).Bright );
    }
    void Update()
    {
#if UNITY_EDITOR
        if( !UnityEditor.EditorApplication.isPlaying ) return;
#endif
        UpdateIcon();
    }
}