using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;


[CustomEditor(typeof(BlendColorSource))]
[CanEditMultipleObjects]
public class BlendColorSourceEditor : ColorSourceBaseEditor
{
	SerializedProperty sourceBeginProperty_;
	SerializedProperty sourceEndProperty_;

	SerializedProperty parameterNameProperty_;
	SerializedProperty valueProperty_;

	bool listFoldOut_ = true;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		if( sourceBeginProperty_ == null )
		{
			sourceBeginProperty_ = serializedObject.FindProperty("SourceBegin");
			sourceEndProperty_ = serializedObject.FindProperty("SourceEnd");
			parameterNameProperty_ = serializedObject.FindProperty("ParameterName");
			valueProperty_ = serializedObject.FindProperty("ParameterValue");
		}

		EditorGUILayout.LabelField("Source", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		{
			float labelWidth = EditorGUIUtility.labelWidth;
			float fieldWidth = EditorGUIUtility.currentViewWidth - labelWidth - 8;

			EditorGUILayout.LabelField("ParameterName", "Parameter");

			EditorGUILayout.BeginHorizontal();
			parameterNameProperty_.stringValue = EditorGUILayout.TextField(GUIContent.none, parameterNameProperty_.stringValue, GUILayout.Width(labelWidth));
			if( ColorManager.ContainsParameter(parameterNameProperty_.stringValue) )
			{
				ColorGameSyncByParameter parameter = ColorManager.GetParameter(parameterNameProperty_.stringValue);
				if( parameter.IsGlobal )
				{
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.Slider(parameter.GlobalValue, parameter.MinParam, parameter.MaxParam, GUILayout.Width(fieldWidth));
					EditorGUI.EndDisabledGroup();
				}
				else
				{
					valueProperty_.floatValue = EditorGUILayout.Slider(valueProperty_.floatValue, parameter.MinParam, parameter.MaxParam, GUILayout.Width(fieldWidth));
				}
			}
			else
			{
				EditorGUILayout.LabelField("---", GUILayout.Width(fieldWidth));
			}
			EditorGUILayout.EndHorizontal();

			DrawSourceProperty("Begin:", sourceBeginProperty_);
			DrawSourceProperty("End:", sourceEndProperty_);
		}
		EditorGUI.indentLevel--;

		DrawBaseInspector();

		serializedObject.ApplyModifiedProperties();
	}

	void DrawSourceProperty(string label, SerializedProperty sourceProperty)
	{
		EditorGUILayout.BeginHorizontal();
		{
			Object newSource = EditorGUILayout.ObjectField(label, sourceProperty.objectReferenceValue, typeof(ColorSourceBase), allowSceneObjects: true);
			if( newSource != sourceProperty.objectReferenceValue )
			{
				sourceProperty.objectReferenceValue = newSource;
				ReferenceChanged(newSource, sourceProperty.objectReferenceValue);
			}
			if( sourceProperty.objectReferenceValue != null )
			{
				EditorGUI.BeginDisabledGroup(true);
				ColorSourceBase sourceColor = (sourceProperty.objectReferenceValue as ColorSourceBase);
				EditorGUILayout.ColorField(sourceColor, GUILayout.Width(65));
				EditorGUI.EndDisabledGroup();
			}
		}
		EditorGUILayout.EndHorizontal();
	}
}