Shader "Landon/TextureAnimation/CrabSlime_GpuInstance"{
	Properties{
		_MainTex ("Texture", 2D) = "white" {}
		[NoScaleOffset] _Ramp ("Ramp", 2D) = "gray" {}


		[MainColor] _Color ("Main Color RGB", Color) = (0.5,0.5,0.5,1.0)
		_ColorMouth ("Mouth Color RGB", Color) = (0.5,0.5,0.5,1.0)
		_ColorTeeth ("Teeth Color RGB, Glow A", Color) = (0.5,0.5,0.5,1.0)

		[NoScaleOffset] _PosTex("position texture", 2D) = "black"{}

		_DT("delta time", float) = 0
		_Length("animation length", Float) = 1
	}
	SubShader{
		Tags { "RenderType"="Opaque" "LightMode"="ForwardBase"}

		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#pragma multi_compile_instancing

			#include "UnityCG.cginc" // for UnityObjectToWorldNormal
			#include "UnityLightingCommon.cginc" // for _LightColor0

			#pragma target 3.0

			struct appdata{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f{
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler2D _MainTex, _PosTex, _Ramp;
			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
				UNITY_DEFINE_INSTANCED_PROP(float4, _PosTex_TexelSize)
				UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
				UNITY_DEFINE_INSTANCED_PROP(float4, _ColorMouth)
				UNITY_DEFINE_INSTANCED_PROP(float4, _ColorTeeth)
				UNITY_DEFINE_INSTANCED_PROP(float, _Length)
				UNITY_DEFINE_INSTANCED_PROP(float, _DT)
			UNITY_INSTANCING_BUFFER_END(Props)

			
			v2f vert (appdata v, uint vid : SV_VertexID, float3 normal : NORMAL){
				v2f o;
				o.normal = UnityObjectToWorldNormal(normal);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				//float t = (_Time.y - UNITY_ACCESS_INSTANCED_PROP(Props, _DT)) / UNITY_ACCESS_INSTANCED_PROP(Props, _Length);
				//t = fmod(t, 1.0);
				//float x = (vid + 0.5) * UNITY_ACCESS_INSTANCED_PROP(Props, _PosTex_TexelSize.x);
				//float y = t;
				//float4 pos = tex2Dlod(_PosTex, float4(x, y, 0, 0));


				//Walk
				float t = (_Time.y - UNITY_ACCESS_INSTANCED_PROP(Props, _DT)) / UNITY_ACCESS_INSTANCED_PROP(Props, _Length);
				t = fmod(t, 0.5);
				float x = (vid + 0.5) * UNITY_ACCESS_INSTANCED_PROP(Props, _PosTex_TexelSize.x);
				float y = t + 0.5;
				float4 pos = tex2Dlod(_PosTex, float4(x, y, 0, 0));




				o.vertex = UnityObjectToClipPos((pos + float3(-0.5,0,-0.3)) * 2.3);
				o.uv = TRANSFORM_TEX(v.uv, UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex));
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target{
				UNITY_SETUP_INSTANCE_ID(i);
				//fixed4 col = tex2D(_MainTex, i.uv);
				//return col;

				half4 col = tex2D(_MainTex, i.uv);
				float3 normal = i.normal;

				half4 col2 = col.r * UNITY_ACCESS_INSTANCED_PROP(Props, _Color) +
					col.g * UNITY_ACCESS_INSTANCED_PROP(Props, _ColorMouth) +
					col.b * UNITY_ACCESS_INSTANCED_PROP(Props, _ColorTeeth);

				float3 dirToSun = _WorldSpaceLightPos0;
				fixed ndl = max(0, dot(normal, dirToSun)*0.5 + 0.5);
				
				fixed3 ramp = tex2D(_Ramp, fixed2(ndl,ndl));
				ramp = lerp(unity_ShadowColor.rgb,_LightColor0.rgb,ramp);
				fixed4 c;
				c.rgb = col2 * ramp;
				return c;
			}
			ENDCG
		}
		Pass {
            Tags {"LightMode"="ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f { 
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
	}
		// for cast-shadow
	FallBack "VertexLit"
}