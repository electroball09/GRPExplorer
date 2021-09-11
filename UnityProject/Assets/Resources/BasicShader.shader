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
        #include "AutoLight.cginc"

        #pragma multi_compile __ _UV_DEBUG_1 _UV_DEBUG_2 _UV_DEBUG_3 _UV_DEBUG_4
        #pragma multi_compile __ _VERTEX_COLOR_DEBUG
        #pragma multi_compile __ _ENABLE_LVM_DEBUG

        #pragma surface surf NoLighting fullforwardshadows// noambient novertexlights noforwardadd

        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _LVM;
        sampler2D _LVMColor;

        float _LVMColorContribution;

        float _BakedAOStrength;
        float _BakedShadowStrength;
        float _IndirectStrength;
        float _DirectStrength;

        fixed4 _AmbientColor;
        float _AmbientStrength;

        float _DebugLVMFade;

        float _SpecularPower;

        float _LVM_R;
        float _LVM_G;
        float _LVM_B;
        float _LVM_A;

        fixed4 _DirectionalLightColor;

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

        fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
        {
            fixed3 ndotl = saturate(dot(lightDir, s.Normal));
            fixed3 color = (s.Albedo * _AmbientColor.rgb * _AmbientStrength) + s.Albedo * ndotl;
            fixed3 finalLerp = lerp(color, s.Albedo, _LVMColorContribution * (1 - _DebugLVMFade));
            return fixed4(finalLerp, s.Alpha);
        }

        void surf (Input IN, inout SurfaceOutput o)
        {

#if _UV_DEBUG_1
            o.Albedo = float4(frac(IN.uv1_MainTex.x), frac(IN.uv1_MainTex.y), 0, 0);
            return;
#endif
#if _UV_DEBUG_2
            o.Albedo = float4(frac(IN.uv2_LVM.x), frac(IN.uv2_LVM.y), 0, 0);
            return;
#endif
#if _UV_DEBUG_3
            o.Albedo = float4(frac(IN.uv3_LVMColor.x), frac(IN.uv3_LVMColor.y), 0, 0);
            return;
#endif
#if _UV_DEBUG_4
            o.Albedo = float4(frac(IN.uv4_LVMColor.x), frac(IN.uv4_LVMColor.y), 0, 0);
            return;
#endif
#if _VERTEX_COLOR_DEBUG
            o.Albedo = IN.color;
            return;
#endif

#if _ENABLE_LVM_DEBUG
            fixed4 lvm = tex2D(_LVM, IN.uv2_LVM);
            fixed4 lvmFinal = lvm * fixed4(_LVM_R, _LVM_G, _LVM_B, 0);
            fixed4 lvmA = lvm.a;
            lvmFinal = lerp(lvmFinal, lvmA, _LVM_A);
            o.Albedo = lvmFinal;
            return;
#endif

            o.Specular = _SpecularPower;

            fixed4 texColor = tex2D (_MainTex, IN.uv_MainTex) * _Color;

            fixed4 n = tex2D (_BumpMap, IN.uv_BumpMap);
            n.xz = n.zx;
            o.Normal = UnpackNormal(n);

            fixed2 lvmColorDirectUV = fixed2(IN.uv2_LVM.x * .5f, IN.uv2_LVM.y);
            fixed4 tmpLvmDirect = tex2D(_LVMColor, lvmColorDirectUV);
            fixed4 lvmDirectCol = tmpLvmDirect * fixed4(1, 1, 1, 0) * _DirectStrength;
            fixed4 lvmAOCol = tmpLvmDirect.a + (1 - _BakedAOStrength * _AO);

            fixed2 lvmColorIndirectUV = fixed2((IN.uv2_LVM.x * .5f) + .5f, IN.uv2_LVM.y);
            fixed4 tmpLvmIndirect = tex2D(_LVMColor, lvmColorIndirectUV);
            fixed4 lvmIndirect = tmpLvmIndirect * fixed4(1, 1, 1, 0) * _IndirectStrength * lvmAOCol.r;
            fixed4 lvmBakedShadow = tmpLvmIndirect.a * _DirectionalLightColor * _BakedShadowStrength;

            fixed4 finalLvmColor = lvmDirectCol + lvmIndirect + lvmBakedShadow;
            fixed3 ambientColor = texColor.rgb * lvmAOCol.r * _AmbientStrength * _AmbientColor.rgb;
            fixed3 litColor = texColor.rgb * finalLvmColor;

            fixed3 finalColor = ambientColor + litColor;

            fixed3 finalLvmLerp = lerp(texColor, finalColor, _LVMColorContribution);

            o.Albedo = finalLvmLerp;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
