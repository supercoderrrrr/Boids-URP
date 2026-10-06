Shader "BoidsUnderwaterScene/UnderwaterParticles"
{
    Properties { _BaseColor("Color",Color)=(.55,.8,.8,.18) _Ring("Bubble ring",Float)=0 }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor; float _Ring;
            CBUFFER_END
            struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
            struct V {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;float4 screen:TEXCOORD1;float eye:TEXCOORD2;};
            V vert(A i) { V o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;o.color=i.color; o.screen=ComputeScreenPos(o.positionCS);o.eye=-TransformWorldToView(TransformObjectToWorld(i.positionOS.xyz)).z;return o; }
            half4 frag(V i):SV_Target
            {
                float r=length(i.uv*2-1);
                float soft=pow(saturate(1-r*r),3);
                float rim=exp(-pow((r-.77)*15,2));
                float highlight=exp(-dot((i.uv-float2(.31,.72))*19,(i.uv-float2(.31,.72))*19));
                float2 rimDirection=(i.uv-.5)/max(length(i.uv-.5),.001);
                float rimLighting=pow(saturate(.5+.5*dot(rimDirection,float2(-.544,.839))),2);
                float ring=saturate(rim*(.28+rimLighting*.65)+highlight*.95);
                float depth=LinearEyeDepth(SampleSceneDepth(i.screen.xy/i.screen.w),_ZBufferParams);
                float fade=saturate((depth-i.eye)*1.6)*saturate((i.eye-.25)*.7);
                half3 color=_BaseColor.rgb*i.color.rgb*lerp(1,.6+highlight*1.4+rimLighting*.65,_Ring);
                return half4(color,lerp(soft,ring,_Ring)*_BaseColor.a*i.color.a*fade);
            }
            ENDHLSL
        }
    }
}
