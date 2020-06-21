using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(ColorSourceBase))]
[CanEditMultipleObjects]
public class ColorSourceBaseEditor : Editor
{
	SerializedProperty targetProperty_;
	SerializedProperty editTypeHProperty_;
	SerializedProperty editTypeSProperty_;
	SerializedProperty editTypeVProperty_;
	SerializedProperty editTypeAProperty_;
	
	SerializedProperty editValueHProperty_;
	SerializedProperty editValueSProperty_;
	SerializedProperty editValueVProperty_;
	SerializedProperty editValueAProperty_;

	SerializedProperty stateDictProperty_;
	SerializedProperty parameterDictProperty_;

	bool staticEditFoldOut_ = true;
	bool interactiveEditFoldOut_ = true;

	protected void DrawBaseInspector()
	{
		if( targetProperty_ == null )
		{
			targetProperty_ = serializedObject.FindProperty("Target");
			editTypeHProperty_ = serializedObject.FindProperty("EditTypeH");
			editTypeSProperty_ = serializedObject.FindProperty("EditTypeS");
			editTypeVProperty_ = serializedObject.FindProperty("EditTypeV");
			editTypeAProperty_ = serializedObject.FindProperty("EditTypeA");
			editValueHProperty_ = serializedObject.FindProperty("EditValueH");
			editValueSProperty_ = serializedObject.FindProperty("EditValueS");
			editValueVProperty_ = serializedObject.FindProperty("EditValueV");
			editValueAProperty_ = serializedObject.FindProperty("EditValueA");
			stateDictProperty_ = serializedObject.FindProperty("StateDict").FindPropertyRelative("list");
			parameterDictProperty_ = serializedObject.FindProperty("ParameterDict").FindPropertyRelative("list");
		}

		// Source / Target

		if( serializedObject.isEditingMultipleObjects == false )
		{
			ColorSourceBase colorSource = (serializedObject.targetObject as ColorSourceBase);
			Color resultColor = colorSource.ResultColor;
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.PropertyField(targetProperty_);
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.ColorField(colorSource.ResultColor, GUILayout.Width(50));
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndHorizontal();

			//EditorGUILayout.LabelField("TargetHSVA", string.Format("H:{0:F3}, S:{1:F3}, V:{2:F3}, A:{3:F3}", colorSource.H, colorSource.S, colorSource.V, colorSource.A));
		}
		else
		{
			EditorGUILayout.PropertyField(targetProperty_);
		}

		float defaultLabelWidth = EditorGUIUtility.labelWidth;

		// Static Edit
		staticEditFoldOut_ = EditorGUILayout.Foldout(staticEditFoldOut_, "Static Edit");
		EditorGUI.indentLevel++;
		if( staticEditFoldOut_ )
		{
			// <image src="D:\UNITY\imagecomments\QS_20200516-025434.png"/>

			EditorGUIUtility.labelWidth = 30;
			float typeWidth = defaultLabelWidth - EditorGUIUtility.labelWidth + 28;
			float valueWidth = 160;
			DrawEditProperty(editTypeHProperty_, editValueHProperty_, "H", typeWidth, valueWidth);
			DrawEditProperty(editTypeSProperty_, editValueSProperty_, "S", typeWidth, valueWidth);
			DrawEditProperty(editTypeVProperty_, editValueVProperty_, "V", typeWidth, valueWidth);
			DrawEditProperty(editTypeAProperty_, editValueAProperty_, "A", typeWidth, valueWidth);
			EditorGUIUtility.labelWidth = defaultLabelWidth;

			// 設定された色を再現する数値を探索する機能
			if( serializedObject.isEditingMultipleObjects == false )
			{
				ColorSourceBase colorSource = (serializedObject.targetObject as ColorSourceBase);
				Color editedColor = colorSource.StaticEditedColor;
				Color targetEditColor = EditorGUILayout.ColorField("EditedColor", editedColor);
				if( targetEditColor != editedColor )
				{
					float targetH, targetS, targetV, targetA;
					ColorPropertyUtil.ToHSVA(targetEditColor, out targetH, out targetS, out targetV, out targetA);
					float sourceH, sourceS, sourceV, sourceA;
					colorSource.GetSourceHSVA(out sourceH, out sourceS, out sourceV, out sourceA);
					float editH, editS, editV, editA;
					if( ColorPropertyUtil.CalcTargetEditValue(sourceH, targetH,
						(ColorPropertyEditType)editTypeHProperty_.enumValueIndex, out editH) )
					{
						editValueHProperty_.floatValue = editH;
					}
					if( ColorPropertyUtil.CalcTargetEditValue(sourceS, targetS,
						(ColorPropertyEditType)editTypeSProperty_.enumValueIndex, out editS) )
					{
						editValueSProperty_.floatValue = editS;
					}
					if( ColorPropertyUtil.CalcTargetEditValue(sourceV, targetV,
						(ColorPropertyEditType)editTypeVProperty_.enumValueIndex, out editV) )
					{
						editValueVProperty_.floatValue = editV;
					}
					if( ColorPropertyUtil.CalcTargetEditValue(sourceA, targetA,
						(ColorPropertyEditType)editTypeAProperty_.enumValueIndex, out editA) )
					{
						editValueAProperty_.floatValue = editA;
					}
				}
			}

		}
		EditorGUI.indentLevel--;

		// Interactive Edit
		interactiveEditFoldOut_ = EditorGUILayout.Foldout(interactiveEditFoldOut_, "Interactive Edit");
		EditorGUI.indentLevel++;
		if( interactiveEditFoldOut_ )
		{
			// State Dict
			float buttonWidth = 50;
			float labelWidth = defaultLabelWidth - 20;
			float fieldWidth = EditorGUIUtility.currentViewWidth - defaultLabelWidth - buttonWidth - 8;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("StateGroup", "State");
			if( GUILayout.Button("+", GUILayout.Width(buttonWidth)) )
			{
				stateDictProperty_.InsertArrayElementAtIndex(stateDictProperty_.arraySize);
			}
			EditorGUILayout.EndHorizontal();
			int removeIndex = -1;
			for( int i = 0; i < stateDictProperty_.arraySize; ++i )
			{
				EditorGUILayout.BeginHorizontal();
				SerializedProperty stateProp = stateDictProperty_.GetArrayElementAtIndex(i);
				SerializedProperty keyProp = stateProp.FindPropertyRelative("Key");
				SerializedProperty valueProp = stateProp.FindPropertyRelative("Value");
				keyProp.stringValue = EditorGUILayout.TextField(GUIContent.none, keyProp.stringValue, GUILayout.Width(labelWidth));

				if( ColorManager.ContainsStateGroup(keyProp.stringValue) )
				{
					ColorGameSyncByState stateGroup = ColorManager.GetStateGroup(keyProp.stringValue);

					if( stateGroup.IsGlobal )
					{
						EditorGUI.BeginDisabledGroup(true);
						int index = Mathf.Max(0, stateGroup.States.IndexOf(stateGroup.GlobalValue));
						EditorGUILayout.LabelField(stateGroup.States[index], GUILayout.Width(fieldWidth));
						EditorGUI.EndDisabledGroup();
					}
					else
					{
						int index = Mathf.Max(0, stateGroup.States.IndexOf(valueProp.stringValue));
						index = EditorGUILayout.Popup(index, stateGroup.States.ToArray(), GUILayout.Width(fieldWidth));
						valueProp.stringValue = stateGroup.States[index];
					}
				}
				else
				{
					EditorGUILayout.LabelField("---", GUILayout.Width(fieldWidth));
				}
				if( GUILayout.Button("-", GUILayout.Width(buttonWidth)) )
				{
					removeIndex = i;
				}
				EditorGUILayout.EndHorizontal();
			}
			if( removeIndex >= 0 )
			{
				stateDictProperty_.DeleteArrayElementAtIndex(removeIndex);
			}

			// Parameter Dict
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("ParameterName", "Parameter");
			if( GUILayout.Button("+", GUILayout.Width(buttonWidth)) )
			{
				parameterDictProperty_.InsertArrayElementAtIndex(parameterDictProperty_.arraySize);
			}
			EditorGUILayout.EndHorizontal();
			removeIndex = -1;
			for( int i = 0; i < parameterDictProperty_.arraySize; ++i )
			{
				EditorGUILayout.BeginHorizontal();
				SerializedProperty parameterProp = parameterDictProperty_.GetArrayElementAtIndex(i);
				SerializedProperty keyProp = parameterProp.FindPropertyRelative("Key");
				SerializedProperty valueProp = parameterProp.FindPropertyRelative("Value");
				keyProp.stringValue = EditorGUILayout.TextField(GUIContent.none, keyProp.stringValue, GUILayout.Width(labelWidth));

				if( ColorManager.ContainsParameter(keyProp.stringValue) )
				{
					ColorGameSyncByParameter parameter = ColorManager.GetParameter(keyProp.stringValue);
					if( parameter.IsGlobal )
					{
						EditorGUI.BeginDisabledGroup(true);
						EditorGUILayout.Slider(parameter.GlobalValue, parameter.MinParam, parameter.MaxParam, GUILayout.Width(fieldWidth));
						EditorGUI.EndDisabledGroup();
					}
					else
					{
						valueProp.floatValue = EditorGUILayout.Slider(valueProp.floatValue, parameter.MinParam, parameter.MaxParam, GUILayout.Width(fieldWidth));
					}
				}
				else
				{
					EditorGUILayout.LabelField("---", GUILayout.Width(fieldWidth));
				}
				if( GUILayout.Button("-", GUILayout.Width(buttonWidth)) )
				{
					removeIndex = i;
				}
				EditorGUILayout.EndHorizontal();
			}
			if( removeIndex >= 0 )
			{
				parameterDictProperty_.DeleteArrayElementAtIndex(removeIndex);
			}
		}
		EditorGUI.indentLevel--;


		EditorGUI.BeginDisabledGroup(true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("beReferencedColors_"), includeChildren: true);
		EditorGUI.EndDisabledGroup();
	}

