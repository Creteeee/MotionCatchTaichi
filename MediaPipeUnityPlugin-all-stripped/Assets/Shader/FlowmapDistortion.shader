Shader "UI/FlowMap_TwoPhase"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)

        _FlowMap("FlowMap (RG)", 2D) = "white" {}
        _FlowSpeed("Flow Intensity", Float) = 0.1
        _TimeSpeed("Flow Speed", Float) = 1.0

        [Toggle]_REVERSE_FLOW("Reverse", Int) = 0

        _Tiling("Tiling", Vector) = (1,1,0,0)
        _Offset("Offset", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _REVERSE_FLOW_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _MainTex;
            sampler2D _FlowMap;

            float4 _MainTex_ST;
            float4 _Tiling;
            float4 _Offset;

            float4 _Color;
            float _FlowSpeed;
            float _TimeSpeed;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv * _Tiling.xy + _Offset.xy;
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                // ------------ Flowmap direction ------------
                float3 flowSample = tex2D(_FlowMap, i.uv).rgb;

                // RG → direction vector -1~1
                float2 dir = flowSample.xy * 2.0 - 1.0;

                // Intensity
                dir *= _FlowSpeed;

                #ifdef _REVERSE_FLOW_ON
                dir *= -1.0;
                #endif

                // ------------ Time phase ------------
                float t = _Time.y * _TimeSpeed * 0.1;

                float phase0 = frac(t);
                float phase1 = frac(t + 0.5);

                // ------------ UV with tiling ------------
                float2 uvBase = i.uv * _MainTex_ST.xy + _MainTex_ST.zw;

                float2 uv0 = uvBase + dir * phase0;
                float2 uv1 = uvBase + dir * phase1;

                float3 c0 = tex2D(_MainTex, uv0).rgb;
                float3 c1 = tex2D(_MainTex, uv1).rgb;

                // ------------ Smooth crossfade ------------
                float flowLerp = abs((0.5 - phase0) / 0.5);

                float3 col = lerp(c0, c1, flowLerp);

                return float4(col, 1.0) * _Color;
            }

            ENDHLSL
        }
    }
}
