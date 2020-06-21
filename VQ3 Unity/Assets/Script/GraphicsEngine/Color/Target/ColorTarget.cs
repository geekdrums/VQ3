using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ColorTarget : MonoBehaviour, IColoredObject
{
	public Color Color = Color.white;
	public List<GameObject> Targets = new List<GameObject>();
	
	List<IColoredObject> colordObjects_ = new List<IColoredObject>();
	List<Component> coloredComponents_ = new List<Component>();

	private void OnValidate()
	{
		colordObjects_.Clear();
		coloredComponents_.Clear();
		if( Targets.Contains(gameObject) == false )
		{
			Targets.Add(gameObject);
		}
		Targets.RemoveAll(obj => obj == null);
		foreach( GameObject gameObj in Targets )
		{
			colordObjects_.AddRange(gameObj.GetComponents<IColoredObject>());
			coloredComponents_.AddRange(gameObj.GetComponents<Text>());
			coloredComponents_.AddRange(gameObj.GetComponents<Image>());
			coloredComponents_.AddRange(gameObj.GetComponents<TextMesh>());
		}
		colordObjects_.Remove(this);
		SetColor(Color);
	}

	public void SetColor(Color color)
	{
		Color = color;
		foreach( IColoredObject coloredObj in colordObjects_ )
		{
			coloredObj.SetColor(color);
		}
		foreach( Component component in coloredComponents_ )
		{
			if( component is Text )
			{
				(component as Text).color = color;
			}
			else if( component is Image )
			{
				(component as Image).color = color;
			}
			else if( component is TextMesh )
			{
				(component as TextMesh).color = color;
			}
		}
	}

	public Color GetColor()
	{
		return Color;
	}
}
