/**
	SpriteStudioPlayer
	
	Sprite consists of multi parts
	
	Copyright(C) 2003-2013 Web Technology Corp. 
	
*/

//#define _DEBUG
//#define _TEST_FRAME_SKIP
#define _USE_SHARED_MATERIAL

using UnityEngine;
using System;
using System.Collections.Generic;

// animation play direction
public enum SsAnimePlayDirection
{
	Forward,
	Reverse,
	RoundTrip,
	ReverseRoundTrip,
}

/// Sub animation controller
[System.Serializable]
public class SsSubAnimeController
{
	// Type of delegate to be called when the animation was just finished
	public	delegate void SubAnimationCallback(SsSubAnimeController subAnime);

	// Delegate to be called when the animation was just finished
	public	SubAnimationCallback	AnimationFinished;

	public	SsAnimation	Animation = null;
	public	float		Frame = 0;
	public	float		Speed = 1;
	public	int			PlayCount = 0;
	public	int			CurrentPlayCount = 0;
	public	bool		BindsToAllParts = false; ///< binds to all parts in the main animation except root part
	public	bool		BindsByPartName = false; ///< binds part by name or index
	public	bool		IsPlaying = true;

	public	void		Play()
	{
		IsPlaying = true;
	}

	public	void		Pause()
	{
		IsPlaying = false;
	}

	public	void		Reset()
	{
		Pause();
		CurrentPlayCount = 0;
	}
	
	public	void		StepFrame(float deltaTime)
	{
		if (Animation == null) return;
		if (!IsPlaying) return;
		
		// step
		Frame += deltaTime * Speed * Animation.FPS;
			
		if (Frame < 0 || (int)Frame > Animation.EndFrame)
		{
			// reached to the edge of animation
			bool repeat = true;
			if (PlayCount > 0)
			{
				// finite play count
				if (++CurrentPlayCount >= PlayCount)
				{
					// completely finished
					repeat = false;
					IsPlaying = false;

					// clamp
					Frame = Frame < 0 ? 0 : (Frame > Animation.EndFrame ? Animation.EndFrame : Frame);
					CurrentPlayCount = PlayCount;

					if (AnimationFinished != null)
					{
						AnimationFinished(this);
					}
				}
			}
			if (repeat)
			{
				// do loop
				Frame %= Animation.EndFrame;
				if (Frame < 0) Frame = Animation.EndFrame + 1 + Frame;
			}
		}
	}
}

