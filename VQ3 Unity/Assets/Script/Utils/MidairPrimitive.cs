using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MidairPrimitive : MonoBehaviour, IColoredObject
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
	public float[] VertexAlphas;

	public int N { get { return (int)Mathf.Ceil(Num); } }
	public int ArcN { get { return Mathf.Min(N, (int)Mathf.Ceil(N * Mathf.Abs(ArcRate))); } }
	public float WholeRadius { get { return Radius - Width; } }

	public Texture2D GrowTexture;
	public MidairPrimitive GrowChild;
	public Animation ownerAnimation;
	public string materialName = "Transparent/Diffuse";
	public string colorName = "_Color";

	float currentArcRate_;
	Color currentColor_;
	float linearFactor_ = 0.3f;
	Vector3[] meshVertices_;
	Vector3[] normalizedVertices_;

	bool needArc_ { get { return Mathf.Abs(ArcRate) < 1.0f; } }
	bool isFlip_ { get { return ArcRate < 0.0f; } }
	int inOffset_ { get { return (isFlip_ ? 1 : 0); } }
	int outOffset_ { get { return (isFlip_ ? 0 : 1); } }

#if UNITY_EDITOR
	Mesh mesh_;
#endif
	Material mat_;
	float cos_;

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

	// Use this for initialization
	void Awake()
	{
#if UNITY_EDITOR
		mesh_ = new Mesh();
		mesh_.hideFlags = HideFlags.DontSave;
#endif
		ownerAnimation = GetComponentInParent<Animation>();

		if( Shader.Find(materialName) == null ) return;
		mat_ = new Material(Shader.Find(materialName));
		mat_.name = materialName;
		mat_.hideFlags = HideFlags.DontSave;

		cos_ = Mathf.Cos(Mathf.PI / N);
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
			UpdateArc(needArc_);
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
			UpdateArc(needArc_);
		}
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
		bool isNChanged = (meshVertices_ == null || meshVertices_.Length != vertexCount);
		if( isNChanged )
		{
			RecalculatePolygon();
		}
	}

	public void UpdateArc(bool force = false)
	{
		CheckVertex();
		if( currentArcRate_ != ArcRate || force )
		{
			ArcRate = Mathf.Clamp(ArcRate, -1.0f, 1.0f);
			if( currentArcRate_ * ArcRate <= 0 ) RecalculatePolygon();

			float OutR = Radius / cos_;
			float InR = Mathf.Max(0, (Radius - Width)) / cos_;

			Vector3 normalVertex = Quaternion.AngleAxis(Angle + Mathf.Sign(ArcRate) * ArcN * (360.0f / N), Vector3.forward) * Vector3.up;
			Vector3 OutVertex = normalVertex * OutR;
			Vector3 InVertex = normalVertex * InR;

			float angle = (2 * Mathf.PI / N) * ((float)ArcN - Mathf.Abs(ArcRate) * N);
			Matrix4x4 rotateMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(Mathf.Sign(ArcRate) * (-angle) * (180.0f / Mathf.PI), Vector3.forward), Vector3.one);
			InVertex = rotateMatrix * InVertex;
			OutVertex = rotateMatrix * OutVertex;
			normalVertex = rotateMatrix * normalVertex;
			float lRatio = cos_;
			float rRatio = 2 * Mathf.Sin(angle / 2) * Mathf.Sin(Mathf.PI / N - angle / 2);
			InVertex *= lRatio / (lRatio + rRatio);
			OutVertex *= lRatio / (lRatio + rRatio);
			meshVertices_[2 * ArcN + inOffset_] = InVertex;
			meshVertices_[2 * ArcN + outOffset_] = OutVertex;
			normalizedVertices_[ArcN] = normalVertex;

			Mesh mesh = UsableMesh;
			mesh.vertices = meshVertices_;


			GetComponent<MeshFilter>().mesh = mesh;
			currentArcRate_ = ArcRate;
		}
	}

	void RecalculateRadius()
	{
		CheckVertex();
		float OutR = Radius / cos_;
		int InOffset = inOffset_;
		int OutOffset = outOffset_;
		for( int i = 0; i < ArcN + (needArc_ ? 0 : 1); ++i )
		{
			if( 2 * i >= meshVertices_.Length )
			{
				//Debug.Log("vertexCount = " + meshVertices.Length + ", i = " + i);
			}
			else
			{
				meshVertices_[2 * i + OutOffset] = normalizedVertices_[i] * OutR;
			}
		}

		Mesh mesh = UsableMesh;
		mesh.vertices = meshVertices_;
		GetComponent<MeshFilter>().mesh = mesh;
	}

	void RecalculateWidth()
	{
		CheckVertex();
		float InR = Mathf.Max(0, (Radius - Width)) / cos_;
		int InOffset = inOffset_;
		int OutOffset = outOffset_;
		for( int i = 0; i < ArcN + (needArc_ ? 0 : 1); ++i )
		{
			if( 2 * i >= meshVertices_.Length )
			{
				//Debug.Log("vertexCount = " + meshVertices.Length + ", i = " + i);
			}
			else
			{
				meshVertices_[2 * i + InOffset] = normalizedVertices_[i] * InR;
			}
		}

		Mesh mesh = UsableMesh;
		mesh.vertices = meshVertices_;
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
			if( mesh_ == null )
			{
				mesh_ = new Mesh();
				mesh_.hideFlags = HideFlags.DontSave;
			}
			mesh = mesh_;
		}
