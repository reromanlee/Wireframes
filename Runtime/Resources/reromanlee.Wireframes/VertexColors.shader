Shader "reromanlee/Wireframes/VertexColors" {

    Properties { 
        
    }

    SubShader {

        Tags { 
            "RenderType"="Opaque" 
        }

        LOD 100

        Pass {

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                half4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                // Unity passes vertex colors through unconverted, so in linear projects they are converted here to
                // match how the same Color looks on a material.
                #if !defined(UNITY_COLORSPACE_GAMMA)
                o.color.rgb = GammaToLinearSpace(o.color.rgb);
                #endif
                return o;
            }

            half4 frag (v2f i) : SV_Target {
                return i.color;
            }

            ENDCG

        }

    }

}