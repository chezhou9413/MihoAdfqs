Shader "Miho/CombatSmoke"
{
    Properties
    {
        _Color ("烟尘颜色", Color) = (.24,.21,.18,.5)
        _Age ("生命周期", Range(0,1)) = 0
        _Seed ("烟团种子", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent+10" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };
            float4 _Color;
            float _Age, _Seed;
            Output vert(Input v)
            {
                Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o;
            }
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)) + _Seed) * 43758.5453); }
            float noise(float2 p)
            {
                float2 a = floor(p), f = frac(p); f = f*f*(3-2*f);
                return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),
                    lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);
            }
            fixed4 frag(Output i) : SV_Target
            {
                float2 p = i.uv*2-1;
                float n = noise(p*4 + float2(_Age*.6, -_Age*.8));
                n = n*.7 + noise(p*9-_Age)*.3;
                float density = smoothstep(.1,.55, n) * pow(saturate(1-length(p)),1.5);
                float fade = smoothstep(0,.07,_Age) * pow(1-_Age, .7);
                return fixed4(_Color.rgb*(.65+n*.7), density*fade*_Color.a);
            }
            ENDCG
        }
    }
}
