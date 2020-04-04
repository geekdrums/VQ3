using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class SaveAnimComponentData
{
	static bool isRecording_ = false;
	static Dictionary<int, Dictionary<string, object>> saveObjectDic_ = new Dictionary<int, Dictionary<string, object>>();

	static SaveAnimComponentData()
	{
		EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
		{
			if( isRecording_ )
			{
				if( EditorApplication.isPlaying )
				{
					foreach( AnimComponent component in GameObject.FindObjectsOfType(typeof(AnimComponent)) )
					{
						SerializeComponent(component);
					}
				}
				else
				{
					foreach( AnimComponent component in Resources.FindObjectsOfTypeAll(typeof(AnimComponent)) )
					{
						DeserializeComponent(component);
					}
				}
			}

			isRecording_ = EditorApplication.isPlaying;
			if( isRecording_ == false && !EditorApplication.isPlaying )
			{
				saveObjectDic_.Clear();
			}
		};
	}

	private static void SerializeComponent(AnimComponent component)
	{
		var dic = new Dictionary<string, object>();
		var type = component.GetType();
		foreach( var field in type.GetFields() )
		{
			dic.Add(field.Name, field.GetValue(component));
		}
		saveObjectDic_.Add(component.GetInstanceID(), dic);
	}

	private static void DeserializeComponent(AnimComponent component)
	{
		if( saveObjectDic_.ContainsKey(component.GetInstanceID()) )
		{
			var type = component.GetType();
			var dict = saveObjectDic_[component.GetInstanceID()];
			foreach( var field in type.GetFields() )
			{
				field.SetValue(component, dict[field.Name]);
			}
		}
	}
}