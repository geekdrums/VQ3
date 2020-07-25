using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(ColorSourceBase))]
public class ColorSourceBaseEditor : Editor
{
	SerializedProperty sourceInstanceProperty_;
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

	static bool staticEditFoldOut_ = false;
	static bool interactiveEditFoldOut_ = false;

	protected bool hasSourceInstance_ = false;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		if( sourceInstanceProperty_ == null )
		{
			sourceInstanceProperty_ = serializedObject.FindProperty("SourceInstance");
		}
		EditorGUILayout.PropertyField(sourceInstanceProperty_);
		hasSourceInstance_ = sourceInstanceProperty_.objectReferenceValue != null;

		ColorSourceBase colorSource = (serializedObject.targetObject as ColorSourceBase);
		if( hasSourceInstance_ )
		{
			// Sourceが指定されたら値をコピーする
			colorSource.ApplySourceInstance();
			EditorGUI.BeginDisabledGroup(true);
		}

		DrawSourceInspector();
		DrawBaseInspector();
		
		if( hasSourceInstance_ )
		{
			EditorGUI.EndDisabledGroup();
		}

		serializedObject.ApplyModifiedProperties();
	}

	public virtual void DrawSourceInspector()
	{

	}

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

		// Target
		ColorSourceBase colorSource = (serializedObject.targetObject as ColorSourceBase);
		Color resultColor = colorSource.ResultColor;
		EditorGUILayout.BeginHorizontal();
		{
			LocalEnableGroup(() =>
			{
				EditorGUILayout.PropertyField(targetProperty_);
			});
			LocalDisableGroup(() =>
			{
				EditorGUILayout.ColorField(colorSource.ResultColor, GUILayout.Width(50));
			});
		}
		EditorGUILayout.EndHorizontal();
		
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
							int index = Mathf.Max(0, stateGroup.States.IndexOf(valueProp.stringValue));
							index = EditorGUILayout.Popup(index, stateGroup.States.ToArray(), GUILayout.Width(fieldWidth));
							valueProp.stringValue = stateGroup.States[index];
						});
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
						LocalDisableGroup(() =>
						{
							EditorGUILayout.Slider(parameter.GlobalValue, parameter.MinParam, parameter.MaxParam, GUILayout.Width(fieldWidth));
						});
					}
					else
					{
						LocalEnableGroup(() =>
						{
							valueProp.floatValue = EditorGUILayout.Slider(valueProp.floatValue, parameter.MinParam, parameter.MaxParam, GUILayout.Width(fieldWidth));
						});
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


		LocalDisableGroup(() =>
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("beReferencedColors_"), includeChildren: true);
		});

		if( hasSourceInstance_ )
		{
			EditorGUI.EndDisabledGroup();
		}
	}

	void DrawEditProperty(SerializedProperty typeProperty, SerializedProperty valueProperty, string name, float typeWidth, float valueWidth)
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(typeProperty, new GUIContent(name), GUILayout.Width(typeWidth));
		if( typeProperty.enumValueIndex == (int)ColorPropertyEditType.None )
		{
			LocalDisableGroup(() =>
			{
				valueProperty.floatValue = 0;
				EditorGUILayout.PropertyField(valueProperty, GUIContent.none, GUILayout.MinWidth(valueWidth));
			});
		}
		else
		{
			EditorGUILayout.PropertyField(valueProperty, GUIContent.none, GUILayout.MinWidth(valueWidth));
		}
		EditorGUILayout.EndHorizontal();
	}
	
	protected void ReferenceChanged(UnityEngine.Object newObject, UnityEngine.Object oldObject)
	{
		foreach( UnityEngine.Object targetObject in serializedObject.targetObjects )
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

	protected void LocalDisableGroup(Action action)
	{
		if( hasSourceInstance_ == false )
		{
			EditorGUI.BeginDisabledGroup(true);
			action();
			EditorGUI.EndDisabledGroup();
		}
		else
		{
			action();
		}
	}

	protected void LocalEnableGroup(Action action)
	{
		if( hasSourceInstance_ == false )
		{
			action();
		}
		else
		{
			EditorGUI.EndDisabledGroup();
			action();
			EditorGUI.BeginDisabledGroup(true);
		}
	}
}