Shader "Miho/CombatBurst"
{
    Properties
    {
        _Color ("闪光颜色", Color) = (1,.35,.08,1)
        _Age ("生命周期", Range(0,1)) = 0
        _Ring ("冲击环", Float) = 0
        _Seed ("形状种子", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent+20" "RenderType"="Transparent" }
        Blend SrcAlpha One
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
            float _Age, _Ring, _Seed;
            Output vert(Input v)
            {
                Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o;
            }
            fixed4 frag(Output i) : SV_Target
            {
                float2 p = i.uv * 2 - 1;
                float r = length(p);
                float angle = atan2(p.y, p.x);
                float rays = pow(abs(sin(angle * 7 + _Seed)), 12);
                float fire = exp(-r * r * 9) + rays * .32 * pow(saturate(1-r), 2);
                float ring = exp(-pow((r - .78) * 28, 2));
                float opacity = lerp(fire, ring, _Ring) * pow(1 - _Age, 1.4);
                float3 rgb = lerp(_Color.rgb, float3(1,.97,.82), exp(-r*r*30) * (1-_Ring));
                return fixed4(rgb, opacity * _Color.a);
            }
            ENDCG
        }
    }
}
