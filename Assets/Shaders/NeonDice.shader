Shader "Rolice/NeonDice"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.08, 0.08, 0.08, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.7
        _Metallic ("Metallic", Range(0, 1)) = 0.8

        [Header(Face Glow)]
        [HDR] _GlowColor ("Glow Color", Color) = (1, 0, 0, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1.5
        _GlowWidth ("Glow Width", Range(0, 0.5)) = 0.15
        _GlowSoftness ("Glow Softness", Range(0.01, 0.5)) = 0.2
        _FaceHalfSize ("Face Half Size", Range(0.01, 2)) = 0.5

        [Header(Light Bleed)]
        _LightBleedStrength ("Light Bleed Strength", Range(0, 3)) = 1.5

        [HideInInspector] _SrcBlend ("Src Blend", Float) = 1
        [HideInInspector] _DstBlend ("Dst Blend", Float) = 0
        [HideInInspector] _ZWrite ("ZWrite", Float) = 1
        [HideInInspector] _Surface ("Surface Type", Float) = 0
        [HideInInspector] _ReflectionFadeFloorY ("Reflection Floor Y", Float) = 0
        [HideInInspector] _ReflectionFadeDist ("Reflection Fade Dist", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _LIGHT_LAYERS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float3 positionOS : TEXCOORD3;
                float3 normalOS : TEXCOORD4;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Smoothness;
                half _Metallic;
                half4 _GlowColor;
                half _GlowIntensity;
                half _GlowWidth;
                half _GlowSoftness;
                half _FaceHalfSize;
                half _LightBleedStrength;
                float _ReflectionFadeFloorY;
                float _ReflectionFadeDist;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
                output.positionOS = input.positionOS.xyz;
                output.normalOS = input.normalOS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                float3 positionWS = input.positionWS;

                half specPower = exp2(10.0 * _Smoothness + 1.0);

                // === 메인 라이트 ===
                float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
                Light mainLight = GetMainLight(shadowCoord);

                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = _BaseColor.rgb * mainLight.color * NdotL * mainLight.shadowAttenuation;

                half3 halfDir = normalize(mainLight.direction + viewDirWS);
                half NdotH = saturate(dot(normalWS, halfDir));
                half3 specular = mainLight.color * pow(NdotH, specPower) * _Smoothness;

                // === 추가 라이트 ===
                half3 additionalDiffuse = 0;
                half3 additionalSpecular = 0;

                InputData inputData = (InputData)0;
                inputData.positionWS = positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDirWS;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

                uint additionalLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(additionalLightCount)
                    Light light = GetAdditionalLight(lightIndex, positionWS);
                    half NdotL_add = dot(normalWS, light.direction);
                    half wrapNdotL = saturate(NdotL_add * 0.5 + 0.5);
                    half atten = light.distanceAttenuation * light.shadowAttenuation;
                    half3 lightContrib = light.color * wrapNdotL * atten;
                    additionalDiffuse += _BaseColor.rgb * lightContrib * _LightBleedStrength;

                    half3 halfDir_add = normalize(light.direction + viewDirWS);
                    half NdotH_add = saturate(dot(normalWS, halfDir_add));
                    additionalSpecular += light.color * pow(NdotH_add, specPower * 0.5) * _Smoothness * atten;
                LIGHT_LOOP_END

                // === 면 글로우 ===
                // 오브젝트 스페이스 좌표 + 노멀 방향으로 면 UV 결정
                // 큐브: -0.5~0.5 범위, 노멀 방향에 수직인 두 축을 사용
                float3 absNorm = abs(input.normalOS);
                float2 rawCoords;
                if (absNorm.x > absNorm.y && absNorm.x > absNorm.z)
                    rawCoords = input.positionOS.yz;
                else if (absNorm.y > absNorm.z)
                    rawCoords = input.positionOS.xz;
                else
                    rawCoords = input.positionOS.xy;

                float2 faceUV = saturate(rawCoords / (_FaceHalfSize * 2.0) + 0.5);
                float2 edgeDist = min(faceUV, 1.0 - faceUV);
                float minEdgeDist = min(edgeDist.x, edgeDist.y);
                half glowMask = 1.0 - smoothstep(_GlowWidth, _GlowWidth + _GlowSoftness, minEdgeDist);
                half3 faceGlow = _GlowColor.rgb * glowMask * _GlowIntensity;

                // === 앰비언트 ===
                half3 ambient = SampleSH(normalWS) * _BaseColor.rgb;

                // === 최종 합성 ===
                half3 baseLight = ambient + diffuse + specular + additionalDiffuse + additionalSpecular;

                half alpha = _BaseColor.a;
                half glowFade = 1.0;
                if (_ReflectionFadeDist > 0)
                {
                    float distFromFloor = abs(positionWS.y - _ReflectionFadeFloorY);
                    float distFactor = saturate(1.0 - distFromFloor / _ReflectionFadeDist);
                    alpha *= distFactor;
                    glowFade = distFactor * distFactor;
                }

                half3 finalColor = baseLight * alpha + faceGlow * glowFade;
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
