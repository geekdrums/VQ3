using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MidairPrimitive : MeshComponentBase, IColoredObject
{
	public uint Num = 3;
	public float ArcRate = 1.0f;
	public float Width = 100;
	public float Radius = 100;
	public Color Color = Color.black;
	public float Angle;
	public float GrowSize = 1;
	public float GrowAlpha = 1;

	#region params

	List<Vector3> meshVertices_ = new List<Vector3>();
	VertexIndexer vertices_;
	VertexIndexer growOutVertices_;
	VertexIndexer growInVertices_;
	/*
	 * 3----------------------------1	growOutVertices_[2*n+1]
	 * 
	 * 2------------------------0		growOutVertices_[2*n]
	 * 3------------------------1		meshVertices_[2*n+1]
	 * 
	 * 
	 * 
	 * 2------------------0				meshVertices_[2*n]
	 * 3------------------1				growInVertices_[2*n + 1]
	 * 
	 * 2--------------0					growInVertices_[2*n]
	 */
	List<Vector3> normalizedVertices_ = new List<Vector3>();
	List<int> vertexIndices_ = new List<int>();
	List<Vector2> vertexUVs_ = new List<Vector2>();
	List<Color> vertexColors_ = new List<Color>();

	int DesiredArcN { get { return Mathf.Min((int)Num, (int)Mathf.Ceil(Num * Mathf.Max(0, ArcRate))); } }
	int DesiredVertexCount { get { return DesiredArcN * 2 + 2; } }

	// property cache
	uint cachedNum_;
	float cachedArcRate_;
	float cachedWidth_;
	float cachedRadius_;
	Color cachedColor_;
	float cachedGrowSize_;
	float cachedGrowAlpha_;

	// calcurated cache
	int arcN_;
	float outRadius_;
	float inRadius_;
	float growOutRadius_;
	float growInRadius_;

	#endregion


	#region unity functions
	
	void Awake()
	{
		UpdatePropertyCache();
	}

	void Update()
	{
		CheckPropertyCacheUpdate();
	}

	protected override void OnValidate()
	{
		base.OnValidate();

		CheckPropertyCacheUpdate();
		MeshDirty = false;
	}

	void CheckPropertyCacheUpdate()
	{
		bool numChanged			= cachedNum_		!= Num;
		bool arcRateChanged		= cachedArcRate_	!= ArcRate;
		bool widthChanged		= cachedWidth_		!= Width;
		bool radiusChanged		= cachedRadius_		!= Radius;
		bool colorChanged		= cachedColor_		!= Color;
		bool growSizeChanged	= cachedGrowSize_	!= GrowSize;
		bool growAlphaChanged	= cachedGrowAlpha_	!= GrowAlpha;

		if( numChanged )
		{
			RecalculatePolygon();
			return;
		}

		if( arcRateChanged )
		{
			RecalculateArc();
		}
		
		if( radiusChanged || growSizeChanged )
		{
			RecalculateRadius();
		}
		else if( widthChanged )
		{
			RecalculateWidth();
		}

		if( colorChanged || growAlphaChanged )
		{
			UpdateColor();
		}
	}

	#endregion


	#region public functions

	public void SetSize(float newSize)
	{
		Radius = newSize;
		RecalculateRadius();
	}

	public void SetGrowSize(float newGrowSize)
	{
		GrowSize = newGrowSize;
		RecalculateRadius();
	}

	public void SetWidth(float newWidth)
	{
		Width = newWidth;
		if( Width >= 0 )
		{
			RecalculateWidth();
		}
		else
		{
			RecalculateRadius();
			RecalculateWidth();
		}
	}

	public void SetArc(float newArc)
	{
		ArcRate = newArc;
		RecalculateArc();
	}

	#endregion


	#region vertex culculate

	void UpdateCulculateCache()
	{
		arcN_ = DesiredArcN;
		float factor = 1.0f / Mathf.Cos(Mathf.PI / Num);
		if( Width >= 0 )
		{
			outRadius_ = Radius * factor;
			inRadius_ = Mathf.Max(0, (Radius - Width)) * factor;
		}
		else
		{
			outRadius_ = (Radius - Width) * factor;
			inRadius_ = Radius * factor;
		}
		growOutRadius_ = outRadius_ + GrowSize;
		growInRadius_ = Mathf.Max(0, inRadius_ - GrowSize);
	}

	void UpdatePropertyCache()
	{
		cachedNum_ = Num;
		cachedArcRate_ = ArcRate;
		cachedWidth_ = Width;
		cachedRadius_ = Radius;
		cachedColor_ = Color;
		cachedGrowSize_ = GrowSize;
		cachedGrowAlpha_ = GrowAlpha;
	}

	bool CheckVertex()
	{
		int vertexCount = DesiredVertexCount;
		bool isNChanged = (vertices_ == null || vertices_.Length != vertexCount);
		if( isNChanged )
		{
			RecalculatePolygon();
			return true;
		}
		return false;
	}

	void RecalculateArc()
	{
		if( CheckVertex() )
		{
			return;
		}

		if( cachedArcRate_ != ArcRate )
		{
			Vector3 normalVertex = Quaternion.AngleAxis(Angle + arcN_ * (360.0f / Num), Vector3.forward) * Vector3.up;
			float angle = (2 * Mathf.PI / Num) * ((float)arcN_ - ArcRate * Num);
			Matrix4x4 rotateMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-angle * (180.0f / Mathf.PI), Vector3.forward), Vector3.one);
			normalVertex = rotateMatrix * normalVertex;
			float lRatio = Mathf.Cos(Mathf.PI / Num);
			float rRatio = 2 * Mathf.Sin(angle / 2) * Mathf.Sin(Mathf.PI / Num - angle / 2);
			float factor = lRatio / (lRatio + rRatio);

			normalizedVertices_[arcN_] = normalVertex;
			vertices_[2 * arcN_] = normalVertex * inRadius_ * factor;
			vertices_[2 * arcN_ + 1] = normalVertex * outRadius_ * factor;
			growInVertices_[2 * arcN_] = normalVertex * growInRadius_ * factor;
			growInVertices_[2 * arcN_ + 1] = vertices_[2 * arcN_];
			growOutVertices_[2 * arcN_] = vertices_[2 * arcN_ + 1];
			growOutVertices_[2 * arcN_ + 1] = normalVertex * growOutRadius_ * factor;

			UpdateVertices();
			cachedArcRate_ = ArcRate;
		}
	}

	void RecalculateRadius()
	{
		if( CheckVertex() )
		{
			return;
		}

		UpdateCulculateCache();

		for( int i = 0; i < arcN_ + 1; ++i )
		{
			if( 2 * i >= vertices_.Length )
			{
				Debug.Log("vertexCount = " + vertices_.Length + ", i = " + i);
			}
			else
			{
				vertices_[2 * i] = normalizedVertices_[i] * inRadius_;
				vertices_[2 * i + 1] = normalizedVertices_[i] * outRadius_;
				growInVertices_[2 * i] = normalizedVertices_[i] * growInRadius_;
				growInVertices_[2 * i + 1] = normalizedVertices_[i] * inRadius_;
				growOutVertices_[2 * i] = normalizedVertices_[i] * outRadius_;
				growOutVertices_[2 * i + 1] = normalizedVertices_[i] * growOutRadius_;
			}
		}

		cachedWidth_ = Width;
		cachedRadius_ = Radius;
		cachedGrowSize_ = GrowSize;
		UpdateVertices();
	}

	void RecalculateWidth()
	{
		if( CheckVertex() )
		{
			return;
		}

		UpdateCulculateCache();

		for( int i = 0; i < arcN_ + 1; ++i )
		{
			if( 2 * i >= vertices_.Length )
			{
				Debug.Log("vertexCount = " + vertices_.Length + ", i = " + i);
			}
			else
			{
				vertices_[2 * i] = normalizedVertices_[i] * inRadius_;
				growInVertices_[2 * i] = normalizedVertices_[i] * growInRadius_;
				growInVertices_[2 * i + 1] = vertices_[2 * i];
			}
		}

		cachedWidth_ = Width;
		cachedGrowSize_ = GrowSize;
		UpdateVertices();
	}

	public override void RecalculatePolygon(bool forceReflesh = false)
	{
		base.RecalculatePolygon();

		if( Num < 3 )
		{
			Num = 3;
		}

		UpdateCulculateCache();
		UpdatePropertyCache();

		int vertexCount = DesiredVertexCount;
		bool isNChanged = (vertices_ == null || vertices_.Length != vertexCount);
		if( isNChanged || forceReflesh )
		{
			// vertices
			int meshVertCount = vertexCount * 3;
			while( meshVertices_.Count < meshVertCount )
			{
				meshVertices_.Add(Vector3.zero);
			}
			if( meshVertices_.Count > meshVertCount )
			{
				meshVertices_.RemoveRange(meshVertCount, meshVertices_.Count - meshVertCount);
			}

			if( vertices_ == null )
			{
				vertices_ = new VertexIndexer(meshVertices_);
				growInVertices_ = new VertexIndexer(meshVertices_);
				growOutVertices_ = new VertexIndexer(meshVertices_);
			}
			vertices_.Set(0, vertexCount);
			growInVertices_.Set(vertexCount, vertexCount);
			growOutVertices_.Set(vertexCount * 2, vertexCount);

			// normal
			int normalVertCount = arcN_ + 1;
			while( normalizedVertices_.Count < normalVertCount )
			{
				normalizedVertices_.Add(Vector3.zero);
			}
			// normalは計算用なので減らしてぴったりにしなくても良い

			// indices
			int vertIndicesCount = 0;
			for( int kinds = 0; kinds < 3; ++kinds )// vert, growIn, growOutの3種類
			{
				for( int i = 0; i < arcN_; ++i )
				{
					for( int j = 0; j < QuadIndices.Length; ++j )
					{
						int vertIndex = vertexCount * kinds + 2 * i + QuadIndices[j];
						if( vertexIndices_.Count <= vertIndicesCount )
						{
							vertexIndices_.Add(vertIndex);
						}
						else
						{
							vertexIndices_[vertIndicesCount] = vertIndex;
						}
						vertIndicesCount++;
					}
				}
			}
			if( vertexIndices_.Count > vertIndicesCount )
			{
				vertexIndices_.RemoveRange(vertIndicesCount, vertexIndices_.Count - vertIndicesCount);
			}

			// uvs
			while( vertexUVs_.Count < meshVertCount )
			{
				if( (vertexUVs_.Count/2) % 2 == 0 )
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
			if( vertexUVs_.Count > meshVertCount )
			{
				vertexUVs_.RemoveRange(meshVertCount, vertexUVs_.Count - meshVertCount);
			}

			// colors
			while( vertexColors_.Count < meshVertCount )
			{
				vertexColors_.Add(Color);
			}
			if( vertexColors_.Count > meshVertCount )
			{
				vertexColors_.RemoveRange(meshVertCount, vertexColors_.Count - meshVertCount);
			}
			Color growAlpha = ColorManager.MakeAlpha(Color, Color.a * GrowAlpha);
			Color growClear = ColorManager.MakeAlpha(Color, 0);
			for( int i = 0; i < vertexCount; ++i )
			{
				vertexColors_[i] = Color;
			}
			for( int i = 0; i < vertexCount; ++i )
			{
				vertexColors_[i + vertexCount] = (i % 2 == 0 ? growClear : growAlpha);
			}
			for( int i = 0; i < vertexCount; ++i )
			{
				vertexColors_[i + vertexCount * 2] = (i % 2 == 0 ? growAlpha : growClear);
			}
		}

		Vector3 normalVertex = Quaternion.AngleAxis(Angle, Vector3.forward) * Vector3.up;
		Vector3 outVertex = normalVertex * outRadius_;
		Vector3 inVertex = normalVertex * inRadius_;
		Vector3 growOutVertex = normalVertex * growOutRadius_;
		Vector3 growInVertex = normalVertex * growInRadius_;

		//vertex
		Matrix4x4 rotateMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis((360.0f / Num), Vector3.forward), Vector3.one);
		for( int i = 0; i < arcN_; ++i )
		{
			// set
			vertices_[2 * i] = inVertex;
			vertices_[2 * i + 1] = outVertex;
			growInVertices_[2 * i] = growInVertex;
			growInVertices_[2 * i + 1] = inVertex;
			growOutVertices_[2 * i] = outVertex;
			growOutVertices_[2 * i + 1] = growOutVertex;
			normalizedVertices_[i] = normalVertex;

			// rotate
			inVertex = rotateMatrix * inVertex;
			outVertex = rotateMatrix * outVertex;
			growInVertex = rotateMatrix * growInVertex;
			growOutVertex = rotateMatrix * growOutVertex;
			normalVertex = rotateMatrix * normalVertex;
		}
		if( ArcRate < 1.0f )
		{
			float angle = (2 * Mathf.PI / Num) * ((float)arcN_ - ArcRate * Num);
			rotateMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-angle * (180.0f / Mathf.PI), Vector3.forward), Vector3.one);
			inVertex = rotateMatrix * inVertex;
			outVertex = rotateMatrix * outVertex;
			growInVertex = rotateMatrix * growInVertex;
			growOutVertex = rotateMatrix * growOutVertex;
			float lRatio = Mathf.Cos(Mathf.PI / Num);
			float rRatio = 2 * Mathf.Sin(angle / 2) * Mathf.Sin(Mathf.PI / Num - angle / 2);
			float factor = lRatio / (lRatio + rRatio);
			inVertex *= factor;
			outVertex *= factor;
			growInVertex *= factor;
			growOutVertex *= factor;
			normalVertex = rotateMatrix * normalVertex;
		}
		vertices_[2 * arcN_] = inVertex;
		vertices_[2 * arcN_ + 1] = outVertex;
		growInVertices_[2 * arcN_] = growInVertex;
		growInVertices_[2 * arcN_ + 1] = inVertex;
		growOutVertices_[2 * arcN_] = outVertex;
		growOutVertices_[2 * arcN_ + 1] = growOutVertex;

		normalizedVertices_[arcN_] = normalVertex;

		PopulateMesh();
	}

	void PopulateMesh()
	{
		Mesh mesh = MeshInstance;
		
		mesh.triangles = null;
		mesh.SetVertices(meshVertices_);
		mesh.SetTriangles(vertexIndices_, 0, false);
		mesh.SetUVs(0, vertexUVs_);
		mesh.SetColors(vertexColors_);
		mesh.RecalculateBounds();

		MeshInstance = mesh;
	}

	void UpdateVertices()
	{
		Mesh mesh = MeshInstance;
		mesh.SetVertices(meshVertices_);
		mesh.RecalculateBounds();
		MeshInstance = mesh;
	}

	#endregion


	#region color update

	//IColoredObject
	public void SetColor(Color newColor)
	{
		Color = newColor;
		UpdateColor();
	}

	public Color GetColor()
	{
		return Color;
	}

	void UpdateColor()
	{
		if( CheckVertex() )
		{
			return;
		}

		Color growAlpha = ColorManager.MakeAlpha(Color, Color.a * GrowAlpha);
		Color growClear = ColorManager.MakeAlpha(Color, 0);
		int vertexCount = DesiredVertexCount;
		for( int i = 0; i < vertexCount; ++i )
		{
			vertexColors_[i] = Color;
		}
		for( int i = 0; i < vertexCount; ++i )
		{
			vertexColors_[i + vertexCount] = (i % 2 == 0 ? growClear : growAlpha);
		}
		for( int i = 0; i < vertexCount; ++i )
		{
			vertexColors_[i + vertexCount * 2] = (i % 2 == 0 ? growAlpha : growClear);
		}

		cachedColor_ = Color;
		cachedGrowAlpha_ = GrowAlpha;

		Mesh mesh = MeshInstance;
		mesh.SetColors(vertexColors_);
	}

	#endregion
}
