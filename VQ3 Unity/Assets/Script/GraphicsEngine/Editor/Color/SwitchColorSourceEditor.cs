using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;


[CustomEditor(typeof(SwitchColorSource))]
public class SwitchColorSourceEditor : ColorSourceBaseEditor
{
	SerializedProperty sourceListProperty_;
	SerializedProperty stateGroupProperty_;
	SerializedProperty stateProperty_;

	bool listFoldOut_ = true;

	public override void DrawSourceInspector()
	{
		if( sourceListProperty_ == null )
		{
			sourceListProperty_ = serializedObject.FindProperty("SourceList");
			stateGroupProperty_ = serializedObject.FindProperty("StateGroupName");
			stateProperty_ = serializedObject.FindProperty("StateName");
		}

		EditorGUILayout.LabelField("Source", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		{
			float labelWidth = EditorGUIUtility.labelWidth;
			float fieldWidth = EditorGUIUtility.currentViewWidth - labelWidth - 8;
			ColorGameSyncByState stateGroup = null;

			EditorGUILayout.LabelField("StateGroup", "State");

			EditorGUILayout.BeginHorizontal();
			stateGroupProperty_.stringValue = EditorGUILayout.TextField(GUIContent.none, stateGroupProperty_.stringValue, GUILayout.Width(labelWidth));
			if( ColorManager.ContainsStateGroup(stateGroupProperty_.stringValue) )
			{
				stateGroup = ColorManager.GetStateGroup(stateGroupProperty_.stringValue);

				if( stateGroup.IsGlobal )
				{
					LocalDisableGroup(() =>
					{
						int index = Mathf.Max(0, stateGroup.States.IndexOf(stateGroup.GlobalValue));
						EditorGUILayout.LabelField(stateGroup.States[index], GUILayout.Width(fieldWidth));
					});
				}
				else
				{
					LocalEnableGroup(() =>
					{
						int index = Mathf.Max(0, stateGroup.States.IndexOf(stateProperty_.stringValue));
						index = EditorGUILayout.Popup(index, stateGroup.States.ToArray(), GUILayout.Width(fieldWidth));
						stateProperty_.stringValue = stateGroup.States[index];
					});
				}
			}
			else
			{
				EditorGUILayout.LabelField("---", GUILayout.Width(fieldWidth));
			}
			EditorGUILayout.EndHorizontal();

			if( stateGroup != null )
			{
				sourceListProperty_.arraySize = stateGroup.States.Count;

				listFoldOut_ = EditorGUILayout.Foldout(listFoldOut_, "Source List");
				if( listFoldOut_ )
				{
					EditorGUI.indentLevel++;
					for( int i = 0; i < sourceListProperty_.arraySize; ++i )
					{
						EditorGUILayout.BeginHorizontal();
						{
							SerializedProperty sourceObjectProp = sourceListProperty_.GetArrayElementAtIndex(i);
							Object newSource = EditorGUILayout.ObjectField(stateGroup.States[i] + ":", sourceObjectProp.objectReferenceValue, typeof(ColorSourceBase), allowSceneObjects: true);
							if( newSource != sourceObjectProp.objectReferenceValue )
							{
								sourceObjectProp.objectReferenceValue = newSource;
								ReferenceChanged(newSource, sourceObjectProp.objectReferenceValue);
							}
							if( sourceObjectProp.objectReferenceValue != null )
							{
								LocalDisableGroup(() =>
								{
									ColorSourceBase sourceColor = (sourceObjectProp.objectReferenceValue as ColorSourceBase);
									EditorGUILayout.ColorField(sourceColor, GUILayout.Width(80));
								});

							}
						}
						EditorGUILayout.EndHorizontal();
					}
					EditorGUI.indentLevel--;
				}
			}
			else
			{
				EditorGUILayout.PropertyField(sourceListProperty_, includeChildren: true);
			}
		}
		EditorGUI.indentLevel--;
	}
}