Shader "Ss/NonColorNonAlphaMulMatCol"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		
	}

	Category
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off
		Lighting Off
		Cull Off
		ColorMaterial AmbientAndDiffuse
				Material {
			Diffuse [_Color]
			Ambient [_Color]
		}
		Blend Off
		
		


		// simple fixed function.
		SubShader {
			Pass {
				SetTexture [_MainTex] {combine texture, texture * primary}
				
				SetTexture [_MainTex] {constantColor [_Color] combine previous * constant DOUBLE, previous * constant}
				
			}
		} 
	}
	//Fallback "Ss/NonColorMixAlpha"
}
