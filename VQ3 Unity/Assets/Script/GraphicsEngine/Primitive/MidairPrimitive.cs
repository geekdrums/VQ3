using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MidairPrimitive : MeshComponentBase, IColoredObject
{
	public uint Num = 3;
	[Range(0, 1)]
	public float ArcRate = 1.0f;
	[Range(0, 1)]
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

	float ArcRateMin { get { return Mathf.Min(ArcRate, StartArcRate); } }
	float ArcRateMax { get { return Mathf.Max(ArcRate, StartArcRate); } }
	int ArcEndVertexIndex { get { return Mathf.Min((int)Num, (int)Mathf.Ceil(Num * Mathf.Clamp01(ArcRateMax))); } }
	int ArcStartVertexIndex { get { return Mathf.Min((int)Num - 1, (int)Mathf.Floor(Num * Mathf.Clamp01(ArcRateMin))); } }
	int DesiredVertexCount { get { return (int)Num * 2 + 2; } }

	// property cache
	uint cachedNum_;
	float cachedArcRateMax_;
	float cachedArcRateMin_;
	float cachedWidth_;
	float cachedRadius_;
	Color cachedColor_;
	float cachedGrowSize_;
	float cachedGrowAlpha_;
	float cachedAngle_;

	// calcurated cache
	int arcStartIndex_;
	int arcEndIndex_;
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
		bool arcRateMaxChanged	= cachedArcRateMax_	!= ArcRateMax;
		bool arcRateMinChanged	= cachedArcRateMin_	!= ArcRateMin;
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

		if( arcRateMaxChanged )
		{
			RecalculateArcEnd();
		}
		if( arcRateMinChanged )
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
		bool arcRateMaxChanged = cachedArcRateMax_ != ArcRateMax;
		bool arcRateMinChanged = cachedArcRateMin_ != ArcRateMin;
		if( arcRateMaxChanged )
		{
			RecalculateArcEnd();
		}
		if( arcRateMinChanged )
		{
			RecalculateArcStart();
		}
	}

	public void SetStartArc(float newStartArc)
	{
		StartArcRate = newStartArc;
		bool arcRateMaxChanged = cachedArcRateMax_ != ArcRateMax;
		bool arcRateMinChanged = cachedArcRateMin_ != ArcRateMin;
		if( arcRateMaxChanged )
		{
			RecalculateArcEnd();
		}
		if( arcRateMinChanged )
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
		cachedArcRateMax_ = ArcRateMax;
		cachedArcRateMin_ = ArcRateMin;
		cachedWidth_ = Width;
		cachedRadius_ = Radius;
		cachedColor_ = Color;
		cachedGrowSize_ = GrowSize;
		cachedGrowAlpha_ = GrowAlpha;
		cachedAngle_ = Angle;
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

	void UpdateArcStartParams()
	{
		arcStartIndex_ = ArcStartVertexIndex;
		if( ArcRateMin > 0.0f )
		{
			if( ArcRateMin >= 1.0f )
			{
				arcStartNormal_ = normalizedVertices_[normalizedVertices_.Length - 1];
			}
			else
			{
				arcStartNormal_ = CalcArcEdgeNormal(ArcRateMin, normalizedVertices_[arcStartIndex_ + 1]);
			}
		}
		else
		{
			arcStartNormal_ = normalizedVertices_[0];
		}
	}

	void UpdateArcEndParams()
	{
		arcEndIndex_ = ArcEndVertexIndex;
		if( ArcRateMax < 1.0f )
		{
			if( ArcRate < StartArcRate )
			{
				arcEndNormal_ = arcStartNormal_;
			}
			else if( ArcRateMax <= 0.0f )
			{
				arcEndNormal_ = normalizedVertices_[0];
			}
			else
			{
				arcEndNormal_ = CalcArcEdgeNormal(ArcRateMax, normalizedVertices_[arcEndIndex_]);
			}
		}
		else
		{
			arcEndNormal_ = normalizedVertices_[normalizedVertices_.Length - 1];
		}
	}

	Vector3 CalcArcEdgeNormal(float arcRate, Vector3 normalVertex)
	{
		float arcN = arcRate * Num;
		float angleOffset = (2 * Mathf.PI / Num) * ((Mathf.Ceil(arcN) - arcN) % 1.0f);
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

		int oldArcEndIndex = arcEndIndex_;
		UpdateArcEndParams();

		for( int i = Math.Max(arcStartIndex_ + 1, arcEndIndex_); i < normalizedVertices_.Length; ++i )
		{
			SetVertex(i, arcEndNormal_);
		}

		if( oldArcEndIndex < arcEndIndex_ )
		{
			for( int i = oldArcEndIndex; i < arcEndIndex_; ++i )
			{
				SetVertex(i, normalizedVertices_[i]);
			}
		}
		else if( arcEndIndex_ < oldArcEndIndex )
		{
			for( int i = oldArcEndIndex; i > arcEndIndex_; --i )
			{
				SetVertex(i, arcEndNormal_);
			}
		}

		cachedArcRateMax_ = ArcRate;
		UpdateVertices();
	}

	void RecalculateArcStart()
	{
		if( CheckVertex() )
		{
			return;
		}

		int oldArcStartIndex = arcStartIndex_;
		UpdateArcStartParams();

		for( int i = 0; i <= arcStartIndex_; ++i )
		{
			SetVertex(i, arcStartNormal_);
		}

		if( oldArcStartIndex < arcStartIndex_ )
		{
			for( int i = oldArcStartIndex; i < arcStartIndex_; ++i )
			{
				SetVertex(i, arcStartNormal_);
			}
		}
		else if( arcStartIndex_ < oldArcStartIndex )
		{
			for( int i = oldArcStartIndex; i > arcStartIndex_; --i )
			{
				SetVertex(i, normalizedVertices_[i]);
			}
		}

		cachedArcRateMin_ = StartArcRate;
		UpdateVertices();
	}

	void RecalculateRadius()
	{
		if( CheckVertex() )
		{
			return;
		}

		UpdateCulculateCache();

		for( int i = 0; i <= arcStartIndex_; ++i )
		{
			SetVertex(i, arcStartNormal_);
		}
		for( int i = arcStartIndex_ + 1; i < arcEndIndex_; ++i )
		{
			SetVertex(i, normalizedVertices_[i]);
		}
		for( int i = arcEndIndex_; i < normalizedVertices_.Length; ++i )
		{
			SetVertex(i, arcEndNormal_);
		}

		cachedWidth_ = Width;
		cachedRadius_ = Radius;
		cachedGrowSize_ = GrowSize;
		if( GrowSize > 0 )
		{
			UpdateColor();
		}
		UpdateVertices();
	}

	void RecalculateWidth()
	{
		if( CheckVertex() )
		{
			return;
		}

		UpdateCulculateCache();

		for( int i = 0; i <= arcStartIndex_; ++i )
		{
			vertices_[2 * i] = arcStartNormal_ * growInRadius_;
		}
		for( int i = arcStartIndex_ + 1; i < arcEndIndex_; ++i )
		{
			vertices_[2 * i] = normalizedVertices_[i] * growInRadius_;
		}
		for( int i = Math.Max(arcStartIndex_ + 1, arcEndIndex_); i < normalizedVertices_.Length; ++i )
		{
			vertices_[2 * i] = arcEndNormal_ * growInRadius_;
		}

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
			int vertexCount = DesiredVertexCount;
			vertices_ = new Vector3[vertexCount];
			normalizedVertices_ = new Vector3[vertexCount / 2];
			// indices
			vertexIndices_ = new int[Num * QuadIndices.Length];
			for( int i = 0; i < Num; ++i )
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
		for( int i = 0; i < normalizedVertices_.Length; ++i )
		{
			normalizedVertices_[i] = normalVertex;
			normalVertex = rotateMatrix * normalVertex;
		}

		UpdateCulculateCache();
		UpdateArcStartParams();
		UpdateArcEndParams();

		for( int i = 0; i <= arcStartIndex_; ++i )
		{
			SetVertex(i, arcStartNormal_);
		}
		for( int i = arcStartIndex_ + 1; i < arcEndIndex_; ++i )
		{
			SetVertex(i, normalizedVertices_[i]);
		}
		for( int i = Math.Max(arcStartIndex_ + 1, arcEndIndex_); i < normalizedVertices_.Length; ++i )
		{
			SetVertex(i, arcEndNormal_);
		}

		UpdateColor();
		PopulateMesh();
		UpdatePropertyCache();
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

	void UpdateColor()
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
