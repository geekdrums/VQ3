using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(RectTransform))]
public class UIGaugeRenderer : MaskableGraphic, IColoredObject
{
	public float Length = 200.0f;
	public float Width = 2.0f;
	public float Rate = 1.0f;
	public float StartRate = 0.0f;
	public float Slide = 0.0f;
	public Vector3 Direction = Vector3.right;
	
	Rect rect_ = new Rect();


#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();

		SetVerticesDirty();
	}
#endif

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		if( Direction.x > 0 )
		{
			rect_.xMin = Length * StartRate;
			rect_.xMax = Length * Rate;
			rect_.yMin = -Width / 2;
			rect_.yMax = Width / 2;
		}
		else if( Direction.x < 0 )
		{
			rect_.xMin = -Length * Rate;
			rect_.xMax = -Length * StartRate;
			rect_.yMin = -Width / 2;
			rect_.yMax = Width / 2;
		}
		else if( Direction.y > 0 )
		{
			rect_.yMin = Length * StartRate;
			rect_.yMax = Length * Rate;
			rect_.xMin = -Width / 2;
			rect_.xMax = Width / 2;
		}
		else if( Direction.y < 0 )
		{
			rect_.yMin = -Length * Rate;
			rect_.yMax = -Length * StartRate;
			rect_.xMin = -Width / 2;
			rect_.xMax = Width / 2;
		}
		else if( Direction.x == 0 )
		{
			rect_.xMin = -Length * Rate / 2;
			rect_.xMax = Length * Rate / 2;
			rect_.yMin = -Width / 2;
			rect_.yMax = Width / 2;
		}
		else return;

		// 左上
		UIVertex lt = UIVertex.simpleVert;
		lt.position = new Vector3(rect_.xMin + Slide, rect_.yMax, 0);
		lt.color = color;

		// 右上
		UIVertex rt = UIVertex.simpleVert;
		rt.position = new Vector3(rect_.xMax + Slide, rect_.yMax, 0);
		rt.color = color;

		// 右下
		UIVertex rb = UIVertex.simpleVert;
		rb.position = new Vector3(rect_.xMax, rect_.yMin, 0);
		rb.color = color;

		// 左下
		UIVertex lb = UIVertex.simpleVert;
		lb.position = new Vector3(rect_.xMin, rect_.yMin, 0);
		lb.color = color;

		if( vh.currentVertCount != 4 )
		{
			vh.Clear();
			vh.AddUIVertexQuad(new UIVertex[] {
				lb, rb, rt, lt
			});
		}
		else
		{
			vh.SetUIVertex(lb, 0);
			vh.SetUIVertex(rb, 1);
			vh.SetUIVertex(rt, 2);
			vh.SetUIVertex(lt, 3);
		}
	}
	
	public void SetLength(float length)
	{
		Length = length;
		SetVerticesDirty();
	}

	public void SetRate(float rate)
	{
		Rate = rate;
		SetVerticesDirty();
	}

	public void SetStartRate(float rate)
	{
		StartRate = rate;
		SetVerticesDirty();
	}

	public void SetWidth(float width)
	{
		Width = width;
		SetVerticesDirty();
	}

	public void SetColor(Color color)
	{
		this.color = color;
		SetVerticesDirty();
	}

	public Color GetColor()
	{
		return color;
	}
}
