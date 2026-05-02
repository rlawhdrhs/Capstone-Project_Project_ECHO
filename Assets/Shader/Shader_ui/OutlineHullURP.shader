Shader "Custom/OutlineHullURP"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,0.3,0.1,1)
        _FlowColor ("Flow Color", Color) = (1,1,0.6,1)
        _OutlineWidth ("Outline Width", Float) = 0.03
        _FlowSpeed ("Flow Speed", Float) = 2.0
        _FlowStrength ("Flow Strength", Float) = 1.5
        _FlowScale ("Flow Scale", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry+10"
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode"="UniversalForward" }

            Cull Front
            ZWrite On
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                half4 _FlowColor;
                float _OutlineWidth;
                float _FlowSpeed;
                float _FlowStrength;
                float _FlowScale;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 posOS = IN.positionOS.xyz + normalize(IN.normalOS) * _OutlineWidth;
                float4 positionWS = mul(GetObjectToWorldMatrix(), float4(posOS, 1.0));

                OUT.positionWS = positionWS.xyz;
                OUT.positionHCS = TransformWorldToHClip(positionWS.xyz);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float t = _Time.y * _FlowSpeed;

                // ¿ùµå ÁÂÇ¥ ±â¹ÝÀ¸·Î Èå¸£´Â ºû ´À³¦
                float wave = sin(IN.positionWS.y * _FlowScale + IN.positionWS.x * 0.5 + t);
                wave = saturate((wave + 1.0) * 0.5);

                // ºûÀÌ Áö³ª°¡´Â ´À³¦ °­Á¶
                wave = pow(wave, 4.0) * _FlowStrength;

                half3 finalColor = _OutlineColor.rgb + _FlowColor.rgb * wave;
                return half4(finalColor, _OutlineColor.a);
            }
            ENDHLSL
        }
    }
}