Shader "Hidden/Rc/PauseBlur"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

        TEXTURE2D(_BlitTexture);
        SAMPLER(sampler_LinearClamp);
        float4 _BlitTexture_TexelSize;
        float  _BlurSize;

        struct Attributes { uint vertexID : SV_VertexID; };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv         : TEXCOORD0;
        };

        Varyings Vert(Attributes input)
        {
            Varyings o;
            // 풀스크린 트라이앵글 (3개 버텍스)
            float2 uv = float2((input.vertexID << 1) & 2, input.vertexID & 2);
            o.positionCS = float4(uv * 2.0 - 1.0, 0.0, 1.0);
            o.uv = uv;
            #if UNITY_UV_STARTS_AT_TOP
            o.uv.y = 1.0 - o.uv.y;
            #endif
            return o;
        }
        ENDHLSL

        // Pass 0 - Horizontal
        Pass
        {
            Name "Horizontal"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragH

            half4 FragH(Varyings i) : SV_Target
            {
                float2 ts = _BlitTexture_TexelSize.xy * _BlurSize;
                half4 c = 0;
                c += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv + float2(-2, 0) * ts) * 0.0625;
                c += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv + float2(-1, 0) * ts) * 0.25;
                c += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv)                       * 0.375;
                c += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv + float2( 1, 0) * ts) * 0.25;
                c += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv + float2( 2, 0) * ts) * 0.0625;
                return c;
            }
            ENDHLSL
        }

        // Pass 1 - Vertical
        Pass
        {
            Name "Vertical"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragV

            half4 FragV(Varyings i) : SV_Target
            {
                float2 ts = _BlitTexture_TexelSize.xy * _BlurSize;
                half4 c = 0;
                c += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv + float2(0, -2) * ts) * 0.0625;
                c += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv + float2(0, -1) * ts) * 0.25;
                c += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv)                       * 0.375;
                c += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv + float2(0,  1) * ts) * 0.25;
                c += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv + float2(0,  2) * ts) * 0.0625;
                return c;
            }
            ENDHLSL
        }
    }
}
