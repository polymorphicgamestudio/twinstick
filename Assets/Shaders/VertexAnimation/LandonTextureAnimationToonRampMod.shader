Shader "Landon/TextureAnimationToonRampMod" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}

		[NoScaleOffset] _Ramp ("Ramp", 2D) = "gray" {}
		[HDR] _DayCol("Day Col", Color) = (1,1,1,1)
		[HDR] _NightCol("Night Col", Color) = (1,1,1,1)

		_PosTex1("position texture", 2D) = "black"{}
		_PosTex2("position texture", 2D) = "black"{}

		_Blend ("Blend", Range(0.0,1.0)) = 1.0

		_DT ("delta time", float) = 0
		_Length ("animation length", Float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100 Cull Off

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			#define ts _PosTex1_TexelSize

			struct appdata {
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex, _Ramp, _PosTex1, _PosTex2, _NmlTex;
			float4 _DayCol, _NightCol, _PosTex1_TexelSize;
			float _DayThreshold, _DayBlend, _Length, _DT, _Blend;
			
			v2f vert (appdata v, uint vid : SV_VertexID, float3 normal : NORMAL) {
				float t = (_Time.y - _DT) / _Length;
				t = fmod(t, 1.0);
				float x = (vid + 0.5) * ts.x;
				float y = t;

				float4 pos1 = tex2Dlod(_PosTex1, float4(x, y, 0, 0));
				float4 pos2 = tex2Dlod(_PosTex2, float4(x, y, 0, 0));
				float4 pos = lerp(pos1,pos2,_Blend);

				v2f o;
				o.vertex = UnityObjectToClipPos(pos);
				o.normal = UnityObjectToWorldNormal(normal);
				o.uv = v.uv;
				return o;
			}
			
			half4 frag (v2f i) : SV_Target {
				half4 col = tex2D(_MainTex, i.uv);
				float3 normal = i.normal;
				float3 dirToSun = _WorldSpaceLightPos0;
				fixed ndl = max(0, dot(normal, dirToSun)*0.5 + 0.5);
				fixed3 ramp = tex2D(_Ramp, fixed2(ndl,ndl));
				ramp = lerp(_NightCol.rgb,_DayCol.rgb,ramp);
				fixed4 c;
				c.rgb = col * ramp;
				return c;
			}
			ENDCG
		}
	}
}