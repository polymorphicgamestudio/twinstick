Shader "Landon/Toon/RampAOFres" {
	Properties {
		//TOONY COLORS
		_Color ("Color", Color) = (0.5,0.5,0.5,1.0)
		_MainTex ("Main Texture (RGB), Color Mask (A)", 2D) = "white" {}
		[NoScaleOffset] _AO ("Ambient Occlusion (R)", 2D) = "white" {}
		[NoScaleOffset] _Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}

		_RimColor ("Rim Color", Color) = (0.8,0.8,0.8,0.6)
		_RimMin ("Rim Min", Range(0,1)) = 0.5
		_RimMax ("Rim Max", Range(0,1)) = 1.0
	}
	
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		
		#pragma surface surf ToonyColorsCustom fullforwardshadows
		//#pragma target 2.0
		//#pragma glsl
		
		fixed4 _Color;
		sampler2D _MainTex, _AO;
		struct Input {
			half2 uv_MainTex;
		};
		
		//Lighting-related variables
		fixed4 _RimColor;
		fixed _RimMin, _RimMax;
		sampler2D _Ramp;
		
		//Custom SurfaceOutput
		struct SurfaceOutputCustom {
			fixed3 Albedo;
			fixed3 Normal;
			fixed3 Emission;
			half Specular;
			fixed Alpha;
		};
		
		inline half4 LightingToonyColorsCustom (SurfaceOutputCustom s, half3 lightDir, half3 viewDir, half atten) {
			s.Normal = normalize(s.Normal);
			fixed ndl = max(0, dot(s.Normal, lightDir)*0.5 + 0.5);
			
			fixed3 ramp = tex2D(_Ramp, fixed2(ndl,ndl));

			// Fres
			half rim = 1.0f - saturate( dot(viewDir, s.Normal) );
			rim = smoothstep(_RimMin, _RimMax, rim);
			half3 rimColor = (_RimColor.rgb * rim) * _RimColor.a;

			#if !(POINT) && !(SPOT)
				ramp *= atten;
			#endif

			//AO stored in alpha
			ramp = saturate((ramp + rimColor) * s.Alpha);

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
			
			o.Albedo = mainTex.rgb + (1 - mainTex.a) * _Color.rgb;
			//o.Alpha = mainTex.a * _Color.a;

			//store AO in Alpha
			o.Alpha = tex2D(_AO, IN.uv_MainTex);
		}
		ENDCG
	}
	Fallback "Diffuse"
}