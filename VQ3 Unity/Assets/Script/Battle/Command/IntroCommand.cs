using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[ExecuteInEditMode]
public class IntroCommand : PlayerCommand
{
    void Start()
    {
		ValidateState();
        ValidatePosition();
        ValidateIcons();
        ValidateColor();
    }
    void Update()
	{
#if UNITY_EDITOR
		if( !UnityEditor.EditorApplication.isPlaying ) return;
#endif
		UpdateIcon();
    }
	public override bool IsUsable()
	{
		return GameContext.CurrentState == GameState.SetMenu ? true : false;
	}
}