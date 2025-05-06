Shader "SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		_OutlineColor("Outline Color", Color) = (1, 0, 0, 1)
		_OutlineExtrusion("Outline extrusion", Float) = 0.1
		_AlphaRange("Alpha Range", Range(0.0, 1.0)) = 0.25
		[KeywordEnum(2D,25D)] _Mode("Mode", int) = 0
    }
    SubShader
    {
        Tags 
		{ 
			"RenderType"="Transparent"
			"Queue"="Transparent"
		}

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float4 color : COLOR0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float4 color: COLOR0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST, _MainColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                return col * i.color;
            }
            ENDCG
        }


		Pass
		{
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Off

			CGPROGRAM

			#pragma shader_feature _MODE_2D _MODE_25D

			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v_extention
			{
				uint adjacency_vertices;
				float2 tangent;
			};

			float4 _OutlineColor;
			float _OutlineExtrusion, _AlphaRange;
			uniform StructuredBuffer<v_extention> _VertexExtention;
			uniform uint _VerticesCount;

			struct appdata
			{
				float4 vertex : POSITION0;
				uint vertexId : SV_VertexID;
			};

			struct v2g
			{
				float4 vertex : POSITION0;
				uint vertexId : TEXCOORD0;
			};

			struct g2f
			{
				float4 vertex : POSITION0;
				float alpha: TEXCOORD0;
			};


			v2g vert(appdata v)
			{
				v2g o;
				o.vertex = v.vertex;
				o.vertexId = v.vertexId;
				return o;
			}

			inline void Fill(inout g2f o, float alpha, in float4 vertex)
			{
				o.vertex = UnityObjectToClipPos(vertex);
				o.alpha = alpha;
			}

			void EmitEdgeGeometry(in v2g in1, in v2g in2, inout TriangleStream<g2f> triStream)
			{
				g2f o_11_m, o_11_p;
				g2f o_21_m, o_21_p;
				g2f o_1, o_2;

				uint index1 = in1.vertexId;
				uint index2 = in2.vertexId;

				v_extention ve1 = _VertexExtention[index1];
				v_extention ve2 = _VertexExtention[index2];

				float4 v1 = in1.vertex;
				float4 v2 = in2.vertex;

				float3 tangent1 = float3(ve1.tangent, 0.0f);
				float3 tangent2 = float3(ve2.tangent, 0.0f);


				#if _MODE_2D

				float3 viewDir1 = float3(0.0f, 0.0f, 1.0f);
				float3 viewDir2 = float3(0.0f, 0.0f, 1.0f);

				#endif


				#if _MODE_25D

				float3 objSpaceCameraPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0)).xyz;

				float3 viewDir1 = normalize(objSpaceCameraPos - v1);
				float3 viewDir2 = normalize(objSpaceCameraPos - v2);

				#endif

				float3 height1 = normalize(cross(viewDir1, tangent1));
				float3 height2 = normalize(cross(viewDir2, tangent2));

				float3 extrusionVec11 = height1 * _OutlineExtrusion;
				float3 extrusionVec21 = height2 * _OutlineExtrusion;


				extrusionVec21 *= lerp(1.0f, -1.0f, (dot(extrusionVec11, extrusionVec21) < 0));

				Fill(o_1, 1.0f, v1);
				Fill(o_2, 1.0f, v2);

				Fill(o_11_p, 0.0f, float4(v1.xyz + extrusionVec11, 1.0f));
				Fill(o_11_m, 0.0f, float4(v1.xyz - extrusionVec11, 1.0f));

				Fill(o_21_p, 0.0f, float4(v2.xyz + extrusionVec21, 1.0f));
				Fill(o_21_m, 0.0f, float4(v2.xyz - extrusionVec21, 1.0f));

				triStream.Append(o_1);
				triStream.Append(o_2);
				triStream.Append(o_11_p);
				triStream.RestartStrip();

				triStream.Append(o_21_p);
				triStream.Append(o_2);
				triStream.Append(o_11_p);
				triStream.RestartStrip();

				triStream.Append(o_1);
				triStream.Append(o_2);
				triStream.Append(o_11_m);
				triStream.RestartStrip();

				triStream.Append(o_21_m);
				triStream.Append(o_2);
				triStream.Append(o_11_m);
				triStream.RestartStrip();

			}

			inline bool CheckEdge(in v2g in1, in v2g in2)
			{
				uint index1 = in1.vertexId;
				uint index2 = in2.vertexId;

				v_extention ve1 = _VertexExtention[index1];
				v_extention ve2 = _VertexExtention[index2];

				uint edgeEncoded1 = ve1.adjacency_vertices;
				uint edgeEncoded2 = ve2.adjacency_vertices;

				uint adj11 = edgeEncoded1 % _VerticesCount;
				uint adj12 = edgeEncoded1 / _VerticesCount;

				uint adj21 = edgeEncoded2 % _VerticesCount;
				uint adj22 = edgeEncoded2 / _VerticesCount;

				bool isValidEdge = (adj11 > 0) && (adj12 > 0) && (adj21 > 0) && (adj22 > 0);

				adj11--;
				adj12--;
				adj21--;
				adj22--;

				bool isOutlineEdge = (adj11 == index2) || (adj12 == index2) || (index1 == adj21) || (index1 == adj22);

				return  isValidEdge && isOutlineEdge;
			}


			void EmitEdgeGeometryIfNeed(in v2g in1, in v2g in2, inout TriangleStream<g2f> triStream)
			{
				if (CheckEdge(in1, in2))
				{
					EmitEdgeGeometry(in1, in2, triStream);
				}
			}


			[maxvertexcount(36)]
			void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
			{
				EmitEdgeGeometryIfNeed(IN[0], IN[1], triStream);
				EmitEdgeGeometryIfNeed(IN[0], IN[2], triStream);
				EmitEdgeGeometryIfNeed(IN[1], IN[2], triStream);
			}


			float4 frag(g2f i) : SV_Target
			{
				float4 col = _OutlineColor;

				float alpha = i.alpha;
				alpha /= _AlphaRange;
				alpha = saturate(alpha);
				col.a *= alpha;

				return col;
			}
			ENDCG
		}
    }
}