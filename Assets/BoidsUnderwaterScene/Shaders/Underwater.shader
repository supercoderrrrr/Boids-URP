Shader "BoidsUnderwaterScene/Underwater"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "UnderwaterCommon.hlsl"
        TEXTURE2D_X(_UnderwaterScattering);
        float4 _UnderwaterScattering_TexelSize;
        float _UnderwaterSurfaceHeight;
        float3 _UnderwaterAbsorption;
        float4 _UnderwaterFogTint;
        float _UnderwaterFogDensity,_UnderwaterShaftIntensity,_UnderwaterDistortion;
        float _UnderwaterChromaticPixels;
        float3 Position(float2 uv)
        {
            float depth = SampleSceneDepth(uv);
            #if !UNITY_REVERSED_Z
                depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
            #endif
            return ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP);
        }
        float WaterDistance(float3 p)
        {
            float3 v = p - _WorldSpaceCameraPos;
            float len = min(length(v), 140.0);
            float up = v.y / max(length(v), .001);
            if (up > .001) len = min(len, max(0, _UnderwaterSurfaceHeight - _WorldSpaceCameraPos.y) / up);
            return len;
        }
        half4 Scatter(Varyings input) : SV_Target
        {
            float3 p = Position(input.texcoord);
            float3 ray = normalize(p - _WorldSpaceCameraPos);
            float distanceInWater = WaterDistance(p);
            float stepLength = min(distanceInWater,80.0) / 32.0;
            float jitter = frac(frac(dot(input.positionCS.xy, float2(.06711056, .00583715))) * 52.9829189);
            float integral = 0;
            Light sun = GetMainLight();
            [unroll] for (int i = 0; i < 32; i++)
            {
                float d = (i + jitter) * stepLength;
                float3 samplePosition = _WorldSpaceCameraPos + ray * d;
                float shadow = MainLightRealtimeShadow(TransformWorldToShadowCoord(samplePosition));
                float surfaceLight = exp(-max(0, _UnderwaterSurfaceHeight - samplePosition.y) * .024);
                float2 entry=samplePosition.xz+sun.direction.xz/max(sun.direction.y,.15)*max(0,_UnderwaterSurfaceHeight-samplePosition.y);
                float aperture=sin(entry.x*.73+sin(entry.y*.31+_Time.y*.09)*1.5)*.5+.5;
                float mottling = .10 + 1.25 * pow(aperture,6);
                integral += shadow * surfaceLight * mottling * exp(-d * .023) * stepLength;
            }
            float phase = .3 + .7 * pow(saturate(dot(ray, sun.direction) * .5 + .5), 6);
            return half4(sun.color * float3(.42, .75, .69) * integral * _UnderwaterShaftIntensity * phase, min(length(p - _WorldSpaceCameraPos), 140));
        }
        half4 Composite(Varyings input) : SV_Target
        {
            float2 uv = input.texcoord;
            float3 p = Position(uv);
            float distanceInWater = WaterDistance(p);
            float depth = min(length(p - _WorldSpaceCameraPos), 140);
            float edge = saturate(min(min(uv.x, 1-uv.x), min(uv.y, 1-uv.y)) * 30);
            float2 flow = float2(sin(uv.y * 15 + _Time.y * .31), sin(uv.x * 13 - _Time.y * .27));
            float2 displaced = uv + flow * _UnderwaterDistortion * edge * saturate(distanceInWater / 15);
            half3 source = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, displaced).rgb;
            // Pixel-sized channel offsets stay subtle across render resolutions
            float2 radial = (uv - .5) * 2;
            float2 chromatic = radial * dot(radial,radial) / _ScaledScreenParams.xy * _UnderwaterChromaticPixels;
            source.r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, displaced + chromatic).r;
            source.b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, displaced - chromatic).b;
            float3 transmission = exp(-_UnderwaterAbsorption * distanceInWater);
            float3 scatterColor = _UnderwaterFogTint.rgb;
            float scatter = 1 - exp(-distanceInWater * _UnderwaterFogDensity);
            float3 volume = 0; float total = 0;
            [unroll] for (int k=0; k<4; k++)
            {
                float2 offset = float2(k % 2, k / 2) - .5;
                float4 sample = SAMPLE_TEXTURE2D_X(_UnderwaterScattering, sampler_LinearClamp, uv + offset * _UnderwaterScattering_TexelSize.xy);
                float weight = 1.0 / (1.0 + abs(sample.a - depth) * 2.0);
                volume += sample.rgb * weight; total += weight;
            }
            return half4(source * transmission + scatterColor * scatter + volume / max(total, .001), 1);
        }
        ENDHLSL
        Pass
        {
            Name "Half resolution shadowed scattering"
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Scatter
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            ENDHLSL
        }
        Pass
        {
            Name "Depth aware underwater composite"
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Composite
            ENDHLSL
        }
    }
}
