using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[ExecuteInEditMode]
public class InvertCommand : PlayerCommand
{
    public int DepthLevel;
	public bool IsLast { get { return DepthLevel >= 1; } }

	public MidairPrimitive CenterRect1 { get { return plane1.GetComponent<MidairPrimitive>(); } }
	public MidairPrimitive CenterRect2 { get { return plane2.GetComponent<MidairPrimitive>(); } }


    public override void SetLink( bool linked )
    {
        base.SetLink( linked && (GameContext.VoxSystem.state == VoxState.Invert || GameContext.VoxSystem.IsOverFlow) );
    }

    void Start()
	{
		ValidateState();
        ValidatePosition();
        ValidateIcons();
        ValidateColor();
    }
    public override void ValidateIcons()
    {

    }
	//protected override void UpdateIcon()
	//{
	//	if( Music.isJustChanged )
	//	{
	//		CommandGraph commandGraph = GetComponentInParent<CommandGraph>();
	//		float distance = (this.transform.position - SelectSpot).magnitude;
	//		if( state >= CommandState.Linked )
	//		{
	//			transform.localScale = commandGraph.MaxScale * (1.0f - distance * commandGraph.ScaleCoeff) * 0.8f;//temp
	//		}
	//		else
	//		{
	//			transform.localScale = commandGraph.MaxScale * (1.0f - distance * commandGraph.ScaleCoeff) * 0.8f;
	//		}
	//	}
	//	transform.rotation = Quaternion.identity;
	//}

	protected override void UpdateIcon()
	{
		if( Music.IsJustChanged )
		{
			CommandGraph commandGraph = GetComponentInParent<CommandGraph>();
			float alpha = 0;
			float distance = (this.transform.position - SelectSpot).magnitude;
			if( GameContext.CurrentState == GameState.SetMenu || GameContext.CurrentState == GameState.Result )
			{
				CenterRect1.SetColor(Color.white);
				CenterRect2.SetColor(Color.white);
				GetComponent<MidairPrimitive>().SetColor(Color.white);
				if( state <= CommandState.Acquired )
				{
					transform.localScale = commandGraph.MaxScale * (1.0f - distance * commandGraph.ScaleCoeff) * 1.0f;
				}
				else if( state <= CommandState.NotAcquired )
				{
					transform.localScale = commandGraph.MaxScale * (1.0f - distance * commandGraph.ScaleCoeff) * 0.8f;
					alpha = 0.85f;
				}
				else//DontKnow
				{
					transform.localScale = Vector3.zero;
				}
				levelCounter.transform.localScale = Vector3.one;
			}
			else
			{
				CenterRect1.SetColor(Color.black);
				CenterRect2.SetColor(Color.black);
				GetComponent<MidairPrimitive>().SetColor(Color.black);
				if( state <= CommandState.Linked )
				{
					transform.localScale = commandGraph.MaxScale * (1.0f - distance * commandGraph.ScaleCoeff) * 0.8f;
				}
				else if( state <= CommandState.Acquired )
				{
					transform.localScale = commandGraph.MaxScale * (1.0f - distance * commandGraph.ScaleCoeff) * 0.8f;
					alpha = Mathf.Clamp((distance + commandGraph.MaskStartPos) * commandGraph.MaskColorCoeff, 0.7f, 1.0f);
				}
				else//NotAcquired,DontKnow
				{
					transform.localScale = Vector3.zero;
				}
				levelCounter.transform.localScale = Vector3.zero;
			}
			maskPlane.GetComponent<Renderer>().material.color = ColorManager.MakeAlpha(Color.black, alpha);
		}
		transform.rotation = Quaternion.identity;
	}
    void Update()
    {
#if UNITY_EDITOR
        if( !UnityEditor.EditorApplication.isPlaying ) return;
#endif
        UpdateIcon();

		if( GameContext.VoxSystem.state != VoxState.Invert )
		{
			CenterRect1.SetSize(0);
			CenterRect2.SetSize(0);
		}
		else
		{
			CenterRect1.SetSize(7 + Music.MusicalCos(4) * 2 - 1);
			CenterRect2.SetSize(9 + Music.MusicalCos(4, 2) * 2 - 1);
		}
    }
}