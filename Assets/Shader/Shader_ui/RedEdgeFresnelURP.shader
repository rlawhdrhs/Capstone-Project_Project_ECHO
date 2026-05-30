Shader "Custom/RedEdgeFresnelURP"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0, 0, 0, 0)
        _EdgeColor ("Edge Color", Color) = (1, 0, 0, 1)
        _Power ("Edge Sharpness", Float) = 4
        _Intensity ("Edge Intensity", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            float4 _BaseColor;
            float4 _EdgeColor;
            float _Power;
            float _Intensity;

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformWorldToHClip(positionWS);

                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(positionWS);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normal = normalize(input.normalWS);
                float3 viewDir = normalize(input.viewDirWS);

                float fresnel = 1.0 - saturate(dot(normal, viewDir));
                fresnel = pow(fresnel, _Power) * _Intensity;

                float alpha = saturate(fresnel);

                float4 color = _EdgeColor;
                color.a = alpha;

                return color;
            }

            ENDHLSL
        }
    }
}