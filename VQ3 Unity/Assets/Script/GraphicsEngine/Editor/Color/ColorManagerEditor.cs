using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(ColorManager))]
[CanEditMultipleObjects]
public class ColorManagerEditor : Editor
{
	SerializedProperty stateGroupsProperty_;
	SerializedProperty parametersProperty_;
	ColorGameSyncByState updatedState_ = null;
	ColorGameSyncByParameter updatedParameter_ = null;
	bool stateGroupFoldOut_ = true;
	bool parameterFoldOut_ = true;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		if( stateGroupsProperty_ == null )
		{
			stateGroupsProperty_ = serializedObject.FindProperty("StateGroups").FindPropertyRelative("list");
			parametersProperty_ = serializedObject.FindProperty("Parameters").FindPropertyRelative("list");
		}

		if( serializedObject.targetObject != ColorManager.Instance )
		{
			EditorGUILayout.LabelField("There are another ColorManager in scene.");
			return;
		}

		EditorGUI.indentLevel++;
		DrawStateGroups();
		EditorGUI.indentLevel--;

		EditorGUI.indentLevel++;
		DrawParameters();
		EditorGUI.indentLevel--;

		serializedObject.ApplyModifiedProperties();

		if( updatedState_ != null )
		{
			updatedState_.RecalcurateReferenceColor();
			updatedState_ = null;
		}
		if( updatedParameter_ != null )
		{
			updatedParameter_.RecalcurateReferenceColor();
			updatedParameter_ = null;
		}
	}

	void DrawStateGroups()
	{
		float defaultLabelWidth = EditorGUIUtility.labelWidth;
		float buttonWidth = 50;

		EditorGUILayout.BeginHorizontal();
		// <image src="..\..\imagecomment\QS_20200517-164241.png"/>
		stateGroupFoldOut_ = EditorGUILayout.Foldout(stateGroupFoldOut_, "StateGroups");
		bool added = false;
		if( GUILayout.Button("+", GUILayout.Width(buttonWidth)) )
		{
			stateGroupsProperty_.InsertArrayElementAtIndex(stateGroupsProperty_.arraySize);
			added = true;
		}
		EditorGUILayout.EndHorizontal();

		if( stateGroupFoldOut_ == false )
		{
			return;
		}

		int removeIndex = -1;
		EditorGUI.indentLevel++;
		for( int i = 0; i < stateGroupsProperty_.arraySize; ++i )
		{
			SerializedProperty arrayElemProp = stateGroupsProperty_.GetArrayElementAtIndex(i);
			SerializedProperty keyProp = arrayElemProp.FindPropertyRelative("Key");
			SerializedProperty valueProp = arrayElemProp.FindPropertyRelative("Value");
			SerializedProperty stateFoldOut = valueProp.FindPropertyRelative("IsFoldOut");

			EditorGUILayout.BeginHorizontal();
			// <image src="..\..\imagecomment\QS_20200517-164518.png"/>
			stateFoldOut.boolValue = EditorGUILayout.Foldout(stateFoldOut.boolValue, keyProp.stringValue);
			if( GUILayout.Button("-", GUILayout.Width(buttonWidth)) )
			{
				removeIndex = i;
			}
			EditorGUILayout.EndHorizontal();
			
			if( stateFoldOut.boolValue )
			{
				// KeyはGroupName
				keyProp.stringValue = EditorGUILayout.TextField(GUIContent.none, keyProp.stringValue);
				// GroupNameはKeyと同じ
				valueProp.FindPropertyRelative("GroupName").stringValue = keyProp.stringValue;

				// ColorGameSyncByState
				EditorGUI.indentLevel++;
				{
					ColorGameSyncByState stateGroup = null;
					if( added == false )
					{
						stateGroup = (serializedObject.targetObject as ColorManager).GetStateGroup_(i);
					}
					// State Names
					SerializedProperty stateNamesProperty = valueProp.FindPropertyRelative("States");
					EditorGUILayout.PropertyField(stateNamesProperty, includeChildren: true);

					// PropertyEditList
					SerializedProperty propertyEditFoldOut = valueProp.FindPropertyRelative("IsPropertyEditListFoldOut");
					propertyEditFoldOut.boolValue = EditorGUILayout.Foldout(propertyEditFoldOut.boolValue, "PropertyEditList");
					EditorGUI.indentLevel++;
					if( propertyEditFoldOut.boolValue )
					{
						// <image src="..\..\imagecomment\QS_20200517-164654.png"/>
						SerializedProperty editListProperty = valueProp.FindPropertyRelative("PropertyEditList");
						editListProperty.arraySize = EditorGUILayout.IntField("Size", editListProperty.arraySize);
						for( int j = 0; j < editListProperty.arraySize; ++j )
						{
							SerializedProperty colorPropertyEdit = editListProperty.GetArrayElementAtIndex(j);
							SerializedProperty propertyType = colorPropertyEdit.FindPropertyRelative("PropertyType");
							SerializedProperty editType = colorPropertyEdit.FindPropertyRelative("EditType");

							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.PropertyField(propertyType, new GUIContent("Property"), GUILayout.Width(defaultLabelWidth + 60));
							EditorGUILayout.PropertyField(editType, GUIContent.none);
							EditorGUILayout.EndHorizontal();

							SerializedProperty valuesProperty = colorPropertyEdit.FindPropertyRelative("Values");
							if( valuesProperty.arraySize != stateNamesProperty.arraySize )
							{
								// ValueはStateNameに対応するので数を揃える
								valuesProperty.arraySize = stateNamesProperty.arraySize;
							}
							if( stateGroup != null )
							{
								EditorGUI.indentLevel++;
								for( int k = 0; k < valuesProperty.arraySize; ++k )
								{
									EditorGUILayout.PropertyField(valuesProperty.GetArrayElementAtIndex(k), new GUIContent(stateGroup.States[k]));
								}
								EditorGUI.indentLevel--;
							}
						}
					}
					EditorGUI.indentLevel--;

					// Global
					SerializedProperty isGlobalProperty = valueProp.FindPropertyRelative("IsGlobal");
					{
						EditorGUILayout.PropertyField(isGlobalProperty);
						if( stateGroup != null && isGlobalProperty.boolValue && stateNamesProperty.arraySize > 0 )
						{
							SerializedProperty globalStateProperty = valueProp.FindPropertyRelative("GlobalValue");
							int index = Mathf.Max(0, stateGroup.States.IndexOf(globalStateProperty.stringValue));
							int newIndex = EditorGUILayout.Popup("Global State", index, stateGroup.States.ToArray());
							if( newIndex != index )
							{
								globalStateProperty.stringValue = stateGroup.States[newIndex];
								updatedState_ = stateGroup;
							}
						}
					}

					// Transition
					EditorGUILayout.PropertyField(valueProp.FindPropertyRelative("TransitionTime"));

					// reference
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.PropertyField(valueProp.FindPropertyRelative("beReferencedColors_"), includeChildren: true);
					EditorGUI.EndDisabledGroup();
				}
				EditorGUI.indentLevel--;
			}
		}
		EditorGUI.indentLevel--;
		if( removeIndex >= 0 )
		{
			stateGroupsProperty_.DeleteArrayElementAtIndex(removeIndex);
		}
	}

	void DrawParameters()
	{
		float defaultLabelWidth = EditorGUIUtility.labelWidth;
		float buttonWidth = 50;

		EditorGUILayout.BeginHorizontal();
		// <image src="..\..\imagecomment\QS_20200517-171345.png"/>
		parameterFoldOut_ = EditorGUILayout.Foldout(parameterFoldOut_, "Parameters");
		bool added = false;
		if( GUILayout.Button("+", GUILayout.Width(buttonWidth)) )
		{
			parametersProperty_.InsertArrayElementAtIndex(parametersProperty_.arraySize);
			added = true;
		}
		EditorGUILayout.EndHorizontal();

		if( parameterFoldOut_ == false )
		{
			return;
		}

		int removeIndex = -1;
		EditorGUI.indentLevel++;
		for( int i = 0; i < parametersProperty_.arraySize; ++i )
		{
			SerializedProperty arrayElemProp = parametersProperty_.GetArrayElementAtIndex(i);
			SerializedProperty keyProp = arrayElemProp.FindPropertyRelative("Key");
			SerializedProperty valueProp = arrayElemProp.FindPropertyRelative("Value");
			SerializedProperty paramFoldOut = valueProp.FindPropertyRelative("IsFoldOut");

			EditorGUILayout.BeginHorizontal();
			// <image src="..\..\imagecomment\QS_20200517-171416.png"/>
			paramFoldOut.boolValue = EditorGUILayout.Foldout(paramFoldOut.boolValue, keyProp.stringValue);
			if( GUILayout.Button("-", GUILayout.Width(buttonWidth)) )
			{
				removeIndex = i;
			}
			EditorGUILayout.EndHorizontal();
			
			if( paramFoldOut.boolValue )
			{
				// KeyはGroupName
				keyProp.stringValue = EditorGUILayout.TextField(GUIContent.none, keyProp.stringValue);
				// GroupNameはKeyと同じ
				valueProp.FindPropertyRelative("ParameterName").stringValue = keyProp.stringValue;

				// ColorGameSyncByParameter
				EditorGUI.indentLevel++;
				{
					EditorGUILayout.PropertyField(valueProp.FindPropertyRelative("MinParam"));
					EditorGUILayout.PropertyField(valueProp.FindPropertyRelative("MaxParam"));
					
					// PropertyEditList
					SerializedProperty propertyEditFoldOut = valueProp.FindPropertyRelative("IsPropertyEditListFoldOut");
					propertyEditFoldOut.boolValue = EditorGUILayout.Foldout(propertyEditFoldOut.boolValue, "PropertyEditList");
					EditorGUI.indentLevel++;
					if( propertyEditFoldOut.boolValue )
					{
						// <image src="..\..\imagecomment\QS_20200517-171308.png"/>
						SerializedProperty editListProperty = valueProp.FindPropertyRelative("PropertyEditList");
						editListProperty.arraySize = EditorGUILayout.IntField("Size", editListProperty.arraySize);
						for( int j = 0; j < editListProperty.arraySize; ++j )
						{
							SerializedProperty colorPropertyEdit = editListProperty.GetArrayElementAtIndex(j);
							SerializedProperty propertyType = colorPropertyEdit.FindPropertyRelative("PropertyType");
							SerializedProperty editType = colorPropertyEdit.FindPropertyRelative("EditType");

							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.PropertyField(propertyType, new GUIContent("Property"), GUILayout.Width(defaultLabelWidth + 60));
							EditorGUILayout.PropertyField(editType, GUIContent.none);
							EditorGUILayout.EndHorizontal();

							EditorGUI.indentLevel++;
							{
								EditorGUILayout.PropertyField(colorPropertyEdit.FindPropertyRelative("MinValue"));
								EditorGUILayout.PropertyField(colorPropertyEdit.FindPropertyRelative("MaxValue"));
							}
							EditorGUI.indentLevel--;
						}
					}
					EditorGUI.indentLevel--;

					// Global
					SerializedProperty isGlobalProperty = valueProp.FindPropertyRelative("IsGlobal");
					{
						ColorGameSyncByParameter parameter = null;
						if( added == false )
						{
							parameter = (serializedObject.targetObject as ColorManager).GetParameter_(i);
						}
						EditorGUILayout.PropertyField(isGlobalProperty);
						if( parameter != null && isGlobalProperty.boolValue && parametersProperty_.arraySize > 0 )
						{
							SerializedProperty globalParamProperty = valueProp.FindPropertyRelative("GlobalValue");
							float newValue = EditorGUILayout.Slider("Global Value", globalParamProperty.floatValue, parameter.MinParam, parameter.MaxParam);
							if( newValue != globalParamProperty.floatValue )
							{
								globalParamProperty.floatValue = newValue;
								updatedParameter_ = parameter;
							}
						}
					}

					// Transition
					EditorGUILayout.PropertyField(valueProp.FindPropertyRelative("TransitionTime"));

					// reference
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.PropertyField(valueProp.FindPropertyRelative("beReferencedColors_"), includeChildren: true);
					EditorGUI.EndDisabledGroup();
				}
				EditorGUI.indentLevel--;
			}
		}
		EditorGUI.indentLevel--;
		if( removeIndex >= 0 )
		{
			parametersProperty_.DeleteArrayElementAtIndex(removeIndex);
		}
	}
}