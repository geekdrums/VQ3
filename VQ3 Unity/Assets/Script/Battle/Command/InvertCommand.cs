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

	public MidairPrimitive CenterRect1 { get { return plane1.GetComponent<MidairPrimitive>(); } }
	public MidairPrimitive CenterRect2 { get { return plane2.GetComponent<MidairPrimitive>(); } }


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
        if( linkLines == null ) linkLines = new List<CommandEdge>();
        ValidatePosition();
        ValidateIcons();
        ValidateColor();
    }
    public override void ValidateIcons()
    {

    }
	protected override void UpdateIcon()
	{
		if( Music.isJustChanged )
		{
			CommandGraph commandGraph = GetComponentInParent<CommandGraph>();
			float distance = ((ParentCommand != null ? ParentCommand.transform.position : this.transform.position) - SelectSpot).magnitude;
			if( IsLinked )
			{
				transform.localScale = commandGraph.MaxScale * (1.0f - distance * commandGraph.ScaleCoeff) * 0.8f;//temp
			}
			else
			{
				transform.localScale = commandGraph.MaxScale * (1.0f - distance * commandGraph.ScaleCoeff) * 0.8f;
			}
		}
		transform.rotation = Quaternion.identity;
	}
    void Update()
    {
#if UNITY_EDITOR
        if( !UnityEditor.EditorApplication.isPlaying ) return;
#endif
		if( ParentCommand != null ) return;
        UpdateIcon();

		if( GameContext.VoxSystem.state != VoxState.Invert )
		{
			CenterRect1.SetSize(0);
			CenterRect2.SetSize(0);
		}
		else
		{
			CenterRect1.SetSize(7 + Music.MusicalSin(4) * 2 - 1);
			CenterRect2.SetSize(9 + Music.MusicalSin(4,2) * 2 - 1);
		}
    }
}