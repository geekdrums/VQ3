Shader "Ss/AddColorSubAlphaMulMatCol"
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
		Blend DstColor SrcAlpha
		AlphaTest NotEqual 0
		BlendOp RevSub
		
		// make possible to treat both alpha value and color blending ratio simultaneously.
		SubShader {
			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
			
				sampler2D _MainTex;
				float4 _Color;
				
				// color.a is not an alpha, it means the effect extent of color blending.
				// pass to v2f.extras[2]
				// 
				// use TEXCOORD1 values as extra infomations for:
				// [0]: 0~3bit color blend type	-> v2f.extras[0]	this is unused in this version.
				// 		4~7bit alpha blend type	-> v2f.extras[1]	(ditto)
				// [1]: alpha value				-> v2f.color.a
				struct appdata_ss {
					float4 vertex		: POSITION;
					float2 texcoord		: TEXCOORD0;
					fixed4 color		: COLOR0;
					fixed2 texcoord1	: TEXCOORD1;	// extra infos
				};
				struct v2f {
					float4	pos		: SV_POSITION;
					float2	uv		: TEXCOORD;
					fixed4	color	: COLOR0;
					half4	colorBlendRate : TEXCOORD1;	// Yuzu.	// values from texcoord1 field.
				};
				
				static const fixed4 ONE_COLOR = {1,1,1,1};

				v2f vert(appdata_ss v)
				{
					v2f o;
					o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
					o.uv = v.texcoord;

					// pass vertex rgb color
					o.color.rgb = v.color.rgb;
					
					// pass alpha
					o.color.a = v.texcoord1[1];
					
					// pass extra info
					o.colorBlendRate = v.color;	// Yuzu.	// color blend rate

					return o;
				}

				half4 frag(v2f i) : COLOR
				{
					fixed4	col = i.color;
					fixed4	tex = tex2D(_MainTex, i.uv);
					fixed	rate = i.colorBlendRate.a;	// Yuzu.

					// color blend function which set at import. 
					col.rgb = (col.rgb * rate) + tex.rgb;

					// mix alpha
					col.a *= tex.a;
					
					col.rgb *= _Color.rgb * 2;
col.a *= _Color.a;
					
					
					return col;
				}
				ENDCG
			}
		}


		// simple fixed function.
		SubShader {
			Pass {
				SetTexture [_MainTex] {combine primary * primary alpha}
				SetTexture [_MainTex] {combine previous + texture, texture}
				SetTexture [_MainTex] {constantColor [_Color] combine previous * constant DOUBLE, previous * constant}
				
			}
		} 
	}
	//Fallback "Ss/NonColorMixAlpha"
}
