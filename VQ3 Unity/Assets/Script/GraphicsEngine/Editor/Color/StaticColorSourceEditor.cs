using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;


[CustomEditor(typeof(StaticColorSource))]
public class StaticColorSourceEditor : ColorSourceBaseEditor
{
	SerializedProperty sourceProperty_;

	public override void DrawSourceInspector()
	{
		if( sourceProperty_ == null )
		{
			sourceProperty_ = serializedObject.FindProperty("Source");
		}
		
		EditorGUILayout.PropertyField(sourceProperty_);

		if( serializedObject.isEditingMultipleObjects == false )
		{
			StaticColorSource sourceColor = (serializedObject.targetObject as StaticColorSource);
			float h, s, v, a;
			sourceColor.GetSourceHSVA(out h, out s, out v, out a);
			//EditorGUILayout.LabelField("SourceHSVA", string.Format("H:{0:F3}, S:{1:F3}, V:{2:F3}, A:{3:F3}", h, s, v, a));
		}
	}
}