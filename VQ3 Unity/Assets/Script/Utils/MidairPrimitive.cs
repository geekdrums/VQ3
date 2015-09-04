﻿using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MidairPrimitive : MonoBehaviour
{

	static readonly int[] quadIndices = new int[] { 0, 2, 1, 3, 1, 2 };
	static readonly Vector2 UVZero = new Vector2(0, 0);
	static readonly Vector2 UVRight = new Vector2(0, 1);
	static readonly Vector2 UVUp  = new Vector2(1, 0);
	static readonly Vector2 UVOne = new Vector2(1, 1);

	public float Num = 3;
	public float ArcRate = 1.0f;
	public float Width = 1;
	public float Radius = 1;
	public float ScaleX = 1;
	public Color Color = Color.white;
	public float Angle;
	public float GrowSize;
	public float GrowAlpha;

	public int N { get { return (int)Mathf.Ceil(Num); } }
	public int ArcN { get { return Mathf.Min(N, (int)Mathf.Ceil(N * ArcRate)); } }
	public float WholeRadius { get { return Radius - Width; } }

	public Texture2D GrowTexture;
	public MidairPrimitive GrowChild;
	public Animation ownerAnimation;
	public string materialName = "Transparent/Diffuse";
	public string colorName = "_Color";

	float targetWidth;
	float targetRadius;
	Color targetColor;
	float targetArcRate;
	float currentArcRate;
	float linearFactor = 0.3f;
	float minDistance = 0.05f;
	Vector3[] meshVertices;
	Vector3[] normalizedVertices;

	Mesh UsableMesh
	{
		get
		{
			Mesh mesh = null;
#if UNITY_EDITOR
			if( UnityEditor.EditorApplication.isPlaying )
			{
				mesh = GetComponent<MeshFilter>().mesh;
			}
			else
			{
				mesh = RecalculatePolygon();
			}
#else
            mesh = GetComponent<MeshFilter>().mesh;
#endif
			return mesh;
		}
	}

	[System.Flags]
	public enum AnimationParams
	{
		None = 0x0,
		Width = 0x1,
		Radius = 0x2,
		Color = 0x4,
		Arc = 0x8,
	}
	public AnimationParams animParam = AnimationParams.None;

	// Use this for initialization
	void Start()
	{
		targetWidth = Width;
		targetRadius = Radius;
		targetColor = Color;
		targetArcRate = ArcRate;
		ownerAnimation = GetComponentInParent<Animation>();
		RecalculatePolygon();
		InitMaterial();
	}

	// Update is called once per frame
	void Update()
	{
#if UNITY_EDITOR
		if( UnityEditor.EditorApplication.isPlaying == false )
		{
			RecalculatePolygon();
			RecalculateRadius();
			RecalculateWidth();
			SetColor(Color);
			UpdateGrow();
			return;
		}
#endif
		if( ownerAnimation != null && ownerAnimation.isPlaying )
		{
			RecalculatePolygon();
			RecalculateRadius();
			RecalculateWidth();
			GetComponent<Renderer>().material.SetColor(colorName, Color);
			UpdateGrow();
		}
		if( animParam != AnimationParams.None )
		{
			UpdateAnimation();
		}
		UpdateArc();
	}


	void UpdateAnimation()
	{
		float d = Mathf.Abs(Radius - targetRadius);
		bool updateRadius = false;
		if( d > float.Epsilon )
		{
			Radius = (d > minDistance ? Mathf.Lerp(Radius, targetRadius, linearFactor) : targetRadius);
			updateRadius = true;
		}
		if( Radius == targetRadius ) animParam &= ~AnimationParams.Radius;

		d = Mathf.Abs(Width - targetWidth);
		bool updateWidth = false;
		if( d > float.Epsilon )
		{
			Width = (d > minDistance ? Mathf.Lerp(Width, targetWidth, linearFactor) : targetWidth);
			updateWidth = true;
		}
		if( Width == targetWidth ) animParam &= ~AnimationParams.Width;

		d = Mathf.Abs(ArcRate - targetArcRate);
		bool updateArc = false;
		if( d > float.Epsilon )
		{
			ArcRate = (d > minDistance ? Mathf.Lerp(ArcRate, targetArcRate, linearFactor) : targetArcRate);
			updateArc = true;
		}
		if( ArcRate == targetArcRate ) animParam &= ~AnimationParams.Arc;

		if( updateRadius )
		{
			RecalculateRadius();
			if( GrowChild != null ) GrowChild.SetSize(Radius + GrowSize);
		}
		if( updateWidth || updateRadius )
		{
			RecalculateWidth();
			if( GrowChild != null ) GrowChild.SetWidth(Width + GrowSize * 2);
		}
		if( updateArc )
		{
			UpdateArc();
			if( GrowChild != null ) GrowChild.SetArc(ArcRate);
		}

		if( GrowChild != null ) GrowChild.SetColor(ColorManager.MakeAlpha(Color, GrowAlpha));
		if( ColorManager.Distance(Color, targetColor) < minDistance )
		{
			Color = targetColor;
			animParam &= ~AnimationParams.Color;
		}
		else
		{
			Color = Color.Lerp(Color, targetColor, linearFactor);
		}
		GetComponent<Renderer>().material.SetColor(colorName, Color);
	}

	public void UpdateGrow()
	{
		if( GrowChild != null )
		{
			GrowChild.SetSize(Radius + GrowSize);
			GrowChild.SetWidth(Width + GrowSize * 2);
			GrowChild.SetColor(ColorManager.MakeAlpha(Color, GrowAlpha));
			GrowChild.SetArc(ArcRate);
		}
	}

	void CheckVertex()
	{
		int vertexCount = ArcN * 2 + 2;
		bool isNChanged = (meshVertices == null || meshVertices.Length != vertexCount);
		if( isNChanged )
		{
			RecalculatePolygon();
		}
	}

	public void UpdateArc(bool force = false)
	{
		CheckVertex();
		if( currentArcRate != ArcRate || force )
		{
			float OutR = Radius / Mathf.Cos(Mathf.PI / N);
			float InR = Mathf.Max(0, (Radius - Width)) / Mathf.Cos(Mathf.PI / N);

			Vector3 normalVertex = Quaternion.AngleAxis(Angle + ArcN * (360.0f / N), Vector3.forward) * Vector3.up;
			Vector3 OutVertex = normalVertex * OutR;
			Vector3 InVertex = normalVertex * InR;

			float angle = (2 * Mathf.PI / N) * ((float)ArcN - ArcRate * N);
			Matrix4x4 rotateMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-angle * (180.0f / Mathf.PI), Vector3.forward), Vector3.one);
			InVertex = rotateMatrix * InVertex;
			OutVertex = rotateMatrix * OutVertex;
			normalVertex = rotateMatrix * normalVertex;
			float lRatio = Mathf.Cos(Mathf.PI / N);
			float rRatio = 2 * Mathf.Sin(angle / 2) * Mathf.Sin(Mathf.PI / N - angle / 2);
			InVertex *= lRatio / (lRatio + rRatio);
			OutVertex *= lRatio / (lRatio + rRatio);
			meshVertices[2 * ArcN] = InVertex;
			meshVertices[2 * ArcN + 1] = OutVertex;
			normalizedVertices[ArcN] = normalVertex;

			Mesh mesh = UsableMesh;
			mesh.vertices = meshVertices;
			GetComponent<MeshFilter>().mesh = mesh;
			currentArcRate = ArcRate;
		}
	}

	void RecalculateRadius()
	{
		CheckVertex();
		float OutR = Radius / Mathf.Cos(Mathf.PI / N);
		for( int i = 0; i < ArcN + 1; ++i )
		{
			if( 2 * i >= meshVertices.Length )
			{
				Debug.Log("vertexCount = " + meshVertices.Length + ", i = " + i);
			}
			else
			{
				meshVertices[2 * i + 1] = normalizedVertices[i] * OutR;
			}
		}
		if( ScaleX != 1.0f )
		{
			for( int i = 0; i <= ArcN; ++i )
			{
				meshVertices[2 * i + 1].x *= ScaleX;
				meshVertices[2 * i].x = meshVertices[2 * i + 1].x - Mathf.Sign(meshVertices[2 * i].x) * Mathf.Abs(meshVertices[2 * i + 1].y - meshVertices[2 * i].y);
			}
		}

		Mesh mesh = UsableMesh;
		mesh.vertices = meshVertices;
		GetComponent<MeshFilter>().mesh = mesh;
	}

	void RecalculateWidth()
	{
		CheckVertex();
		float InR = Mathf.Max(0, (Radius - Width)) / Mathf.Cos(Mathf.PI / N);
		for( int i = 0; i < ArcN + 1; ++i )
		{
			if( 2 * i >= meshVertices.Length )
			{
				Debug.Log("vertexCount = " + meshVertices.Length + ", i = " + i);
			}
			else
			{
				meshVertices[2 * i] = normalizedVertices[i] * InR;
			}
		}
		if( ScaleX != 1.0f )
		{
			for( int i = 0; i <= ArcN; ++i )
			{
				meshVertices[2 * i].x = meshVertices[2 * i + 1].x - Mathf.Sign(meshVertices[2 * i].x) * Mathf.Abs(meshVertices[2 * i + 1].y - meshVertices[2 * i].y);
			}
		}

		Mesh mesh = UsableMesh;
		mesh.vertices = meshVertices;
		GetComponent<MeshFilter>().mesh = mesh;
	}

	Mesh RecalculatePolygon()
	{
		if( Num < 3 )
		{
			Num = 3;
		}

		Mesh mesh = null;
#if UNITY_EDITOR
		if( UnityEditor.EditorApplication.isPlaying )
		{
			mesh = GetComponent<MeshFilter>().mesh;
		}
		else
		{
			mesh = new Mesh();
			mesh.hideFlags = HideFlags.DontSave;
		}
#else
        mesh = GetComponent<MeshFilter>().mesh;
#endif

		int vertexCount = ArcN * 2 + 2;
		bool isNChanged = (mesh.vertices.Length != vertexCount || meshVertices == null || meshVertices.Length != vertexCount);
		if( isNChanged )
		{
			mesh.triangles = null;
			meshVertices = new Vector3[vertexCount];
			normalizedVertices = new Vector3[ArcN+1];

			float OutR = Radius / Mathf.Cos(Mathf.PI / N);
			float InR = Mathf.Max(0, (Radius - Width)) / Mathf.Cos(Mathf.PI / N);

			Vector3 normalVertex = Quaternion.AngleAxis(Angle, Vector3.forward) * Vector3.up;
			Vector3 OutVertex = normalVertex * OutR;
			Vector3 InVertex = normalVertex * InR;

			//vertex
			Matrix4x4 rotateMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis((360.0f / N), Vector3.forward), Vector3.one);
			for( int i = 0; i < ArcN; ++i )
			{
				meshVertices[2 * i] = InVertex;
				meshVertices[2 * i + 1] = OutVertex;
				normalizedVertices[i] = normalVertex;
				InVertex = rotateMatrix * InVertex;
				OutVertex = rotateMatrix * OutVertex;
				normalVertex = rotateMatrix * normalVertex;
			}
			if( ArcRate < 1.0f )
			{
				float angle = (2*Mathf.PI / N) * ((float)ArcN - ArcRate * N);
				rotateMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-angle * (180.0f / Mathf.PI), Vector3.forward), Vector3.one);
				InVertex = rotateMatrix * InVertex;
				OutVertex = rotateMatrix * OutVertex;
				float lRatio = Mathf.Cos(Mathf.PI / N);
				float rRatio = 2 * Mathf.Sin(angle / 2) * Mathf.Sin(Mathf.PI / N - angle / 2);
				InVertex *= lRatio / (lRatio + rRatio);
				OutVertex *= lRatio / (lRatio + rRatio);
				normalVertex = rotateMatrix * normalVertex;
			}
			meshVertices[2 * ArcN] = InVertex;
			meshVertices[2 * ArcN + 1] = OutVertex;
			if( ScaleX != 1.0f )
			{
				for( int i = 0; i <= ArcN; ++i )
				{
					meshVertices[2 * i + 1].x *= ScaleX;
					meshVertices[2 * i].x = meshVertices[2 * i + 1].x - Mathf.Sign(meshVertices[2 * i].x) * Mathf.Abs(meshVertices[2 * i + 1].y - meshVertices[2 * i].y);
				}
			}

			normalizedVertices[ArcN] = normalVertex;
			mesh.vertices = meshVertices;

			// uv
			mesh.uv = new Vector2[vertexCount];
			Vector2[] uvs = new Vector2[vertexCount];
			for( int i = 0; i < ArcN + 1; ++i )
			{
				if( i % 2 == 0 )
				{
					uvs[2 * i] = UVZero;
					uvs[2 * i + 1] = UVRight;
				}
				else
				{
					uvs[2 * i] = UVUp;
					uvs[2 * i + 1] = UVOne;
				}
			}
			mesh.uv = uvs;

			//normal
			int[] indices = new int[6 * ArcN];
			for( int i = 0; i < ArcN; ++i )
			{
				for( int j = 0; j < 6; ++j )
				{
					indices[6 * i + j] = (2 * i + quadIndices[j]);// % mesh.vertices.Length;
				}
			}
			mesh.SetIndices(indices, MeshTopology.Triangles, 0);
			mesh.RecalculateNormals();

			GetComponent<MeshFilter>().mesh = mesh;

			if( GrowChild != null )
			{
				GrowChild.Num = Num;
				GrowChild.RecalculatePolygon();
			}

			currentArcRate = ArcRate;
		}

		return mesh;
	}

	public void SetTargetSize(float newTargetSize)
	{
		if( targetRadius == newTargetSize ) return;
		targetRadius = newTargetSize;
		animParam |= AnimationParams.Radius;
	}
	public void SetTargetWidth(float newTargetWidth)
	{
		if( targetWidth == newTargetWidth ) return;
		targetWidth = newTargetWidth;
		animParam |= AnimationParams.Width;
	}
	public void SetTargetColor(Color newTargetColor)
	{
		if( targetColor == newTargetColor ) return;
		targetColor = newTargetColor;
		animParam |= AnimationParams.Color;
	}
	public void SetTargetArc(float newTargetArcRate)
	{
		if( targetArcRate == newTargetArcRate ) return;
		targetArcRate = newTargetArcRate;
		animParam |= AnimationParams.Arc;
	}

	public void SetAnimationSize(float startSize, float endSize)
	{
		SetSize(startSize);
		SetTargetSize(endSize);
	}
	public void SetAnimationWidth(float startWidth, float endWidth)
	{
		SetWidth(startWidth);
		SetTargetWidth(endWidth);
	}
	public void SetAnimationColor(Color startColor, Color endColor)
	{
		SetColor(startColor);
		SetTargetColor(endColor);
	}
	public void SetAnimationArc(float startArc, float endArc)
	{
		SetArc(startArc);
		SetTargetArc(endArc);
	}

	public void SetSize(float newSize)
	{
		animParam &= ~AnimationParams.Radius;
		targetRadius = newSize;
		Radius = targetRadius;
		RecalculateRadius();
		if( GrowChild != null )
		{
			GrowChild.SetSize(Radius + GrowSize);
		}
	}
	public void SetWidth(float newWidth)
	{
		animParam &= ~AnimationParams.Width;
		targetWidth = newWidth;
		Width = targetWidth;
		RecalculateWidth();
		if( GrowChild != null )
		{
			GrowChild.SetWidth(Width + GrowSize * 2);
		}
	}
	public void SetColor(Color newTargetColor)
	{
		animParam &= ~AnimationParams.Color;
		targetColor = newTargetColor;
		Color = targetColor;
		if( GrowChild != null )
		{
			GrowChild.SetColor(ColorManager.MakeAlpha(Color, GrowAlpha));
		}

#if UNITY_EDITOR
		if( !UnityEditor.EditorApplication.isPlaying )
		{
			InitMaterial();
			return;
		}
#endif
		GetComponent<Renderer>().material.SetColor(colorName, Color);
	}

	public void SetArc(float newArc)
	{
		animParam &= ~AnimationParams.Arc;
		targetArcRate = newArc;
		ArcRate = targetArcRate;
		UpdateArc();
		if( GrowChild != null )
		{
			GrowChild.SetArc(Width + GrowSize * 2);
		}
	}

	void InitMaterial()
	{
		Material mat = new Material(Shader.Find(materialName));
		mat.name = "mat";
		mat.hideFlags = HideFlags.DontSave;
		mat.SetColor(colorName, Color);
		if( materialName == "Standard" )
		{
			mat.SetInt("_Mode", 3);
		}
		if( this.name == "_grow" )
		{
			if( GetComponentInParent<MidairPrimitive>() != null )
			{
				mat.mainTexture = GetComponentInParent<MidairPrimitive>().GrowTexture;
			}
			else return;
		}
		GetComponent<Renderer>().material = mat;
	}

	public void SetGrowSize(float newGrowSize)
	{
		if( GrowChild != null )
		{
			GrowSize = newGrowSize;
			GrowChild.SetSize(Radius + GrowSize);
		}
	}
	public void SetGrowAlpha(float newGrowAlpha)
	{
		if( GrowChild != null )
		{
			GrowAlpha = newGrowAlpha;
			GrowChild.SetColor(ColorManager.MakeAlpha(Color, GrowAlpha));
		}
	}

	public void SetLinearFactor(float factor)
	{
		linearFactor = factor;
	}

	void OnValidate()
	{
		RecalculatePolygon();
		RecalculateRadius();
		RecalculateWidth();
		UpdateGrow();
	}
}
