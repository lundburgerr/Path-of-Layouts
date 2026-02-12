Shader "Custom/PathOfLayouts/FogStamp"
{
    Properties
    {
        [MainTexture] _MainTex ("Base (RGB)", 2D) = "white" {}

        _Split        ("Split (0..1 across soft ring)", Range(0,1)) = 0.5
        _MidReveal    ("Reveal at Split", Range(0,1)) = 0.2
        _PowA         ("Inner Curve Power", Range(0.1, 6)) = 1.0
        _PowB         ("Outer Curve Power", Range(0.1, 6)) = 1.0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "Stamp"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_PrevMask);
            SAMPLER(sampler_PrevMask);

            float2 _PlayerUv;
            float  _HardRadiusUv;
            float  _SoftRadiusUv;
            float  _Aspect;

            float  _Split;
            float  _MidReveal;
            float  _PowA;
            float  _PowB;

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
                half prevFog = SAMPLE_TEXTURE2D(_PrevMask, sampler_PrevMask, i.uv).r;

                // Aspect-correct distance
                float2 d = i.uv - _PlayerUv;
                d.x *= _Aspect;
                float dist = length(d);

                // Inside hard radius: fully revealed => fog 0
                if (dist <= _HardRadiusUv)
                    return half4(0, 0, 0, 1);

                // Outside soft radius: no effect
                if (dist >= _SoftRadiusUv)
                    return half4(prevFog, 0, 0, 1);

                // Soft ring normalized coordinate: t=0 at hard edge, t=1 at soft edge
                float denom = max(1e-6, (_SoftRadiusUv - _HardRadiusUv));
                float t = saturate((dist - _HardRadiusUv) / denom);

                float split = saturate(_Split);

                // Continuous reveal curve:
                // segment A: reveal 1.0 -> _MidReveal over t: [0..split]
                // segment B: reveal _MidReveal -> 0.0 over t: [split..1]
                float reveal;

                if (t < split)
                {
                    float a = t / max(1e-6, split);            // 0..1
                    a = pow(a, max(1e-6, _PowA));              // non-linear
                    reveal = lerp(1.0, _MidReveal, a);
                }
                else
                {
                    float b = (t - split) / max(1e-6, 1.0 - split); // 0..1
                    b = pow(b, max(1e-6, _PowB));
                    reveal = lerp(_MidReveal, 0.0, b);
                }

                // Convert reveal -> fog
                float stampFog = 1.0 - reveal;

                // Persistence: only reduce fog where this stamp is more revealing
                half outFog = min(prevFog, (half)stampFog);

                return half4(outFog, 0, 0, 1);
            }
            ENDHLSL
        }
    }
}
