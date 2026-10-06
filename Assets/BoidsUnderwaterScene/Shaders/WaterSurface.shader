Shader "BoidsUnderwaterScene/WaterSurface"
{
    Properties
    {
        [Normal] _NormalMap("Surface normal", 2D)="bump" {}
        _WaveHeight("Wave height", Range(0,1))=.28
    }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
        Pass
        {
            Tags {"LightMode"="UniversalForward"}
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "UnderwaterCommon.hlsl"
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            CBUFFER_START(UnityPerMaterial)
            float _WaveHeight;
            CBUFFER_END
            struct A {float4 positionOS:POSITION;};
            struct V {float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0;};
            V vert(A i)
            {
                V o; float3 p = TransformObjectToWorld(i.positionOS.xyz);
                p.y += (sin(p.x * .24 + p.z * .13 + _Time.y * .6) + .48 * sin(p.z * .36 - p.x * .17 - _Time.y * .43)) * _WaveHeight;
                o.positionCS = TransformWorldToHClip(p); o.positionWS = p; return o;
            }
            half4 frag(V i):SV_Target
            {
                float2 uv = i.positionWS.xz * .12;
                float3 n1 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,uv + float2(.012,.017) * _Time.y));
                float3 n2 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,uv * .63 + float2(-.013,.009) * _Time.y));
                float3 n = normalize(float3((n1.xy + n2.xy) * .25, 1)).xzy;
                float3 v = normalize(_WorldSpaceCameraPos - i.positionWS);
                float cosine = abs(dot(n,v));
                float tir = 1 - smoothstep(.63, .79, cosine);
                float patterns = UnderwaterCaustics(i.positionWS.xz * .45, _Time.y);
                float3 sky = lerp(float3(.16,.39,.44),float3(.56,.78,.75), cosine);
                float3 internalReflection = float3(.018,.10,.115) * (1.0 + (n1.x+n2.y)*.32) + patterns * float3(.002,.008,.009);
                float3 color = lerp(sky, internalReflection, tir * .92);
                Light sun = GetMainLight();
                float3 sunThroughWater = normalize(float3(sun.direction.x * .75, sun.direction.y, sun.direction.z * .75));
                float highlight = pow(saturate(dot(-v, sunThroughWater + (n-float3(0,1,0)) * .45)), 160);
                color += highlight * float3(2.1,2.0,1.55);
                return half4(color,1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Unlit/DepthOnly"
    }
}
