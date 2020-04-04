using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(RectTransform))]
public class UIMidairPrimitive : MaskableGraphic, IColoredObject
{
	[FormerlySerializedAs("N")]
	public uint Num = 3;
	public float Width = 10;
	public float Radius = 10;
	[Range(0,1)]
	public float ArcRate = 1.0f;
	[Range(0, 1)]
	public float StartArcRate = 0.0f;
	public float Angle;
	public float GrowSize = 1;
	public float GrowAlpha = 1;

	#region params

	UIVertex[] vertices_;
	UIVertex[] growOutVertices_;
	UIVertex[] growInVertices_;
	/*
	 * 3----------------------------1	growOutVertices_[2*n+1]
	 * 
	 * 2------------------------0		growOutVertices_[2*n]
	 * 3------------------------1		uiVertices_[2*n+1]
	 * 
	 * 
	 * 
	 * 2------------------0				uiVertices_[2*n]
	 * 3------------------1				growInVertices_[2*n + 1]
	 * 
	 * 2--------------0					growInVertices_[2*n]
	 */
	Vector3[] normalizedVertices_;
	Vector3 arcEndNormal_;
	Vector3 arcStartNormal_;
	List<int> vertexIndices_ = new List<int>();

	// param cache
	uint cachedNum_ = 0;
	float cachedArcRate_;
	float cachedStartArcRate_;
	float cachedWidth_;
	float cachedRadius_;
	float cachedGrowSize_;
	float cachedGrowAlpha_;

	int arcStartIndex_;
	int arcEndIndex_;
	float outRadius_;
	float inRadius_;
	float growOutRadius_;
	float growInRadius_;


	int ArcEndVertexIndex { get { return Mathf.Min((int)Num, (int)Mathf.Ceil(Num * Mathf.Clamp01(ArcRate))); } }
	int ArcStartVertexIndex { get { return Mathf.Min((int)Num - 1, (int)Mathf.Floor(Num * Mathf.Clamp01(StartArcRate))); } }
	uint DesiredVertexCount { get { return Num * 2 + 2; } }

	static readonly int[] QuadIndices = new int[] { 0, 2, 1, 3, 1, 2 };

	#endregion


	#region unity functions

	protected override void Awake()
	{
		base.Awake();
		CheckInit();
	}

	bool CheckInit()
	{
		if( vertices_ == null )
		{
			RecalculatePolygon();
			return true;
		}
		return false;
	}

	void Update()
	{
		if( cachedNum_ != Num )
		{
			RecalculatePolygon();
		}
		else
		{
			if( cachedRadius_ != Radius )
			{
				RecalculateRadius();
			}
			else if( cachedWidth_ != Width )
			{
				RecalculateWidth();
			}
			else if( cachedGrowSize_ != GrowSize )
			{
				RecalculateGrowSize();
			}


			if( cachedArcRate_ != ArcRate )
			{
				RecalculateArcEnd();
			}
			if( cachedStartArcRate_ != StartArcRate )
			{
				RecalculateArcStart();
			}

			if( cachedGrowAlpha_ != GrowAlpha )
			{
				UpdateColor();
			}
		}
	}

#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();

		RecalculatePolygon();
		SetVerticesDirty();
	}
