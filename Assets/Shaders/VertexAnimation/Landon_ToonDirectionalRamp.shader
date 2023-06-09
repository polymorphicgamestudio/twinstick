Shader "Landon/Toon/Directional Ramp" {
	Properties {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
		[NoScaleOffset] _Ramp ("Ramp", 2D) = "gray" {}
		[HDR] _DayCol("Day Col", Color) = (1,1,1,1)
		[HDR] _NightCol("Night Col", Color) = (1,1,1,1)
    }

	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// include file that contains UnityObjectToWorldNormal helper function
			#include "UnityCG.cginc"

			struct appdata {
                float4 vertex : POSITION; // vertex position
                float2 uv : TEXCOORD0; // texture coordinate
            };

			struct v2f {
				float2 uv : TEXCOORD0;
				half3 worldNormal : TEXCOORD1;
				float4 pos : SV_POSITION;
			};

			v2f vert (float4 vertex : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0) {
				v2f o;
				o.pos = UnityObjectToClipPos(vertex);
				o.worldNormal = UnityObjectToWorldNormal(normal);
				o.uv = uv;
				return o;
			}

			sampler2D _MainTex, _Ramp;
			float4 _DayCol, _NightCol;
			float _DayThreshold, _DayBlend;
            
			fixed4 frag (v2f i) : SV_Target {

				fixed4 col = tex2D(_MainTex, i.uv);
				float3 normal = i.worldNormal;
				float3 dirToSun = _WorldSpaceLightPos0;

				fixed ndl = max(0, dot(normal, dirToSun)*0.5 + 0.5);
				fixed3 ramp = tex2D(_Ramp, fixed2(ndl,ndl));
                //return col;
				ramp = lerp(_NightCol.rgb,_DayCol.rgb,ramp);
				fixed4 c;
				c.rgb = col * ramp;
				return c;
			}
			ENDCG
        }
    }
	// for cast-shadow
	FallBack "VertexLit"
}