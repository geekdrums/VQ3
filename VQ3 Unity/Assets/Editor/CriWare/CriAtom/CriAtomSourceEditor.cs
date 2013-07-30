/****************************************************************************
 *
 * CRI Middleware SDK
 *
 * Copyright (c) 2011-2012 CRI Middleware Co.,Ltd.
 *
 * Library  : CRI Atom
 * Module   : CRI Atom for Unity Editor
 * File     : CriAtomSourceEditor.cs
 *
 ****************************************************************************/
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

[CustomEditor(typeof(CriAtomSource))]
public class CriAtomSourceEditor : Editor
{
	#region Variables
	private CriAtomSource source = null;
	private bool showAisac;
	//private bool showPreview = true;
	#endregion

	#region Functions
	private void OnEnable()
	{
		this.source = (CriAtomSource)base.target;
	}
	
	public override void OnInspectorGUI()
	{
		if (this.source == null) {
			return;
		}
		
		EditorGUI.indentLevel++;
		this.source.cueSheet = EditorGUILayout.TextField("Cue Sheet", this.source.cueSheet);
		this.source.cueName = EditorGUILayout.TextField("Cue Name", this.source.cueName);		
		this.source.playOnStart = EditorGUILayout.Toggle("Play On Start", this.source.playOnStart);
		EditorGUILayout.Space();
		this.source.volume = EditorGUILayout.Slider("Volume", this.source.volume, 0.0f, 1.0f);
		this.source.pitch = EditorGUILayout.Slider("Pitch", this.source.pitch, -1200f, 1200);

        #region AISAC
		this.showAisac = EditorGUILayout.Foldout(this.showAisac, "AISAC Control");
		EditorGUI.indentLevel++;
		if (this.showAisac) {
			for (uint i = 0; i < CriAtomSource.MaxAisac; i++) {
				float aisac = this.source.GetAisac(i);
				aisac = EditorGUILayout.Slider("AISAC" + i, aisac, 0.0f, 1.0f);
				this.source.SetAisac(i, aisac);
			}
		}
		EditorGUI.indentLevel--;
		EditorGUI.indentLevel--;

        #endregion

        /*#region preview
		if (this.source.acb != null) {
			this.showPreview = EditorGUILayout.Foldout(this.showPreview, "Preview");
			if (showPreview) {
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.PrefixLabel("  ");
					if (GUILayout.Button("Play", EditorStyles.miniButtonLeft)) {
						this.source.Play();
					}
					if (GUILayout.Button("Stop", EditorStyles.miniButtonRight)) {
						this.source.Stop();
					}
				}
				EditorGUILayout.EndHorizontal();
			}
		}
        #endregion*/
	}
	#endregion
} // end of class

/* end of file */
