Shader "Custom/PathOfLayouts/Fog"
{
    Properties
    {
        [MainTexture] _MainTex ("Base (RGB)", 2D) = "white" {}

        _FogMask   ("Fog Mask (R: 1=fog, 0=revealed)", 2D) = "white" {}

        _FogAlpha  ("Fog Alpha",  Range(0,1)) = 1
        _EdgeAlpha ("Edge Alpha", Range(0,1)) = 1

        _EdgeMin   ("Edge Min (0..1)", Range(0,1)) = 0.5882353   // 150/255
        _EdgeMax   ("Edge Max (0..1)", Range(0,1)) = 0.7843137   // 200/255
        _EdgeSoft  ("Edge Softness",   Range(0,0.25)) = 0.02

        _Blue      ("Edge Blue", Color) = (0.20, 0.40, 1.00, 1)
    }

    SubShader
    {
        // UI overlay: transparent queue, URP
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "FogOverlay"
            Cull Off
            ZWrite Off
            ZTest Always

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_FogMask);
            SAMPLER(sampler_FogMask);

            float _FogAlpha;
            float _EdgeAlpha;

            float _EdgeMin;
            float _EdgeMax;
            float _EdgeSoft;

            float4 _Blue;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                // mask in 0..1 where 1 = fully fogged, 0 = revealed
                half m = SAMPLE_TEXTURE2D(_FogMask, sampler_FogMask, i.uv).r;

                // Base fog color: black. Alpha scales with mask.
                half3 fogCol = half3(0, 0, 0);
                half  fogA   = saturate(m * (half)_FogAlpha);

                // Edge band selection with soft boundaries:
                // edgeMask ~1 when m in [_EdgeMin, _EdgeMax], else 0 (with softness)
                half a0 = smoothstep((half)_EdgeMin, (half)(_EdgeMin + _EdgeSoft), m);
                half a1 = 1.0h - smoothstep((half)(_EdgeMax - _EdgeSoft), (half)_EdgeMax, m);
                half edgeMask = saturate(a0 * a1);

                half3 edgeCol = (half3)_Blue.rgb;
                half3 col = lerp(fogCol, edgeCol, edgeMask);

                half a = saturate(fogA + edgeMask * (half)_EdgeAlpha);

                return half4(col, a);
            }
            ENDHLSL
        }
    }
}
