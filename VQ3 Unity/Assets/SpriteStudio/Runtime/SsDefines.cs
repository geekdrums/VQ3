/**
	SpriteStudio
	
	Global defines
	
	Copyright(C) 2003-2013 Web Technology Corp. 
	
*/

using System;


/// part types. (DO NOT CHANGE THIS ORDER!!)
public enum SsPartType
{
	Normal,
	Root,
	Null,
	Bound,
	Sound,

	Num		///< the number of elements
}

/// IDs which inidicates the type of attribute. (DO NOT CHANGE THIS ORDER!!)
public enum SsKeyAttr
{
	PosX,
	PosY,
	Angle,
	ScaleX,
	ScaleY,
	Trans,
	Prio,
	FlipH,
	FlipV,
	Hide,
	PartsCol,
	PartsPal,
	Vertex,
	User,
	Sound,
	ImageOffsetX,
	ImageOffsetY,
	ImageOffsetW,
	ImageOffsetH,
	OriginOffsetX,
	OriginOffsetY,

	Num		///< the number of elements
}

/// Flags indicate IDs which inidicates the type of attribute. (DO NOT CHANGE THIS ORDER!!)
[Flags]
public enum SsKeyAttrFlags
{
//	static public operator bool(SsKeyAttrFlags v) {return v != 0;}
	
	PosX			= (1<<SsKeyAttr.PosX),
	PosY			= (1<<SsKeyAttr.PosY),
	Pos				= PosX|PosY,
	Angle			= (1<<SsKeyAttr.Angle),
	ScaleX			= (1<<SsKeyAttr.ScaleX),
	ScaleY			= (1<<SsKeyAttr.ScaleY),
	Scale			= ScaleX|ScaleY,
	Trans			= (1<<SsKeyAttr.Trans),
	Prio			= (1<<SsKeyAttr.Prio),
	FlipH			= (1<<SsKeyAttr.FlipH),
	FlipV			= (1<<SsKeyAttr.FlipV),
	Flip			= FlipH|FlipV,
	Hide			= (1<<SsKeyAttr.Hide),
	PartsCol		= (1<<SsKeyAttr.PartsCol),
	PartsPal		= (1<<SsKeyAttr.PartsPal),
	Vertex			= (1<<SsKeyAttr.Vertex),
	User			= (1<<SsKeyAttr.User),
	Sound			= (1<<SsKeyAttr.Sound),
	ImageOffsetX	= (1<<SsKeyAttr.ImageOffsetX),
	ImageOffsetY	= (1<<SsKeyAttr.ImageOffsetY),
	ImageOffsetXY	= ImageOffsetX|ImageOffsetY,
	ImageOffsetW	= (1<<SsKeyAttr.ImageOffsetW),
	ImageOffsetH	= (1<<SsKeyAttr.ImageOffsetH),
	ImageOffsetWH	= ImageOffsetW|ImageOffsetH,
	ImageOffset		= ImageOffsetXY|ImageOffsetWH,
	OriginOffsetX	= (1<<SsKeyAttr.OriginOffsetX),
	OriginOffsetY	= (1<<SsKeyAttr.OriginOffsetY),
	OriginOffset	= OriginOffsetX|OriginOffsetY,
	
	AllMask			= (1<<SsKeyAttr.Num)-1
}

/// IDs which inidicates the value type of key. (DO NOT CHANGE THIS ORDER!!)
public enum SsKeyValueType
{
	Data,		///< actually decimal or integer
	Param,		///< actually boolean
	Point,		///< x,y
	Palette,	///< left,top,right,bottom
	Color,		///< single or vertex colors
	Vertex,		///< vertex positions relative to origin
	User,		///< user defined data(numeric|point|rect|string...)
	Sound,		///< sound id, volume, note on...

	Num		///< the number of elements
}

/// IDs which inidicates what the type of values a key has, if SsKeyValueType is "Data".
public enum SsKeyCastType
{
	Int,
	Float,
	Bool,
	Hex,
	Degree,
	Other,

	Num		///< the number of elements
}

/// (DO NOT CHANGE THIS ORDER!!)
public enum SsColorBlendTarget
{
	None,		//#define COLORTYPE_NONE	0
	Overall,	//#define COLORTYPE_PARTS	1
	Vertex,		//#define COLORTYPE_VERTEX	2

