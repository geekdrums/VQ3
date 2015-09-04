using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[ExecuteInEditMode]
public class IntroCommand : PlayerCommand
{
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
    }
	public override bool IsUsable()
	{
		return GameContext.State == GameState.Setting ? true : false;
	}
}