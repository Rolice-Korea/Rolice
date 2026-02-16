Shader "UI/HDR OutlineGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // Glow (RcUIOutlineGlow 컴포넌트에서 설정)
        _BaseColor      ("Base Color",       Color)        = (0.15, 0.15, 0.15, 1)
        [HDR] _GlowColor ("Glow Color",      Color)        = (0.3, 1, 0.8, 1)
        _GlowIntensity  ("Glow Intensity",   Range(0, 10)) = 2
        _BorderWidth    ("Border Width",     Range(0.01, 0.5)) = 0.08
        _Softness       ("Softness",         Range(0, 1))  = 0.5
        _BloomFalloff   ("Bloom Falloff",    Range(0.1, 3.0)) = 0.4

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

            // ── Input / Output ──────────────────────────────────

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                float2 rectUV     : TEXCOORD1;   // IMeshModifier 정규화 좌표 (0~1)
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                float4 mask       : TEXCOORD1;
                float2 rectPos    : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // ── Textures & Constants ────────────────────────────

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST;
                float4 _ClipRect;
                float  _UIMaskSoftnessX;
                float  _UIMaskSoftnessY;

                float4 _BaseColor;
                float4 _GlowColor;
                float  _GlowIntensity;
                float  _BorderWidth;
                float  _Softness;
                float  _BloomFalloff;
                float2 _RectSize;
            CBUFFER_END

            // ── Vertex ──────────────────────────────────────────

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv        = TRANSFORM_TEX(input.uv, _MainTex);
                output.color     = input.color * _Color;
                output.rectPos   = input.rectUV;

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

            // ── Fragment ────────────────────────────────────────

            float4 frag(Varyings input) : SV_Target
            {
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // 엣지 거리: 픽셀 단위로 계산 후 짧은 변 기준 정규화 (4변 균일)
                float2 pos = input.rectPos;
                float minDim = min(_RectSize.x, _RectSize.y);
                float2 pixelDist = min(pos, 1.0 - pos) * _RectSize;
                float edgeDist = min(pixelDist.x, pixelDist.y) / minDim;

                // 테두리 마스크
                float hardEdge = _BorderWidth * (1.0 - _Softness);
                float borderMask = 1.0 - smoothstep(hardEdge, _BorderWidth, edgeDist);

                // 색상 블렌딩
                float4 result = _BaseColor;
                result.rgb = lerp(result.rgb, _GlowColor.rgb, borderMask);

                // HDR Intensity: 테두리 영역에 블룸 보장
                float glowMask = pow(borderMask, _BloomFalloff);
                result.rgb *= lerp(1.0, _GlowIntensity, glowMask);

                float4 color = texColor * result * input.color;
                color.rgb *= color.a;   // Premultiplied alpha

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