/// A sprite object behaviour which plays an animation.
//[System.Serializable]
[AddComponentMenu("Sprite Studio/SsSprite")]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class SsSprite : MonoBehaviour
{
	// Create game object with this script 
	static public GameObject CreateGameObject(string name)
	{
		GameObject go = new GameObject(name);
		go.AddComponent<SsSprite>();
		return go;
	}

	// Create game object with this script and animation which in 'Resources' folder.
	static public GameObject CreateGameObjectWithAnime(string name, string animName)
	{
		GameObject go = new GameObject(name);
		SsSprite sprite = go.AddComponent<SsSprite>();
		SsAnimation anim = (SsAnimation)Resources.Load(animName, typeof(SsAnimation));
		sprite.Animation = anim;
		return go;
	}

	// sub animations
	public	List<SsSubAnimeController>	subAnimations = new List<SsSubAnimeController>();

	//--------- public accessible parameters
	
	// Getter and setter of the animation 
	// This doesn't any operations if passed animation equals to the current.
	public	SsAnimation		Animation
	{
		get {return _animation;}
		set
		{
			_animation = value;
			if (value == _prevAnimation) return;
			
			// clear or reinitialize
			Init();
			
			// init animation range
			_startAnimeFrame = 0;
			if (_animation)
				_endAnimeFrame = _animation.EndFrame;
			else
				_endAnimeFrame = 0;
			
			_prevAnimation = _animation;
	 		
			// auto start after changed
			if (PlayAtStart && Application.isPlaying)
				Play();
		}
	}

	// Replace animation without rebuilding mesh.
	// This is faster than changing animation via Animation property.
	// This requires the equality of parts struct between before and after.
	// Do the same behaviour as Animation property if no animation is set.
	public	void	ReplaceAnimation(SsAnimation anm)
	{
		if (!_animation)
		{
			// do the same Animation property because it needs to build a mesh.
			Animation = anm;
			return;
		}
		
		if (anm.PartList.Length != _animation.PartList.Length)
		{
			Debug.LogError("Can't replace animation because of the difference of the parts count.\n" +
				anm.name + ": " + anm.PartList.Length + " " + _animation.name + ": " + _animation.PartList.Length);
			return;
		}
		
		_animation = anm;
		if (anm == _prevAnimation) return;

		if (!_animation)
		{
			Init();
			return;
		}
		
		// initialize values
		ResetAnimationStatus();
		Pause();
		
		// get parts and images.
		_partResList = _animation.PartList;
		_imageList = _animation.ImageList;
		_partsNum = _partResList.Length;

		// replace parts resources
		for (int i = 0; i < _parts.Length; ++i)
			_parts[i]._res = _partResList[i];

		// must invoke vertex update to display initial posed animation.
		_vertChanged = true;
		
		// also must update uv
		_uvChanged = true;
		
		// reflect blend settings to shader
		_extraChanged = true;
			
		// to fix the problem that h/vFlipped animation from initial time doesn't affects mesh.
		// created children transforms are updated inside.
		UpdateAlways();

		// init animation range
		_startAnimeFrame = 0;
		if (_animation)
			_endAnimeFrame = _animation.EndFrame;
		else
			_endAnimeFrame = 0;
		
		_prevAnimation = _animation;
 		
		// auto start after changed
		if (PlayAtStart && Application.isPlaying)
			Play();
	}
	[SerializeField,HideInInspector] internal	SsAnimation		_animation;
	SsAnimation		_prevAnimation;

	// shortcut to gameObject.transform
	internal	Transform	_transform;
	//[SerializeField/*,HideInInspector*/] public	Matrix4x4	_rootMatrix;	// expose to SpriteEditor class
	
	// Get a part of the animation by index
	public	SsPart			GetPart(int index)
	{
		if (_parts == null) return null;
		if (index >= _parts.Length) return null;
		return _parts[index];
	}
	
	// Get a part of the animation by name
	public	SsPart			GetPart(string name)
	{
		if (name == null)
			return _parts[0];
		foreach (SsPart e in _parts)
			if (e._res.Name == name)
				return e;
		return null;
	}
	// Get a part of the animation by index
	public	SsPart[]		GetParts()
	{
		return _parts;
	}
	
	// Transform at a child part in world space.
	public	Transform		TransformAt(string name)
	{
		if (name == null)
			return _transform;
		SsPart e = GetPart(name);
		if (e == null) return null;
		return e._transform;
	}
	
	// Position of this gameObject in world space.
	public	Vector3			Position
	{
		get	{return _transform.position;}
		set	{_transform.position = value;}
	}
	
	// Position at a child part in world space.
	public	Vector3			PositionAt(string name)
	{
		Transform t = TransformAt(name);
		if (!t)
		{
			Debug.LogError("Can't find child part: " + name);
			return Vector3.zero;
		}
		return t.position;
	}

	// Local rotation of the root part.
	public	Vector3			Rotation
	{
		get	{return _transform.localRotation.eulerAngles;}
		set	{
			Quaternion rot = Quaternion.identity;
			rot.eulerAngles = value;
			_transform.localRotation = rot;
		}
	}
	
	// Local scale of the root part.
	public	Vector3			Scale
	{
		get	{return _transform.localScale;}
		set	{_transform.localScale = value;}
	}
	
	// Horizontal flip state
	public	bool			hFlip;

	// Vertical flip state
	public	bool			vFlip;
	
	// Start to play the animation or resume paused animation.
	public	void			Play()
	{
		_isPlaying = true;
		_isFinished = false;
	}
	
	// Pause the animation
	public	void			Pause()
	{
		_isPlaying = false;
	}
	bool	_isPlaying = true;	// is playing now?
	
	// Is the animation playing?
	public	bool	IsPlaying()
	{
		return _isPlaying;
	}
	
	// Type of delegate to be called when the animation was just finished
	public	delegate void AnimationCallback(SsSprite sprite);

	// Delegate to be called when the animation was just finished
	public	AnimationCallback	AnimationFinished;
	
	// Is the animation finished?
	public	bool IsAnimationFinished()
	{
		return _isFinished;
	}
	bool	_isFinished = false; // is finished?
		
	// Current animation frame
	public	float			AnimFrame
	{
		get {return _animeFrame;}
		set
		{
			if (value < 0f) return;
			if (!_animation || value > _animation.EndFrame) return;
			_animeFrame = value;
		}
	}
	public		float		_animeFrame;	// 'public' to show in inspector
	internal	float		_prevFrame;
	internal	int			_prevStepSign;
	
	// Start animation frame
	public	float			StartFrame
	{
		get {return _startAnimeFrame;}
		set
		{
			if (value < 0f) return;
			if (!_animation || value > _animation.EndFrame || value > _endAnimeFrame) return;
			_startAnimeFrame = (int)value;
		}
	}
	public	int				_startAnimeFrame;	// 'public' to show in inspector
	
	// End animation frame
	public	float			EndFrame
	{
		get {return _endAnimeFrame;}
		set
		{
			if (value < 0f) return;
			if (!_animation || value > _animation.EndFrame || value < _startAnimeFrame) return;
			_endAnimeFrame = (int)value;
		}
	}
	public	int				_endAnimeFrame;	// 'public' to show in inspector

	// Set Start/End animation frame
	public	void			SetStartEndFrame(int start, int end)
	{
		_startAnimeFrame = start < 0 ? 0 : (start > _animation.EndFrame ? _animation.EndFrame : start);
		_endAnimeFrame = end < 0 ? 0 : (end > _animation.EndFrame ? _animation.EndFrame : end);
	}
	
	// Is last frame
	public	bool			IsLastFrame()
	{
		return _animeFrame >= _endAnimeFrame;
	}

	// Play direction
	public	SsAnimePlayDirection	PlayDirection
	{
		get {return _playDirection;}
		set {
			_playDirection = value;
			ResetAnimationStatus();
		}
	}
	
	// Set play direction with options
	public	void	SetPlayDirection(SsAnimePlayDirection dir, bool keepFrame)
	{
		_playDirection = dir;
		float frame = _animeFrame;
		ResetAnimationStatus();
		if (keepFrame)
			_animeFrame = frame;
	}
	public	SsAnimePlayDirection	_playDirection;	// 'public' to show in inspector
	[HideInInspector] public	SsAnimePlayDirection	_prevPlayDirection;	// expose to SpriteEditor class

	// Is the type of play direction "Round Trip"?
	bool			IsRoundTrip
	{
		get {
			return PlayDirection == SsAnimePlayDirection.RoundTrip
				|| PlayDirection == SsAnimePlayDirection.ReverseRoundTrip;
		}
	}
	internal int	_currentStepSign = 1;
	bool			_returnTrip = false;

	// Play count (0=eternal)
	public	int				PlayCount = 0;
	int				_currentPlayCount = 0;
	
	// Play speed
	public	float			Speed = 1f;
	
	// Play at Start()
	public	bool			PlayAtStart = true;
	
	// Destroy the client game object when current animation is finished.
	public	bool			DestroyAtEnd = false;
	
	// Time to destroy automatically by seconds. Nothing effects if this value is zero.
	public	float			LifeTime
	{
		get {return _lifeTime;}
		set {
			_lifeTime = value;
			_lifeTimeCount = 0;
		}
	}
	public	float	_lifeTime = 0;	// 'public' to show in inspector
	float	_lifeTimeCount;
	
	// Intersects with another sprite?
	public	bool			IntersectsByBounds(SsSprite other, bool ignoreZ)
	{
		if (!_mesh || !other._mesh) return false;
		if (ignoreZ)
		{
			// create temporary and modify it then test.
			Bounds b = other._bounds;
			Vector3 v = b.center;
			v.z = _bounds.center.z;
			b.center = v;
			return _bounds.Intersects(b);
		}
		else
			return _bounds.Intersects(other._bounds);
	}

	// Intersects with another sprite by bounding parts?
	public	bool			IntersectsByBoundingParts(SsSprite other, bool ignoreZ, bool useAABB)
	{
		foreach (var m in _boundPartList)
		{
			foreach (var o in other._boundPartList)
			{
				if (useAABB)
				{
					if (m.IntersectsByAABB(o, ignoreZ))
						return true;
				}
				else
				{
					if (m.Intersects(o, ignoreZ))
						return true;
				}
			}
		}
		return false;
	}

	// Is point contained in the bounding box?
	public	bool			ContainsPoint(Vector3 point, bool ignoreZ)
	{
		if (!_mesh) return false;
		if (ignoreZ)
		{
			// create temporaries and modify them then test.
			Vector3 v = point;
			v.z = _bounds.center.z;
			return _bounds.Contains(v);
		}
		else
			return _bounds.Contains(point);
	}
	
	// List of the resources of all parts contained in the animation.
	public	SsPartRes[]		PartResList
	{
		get {return _partResList;}
	}
	
	// update the bounding box of the BoxCollider when the animation is updated.
	public	bool			UpdateCollider = false;

	//--------- followings are for debug
	
	// Draw bounding box generated Unity Bounds class.
	public	bool			DrawBoundingBox;

	// Draw bounding parts
	public	bool			DrawBoundingParts;

	//--------- works
	SsPartRes[]		_partResList;
	SsImageFile[]	_imageList;
	int				_partsNum;
	SsPart[]		_parts;
	Bounds			_bounds = new Bounds(Vector3.zero, Vector3.zero);
	List<SsPart>	_boundPartList;
	internal int	_playFrameLength;

	MeshFilter		_meshFilter;
	MeshRenderer	_meshRenderer;
	BoxCollider		_boxCollider;
	SphereCollider	_sphereCollider;
	CapsuleCollider	_capsuleCollider;

	//public	bool	ExistsMaterial {get {return _materials != null;}}

	//--------- followings are by way of allowing SsPart to directly access to me.
    internal	SsPart		_rootPart;
	internal	Material[]	_materials;
    internal 	Mesh		_mesh;
    internal 	Vector3[]	_vertices;
	internal 	Vector2[]	_uvs;
    internal 	Color[]		_colors;
    internal 	Vector2[]	_extras;	// use texcoord1 as the effect extent of color blending, color blend type and alpha blend type.
	
	// temporary values
    internal	bool		_vertChanged;
    internal	bool		_uvChanged;
    internal	bool		_colorChanged;
    internal	bool		_prioChanged;
    internal	bool		_matChanged;
    internal	bool		_extraChanged;

	// imported time to get notified of the attached animation reimported.
	[SerializeField,HideInInspector] int ImportedTime;
	[SerializeField,HideInInspector] int ImportedTimeHigh;

//	public SsSprite()
//	{
//		if (!gameObject) return;	// forbidden access
//		if (Application.isPlaying) return;
//		LastSprite = this;	// null if this is prefab
//	}
	
	void
	Awake()
	{
//		Debug.Log("Awake(): " + gameObject.name);
#if _DEBUG
		DrawBoundingBox = true;
#endif
		_transform = transform;
		EnsureMeshComponent();
		_boxCollider = GetComponent<BoxCollider>();
		_sphereCollider = GetComponent<SphereCollider>();
		_capsuleCollider = GetComponent<CapsuleCollider>();

		Init();
	}
	
	void
	EnsureMeshComponent()
	{
 		if (!_meshFilter)
			_meshFilter = GetComponent<MeshFilter>();
 		if (!_meshRenderer)
			_meshRenderer = GetComponent<MeshRenderer>();

		// unneccesary options should be omitted.
		_meshRenderer.castShadows = false;
		_meshRenderer.receiveShadows = false;
	}

	void
	Start()
	{
//		Debug.Log("Start(): " + gameObject.name);
			
 		// auto start at playing.
		if (PlayAtStart && Application.isPlaying)
			Play();
	}

	void
	Init()
	{
		// if don't do this, Transforms(GameObjects) are created every time as come into ExecuteInEditMode.
		DeleteTransformChildren();

		//Debug.Log("Init() " + gameObject.name);
		if (!_animation)
		{
			// clear variables
			//Debug.LogWarning("No anime resource attached-> " + gameObject.name);
				
			// works
			_partResList	= null;
			_imageList		= null;
			_partsNum		= 0;
			_parts			= null;
			_boundPartList	= null;
			_materials	= null;
			_vertices	= null;
			_uvs		= null;
			_colors		= null;
			_extras		= null;
			_mesh		= null;
			_meshFilter.mesh = null;
#if _USE_SHARED_MATERIAL
			_meshRenderer.sharedMaterials = new Material[1];
#else
			_meshRenderer.materials = new Material[1];
#endif

			// statuses
			_isPlaying = _isFinished = false;
			PlayCount = 0;
			Speed = 1f;
			_prevPlayDirection = _playDirection = SsAnimePlayDirection.Forward;
			ResetAnimationStatus();
			_startAnimeFrame = _endAnimeFrame = _playFrameLength = 0;
			return;
		}

		EnsureMeshComponent();

		// initialize values
		ResetAnimationStatus();
		Pause();
		_isFinished = false;
		
		// get parts and images.
		_partResList = _animation.PartList;
		_imageList = _animation.ImageList;
		_partsNum = _partResList.Length;

		// create material array. actually root doesn't need material so can cut down.
		_materials = new Material[_partsNum - 1];
		
		// create vertices...
		int vertNum = (_partsNum - 1) * 4;
		_vertices	= new Vector3[vertNum];
		_uvs		= new Vector2[vertNum];
		_colors		= new Color[vertNum];
		_extras		= new Vector2[vertNum];

		// now create mesh
		_mesh = new Mesh();
		_mesh.vertices	= _vertices;	// to avoid "out of bounds" error when we set triangle indices.
		_mesh.subMeshCount = _partsNum - 1;
		_mesh.uv2 = _extras;
		
		// attach to MeshFilter
		_meshFilter.mesh = _mesh;
		// create sprite parts and pass to resource used
		_parts = new SsPart[_partsNum];
		for (int i = 0; i < _parts.Length; ++i)
		{
			SsPartRes partRes = _partResList[i];
			SsImageFile image = partRes.IsRoot ? null : _imageList[partRes.SrcObjId];
			_parts[i] = new SsPart(this, i, partRes, image);
			// set root sprite
			if (i == 0)
				_rootPart = _parts[i];
			else
			{
				// set individual material to buffer
				_materials[i - 1] = _parts[i]._material;
			}
			// register bound parts separately for fast access
			if (partRes.Type == SsPartType.Bound)
			{
				if (_boundPartList == null)
					_boundPartList = new List<SsPart>();
				_boundPartList.Add(_parts[i]);
			}
		}
			
		// set materials to affect
#if _USE_SHARED_MATERIAL
		_meshRenderer.sharedMaterials = _materials;
#else
		_meshRenderer.materials = _materials;
#endif
		// attach values to the Mesh
		_mesh.vertices	= _vertices;
		_mesh.uv		= _uvs;

		// at first time must invoke vertex update to display initial posed animation.
		_vertChanged = true;
		
		// reflect blend settings to shader
		_extraChanged = true;

#if false
		// use user data numeric value as PlayCount
		if (PartResList[0].UserKeys.Count > 0)
		{
			SsUserDataKeyFrame userKey = (SsUserDataKeyFrame)PartResList[0].GetKey(SsKeyAttr.User, 0);
			PlayCount = userKey.Value.Num;
		}
#endif
		// to fix the problem that h/vFlipped animation from initial time doesn't affects mesh
		UpdateAlways();
	}

	void
	OnEnable()
	{
		//Debug.Log(name + " OnEnable()");
	}

	void
	EnsureLatestAnime()
	{
		// reinitialize with latest animation data due to possibly invalid animation when in editor mode.
		if (_animation
		&&	!_animation.EqualsImportedTime(ImportedTime, ImportedTimeHigh))
		{
//			Debug.Log("Reinit");
			EnsureMeshComponent();
			Init();
			ImportedTime = _animation.ImportedTime;
			ImportedTimeHigh = _animation.ImportedTimeHigh;
		}
	}

// 	void
// 	OnDisable()
// 	{
//		Debug.Log(name + " OnDisable()");
//	}

	void
	OnDestroy()
	{
//		Debug.Log(name + " OnDestroy()");
		DeleteTransformChildren();
	}

	void
	DeleteTransformChildren()
	{
		// free mesh
		DestroyImmediate(_mesh);
		
		// delete children
		if (_transform)
			foreach (Transform child in _transform)
			    DestroyImmediate(child.gameObject);

#if true // changed to true 2012.08.08
		if (_parts != null)
		{
			// delete resources created in parts individually 
			for (int i = 0; i < _parts.Length; ++i)
				_parts[i].DeleteResources();
		}
#endif
	}

	// expose to SpriteEditor class
	public void
	ResetAnimationStatus()
	{
		if (PlayDirection == SsAnimePlayDirection.Reverse
		||	PlayDirection == SsAnimePlayDirection.ReverseRoundTrip)
		{
			_currentStepSign = -1;
			_animeFrame = _endAnimeFrame;
		}
		else
		{
			_currentStepSign = 1;
			_animeFrame = _startAnimeFrame;
		}
		_playFrameLength = _endAnimeFrame + 1 - _startAnimeFrame;
		// reset play status
		_currentPlayCount = 0;
		_returnTrip = false;
		
		_prevFrame = -1;
		_prevStepSign = _currentStepSign;
	}	
	
	public void
	UpdateVertex()
	{
		_vertChanged = true;
	}

	void
	Update()
	{
		// draw bounding box
		if (DrawBoundingBox && _mesh != null)
		{
			if (!Application.isPlaying)
			    updateBoundingBox();
			// get rectangle from bounding box.
			Vector3 lt = _bounds.min;
			Vector3 rb = _bounds.max;
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

		// draw bounding parts
		if (DrawBoundingParts && _mesh != null && _boundPartList != null)
		{
			foreach (var e in _boundPartList)
			{
				e.DrawBoundingPart();
				e.DrawBoundingPartAABB();
			}
		}

		// igone calling Update() automatically from Unity in edit mode 
		if (!Application.isPlaying)
		{
			EnsureLatestAnime();
			return;
		}
		
		// update actually
		UpdateAlways();
	}

#if _TEST_FRAME_SKIP
	static int	_frameSkipCount = 0;
#endif
	
	// expose to SpriteEditor class
	public void
	UpdateAlways()
	{
		if (_animation == null) return;
		if (_parts == null) return;
		// when this object is created from prefab, Unity comes here though all of _parts is not yet constructed... 
		if (_parts[_partsNum - 1] == null) return;

 		if (!Application.isPlaying)
		{
			if (_animeFrame < _startAnimeFrame)
				_animeFrame = _startAnimeFrame;
			else if (_animeFrame > _endAnimeFrame)
				_animeFrame = _endAnimeFrame;
		}
		
		// check for automatic destruction by life time
		if (_lifeTime > 0)
		{
			_lifeTimeCount += Time.deltaTime;
			if (_lifeTimeCount >= _lifeTime)
			{
				Destroy(gameObject);
				return;
			}
		}

		if (subAnimations != null)
		{
			// apply sub animations
			foreach (var e in subAnimations)
			{
				if (e.Animation == null) continue;
				
				if (e.BindsToAllParts)
				{
					// bind to all parts in the main animation
					for (int i = 1; i < _parts.Length; ++i)
					{
						// bind animation from all parts in the sub animation
						for (int index = 0; index < e.Animation.PartList.Length; ++index)
						{
							_parts[i].AddSubAnime(e, e.Animation.PartList[index]);
						}
					}
				}
				else
				{
					// bind each part by name or index
					for (int index = 0; index < e.Animation.PartList.Length; ++index)
					{
						SsPart part = null;
						if (e.BindsByPartName)
						{
							// bind part by name
							part = GetPart(e.Animation.PartList[index].Name);
						}
						else
						{
							// bind by index
							part = GetPart(index);
						}
						if (part != null)
						{
							// register sub anime to do inside Update() below.
							part.AddSubAnime(e, e.Animation.PartList[index]);
						}
					}
				}
			}
		}

		// update all parts
		foreach (SsPart e in _parts)
			e.Update();

		if (_prioChanged)
		{
			// sort draw order by priority
			SortByPriority();
			_prioChanged = false;
		}
		
		// update mesh contents if anything was changed.
		if (_vertChanged)
		{
			_mesh.vertices = _vertices;
			updateBoundingBox();
			_vertChanged = false;
		}
		if (_uvChanged)
		{
			_mesh.uv = _uvs;
			_uvChanged = false;
		}
		if (_colorChanged)
		{
			_mesh.colors = _colors;
			_colorChanged = false;
		}
		if (_extraChanged)
		{
			_mesh.uv2 = _extras;
			_extraChanged = false;
		}

		// update materials
		if (_matChanged)
		{
#if _USE_SHARED_MATERIAL
			_meshRenderer.sharedMaterials = _materials;
#else
			_meshRenderer.materials = _materials;
#endif
			_matChanged = false;
		}

		if (_isPlaying)
		{
			// step animation frame
			float step = Time.deltaTime * _animation.FPS * Speed * _currentStepSign;
#if _TEST_FRAME_SKIP
			if (Time.deltaTime * _animation.FPS > 1)
				++_frameSkipCount;
//				Debug.Log("lost frame at " + Time.frameCount);
#endif
			_prevFrame = _animeFrame;
			_animeFrame += step;
			
			//if (step >= _playFrameLength)
			//	Debug.LogWarning("Too long frameskip!! " + step);
			
			// end of animation
			if (_animeFrame < _startAnimeFrame
			||	(int)_animeFrame > _endAnimeFrame) // 2012.02.05 cares about last frame
			{
				bool clipFrame = true;
				if (IsRoundTrip)
				{
					// reverse direction
					_prevStepSign = _currentStepSign;
					_currentStepSign *= -1;
					if (!_returnTrip)
					{
						// now onto return path during round trip
						_returnTrip = true;
					}
					else
						_returnTrip = false;
				}
				else
					clipFrame = false;
				
				if (!_returnTrip)
				{
					if (PlayCount == 0)
					{
						// endless
					}
					else
					{
						if (++_currentPlayCount >= PlayCount)
						{
							// really end
							clipFrame = true;
							_isPlaying = false;
							_isFinished = true;
							if (AnimationFinished != null)
								AnimationFinished(this);
							// destroy the client game object
							if (DestroyAtEnd)
								Destroy(gameObject);
						}
					}
				}
				
				float overrun = 0;
				if (_animeFrame < _startAnimeFrame)
				{
					overrun = (_animeFrame - _startAnimeFrame) % _playFrameLength;
					overrun *= -1;
				}
				else if ((int)_animeFrame > _endAnimeFrame)
				{
					overrun = (_animeFrame - 1 - _endAnimeFrame) % _playFrameLength;
				}
				if (overrun < 0) overrun = 0;
				
				if (clipFrame)
				{
					// stop
					if (IsRoundTrip)
					{
						// reverse direction
						
						// reflect overrun
						if (_animeFrame < _startAnimeFrame)
							_animeFrame = _startAnimeFrame + overrun;
						else if (_animeFrame > _endAnimeFrame)
							_animeFrame = _endAnimeFrame - overrun;
					}
					else
					{
						// stop
						if (_animeFrame < _startAnimeFrame)
							_animeFrame = _startAnimeFrame;
						else if (_animeFrame > _endAnimeFrame)
							_animeFrame = _endAnimeFrame;
					}
				}
				else
				{
					// repeat
					if (_animeFrame < _startAnimeFrame)
						_animeFrame = _endAnimeFrame + 1 - overrun; // 2012.02.05 cares about last frame
					else if (_animeFrame > _endAnimeFrame)
						_animeFrame = _startAnimeFrame + overrun;
				}
			}
			
			// step sub animations
			foreach (var e in subAnimations)
			{
				e.StepFrame(Time.deltaTime);
			}
		}
	}

	internal int
	_StepFrameFromPrev(int frame)
	{
		frame += (Speed > 0 ? _prevStepSign : -_prevStepSign);
		if (frame > _endAnimeFrame)
		{
			if (IsRoundTrip)
			{
				_prevStepSign *= -1;
				frame = _endAnimeFrame - 1;
				if (frame < 0) frame = _endAnimeFrame;
			}
			else
				frame = _startAnimeFrame;
		}
		else if (frame < _startAnimeFrame)
		{
			if (IsRoundTrip)
			{
				_prevStepSign *= -1;
				frame = _startAnimeFrame + 1;
			}
			else
				frame = _endAnimeFrame;
		}
		return frame;
	}

#if _TEST_FRAME_SKIP
	void OnGUI()
	{
	    GUILayout.Label("Skipped frames: " + _frameSkipCount);
	}
#endif

	internal void
	updateBoundingBox()
	{
		_mesh.RecalculateBounds();
		_mesh.RecalculateNormals();
		// get latest bounds
		// _mesh.bounds are not applied transform of this gameObject so that use renderer's.
		_bounds = renderer.bounds;
#if false
		// even Z position to compare the bound's intersection with another sprite
		Vector3 v = _bounds.center;
		v.z = 0f;
		_bounds.center = v;
#endif
		if (UpdateCollider)
		{
			// destroy and create! there is no way to update the bounding box/sphere of the box collider.
			if (_boxCollider)
			{
				DestroyImmediate(_boxCollider);
				_boxCollider = gameObject.AddComponent<BoxCollider>();
				// thicken size to prevent penetration
				Vector3 size = _boxCollider.size;
				size.z = 1f;
				_boxCollider.size = size;
			}
			else if (_sphereCollider)
			{
				DestroyImmediate(_sphereCollider);
				_sphereCollider = gameObject.AddComponent<SphereCollider>();
			}
			else if (_capsuleCollider)
			{
				DestroyImmediate(_capsuleCollider);
				_capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
			}
		}
	}
		
	void
	SortByPriority()
	{
		SsPart[] sorted = new SsPart[_parts.Length];
		for (int i = 0; i < _parts.Length; ++i)
			sorted[i] = _parts[i];
		
		// sort
		Array.Sort(sorted);
		
		// change index in submesh array to change draw order
		int index = 0;
		for (int i = 0; i < _parts.Length; ++i)
		{
			if (!sorted[i]._res.IsRoot)
			{
				sorted[i].SetToSubmeshArray(index);
				// change material order same as submesh
				_materials[index] = sorted[i]._material;
				++index;
			}
		}
			
		// update material list
		_matChanged = true;
	}

	// get a part of this sprite by index.
	internal SsPart
	Sprite(int index)
	{
		if (index < 0 || index >= _partsNum) return null;
		return _parts[index];
	}
}
