Shader "Custom/BasicShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap("Normal", 2D) = "blue" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
		#pragma multi_compile __ _UV_DEBUG_0 _UV_DEBUG_1 _UV_DEBUG_2 _UV_DEBUG_3 _UV_DEBUG_4
		#pragma multi_compile __ _VERTEX_COLOR_DEBUG
        #pragma multi_compile __ _NORMAL_DEBUG

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
			float4 color : COLOR;
            float2 uv1_MainTex;
            float2 uv2_MainTex;
            float2 uv3_MainTex;
            float2 uv4_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

            fixed4 n = tex2D (_BumpMap, IN.uv_BumpMap);
            //o.Normal = UnpackNormal(n);
            //o.Normal = n;

#if _UV_DEBUG_0
			o.Albedo = float4(frac(IN.uv_MainTex.x), frac(IN.uv_MainTex.y), 0, 0);
#endif
#if _UV_DEBUG_1
            o.Albedo = float4(frac(IN.uv1_MainTex.x), frac(IN.uv1_MainTex.y), 0, 0);
#endif
#if _UV_DEBUG_2
            o.Albedo = float4(frac(IN.uv2_MainTex.x), frac(IN.uv2_MainTex.y), 0, 0);
#endif
#if _UV_DEBUG_3
            o.Albedo = float4(frac(IN.uv3_MainTex.x), frac(IN.uv3_MainTex.y), 0, 0);
#endif
#if _UV_DEBUG_4
            o.Albedo = float4(frac(IN.uv4_MainTex.x), frac(IN.uv4_MainTex.y), 0, 0);
#endif
#if _VERTEX_COLOR_DEBUG
			o.Albedo = IN.color;
#endif
#if _NORMAL_DEBUG
            o.Albedo = (o.Normal + 1) / 2;
#endif

            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
