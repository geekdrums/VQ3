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
	SerializedProperty speedProperty_;

	System.Action rightClickAction_ = null;
	AnimInfo copiedAnim_ = null;
	int selectedIndex_ = -1;
	bool isMultiSelected_ = false;

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

	static bool childListFoldOut_ = true;
	

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		if( animListProperty_ == null )
		{
			animListProperty_ = serializedObject.FindProperty("AnimInfoList");
			playOnStartProperty_ = serializedObject.FindProperty("PlayOnStart");
			resetOnEndProperty_ = serializedObject.FindProperty("ResetOnEnd");
			delayProperty_ = serializedObject.FindProperty("Delay");
			speedProperty_ = serializedObject.FindProperty("Speed");
			delayTimeUnitProperty_ = serializedObject.FindProperty("DelayTimeUnit");
			childListProperty_ = serializedObject.FindProperty("ChildAnimList");
		}

		/// <img src="D:\UNITY\imagecomments\QS_20200513-035459.png"/>
		EditorGUILayout.PropertyField(playOnStartProperty_);
		EditorGUILayout.PropertyField(resetOnEndProperty_);
		EditorGUILayout.PropertyField(delayProperty_);
		EditorGUILayout.PropertyField(delayTimeUnitProperty_);
		EditorGUILayout.PropertyField(speedProperty_);

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
			/// <img src="D:\UNITY\imagecomments\QS_20200513-035744.png"/>
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
			bool isLeftClick = Event.current.type == EventType.MouseDown && Event.current.button == 0;

			float timeRange = (serializedObject.targetObject as AnimComponent).TotalTimeSec;
			if( dragState_ != TimeRangeDragState.None && Event.current.type == EventType.MouseUp )
			{
				dragState_ = TimeRangeDragState.None;
			}

			int oldSelectedIndex = selectedIndex_;
			bool isNowSelected = false;
			for( int i = 0; i < animListProperty_.arraySize; ++i )
			{
				if( DrawAnimInfo(i, timeRange, isRightClick, isLeftClick) )
				{
					selectedIndex_ = i;
					isNowSelected = true;
				}
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
			}
			GUI.color = Color.white;

			// 選択が変化したら
			if( isNowSelected )
			{
				if( Event.current.modifiers == EventModifiers.None )
				{
					// ほかを選択解除
					for( int i = 0; i < animListProperty_.arraySize; ++i )
					{
						if( i == selectedIndex_ ) continue;
						var selectedProp = animListProperty_.GetArrayElementAtIndex(i).FindPropertyRelative("IsSelected");
						selectedProp.boolValue = false;
					}
					isMultiSelected_ = false;
				}
				else if( Event.current.modifiers == EventModifiers.Shift && oldSelectedIndex >= 0 )
				{
					// oldからnewまでを選択
					for( int i = Mathf.Min(oldSelectedIndex, selectedIndex_); i < Mathf.Max(oldSelectedIndex, selectedIndex_); ++i )
					{
						if( i == selectedIndex_ ) continue;
						var selectedProp = animListProperty_.GetArrayElementAtIndex(i).FindPropertyRelative("IsSelected");
						selectedProp.boolValue = true;
					}
					isMultiSelected_ = true;
				}
				else if( Event.current.modifiers == EventModifiers.Control )
				{
					isMultiSelected_ = true;
				}
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
				if( GUILayout.Button("Save") )
				{
					SaveAnimComponentData.DeserializeComponent(target as AnimComponent);
				}
			}
		}
		EditorGUILayout.Space();

		childListFoldOut_ = EditorGUILayout.Foldout(childListFoldOut_, "ChildAnimList");
		if( childListFoldOut_ )
		{
			EditorGUI.indentLevel++;
			childListProperty_.arraySize = EditorGUILayout.IntField("Size", childListProperty_.arraySize);
			for( int i = 0; i < childListProperty_.arraySize; ++i )
			{
				EditorGUILayout.BeginHorizontal();
				var childAnimProp = childListProperty_.GetArrayElementAtIndex(i);
				EditorGUILayout.PropertyField(childAnimProp, new GUIContent(""), GUILayout.Width(EditorGUIUtility.labelWidth));
				var childAnim = childAnimProp.objectReferenceValue as AnimComponent;
				if(childAnim != null )
				{
					childAnim.Delay = EditorGUILayout.FloatField("", childAnim.Delay);
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUI.indentLevel--;
		}
		
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

	bool DrawAnimInfo(int index, float timeRange, bool isRightClick = false, bool isLeftClick = false)
	{
		bool wasLeftClicked = false;
		SerializedProperty animStruct = animListProperty_.GetArrayElementAtIndex(index);
		SerializedProperty objectProp = animStruct.FindPropertyRelative("Object");
		SerializedProperty paramProp = animStruct.FindPropertyRelative("AnimParam");
		SerializedProperty selectedProp = animStruct.FindPropertyRelative("IsSelected");

		GUI.color = selectedProp.boolValue ? Color.white : Color.gray;

		AnimParamType paramType = ((AnimParamType)(paramProp.enumValueIndex));
		UnityEngine.Object refObj = objectProp.objectReferenceValue;

		// object & property & interp params
		/// <img src="D:\UNITY\imagecomments\QS_20200513-035744.png"/>
		EditorGUILayout.BeginHorizontal();
		{
			// object
			EditorGUILayout.PropertyField(objectProp, GUIContent.none, GUILayout.MinWidth(100));
			Rect rect = GUILayoutUtility.GetLastRect();
			if( isRightClick && rect.Contains(Event.current.mousePosition) )
			{
				ListItemRightClickMenu(index);
			}
			rect.width = EditorGUIUtility.currentViewWidth;
			rect.height = EditorGUIUtility.singleLineHeight * 3;
			if( isLeftClick && rect.Contains(Event.current.mousePosition) )
			{
				wasLeftClicked = true;
				selectedProp.boolValue = true;
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
				int oldValue = interpTypeProp.enumValueIndex;
				interpTypeProp.enumValueIndex = (int)(InterpType)EditorGUILayout.EnumPopup((InterpType)interpTypeProp.enumValueIndex, GUILayout.Width(100));
				if( interpTypeProp.enumValueIndex != oldValue && isMultiSelected_ )
				{
					OnMultiSelectEdit(index, "Interp", (InterpType)interpTypeProp.enumValueIndex, true);
				}
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
			// from / to
			/// <img src="D:\UNITY\imagecomments\QS_20200513-040033.png"/>
			EditorGUILayout.BeginHorizontal();
			{
				// indent
				EditorGUILayout.LabelField("", GUILayout.Width(20));

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
		/// <img src="D:\UNITY\imagecomments\QS_20200513-040129.png"/>
		EditorGUILayout.BeginHorizontal();
		{
			// indent
			EditorGUILayout.LabelField("", GUILayout.Width(20));

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
				if( Event.current.modifiers != EventModifiers.Alt )
				{
					minValue = ((int)(minValue * 10)) / 10.0f;
				}
				float oldValue = delayProp.floatValue;
				delayProp.floatValue = TimeUtility.ConvertTime(minValue, TimeUnitType.Sec, timUnit);
				if( isMultiSelected_ )
				{
					OnMultiSelectEdit(index, "Delay", delayProp.floatValue - oldValue, true);
				}
			}
			else if( dragState_ == TimeRangeDragState.MaxDragging && lastMaxValue != maxValue )
			{
				if( Event.current.modifiers != EventModifiers.Alt )
				{
					maxValue = ((int)(maxValue * 10)) / 10.0f;
				}
				float oldValue = timeProp.floatValue;
				timeProp.floatValue = TimeUtility.ConvertTime(maxValue - minValue, TimeUnitType.Sec, timUnit);
				if( isMultiSelected_ )
				{
					OnMultiSelectEdit(index, "Time", timeProp.floatValue - oldValue, true);
				}
			}

			EditorGUILayout.PropertyField(timeProp, GUIContent.none, GUILayout.Width(40));

			EditorGUILayout.PropertyField(endOptionProp, GUIContent.none, GUILayout.Width(60));
		}
		EditorGUILayout.EndHorizontal();

		return wasLeftClicked;
	}

	void OnMultiSelectEdit(int editedIndex, string propName, object propValue, bool valueIsDiff = false)
	{
		for( int i = 0; i < animListProperty_.arraySize; ++i )
		{
			if( i == editedIndex ) continue;
			var animProp = animListProperty_.GetArrayElementAtIndex(i);
			var selectedProp = animProp.FindPropertyRelative("IsSelected");
			if( selectedProp.boolValue )
			{
				var targetProp = animProp.FindPropertyRelative(propName);
				if( propValue is float )
				{
					targetProp.floatValue = (valueIsDiff ? targetProp.floatValue : 0) + (float)propValue;
				}
				else if( propValue is int )
				{
					targetProp.intValue = (valueIsDiff ? targetProp.intValue : 0) + (int)propValue;
				}
				else if( propValue is Vector3 )
				{
					targetProp.vector3Value = (valueIsDiff ? targetProp.vector3Value : Vector3.zero) + (Vector3)propValue;
				}
				else if( propValue is bool )
				{
					targetProp.boolValue = (bool)propValue;
				}
				else if( propValue.GetType().IsEnum )
				{
					targetProp.enumValueIndex = (int)propValue;
				}
			}
		}
	}

	void ListItemRightClickMenu(int index)
	{
		/// <img src="D:\UNITY\imagecomments\QS_20200513-040214.png"/>
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
		anim.FindPropertyRelative("AnimParam").enumValueIndex = (int)AnimParamType.PrimitiveRadius;
		anim.FindPropertyRelative("Interp").enumValueIndex = (int)InterpType.ExpoOut;
		anim.FindPropertyRelative("Time").floatValue = 0.5f;
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