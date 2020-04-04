using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(MidairPrimitive))]
[CanEditMultipleObjects]
public class PrimitiveEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		foreach( Object target in serializedObject.targetObjects )
		{
			MidairPrimitive primitive = target as MidairPrimitive;
			if( primitive != null && primitive.MeshDirty )
			{
				primitive.RecalculatePolygon(forceReflesh: true);
			}
		}
	}
}

[CustomEditor(typeof(GaugeRenderer))]
[CanEditMultipleObjects]
public class GaugeEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		foreach( Object target in serializedObject.targetObjects )
		{
			GaugeRenderer primitive = target as GaugeRenderer;
			if( primitive != null && primitive.MeshDirty )
			{
				primitive.RecalculatePolygon();
			}
		}
	}
}

[CustomEditor(typeof(MidairRect))]
[CanEditMultipleObjects]
public class MidairRectEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		foreach( Object target in serializedObject.targetObjects )
		{
			MidairRect primitive = target as MidairRect;
			if( primitive != null && primitive.MeshDirty )
			{
				primitive.RecalculatePolygon();
			}
		}
	}
}

[CustomEditor(typeof(ShadePrimitive))]
[CanEditMultipleObjects]
public class ShadePrimitiveEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		foreach( Object target in serializedObject.targetObjects )
		{
			ShadePrimitive shade = target as ShadePrimitive;
			if( shade != null && shade.MeshDirty )
			{
				shade.RecalculatePolygon(forceReflesh: true);
			}
		}
	}
}

[CustomEditor(typeof(LightForShadePrimitive))]
[CanEditMultipleObjects]
public class LightForShadePrimitiveEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		foreach( Object target in serializedObject.targetObjects )
		{
			LightForShadePrimitive light = target as LightForShadePrimitive;
			light.CheckUpdate();
		}
	}
}
