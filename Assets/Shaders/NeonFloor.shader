Shader "Rolice/NeonFloor"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.05, 0.05, 0.12, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.65
        _Metallic ("Metallic", Range(0, 1)) = 0.0

        [Header(Fresnel Edge Glow)]
        _FresnelColor ("Fresnel Color", Color) = (0.3, 0.4, 0.8, 1)
        _FresnelPower ("Fresnel Power", Range(1, 10)) = 3.0
        _FresnelIntensity ("Fresnel Intensity", Range(0, 2)) = 0.5

        [Header(Light Bleed)]
        _LightBleedStrength ("Light Bleed Strength", Range(0, 3)) = 1.5
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
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Smoothness;
                half _Metallic;
                half4 _FresnelColor;
                half _FresnelPower;
                half _FresnelIntensity;
                half _LightBleedStrength;
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

                // === 추가 라이트 (Forward+ 호환) ===
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

                    // Wrap lighting - 빛이 넓게 번지도록
                    half NdotL_add = dot(normalWS, light.direction);
                    half wrapNdotL = saturate(NdotL_add * 0.5 + 0.5);

                    half atten = light.distanceAttenuation * light.shadowAttenuation;
                    half3 lightContrib = light.color * wrapNdotL * atten;
                    additionalDiffuse += _BaseColor.rgb * lightContrib * _LightBleedStrength;

                    // 스페큘러
                    half3 halfDir_add = normalize(light.direction + viewDirWS);
                    half NdotH_add = saturate(dot(normalWS, halfDir_add));
                    additionalSpecular += light.color * pow(NdotH_add, specPower * 0.5) * _Smoothness * atten;
                LIGHT_LOOP_END

                // === 프레넬 엣지 글로우 ===
                half fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower);
                half3 fresnelGlow = _FresnelColor.rgb * fresnel * _FresnelIntensity;

                // === 앰비언트 ===
                half3 ambient = SampleSH(normalWS) * _BaseColor.rgb;

                // === 최종 합성 ===
                half3 finalColor = ambient + diffuse + specular + additionalDiffuse + additionalSpecular + fresnelGlow;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }

        // 그림자 캐스터
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

        // 뎁스 패스
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
