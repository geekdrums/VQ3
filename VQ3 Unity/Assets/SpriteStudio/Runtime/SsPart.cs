/**
	SpriteStudioPlayer
	
	A part of sprite
	
	Copyright(C) 2003-2013 Web Technology Corp. 
	
*/

// apply X,Y position of root part as pivot.
#define	_APPLY_ROOT_POS_AS_PIVOT
//#define	_MAKE_ROOT_TO_LOCAL_TRANSFORM
//#define	_USE_UNIFIED_SHADER

// currently not public 2012.3.27
//#define _USE_TRIANGLE_STRIP

//#define _BOUND_PART_DRAW_AS_INVALID
//#define _MOVE_BOUND_PART_TO_THE_FRONT

// inherits hide status when forced visible is available.
#define _INHERITS_FORCE_VISIBLE
	
using UnityEngine;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class SsPart : IComparable<SsPart>
{
	//--------- public
	static public bool operator true(SsPart p){return p != null;}
	static public bool operator false(SsPart p){return p == null;}

	// Type of delegate to be called when the current frame gets the key frame.
	public	delegate void KeyframeCallback(SsPart part, SsAttrValueInterface value);

	// Event handler to be called when the current frame gets the UserData key frame.
	public event	KeyframeCallback	OnUserDataKey;

	// Event handler to be called when the current frame gets the Sound key frame.
	public event	KeyframeCallback	OnSoundKey;

	internal	bool ExistsOnUserDataKey {get {return OnUserDataKey != null;}}
	internal	bool ExistsOnSoundKey {get {return OnSoundKey != null;}}
	
	public	SsSprite	Sprite {get {return _mgr;}}
	
	//--------- references
	internal	SsSprite	_mgr;
	internal	SsPartRes	_res;
	internal	SsPart		_parent;

	//--------- statuses
	internal	int			_priority;	///< draw order priority. earlier -128 <= +127 later
	internal	bool		_visible;
	
	bool _forceVisibleAvailable = false;
	bool _forceVisible = true;

	public void
	ForceShow(bool v)
	{
		_forceVisibleAvailable = true;
		_forceVisible = v;
		if (_triIndices != null)
			SetToSubmeshArray(_subMeshIndex);
	}

	public void
	ResetForceShow()
	{
		_forceVisibleAvailable = false;
		if (_triIndices != null)
			SetToSubmeshArray(_subMeshIndex);
	}

	bool _forceAlphaAvailable = false;
	float _forceAlpha = 1f;

	public void
	ForceAlpha(float v)
	{
		_forceAlphaAvailable = true;
		_forceAlpha = v;
	}

	public void
	ResetForceAlpha()
	{
		_forceAlphaAvailable = false;
	}	
	internal	Vector3		_pos;			///< local position
	internal	Quaternion	_quaternion;	///< local rotation
	internal	Vector3		_scale;			///< local scale
#if _MAKE_ROOT_TO_LOCAL_TRANSFORM
	internal	Quaternion	_rootSpaceQuaternion;	///< root space rotation
	internal	bool		_rotChanged;
#endif
	internal	Transform 	_transform;		///< my transform. created by user when needed.

	//--------- settings
 	internal	Material	_material;
	public		Material	GetMaterial()
	{
		return _material;
	}

	//--------- works
	internal	Mesh		_mesh;
	internal	Matrix4x4	_pivotMatrix;
	
	// Matrix that transforms a point from root part space into this part's local space (Read Only).
	public	Matrix4x4		RootToPartMatrix {
		get {return _pivotMatrix;}
	}
	Vector3[]				_orgVertices;
	
	internal Vector3[]		_vertPositions;
	int[]		_triIndices;
	int			_index;		///< index of parts
	int			_vIndex;	///< actual index in vertex/color/uv buffers.

#if _APPLY_ROOT_POS_AS_PIVOT
	Vector3		_rootPivot;
#endif
	
	int 		_subMeshIndex;
	
	// UV offset
	int			_imgOfsX;
	int			_imgOfsY;
	int			_imgOfsW;
	int			_imgOfsH;
	
	// origin offset
	Vector3		_originOffset = Vector3.zero;

	bool		_hasTransparency;
	internal	bool	_flipH, _flipV;

	static int[,] _flippedUvIndices = {
		{1,0,3,2},	// H flipped
		{3,2,1,0},	// V flipped
		{2,3,0,1},	// HV flipped
	};
	SsColorBlendKeyValue _colorBlendKeyValue;
	
	Material 		_originalMaterial;
	Material 		_individualMaterial;

	Color			_materialColor;
	Color			_vertexColor;
	float			_alpha;
	bool 			_useCgShader = false;
#if _USE_UNIFIED_SHADER
	bool 			_useUnifiedShader = false;
#endif
	
	SsShaderType					_shaderType;
	SsColorBlendOperation			_colorBlendType;

	SsColorBlendOperation	ColorBlendType
	{
		get {return _colorBlendType;}
		set {
			_colorBlendType = value;
			_shaderType = SsShaderManager.EnumToType(value, _res.AlphaBlendType, SsMaterialColorBlendOperation.Non);
#if _USE_UNIFIED_SHADER
			if (_useUnifiedShader)
			{
				// must set value to the all vertices to allow vertex shader to get the value.
				for (int i = 0; i < 4; ++i)
					_mgr._extras[_vIndex + i][0] = (float)_shaderType;
				_mgr._extraChanged = true;
			}
			else 
#endif
			{
				_material = _res.imageFile.GetMaterial(_shaderType);
				_mgr._materials[_subMeshIndex] = _material;
				_mgr._matChanged = true;
			}
		}
	}
	float			AlphaValue
	{
		get {return _alpha;}
		set {
			// must set value to the all vertices to allow vertex shader to get the value.
			_alpha = value;
			if (_useCgShader && ColorBlendType != SsColorBlendOperation.Non)
			{
				for (int i = 0; i < 4; ++i)
					_mgr._extras[_vIndex + i][1] = value;
				_mgr._extraChanged = true;
			}
			else
			{
				// set value to vertex color alpha.
				for (int i = 0; i < 4; ++i)
					_mgr._colors[_vIndex + i].a = value;
				_mgr._colorChanged = true;
			}
		}
	}

	public int
	CompareTo(SsPart other)
	{
		if (_priority == other._priority)
			return _index.CompareTo(other._index);
		return _priority.CompareTo(other._priority);
	}

	internal
	SsPart(
		SsSprite	manager,
		int			index,		///< index of parts
		SsPartRes	partRes,	///< part resource
		SsImageFile	imageFile)	///< source image path, texture and material.
	{
		_mgr			= manager;
		_index			= index;
		_vIndex			= (index - 1) * 4;
		_subMeshIndex	= index - 1;
		_res			= partRes;
		_mesh			= _mgr._mesh;

		if (_res.HasParent)
		{
			_parent = _mgr.Sprite(_res.ParentId);
			if (_parent == null)
			{
				Debug.LogError("##### parent sprite must be created already!!");
				//throw ArgumentNullException;
				Debug.Break();
				return;
			}
		}

		// attach parent's transform to inherit its SRT.
		_pivotMatrix = Matrix4x4.identity;
		switch (_res.Type)
		{
		case SsPartType.Root:
			// root has only position, no vertices
#if _APPLY_ROOT_POS_AS_PIVOT
			_rootPivot = new Vector3(-_res.PosX(0), +_res.PosY(0), 0f);
#endif
			break;
		case SsPartType.Normal:
		case SsPartType.Bound:
			// each vertices are attached to pivot
			_vertPositions = new Vector3[4];
			_orgVertices = new Vector3[4];
			for (int i = 0; i < _vertPositions.Length; ++i)
			{
				_orgVertices[i] = _vertPositions[i] = _res.OrgVertices[i];
#if _MOVE_BOUND_PART_TO_THE_FRONT
				if (_res.Type == SsPartType.Bound)
				{
					_vertPositions[i].z = -0.1f;
					_orgVertices[i].z = -0.1f;
				}
#endif
			}
			break;
		default:
			// other types don't require vertices.
			break;
		}

		// set startup value
		_visible	= !_res.Hide(0);
		_flipH		= false;
		_flipV		= false;
		_pos		= Vector3.zero;
		_quaternion	= Quaternion.identity;
		_scale		= Vector3.one;
#if _MAKE_ROOT_TO_LOCAL_TRANSFORM
		_rotChanged	= false;
#endif
		// not any normal types don't require a material, colors, and vertices.
		if (_res.Type != SsPartType.Normal
		&&	_res.Type != SsPartType.Bound)
			return;
		
		// shortcut
#if _USE_UNIFIED_SHADER
		_useUnifiedShader = _res.imageFile.useUnifiedShader;
#endif
		_useCgShader = (SystemInfo.graphicsShaderLevel >= 20);

		if (_res.Type == SsPartType.Bound)
		{
#if !_BOUND_PART_DRAW_AS_INVALID
			// set vertex color to transparent red.
			// this alpha value will be overwritten in AlphaValue property later.
			_vertexColor = new Color(1,0,0,1);
			for (int i = 0; i < 4; ++i)
			{
				// set UVs to a point at left-top.
				_mgr._uvs[_vIndex + i] = Vector2.zero;
		
				// set vertex colors
				_mgr._colors[_vIndex + i] = _vertexColor;
			}
			// invisible is default.
			_visible = false;
#else
			_visible = _mgr.DrawBoundingParts;
#endif
		}
		else
		{
			// default vertex color
			_vertexColor = new Color(1,1,1,1);
			for (int i = 0; i < 4; ++i)
			{
				// set UVs. use precalculated UVs, it is stored clockwise
				_mgr._uvs[_vIndex + i] = _res.UVs[i];
		
				// set vertex colors
				_mgr._colors[_vIndex + i] = _vertexColor;
			}
			// set blend type and _shaderType
			ColorBlendType = SsColorBlendOperation.Non;
		}
		
		// set boolean about having transparency
		if (_res.Type == SsPartType.Bound)
		{
#if _BOUND_PART_DRAW_AS_INVALID
			// become purple that mean invalid.
			_material = null;
#else
			// needs any appropriate material
			_hasTransparency = true;
			AlphaValue = 0.5f;
#endif
		}
		else
		{
			_hasTransparency = _res.HasTrancparency
				|| (_parent != null && _res.Inherits(SsKeyAttr.Trans));// always inherits parent's alpha whether the immediate parent has transparency or not. 2012.12.19 bug fixed
			// set alpha value
			AlphaValue = _res.Trans(0);

			// set appropriate material. _shaderType was set inside ColorBlendType property.
			_material = imageFile.GetMaterial(_shaderType);
		}
				
		//--------- calculates various info...
		Update(true);

		// set triangle indices. never changed so far.
#if _USE_TRIANGLE_STRIP
		_triIndices = new int[]{_vIndex+0,_vIndex+1,_vIndex+3,_vIndex+2};// order is LT->RT->LB->RB.
#else
		_triIndices = new int[]{_vIndex+0,_vIndex+1,_vIndex+2,_vIndex+2,_vIndex+3,_vIndex+0};	// order is LT->RT->RB->RB->LB->LT
#endif
		
		SetToSubmeshArray(_index - 1);
	}
	
	internal void
	SetToSubmeshArray(int index)
	{
#if _INHERITS_FORCE_VISIBLE
		bool v = _visible;
#else
		bool v = _forceVisibleAvailable ? _forceVisible : _visible;
#endif
		// if _triIndices is null, draw nothing.
#if _USE_TRIANGLE_STRIP
		_mesh.SetTriangleStrip(v ? _triIndices : null, index);
#else
		_mesh.SetTriangles(v ? _triIndices : null, index);
#endif
		_subMeshIndex = index;
	}
	
	internal void
	Show(bool v)
	{
		_visible = v;

#if _INHERITS_FORCE_VISIBLE
#else
		if (_forceVisibleAvailable)
			v = _forceVisible;
#endif
		
#if _USE_TRIANGLE_STRIP
		_mesh.SetTriangleStrip(v ? _triIndices : null, _subMeshIndex);
#else
		_mesh.SetTriangles(v ? _triIndices : null, _subMeshIndex);
#endif
	}
	
	internal void
	Update(bool initialize = false)
	{
		UpdateSub(_res, (int)_mgr.AnimFrame, initialize);

		if (_subAnimes != null && _subAnimes.Count > 0)
		{
			// apply sub animations
			int i = 0;
			foreach (var e in _subAnimes)
			{
				UpdateSub(_subAnimePartRes[i], (int)e.Frame);
				++i;
			}

			_subAnimes.Clear();
			_subAnimePartRes.Clear();
		}
	}

	List<SsSubAnimeController>	_subAnimes = new List<SsSubAnimeController>();
	List<SsPartRes>				_subAnimePartRes = new List<SsPartRes>();

	internal void
	AddSubAnime(SsSubAnimeController subAnime, SsPartRes subAnimePartRes)
	{
		if (!subAnimePartRes.HasAttrFlags(SsKeyAttrFlags.AllMask)) return;
		_subAnimes.Add(subAnime);
		_subAnimePartRes.Add(subAnimePartRes);
	}
		
	internal void
	UpdateSub(SsPartRes res, int frame, bool initialize = false)
	{
		// priority 
		if (res.HasAttrFlags(SsKeyAttrFlags.Prio))
		{
			int nowPrio = (int)res.Prio(frame);
			if (_priority != nowPrio)
			{
				_priority = nowPrio;
				_mgr._prioChanged = true;
			}
		}
		
		// visibility
		if (res.HasAttrFlags(SsKeyAttrFlags.Hide))
		{
			if (res.IsRoot)
			{
				_visible = !res.Hide(frame);
			}
			else
			if (res.Type == SsPartType.Normal)
			{
				bool nowVisible;
				if (res.IsBeforeFirstKey(frame))
					nowVisible = false;
				else
				{
					if	(_parent != null
					&&	!_parent._res.IsRoot
					&&	(res.InheritRate(SsKeyAttr.Hide) > 0.5f))
						nowVisible = _parent._visible;
					else
						nowVisible = !res.Hide(frame);
				}
#if _INHERITS_FORCE_VISIBLE
				if (_forceVisibleAvailable)
					nowVisible = _forceVisible;
#endif
				if (nowVisible != _visible)
					Show(nowVisible);
			}
		}

		// vertex color
		if (res.HasAttrFlags(SsKeyAttrFlags.PartsCol))
		{
			SsColorBlendKeyValue cbk = res.PartsCol(frame);
			SsColorBlendOperation cbkOp = ColorBlendType;
			if (cbk == null)
			{
				if (_colorBlendKeyValue != null)
				{
					// set back default color
					cbkOp = SsColorBlendOperation.Non;
					for (int i = 0; i < 4; ++i)
						_mgr._colors[_vIndex + i] = _vertexColor;
					_mgr._colorChanged = true;
				}
			}
			else
			{
				cbkOp = cbk.Operation;
				if (cbk.Target == SsColorBlendTarget.Vertex)
				{
					// vertex colors
					for (int i = 0; i < 4; ++i)
						_mgr._colors[_vIndex + i] = GetBlendedColor(cbk.Colors[i], cbk.Operation);
				}
				else
				{
					// affect a color to overall, so it doesn't inidicate that this is not vertex color.
					Color c = GetBlendedColor(cbk.Colors[0], cbk.Operation);
					for (int i = 0; i < 4; ++i)
						_mgr._colors[_vIndex + i] = c;
				}
				_mgr._colorChanged = true;
			}
			_colorBlendKeyValue = cbk;

			if (_mgr._colorChanged)
			{
				if (cbkOp != ColorBlendType)
				{
					// change other shader
					ColorBlendType = cbkOp;
					// place stored alpha is variable with color blend type. where is simply in color.a if blend is none.
					AlphaValue = AlphaValue;
				}
			}
		}
		
		// transparency
		if (_hasTransparency)
		{
			float nowAlpha = res.Trans(frame);
			if (_parent != null && res.Inherits(SsKeyAttr.Trans))
			{
				float parentAlpha;
				// if parent is root, it doesn't have material.
				if (_parent._material == null)
					parentAlpha = _parent._res.Trans(frame);
				else
					parentAlpha = _parent.AlphaValue;
				// just multiply simply 
				nowAlpha = parentAlpha * nowAlpha;
			}
			if (_forceAlphaAvailable)
				nowAlpha = _forceAlpha;
			if (nowAlpha != AlphaValue)
				AlphaValue = nowAlpha;
		}

		// scale
		if (res.HasAttrFlags(SsKeyAttrFlags.Scale))
		{
			var scale = new Vector3(res.ScaleX(frame), res.ScaleY(frame), 1f);
			if (scale != _scale)
			{
				_scale = scale;
				_mgr._vertChanged = true;
			}
		}

		// rotation (now supports only Z axis)
		if (res.HasAttrFlags(SsKeyAttrFlags.Angle))
		{
			var ang = res.Angle(frame);
			// SpriteStudio demands me to Z axis rotation consistently.
			if (_parent)
			{
				// reverse angle direction if parent part's scale is negative value.
				if (_parent._pivotMatrix.m00 * _parent._pivotMatrix.m11 < 0)
					ang *= -1;
				if (_mgr.hFlip ^ _mgr.vFlip) ang *= -1;
			}
			Quaternion rot = Quaternion.Euler(0,0,ang);
			if (rot != _quaternion)
			{
				_quaternion = rot;
				_mgr._vertChanged = true;
#if _MAKE_ROOT_TO_LOCAL_TRANSFORM
				_rotChanged = true;
#endif
			}
#if _MAKE_ROOT_TO_LOCAL_TRANSFORM
			else
				_rotChanged = false;
#endif
		}

		// translate
		if (res.HasAttrFlags(SsKeyAttrFlags.Pos))
		{
			var	pos = new Vector3(res.PosX(frame), -res.PosY(frame));
#if false
			if (_parent != null)
			{
			    if (_parent._flipH)
					pos.x *= -1;
				if (_parent._flipV)
					pos.y *= -1;
			}
#endif
#if _APPLY_ROOT_POS_AS_PIVOT
			// apply X,Y position as pivot if this is root.
			if (res.IsRoot)
				pos += _rootPivot;
#endif
			
#if false
			// update vertices when position is changed.
			if (_pos != pos)
#endif
			{
				_pos = pos;
				_mgr._vertChanged = true;
			}
		}
		
		bool orgVertChanged = false;

		// UV animation
		if (res.HasAttrFlags(SsKeyAttrFlags.ImageOffset))
		{
			int nowImgOfs = res.ImageOffsetX(frame);
			if (nowImgOfs != _imgOfsX)
			{
				_imgOfsX = nowImgOfs;
				_mgr._uvChanged = true;
			}
			nowImgOfs = res.ImageOffsetY(frame);
			if (nowImgOfs != _imgOfsY)
			{
				_imgOfsY = nowImgOfs;
				_mgr._uvChanged = true;
			}
			bool sizeChnaged = false;
			nowImgOfs = res.ImageOffsetW(frame);
			if (nowImgOfs != _imgOfsW)
			{
				_imgOfsW = nowImgOfs;
				_mgr._uvChanged = true;
				sizeChnaged = true;
			}
			nowImgOfs = res.ImageOffsetH(frame);
			if (nowImgOfs != _imgOfsH)
			{
				_imgOfsH = nowImgOfs;
				_mgr._uvChanged = true;
				sizeChnaged = true;
			}
			if (sizeChnaged)
			{
				// modify polygon size
				Vector2 size = res.PicArea.WH();
				size.x += _imgOfsW;
				size.y += _imgOfsH;
				_orgVertices = res.GetVertices(size);
				orgVertChanged = true;
			}
			if (_mgr._uvChanged)
			{
				res.CalcUVs(_imgOfsX, _imgOfsY, _imgOfsW, _imgOfsH);
			}
		}
		
		// origin animation
		if (res.HasAttrFlags(SsKeyAttrFlags.OriginOffset))
		{
			int nowOrgOfsX = -res.OriginOffsetX(frame);
			if (nowOrgOfsX != _originOffset.x)
			{
				_originOffset.x = nowOrgOfsX;
				orgVertChanged = true;
			}
			int nowOrgOfsY = res.OriginOffsetY(frame);
			if (nowOrgOfsY != _originOffset.y)
			{
				_originOffset.y = nowOrgOfsY;
				orgVertChanged = true;
			}
		}

		if (res.HasAttrFlags(SsKeyAttrFlags.Vertex))
			orgVertChanged = true;
		    
		// vertex modification
		if (orgVertChanged
		&&	_vertPositions != null)
		{
			for (int i = 0; i < _vertPositions.Length; ++i)
			{
				_vertPositions[i] = _orgVertices[i];
				if (res.HasAttrFlags(SsKeyAttrFlags.Vertex))
				    _vertPositions[i] += res.Vertex(frame).Vertex3(i);
				if (res.HasAttrFlags(SsKeyAttrFlags.OriginOffset))
					_vertPositions[i] += _originOffset;
			}
			orgVertChanged = false;
			_mgr._vertChanged = true;
		}

		// flip image only. the setting is given from anime resource.
		bool dontFlipCoord = res.IsRoot ? false : _mgr._animation.hvFlipForImageOnly;

		// flip H
		bool nowFlipH = false;
		
		if (res.IsRoot)
			nowFlipH = _mgr.hFlip;
		else
		{
			if (dontFlipCoord)
			{
				if (_parent != null  && res.Inherits(SsKeyAttr.FlipH))
					if (!_parent._res.IsRoot)
						nowFlipH = _parent._flipH;
			}
			if (res.FlipH(frame))
				nowFlipH = !nowFlipH;
		}

		if (!dontFlipCoord)
		{
			if ((nowFlipH && _scale.x > 0f)
			||	(!nowFlipH && _scale.x < 0f))
			{
				_scale.x *= -1;
				_mgr._vertChanged = true;
			}
		}

		// flip V
		bool nowFlipV = false;

		if (res.IsRoot)
			nowFlipV = _mgr.vFlip;
		else
		{
			if (dontFlipCoord)
			{
				if (_parent != null  && res.Inherits(SsKeyAttr.FlipV))
					if (!_parent._res.IsRoot)
						nowFlipV = _parent._flipV;	// 2012.06.27 fixed an issue that nowFlipV refers _parent._flipH
			}
			if (res.FlipV(frame))
				nowFlipV = !nowFlipV;
		}
		if (!dontFlipCoord)
		{
			if ((nowFlipV && _scale.y > 0f)
			||	(!nowFlipV && _scale.y < 0f))
			{
				_scale.y *= -1;
				_mgr._vertChanged = true;
			}
		}

		if (nowFlipH != _flipH
		||	nowFlipV != _flipV)
		{
			_flipH = nowFlipH;
			_flipV = nowFlipV;
			if (dontFlipCoord)
				_mgr._uvChanged = true;
		}

		// udpate uv indices
		if (_mgr._uvChanged
		&&	res.UVs != null	// root part has no UVs
		&&	res.UVs.Length == 4)
		{
			if (dontFlipCoord)
			{
				int index = -1;
				if (nowFlipV) index = 1;
				if (nowFlipH) ++index;
				for (int i = 0; i < 4; ++i)
					_mgr._uvs[_vIndex + i] = res.UVs[ index >= 0 ? _flippedUvIndices[index, i] : i ];
			}
			else
			{
				for (int i = 0; i < 4; ++i)
					_mgr._uvs[_vIndex + i] = res.UVs[i];
			}
		}

		// update vertex buffer
		if (_mgr._vertChanged)
		{
			// udpate matrix
			var p = _pos;
			var s = _scale;
			if (_parent)
			{
				// previously apply compensated value if this doesn't want to inherit parent's value.
				if (!res.Inherits(SsKeyAttr.ScaleX))
				{
					s.x /= _parent._scale.x;
					p.x /= _parent._scale.x;
				}
				if (!res.Inherits(SsKeyAttr.ScaleY))
				{
					s.y /= _parent._scale.y;
					p.y /= _parent._scale.y;
				}
			}
			_pivotMatrix.SetTRS(p, _quaternion, s);
			
			// multiply parent's
			if (_parent)
				_pivotMatrix = _parent._pivotMatrix * _pivotMatrix;
			
			if (_vertPositions != null)
			{
				// apply matrix to vertices
				for (int i = 0; i < _vertPositions.Length; ++i)
				{
					Vector3 v = _pivotMatrix.MultiplyPoint3x4(_vertPositions[i]);
					_mgr._vertices[_vIndex + i] = v;
				}
	#if false
				if (_drawPartsRect)
				{
					// get rectangle from bounding box.
					Vector3 lt = _vertTransforms[0].position;
					Vector3 rb = _vertTransforms[2].position;
					Vector3 rt = lt;
					rt.x = rb.x;
					Vector3 lb = lt;
					lb.y = rb.y;
					// draw rectangle
					Debug.DrawLine(lt, rt, Color.red);
					Debug.DrawLine(rt, rb, Color.red);
					Debug.DrawLine(rb, lb, Color.red);
					Debug.DrawLine(lb, lt, Color.red);
				}
	#endif
			}
		}
		
		if (_transform)
		{
#if _MAKE_ROOT_TO_LOCAL_TRANSFORM
			// update quaternion in root space and my transform
			if (_parent != null && _parent._rotChanged)
			{
				// apply parent's rotation on ahead
				_rootSpaceQuaternion = _quaternion * _parent._quaternion;
				UpdateRootTransform();
			}
			else
#endif
			if (_mgr._vertChanged)
			{
				// update transform in this part's local space.
				_transform.localPosition	= _pos;
				_transform.localRotation	= _quaternion;
				_transform.localScale		= _scale;
			}
		}
		
		// ignore when called from initializing.
		if (initialize) return;
		
		// do callback at userdata key
		if (ExistsOnUserDataKey
		&&	_res.HasAttrFlags(SsKeyAttrFlags.User))
		{
			_OnEvent(SsKeyAttr.User);
		}

		// do callback at sound key
		if (ExistsOnSoundKey
		&&	_res.HasAttrFlags(SsKeyAttrFlags.Sound))
		{
			_OnEvent(SsKeyAttr.Sound);
		}
	}
	
	void
	_OnEvent(SsKeyAttr attr)
	{
		// take value at just key frame.
		SsAttrValue[] values = _res.GetAttrValues(attr);
		if (values.Length == 0) return;

		// ignore within previous frame
		if ((int)_mgr._animeFrame == (int)_mgr._prevFrame)
		{
			return;
		}
		
		// trace each frame from previous frame to current frame.
		int destFrame = (int)_mgr._animeFrame;
		int frame = (int)_mgr._prevFrame;
		
		for (int i = 0; i < _mgr._playFrameLength; ++i)
		{
			frame = _mgr._StepFrameFromPrev(frame);
			
			SsAttrValue v;
			if (frame < values.Length)
			{
				// get value at this frame
				v = values[frame];
			}
			else
			{
				// refer to the single value consolidated from multiple keyframes
				v = values[0];
			}
			
			// is this a generated value from interpolation?
			if (!v.HasValue)
			{
				// is time just at key?
				SsKeyFrameInterface key = _res.GetKey(attr, v.RefKeyIndex);
				if (key.Time == frame)
				{
					// OK
					switch (attr)
					{
					case SsKeyAttr.User:
						SsUserDataKeyFrame userKey = (SsUserDataKeyFrame)key;
						OnUserDataKey(this, userKey.Value);
						break;
					case SsKeyAttr.Sound:
						SsSoundKeyFrame soundKey = (SsSoundKeyFrame)key;
						OnSoundKey(this, soundKey.Value);
						break;
					default:
						Debug.LogWarning("Not implemented event: " + attr);
						break;
					}
				}
			}
			if (frame == destFrame)
			{
				break;
			}
		}
	}

	// now do nothing, operation will be done inside shaders.
	Color
	GetBlendedColor(SsColorRef src, SsColorBlendOperation op)
	{
		Color c = (Color)src;

		if (op == SsColorBlendOperation.Mul)
		{
			// to decrease vertex color's effect, must come near color value to 1
#if false
			// lerp with C#
			c.r = SsInterpolation.Linear(1f - c.a, c.r, 1f);
			c.g = SsInterpolation.Linear(1f - c.a, c.g, 1f);
			c.b = SsInterpolation.Linear(1f - c.a, c.b, 1f);
			// shader: combine texture * primary, texture
#else
			// lerp with shader
			// shader is like this:
			// combine primary lerp (primary) constant=(1,1,1,1), texture
			// combine previous * texture, texture
#endif
		}
		return c;
	}
	
	// create my transform and return it if it doesn't exists.
	public Transform
	CreateTransform()
	{
		if (_transform) return _transform;
		
#if true	// from me to root
		// create new object
		GameObject go = new GameObject(_res.Name);
		_transform = go.transform;
		if (_parent)
		{
			// attach to parent's transform recursively.
			if (_parent._transform == null)
				_transform.parent = _parent.CreateTransform();
			else
				_transform.parent = _parent._transform;
		}
		else
		{
			// attach to the transform of the Sprite's gameObject.
			_transform.parent = _mgr._transform;
		}
#else	// from root to me
		// create new object
		Transform parentTransform;
		if (_parent)
		{
			// attach to parent's transform recursively.
			if (_parent._transform == null)
				parentTransform = _parent.CreateTransform();
			else
				parentTransform = _parent._transform;
		}
		else
		{
			// attach to the transform of the Sprite's gameObject.
			parentTransform = _mgr._transform;
		}
		GameObject go = new GameObject(_res.Name);
		_transform = go.transform;
		_transform.parent = parentTransform;
#endif
		// update transform in this part's local space.
		_transform.localPosition	= _pos;
		_transform.localRotation	= _quaternion;
		_transform.localScale		= _scale;
		return _transform;
	}
		
#if _MAKE_ROOT_TO_LOCAL_TRANSFORM
	// get transform in root part's space.
	internal void
	UpdateRootTransform()
	{
		_rootToThisTransform.localPosition	= new Vector3(_pivotMatrix.m03, _pivotMatrix.m13, _pivotMatrix.m23);
//		_rootToThisTransform.localRotation	= QuaternionFromMatrix(_pivotMatrix);
		_rootToThisTransform.localRotation	= _parent ? _rootSpaceQuaternion : _quaternion;
		_rootToThisTransform.localScale		= new Vector3(_pivotMatrix.m00, _pivotMatrix.m11, _pivotMatrix.m22);
	}

	internal static Quaternion
	QuaternionFromMatrix(Matrix4x4 m)
	{
		Quaternion q = new Quaternion();
		q.w = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] + m[1,1] + m[2,2] ) ) / 2; 
		q.x = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] - m[1,1] - m[2,2] ) ) / 2; 
		q.y = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] + m[1,1] - m[2,2] ) ) / 2; 
		q.z = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] - m[1,1] + m[2,2] ) ) / 2; 
		q.x *= Mathf.Sign( q.x * ( m[2,1] - m[1,2] ) );
		q.y *= Mathf.Sign( q.y * ( m[0,2] - m[2,0] ) );
		q.z *= Mathf.Sign( q.z * ( m[1,0] - m[0,1] ) );
		return q;
	}
