// These shaders are not referenced directly but via UsePass command.
Shader "Ss/Unified"
{
	Properties
	{
//		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_OneColor ("Constant Color(1,1,1,1)", Color) = (1,1,1,1)
	}

	Category
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMaterial AmbientAndDiffuse
		Lighting Off
		Cull Off
		Material {
			Diffuse [_Color]
			Ambient [_Color]
		}
		// Unified color blending
		SubShader {
			Pass {
				Name	"ColorBlend"
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
				// [0]: 1~5bit color blend type	-> v2f.extras[0]
				// 		6~9bit alpha blend type	-> v2f.extras[1]
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
					half4	extras	: TEXCOORD1;		// values from texcoord1 field.
				};
				
				static const fixed4 ONE_COLOR = {1,1,1,1};
				
				v2f vert(appdata_ss v)
				{
					v2f o;
					o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
					o.uv = v.texcoord;

					// get color blend type
					fixed colorBlend = clamp(v.texcoord1[0], 0, 0x0f);
					if (colorBlend == 0)
					{
						// none
						o.color = ONE_COLOR;
					}
					else
					{
						// all of others
						o.color.rgb = v.color.rgb;
					}
					
					// get alpha blend type
//					int alphaBlend = (int)v.texcoord1[0] / 0x10;
					int alphaBlend = (int)(v.texcoord1.x / 16.0);
					
					// pass alpha
					o.color.a = v.texcoord1[1];
					
					// pass extra info
					o.extras[0] = colorBlend;
					o.extras[1] = alphaBlend;
					o.extras[2] = v.color.a;	// color blend rate
					o.extras[3] = 0.0f;			// No use

					// Currently Blend* statements need CgFx support.
					// set alpha blend type
					//AlphaBlending = true;
					/*
					if (alphaBlend == 0)
					{
						// none
						BlendOp = 
					}
					if (alphaBlend == 1)
					{
						// mix
						BlendOp = int(FuncAdd);
						//BlendFunc = int2(Zero, One);
					}
					else if (alphaBlend == 2)
					{
						// mul
						BlendOp = int(FuncMul);
					}
					else if (alphaBlend == 3)
					{
						// add
						BlendOp = int(Add);
					}
					else if (alphaBlend == 4)
					{
						// sub
						BlendOp = int(Subtract);
					}
					*/

					return o;
				}
				
				half4 frag(v2f i) : COLOR
				{
					fixed4 col = i.color;
					fixed4 tex = tex2D(_MainTex, i.uv);

					half	colorBlend = i.extras[0];
					half	alphaBlend = i.extras[1];
					fixed	rate = i.extras[2];

					// color blend
					if (colorBlend == 1)
					{
						// mix
						col.rgb = col.rgb * rate + tex.rgb * (1 - rate);
					}
					else if (colorBlend == 2)
					{
						// mul
						col.rgb = col.rgb * rate + tex.rgb * (1 - rate);
						col.rgb = tex.rgb;
					}
					else if (colorBlend == 3)
					{
						// add
						col.rgb = (col.rgb * rate) + tex.rgb;
					}
					else if (colorBlend == 4)
					{
						// sub
						col.rgb = tex.rgb - (col.rgb * rate);
					}
					else
					{
						// none
						col.rgb = tex.rgb;
					}
					
					if (alphaBlend == 2)
					{
						// mul
						col.rgb *= col.a;
					}
					
					// mix alpha
					col.a *= tex.a;
					
					// blend material color
// 					col.rgb *= _Color.rgb * 2;
// 					col.a *= _Color.a;

					return col;
				}
				ENDCG
			}
		}
		// simple texture + material color combiner
		SubShader {
			Pass {
				Name	"Simple"
				SetTexture [_MainTex] {combine texture, texture}
// 				SetTexture [_MainTex] {
// 					constantColor [_Color]
// 					combine previous * constant DOUBLE, previous * constant
// 				}
			}
		}
	}
	// Each color blending
	Category
	{
		// No color blend
		SubShader {
			Pass {
				Name	"NonColor"
				SetTexture [_MainTex] {combine texture, texture}
			}
		}
		// Mix color
		SubShader {
			Pass {
				Name	"MixColor"
				SetTexture [_MainTex] {combine primary lerp(primary) texture, texture}
			}
		} 
		// Mul color
		SubShader {
			Pass {
				Name	"MulColor"
				SetTexture [_MainTex] {
					ConstantColor [_OneColor]
					combine primary lerp (primary) constant, texture
				}
				SetTexture [_MainTex] {combine previous * texture, texture}
			}
		} 
		// Add color
		SubShader {
			Pass {
				Name	"AddColor"
				SetTexture [_MainTex] {combine primary * primary alpha}
				SetTexture [_MainTex] {combine previous + texture, texture}
			}
		} 
		// Sub color
		SubShader {
			Pass {
				Name	"SubColor"
				SetTexture [_MainTex] {combine primary * primary alpha}
				SetTexture [_MainTex] {combine texture - previous, texture}
			}
		} 
	}
	// Each alpha blending
	Category
	{
		// Alpha blend: Non
		SubShader {
			Pass {
				Name	"NonAlpha"
				Blend	Off
			}
		}
		// Alpha blend: Mix
		SubShader {
			Pass {
				Name	"MixAlpha"
				Blend	SrcAlpha OneMinusSrcAlpha
			}
		}
		// Alpha blend: Mul
		SubShader {
			Pass {
				Name	"MulAlpha"
				Blend	SrcAlpha OneMinusSrcAlpha
			}
		}
		// Alpha blend: Add
		SubShader {
			Pass {
				Name	"AddAlpha"
				Blend	SrcAlpha One
			}
		}
		// Alpha blend: Sub
		// this is not equivalent to the appearance in SpriteStudio.
		// because subtraction is not supported and the Color doesn't accept minus value.
		// CORRECT:		dst.rgb - src.rgb * src.a
		// SUBSTITUION:	dst.rgb + (1 - src.rgb) * src.a
		SubShader {
			Pass {
				Name	"SubAlpha"
				Blend	OneMinusSrcColor OneMinusSrcAlpha
				AlphaTest NotEqual 0
			}
		}
	}
}
