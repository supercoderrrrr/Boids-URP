Shader "Cinematic Reef/Living Surface"
{
    Properties
    {
        _BaseMap("Albedo",2D)="white"{}
        [Normal] _BumpMap("Normal",2D)="bump"{}
        _BaseColor("Tint",Color)=(1,1,1,1)
        _Tile("World tiling",Float)=.22
        _Triplanar("Triplanar",Float)=1
        _NormalStrength("Normal strength",Float)=.75
        _Smoothness("Smoothness",Range(0,1))=.18
        _Fish("Fish deformation",Float)=0
        _Sway("Plant sway",Float)=0
        _Algae("Algae",Range(0,1))=.25
        _Desaturation("Desaturation",Range(0,1))=0
        _SandRipples("Sand ripples",Float)=0
        _Cull("Cull",Float)=2
    }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "ReefCommon.hlsl"
        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
        float _ReefCaustics;
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float _Tile, _Triplanar, _NormalStrength, _Smoothness, _Fish, _Sway, _Algae, _Desaturation, _SandRipples;
        CBUFFER_END
        struct A {float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID};
        struct V {float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float3 normalWS:TEXCOORD1; float2 uv:TEXCOORD2; UNITY_VERTEX_INPUT_INSTANCE_ID};
        float3 Deform(float3 p)
        {
            float phase = dot(unity_ObjectToWorld._m03_m13_m23, float3(.53,.73,.37));
            float tail = 1 - smoothstep(-.5,.2,p.z);
            p.x += sin(_Time.y * 12 + p.z * 6 + phase) * tail * tail * .14 * _Fish;
            float height = max(p.y,0);
            p.x += sin(_Time.y * .7 + phase + height * .6) * height * .06 * _Sway;
            p.z += cos(_Time.y * .51 + phase + height * .3) * height * .035 * _Sway;
            return p;
        }
        V vert(A i)
        {
            V o; UNITY_SETUP_INSTANCE_ID(i); UNITY_TRANSFER_INSTANCE_ID(i,o);
            float3 p = Deform(i.positionOS.xyz);
            o.positionWS = TransformObjectToWorld(p); o.positionCS = TransformWorldToHClip(o.positionWS);
            o.normalWS = TransformObjectToWorldNormal(i.normalOS); o.uv = i.uv; return o;
        }
        half4 frag(V i):SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(i);
            float3 n = normalize(i.normalWS);
            float3 albedo;
            if (_Triplanar > .5)
            {
                float3 w = pow(abs(n),4); w /= max(w.x+w.y+w.z,.001);
                float3 p = i.positionWS * _Tile;
                albedo = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.zy).rgb*w.x
                    + SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.xz).rgb*w.y
                    + SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.xy).rgb*w.z;
                float3 nx = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,p.zy));
                float3 ny = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,p.xz));
                float3 nz = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,p.xy));
                n = normalize(n + (float3(0,nx.y,nx.x)*w.x + float3(ny.x,0,ny.y)*w.y + float3(nz.x,nz.y,0)*w.z) * _NormalStrength);
                float macro = .84 + .16 * sin(i.positionWS.x * .37 + sin(i.positionWS.z*.24) * 2 + i.positionWS.y*.18);
                albedo *= macro;
                albedo = lerp(albedo, albedo*float3(.45,.66,.44), saturate(n.y*.7+.2) * _Algae);
            }
            else albedo = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb;
            albedo = lerp(albedo,dot(albedo,float3(.2126,.7152,.0722)).xxx,_Desaturation) * _BaseColor.rgb;
            float ripplePhase=i.positionWS.x*13+sin(i.positionWS.z*.75)*1.4;
            float rippleFade=saturate(1-fwidth(ripplePhase)*.4);
            n=normalize(n+float3(cos(ripplePhase)*.18,0,sin(ripplePhase)*.035)*_SandRipples*rippleFade);
            Light light = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
            float ndl = saturate(dot(n,light.direction));
            float3 view = normalize(_WorldSpaceCameraPos-i.positionWS);
            float spec = pow(saturate(dot(n,normalize(view+light.direction))), lerp(8,100,_Smoothness)) * _Smoothness;
            float3 ambient = lerp(float3(.035,.065,.075),float3(.20,.27,.28),saturate(n.y*.5+.5));
            float caustic = ReefCaustics(i.positionWS.xz + i.positionWS.y * .18,_Time.y) * saturate(n.y*.65+.3);
            float3 color = albedo * (ambient + light.color * (ndl + caustic*_ReefCaustics) * light.shadowAttenuation);
            color += light.color * spec * light.shadowAttenuation * .38;
            return half4(color,1);
        }
        ENDHLSL
        Pass
        {
            Tags {"LightMode"="UniversalForward"}
            Cull [_Cull]
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            ENDHLSL
        }
        Pass
        {
            Tags {"LightMode"="ShadowCaster"}
            ColorMask 0 ZWrite On Cull [_Cull]
            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing
            float3 _LightDirection;
            V ShadowVert(A i)
            {
                V o = vert(i);
                o.positionCS = TransformWorldToHClip(ApplyShadowBias(o.positionWS,o.normalWS,_LightDirection));
                #if UNITY_REVERSED_Z
                    o.positionCS.z = min(o.positionCS.z, UNITY_NEAR_CLIP_VALUE * o.positionCS.w);
                #else
                    o.positionCS.z = max(o.positionCS.z, UNITY_NEAR_CLIP_VALUE * o.positionCS.w);
                #endif
                return o;
            }
            half4 DepthFrag(V i):SV_Target {return 0;}
            ENDHLSL
        }
        Pass
        {
            Tags {"LightMode"="DepthOnly"}
            ColorMask R ZWrite On Cull [_Cull]
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing
            half4 DepthFrag(V i):SV_Target {return i.positionCS.z;}
            ENDHLSL
        }
    }
}
