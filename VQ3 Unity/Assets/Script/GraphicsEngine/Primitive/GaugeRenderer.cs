using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GaugeRenderer : MeshComponentBase, IColoredObject
{
	public float Length = 200.0f;
	public float Width = 2.0f;
	public float Rate = 1.0f;
	public float StartRate = 0.0f;
	public Color LineColor = Color.black;
	public float Slide = 0.0f;
	public Vector3 Direction = Vector3.right;
	
	Rect rect_ = new Rect();
	List<Vector3> meshVertices_;
	List<Color> meshColors_;

	public override void RecalculatePolygon(bool forceReflesh = false)
	{
		base.RecalculatePolygon();

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

		Mesh mesh = MeshInstance;
		
		if( meshVertices_ == null || meshVertices_.Count != 4 )
		{
			int meshVertCount = 4;
			meshVertices_ = new List<Vector3>();
			for( int i = 0; i < meshVertCount; ++i )
			{
				meshVertices_.Add(new Vector3());
			}
			mesh.SetVertices(meshVertices_);

			List<int> vertexIndices = new List<int>();
			for( int i = 0; i < QuadIndices.Length; ++i )
			{
				vertexIndices.Add(QuadIndices[i]);
			}
			mesh.SetTriangles(vertexIndices, 0, false);

			List<Vector2> vertexUVs = new List<Vector2>();
			vertexUVs.Add(UVZero);
			vertexUVs.Add(UVRight);
			vertexUVs.Add(UVUp);
			vertexUVs.Add(UVOne);
			mesh.SetUVs(0, vertexUVs);


			meshColors_ = new List<Color>();
			for( int i = 0; i < meshVertCount; ++i )
			{
				meshColors_.Add(LineColor);
			}
		}

		// 左上
		meshVertices_[0] = new Vector3(rect_.xMin + Slide, rect_.yMax, 0);

		// 左下
		meshVertices_[1] = new Vector3(rect_.xMin, rect_.yMin, 0);

		// 右上
		meshVertices_[2] = new Vector3(rect_.xMax + Slide, rect_.yMax, 0);

		// 右下
		meshVertices_[3] = new Vector3(rect_.xMax, rect_.yMin, 0);

		for( int i = 0; i < meshColors_.Count; ++i )
		{
			meshColors_[i] = LineColor;
		}

		mesh.SetVertices(meshVertices_);
		mesh.SetColors(meshColors_);
		mesh.RecalculateBounds();

		MeshInstance = mesh;
	}

	public void SetLength(float length)
	{
		Length = length;
		RecalculatePolygon();
	}

	public void SetRate(float rate)
	{
		Rate = rate;
		RecalculatePolygon();
	}

	public void SetStartRate(float rate)
	{
		StartRate = rate;
		RecalculatePolygon();
	}

	public void SetWidth(float width)
	{
		Width = width;
		RecalculatePolygon();
	}

	public void SetColor(Color color)
	{
		LineColor = color;

		UpdateColor();
	}

	public Color GetColor()
	{
		return LineColor;
	}

	void UpdateColor()
	{
		if( meshColors_ != null )
		{
			for( int i = 0; i < meshColors_.Count; ++i )
			{
				meshColors_[i] = LineColor;
			}

			Mesh mesh = MeshInstance;
			mesh.SetColors(meshColors_);
		}
		else
		{
			RecalculatePolygon();
		}
	}
}
