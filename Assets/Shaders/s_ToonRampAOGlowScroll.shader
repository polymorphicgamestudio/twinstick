//uses unity_ShadowColor


Shader "Landon/Toon/Ramp AO Glow Scroll" {
	Properties {
		_MainTex ("Texture (RGB) AO (A)", 2D) = "white" {}
		[NoScaleOffset] _Glow ("AO (R) Glow Mask (G) Glow Pattern (B)", 2D) = "white" {}
		[NoScaleOffset] _Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}

		[HDR] _Color ("Glow Color", Color) = (0.5,0.5,0.5,1.0)
		_Scroll ("Scroll Speed", Range(-5,5)) = 0.5
	}
	
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		
		#pragma surface surf ToonyColorsCustom fullforwardshadows// noambient
		//#pragma target 2.0
		//#pragma glsl
		
		fixed4 _Color;
		fixed _Scroll;
		sampler2D _MainTex, _Glow, _Ramp;
		struct Input {
			half2 uv_MainTex;
		};
		
		//Custom SurfaceOutput
		struct SurfaceOutputCustom {
			fixed3 Albedo;
			fixed3 Normal;
			fixed3 Emission;
			//half Specular;
			fixed Alpha;
		};
		
		inline half4 LightingToonyColorsCustom (SurfaceOutputCustom s, half3 lightDir, half3 viewDir, half atten) {
			fixed ndl = max(0, dot(s.Normal, lightDir)*0.5 + 0.5);
			
			fixed3 ramp = tex2D(_Ramp, fixed2(ndl,ndl));
			#if !(POINT) && !(SPOT)
				ramp *= atten;
			#endif

			//AO stored in alpha
			ramp *= s.Alpha;

			ramp = lerp(unity_ShadowColor.rgb,fixed3(1,1,1),ramp);
			fixed4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * ramp;
			c.a = 1;
			#if (POINT || SPOT)
				c.rgb *= atten;
			#endif

			return c;
		}
		
		void surf (Input IN, inout SurfaceOutputCustom o) {
			fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 glowTex = tex2D(_Glow, IN.uv_MainTex);

			fixed2 scroll = fixed2(0, frac(_Time.x*_Scroll));
			fixed3 glowScroll = tex2D(_Glow, IN.uv_MainTex - scroll);

			o.Albedo = mainTex.rgb;
			o.Alpha = mainTex.a;
			o.Emission = glowTex.r * glowScroll.g * _Color;
		}
		ENDCG
	}
	Fallback "Diffuse"
}