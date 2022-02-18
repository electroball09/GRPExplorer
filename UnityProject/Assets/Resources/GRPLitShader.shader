Shader "GRP/Lit Basic"
{
    Properties
    {
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _BumpMap("Normal", 2D) = "blue" {}
        _LVM("LVM", 2D) = "black" {}
        _LVMColor("LVM Color", 2D) = "black" {}
    }
    SubShader
    {
        Pass
        {
            Tags { "Queue" = "Geometry" "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma multi_compile __ SHADOWS_SCREEN
            #pragma multi_compile __ _DEBUG_VIEW

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _BumpMap;
            float4 _BumpMap_ST;

            sampler2D _LVM;
            float4 _LVM_ST;
            sampler2D _LVMColor;
            float4 _LVMColor_ST;

            float _LVMColorContribution;

            float _BakedAOStrength;
            float _BakedShadowStrength;
            float _IndirectStrength;
            float _DirectStrength;

            fixed4 _AmbientColor;
            float _AmbientStrength;

            samplerCUBE _Cubemap;

            fixed4 _DirectionalLightColor;
            float _DirectionalLightIntensity;
            float _DirectionalLightEnabled;

            float _DEBUG_NORMAL;
            float _DEBUG_UV0;
            float _DEBUG_UV1;
            float _DEBUG_UV2;
            float _DEBUG_UV3;
            float _DEBUG_UV4;
            float _DEBUG_VERTEX_COLOR;
            float _DebugR;
            float _DebugG;
            float _DebugB;
            float _DebugA;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
                float4 tangent : TANGENT;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float2 uv3 : TEXCOORD3;
                float2 uv4 : TEXCOORD4;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float2 uv3 : TEXCOORD3;
                float2 uv4 : TEXCOORD4;
                SHADOW_COORDS(5)
                float3 worldRefl : TEXCOORD6;
                float3 lightDir : TEXCOORD7;
            };

            v2f vert (appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = v.normal;
                o.color = v.color;

                o.uv0 = TRANSFORM_TEX(v.uv0, _MainTex);
                o.uv1 = TRANSFORM_TEX(v.uv1, _LVMColor);
                o.uv2 = TRANSFORM_TEX(v.uv2, _LVM);
                o.uv3 = TRANSFORM_TEX(v.uv3, _LVMColor);
                o.uv4 = TRANSFORM_TEX(v.uv4, _LVMColor);

                TRANSFER_SHADOW(o);

                fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;

                o.worldRefl = reflect(-worldViewDir, worldNormal);

                float3x3 worldToTangent = float3x3(worldTangent, worldBinormal, worldNormal);

                // Transform the light and view dir from world space to tangent space
                o.lightDir = mul(worldToTangent, WorldSpaceLightDir(v.vertex));

                return o;
            }

            float3 DecodeNormal(float4 norm)
            {
                float3 decoded = norm.xyz * 2 - 1;
                decoded.xz = decoded.zx;
                decoded.z = sqrt(1 - saturate(dot(decoded.xy, decoded.xy)));
                return normalize(decoded);
            }

            fixed4 frag(v2f i) : SV_Target
            {
#if defined(_DEBUG_VIEW)
                fixed4 col = 0;
                col.xy += i.uv0 * _DEBUG_UV0;
                col.xy += i.uv1 * _DEBUG_UV1;
                col.xy += i.uv2 * _DEBUG_UV2;
                col.xy += i.uv3 * _DEBUG_UV3;
                col.xy += i.uv4 * _DEBUG_UV4;
                col.xyz += (i.normal + 1) / 2 * _DEBUG_NORMAL;
                col.xyz += i.color.rgb * _DEBUG_VERTEX_COLOR;
                col.rgb *= float3(_DebugR, _DebugG, _DebugB) + col.a * _DebugA;
                return col;
#endif

                //return float4(i.lightDir, 1);
                //return tex2D(_LVM, i.uv1);

                //return float4(i.lightDir, 1);
                fixed4 texColor = tex2D(_MainTex, i.uv0);
                float3 tangentNormal = DecodeNormal(tex2D(_BumpMap, i.uv0));
                //return fixed4(tangentNormal, 1);
                float dirLight = max(0, dot(tangentNormal, normalize(i.lightDir)));
                dirLight = lerp(1, dirLight, _DirectionalLightEnabled);
                //return float4(dirLight, 1);

                fixed2 lvmUv = i.uv1;

                fixed2 lvmColorDirectUV = fixed2(lvmUv.x * .5f, lvmUv.y);
                fixed4 tmpLvmDirect = tex2D(_LVMColor, lvmColorDirectUV);
                fixed4 lvmDirectCol = tmpLvmDirect * fixed4(1, 1, 1, 0) * _DirectStrength;
                fixed4 lvmAOCol = lerp(1, tmpLvmDirect.a, _LVMColorContribution * _BakedAOStrength);

                fixed2 lvmColorIndirectUV = fixed2((lvmUv.x * .5f) + .5f, lvmUv.y);
                fixed4 tmpLvmIndirect = tex2D(_LVMColor, lvmColorIndirectUV);
                fixed4 lvmIndirect = tmpLvmIndirect * fixed4(1, 1, 1, 0) * _IndirectStrength * lvmAOCol.r;
                fixed4 lvmBakedShadow = lerp(1, tmpLvmIndirect.a, _BakedShadowStrength);

                float attenuation = max(lvmBakedShadow, 1 - _LVMColorContribution);
                //return attenuation;
#if defined(SHADOWS_SCREEN)
                attenuation = min(SHADOW_ATTENUATION(i), attenuation);
#endif
                //return attenuation;

                fixed4 ambientCube = texCUBE(_Cubemap, i.worldRefl);
                //return ambientCube;

                fixed4 lvmAggregate = lvmDirectCol + lvmIndirect;
                fixed3 ambientColor = texColor.rgb * _AmbientColor.rgb * lvmAOCol.r * _AmbientStrength;
                //return fixed4(ambientColor, 1);
                fixed3 dirLightColor = attenuation * dirLight * _DirectionalLightColor * _DirectionalLightIntensity;
                //return fixed4(dirLightColor, 1);
                fixed3 litColor = texColor.rgb * (lvmAggregate + dirLightColor);
                //return fixed4(litColor, 1);

                fixed3 finalColor = ambientColor + litColor;
                fixed3 finalColorNoLVM = ambientColor + (texColor.rgb * dirLightColor);
                //return fixed4(finalColorNoLVM, 1);

                fixed3 finalLvmLerp = lerp(finalColorNoLVM, finalColor, _LVMColorContribution);

                return fixed4(finalLvmLerp, texColor.a);
            }
            ENDCG
        }

        Pass
        {
            Tags {"LightMode" = "ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            struct VertexData
            {
                float4 position : POSITION;
            };

            float4 vert(VertexData v) : SV_POSITION
            {
                return UnityObjectToClipPos(v.position);
            }

            half4 frag() : SV_TARGET
            {
                return 0;
            }
            ENDCG
        }
    }
}
