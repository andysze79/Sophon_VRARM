Shader "Spacemask/Surface/Transparent"
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
		[HDR]_EmissionColor2("Color2", Color) = (0,0,0)
		_EmissionMap2("Emission2", 2D) = "white" {}
		//_spread ("fadeSpan", Range(0,1)) = 1.0
		[Toggle] _inverse("inverse", Float) = 0
		[Toggle(RIPPLES)] _rings("ripples", Float) = 0
		//_n_rings ("n_rings", Range(0,32)) = 1.0
		//_ringOffset("ringOffset", Range(0,1)) = 1.0

    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType"="Clipping" "IsEmissive" = "true"}
        LOD 200
		//Cull Off
   
        CGPROGRAM
 
        #pragma surface surf Standard fullforwardshadows alpha:fade
		//make custom standard shader pass to get decent shadows
		#pragma multi_compile __ FADE_PLANE FADE_SPHERE FADE_SPHERES
		#pragma multi_compile __ RINGS
		#pragma multi_compile __ RIPPLES
		#include "CGIncludes/section_clipping_CS.cginc"

        #pragma target 3.0
 
        sampler2D _MainTex;
		sampler2D _EmissionMap;
		sampler2D _MainTex2;
		sampler2D _EmissionMap2;
 
        struct Input {
            float2 uv_MainTex;
			float3 worldPos;
        };
 
        half _Glossiness;
        half _Metallic;
		fixed4 _EmissionColor;
        fixed4 _Color;
		half _Glossiness2;
		half _Metallic2;
		fixed4 _EmissionColor2;
		fixed4 _Color2;
 
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 em = tex2D(_EmissionMap, IN.uv_MainTex) * _EmissionColor;
		#if FADE_PLANE || FADE_SPHERE || FADE_SPHERES
			float4 fade = PLANE_FADE(IN.worldPos);
			float transp = fade.a;
			if(_inverse==1) transp = 1 - transp;

			fixed4 c2 = tex2D(_MainTex2, IN.uv_MainTex) * _Color2;
			fixed4 em2 = tex2D(_EmissionMap2, IN.uv_MainTex) * _EmissionColor2;

			o.Alpha = c.a*transp + c2.a*(1 - transp);
			o.Albedo = (c.rgb*c.a*transp + c2.rgb*c2.a*(1 - transp)) / o.Alpha;
			o.Emission = (em.rgb*c.a*transp + em2.rgb*c2.a*(1 - transp)) / o.Alpha;
			if (o.Alpha == 0) o.Albedo = fixed3(0, 0, 0);
			o.Metallic = transp*_Metallic + (1 - transp)*_Metallic2;
			o.Smoothness = transp * _Glossiness + (1 - transp)*_Glossiness;
			#if RIPPLES
			o.Normal.y += transp;
			#endif
		#else
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Emission = em;
			o.Alpha = c.a;
		#endif
        }

        ENDCG
    }
    FallBack "Standard"
}
