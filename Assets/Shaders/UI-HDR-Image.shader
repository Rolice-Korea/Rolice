Shader "UI/HDR Image"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [HDR] _HDRColor    ("HDR Color",     Color)        = (1,1,1,1)
        _HDRIntensity      ("HDR Intensity",  Range(0, 20)) = 1

        // UI Stencil
        _StencilComp      ("Stencil Comparison",  Float) = 8
        _Stencil          ("Stencil ID",          Float) = 0
        _StencilOp        ("Stencil Operation",   Float) = 0
        _StencilWriteMask ("Stencil Write Mask",  Float) = 255
        _StencilReadMask  ("Stencil Read Mask",   Float) = 255
        _ColorMask        ("Color Mask",          Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"            = "Transparent"
            "IgnoreProjector"  = "True"
            "RenderType"       = "Transparent"
            "PreviewType"      = "Plane"
            "CanUseSpriteAtlas"= "True"
            "RenderPipeline"   = "UniversalPipeline"
        }

        Stencil
        {
            Ref      [_Stencil]
            Comp     [_StencilComp]
            Pass     [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                float4 mask       : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST;
                float4 _ClipRect;
                float  _UIMaskSoftnessX;
                float  _UIMaskSoftnessY;

                float4 _HDRColor;
                float  _HDRIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv        = TRANSFORM_TEX(input.uv, _MainTex);
                output.color     = input.color * _Color;

                // UI Mask
                float2 pixelSize = output.positionCS.w;
                pixelSize /= abs(float2(_ScreenParams.x * UNITY_MATRIX_P[0][0],
                                        _ScreenParams.y * UNITY_MATRIX_P[1][1]));
                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                float2 maskSoftness = max(float2(_UIMaskSoftnessX, _UIMaskSoftnessY),
                                          pixelSize.xy);
                output.mask = float4(input.positionOS.xy * 2 - clampedRect.xy - clampedRect.zw,
                                     0.25 / (0.25 * maskSoftness.xy));

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // HDR: 텍스처 색상에 HDR 컬러와 인텐시티를 곱해 1.0 초과 값 출력 → Bloom 트리거
                float4 color = texColor * input.color;
                color.rgb *= _HDRColor.rgb * _HDRIntensity;
                color.a  *= _HDRColor.a;

                color.rgb *= color.a; // Premultiplied alpha

                #ifdef UNITY_UI_CLIP_RECT
                float2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(input.mask.xy)) *
                                     input.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDHLSL
        }
    }

    FallBack "UI/Default"
}