#endif
	// Intersects with another part?
	// This function is slow so you should use IntersectsByAABB if you don't need to consider rotaion or vertex modification.
	public	bool
	Intersects(SsPart other, bool ignoreZ)
	{
		if (!ignoreZ)
		{
			// check only left-top vertex
			if (_mgr._vertices[_vIndex].z != other._mgr._vertices[other._vIndex].z)
				return false;
		}
		// make offset vectors from latest vertices
		Vector3[] myPoints = MakeGlobalPoints(_mgr.transform.position, _mgr._vertices, _vIndex);
		Vector3[] otPoints = MakeGlobalPoints(other._mgr.transform.position, other._mgr._vertices, other._vIndex);
		// containes other points in my plane?
		for (int i = 0; i < 4; ++i)
			if (ContainsPointInPlane(myPoints, otPoints[i]))
				return true;
		// containes my points in other plane?
		for (int i = 0; i < 4; ++i)
			if (ContainsPointInPlane(otPoints, myPoints[i]))
				return true;
		return false;
	}

	internal void
	DrawBoundingPart()
	{
		// make global vertices
		Vector3[] myPoints = MakeGlobalPoints(_mgr.transform.position, _mgr._vertices, _vIndex);
		// draw bounds
		DrawQuadrangle(myPoints[0], myPoints[1], myPoints[2], myPoints[3], Color.green);
	}
	
	Vector3[] MakeGlobalPoints(Vector3 gpos, Vector3[] verts, int startIndex)
	{
		// vertex order is:
		// 01
		// 32
		// make offset vectors
		Vector3[] points = new Vector3[4];
		for (int i = 0; i < 4; ++i)
			points[i] = gpos + verts[startIndex + i];
		return points;
	}
					
	bool
	ContainsPointInPlane(Vector3[] v, Vector3 pt)
	{
		Vector3[]	subvecs = {v[0] - pt, v[1] - pt, v[2] - pt, v[3] - pt};
		if (Vector3.Cross(subvecs[0], subvecs[1]).z <= 0
		&&	Vector3.Cross(subvecs[1], subvecs[2]).z <= 0
		&&	Vector3.Cross(subvecs[2], subvecs[3]).z <= 0
		&&	Vector3.Cross(subvecs[3], subvecs[0]).z <= 0)
			return true;
		return false;
	}

	// Intersects with another part by AABB?
	public	bool
	IntersectsByAABB(SsPart other, bool ignoreZ)
	{
		// get latest vertices
		Vector3[] v = _mgr._vertices;
		Vector3[] o = other._mgr._vertices;
		if (!ignoreZ)
		{
			// check only left-top vertex
			if (v[_vIndex].z != o[other._vIndex].z)
				return false;
		}
		// vertex order is:
		// 01
		// 32
		// get min, max position
		Vector3 mlt, mrb, olt, orb;
		MakeMinMaxPosition(_mgr.transform.position, v, _vIndex, out mlt, out mrb);
		MakeMinMaxPosition(other._mgr.transform.position, o, other._vIndex, out olt, out orb);
#if true
		if (mrb.x >= olt.x && mrb.y >= olt.y
		&&	orb.x >= mlt.x && orb.y >= mlt.y)
			return true;
		return false;
#else
		var mBounds = new Bounds(Vector3.zero, Vector3.zero);
		var oBounds = new Bounds(Vector3.zero, Vector3.zero);
		// make my bounds
		mBounds.SetMinMax(mlt, mrb);
		mBounds.center = mpivot + new Vector3(_pivotMatrix.m03, _pivotMatrix.m13, _pivotMatrix.m23);
		// make other bounds
		oBounds.SetMinMax(olt, orb);
		oBounds.center = opivot + new Vector3(other._pivotMatrix.m03, other._pivotMatrix.m13, other._pivotMatrix.m23);
		return mBounds.Intersects(oBounds);
#endif
	}

	internal void
	DrawBoundingPartAABB()
	{
		// make vertices sorted by clockwise from left top
		Vector3 lt, rb;
		MakeMinMaxPosition(_mgr.transform.position, _mgr._vertices, _vIndex, out lt, out rb);
		// draw bounds
		DrawRectangle(lt, rb, Color.red);
	}
	
	static Vector3 sVec3Min = new Vector3(float.MinValue, float.MinValue);
	static Vector3 sVec3Max = new Vector3(float.MaxValue, float.MaxValue);
	
	void
	MakeMinMaxPosition(Vector3 gpos, Vector3[] verts, int startIndex, out Vector3 lt, out Vector3 rb)
	{
		// vertex order is:
		// 01
		// 32
		// get min, max position
		lt = sVec3Max;
		rb = sVec3Min;
		for (int i = 0; i < 4; ++i)
		{
			Vector3 v = verts[startIndex + i] + gpos;
			if (v.x < lt.x) lt.x = v.x;
			if (v.x > rb.x) rb.x = v.x;
			if (v.y < lt.y) lt.y = v.y;
			if (v.y > rb.y) rb.y = v.y;
		}
	}
	
	internal void
	DrawBounds(Bounds bounds, Color color)
	{
		// get rectangle from bounding box.
		Vector3 lt = bounds.min;
		Vector3 rb = bounds.max;
		Vector3 rt = lt;
		rt.x = rb.x;
		Vector3 lb = lt;
		lb.y = rb.y;
		DrawQuadrangle(lt, rt, rb, lb, color);
	}
	
	internal void
	DrawRectangle(Vector3 lt, Vector3 rb, Color color)
	{
		Vector3 rt = lt;
		rt.x = rb.x;
		Vector3 lb = lt;
		lb.y = rb.y;
		DrawQuadrangle(lt, rt, rb, lb, color);
	}

	// parameters are clockwise
	internal void
	DrawQuadrangle(Vector3 lt, Vector3 rt, Vector3 rb, Vector3 lb, Color color)
	{
		// draw rectangle
		Debug.DrawLine(lt, rt, color);
		Debug.DrawLine(rt, rb, color);
		Debug.DrawLine(rb, lb, color);
		Debug.DrawLine(lb, lt, color);
	}
	
	// individualize material
	public Material
	IndividualizeMaterial(bool mulMatColor)
	{
		if (_material == null) return null;
#if true
		// do nothing if individualized already
		if (_individualMaterial != null) return _individualMaterial;
#else
		// delete old before
		if (_individualMaterial != null)
		{
			//GameObject.DestroyImmediate(_individualMaterial, false);
			GameObject.Destroy(_individualMaterial);
		}
#endif
		// instantiate the original material assigned statically then replace to it
		_individualMaterial = (Material)UnityEngine.Object.Instantiate(_material);
		// save original material
		if (_originalMaterial == null)
			_originalMaterial = _material;
		_material = _individualMaterial;
		if (mulMatColor)
		{
			// replace shader blends material color
			var shaderType = SsShaderManager.EnumToType(_colorBlendType, _res.AlphaBlendType, SsMaterialColorBlendOperation.Mul);
			_material.shader = SsShaderManager.Get(shaderType, false);
		}
		_mgr._materials[_subMeshIndex] = _material;
		// to update
		_mgr._matChanged = true;
		return _material;
	}
	
	// change material
	public void
	ChangeMaterial(Material mat)
	{
		// save original material
		if (_originalMaterial == null)
			_originalMaterial = _material;
		// replace
		_material = mat;
		_mgr._materials[_subMeshIndex] = _material;
		// to update
		_mgr._matChanged = true;
	}

	// revert changed material
	public void
	RevertChangedMaterial()
	{
		if (_originalMaterial == null) return;
		// reset
		if (_individualMaterial != null)	// changed to true 2012.08.08
			GameObject.Destroy(_individualMaterial);
		_mgr._materials[_subMeshIndex] = _material = _originalMaterial;
		// to update
		_mgr._matChanged = true;
	}
	
#if true	// changed to true 2012.08.08
	// delete resources created on runtime
	internal void
	DeleteResources()
	{
		// delete material
		if (_individualMaterial != null)
		{
			//GameObject.DestroyImmediate(_individualMaterial, false);
			GameObject.Destroy(_individualMaterial);
		}
	}
#endif
	
}