#endif

	#endregion


	#region public functions

	public void SetNum(uint num)
	{
		Num = num;
		RecalculatePolygon();
	}

	public void SetSize(float newSize)
	{
		Radius = newSize;
		RecalculateRadius();
	}

	public void SetWidth(float newWidth)
	{
		Width = newWidth;
		RecalculateWidth();
	}

	public void SetArc(float newArc)
	{
		ArcRate = newArc;
		RecalculateArcEnd();
	}

	public void SetStartArc(float newStartArc)
	{
		StartArcRate = newStartArc;
		RecalculateArcStart();
	}

	public void SetGrowSize(float newGrowSize)
	{
		GrowSize = newGrowSize;
		RecalculateGrowSize();
	}

	#endregion


	#region vertex culculate

	void UpdateArcStartParams()
	{
		arcStartIndex_ = ArcStartVertexIndex;
		if( StartArcRate > 0.0f )
		{
			if( StartArcRate >= 1.0f )
			{
				arcStartNormal_ = normalizedVertices_[normalizedVertices_.Length - 1];
			}
			else
			{
				arcStartNormal_ = CalcArcEdgeNormal(StartArcRate, normalizedVertices_[arcStartIndex_ + 1]);
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
		if( ArcRate < 1.0f )
		{
			if( ArcRate < StartArcRate )
			{
				arcEndNormal_ = arcStartNormal_;
			}
			else if( ArcRate <= 0.0f )
			{
				arcEndNormal_ = normalizedVertices_[0];
			}
			else
			{
				arcEndNormal_ = CalcArcEdgeNormal(ArcRate, normalizedVertices_[arcEndIndex_]);
			}
		}
		else
		{
			arcEndNormal_ = normalizedVertices_[normalizedVertices_.Length - 1];
		}
	}

	void RecalculateArcEnd()
	{
		if( CheckInit() ) return;

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

		cachedArcRate_ = ArcRate;

		SetVerticesDirty();
	}

	void RecalculateArcStart()
	{
		if( CheckInit() ) return;

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

		cachedStartArcRate_ = StartArcRate;

		SetVerticesDirty();
	}
	
	void UpdateRadiusParams()
	{
		float factor = 1.0f / Mathf.Cos(Mathf.PI / Num);
		outRadius_ = Radius * factor;
		inRadius_ = Mathf.Max(0, (Radius - Width)) * factor;
		growOutRadius_ = outRadius_ + GrowSize;
		growInRadius_ = Mathf.Max(0, inRadius_ - GrowSize);
	}

	void RecalculateRadius()
	{
		if( CheckInit() ) return;

		UpdateRadiusParams();

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

		cachedRadius_ = Radius;

		SetVerticesDirty();
	}

	void RecalculateWidth()
	{
		if( CheckInit() ) return;

		UpdateRadiusParams();

		for( int i = 0; i <= arcStartIndex_; ++i )
		{
			vertices_[2 * i].position = arcStartNormal_ * inRadius_;
			growInVertices_[2 * i].position = arcStartNormal_ * growInRadius_;
			growInVertices_[2 * i + 1].position = vertices_[2 * i].position;
		}
		for( int i = arcStartIndex_ + 1; i < arcEndIndex_; ++i )
		{
			vertices_[2 * i].position = normalizedVertices_[i] * inRadius_;
			growInVertices_[2 * i].position = normalizedVertices_[i] * growInRadius_;
			growInVertices_[2 * i + 1].position = vertices_[2 * i].position;
		}
		for( int i = Math.Max(arcStartIndex_ + 1, arcEndIndex_); i < normalizedVertices_.Length; ++i )
		{
			vertices_[2 * i].position = arcEndNormal_ * inRadius_;
			growInVertices_[2 * i].position = arcEndNormal_ * growInRadius_;
			growInVertices_[2 * i + 1].position = vertices_[2 * i].position;
		}

		cachedWidth_ = Width;

		SetVerticesDirty();
	}

	void RecalculateGrowSize()
	{
		if( CheckInit() ) return;

		growOutRadius_ = outRadius_ + GrowSize;
		growInRadius_ = Mathf.Max(0, inRadius_ - GrowSize);

		for( int i = 0; i <= arcStartIndex_; ++i )
		{
			growInVertices_[2 * i].position = arcStartNormal_ * growInRadius_;
			growOutVertices_[2 * i + 1].position = arcStartNormal_ * growOutRadius_;
		}
		for( int i = arcStartIndex_ + 1; i < arcEndIndex_; ++i )
		{
			growInVertices_[2 * i].position = normalizedVertices_[i] * growInRadius_;
			growOutVertices_[2 * i + 1].position = normalizedVertices_[i] * growOutRadius_;
		}
		for( int i = Math.Max(arcStartIndex_ + 1, arcEndIndex_); i < normalizedVertices_.Length; ++i )
		{
			growInVertices_[2 * i].position = arcEndNormal_ * growInRadius_;
			growOutVertices_[2 * i + 1].position = arcEndNormal_ * growOutRadius_;
		}

		cachedGrowSize_ = GrowSize;

		SetVerticesDirty();
	}

	void RecalculatePolygon()
	{
		if( Num < 3 )
		{
			Num = 3;
		}

		bool shouldRecreateVertex = (vertices_ == null || cachedNum_ != Num);
		if( shouldRecreateVertex )
		{
			uint vertexCount = DesiredVertexCount;
			vertices_ = new UIVertex[vertexCount];
			growInVertices_ = new UIVertex[vertexCount];
			growOutVertices_ = new UIVertex[vertexCount];
			normalizedVertices_ = new Vector3[vertexCount/2];
			for( int i = 0; i < vertices_.Length; ++i )
			{
				vertices_[i] = UIVertex.simpleVert;
				growInVertices_[i] = UIVertex.simpleVert;
				growOutVertices_[i] = UIVertex.simpleVert;
			}

			vertexIndices_.Clear();
			for( int i = 0; i < Num; ++i )
			{
				for( int j = 0; j < QuadIndices.Length; ++j )
				{
					vertexIndices_.Add(2 * i + QuadIndices[j]);
				}
			}

			cachedNum_ = Num;
		}

		//calc normal
		Vector3 normalVertex = Quaternion.AngleAxis(Angle, Vector3.forward) * Vector3.up;
		Matrix4x4 rotateMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis((360.0f / Num), Vector3.forward), Vector3.one);
		for( int i = 0; i < normalizedVertices_.Length; ++i )
		{
			normalizedVertices_[i] = normalVertex;
			normalVertex = rotateMatrix * normalVertex;
		}

		UpdateRadiusParams();
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

		cachedRadius_ = Radius;
		cachedWidth_ = Width;

		cachedArcRate_ = ArcRate;
		cachedStartArcRate_ = StartArcRate;

		cachedGrowSize_ = GrowSize;
		cachedGrowAlpha_ = GrowAlpha;

		UpdateColor();
	}

	void SetVertex(int index, Vector3 normalVertex)
	{
		Vector3 outVertex = normalVertex * outRadius_;
		Vector3 inVertex = normalVertex * inRadius_;
		Vector3 growOutVertex = normalVertex * growOutRadius_;
		Vector3 growInVertex = normalVertex * growInRadius_;

		vertices_[2 * index].position = inVertex;
		vertices_[2 * index + 1].position = outVertex;
		growInVertices_[2 * index].position = growInVertex;
		growInVertices_[2 * index + 1].position = inVertex;
		growOutVertices_[2 * index].position = outVertex;
		growOutVertices_[2 * index + 1].position = growOutVertex;
	}

	Vector3 CalcArcEdgeNormal(float arcRate, Vector3 normalVertex)
	{
		float angleOffset = (2 * Mathf.PI / Num) * (1.0f - (arcRate * Num) % 1.0f);
		Matrix4x4 rotateMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-angleOffset * (180.0f / Mathf.PI), Vector3.forward), Vector3.one);
		normalVertex = rotateMatrix * normalVertex;
		float lRatio = Mathf.Cos(Mathf.PI / Num);
		float rRatio = 2 * Mathf.Sin(angleOffset / 2) * Mathf.Sin(Mathf.PI / Num - angleOffset / 2);
		float factor = lRatio / (lRatio + rRatio);

		return normalVertex * factor;
	}
	

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		if( vertices_ == null )
		{
			RecalculatePolygon();
		}

		int unitVertCount = vertices_.Length;
		if( vh.currentVertCount != unitVertCount * 3 )
		{
			vh.Clear();

			for( int i = 0; i < unitVertCount; ++i )
			{
				vh.AddVert(vertices_[i]);
			}
			for( int i = 0; i + 2 < vertexIndices_.Count; i += 3 )
			{
				vh.AddTriangle(vertexIndices_[i], vertexIndices_[i + 1], vertexIndices_[i + 2]);
			}
			
			for( int i = 0; i < unitVertCount; ++i )
			{
				vh.AddVert(growInVertices_[i]);
			}
			for( int i = 0; i + 2 < vertexIndices_.Count; i += 3 )
			{
				vh.AddTriangle(unitVertCount + vertexIndices_[i], unitVertCount + vertexIndices_[i + 1], unitVertCount + vertexIndices_[i + 2]);
			}

			for( int i = 0; i < unitVertCount; ++i )
			{
				vh.AddVert(growOutVertices_[i]);
			}
			for( int i = 0; i + 2 < vertexIndices_.Count; i += 3 )
			{
				vh.AddTriangle(unitVertCount * 2 + vertexIndices_[i], unitVertCount * 2 + vertexIndices_[i + 1], unitVertCount * 2 + vertexIndices_[i + 2]);
			}
		}
		else
		{
			for( int i = 0; i < unitVertCount; ++i )
			{
				vh.SetUIVertex(vertices_[i], i);
			}
			for( int i = 0; i < unitVertCount; ++i )
			{
				vh.SetUIVertex(growInVertices_[i], unitVertCount + i);
			}
			for( int i = 0; i < unitVertCount; ++i )
			{
				vh.SetUIVertex(growOutVertices_[i], unitVertCount * 2 + i);
			}
		}
	}

	#endregion


	#region color update
	
	public void SetGrowAlpha(float newGrowAlpha)
	{
		GrowAlpha = newGrowAlpha;
		UpdateColor();
		SetVerticesDirty();
	}

	public void SetColor(Color newColor)
	{
		color = newColor;
		UpdateColor();
		SetVerticesDirty();
	}

	public Color GetColor()
	{
		return color;
	}

	void UpdateColor()
	{
		if( vertices_ == null )
		{
			RecalculatePolygon();
		}
		else
		{
			for( int i = 0; i < vertices_.Length; ++i )
			{
				vertices_[i].color = color;
				growInVertices_[i].color = ColorManager.MakeAlpha(color, color.a * (i % 2 == 0 ? 0 : GrowAlpha));
				growOutVertices_[i].color = ColorManager.MakeAlpha(color, color.a * (i % 2 == 0 ? GrowAlpha : 0));
			}
			cachedGrowAlpha_ = GrowAlpha;
		}
	}

	#endregion
}
