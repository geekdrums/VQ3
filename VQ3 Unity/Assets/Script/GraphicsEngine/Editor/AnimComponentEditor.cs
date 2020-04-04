using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(AnimComponent))]
[CanEditMultipleObjects]
public class AnimComponentEditor : Editor
{
	SerializedProperty animListProperty_;
	SerializedProperty childListProperty_;

	SerializedProperty playOnStartProperty_;
	SerializedProperty resetOnEndProperty_;
	SerializedProperty delayProperty_;
	SerializedProperty delayTimeUnitProperty_;

	System.Action rightClickAction_ = null;
	AnimInfo copiedAnim_ = null;

	enum FromOrTo
	{
		To,
		From,
	}

	enum TimeRangeDragState
	{
		None,
		MinDragging,
		MaxDragging,
	}
	TimeRangeDragState dragState_;
	

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		if( animListProperty_ == null )
		{
			animListProperty_ = serializedObject.FindProperty("AnimInfoList");
			playOnStartProperty_ = serializedObject.FindProperty("PlayOnStart");
			resetOnEndProperty_ = serializedObject.FindProperty("ResetOnEnd");
			delayProperty_ = serializedObject.FindProperty("Delay");
			delayTimeUnitProperty_ = serializedObject.FindProperty("DelayTimeUnit");
			childListProperty_ = serializedObject.FindProperty("ChildAnimList");
		}
		EditorGUILayout.PropertyField(playOnStartProperty_);
		EditorGUILayout.PropertyField(resetOnEndProperty_);
		EditorGUILayout.PropertyField(delayProperty_);
		EditorGUILayout.PropertyField(delayTimeUnitProperty_);

		/*
		// AnimParamTypeが追加されたときに、ズレてしまうEnumの値を修正する
		if( GUILayout.Button("Remap") )
		{
			RemapAnimParam();
		}
		*/

		if( serializedObject.isEditingMultipleObjects )
		{
			EditorGUILayout.LabelField("Cannot draw anim info list while multi editing", EditorStyles.boldLabel);
			serializedObject.ApplyModifiedProperties();
			return;
		}

