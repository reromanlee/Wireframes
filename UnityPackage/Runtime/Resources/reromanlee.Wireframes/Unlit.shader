Shader "reromanlee/Wireframes/Unlit"
{
    // Unlit vertex colors on Wireframes meshes. Each pipeline gets its own SubShader: URP's and HDRP's only exist in
    // projects that have that pipeline, and the last one serves the Built-in Render Pipeline.
    Properties
    {
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("Depth Test", Float) = 4
        [Enum(Off, 0, On, 1)] _ZWrite ("Depth Write", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Destination Blend", Float) = 0
        [ToggleUI] _WireframesVertexAlpha ("Use Vertex Alpha", Float) = 0
        _WireframesAlphaScale ("Alpha Scale", Range(0, 1)) = 1
    }

    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.universal"
        }

        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            Name "Wireframes"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            ZTest [_ZTest]
            ZWrite [_ZWrite]
            Blend [_SrcBlend] [_DstBlend]
            Cull Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.reromanlee.wireframes/Runtime/Shaders/Wireframes.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half _WireframesVertexAlpha;
                half _WireframesAlphaScale;
            CBUFFER_END

            struct Attributes
            {
                float3 position : POSITION;
                half4 color : COLOR;
                float bone : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformWorldToHClip(WireframesSkin(input.position, input.bone));
                half alpha = lerp(1.0h, input.color.a, _WireframesVertexAlpha) * _WireframesAlphaScale;
                output.color = half4(WireframesColor(input.color.rgb), alpha);
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                return input.color;
            }
            ENDHLSL
        }
    }

    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.high-definition"
        }

        Tags { "RenderPipeline" = "HDRenderPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            Name "Wireframes"
            Tags { "LightMode" = "ForwardOnly" }

            ZTest [_ZTest]
            ZWrite [_ZWrite]
            Blend [_SrcBlend] [_DstBlend]
            Cull Off

            HLSLPROGRAM
            #pragma target 4.5
            #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "Packages/com.reromanlee.wireframes/Runtime/Shaders/Wireframes.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half _WireframesVertexAlpha;
                half _WireframesAlphaScale;
            CBUFFER_END

            struct Attributes
            {
                float3 position : POSITION;
                half4 color : COLOR;
                float bone : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                // HDRP renders relative to the camera.
                float3 world = WireframesSkin(input.position, input.bone);
                output.positionCS = TransformWorldToHClip(GetCameraRelativePositionWS(world));
                half alpha = lerp(1.0h, input.color.a, _WireframesVertexAlpha) * _WireframesAlphaScale;
                output.color = half4(WireframesColor(input.color.rgb), alpha);
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                return input.color;
            }
            ENDHLSL
        }
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            Name "Wireframes"

            ZTest [_ZTest]
            ZWrite [_ZWrite]
            Blend [_SrcBlend] [_DstBlend]
            Cull Off

            CGPROGRAM
            #pragma target 3.5
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "UnityCG.cginc"
            #include "Packages/com.reromanlee.wireframes/Runtime/Shaders/Wireframes.hlsl"

            half _WireframesVertexAlpha;
            half _WireframesAlphaScale;

            struct Attributes
            {
                float3 position : POSITION;
                half4 color : COLOR;
                float bone : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = mul(UNITY_MATRIX_VP, float4(WireframesSkin(input.position, input.bone), 1.0));
                half alpha = lerp(1.0h, input.color.a, _WireframesVertexAlpha) * _WireframesAlphaScale;
                output.color = half4(WireframesColor(input.color.rgb), alpha);
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                return input.color;
            }
            ENDCG
        }
    }
}
