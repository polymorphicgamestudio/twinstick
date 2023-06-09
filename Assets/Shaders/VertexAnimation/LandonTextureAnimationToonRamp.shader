Shader "Landon/TextureAnimationToonRamp" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}

		[NoScaleOffset] _Ramp ("Ramp", 2D) = "gray" {}
		[HDR] _DayCol("Day Col", Color) = (1,1,1,1)
		[HDR] _NightCol("Night Col", Color) = (1,1,1,1)

		_PosTex("position texture", 2D) = "black"{}
		_DT ("delta time", float) = 0
		_Length ("animation length", Float) = 1
		[Toggle(ANIM_LOOP)] _Loop("loop", Float) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100 Cull Off

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile ___ ANIM_LOOP

			#include "UnityCG.cginc"

			#define ts _PosTex_TexelSize

			struct appdata {
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex, _Ramp, _PosTex, _NmlTex;
			float4 _DayCol, _NightCol, _PosTex_TexelSize;
			float _DayThreshold, _DayBlend, _Length, _DT;
			
			v2f vert (appdata v, uint vid : SV_VertexID, float3 normal : NORMAL) {
				float t = (_Time.y - _DT) / _Length;
				#if ANIM_LOOP
					t = fmod(t, 1.0);
				#else
					t = saturate(t);
				#endif
				float x = (vid + 0.5) * ts.x;
				float y = t;
				float4 pos = tex2Dlod(_PosTex, float4(x, y, 0, 0));

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