	void DrawEditProperty(SerializedProperty typeProperty, SerializedProperty valueProperty, string name, float typeWidth, float valueWidth)
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(typeProperty, new GUIContent(name), GUILayout.Width(typeWidth));
		if( typeProperty.enumValueIndex == (int)ColorPropertyEditType.None )
		{
			EditorGUI.BeginDisabledGroup(true);
			valueProperty.floatValue = 0;
			EditorGUILayout.PropertyField(valueProperty, GUIContent.none, GUILayout.MinWidth(valueWidth));
			EditorGUI.EndDisabledGroup();
		}
		else
		{
			EditorGUILayout.PropertyField(valueProperty, GUIContent.none, GUILayout.MinWidth(valueWidth));
		}
		EditorGUILayout.EndHorizontal();
	}
	
	protected void ReferenceChanged(Object newObject, Object oldObject)
	{
		if( serializedObject.isEditingMultipleObjects )
		{
			foreach( Object targetObject in serializedObject.targetObjects )
			{
				if( oldObject != null )
				{
					(oldObject as ColorSourceBase).NotBeReferencedBy(targetObject as ColorSourceBase);
				}
				if( newObject != null )
				{
					(newObject as ColorSourceBase).BeReferencedBy(targetObject as ColorSourceBase);
				}
			}
		}
		else
		{
			if( oldObject != null )
			{
				(oldObject as ColorSourceBase).NotBeReferencedBy(serializedObject.targetObject as ColorSourceBase);
			}
			if( newObject != null )
			{
				(newObject as ColorSourceBase).BeReferencedBy(serializedObject.targetObject as ColorSourceBase);
			}
		}
	}
}