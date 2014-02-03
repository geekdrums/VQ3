/**
	SpriteStudioPlayer
	
	Shader Manager
 
	Copyright(C) 2003-2013 Web Technology Corp. 
	
*/

//#define _BUILD_UNIFIED_SHADERS

using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Flags]
public enum SsShaderType
{
	NonColor	= 0,
	MixColor	= 1,
	MulColor	= 2,
	AddColor	= 3,
	SubColor	= 4,
	ColorMask	= 0x0f,

	AlphaShift	= 4,
	NonAlpha	= (0<<AlphaShift),
	MixAlpha	= (1<<AlphaShift),
	MulAlpha	= (2<<AlphaShift),
	AddAlpha	= (3<<AlphaShift),
	SubAlpha	= (4<<AlphaShift),
	AlphaMask	= (0xf<<AlphaShift),
	
	MatColShift	= 8,
	NonMatColor	= (0<<MatColShift),
	MulMatColor	= (1<<MatColShift),

	Mask		= 0x1ff,
	Num			= (int)SsColorBlendOperation.Num * (int)SsAlphaBlendOperation.Num * (int)SsMaterialColorBlendOperation.Num,	///< num of shader types
}

static public class SsShaderManager
{
	static private	Dictionary<SsShaderType, Shader>	_shaderList = new Dictionary<SsShaderType, Shader>();
	static private	Shader[]	_unifiedShaderList = new Shader[(int)SsAlphaBlendOperation.Num];
	static private	bool	_initialized = false;

	static public void		Initialize()
	{
		if (_initialized) return;
			
		// get parted shaders
		for (int cb = 0; cb < (int)SsColorBlendOperation.Num; ++cb)
		{
			for (int ab = 0; ab < (int)SsAlphaBlendOperation.Num; ++ab)
			{
				for (int mb = 0; mb < (int)SsMaterialColorBlendOperation.Num; ++mb)
				{
					var cbType = (SsColorBlendOperation)cb;
					var abType = (SsAlphaBlendOperation)ab;
					var mbType = (SsMaterialColorBlendOperation)mb;
					SsShaderType key = EnumToType(cbType, abType, mbType);
					string shaderName = "Ss/" + cbType + "Color" + abType + "Alpha";
					// add suffix when material color blending is enabled
					if (mbType != SsMaterialColorBlendOperation.Non)
						shaderName += mbType + "MatCol";
					_shaderList[key] = Shader.Find(shaderName);
					if (_shaderList[key] == null)
						Debug.LogError("not found shader!!: " + shaderName);
				}
			}
		}
#if _BUILD_UNIFIED_SHADERS
		// get color blend unified shaders
		for (int ab = 0; ab < (int)SsAlphaBlendOperation.Num; ++ab)
		{
			var abType = (SsAlphaBlendOperation)ab;
			string shaderName = "Ss/UniColor" + abType + "Alpha";
			_unifiedShaderList[ab] = Shader.Find(shaderName);
			if (_unifiedShaderList[ab] == null)
				Debug.LogError("not found shader!!: " + shaderName);
		}
#endif
		_initialized = true;
	}

	static public	Shader	Get(SsShaderType t, bool unified)
	{
		if (!_initialized)
			Initialize();
		
		if (unified)
		{
			// experimental...
			return _unifiedShaderList[(int)t >> (int)SsShaderType.AlphaShift];
		}
		else
		{
			Shader shader;
			if (_shaderList.TryGetValue(t, out shader))
			{
				return shader;
			}
			else
			{
				// not found, could be not yet supported type...
				return null;
			}
		}
	}
	
	static public	SsShaderType
	EnumToType(SsColorBlendOperation color, SsAlphaBlendOperation alpha, SsMaterialColorBlendOperation matColor)
	{
		return (SsShaderType)( (int)color | ((int)alpha << (int)SsShaderType.AlphaShift) | ((int)matColor << (int)SsShaderType.MatColShift) );
	}

	static public	int
	ToSerial(SsShaderType t)
	{
		int i = (int)t;
		return	(i >> (int)SsShaderType.MatColShift) * ((int)SsAlphaBlendOperation.Num * (int)SsColorBlendOperation.Num)
			+	(i >> (int)SsShaderType.AlphaShift) * (int)SsColorBlendOperation.Num
			+	(i & (int)SsShaderType.ColorMask);
	}
}
