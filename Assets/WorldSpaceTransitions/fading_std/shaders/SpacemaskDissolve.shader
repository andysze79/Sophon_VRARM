Shader "Spacemask/Surface/Dissolve" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		[HDR]_EmissionColor("Color", Color) = (0,0,0)
		_EmissionMap("Emission", 2D) = "white" {}
		_Color2("Color2", Color) = (1,1,1,1)
		_MainTex2("Albedo2 (RGB)", 2D) = "white" {}
		_Glossiness2("Smoothness2", Range(0,1)) = 0.5
		_Metallic2("Metallic2", Range(0,1)) = 0.0
		_BumpMap2("Normalmap", 2D) = "bump" {}
		[HDR]_EmissionColor2("Color2", Color) = (0,0,0)
		_EmissionMap2("Emission2", 2D) = "white" {}

		[Toggle] _inverse("inverse", Float) = 0

		[HideInInspector][Toggle(DISSOLVE)] _dissolve("dissolveTexture", Float) = 1
	}
	SubShader 
	{
		Tags { "Queue" = "Geometry" "RenderType"="Clipping" }
		LOD 200

		// ------------------------------------------------------------------

				
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard addshadow
		#pragma multi_compile __ FADE_PLANE FADE_SPHERE FADE_SPHERES

		#pragma shader_feature DISSOLVE
		#pragma multi_compile __ NOISETRIPLANAR
		#include "CGIncludes/section_clipping_CS.cginc"

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _EmissionMap;
		sampler2D _MainTex2;
		sampler2D _EmissionMap2;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
			#if NOISETRIPLANAR
			float3 worldNormal;
			#endif
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _EmissionColor;
		half _Glossiness2;
		half _Metallic2;
		fixed4 _Color2;
		fixed4 _EmissionColor2;

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			bool _masked_out = false;
			#if (FADE_PLANE || FADE_SPHERE || FADE_SPHERES)
			#if NOISETRIPLANAR
			_masked_out = OUT_MASKED(IN.worldPos, IN.worldNormal);
			#else
			_masked_out = OUT_MASKED(IN.worldPos);
			#endif
			#endif
			// Albedo comes from a texture tinted by color
			fixed4 c = _masked_out? (tex2D (_MainTex2, IN.uv_MainTex) * _Color2) : (tex2D(_MainTex, IN.uv_MainTex) * _Color);
			o.Albedo = c.rgb;
			
			// Metallic and smoothness come from slider variables
			o.Metallic = _masked_out ? _Metallic2 :_Metallic;
			o.Smoothness = _masked_out ? _Glossiness2 :_Glossiness;
			o.Emission = _masked_out ? _EmissionColor2 : _EmissionColor;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
