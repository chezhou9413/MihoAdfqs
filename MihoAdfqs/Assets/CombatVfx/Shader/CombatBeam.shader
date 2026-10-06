Shader "Miho/CombatBeam"
{
    Properties
    {
        _Color ("光束颜色", Color) = (1,0.4,0.1,1)
        _Phase ("流动相位", Float) = 0
        _Energy ("能量流", Float) = 0
        _Flame ("火焰锥", Float) = 0
        _Trail ("炮弹渐隐拖尾", Float) = 0
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
            float _Phase, _Energy, _Flame, _Trail;
            Output vert(Input v)
            {
                Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o;
            }
            fixed4 frag(Output i) : SV_Target
            {
                float side = abs(i.uv.x * 2 - 1);
                if (_Flame > .5)
                {
                    side /= lerp(.1, 1, i.uv.y);
                    float turbulence = .78 + .22 * sin(i.uv.y * 52 - _Phase * 27 + sin(i.uv.x * 18) * 3);
                    float heat = pow(saturate(1-side), 2) * turbulence;
                    float3 fire = lerp(float3(1,.18,.025), float3(1,.95,.55), heat);
                    return fixed4(fire, heat * smoothstep(1,.58,i.uv.y) * _Color.a);
                }
                //拖尾从弹体向后逐渐收细，透明度沿同一方向衰减。
                float tail = 1;
                if (_Trail > .5)
                {
                    side /= lerp(.06, 1, i.uv.y);
                    tail = pow(saturate(i.uv.y), 1.4);
                }
                float halo = pow(saturate(1 - side), 2);
                float core = exp(-side * side * 100);
                float cap = smoothstep(0, .035, i.uv.y) * smoothstep(0, .035, 1 - i.uv.y);
                float flow = 1 + _Energy * .14 * sin(i.uv.y * 85 - _Phase * 38);
                float3 rgb = lerp(_Color.rgb, float3(1, .97, .87), core * .85);
                return fixed4(rgb * flow, saturate((halo * .7 + core) * cap * tail * _Color.a));
            }
            ENDCG
        }
    }
}
