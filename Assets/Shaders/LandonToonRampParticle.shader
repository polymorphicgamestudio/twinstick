Shader "Landon/Toon/RampParticle" {
	Properties {
		_HColor ("Highlight Color", Color) = (0.6,0.6,0.6,1.0)
		_SColor ("Shadow Color", Color) = (0.3,0.3,0.3,1.0)
		
		//_MainTex ("Main Texture (RGB)", 2D) = "white" {}
		
		_Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}
	}
	
	SubShader {
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType"="Transparent" }//"RenderType"="Opaque" }

		
		CGPROGRAM
		
		#pragma surface surf ToonyColorsCustom fullforwardshadows alpha:fade

		//sampler2D _MainTex;
		struct Input {
			//half2 uv_MainTex;
			float4 color : COLOR;
		};

		fixed4 _HColor, _SColor;
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
		#if !(POINT) && !(SPOT)
			ramp *= atten;
		#endif
			_SColor = lerp(_HColor, _SColor, s.Alpha);	//Shadows intensity through alpha
			ramp = lerp(_SColor.rgb,_HColor.rgb,ramp);
			fixed4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * ramp;
			c.a = s.Alpha;
		#if (POINT || SPOT)
			c.rgb *= atten;
		#endif
			return c;
		}
		
		void surf (Input IN, inout SurfaceOutputCustom o) {
			//fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex);
			
			o.Albedo = IN.color; //mainTex.rgb;
			o.Alpha = IN.color.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
}