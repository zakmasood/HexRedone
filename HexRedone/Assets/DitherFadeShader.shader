Shader"Custom/DitherFadeShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _DitherThreshold ("Dither Threshold", Range(0, 1)) = 0.5
        _FadeStart ("Fade Start", Float) = 10
        _FadeEnd ("Fade End", Float) = 20
    }
    
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
LOD100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
    float4 worldPos : TEXCOORD1;
};

sampler2D _MainTex;
float4 _MainTex_ST;
float _DitherThreshold;
float _FadeStart;
float _FadeEnd;

v2f vert(appdata v)
{
    v2f o;
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    return o;
}

fixed4 frag(v2f i) : SV_Target
{
    fixed4 col = tex2D(_MainTex, i.uv);
                
    float distance = length(i.worldPos - _WorldSpaceCameraPos);
    float fade = 1 - saturate((distance - _FadeStart) / (_FadeEnd - _FadeStart));
                
    float2 screenPos = i.vertex.xy / i.vertex.w;
    float2 ditherCoord = screenPos * _ScreenParams.xy * _DitherThreshold;
    float dither = frac(dot(float2(171.0, 231.0), ditherCoord));
                
    clip(fade - dither);
                
    return col;
}
            ENDCG
        }
    }
}