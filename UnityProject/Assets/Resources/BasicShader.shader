Shader "Custom/BasicShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap("Normal", 2D) = "blue" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _LVM ("LVM", 2D) = "black" {}
        _LVMColor ("LVM Color", 2D) = "black" {}
        _AO ("AO", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma multi_compile __ _UV_DEBUG_0 _UV_DEBUG_1 _UV_DEBUG_2 _UV_DEBUG_3 _UV_DEBUG_4
        #pragma multi_compile __ _VERTEX_COLOR_DEBUG
        #pragma multi_compile __ _NORMAL_DEBUG
        #pragma multi_compile __ _ENABLE_LVM_DEBUG

        #pragma surface surf Standard fullforwardshadows

        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _LVM;
        sampler2D _LVMColor;

        float _BakedAOStrength;
        float _BakedShadowStrength;
        float _IndirectStrength;
        float _DirectStrength;

        float _LVMColorDebug;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
			float4 color : COLOR;
            float2 uv1_MainTex;
            float2 uv2_LVM;
            float2 uv3_LVMColor;
            float2 uv4_LVMColor;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        half _AO;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

            fixed4 n = tex2D (_BumpMap, IN.uv_BumpMap);
            n.xz = n.zx;
            o.Normal = UnpackNormal(n);

            fixed2 lvmColorDirectUV = fixed2(IN.uv2_LVM.x * .5f, IN.uv2_LVM.y);
            fixed4 tmpLvmDirect = tex2D(_LVMColor, lvmColorDirectUV);
            fixed4 lvmDirectCol = tmpLvmDirect * fixed4(1, 1, 1, 0) * _DirectStrength;
            fixed4 lvmAOCol = tmpLvmDirect.a + (1 - _BakedAOStrength * _AO);

            fixed2 lvmColorIndirectUV = fixed2((IN.uv2_LVM.x * .5f) + .5f, IN.uv2_LVM.y);
            fixed4 tmpLvmIndirect = tex2D(_LVMColor, lvmColorIndirectUV);
            fixed4 lvmIndirect = tmpLvmIndirect * fixed4(1, 1, 1, 0) * _IndirectStrength;
            fixed4 lvmBakedShadow = tmpLvmIndirect.a * _BakedShadowStrength;

            o.Occlusion = lvmAOCol.r;

            fixed4 finalLvmColor = lvmDirectCol + lvmIndirect + lvmBakedShadow;

            o.Albedo += finalLvmColor;

            fixed3 colorLerp = lerp(o.Albedo, finalLvmColor.rgb, _LVMColorDebug);

            o.Albedo = colorLerp;

#if _ENABLE_LVM_DEBUG
            o.Albedo = tex2D(_LVM, IN.uv2_LVM);
#endif
#if _UV_DEBUG_0
			o.Albedo = float4(frac(IN.uv_MainTex.x), frac(IN.uv_MainTex.y), 0, 0);
#endif
#if _UV_DEBUG_1
            o.Albedo = float4(frac(IN.uv1_MainTex.x), frac(IN.uv1_MainTex.y), 0, 0);
#endif
#if _UV_DEBUG_2
            o.Albedo = float4(frac(IN.uv2_LVM.x), frac(IN.uv2_LVM.y), 0, 0);
#endif
#if _UV_DEBUG_3
            o.Albedo = float4(frac(IN.uv3_LVMColor.x), frac(IN.uv3_LVMColor.y), 0, 0);
#endif
#if _UV_DEBUG_4
            o.Albedo = float4(frac(IN.uv4_LVMColor.x), frac(IN.uv4_LVMColor.y), 0, 0);
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
        }
        ENDCG
    }
    FallBack "Diffuse"
}
