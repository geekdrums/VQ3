using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MidairRect : MeshComponentBase, IColoredObject
{
	public float Height = 100;
	public float Width = 100;
	public Vector2 Pivot = Vector2.zero;
	public Color RectColor = Color.black;
	public float Thickness = 100;
	public float GrowInSize = 0;
	public float GrowOutSize = 0;
	public float GrowAlpha = 0;

	List<Vector3> meshVertices_;
	VertexIndexer vertices_;
	VertexIndexer growOutVertices_;
	VertexIndexer growInVertices_;
	List<int> vertexIndices_;
	List<Vector2> vertexUVs_;
	List<Color> meshColors_;

	static int RightUpOuterIndex = 1;
	static int LeftUpOuterIndex = 3;
	static int LeftDownOuterIndex = 5;
	static int RightDownOuterIndex = 7;

	static int RightUpInnerIndex = 0;
	static int LeftUpInnerIndex = 2;
	static int LeftDownInnerIndex = 4;
	static int RightDownInnerIndex = 6;

	void Update()
	{
	}
	
	public override void RecalculatePolygon(bool forceReflesh = false)
	{
		base.RecalculatePolygon();

		Mesh mesh = MeshInstance;

		int num = 4;
		int vertCount = num * 2;
		int meshVertCount = vertCount * 3;
		if( vertices_ == null || forceReflesh )
		{
			meshVertices_ = new List<Vector3>();
			for( int i = 0; i < meshVertCount; ++i )
			{
				meshVertices_.Add(new Vector3());
			}
			mesh.SetVertices(meshVertices_);

			vertices_ = new VertexIndexer(meshVertices_);
			growInVertices_ = new VertexIndexer(meshVertices_);
			growOutVertices_ = new VertexIndexer(meshVertices_);

			vertices_.Set(0, 8);
			growInVertices_.Set(8, 8);
			growOutVertices_.Set(8 * 2, 8);

			vertexIndices_ = new List<int>();
			for( int kinds = 0; kinds < 3; ++kinds ) // vert, growIn, growOutの3種類
			{
				for( int i = 0; i < num; ++i )
				{
					for( int j = 0; j < QuadIndices.Length; ++j )
					{
						vertexIndices_.Add(vertCount * kinds + (2 * i + QuadIndices[j]) % vertCount);
					}
				}
			}

			vertexUVs_ = new List<Vector2>();
			for( int i = 0; i < meshVertCount/2; ++i )
			{
				if( i % 2 == 0 )
				{
					vertexUVs_.Add(UVZero);
					vertexUVs_.Add(UVRight);
				}
				else
				{
					vertexUVs_.Add(UVUp);
					vertexUVs_.Add(UVOne);
				}
			}
			
			meshColors_ = new List<Color>();
			for( int i = 0; i < meshVertCount; ++i )
			{
				meshColors_.Add(RectColor);
			}
		}


		// 値をキャッシュ
		Rect rect = new Rect(-Pivot.x * Width, -Pivot.y * Height, Width, Height);
		Vector3 center = rect.center;
		float xMax = rect.xMax;
		float xMin = rect.xMin;
		float yMax = rect.yMax;
		float yMin = rect.yMin;

		// rectの外側を設定
		// 右上
		vertices_[RightUpOuterIndex] = new Vector3(xMax, yMax, 0);
		// 左上
		vertices_[LeftUpOuterIndex] = new Vector3(xMin, yMax, 0);
		// 左下
		vertices_[LeftDownOuterIndex] = new Vector3(xMin, yMin, 0);
		// 右下
		vertices_[RightDownOuterIndex] = new Vector3(xMax, yMin, 0);

		// rectの内側、Thicknessが十分大きければcenterで良い
		if( Math.Min(rect.width, rect.height) / 2 <= Thickness )
		{
			vertices_[RightUpInnerIndex] = center;
			vertices_[LeftUpInnerIndex] = center;
			vertices_[LeftDownInnerIndex] = center;
			vertices_[RightDownInnerIndex] = center;
		}
		else
		{
			vertices_[RightUpOuterIndex] = new Vector3(xMax - Thickness, yMax - Thickness, 0);
			vertices_[LeftUpOuterIndex] = new Vector3(xMin + Thickness, yMax - Thickness, 0);
			vertices_[LeftDownOuterIndex] = new Vector3(xMin + Thickness, yMin + Thickness, 0);
			vertices_[RightDownOuterIndex] = new Vector3(xMax - Thickness, yMin + Thickness, 0);
		}

		// growOutを設定。内側の座標はRect外側と一致（頂点カラーのために別Vertexにしている）
		growOutVertices_[RightUpInnerIndex] = vertices_[RightUpOuterIndex];
		growOutVertices_[LeftUpInnerIndex] = vertices_[LeftUpOuterIndex];
		growOutVertices_[LeftDownInnerIndex] = vertices_[LeftDownOuterIndex];
		growOutVertices_[RightDownInnerIndex] = vertices_[RightDownOuterIndex];
		// 外側はGrowOutSizeの分だけ広げる
		growOutVertices_[RightUpOuterIndex] = new Vector3(xMax + GrowOutSize, yMax + GrowOutSize, 0);
		growOutVertices_[LeftUpOuterIndex] = new Vector3(xMin - GrowOutSize, yMax + GrowOutSize, 0);
		growOutVertices_[LeftDownOuterIndex] = new Vector3(xMin - GrowOutSize, yMin - GrowOutSize, 0);
		growOutVertices_[RightDownOuterIndex] = new Vector3(xMax + GrowOutSize, yMin - GrowOutSize, 0);

		// growInを設定。外側の座標はRect内側と一致
		growInVertices_[RightUpOuterIndex] = vertices_[RightUpInnerIndex];
		growInVertices_[LeftUpOuterIndex] = vertices_[LeftUpInnerIndex];
		growInVertices_[LeftDownOuterIndex] = vertices_[LeftDownInnerIndex];
		growInVertices_[RightDownOuterIndex] = vertices_[RightDownInnerIndex];
		// 内側はGrowInSizeの分だけ狭めるが、狭ければcenterで良い
		if( Math.Min(rect.width, rect.height) / 2 <= (Thickness + GrowInSize) )
		{
			growInVertices_[RightUpInnerIndex] = center;
			growInVertices_[LeftUpInnerIndex] = center;
			growInVertices_[LeftDownInnerIndex] = center;
			growInVertices_[RightDownInnerIndex] = center;
		}
		else
		{
			growInVertices_[RightUpInnerIndex] = new Vector3(xMax - (Thickness + GrowInSize), yMax - (Thickness + GrowInSize), 0);
			growInVertices_[LeftUpInnerIndex] = new Vector3(xMin + (Thickness + GrowInSize), yMax - (Thickness + GrowInSize), 0);
			growInVertices_[LeftDownInnerIndex] = new Vector3(xMin + (Thickness + GrowInSize), yMin + (Thickness + GrowInSize), 0);
			growInVertices_[RightDownInnerIndex] = new Vector3(xMax - (Thickness + GrowInSize), yMin + (Thickness + GrowInSize), 0);
		}

		Color growAlpha = ColorManager.MakeAlpha(RectColor, RectColor.a * GrowAlpha);
		Color growClear = ColorManager.MakeAlpha(RectColor, 0);
		for( int i = 0; i < vertCount; ++i )
		{
			meshColors_[i] = RectColor;
		}
		for( int i = 0; i < vertCount; ++i )
		{
			meshColors_[i + vertCount] = i % 2 == 0 ? growClear : growAlpha;
		}
		for( int i = 0; i < vertCount; ++i )
		{
			meshColors_[i + vertCount * 2] = i % 2 == 0 ? growAlpha : growClear;
		}
		
		mesh.SetVertices(meshVertices_);
		mesh.SetColors(meshColors_);
		mesh.RecalculateBounds();

		MeshInstance = mesh;
	}

	public void SetColor(Color color)
	{
		RectColor = color;
		UpdateColor();
	}

	public Color GetColor()
	{
		return RectColor;
	}

	void UpdateColor()
	{
		if( meshColors_ != null )
		{
			int vertCount = 8;
			Color growAlpha = ColorManager.MakeAlpha(RectColor, RectColor.a * GrowAlpha);
			Color growClear = ColorManager.MakeAlpha(RectColor, 0);
			for( int i = 0; i < vertCount; ++i )
			{
				meshColors_[i] = RectColor;
			}
			for( int i = 0; i < vertCount; ++i )
			{
				meshColors_[i + vertCount] = i % 2 == 0 ? growClear : growAlpha;
			}
			for( int i = 0; i < vertCount; ++i )
			{
				meshColors_[i + vertCount * 2] = i % 2 == 0 ? growAlpha : growClear;
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
