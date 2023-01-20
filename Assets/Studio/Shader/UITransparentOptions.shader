Shader "UI/Transparent Options"
{
	Properties
	{
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
		_Mask ("Mask Alpha (A)", 2D) = "black" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Intensity ("Intensity", Range(0,10)) = 1
		_Cutout ("Cutout", Range(0, 1)) = 0
		[Enum(Red,0,Green,1,Blue,2,Alpha,3)] _MaskChannel ("MaskChannel", Int) = 1
		[IntRange] _Stencil ("Stencil Id", Range(0, 255)) = 0
		[IntRange] _StencilReadMask ("StencilReadMask", Range(0, 255)) = 255
		[IntRange] _StencilWriteMask ("StencilWriteMask", Range(0, 255)) = 255
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("StencilComp", Int) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("StencilOp", Int) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Stencil Fail", Int) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("Stencil ZFail", Int) = 0
		[Enum(Nothing,0,Alpha,1,Blue,2,Green,4,Red,8,All,15)] _ColorMask("Color Mask", Int) = 15
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Culling", Int) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Int) = 4
		[Enum(Off,0,On,1)] _ZWrite("ZWrite", Int) = 0
		[Enum(UnityEngine.Rendering.BlendOp)] _BlendOp ("Blending Op", Int) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Blending Source", Int) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Blending Dest", Int) = 10
		[KeywordEnum(Add, Overlay)] _Tint ("Tint Method", Int) = 0
		_TintColor ("Tint Color", Color) = (0,0,0,0)
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Int) = 0
		[Toggle(GRAYSCALE)] _Grayscale ("Grayscale", Int) = 0
		_Saturate ("Saturate", Range(0,1)) = 0
	}
	
	SubShader
	{
		LOD 200

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		Pass
		{
			Lighting Off
			ColorMask [_ColorMask]
			Stencil {
				Ref [_Stencil]
				ReadMask [_StencilReadMask]
				WriteMask [_StencilWriteMask]
				Comp [_StencilComp]
				Pass [_StencilOp]
				Fail [_StencilFail]
				ZFail [_StencilZFail]
			}
			Cull [_CullMode]
			ZTest [_ZTest]
			ZWrite [_ZWrite]
			BlendOp [_BlendOp]
			Blend [_SrcBlend] [_DstBlend]
			Lighting Off
			Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_local _ GRAYSCALE
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP
			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma shader_feature_local _TINT_ADD _TINT_OVERLAY
			#pragma shader_feature_local MASK
			#pragma enable_cbuffer
			
			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			CBUFFER_START(UnityPerMaterial)
			sampler2D _MainTex;
			half4 _MainTex_ST;
			sampler2D _Mask;
			half4 _Mask_ST;
			fixed4 _Color;
			fixed4 _TintColor;
			fixed _Intensity;
			fixed _Saturate;
			fixed _Cutout;
			fixed _MaskChannel;
			CBUFFER_END
			
			float4 _ClipRect;
			
			struct appdata_t
			{
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};
	
			struct v2f
			{
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				#if MASK
				half2 texcoord1 : TEXCOORD2;
				#endif
				fixed4 color : COLOR;
			};
	
			v2f vert (appdata_t v)
			{
				v2f o;
				o.worldPosition = v.vertex;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				#if MASK
				o.texcoord1 = TRANSFORM_TEX(v.texcoord, _Mask);
				#endif
				o.color = v.color * _Color * _Intensity;
				return o;
			}
			
			fixed4 frag (v2f IN) : COLOR
			{
				fixed4 col = tex2D(_MainTex, IN.texcoord) * IN.color;
				#if GRAYSCALE
				col.rgb = lerp(dot(col.rgb, fixed3(0.3, 0.59, 0.11)), col.rgb, _Saturate);
				#endif

				#if MASK
				fixed4 mask = tex2D(_Mask, IN.texcoord1);
				col.a *= mask[_MaskChannel];
				#endif
				
				#if _TINT_ADD
				col += _TintColor;
				#elif _TINT_OVERLAY
				col.rgb = (_TintColor.rgb * _TintColor.a) + (1 - _TintColor.a) * col.rgb;
				#endif
				
				#if UNITY_UI_CLIP_RECT
				col.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				#endif
				#if UNITY_UI_ALPHACLIP
				clip(col.a - _Cutout);
				#endif
				return col;
			}
			ENDCG
		}
	}
	
	SubShader
	{
		LOD 100

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		Pass
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }
			Offset -1, -1
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMaterial AmbientAndDiffuse
			
			SetTexture [_MainTex]
			{
				Combine Texture * Primary
			}
		}
	}
	CustomEditor "Studio.Framework.TransparentFxGUI"
}