	Num		///< the number of elements
}

/// (DO NOT CHANGE THIS ORDER!!)
public enum SsColorBlendOperation
{
	Non,	// actually ssax file has never this value, only used internally.
	Mix,
	Mul,
	Add,
	Sub,

	Num		///< the number of elements
}

/// (DO NOT CHANGE THIS ORDER!!)
public enum SsAlphaBlendOperation
{
	Non,	// actually ssax file has never this value, only used internally.
	Mix,
	Mul,
	Add,
	Sub,

	Num		///< the number of elements
}

/// (DO NOT CHANGE THIS ORDER!!)
public enum SsMaterialColorBlendOperation
{
	Non,
	Mul,

	Num		///< the number of elements
}

/// interpolation types. (DO NOT CHANGE THIS ORDER!!)
public enum SsInterpolationType
{
	None,		//CURVE_NONE = 0,
	Linear,		//CURVE_LINEAR,
	Hermite,	//CURVE_HERMITE,
	Bezier,		//CURVE_BEZIER,

	Num		///< the number of elements
};

//サウンドキー用
[Flags]
public enum SsSoundKeyFlags
{
							//#define SOUNDKEY_USE_NONE		0x0000
	Note		= (1<<0),	//#define SOUNDKEY_USE_NOTE		0x0001
	Volume		= (1<<1),	//#define SOUNDKEY_USE_VOLUME	0x0002
	UserData	= (1<<2),	//#define SOUNDKEY_USE_USERDATA	0x0004
}

/// this type info is contained in part info. (DO NOT CHANGE THIS ORDER!!)
public enum SsInheritanceType
{
	Parent,		///< take the entirety of parent's value.
	Self,		///< use InheritState field to blend values of self and parent.

	Num		///< the number of elements
}

/// IDs what kind of source object is. (DO NOT CHANGE THIS ORDER!!)
public enum SsSourceObjectType
{
	Anim,	//#define SSIO_OBJTYPE_ANIM    0
	Root,	//#define SSIO_OBJTYPE_ROOT    1
	Null,	//#define SSIO_OBJTYPE_NULL    2
//	Bound,	//#define SSIO_OBJTYPE_HITTEST 3
//	Sound,	//#define SSIO_OBJTYPE_SOUND   4

	Num		///< the number of elements
}

public enum SsAnimeFormatVersion
{
	V100  = 0x00010070,	//#define SSA_CURRENT_FORMAT_VERSION_100  0x00010070  // SSA Version 1.70
	V199  = 0x00019900,	//#define SSA_CURRENT_FORMAT_VERSION_199  0x00019900  // SSA Version 1.99.00
	V200  = 0x00020000,	//#define SSA_CURRENT_FORMAT_VERSION_200  0x00020000  // SSA Version 2.00
	V300  = 0x00030000,	//#define SSA_CURRENT_FORMAT_VERSION_300  0x00030000  // SSA Version 3.00
	V315  = 0x00031500,	//#define SSA_CURRENT_FORMAT_VERSION_315  0x00031500  // SSA Version 3.15
	V320  = 0x00032000,	//#define SSA_CURRENT_FORMAT_VERSION_320  0x00032000  // SSA Version 3.20
	V332  = 0x00033200,	//#define SSA_CURRENT_FORMAT_VERSION_332  0x00033200  // SSA Version 3.32
	V400  = 0x00040000,	//#define SSA_CURRENT_FORMAT_VERSION_332  0x00033200  // SSA Version 4.00
}

public enum SsSceneFormatVersion
{
	V300  = 0x00030000,	//#define SSS_CURRENT_FORMAT_VERSION_300  0x00030000  // SSS Version 3.00
	V315  = 0x00031500,	//#define SSS_CURRENT_FORMAT_VERSION_315  0x00031500  // SSS Version 3.15
	V320  = 0x00032000,	//#define SSS_CURRENT_FORMAT_VERSION_320  0x00032000  // SSS Version 3.20
	V332  = 0x00033200,	//#define SSS_CURRENT_FORMAT_VERSION_332  0x00033200  // SSA Version 3.32  Ver.3 Final
}
