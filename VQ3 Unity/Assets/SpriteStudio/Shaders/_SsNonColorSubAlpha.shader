Shader "Ss/NonColorSubAlpha"
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
		
		Blend DstColor SrcAlpha
		AlphaTest NotEqual 0
		BlendOp RevSub
		


		// simple fixed function.
		SubShader {
			Pass {
				SetTexture [_MainTex] {combine texture, texture * primary}
				
				
				
			}
		} 
	}
	//Fallback "Ss/NonColorMixAlpha"
}
