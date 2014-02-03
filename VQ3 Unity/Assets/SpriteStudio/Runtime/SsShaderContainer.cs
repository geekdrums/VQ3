/**
	SpriteStudioPlayer

	Shader container
	
	Copyright(C) 2003-2013 Web Technology Corp. 
	
*/

using UnityEngine;
using System.Collections.Generic;

// shader container asset. this is just for saving the all of generated shaders into published data.
[System.Serializable]
public class SsShaderContainer : MonoBehaviour
{
	public	List<Shader>	_shaders = new List<Shader>();
}
