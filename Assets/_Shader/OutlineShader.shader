Shader "Custom/URP_OutlineShader"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,1,1,1) // 흰색 테두리
        _OutlineWidth ("Outline Width", Range(0, 0.05)) = 0.01
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "RenderPipeline"="UniversalPipeline" 
            "Queue"="Transparent+1" 
        }
        LOD 100

        Pass
        {
            Name "Outline"
            Tags { "LightMode"="UniversalForward" }
            Cull Front
            ZWrite On
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
            };

            float _OutlineWidth;
            float4 _OutlineColor;

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // 오브젝트 공간에서 노멀 방향으로 정점 확장
                float3 scaledPosition = input.positionOS.xyz + (input.normalOS * _OutlineWidth);
                
                // 클립 공간으로 변환 (URP 방식)
                output.positionCS = TransformObjectToHClip(scaledPosition);
                output.color = _OutlineColor;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return input.color;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}