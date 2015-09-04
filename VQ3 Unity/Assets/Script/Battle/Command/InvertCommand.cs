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


	void Awake()
	{
		ValidateState();
	}

    void Start()
	{
        ValidatePosition();
        ValidateColor();
    }

	void Update()
	{
#if UNITY_EDITOR
		if( !UnityEditor.EditorApplication.isPlaying ) return;
#endif
		UpdateTransform();

		if( GameContext.VoxSystem.State != VoxState.Overload )
		{
			GetComponent<MidairPrimitive>().SetColor(Color.white);
		}
		else
		{
			GetComponent<MidairPrimitive>().SetColor(Color.black);
		}
	}

	public override void SetLink(bool linked)
	{
		base.SetLink(linked && (GameContext.VoxSystem.State == VoxState.Overload || GameContext.VoxSystem.IsOverFlow));
	}
}