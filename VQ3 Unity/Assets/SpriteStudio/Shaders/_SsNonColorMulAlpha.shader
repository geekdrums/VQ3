Shader "Ss/NonColorMulAlpha"
{
	Properties
	{
		
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		
	}

	Category
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off
		Lighting Off
		Cull Off
		ColorMaterial AmbientAndDiffuse
		
		Blend DstColor OneMinusSrcAlpha
		AlphaTest NotEqual 0
		
		


		// simple fixed function.
		SubShader {
			Pass {
				SetTexture [_MainTex] {combine texture, texture * primary}
				
				
				SetTexture [_MainTex] {combine previous * primary alpha, previous}
			}
		} 
	}
	//Fallback "Ss/NonColorMixAlpha"
}
