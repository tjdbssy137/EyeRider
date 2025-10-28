Shader "UI/InverseHoleMask"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _HolePos ("Hole Position", Vector) = (0.5, 0.5, 0, 0)
        _HoleSize ("Hole Size", Vector) = (0.1, 0.1, 0, 0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanvasMaterial"="True" }
        LOD 100

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _HolePos;
            float4 _HoleSize;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // 화면 비율 보정: HolePos와 HoleSize를 0~1 UV 단위로 처리
                float2 diff = (i.uv - _HolePos.xy) / _HoleSize.xy;
                float dist = length(diff);

                if (dist < 0.5) // 반경 기준
                    col.a = 0;

                return col;
            }
            ENDCG
        }
    }
}
