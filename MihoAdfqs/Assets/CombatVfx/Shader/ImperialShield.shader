Shader "Miho/ImperialShield"
{
    Properties
    {
        _Color ("能量颜色", Color) = (1,.63,.16,1)
        _Phase ("游戏时间", Float) = 0
        _State ("容量、充能、停机、拼合", Vector) = (1,0,0,1)
        _Event ("破盾时间、重启时间、种子、双层", Vector) = (10,10,0,0)
        _Fragment ("碎片模式", Float) = 0
        _Age ("碎片进度", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent+25" "RenderType"="Transparent" }
        Blend One OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct Input
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float4 cell : TEXCOORD1;
            };
            struct Output
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 cell : TEXCOORD1;
                float3 normal : TEXCOORD2;
                float3 shell : TEXCOORD3;
                float4 impact : TEXCOORD4;
            };
            float4 _Color, _State, _Event, _Hits[4];
            float _Phase, _Fragment, _Age;

            //绕固定轴翻转薄片，无需为每片创建Transform或更新CPU矩阵。
            float3 RotateShard(float3 p, float3 axis, float angle)
            {
                float sine, cosine;
                sincos(angle,sine,cosine);
                return p*cosine + cross(axis,p)*sine + axis*dot(axis,p)*(1-cosine);
            }

            //命中位置先内凹，红色应力与回弹沿球面传播，只计算最近四次命中。
            void ImpactDeformation(float3 plateNormal, inout float3 position, inout float3 normal, out float2 effect)
            {
                float depth = 0;
                float3 slope = 0;
                effect = 0;
                [unroll] for (int hit=0; hit<4; hit++)
                {
                    float4 h = _Hits[hit];
                    if (h.w <= 0 || h.z < 0 || h.z >= 1.1) continue;
                    float3 source = float3(h.x,sqrt(saturate(1-dot(h.xy,h.xy))),h.y);
                    float distance = length(plateNormal-source);
                    float delay = distance*.32;
                    float age = max(0,h.z-delay);
                    float reach = exp(-distance*distance*9.5);
                    float life = saturate(1-h.z/1.1);
                    float press = (1-exp(-age*45))*exp(-age*5.5)*reach*life*h.w;
                    float rebound = sin(max(0,age-.16)*16)*exp(-age*6)*reach*life*h.w;
                    depth += press*.17-rebound*.022;
                    slope += (source-plateNormal*dot(source,plateNormal))*press*2.5;
                    float wave = exp(-pow((distance-h.z*1.45)*12,2))*exp(-h.z*3)*life*h.w;
                    effect.x = max(effect.x,press*2.5);
                    effect.y = max(effect.y,wave);
                }
                position -= plateNormal*clamp(depth,-.012,.10);
                normal = normalize(normal+slope);
                effect = saturate(effect);
            }

            //每片整体沿球面法线呼吸，拼片、受击与裂解时让对应动画优先。
            Output vert(Input v)
            {
                Output o;
                o.cell = v.cell;
                o.shell = normalize(v.vertex.xyz).xzy;
                o.impact = float4(0,0,1,0);
                if (_Fragment > .5)
                {
                    float seed = v.cell.z;
                    float age = saturate(_Age*(1+seed*.35));
                    float angle = v.cell.x*2.399963 + _Event.z;
                    float3 direction = float3(cos(angle),0,sin(angle));
                    float3 axis = normalize(float3(direction.z,.35,-direction.x));
                    float spin = seed*6.283185 + age*(5+seed*9);
                    float scale = .045 + seed*.045;
                    v.vertex.xyz = RotateShard(v.vertex.xyz*scale,axis,spin)
                        + direction*(.10+seed*.28+age*(2-age)*(.38+seed*.62));
                    v.vertex.z += sin(age*3.141593)*(.12+seed*.16);
                    v.normal = RotateShard(v.normal,axis,spin);
                    o.cell.w = age;
                }
                else
                {
                    float3 plateNormal = float3(v.cell.x,v.cell.w,v.cell.y);
                    float2 effect;
                    ImpactDeformation(plateNormal,v.vertex.xyz,v.normal,effect);
                    o.impact.xy = effect;
                    //固定种子控制各片的周期和幅度，不逐帧抽随机数或拉扯六边形。
                    float seed = frac(sin(dot(plateNormal,float3(12.9898,78.233,37.719)))*43758.5453);
                    float phase = (_Phase+_Event.z*.37)*(1.1+seed*.65)+seed*6.283185;
                    float pulse = .72*sin(phase)+.28*sin(phase*.61+seed*9.7);
                    float breatheMask = smoothstep(.85,1,_State.w)*(1-_State.z)*step(.72,_Event.x)
                        * smoothstep(.05,.38,plateNormal.y)*(1-saturate(effect.x+effect.y*.5));
                    v.vertex.xyz += plateNormal*pulse*(.006+seed*.010)*breatheMask;
                    o.impact.w = pulse*breatheMask;
                    //把每片的流动方向传给像素阶段，避免逐像素重复计算方向。
                    sincos(seed*6.283185,o.cell.x,o.cell.y);
                    o.cell.w = seed;
                    float progress = saturate((_State.w-v.cell.z*.25)/.75);
                    float loose = (1-progress)*(1-progress);
                    float spin = loose*(v.cell.z-.5)*2.4;
                    float travel = loose*.20;
                    o.impact.z = smoothstep(0,.12,progress);
                    if (_Event.x >= 0 && _Event.x < .72)
                    {
                        float fracture = saturate((_Event.x-v.cell.z*.12)/.48);
                        float expansion = fracture*(2-fracture);
                        travel = expansion*.24;
                        spin = expansion*(v.cell.z-.5)*5;
                        o.impact.z = 1-smoothstep(.1,1,fracture);
                    }
                    if (travel > 0)
                    {
                        float3 axis = normalize(cross(plateNormal,float3(.23,.91,.34)));
                        v.vertex.xyz = RotateShard(v.vertex.xyz-plateNormal*.5,axis,spin)
                            + plateNormal*(.5+travel);
                        v.normal = RotateShard(v.normal,axis,spin);
                    }
                }
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = v.normal.xzy;
                return o;
            }

            //六边形只计算片内距离，不扭曲坐标，也不执行光线步进或折射采样。
            float HexRadius(float2 p)
            {
                p = abs(p);
                return max(p.x*1.154701,p.y+p.x*.577350);
            }

            //薄片有窄亮边、透光面和侧壁，避免呈现厚重水晶或金属块的观感。
            float4 frag(Output i) : SV_Target
            {
                //只显示朝向镜头的球壳，后半球不叠成第二层蜂窝网。
                if (_Fragment < .5) clip(i.shell.z);
                float3 panelNormal = normalize(i.normal);
                float3 lamp = normalize(float3(-.55,.65,1));
                float diffuse = saturate(dot(panelNormal,lamp));
                float specular = pow(saturate(dot(panelNormal,normalize(lamp+float3(0,0,1)))),32);
                float fresnel = pow(1-abs(panelNormal.z),3);
                float polygon = HexRadius(i.uv);
                float aa = max(fwidth(polygon)*.5,.002);
                if (_Fragment > .5)
                {
                    float mask = 1-smoothstep(.90-aa,.90+aa,polygon);
                    float border = smoothstep(.85-aa,.90,polygon);
                    float fade = pow(1-i.cell.w,1.4)*mask*_Color.a;
                    float shardLight = (.20+diffuse*.16+border*.75+specular*.6+fresnel*.25)*fade;
                    float3 shardColor = lerp(_Color.rgb,float3(1,.97,.9),saturate(specular*.65+border*.2));
                    return float4(shardColor*shardLight*1.3,fade*(.14+border*.30));
                }

                float3 normal = normalize(i.shell);
                float2 p = normal.xy;
                float shatter = saturate(_Event.x / .55);
                float breaking = 1 - step(.72,_Event.x);
                float radius2 = dot(p,p);
                float radius = sqrt(radius2);
                float phase = _Phase + _Event.z;
                float rim = smoothstep(.92-aa,.96,polygon);
                float seed = i.cell.z;
                float edge = pow(1-normal.z,2.5);
                float shell = exp(-pow((radius-.945)*70,2));
                float inner = exp(-pow((radius-.875)*90,2)) * _Event.w;
                float highlight = pow(saturate(dot(normal,normalize(float3(-.45,.6,.85)))),36);
                float breath = .96 + .04*i.impact.w;
                float offline = _State.z;
                float rebuild = _State.w;
                float charge = _State.y*lerp(1,rebuild,offline);
                float scan = exp(-pow((p.y - (frac(phase*.35)*2.4-1.2))*20,2));
                float energy = saturate(_State.x);
                float flicker = lerp(.78+.22*sin(phase*29),1,smoothstep(0,.25,energy));
                float present = lerp(1,.55,offline);
                float face = (.20+diffuse*.15+specular*.3)*(1-step(.72,seed)*.15);
                float intensity = (face + fresnel*.18 + rim*.32
                    + edge*.18 + shell*.45 + inner*.25 + highlight*.12) * breath * flicker * present;
                //重建中的薄片边缘显示断续电弧和充能扫描。
                float arc = pow(saturate(sin(atan2(p.y,p.x)*5-phase*2)),8);
                intensity += offline * shell * arc * (.045 + rebuild*.16);
                intensity += charge * (scan * (.05+rim*.27) + shell*.07);

                float heat = i.impact.x;
                float wave = i.impact.y;
                //能量带在片内缓慢掠过，避开接缝，并在受击变红时减弱。
                float across = dot(i.uv,i.cell.xy);
                float along = dot(i.uv,float2(-i.cell.y,i.cell.x));
                float bend = sin(across*4.5+phase*.45+i.cell.w*6.283185)*.09;
                float stream = .5+.5*sin((along*.65+bend-phase*(.15+i.cell.w*.08)+i.cell.w)*6.283185);
                float flow = pow(stream,8)*(1-smoothstep(.72,.94,polygon));
                float flowGate = (1-offline)*(1-breaking)*smoothstep(.7,1,rebuild)*(1-heat*.8);
                intensity += flow*(.13+diffuse*.05)*flowGate;
                intensity += heat*(.16+rim*.3)+wave*(.08+rim*.32);
                //裂解时蜂窝接缝先过曝，再按单元先后消失。
                intensity += breaking*(rim*.4+.10)*(1-shatter);
                float reform = saturate(1-_Event.y/1.1);
                float ring = exp(-pow((radius-saturate(_Event.y/1.1)*1.2)*35,2));
                intensity += reform*(ring*.6+rim*.12);
                float3 tint = lerp(_Color.rgb,float3(1,.24,.07),(1-energy)*.38*(1-offline));
                float3 color = lerp(tint,float3(1,.97,.88),saturate(specular*.25+rim*.12+breaking*.35+flow*flowGate*.14));
                //变红只作用于受击片与正在传播的相邻片，避免整罩同时闪红。
                color = lerp(color,float3(1,.025,.008),saturate(heat+wave*.5));
                float mask = 1-smoothstep(.965,1,radius);
                //发光与背景遮盖分别控制，亮地面上仍能看清罩面及蜂窝接缝。
                float light = saturate(intensity) * mask * _Color.a * i.impact.z;
                return float4(color * light * 1.3, light * .75);
            }
            ENDCG
        }
    }
}
