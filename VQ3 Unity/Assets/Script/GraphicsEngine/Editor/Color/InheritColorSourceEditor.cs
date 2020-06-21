using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;


[CustomEditor(typeof(InheritColorSource))]
[CanEditMultipleObjects]
public class InheritColorSourceEditor : ColorSourceBaseEditor
{
	SerializedProperty sourceProperty_;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

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

		//EditorGUILayout.LabelField("SourceHSVA", string.Format("H:{0:F3}, S:{1:F3}, V:{2:F3}, A:{3:F3}", sourceColor.H, sourceColor.S, sourceColor.V, sourceColor.A));

		DrawBaseInspector();

		serializedObject.ApplyModifiedProperties();
	}
}