#else
        mesh = GetComponent<MeshFilter>().mesh;
#endif

		int vertexCount = ArcN * 2 + 2;
		bool isNChanged = (mesh.vertices.Length != vertexCount || meshVertices_ == null || meshVertices_.Length != vertexCount || currentArcRate_ * ArcRate <= 0);
		if( isNChanged )
		{
			cos_ = Mathf.Cos(Mathf.PI / N);
			mesh.triangles = null;
			meshVertices_ = new Vector3[vertexCount];
			normalizedVertices_ = new Vector3[ArcN+1];

			float OutR = Radius / cos_;
			float InR =  Mathf.Max(0, (Radius - Width)) / cos_;

			Vector3 normalVertex = Quaternion.AngleAxis(Mathf.Sign(ArcRate) * Angle, Vector3.forward) * Vector3.up;
			Vector3 OutVertex = normalVertex * OutR;
			Vector3 InVertex = normalVertex * InR;
			int InOffset = inOffset_;
			int OutOffset = outOffset_;

			//vertex
			Matrix4x4 rotateMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(Mathf.Sign(ArcRate) * (360.0f / N), Vector3.forward), Vector3.one);
			for( int i = 0; i < ArcN; ++i )
			{
				meshVertices_[2 * i + InOffset] = InVertex;
				meshVertices_[2 * i + OutOffset] = OutVertex;
				normalizedVertices_[i] = normalVertex;
				InVertex = rotateMatrix * InVertex;
				OutVertex = rotateMatrix * OutVertex;
				normalVertex = rotateMatrix * normalVertex;
			}
			ArcRate = Mathf.Clamp(ArcRate, -1.0f, 1.0f);
			if( needArc_ )
			{
				float angle = (2 * Mathf.PI / N) * ((float)ArcN - Mathf.Abs(ArcRate) * N);
				rotateMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(Mathf.Sign(ArcRate) * (-angle) * (180.0f / Mathf.PI), Vector3.forward), Vector3.one);
				InVertex = rotateMatrix * InVertex;
				OutVertex = rotateMatrix * OutVertex;
				float lRatio = cos_;
				float rRatio = 2 * Mathf.Sin(angle / 2) * Mathf.Sin(Mathf.PI / N - angle / 2);
				InVertex *= lRatio / (lRatio + rRatio);
				OutVertex *= lRatio / (lRatio + rRatio);
				normalVertex = rotateMatrix * normalVertex;
			}
			meshVertices_[2 * ArcN + InOffset] = InVertex;
			meshVertices_[2 * ArcN + OutOffset] = OutVertex;

			normalizedVertices_[ArcN] = normalVertex;

			mesh.vertices = meshVertices_;

			// color
			if( VertexAlphas != null && VertexAlphas.Length == N )
			{
				Color[] colors = new Color[vertexCount];
				for( int i = 0; i < ArcN; ++i )
				{
					colors[2 * i + InOffset] = ColorManager.MakeAlpha(Color.white, Mathf.Lerp(VertexAlphas[i], VertexAlphas[(i + N/2) % N], (OutR - InR)/(OutR * 2)));
					colors[2 * i + OutOffset] = ColorManager.MakeAlpha(Color.white, VertexAlphas[i]);
				}
				colors[2 * ArcN + InOffset] = ColorManager.MakeAlpha(Color.white, Mathf.Lerp(VertexAlphas[0], VertexAlphas[N/2], (OutR - InR)/(OutR * 2)));
				colors[2 * ArcN + OutOffset] = ColorManager.MakeAlpha(Color.white, VertexAlphas[0]);

				mesh.colors = colors;
			}

			// uv
			mesh.uv = new Vector2[vertexCount];
			Vector2[] uvs = new Vector2[vertexCount];
			for( int i = 0; i < ArcN + 1; ++i )
			{
				if( i % 2 == 0 )
				{
					uvs[2 * i + InOffset] = UVZero;
					uvs[2 * i + OutOffset] = UVRight;
				}
				else
				{
					uvs[2 * i + InOffset] = UVUp;
					uvs[2 * i + OutOffset] = UVOne;
				}
			}
			mesh.uv = uvs;

			//normal
			int[] indices = new int[6 * ArcN];
			for( int i = 0; i < ArcN; ++i )
			{
				for( int j = 0; j < 6; ++j )
				{
					indices[6 * i + j] = (2 * i + quadIndices[j]);
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

			currentArcRate_ = ArcRate;
		}

		return mesh;
	}

	public void SetTargetSize(float newTargetSize)
	{
		AnimManager.AddAnim(gameObject, newTargetSize, ParamType.PrimitiveRadius, AnimType.Linear, linearFactor_);
	}
	public void SetTargetWidth(float newTargetWidth)
	{
		AnimManager.AddAnim(gameObject, newTargetWidth, ParamType.PrimitiveWidth, AnimType.Linear, linearFactor_);
	}
	public void SetTargetColor(Color newTargetColor)
	{
		AnimManager.AddAnim(gameObject, newTargetColor, ParamType.Color, AnimType.Linear, linearFactor_);
	}
	public void SetTargetArc(float newTargetArcRate)
	{
		AnimManager.AddAnim(gameObject, newTargetArcRate, ParamType.PrimitiveArc, AnimType.Linear, linearFactor_);
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
		Radius = newSize;
		RecalculateRadius();
		RecalculateWidth();
		if( GrowChild != null )
		{
			GrowChild.SetSize(Radius + GrowSize);
		}
	}
	public void SetWidth(float newWidth)
	{
		Width = newWidth;
		RecalculateWidth();
		if( GrowChild != null )
		{
			GrowChild.SetWidth(Width + GrowSize * 2);
		}
	}
	public void SetColor(Color newColor)
	{
		if( currentColor_ == newColor )
		{
			return;
		}

		Color = newColor;
		currentColor_ = Color;
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
	
	//IColoredObject
	public Color GetColor()
	{
		return Color;
	}

	public void SetArc(float newArc)
	{
		ArcRate = newArc;
		UpdateArc();
		if( GrowChild != null )
		{
			GrowChild.SetArc(newArc);
		}
	}

	void InitMaterial()
	{
		if( mat_ == null || mat_.name != materialName )
		{
			if( Shader.Find(materialName) == null ) return;
			mat_ = new Material(Shader.Find(materialName));
		}

		mat_.name = materialName;
		mat_.hideFlags = HideFlags.DontSave;
		mat_.SetColor(colorName, Color);
		if( materialName == "Standard" )
		{
			mat_.SetInt("_Mode", 3);
		}
		if( this.name == "_grow" )
		{
			if( GetComponentInParent<MidairPrimitive>() != null )
			{
				mat_.mainTexture = GetComponentInParent<MidairPrimitive>().GrowTexture;
			}
			else return;
		}
		GetComponent<Renderer>().material = mat_;
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
		linearFactor_ = factor;
	}
}
