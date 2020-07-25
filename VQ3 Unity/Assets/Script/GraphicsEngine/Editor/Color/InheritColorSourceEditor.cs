using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;


[CustomEditor(typeof(InheritColorSource))]
public class InheritColorSourceEditor : ColorSourceBaseEditor
{
	SerializedProperty sourceProperty_;

	public override void DrawSourceInspector()
	{
		if( sourceProperty_ == null )
		{
			sourceProperty_ = serializedObject.FindProperty("Source");
		}
		
		EditorGUILayout.BeginHorizontal();
		{
			Object newSource = EditorGUILayout.ObjectField("Source", sourceProperty_.objectReferenceValue, typeof(ColorSourceBase), allowSceneObjects: true);
			if( newSource != sourceProperty_.objectReferenceValue )
			{
				sourceProperty_.objectReferenceValue = newSource;
				ReferenceChanged(newSource, sourceProperty_.objectReferenceValue);
			}
			EditorGUI.BeginDisabledGroup(true);
			if( sourceProperty_.objectReferenceValue != null )
			{
				ColorSourceBase sourceColor = (sourceProperty_.objectReferenceValue as ColorSourceBase);
				EditorGUILayout.ColorField(sourceColor, GUILayout.Width(50));
			}
			EditorGUI.EndDisabledGroup();
		}
		EditorGUILayout.EndHorizontal();
	}
}