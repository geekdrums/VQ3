using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[ExecuteInEditMode]
public class RevertCommand : PlayerCommand
{
	public MidairPrimitive GrowEdge { get { return plane1.GetComponent<MidairPrimitive>(); } }
	public MidairPrimitive EyeArc { get { return plane2.GetComponent<MidairPrimitive>(); } }
	public MidairPrimitive CenterRect { get { return centerPlane.GetComponent<MidairPrimitive>(); } }
	public MidairPrimitive EyeEdge { get { return centerIcon.GetComponent<MidairPrimitive>(); } }

	Vector3 centerEyePosition;

    void Start()
	{
		ValidateState();
        ValidatePosition();
        ValidateIcons();
        ValidateColor();
		centerEyePosition = EyeArc.transform.localPosition;
    }
    public override void ValidateIcons()
    {
        ValidatePosition();

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
		EyeArc.SetTargetArc((float)GameContext.VoxSystem.currentVP/VoxSystem.InvertVP);

		if( GameContext.VoxSystem.state != VoxState.Invert && GameContext.VoxSystem.IsReadyEclipse )
		{
			CenterRect.SetColor(Color.white);
			EyeEdge.SetColor(Color.clear);
			EyeArc.SetColor(Color.black);
			GrowEdge.SetGrowSize(Music.MusicalSin(4)*2);
			Vector3 lookDirection = (SelectSpot - EyeArc.transform.position);
			lookDirection.z = 0;
			float distance = lookDirection.magnitude;
			lookDirection.Normalize();
			EyeArc.transform.localPosition = centerEyePosition + lookDirection * Mathf.Clamp(distance/3.0f, 0.0f, 2.0f);
			EyeArc.SetTargetWidth(EyeEdge.Radius);
		}
		else
		{
			CenterRect.SetColor(Color.black);
			EyeEdge.SetColor(Color.white);
			EyeArc.SetColor(Color.white);
			GrowEdge.SetGrowSize(0.5f);
			EyeEdge.SetTargetWidth(0.5f);
			EyeArc.transform.localPosition = centerEyePosition;
			EyeArc.SetTargetWidth(EyeEdge.Radius - EyeEdge.Width);
		}
    }
}