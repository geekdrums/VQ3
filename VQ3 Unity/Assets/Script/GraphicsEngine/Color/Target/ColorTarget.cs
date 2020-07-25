using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ColorTarget : MonoBehaviour, IColoredObject
{
	public Color TargetColor = Color.white;
	public float LinearFactor = 0.0f;

	public List<GameObject> Targets = new List<GameObject>();

	Color currentColor_ = Color.white;
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
			ColorTarget target= gameObj.GetComponent<ColorTarget>();
			IColoredObject coloredObj = gameObj.GetComponent<IColoredObject>();
			if( target != null && target.Equals(this) == false )
			{
				colordObjects_.Add(target);
			}
			else if( coloredObj != null && coloredObj.Equals(this) == false )
			{
				colordObjects_.Add(coloredObj);
			}
			else
			{
				Component coloredComponent = gameObj.GetComponent<Text>();
				if( coloredComponent == null ) coloredComponent = gameObj.GetComponent<Image>();
				if( coloredComponent == null ) coloredComponent = gameObj.GetComponent<TextMesh>();
				if( coloredComponent == null ) coloredComponent = gameObj.GetComponent<LineRenderer>();
				if( coloredComponent == null ) coloredComponent = gameObj.GetComponent<Renderer>();
				if( coloredComponent != null )
				{
					coloredComponents_.Add(coloredComponent);
				}
			}
		}
		colordObjects_.Remove(this);
		SetColor(TargetColor);
	}

	void Awake()
	{
		currentColor_ = TargetColor;
	}

	void Update()
	{
		if( LinearFactor > 0.0f && currentColor_ != TargetColor )
		{
			currentColor_ = Color.Lerp(currentColor_, TargetColor, LinearFactor);
			UpdateColor(currentColor_);
		}
	}

	public void UpdateToTarget()
	{
		currentColor_ = TargetColor;
		UpdateColor(TargetColor);
	}

	public void ForceSetColor(Color color)
	{
		SetColor(color);
		UpdateToTarget();
	}

	public void SetColor(Color color)
	{
		TargetColor = color;
#if UNITY_EDITOR
		if( !UnityEditor.EditorApplication.isPlaying )
		{
			UpdateColor(color);
			return;
		}
#endif
		if( LinearFactor <= 0.0f )
		{
			UpdateToTarget();
		}
	}

	public Color GetColor()
	{
		return TargetColor;
	}

	void UpdateColor(Color color)
	{
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
			else if( component is LineRenderer )
			{
				(component as LineRenderer).startColor = (component as LineRenderer).endColor = color;
			}
			else if( component is Renderer )
			{
				Material mat = new Material(Shader.Find("Shader Graphs/SimpleColor"));
				mat.hideFlags = HideFlags.DontSave;
				mat.SetColor("_Color", color);
				(component as Renderer).material = mat;
			}
		}
	}
}
