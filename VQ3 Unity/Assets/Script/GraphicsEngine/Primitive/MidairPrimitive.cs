using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MidairPrimitive : MeshComponentBase, IColoredObject
{
	public int Num = 3;
	public float ArcRate = 1.0f;
	public float StartArcRate = 0.0f;
	public float Width = 1.0f;
	public float Radius = 1.0f;
	public Color Color = Color.black;
	public float Angle;
	public float GrowSize = 0;
	[Range(0, 1)]
	public float GrowAlpha = 1;

	#region params

	Vector3[] vertices_;
	/*
	 * 3------------------------1		vertices_[2*n+1]
	 * 
	 * 
	 * 
	 * 2------------------0				vertices_[2*n]
	 */
	Vector3[] normalizedVertices_;
	Vector3 arcEndNormal_;
	Vector3 arcStartNormal_;
	int[] vertexIndices_;
	Vector2[] vertexUVs_;

	// property cache
	int cachedNum_;
	float cachedArcRate_;
	float cachedStartArcRate_;
	float cachedWidth_;
	float cachedRadius_;
	Color cachedColor_;
	float cachedGrowSize_;
	float cachedGrowAlpha_;
	float cachedAngle_;

	// calcurated cache
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
		bool arcEndChanged		= cachedArcRate_	!= ArcRate;
		bool arcStartChanged	= cachedStartArcRate_	!= StartArcRate;
		bool widthChanged		= cachedWidth_		!= Width;
		bool radiusChanged		= cachedRadius_		!= Radius;
		bool colorChanged		= cachedColor_		!= Color;
		bool growSizeChanged	= cachedGrowSize_	!= GrowSize;
		bool growAlphaChanged	= cachedGrowAlpha_	!= GrowAlpha;
		bool angleChanged		= cachedAngle_		!= Angle;

		if( numChanged || angleChanged )
		{
			RecalculatePolygon();
			return;
		}

		if( arcEndChanged )
		{
			RecalculateArcEnd();
		}
		if( arcStartChanged )
		{
			RecalculateArcStart();
		}

		if( radiusChanged || growSizeChanged )
		{
			RecalculateRadius();
		}
		else if( widthChanged )
		{
			if( Width < 0 )
			{
				RecalculateRadius();
			}
			else
			{
				RecalculateWidth();
			}
		}

		if( colorChanged || growAlphaChanged || growSizeChanged )
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
		bool arcEndChanged = cachedArcRate_ != ArcRate;
		if( arcEndChanged )
		{
			RecalculateArcEnd();
		}
	}

	public void SetStartArc(float newStartArc)
	{
		StartArcRate = newStartArc;
		bool arcStartChanged = cachedStartArcRate_ != StartArcRate;
		if( arcStartChanged )
		{
			RecalculateArcStart();
		}
	}

	#endregion


	#region vertex culculate

	void UpdateCulculateCache()
	{
		float factor = 1.0f / Mathf.Cos(Mathf.PI / Num);
		if( Width >= 0 )
		{
			outRadius_ = Mathf.Max(0, Radius) * factor;
			inRadius_ = Mathf.Max(0, (Radius - Width)) * factor;
		}
		else
		{
			outRadius_ = (Mathf.Max(0, Radius) - Width) * factor;
			inRadius_ = Mathf.Max(0, Radius) * factor;
		}
		growOutRadius_ = outRadius_ + Mathf.Max(0, GrowSize);
		growInRadius_ = Mathf.Max(0, inRadius_ - Mathf.Max(0, GrowSize));
	}

	void UpdatePropertyCache()
	{
		cachedNum_ = Num;
		cachedArcRate_ = ArcRate;
		cachedStartArcRate_ = StartArcRate;
		cachedWidth_ = Width;
		cachedRadius_ = Radius;
		cachedColor_ = Color;
		cachedGrowSize_ = GrowSize;
		cachedGrowAlpha_ = GrowAlpha;
		cachedAngle_ = Angle;
	}

	bool CheckVertex()
	{
		int vertexCount = Num * 2 + 2;
		bool isNChanged = (vertices_ == null || vertices_.Length != vertexCount);
		if( isNChanged )
		{
			RecalculatePolygon();
			return true;
		}
		return false;
	}

	void UpdateArcStartParams()
	{
		float startArcRateFrac = Math.Max(0.0f, StartArcRate % 1.0f);
		if( startArcRateFrac == 0.0f )
		{
			arcStartNormal_ = normalizedVertices_[0];
		}
		else
		{
			int nearestEdge = ((int)Mathf.Floor(Num * startArcRateFrac) + 1) % Num;
			arcStartNormal_ = CalcArcEdgeNormal(startArcRateFrac, nearestEdge);
		}
	}

	void UpdateArcEndParams()
	{
		float arcRateFrac = Math.Max(0.0f, ArcRate % 1.0f);
		if( arcRateFrac == 0.0f )
		{
			arcEndNormal_ = normalizedVertices_[0];
		}
		else
		{
			int nearestEdge = (int)Mathf.Ceil(Num * arcRateFrac) % Num;
			arcEndNormal_ = CalcArcEdgeNormal(arcRateFrac, nearestEdge);
		}
	}

	Vector3 CalcArcEdgeNormal(float arcRate, int nearestEdge)
	{
		Vector3 normalVertex = normalizedVertices_[nearestEdge];
		float angleOffset = (2 * Mathf.PI) * ((float)nearestEdge / Num - arcRate);
		Matrix4x4 rotateMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-angleOffset * (180.0f / Mathf.PI), Vector3.forward), Vector3.one);
		normalVertex = rotateMatrix * normalVertex;
		float lRatio = Mathf.Cos(Mathf.PI / Num);
		float rRatio = 2 * Mathf.Sin(angleOffset / 2) * Mathf.Sin(Mathf.PI / Num - angleOffset / 2);
		float factor = lRatio / (lRatio + rRatio);

		return normalVertex * factor;
	}

	void RecalculateArcEnd()
	{
		if( CheckVertex() )
		{
			return;
		}
		
		UpdateArcEndParams();
		RecalculateVertices();
		UpdateVertices();
		cachedArcRate_ = ArcRate;
	}

	void RecalculateArcStart()
	{
		if( CheckVertex() )
		{
			return;
		}
		
		UpdateArcStartParams();
		RecalculateVertices();
		UpdateVertices();
		cachedStartArcRate_ = StartArcRate;
	}

	void RecalculateRadius()
	{
		if( CheckVertex() )
		{
			return;
		}

		UpdateCulculateCache();
		RecalculateVertices();
		UpdateVertices();

		cachedWidth_ = Width;
		cachedRadius_ = Radius;
		cachedGrowSize_ = GrowSize;
		if( GrowSize > 0 )
		{
			UpdateColor();
		}
	}

	void RecalculateWidth()
	{
		if( CheckVertex() )
		{
			return;
		}

		UpdateCulculateCache();
		RecalculateVertices();

		cachedWidth_ = Width;
		cachedGrowSize_ = GrowSize;
		if( GrowSize > 0 )
		{
			UpdateColor();
		}
		UpdateVertices();
	}

	public override void RecalculatePolygon(bool forceReflesh = false)
	{
		base.RecalculatePolygon();

		if( Num < 3 )
		{
			Num = 3;
		}

		bool shouldRecreateVertex = (vertices_ == null || cachedNum_ != Num);
		if( shouldRecreateVertex || forceReflesh )
		{
			int vertexCount = Num * 2 + 4;
			vertices_ = new Vector3[vertexCount];
			normalizedVertices_ = new Vector3[Num];
			// indices
			vertexIndices_ = new int[(Num + 1) * QuadIndices.Length];
			for( int i = 0; i <= Num; ++i )
			{
				for( int j = 0; j < QuadIndices.Length; ++j )
				{
					int vertIndex = 2 * i + QuadIndices[j];
					vertexIndices_[i * QuadIndices.Length + j] = vertIndex;
				}
			}
			// uvs
			vertexUVs_ = new Vector2[vertexCount];
			for( int i = 0; i < vertexCount; ++i )
			{
				vertexUVs_[i] = QuadUVs[i % QuadUVs.Length];
			}
		}

		//calc normal
		Vector3 normalVertex = Quaternion.AngleAxis(Angle, Vector3.forward) * Vector3.up;
		Matrix4x4 rotateMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis((360.0f / Num), Vector3.forward), Vector3.one);
		for( int i = 0; i < Num; ++i )
		{
			normalizedVertices_[i] = normalVertex;
			normalVertex = rotateMatrix * normalVertex;
		}

		UpdateCulculateCache();
		UpdateArcStartParams();
		UpdateArcEndParams();
		RecalculateVertices();

		UpdateColor();
		PopulateMesh();
		UpdatePropertyCache();
	}

	void RecalculateVertices()
	{
		SetVertex(0, arcStartNormal_);

		float arcStart = Num * (StartArcRate % 1.0f);
		float arcEnd = Num * (ArcRate % 1.0f);
		if( arcStart == arcEnd && ArcRate != StartArcRate )
		{
			// 一周する
			arcEnd = arcStart + Num;
		}
		float prevArc = arcStart;
		int currentArc = (int)Mathf.Floor(arcStart) + 1;
		int vertIndex = 1;
		int vertMax = vertices_.Length / 2;
		while( (prevArc <= arcEnd && arcEnd <= currentArc) == false && vertIndex < vertMax - 1 )
		{
			currentArc %= Num;
			SetVertex(vertIndex, normalizedVertices_[currentArc]);
			++currentArc;
			prevArc = ((int)prevArc + 1) % Num;
			++vertIndex;
		}

		SetVertex(vertIndex, arcEndNormal_);
		++vertIndex;

		for( ; vertIndex < vertMax; ++vertIndex )
		{
			SetVertex(vertIndex, arcEndNormal_);
		}
	}

	void SetVertex(int index, Vector3 normalVertex)
	{
		Vector3 growOutVertex = normalVertex * growOutRadius_;
		Vector3 growInVertex = normalVertex * growInRadius_;

		vertices_[2 * index] = growInVertex;
		vertices_[2 * index + 1] = growOutVertex;
	}

	void PopulateMesh()
	{
		Mesh mesh = MeshInstance;
		mesh.triangles = null;
		mesh.vertices = vertices_;
		mesh.triangles = vertexIndices_;
		mesh.uv = vertexUVs_;
		mesh.RecalculateBounds();
	}

	void UpdateVertices()
	{
		Mesh mesh = MeshInstance;
		mesh.vertices = vertices_;
		mesh.RecalculateBounds();
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

	protected override string DefaultMaterialName { get { return "Shader Graphs/MidairPrimitiveShader"; } }
	protected static int ColorPropertyID;
	protected static int GrowRateOuterPropertyID;
	protected static int GrowRateInnerPropertyID;
	protected static int GrowAlphaPropertyID;

	static MidairPrimitive()
	{
		ColorPropertyID = Shader.PropertyToID("_Color");
		GrowRateOuterPropertyID = Shader.PropertyToID("_GrowRateOuter");
		GrowRateInnerPropertyID = Shader.PropertyToID("_GrowRateInner");
		GrowAlphaPropertyID = Shader.PropertyToID("_GrowAlpha");
	}

	public void UpdateColor()
	{
		if( material_ == null )
		{
			MeshDirty = true;
			return;
		}
		material_.SetColor(ColorPropertyID, Color);
		material_.SetFloat(GrowAlphaPropertyID, Mathf.Clamp01(GrowAlpha));
		float sizePerRate = (growOutRadius_ - growInRadius_);
		material_.SetFloat(GrowRateOuterPropertyID, Mathf.Clamp01(GrowSize / sizePerRate));
		material_.SetFloat(GrowRateInnerPropertyID, Mathf.Clamp01(Mathf.Min(GrowSize, inRadius_ - growInRadius_) / sizePerRate));
		
		cachedColor_ = Color;
		cachedGrowAlpha_ = GrowAlpha;
	}

	#endregion
}
