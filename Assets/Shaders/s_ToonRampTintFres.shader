Shader "Landon/Toon/RampAOFresTEST" {
	Properties {
		[MainTexture] _MainTex ("Main R, Mouth G, Teeth B, AO A", 2D) = "white" {}
		[NoScaleOffset] _Ramp ("Toon Ramp RGB", 2D) = "gray" {}
		[MainColor] _Color ("Main Color RGB", Color) = (0.5,0.5,0.5,1.0)
		_ColorFres ("Fres Color RGB, Fres Glow A", Color) = (0.5,0.5,0.5,1.0)
		_ColorMouth ("Mouth Color RGB", Color) = (0.5,0.5,0.5,1.0)
		_ColorTeeth ("Teeth Color RGB, Glow A", Color) = (0.5,0.5,0.5,1.0)
		_RimMin ("Rim Min", Range(0,1)) = 0.5
		_RimMax ("Rim Max", Range(0,1)) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		CGPROGRAM
		
		#pragma surface surf ToonyColorsCustom fullforwardshadows
		//#pragma target 2.0
		//#pragma glsl
		
		fixed4 _Color, _ColorFres, _ColorMouth, _ColorTeeth;
		fixed _RimMin, _RimMax;
		sampler2D _MainTex, _Ramp;

		struct Input {
			half2 uv_MainTex;
			half3 viewDir;
		};
		struct SurfaceOutputCustom {
			fixed3 Albedo;
			fixed3 Normal;
			fixed3 Emission;
			half Specular;
			fixed Alpha;
		};
		inline half4 LightingToonyColorsCustom (SurfaceOutputCustom s, half3 lightDir, half3 viewDir, half atten) {
			s.Normal = normalize(s.Normal);

			fixed3 main = s.Albedo * _LightColor0.rgb;

			//Fres
			half rim = 1.0f - saturate(dot(viewDir, s.Normal));
			rim = smoothstep(_RimMin, _RimMax, rim);
			half3 rimColor = (_ColorFres.rgb * rim) * _ColorFres.a;
			//Fres mask stored in Specular;
			fixed3 fres = rimColor * s.Specular * s.Specular;

			//Shadow
			fixed ndl = max(0, dot(s.Normal, lightDir)*0.5 + 0.5);
			fixed3 shadow = tex2D(_Ramp, fixed2(ndl,ndl));
			#if !(POINT) && !(SPOT)
				shadow *= atten;
			#endif
			//Add AO to shadow. AO stored in alpha
			shadow *= s.Alpha;
			//Color shadow
			shadow = saturate(shadow + unity_ShadowColor.rgb);

			//Combine
			fixed4 c;
			c.rgb = main * shadow + fres;
			c.a = 1;
			#if (POINT || SPOT)
			c.rgb *= atten;
			#endif

			return c;
		}
		void surf (Input IN, inout SurfaceOutputCustom o) {
			fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex);
			
			// Fres
			half rim = 1.0f - saturate( dot(IN.viewDir, o.Normal) );
			rim = smoothstep(_RimMin, _RimMax, rim);
			//half3 mainColor = rim * _ColorFres.rgb  +  (1-rim) * _Color.rgb;

			half3 mainColor = rim * _ColorFres.rgb  +  (1-rim) * _Color.rgb;



			o.Albedo = mainColor * mainTex.r + _ColorMouth * mainTex.g + _ColorTeeth * mainTex.b;
			
			//make teeth glow
			o.Emission = _ColorTeeth.rgb * mainTex.b * _ColorTeeth.a;
			//store Fres mask in Specular;
			o.Specular = mainTex.r;
			//store AO in Alpha
			o.Alpha = mainTex.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
}