		EditorGUILayout.Space();
		{
			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel, GUILayout.MinWidth(100));
				EditorGUILayout.LabelField("Param", GUILayout.Width(120));
				EditorGUILayout.LabelField("Interp", GUILayout.Width(100));
			}
			GUILayout.EndHorizontal();

			if( rightClickAction_ != null )
			{
				rightClickAction_();
				rightClickAction_ = null;
			}
			bool isRightClick = rightClickAction_ == null && Event.current.type == EventType.MouseDown && Event.current.button == 1;
			
			float timeRange = (serializedObject.targetObject as AnimComponent).TotalTimeSec;
			if( dragState_ != TimeRangeDragState.None && Event.current.type == EventType.MouseUp )
			{
				dragState_ = TimeRangeDragState.None;
			}

			for( int i = 0; i < animListProperty_.arraySize; ++i )
			{
				DrawAnimInfo(i, timeRange, isRightClick);
			}

			GUILayout.BeginHorizontal();
			{
				if( GUILayout.Button("Add") )
				{
					animListProperty_.InsertArrayElementAtIndex(animListProperty_.arraySize);
					InitializeProperties(animListProperty_.GetArrayElementAtIndex(animListProperty_.arraySize - 1));
				}
				if( GUILayout.Button("Remove") && animListProperty_.arraySize > 0 )
				{
					animListProperty_.DeleteArrayElementAtIndex(animListProperty_.arraySize - 1);
				}
				if( GUILayout.Button("Clear") && animListProperty_.arraySize > 0 )
				{
					animListProperty_.ClearArray();
				}
			}
			GUILayout.EndHorizontal();

			if( UnityEditor.EditorApplication.isPlaying )
			{
				GUILayout.BeginHorizontal();
				{
					AnimComponent animation = target as AnimComponent;
					if( GUILayout.Button("Play") )
					{
						animation.Create();
						animation.Play();
					}
					if( GUILayout.Button("Reset") )
					{
						animation.Create();
						animation.ResetAnim();
					}
					if( GUILayout.Button("End") )
					{
						animation.Create();
						animation.EndAnim();
					}
				}
				GUILayout.EndHorizontal();
			}
		}
		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(childListProperty_, includeChildren: true);

		serializedObject.ApplyModifiedProperties();
	}

	void RemapAnimParam()
	{
		List<AnimParamType> addedParams = new List<AnimParamType>();
		addedParams.Add(AnimParamType.PrimitiveStartArc);

		foreach( Object obj in targets )
		{
			foreach( AnimInfo ai in (obj as AnimComponent).AnimInfoList )
			{
				int offset = 0;
				foreach( AnimParamType addedParam in addedParams )
				{
					if( addedParam < ai.AnimParam )
					{
						++offset;
					}
					else
					{
						break;
					}
				}
				ai.AnimParam += offset;
			}
		}
	}

	void DrawAnimInfo(int index, float timeRange, bool isRightClick = false)
	{
		SerializedProperty animStruct = animListProperty_.GetArrayElementAtIndex(index);
		SerializedProperty objectProp = animStruct.FindPropertyRelative("Object");
		SerializedProperty paramProp = animStruct.FindPropertyRelative("AnimParam");

		AnimParamType paramType = ((AnimParamType)(paramProp.enumValueIndex));
		UnityEngine.Object refObj = objectProp.objectReferenceValue;
		
		// object & property & interp params
		EditorGUILayout.BeginHorizontal();
		{
			// object
			EditorGUILayout.PropertyField(objectProp, GUIContent.none, GUILayout.MinWidth(100));
			if( isRightClick && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) )
			{
				ListItemRightClickMenu(index);
			}

			// param
			EditorGUILayout.PropertyField(paramProp, GUIContent.none, GUILayout.Width(120));

			// prop name
			if( AnimParamType.ShaderFloat <= paramType && paramType <= AnimParamType.ShaderColor )
			{
				SerializedProperty propNameProperty = animStruct.FindPropertyRelative("PropertyName");
				EditorGUILayout.PropertyField(propNameProperty, GUIContent.none, GUILayout.Width(120));
			}

			// interp type
			if( paramType < AnimParamType.TurnOn && paramType != AnimParamType.Text )
			{
				SerializedProperty interpTypeProp = animStruct.FindPropertyRelative("Interp");
				EditorGUILayout.PropertyField(interpTypeProp, GUIContent.none, GUILayout.Width(100));
			}
			else
			{
				EditorGUILayout.LabelField("", GUILayout.Width(100));
			}
		}
		EditorGUILayout.EndHorizontal();

		// init & target params
		SerializedProperty targetProp = null;
		SerializedProperty initialProp = null;
		switch( paramType )
		{
			case AnimParamType.Scale:
			case AnimParamType.ScaleX:
			case AnimParamType.ScaleY:
			case AnimParamType.ScaleZ:
			case AnimParamType.RotationX:
			case AnimParamType.RotationY:
			case AnimParamType.RotationZ:
			case AnimParamType.PositionX:
			case AnimParamType.PositionY:
			case AnimParamType.PositionZ:
			case AnimParamType.SizeDeltaX:
			case AnimParamType.SizeDeltaY:
			case AnimParamType.AnchoredPositionX:
			case AnimParamType.AnchoredPositionY:
			case AnimParamType.PrimitiveRadius:
			case AnimParamType.PrimitiveWidth:
			case AnimParamType.PrimitiveArc:
			case AnimParamType.PrimitiveStartArc:
			case AnimParamType.GaugeLength:
			case AnimParamType.GaugeRate:
			case AnimParamType.GaugeStartRate:
			case AnimParamType.GaugeWidth:
			case AnimParamType.ShaderFloat:
			case AnimParamType.AlphaColor:
			case AnimParamType.TextAlphaColor:
			{
				targetProp = animStruct.FindPropertyRelative("TargetFloat");
				initialProp = animStruct.FindPropertyRelative("InitialFloat");
			}
			break;
			case AnimParamType.Blink:
			{
				targetProp = animStruct.FindPropertyRelative("TargetInt");
			}
			break;
			case AnimParamType.Position:
			case AnimParamType.AnchoredPosition:
			case AnimParamType.SizeDelta:
			case AnimParamType.ShaderVector3:
			case AnimParamType.ShaderVector2:
			{
				targetProp = animStruct.FindPropertyRelative("TargetVector");
				initialProp = animStruct.FindPropertyRelative("InitialVector");
			}
			break;
			case AnimParamType.Color:
			case AnimParamType.TextColor:
			case AnimParamType.ShaderColor:
			{
				targetProp = animStruct.FindPropertyRelative("TargetColor");
				initialProp = animStruct.FindPropertyRelative("InitialColor");
			}
			break;
			case AnimParamType.Text:
			{
				targetProp = animStruct.FindPropertyRelative("TargetString");
				if( targetProp.stringValue == "" && refObj != null && refObj is GameObject )
				{
					Text uitext = (refObj as GameObject).GetComponent<Text>();
					if( uitext != null )
					{
						targetProp.stringValue = uitext.text;
					}
					else
					{
						TextMesh text = (refObj as GameObject).GetComponent<TextMesh>();
						if( text != null )
						{
							targetProp.stringValue = text.text;
						}
					}
				}
			}
			break;
			case AnimParamType.TurnOn:
			case AnimParamType.TurnOff:
			case AnimParamType.Flash:
			default:
			break;
		}
		if( targetProp != null )
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("", GUILayout.Width(10));

				if( initialProp != null )
				{
					SerializedProperty hasInitialValueProp = animStruct.FindPropertyRelative("HasInitialValue");
					hasInitialValueProp.boolValue = FromOrTo.From == (FromOrTo)EditorGUILayout.EnumPopup(hasInitialValueProp.boolValue ? FromOrTo.From : FromOrTo.To, GUILayout.Width(60));

					if( hasInitialValueProp.boolValue )
					{
						EditorGUILayout.PropertyField(initialProp, GUIContent.none, GUILayout.Width(140));
						EditorGUILayout.LabelField("To", GUILayout.Width(30));
					}
				}
				else
				{
					if( paramType == AnimParamType.Blink )
					{
						EditorGUILayout.LabelField("Interval", GUILayout.Width(60));
					}
					else
					{
						EditorGUILayout.LabelField("To", GUILayout.Width(60));
					}
				}
				
				EditorGUILayout.PropertyField(targetProp, GUIContent.none);
			}
			EditorGUILayout.EndHorizontal();
		}

		// time params
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.LabelField("", GUILayout.Width(10));

			SerializedProperty delayProp = animStruct.FindPropertyRelative("Delay");
			SerializedProperty timeProp = animStruct.FindPropertyRelative("Time");
			SerializedProperty timeUnitProp = animStruct.FindPropertyRelative("TimeUnit");
			SerializedProperty endOptionProp = animStruct.FindPropertyRelative("EndOption");

			EditorGUILayout.PropertyField(timeUnitProp, GUIContent.none, GUILayout.Width(60));

			EditorGUILayout.PropertyField(delayProp, GUIContent.none, GUILayout.Width(40));

			TimeUnitType timUnit = (TimeUnitType)timeUnitProp.enumValueIndex;
			float minValue = TimeUtility.ConvertTime(delayProp.floatValue, timUnit);
			float lastMinValue = minValue;
			float maxValue = TimeUtility.ConvertTime(delayProp.floatValue + timeProp.floatValue, timUnit);
			float lastMaxValue = maxValue;
			EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, 0, timeRange);
			if( dragState_ == TimeRangeDragState.None )
			{
				// Delay移動した時はTimeを縮めたくないので、移動はどちらか片方のみにする
				if( lastMinValue != minValue )
				{
					dragState_ = TimeRangeDragState.MinDragging;
				}
				else if( lastMaxValue != maxValue )
				{
					dragState_ = TimeRangeDragState.MaxDragging;
				}
			}
			else if( dragState_ == TimeRangeDragState.MinDragging && lastMinValue != minValue )
			{
				delayProp.floatValue = TimeUtility.ConvertTime(minValue, TimeUnitType.Sec, timUnit);
			}
			else if( dragState_ == TimeRangeDragState.MaxDragging && lastMaxValue != maxValue )
			{
				timeProp.floatValue = TimeUtility.ConvertTime(maxValue - minValue, TimeUnitType.Sec, timUnit);
			}

			EditorGUILayout.PropertyField(timeProp, GUIContent.none, GUILayout.Width(40));

			EditorGUILayout.PropertyField(endOptionProp, GUIContent.none, GUILayout.Width(60));
		}
		EditorGUILayout.EndHorizontal();
	}

	void ListItemRightClickMenu(int index)
	{
		GenericMenu menu = new GenericMenu();
		menu.AddItem(new GUIContent("Delete"), false, () => {
			rightClickAction_ = () =>
			{
				animListProperty_.DeleteArrayElementAtIndex(index);
			};
		});

		menu.AddItem(new GUIContent("Copy"), false, () => {
			rightClickAction_ = () =>
			{
				copiedAnim_ = (serializedObject.targetObject as AnimComponent).AnimInfoList[index];
			};
		});

		menu.AddItem(new GUIContent("Cut"), false, () => {
			rightClickAction_ = () =>
			{
				copiedAnim_ = (serializedObject.targetObject as AnimComponent).AnimInfoList[index];
				animListProperty_.DeleteArrayElementAtIndex(index);
			};
		});

		if( copiedAnim_ != null )
		{
			menu.AddItem(new GUIContent("Paste"), false, () => {
				rightClickAction_ = () =>
				{
					animListProperty_.InsertArrayElementAtIndex(index + 1);
					SerializedProperty newAnim = animListProperty_.GetArrayElementAtIndex(index + 1);
					CopyProperties(newAnim, copiedAnim_);
				};
			});
		}

		menu.AddItem(new GUIContent("Duplicate"), false, () => {
			rightClickAction_ = () =>
			{
				animListProperty_.InsertArrayElementAtIndex(index + 1);
				SerializedProperty srcAnim = animListProperty_.GetArrayElementAtIndex(index);
				SerializedProperty newAnim = animListProperty_.GetArrayElementAtIndex(index + 1);
				CopyProperties(newAnim, srcAnim);
			};
		});

		menu.ShowAsContext();
	}

	void ObjectRightClickMenu(SerializedProperty objProp)
	{
		GameObject gameObj = serializedObject.targetObject as GameObject;
		if( gameObj == null )
		{
			MonoBehaviour behaviour = serializedObject.targetObject as MonoBehaviour;
			if( behaviour != null )
			{
				gameObj = behaviour.gameObject;
			}
		}

		if( gameObj != null )
		{
			GenericMenu menu = new GenericMenu();

			menu.AddItem(new GUIContent(gameObj.name + " (this)"), gameObj == objProp.objectReferenceValue, () => {
				rightClickAction_ = () => objProp.objectReferenceValue = gameObj;
			});

			for( int i = 0; i < gameObj.transform.childCount; ++i )
			{
				GameObject child = gameObj.transform.GetChild(i).gameObject;
				menu.AddItem(new GUIContent(child.name), child == objProp.objectReferenceValue, () => {
					rightClickAction_ = () => objProp.objectReferenceValue = child;
				});
			}

			menu.ShowAsContext();
		}
	}

	void InitializeProperties(SerializedProperty anim)
	{
		anim.FindPropertyRelative("Object").objectReferenceValue = (serializedObject.targetObject as AnimComponent).gameObject;
		anim.FindPropertyRelative("AnimParam").enumValueIndex = (int)AnimParamType.Position;
		anim.FindPropertyRelative("Interp").enumValueIndex = (int)InterpType.Linear;
		anim.FindPropertyRelative("Time").floatValue = 1.0f;
		anim.FindPropertyRelative("Delay").floatValue = 0.0f;
		anim.FindPropertyRelative("EndOption").enumValueIndex = (int)AnimEndOption.None;

		anim.FindPropertyRelative("TargetFloat").floatValue = 0.0f;
		anim.FindPropertyRelative("TargetInt").intValue = 1;
		anim.FindPropertyRelative("TargetVector").vector3Value = new Vector3();
		anim.FindPropertyRelative("TargetColor").colorValue = Color.white;
		anim.FindPropertyRelative("TargetString").stringValue = "";

		anim.FindPropertyRelative("HasInitialValue").boolValue = false;
		anim.FindPropertyRelative("InitialFloat").floatValue = 0.0f;
		anim.FindPropertyRelative("InitialVector").vector3Value = new Vector3();
		anim.FindPropertyRelative("InitialColor").colorValue = Color.white;

		anim.FindPropertyRelative("PropertyName").stringValue = "";
	}

	void CopyProperties(SerializedProperty dst, SerializedProperty src)
	{
		dst.FindPropertyRelative("Object").objectReferenceValue	= src.FindPropertyRelative("Object").objectReferenceValue;
		dst.FindPropertyRelative("AnimParam").enumValueIndex	= src.FindPropertyRelative("AnimParam").enumValueIndex;
		dst.FindPropertyRelative("Interp").enumValueIndex		= src.FindPropertyRelative("Interp").enumValueIndex;
		dst.FindPropertyRelative("TimeUnit").enumValueIndex		= src.FindPropertyRelative("TimeUnit").enumValueIndex;
		dst.FindPropertyRelative("Time").floatValue				= src.FindPropertyRelative("Time").floatValue;
		dst.FindPropertyRelative("Delay").floatValue			= src.FindPropertyRelative("Delay").floatValue;
		dst.FindPropertyRelative("EndOption").enumValueIndex	= src.FindPropertyRelative("EndOption").enumValueIndex;

		dst.FindPropertyRelative("TargetFloat").floatValue		= src.FindPropertyRelative("TargetFloat").floatValue;
		dst.FindPropertyRelative("TargetInt").intValue			= src.FindPropertyRelative("TargetInt").intValue;
		dst.FindPropertyRelative("TargetVector").vector3Value	= src.FindPropertyRelative("TargetVector").vector3Value;
		dst.FindPropertyRelative("TargetColor").colorValue		= src.FindPropertyRelative("TargetColor").colorValue;
		dst.FindPropertyRelative("TargetString").stringValue	= src.FindPropertyRelative("TargetString").stringValue;

		dst.FindPropertyRelative("HasInitialValue").boolValue	= src.FindPropertyRelative("HasInitialValue").boolValue;
		dst.FindPropertyRelative("InitialFloat").floatValue		= src.FindPropertyRelative("InitialFloat").floatValue;
		dst.FindPropertyRelative("InitialVector").vector3Value	= src.FindPropertyRelative("InitialVector").vector3Value;
		dst.FindPropertyRelative("InitialColor").colorValue		= src.FindPropertyRelative("InitialColor").colorValue;

		dst.FindPropertyRelative("PropertyName").stringValue	= src.FindPropertyRelative("PropertyName").stringValue;
	}

	void CopyProperties(SerializedProperty dst, AnimInfo src)
	{
		dst.FindPropertyRelative("Object").objectReferenceValue	= src.Object;
		dst.FindPropertyRelative("AnimParam").enumValueIndex	= (int)src.AnimParam;
		dst.FindPropertyRelative("Interp").enumValueIndex		= (int)src.Interp;
		dst.FindPropertyRelative("TimeUnit").enumValueIndex		= (int)src.TimeUnit;
		dst.FindPropertyRelative("Time").floatValue				= src.Time;
		dst.FindPropertyRelative("Delay").floatValue			= src.Delay;
		dst.FindPropertyRelative("EndOption").enumValueIndex	= (int)src.EndOption;

		dst.FindPropertyRelative("TargetFloat").floatValue		= src.TargetFloat;
		dst.FindPropertyRelative("TargetInt").intValue			= src.TargetInt;
		dst.FindPropertyRelative("TargetVector").vector3Value	= src.TargetVector;
		dst.FindPropertyRelative("TargetColor").colorValue		= src.TargetColor;
		dst.FindPropertyRelative("TargetString").stringValue	= src.TargetString;

		dst.FindPropertyRelative("HasInitialValue").boolValue	= src.HasInitialValue;
		dst.FindPropertyRelative("InitialFloat").floatValue		= src.InitialFloat;
		dst.FindPropertyRelative("InitialVector").vector3Value	= src.InitialVector;
		dst.FindPropertyRelative("InitialColor").colorValue		= src.InitialColor;

		dst.FindPropertyRelative("PropertyName").stringValue	= src.PropertyName;
	}
}