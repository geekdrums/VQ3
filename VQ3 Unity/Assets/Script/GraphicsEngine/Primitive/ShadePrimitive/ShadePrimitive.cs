using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ShadePrimitive : MeshComponentBase, IColoredObject
{
	public LightForShadePrimitive Light;

	public int Num = 1000;
	public float Radius = 2000;
	public Color Color = Color.grey;
	public float Alpha = 0.5f;
	public int MipLevel = 2;
	public float UpdateThreashold = 5.0f;

	#region params

	class VertexInfo
	{
		public Vector2 Normal;
		public Vector3 Outer;
		public Vector3 Inner;
		public bool HitFlag;
		public int VertexIndex;
		public bool IsVertexAdded { get { return VertexIndex >= 0; } }
	}
	VertexInfo[] vertexInfos_;

	List<Vector3> meshVertices_ = new List<Vector3>();
	List<int> vertexIndices_ = new List<int>();
	List<Vector2> vertexUVs_ = new List<Vector2>();
	List<Color> vertexColors_ = new List<Color>();
	
	SpriteRenderer spriteRenderer_;

	class TextureAlphaIndexer
	{
		public int Width { get { return width_; } }
		public int Height { get { return height_; } }
		public Vector2 Center { get { return new Vector2(width_ / 2, height_ / 2); } }

		Texture2D texture_;
		int width_;
		int originalWidth_;
		int height_;
		int originalHeight_;
		bool[][] map_;

		public TextureAlphaIndexer(Texture2D texture, int mipLevel)
		{
			texture_ = texture;
			originalWidth_ = texture.width;
			originalHeight_ = texture.height;
			Color32[] colors = texture.GetPixels32(mipLevel);
			width_ = Math.Max(1, texture.width >> mipLevel);
			height_ = Math.Max(1, texture.height >> mipLevel);

			map_ = new bool[height_][];
			for( int y = 0; y < height_; ++y )
			{
				map_[y] = new bool[width_];
				for( int x = 0; x < width_; ++x )
				{
					map_[y][x] = colors[y * width_ + x].a > 0;
				}
			}
		}

		public bool Get(int x, int y)
		{
			if( 0 <= y && y < map_.Length )
			{
				if( 0 <= x && x < map_[y].Length )
				{
					return map_[y][x];
				}
			}
			return false;
		}

		public bool Hit(int startX, int startY, int endX, int endY, out Vector2 hit)
		{
			hit.x = 0;
			hit.y = 0;

			startX = (startX < 0 ? 0 : (width_ <= startX ? width_ - 1 : startX));
			startY = (startY < 0 ? 0 : (height_ <= startY ? height_ - 1 : startY));
			
			endX = (endX < 0 ? 0 : (width_ <= endX ? width_ - 1 : endX));
			endY = (endY < 0 ? 0 : (height_ <= endY ? height_ - 1 : endY));
			
			if( startX == endX /* 縦直線の場合 */ )
			{
				int signY = Math.Sign(endY - startY);
				for( int y = startY; y != endY; y += signY )
				{
					if( map_[y][startX] )
					{
						hit.x = startX;
						hit.y = y;
						return true;
					}
				}

				return false;
			}
			else if( startY == endY /* 横直線の場合 */ )
			{
				int signX = Math.Sign(endX - startX);
				for( int x = startX; x != endX; x += signX )
				{
					if( map_[startY][x] )
					{
						hit.x = x;
						hit.y = startY;
						return true;
					}
				}

				return false;
			}
			else
			{
				int diffX = endX - startX;
				int diffY = endY - startY;
				int signX = Math.Sign(diffX);
				int signY = Math.Sign(diffY);
				diffX *= signX;// abs
				diffY *= signY;// abs
				if( diffX == diffY /* 斜め直線の場合 */ )
				{
					int x = startX;
					int y = startY;
					for( ; x != endX; x += signX, y += signY )
					{
						if( map_[y][x] )
						{
							hit.x = x;
							hit.y = y;
							return true;
						}
					}

					return false;
				}
				else if( diffX > diffY /* 横方向の方が長い移動の場合 */ )
				{
					float y = startY;
					float yStep = signY * (float)diffY / diffX;
					// x方向には1ずつ進む
					for( int x = startX; x != endX; x += signX )
					{
						if( map_[(int)y][x] )
						{
							hit.x = x;
							hit.y = (int)y;
							return true;
						}
						y += yStep;
					}

					return false;
				}
				else //if( diffX < diffY /* 縦方向の方が長い移動の場合 */ )
				{
					float x = startX;
					float xStep = signX * (float)diffX / diffY;
					// y方向には1ずつ進む
					for( int y = startY; y != endY; y += signY )
					{
						if( map_[y][(int)x] )
						{
							hit.x = (int)x;
							hit.y = y;
							return true;
						}
						x += xStep;
					}

					return false;
				}
			}
		}
	}
	TextureAlphaIndexer texture_;

	Vector2 lightPositionCache_;
	int vertexCount_;
	Rect worldSpriteRect_;
	Rect textureUVRect_;
	Vector2 spriteRectCenter_;
	Vector2 textureCenter_;
	Vector2 leftTop_;
	Vector2 leftBottom_;
	Vector2 rightTop_;
	Vector2 rightBottom_;
	float worldToTexture_;
	float textureToWorld_;

	#endregion


	#region unity functions
	
	protected override void OnValidate()
	{
		if( spriteRenderer_ == null )
		{
			spriteRenderer_ = transform.parent.GetComponent<SpriteRenderer>();
		}

		if( Light != null )
		{
			Light.AddTarget(this);
		}

		base.OnValidate();
	}

	void Update()
	{
		CheckLightPosition();
	}

	public void CheckLightPosition()
	{
		Vector2 lightPosition = Light.transform.position - transform.position;

		if( Vector2.Distance(lightPosition, lightPositionCache_) > UpdateThreashold )
		{
			RecalculatePolygon();
		}
	}

	#endregion


	#region vertex culculate
	
	void UpdateCachedTextureParam()
	{
		if( MipLevel >= spriteRenderer_.sprite.texture.mipmapCount )
		{
			MipLevel = spriteRenderer_.sprite.texture.mipmapCount - 1;
		}
		texture_ = new TextureAlphaIndexer(spriteRenderer_.sprite.texture, MipLevel);

		// テクスチャが表示されているRect情報を集める
		float unitPerPixel = 1.0f / spriteRenderer_.sprite.pixelsPerUnit;
		worldSpriteRect_ = new Rect(0, 0, spriteRenderer_.sprite.texture.width * unitPerPixel, spriteRenderer_.sprite.texture.height * unitPerPixel);
		spriteRectCenter_ = worldSpriteRect_.size / 2;

		worldToTexture_ = texture_.Width / worldSpriteRect_.width;
		textureToWorld_ = 1.0f / worldToTexture_;

		textureCenter_ = texture_.Center;
		textureUVRect_ = new Rect(0, 0, texture_.Width - 1, texture_.Height - 1);
		leftTop_ = new Vector2(0, texture_.Height - 1);
		leftBottom_ = new Vector2(0, 0);
		rightTop_ = new Vector2(texture_.Width - 1, texture_.Height - 1);
		rightBottom_ = new Vector2(texture_.Width - 1, 0);
	}

	public override void RecalculatePolygon(bool forceReflesh = false)
	{
		base.RecalculatePolygon();

		if( Light == null )
		{
			return;
		}

		if( spriteRenderer_ == null )
		{
			spriteRenderer_ = transform.parent.GetComponent<SpriteRenderer>();
		}
		
		Vector2 lightPosition = Light.transform.position - transform.position;
		lightPositionCache_ = lightPosition;

		if( Num < 30 )
		{
			Num = 30;
		}

		if( texture_ == null || forceReflesh )
		{
			UpdateCachedTextureParam();
		}

		if( forceReflesh || vertexInfos_ == null || vertexInfos_.Length != Num )
		{
			InitVertexInfos(lightPosition);
		}

		CalculateVertexInfos(lightPosition, forceReflesh);
		
		Color innerColor = ColorPropertyUtil.MakeAlpha(Color, Alpha);
		Color outerColor = Color;

		// 面を構成
		vertexCount_ = 0;
		int faceCount = 0;
		for( int i = 0; i < vertexInfos_.Length; ++i )
		{
			// 隣のEdgeが揃っていて初めて面になる
			int iNext = (i + 1) % vertexInfos_.Length;
			if( vertexInfos_[i].HitFlag && vertexInfos_[iNext].HitFlag )
			{
				if( vertexInfos_[i].IsVertexAdded == false )
				{
					SetVert(ref vertexCount_, i, innerColor, outerColor);
				}
				if( vertexInfos_[iNext].IsVertexAdded == false )
				{
					SetVert(ref vertexCount_, iNext, innerColor, outerColor);
				}

				// cf. int[] QuadIndices = new int[] { 0, 2, 1, 3, 1, 2 };
				int vertIndices = faceCount * 6;
				if( vertexIndices_.Count < vertIndices + 6 )
				{
					vertexIndices_.Add(vertexInfos_[i].VertexIndex);
					vertexIndices_.Add(vertexInfos_[iNext].VertexIndex);
					vertexIndices_.Add(vertexInfos_[i].VertexIndex + 1);
					vertexIndices_.Add(vertexInfos_[iNext].VertexIndex + 1);
					vertexIndices_.Add(vertexInfos_[i].VertexIndex + 1);
					vertexIndices_.Add(vertexInfos_[iNext].VertexIndex);
				}
				else
				{
					vertexIndices_[vertIndices + 0] = vertexInfos_[i].VertexIndex;
					vertexIndices_[vertIndices + 1] = vertexInfos_[iNext].VertexIndex;
					vertexIndices_[vertIndices + 2] = vertexInfos_[i].VertexIndex + 1;
					vertexIndices_[vertIndices + 3] = vertexInfos_[iNext].VertexIndex + 1;
					vertexIndices_[vertIndices + 4] = vertexInfos_[i].VertexIndex + 1;
					vertexIndices_[vertIndices + 5] = vertexInfos_[iNext].VertexIndex;
				}
				++faceCount;
			}
		}

		// リストの余った部分を削る
		if( meshVertices_.Count > vertexCount_ )
		{
			meshVertices_.RemoveRange(vertexCount_, meshVertices_.Count - vertexCount_);
		}
		if( vertexUVs_.Count > vertexCount_ )
		{
			vertexUVs_.RemoveRange(vertexCount_, vertexUVs_.Count - vertexCount_);
		}
		if( vertexColors_.Count > vertexCount_ )
		{
			vertexColors_.RemoveRange(vertexCount_, vertexColors_.Count - vertexCount_);
		}
		int vertIndicesCount = faceCount * 6;
		if( vertexIndices_.Count > vertIndicesCount )
		{
			vertexIndices_.RemoveRange(vertIndicesCount, vertexIndices_.Count - vertIndicesCount);
		}

		PopulateMesh();

		MeshDirty = false;
	}

	void SetVert(ref int vertexCount, int i, Color innerColor, Color outerColor)
	{
		vertexInfos_[i].VertexIndex = vertexCount;
		if( meshVertices_.Count < vertexCount + 1 )
		{
			meshVertices_.Add(vertexInfos_[i].Inner);
			meshVertices_.Add(vertexInfos_[i].Outer);
		}
		else
		{
			meshVertices_[vertexCount] = vertexInfos_[i].Inner;
			meshVertices_[vertexCount + 1] = vertexInfos_[i].Outer;
		}

		if( vertexColors_.Count < vertexCount + 1 )
		{
			vertexColors_.Add(innerColor);
			vertexColors_.Add(outerColor);
		}
		else
		{
			vertexColors_[vertexCount] = innerColor;
			vertexColors_[vertexCount + 1] = outerColor;
		}

		if( vertexUVs_.Count < vertexCount + 1 )
		{
			if( (vertexCount / 2) % 2 == 0 )
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

		vertexCount += 2;
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
	}

	void InitVertexInfos(Vector2 lightPosition)
	{
		vertexInfos_ = new VertexInfo[Num];
		Matrix4x4 rotateMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis((360.0f / Num), Vector3.forward), Vector3.one);
		Vector2 normal = Vector3.up;
		for( int n = 0; n < Num; ++n )
		{
			vertexInfos_[n] = new VertexInfo();
			vertexInfos_[n].Normal = normal;
			vertexInfos_[n].HitFlag = false;
			vertexInfos_[n].VertexIndex = -1;

			normal = rotateMatrix * normal;
		}
	}
	
	void CalculateVertexInfos(Vector2 lightPosition, bool forceReflesh = false)
	{
		int startN;
		int endN;

		// textureのUV座標系に変換したlightPosition
		Vector2 lightPositionUV = lightPosition * worldToTexture_ + textureCenter_;

		Vector2[] intersects = new Vector2[2];
		bool rectContainsLightPosition = false;
		if( textureUVRect_.Contains(lightPositionUV) )
		{
			// 内部に光源があるなら、交差点の一つは光源自身で決まり
			rectContainsLightPosition = true;
			intersects[0] = lightPositionUV;

			startN = 0;
			endN = Num;
		}
		else
		{
			// 内部に光源がなければ、N方向すべてを探索する必要はないので、中心点の角度から左右に順番に走査していく
			startN = (int)(Num * (((Vector2.SignedAngle(Vector2.up, -lightPosition)/*[-180, 180]*/ / 360.0f/*[-0.5, 0.5]*/) + 1.0f) % 1.0f/*[0.0, 1.0]*/));
			endN = (startN + Num / 2) % Num;
		}

		//reset
		for( int i = 0; i < Num; ++i )
		{
			vertexInfos_[i].HitFlag = false;
			vertexInfos_[i].VertexIndex = -1;
		}

		// cache
		float endRadius = lightPosition.magnitude + Radius;
		float endRadiusUV = endRadius * worldToTexture_;

		int n = startN;
		int sign = 1;
		while( n != endN )
		{
			int numIntersects = 0;
			if( rectContainsLightPosition )
			{
				++numIntersects;
			}

			Vector2 lightEndUV = lightPositionUV + vertexInfos_[n].Normal * endRadiusUV;
			float u, v;
			float minV = (rectContainsLightPosition ? 0.0f : 1.0f);
			int lightSideIndex = 0;
			// 上辺判定
			if( numIntersects < 2 && IntersectLine(leftTop_, rightTop_, lightPositionUV, lightEndUV, out intersects[numIntersects], out u, out v) )
			{
				if( v < minV )
				{
					lightSideIndex = numIntersects;
					minV = v;
				}
				++numIntersects;
			}
			// 下辺判定
			if( numIntersects < 2 && IntersectLine(leftBottom_, rightBottom_, lightPositionUV, lightEndUV, out intersects[numIntersects], out u, out v) )
			{
				if( v < minV )
				{
					lightSideIndex = numIntersects;
					minV = v;
				}
				++numIntersects;
			}
			// 左辺判定
			if( numIntersects < 2 && IntersectLine(leftTop_, leftBottom_, lightPositionUV, lightEndUV, out intersects[numIntersects], out u, out v) )
			{
				if( v < minV )
				{
					lightSideIndex = numIntersects;
					minV = v;
				}
				++numIntersects;
			}
			// 右辺判定
			if( numIntersects < 2 && IntersectLine(rightTop_, rightBottom_, lightPositionUV, lightEndUV, out intersects[numIntersects], out u, out v) )
			{
				if( v < minV )
				{
					lightSideIndex = numIntersects;
					minV = v;
				}
				++numIntersects;
			}

			// 交わる2点が判明していればそこからテクスチャを探索する
			if( numIntersects == 2 )
			{
				Vector2 lightEdgeUV = intersects[lightSideIndex];
				Vector2 shadeEdgeUV = intersects[(lightSideIndex + 1) % 2];
				Vector2 hitPositionUV;
				if( texture_.Hit((int)lightEdgeUV.x, (int)lightEdgeUV.y, (int)shadeEdgeUV.x, (int)shadeEdgeUV.y, out hitPositionUV) )
				{
					vertexInfos_[n].HitFlag = true;
					vertexInfos_[n].Inner = hitPositionUV * textureToWorld_ - spriteRectCenter_;
					vertexInfos_[n].Outer = lightPosition + vertexInfos_[n].Normal * endRadius;
				}
			}
			else
			{
				if( rectContainsLightPosition )
				{
					// Radiusが短すぎたりしない限りはありえない。いずれにしても一周するまで判定する。
					break;
				}
				else if( sign == 1 )
				{
					// 逆周りで探索
					sign = -1;
					n = startN;
				}
				else
				{
					// 両方探索し終えた
					break;
				}
			}

			n += sign;
			if( rectContainsLightPosition )
			{
				// n == Numに来たら終わらせるので放っておく
			}
			else
			{
				// 循環させる
				if( n == Num ) n = 0;
				else if( n == -1 ) n = Num - 1;
			}
		} // while
	}
	
	public static bool IntersectLine(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2, out Vector2 intersection, out float u, out float v)
	{
		/* https://setchi.hatenablog.com/entry/2017/07/12/202756 */
		var d = (end1.x - start1.x) * (end2.y - start2.y) - (end1.y - start1.y) * (end2.x - start2.x);

		if( d == 0.0f )
		{
			v = 0;
			u = 0;
			intersection.x = 0;
			intersection.y = 0;
			return false;
		}

		u = ((start2.x - start1.x) * (end2.y - start2.y) - (start2.y - start1.y) * (end2.x - start2.x)) / d;
		v = ((start2.x - start1.x) * (end1.y - start1.y) - (start2.y - start1.y) * (end1.x - start1.x)) / d;

		if( u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f )
		{
			intersection.x = 0;
			intersection.y = 0;
			return false;
		}

		intersection.x = start1.x + u * (end1.x - start1.x);
		intersection.y = start1.y + u * (end1.y - start1.y);

		return true;
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
		Color innerColor = ColorPropertyUtil.MakeAlpha(Color, Alpha);
		Color outerColor = Color;
		for( int i = 0; i < vertexCount_; ++i )
		{
			vertexColors_[i] = (i % 2 == 0 ? innerColor : outerColor);
		}

		Mesh mesh = MeshInstance;
		mesh.SetColors(vertexColors_);
	}

	#endregion

}
