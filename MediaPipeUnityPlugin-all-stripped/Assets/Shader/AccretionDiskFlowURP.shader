Shader "Custom/HorizontalAccretionDisk_URP"
{
    Properties
    {
        _ColorA ("Top Color", Color) = (1, 0.85, 0.6, 1)
        _ColorB ("Bottom Color", Color) = (0.6, 0.8, 1, 1)
        
        _Intensity ("Brightness", Range(0,4)) = 1.5

        _CurveAmp ("Curve Amplitude", Range(0,0.5)) = 0.15
        _CurveFreq ("Curve Frequency", Range(1,20)) = 8
        _Thickness ("Band Thickness", Range(0.01,0.3)) = 0.08
        _Sharpness ("Sharpness", Range(1,50)) = 10

        _FlowSpeed ("Flow Speed", Range(-5,5)) = 1.0
        
        _Tiling ("Tiling", Vector) = (1,1,0,0)
        _Offset ("Offset", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "HorizontalDisk"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float3 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            float4 _ColorA;
            float4 _ColorB;

            float _Intensity;
            float _CurveAmp;
            float _CurveFreq;
            float _Thickness;
            float _Sharpness;
            float _FlowSpeed;

            float4 _Tiling;
            float4 _Offset;

            // S 型曲线控制 y 中心线
            float sCurve(float x)
            {
                return _CurveAmp * sin(x * _CurveFreq + _Time.y * _FlowSpeed);
            }

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv * _Tiling.xy + _Offset.xy;
                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // 两条波动线位置
                float ycTop    = 0.5 + sCurve(uv.x * 6.283);
                float ycBottom = 0.5 - sCurve(uv.x * 6.283);

                float distTop    = abs(uv.y - ycTop);
                float distBottom = abs(uv.y - ycBottom);

                // 高斯形状亮度
                float bandTop    = exp(-_Sharpness * pow(distTop / _Thickness, 2));
                float bandBottom = exp(-_Sharpness * pow(distBottom / _Thickness, 2));

                float3 col =
                    bandTop * _ColorA.rgb +
                    bandBottom * _ColorB.rgb;

                float alpha = saturate((bandTop + bandBottom) * _Intensity);

                return float4(col * _Intensity, alpha);
            }

            ENDHLSL
        }
    }
}
