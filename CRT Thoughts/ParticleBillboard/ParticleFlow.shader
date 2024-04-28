Shader "SimuCat/Ballistic/Particle Dispersion"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}
        _Color("Particle Colour", color) = (1, 1, 1, 1)

        // Particle Decal Array
        _ArraySpacing("Array Spacing", Vector) = (0.1,0.1,0.1,0)

        // x,y,z count of array w= total.
        _ArrayDimension("Array Dimension", Vector) = (128,80,1,10240)
        _MarkerScale ("Marker Scale", Range(0.01,2)) = 1
        _Scale("Scale Demo",Float) = 1

        // Play Control
        _BaseTime("Base Time Offset", Float)= 0
        _PauseTime("Freeze time",Float) = 0
        _Play("Play Animation", Float) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
		    //#include "../include/spectrum_zucconi.cginc"
		    #include "../include/pcg_hash.cginc"

            #define ObjectScale length(unity_ObjectToWorld._m00_m10_m20)

            #define ObjectScaleVec float3( \
                length(unity_ObjectToWorld._m00_m10_m20),\
                length(unity_ObjectToWorld._m01_m11_m21),\
                length(unity_ObjectToWorld._m02_m12_m22))

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				uint id : SV_VertexID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };
            
            //#define M(U) tex2D(_MomentumMap, float2(U))
            #define M(U) tex2Dlod(_MomentumMap, float4(U))

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            float4 _ArraySpacing;
            float4 _ArrayDimension;
            float _MarkerScale;
            float _Scale;

            float _BaseTime;
            float _PauseTime;
            float _Play;

            // 2/Pi
            #define twoDivPi 0.63661977236758


            v2f vert (appdata v)
            {
                v2f o;
                uint quadID = v.id/3;

                // Get hash of quad ID and also random 0-1;
                uint idHash = pcg_hash(quadID);
                //uint idHashFlip = idHash ^ 0x7FFFF;
                float hsh01 = (float)(idHash & 0x7FFFFF);
                float div = 0x7FFFFF;
                hsh01 = hsh01/div;
                
                // Use vertex ID to work out which triangle corner we have
                uint cornerID = v.id%3;
                float3 centerOffset;

                switch(cornerID)
                {
                    case 2:
                        centerOffset = float3(0,0.57735027,0); 
                        break;
                    case 1:
                        centerOffset = float3(-.5,-0.288675135,0);
                        break;
                    default:
                        centerOffset = float3(0.5,-0.288675135,0);
                        break;
                }

                float3 vertexOffset = centerOffset*_ArraySpacing;
                float3 arrayPointInModel = v.vertex - vertexOffset;

                float markerScale =  _MarkerScale/_Scale;

                float2 modelCentre = ((_ArrayDimension.xy - float2(1,1)) * _ArraySpacing.xy);
                modelCentre *= 0.5; // Align centre
                /*
                    Move particle here
                float2 particlePos = modelCentre; // Stub
                quadCentreInModel = //posIsInside * quadCentreInModel + (1-posIsInside)*arrayPointInModel; 
                float3 quadCentreInModel = arrayPointInModel; 
                */
                vertexOffset *= markerScale;                    // Scale the quad corner offset to world, now we billboard
                v.vertex.xyz=arrayPointInModel+vertexOffset;

                float4 camModelCentre = float4(arrayPointInModel,1.0);
                float4 camVertexOffset = float4(vertexOffset,0);
                // Three steps in one line
                //      1) Inner step is to use UNITY_MATRIX_MV to get the camera-oriented coordinate of the centre of the billboard.
                //         Here, the xy coords of the billboarded vertex are always aligned to the camera XY so...
                //      2) Just add the scaled xy model offset to lock the vertex orientation to the camera view.
                //      3) Transform the result by the Projection matrix (UNITY_MATRIX_P) and we now have the billboarded vertex in clip space.

                o.vertex = mul(UNITY_MATRIX_P,mul(UNITY_MATRIX_MV, camModelCentre) + camVertexOffset);
                /*
                Standard code
                o.vertex = UnityObjectToClipPos (v.vertex);
                */
                o.color = float4(_Color.rgb,1.);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                col.rgb *= i.color.rgb;
                col.a *= i.color.a;
                if(col.a < 0)
                {
					clip(-1);
					col = 0;
				}
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
