Shader "UI/InverseHoleMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CenterPx("Center Pixel", Vector) = (0,0,0,0)
        _RadiusPx("Radius Pixel", Float) = 100
        _FeatherPx("Feather Pixel", Float) = 120
        _CanvasSize("Canvas Size", Vector) = (1080,1920,0,0)
        _BackgroundAlpha("Background Alpha", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _CenterPx;
            float _RadiusPx;
            float _FeatherPx;
            float4 _CanvasSize;
            float _BackgroundAlpha;

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            VertexOutput Vert(VertexInput v)
            {
                VertexOutput o;
                o.pos = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 Frag(VertexOutput i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // 세로비
                float2 canvas = _CanvasSize.xy;
                float2 uvPx = float2(i.uv.x * canvas.x, i.uv.y * canvas.y);
                float2 centerPx = _CenterPx.xy;
                float2 d = uvPx - centerPx;
                float dist = length(d);
                float radiusPx = _RadiusPx;
                float featherPx = _FeatherPx;
                float alpha = smoothstep(radiusPx, radiusPx + featherPx, dist);
                alpha *= _BackgroundAlpha;
                col.a = alpha;
                return col;


                // 가로비
                // float2 canvas = _CanvasSize.xy;
                // float2 uvPx = i.uv * canvas;
                // float2 d = uvPx - _CenterPx.xy;
                // float dist = length(d);
                // float radiusPx = _RadiusPx;
                // float featherPx = _FeatherPx;
                // float alpha = smoothstep(radiusPx, radiusPx + featherPx, dist);
                // alpha *= _BackgroundAlpha;
                // col.a = alpha;
                // return col;
            }



            ENDHLSL
        }
    }
}
