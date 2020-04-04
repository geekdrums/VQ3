using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MeshComponentBase : MonoBehaviour
{
	public class VertexIndexer
	{
		public VertexIndexer(List<Vector3> vertices)
		{
			vertices_ = vertices;
		}

		List<Vector3> vertices_;
		int baseIndex_;

		public int Length { get { return length_; } }
		int length_ = 0;

		public Vector3 this[int i]
		{
			get
			{
				if( i < length_ && baseIndex_ + i < vertices_.Count )
				{
					return vertices_[baseIndex_ + i];
				}
				else
				{
					Debug.Log("index out of range i = " + i.ToString());
					return Vector3.zero;
				}
			}
			set
			{
				if( i < length_ && baseIndex_ + i < vertices_.Count )
				{
					vertices_[baseIndex_ + i] = value;
				}
				else
				{
					Debug.Log("index out of range i = " + i.ToString());
				}
			}
		}

		public void Set(int baseIndex, int length)
		{
			baseIndex_ = baseIndex;
			length_ = length;
		}
	}

	protected Mesh MeshInstance
	{
		get
		{
			if( mesh_ == null )
			{
				mesh_ = new Mesh();
				mesh_.MarkDynamic();
			}
			return mesh_;
		}
		set
		{
			mesh_ = value;
			meshFilter_.sharedMesh = value;
		}
	}
	protected MeshFilter meshFilter_;
	protected Mesh mesh_;
	protected Renderer renderer_;

	public bool MeshDirty { get; protected set; }

	protected static readonly string DefaultMaterialName = "Shader Graphs/SimpleVertexColor";
	protected static readonly int[] QuadIndices = new int[] { 0, 2, 1, 3, 1, 2 };
	protected static readonly Vector2 UVZero = new Vector2(0, 0);
	protected static readonly Vector2 UVRight = new Vector2(0, 1);
	protected static readonly Vector2 UVUp = new Vector2(1, 0);
	protected static readonly Vector2 UVOne = new Vector2(1, 1);

	protected virtual void Start()
	{
		RecalculatePolygon();
	}

	public virtual void RecalculatePolygon(bool forceReflesh = false)
	{
		if( meshFilter_ == null )
		{
			meshFilter_ = GetComponent<MeshFilter>();
		}

		if( renderer_ == null )
		{
			renderer_ = GetComponent<Renderer>();
		}

		if( renderer_ != null && renderer_.sharedMaterial == null )
		{
			InitMaterial();
		}

		MeshDirty = false;
	}

	protected virtual void OnValidate()
	{
		if( meshFilter_ == null )
		{
			meshFilter_ = GetComponent<MeshFilter>();
		}

		if( renderer_ == null )
		{
			renderer_ = GetComponent<Renderer>();
		}

		if( renderer_ != null && renderer_.sharedMaterial == null )
		{
			InitMaterial();
		}

		MeshDirty = true;
	}

	protected void InitMaterial()
	{
		Material mat = new Material(Shader.Find(DefaultMaterialName));
		mat.name = "mat";
		mat.hideFlags = HideFlags.DontSave;
		renderer_.sharedMaterial = mat;
	}
}
