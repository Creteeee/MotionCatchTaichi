Shader "Unlit/Sky"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.5, 0.5, 1.0, 1.0)  // 顶部颜色
        _BottomColor ("Bottom Color", Color) = (1.0, 1.0, 1.0, 1.0)  // 底部颜色
        _Exponent ("Exponent", Float) = 1.0  // 控制渐变强度
        _Edge1("Edge1",float)=0.2
        _Edge2("Edge2",float)=0.3
    }
    SubShader
    {
        Tags {"Queue" = "Background" "RenderType"="Background"}
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldDir : TEXCOORD0;
            };

            float4 _TopColor;
            float4 _BottomColor;
            float _Exponent;
            float _Edge1;
            float _Edge2;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);

                // Get the world direction (normalized)
                o.worldDir = normalize(mul(unity_ObjectToWorld, v.vertex).xyz);

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Calculate the vertical factor based on the Y component of the world direction
                float verticalFactor = pow(saturate(i.worldDir.y * 0.5 + 0.5), _Exponent);
                verticalFactor = smoothstep(_Edge1,_Edge2,verticalFactor);
                // Interpolate between the top and bottom colors
                half4 color = lerp(_BottomColor, _TopColor, verticalFactor);

                return half4(color.rgb, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "RenderFX/Skybox"
}
