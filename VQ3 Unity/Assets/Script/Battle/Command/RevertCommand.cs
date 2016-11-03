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

	void Awake()
	{
		ValidateState();
	}
    void Start()
	{
        ValidatePosition();
        ValidateIcons();
        ValidateColor();
		centerEyePosition = EyeArc.transform.localPosition;
    }
    public override void ValidateIcons()
    {
        ValidatePosition();

        GrowEdge.SetColor( ColorManager.GetThemeColor( themeColor ).Bright );
    }
    void Update()
    {
#if UNITY_EDITOR
        if( !UnityEditor.EditorApplication.isPlaying ) return;
#endif

		UpdateTransform();

		if( GameContext.BattleState < BattleState.Intro ) return;

		EyeArc.SetTargetArc((float)GameContext.LuxSystem.CurrentVP/GameContext.LuxSystem.OverflowVP);

		if( GameContext.LuxSystem.State != LuxState.Overload && GameContext.LuxSystem.IsOverFlow )
		{
			CenterRect.SetColor(Color.white);
			EyeEdge.SetColor(Color.clear);
			EyeArc.SetColor(Color.black);
			GrowEdge.SetGrowSize(Music.MusicalCos(4));
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

	public override GameObject InstantiateIconObj(GameObject iconParent)
	{
		GameObject iconObj = base.InstantiateIconObj(iconParent);
		iconObj.transform.FindChild("EyeCircle").transform.localPosition = centerEyePosition;
		return iconObj;
	}
}