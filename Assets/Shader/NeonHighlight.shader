Shader "Custom/NeonEdgeSprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,1,1,1)
        _OutlineThickness ("Outline Thickness (px)", Range(0.0, 10.0)) = 1.0
        _AlphaThreshold ("Alpha Threshold", Range(0,1)) = 0.01
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "OutlinedSprite"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float4 _OutlineColor;
            float _OutlineThickness;
            float _AlphaThreshold;
            float4 _MainTex_TexelSize;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 pos : SV_POSITION; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                fixed4 col = tex2D(_MainTex, uv) * _Color;
                // Draw sprite where opaque
                if (col.a > _AlphaThreshold)
                {
                    return col;
                }
                // Outline sampling
                float2 offs = _OutlineThickness * _MainTex_TexelSize.xy;
                float sum = 0;
                sum += tex2D(_MainTex, uv + float2( offs.x,  0   )).a;
                sum += tex2D(_MainTex, uv + float2(-offs.x,  0   )).a;
                sum += tex2D(_MainTex, uv + float2( 0,      offs.y)).a;
                sum += tex2D(_MainTex, uv + float2( 0,     -offs.y)).a;
                sum += tex2D(_MainTex, uv + float2( offs.x,  offs.y)).a;
                sum += tex2D(_MainTex, uv + float2(-offs.x,  offs.y)).a;
                sum += tex2D(_MainTex, uv + float2( offs.x, -offs.y)).a;
                sum += tex2D(_MainTex, uv + float2(-offs.x,-offs.y)).a;

                if (sum > _AlphaThreshold)
                {
                    return _OutlineColor;
                }
                // Neither fill nor outline: discard
                discard;
                // Required return to satisfy compiler (unreachable after discard)
                return fixed4(0,0,0,0);
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}
