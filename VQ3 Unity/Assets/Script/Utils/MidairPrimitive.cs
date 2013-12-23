using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class MidairPrimitive : MonoBehaviour {

    public int N;

    //when Radius == Width, this will make no midair polygon.
    public float Width;
    public float Radius;
    public Color Color;
    public float GrowSize;
    public float GrowAlpha;

    public Texture2D GrowTexture;

    float targetWidth;
    float targetRadius;
    Color targetColor;
    float linearFactor = 0.3f;
    float minDistance = 0.05f;

    MidairPrimitive GrowChild;

	// Use this for initialization
	void Start () {
        targetWidth = Width;
        targetRadius = Radius;
        targetColor = Color;
        RecalculatePolygon();
	}
	
	// Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if( !UnityEditor.EditorApplication.isPlaying ) return;
#endif
        
        UpdateAnimation();
        UpdateGrow();
	}


    void UpdateAnimation()
    {
        float d = Mathf.Abs( Radius - targetRadius );
        if( d > minDistance / 2 )
        {
            Radius = (d > minDistance ? Mathf.Lerp( Radius, targetRadius, linearFactor ) : targetRadius);
            RecalculateRadius();
        }
        d = Mathf.Abs( Width - targetWidth );
        if( d > minDistance / 2 )
        {
            Width = (d > minDistance ? Mathf.Lerp( Width, targetWidth, linearFactor ) : targetWidth);
            RecalculateWidth();
        }
        Color = Color.Lerp( Color, targetColor, linearFactor );
        renderer.material.color = Color;
    }

    void UpdateGrow()
    {
        if( GrowSize > 0 && GrowChild == null )
        {
            GrowChild = (Instantiate( this.gameObject, transform.position, transform.rotation ) as GameObject).GetComponent<MidairPrimitive>();
            GrowChild.transform.parent = this.transform;
            GrowChild.transform.localScale = Vector3.one;
            GrowChild.renderer.material.mainTexture = GrowTexture;
            GrowChild.GrowSize = 0;
        }
        if( GrowChild != null )
        {
            GrowChild.SetColor( Color * GrowAlpha );
            GrowChild.SetSize( Radius + GrowSize );
            GrowChild.SetWidth( Width + GrowSize * 2 );
            GrowChild.renderer.enabled = renderer.enabled;
        }
    }

    void RecalculateRadius()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        float OutR = Radius / Mathf.Cos( Mathf.PI / N );
        for( int i = 0; i < N; ++i )
        {
            mesh.vertices[2 * i + 1] = mesh.vertices[2 * i + 1].normalized * OutR;
        }
    }
    void RecalculateWidth()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        float InR = (Radius - Width) / Mathf.Cos( Mathf.PI / N );
        for( int i = 0; i < N; ++i )
        {
            if( 2 * i >= mesh.vertexCount )
            {
                Debug.Log( "vertexCount = " + mesh.vertexCount + ", i = " + i );
            }
            else
            {
                mesh.vertices[2 * i] = mesh.vertices[2 * i].normalized * InR;
            }
        }
    }
    public void RecalculatePolygon()
    {
        if( N < 3 ) N = 3;
        if( Width > Radius ) Width = Radius;

        Mesh mesh = new Mesh();
        
        float OutR = Radius / Mathf.Cos( Mathf.PI / N );
        float InR = (Radius - Width) / Mathf.Cos( Mathf.PI / N );
        Matrix4x4 rotateMatrix = Matrix4x4.TRS( Vector3.zero, Quaternion.AngleAxis( 360.0f / N, Vector3.forward ), Vector3.one );

        Vector3 OutVertex = Vector3.up * OutR;
        Vector3 InVertex = Vector3.up * InR;
        Vector3[] vertices = new Vector3[N * 2];
        for( int i = 0; i < N; ++i )
        {
            vertices[2 * i] = InVertex;
            vertices[2 * i + 1] = OutVertex;
            InVertex = rotateMatrix * InVertex;
            OutVertex = rotateMatrix * OutVertex;
        }
        mesh.vertices = vertices;

        Vector2[] uvs = new Vector2[2 * N];
        for( int i = 0; i < N; ++i )
        {
            if( i % 2 == 0 )
            {
                uvs[2 * i] = new Vector2( 0, 0 );
                uvs[2 * i + 1] = new Vector2( 0, 1 );
            }
            else
            {
                uvs[2 * i] = new Vector2( 1, 0 );
                uvs[2 * i + 1] = new Vector2( 1, 1 );
            }
        }
        mesh.uv = uvs;

        int[] quadIndices = new int[] { 0, 2, 1, 3, 1, 2 };
        int[] indices = new int[6 * N];
        for( int i = 0; i < N; ++i )
        {
            for( int j = 0; j < 6; ++j )
            {
                indices[6 * i + j] = (2 * i + quadIndices[j]) % vertices.Length;
            }
        }
        mesh.SetIndices( indices, MeshTopology.Triangles, 0 );

        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void SetTargetSize( float newTargetSize )
    {
        targetRadius = newTargetSize;
    }
    public void SetTargetWidth( float newTargetWidth )
    {
        targetWidth = newTargetWidth;
    }
    public void SetTargetColor( Color newTargetColor )
    {
        targetColor = newTargetColor;
    }

    public void SetSize( float newSize )
    {
        SetTargetSize( newSize );
        Radius = targetRadius;
        RecalculateRadius();
    }
    public void SetWidth( float newWidth )
    {
        SetTargetWidth( newWidth );
        Width = targetWidth;
        RecalculateWidth();
    }
    public void SetColor( Color newTargetColor )
    {
        SetTargetColor( newTargetColor );
        Color = targetColor;
        renderer.material.color = Color;
    }

    public void SetLinearFactor( float factor )
    {
        linearFactor = factor;
    }
    
#if UNITY_EDITOR
    void OnGUI()
    {
        RecalculatePolygon();
    }
    void OnRenderObject()
    {
        if( UnityEditor.EditorApplication.isPlaying ) return;
        RecalculatePolygon();
    }
#endif
}
