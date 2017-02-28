﻿Shader "Voxel Importer/VertexColor" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
	}

	SubShader {
		Tags {"RenderType"="Opaque"}
	
		Lighting Off
		Offset -2, -2

		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
			
				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float4 color : COLOR;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					float4 color : COLOR;
				};

				fixed4 _Color;
			
				v2f vert (appdata_t v)
				{
					v2f o;
#if UNITY_VERSION >= 540
					o.vertex = UnityObjectToClipPos(v.vertex);
#else
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
#endif
					o.color = v.color;
					return o;
				}
			
				fixed4 frag (v2f i) : COLOR
				{
					fixed4 col = _Color * i.color;
					return col;
				}
			ENDCG
		}
	}
}
