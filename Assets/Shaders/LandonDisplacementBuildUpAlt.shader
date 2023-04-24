//uses unity_ShadowColor

Shader "Landon/Toon/DisplacementBuildUpAlt" {
	Properties {
		_MainTex ("Texture (RGB) AO (A)", 2D) = "white" {}
		[NoScaleOffset] _Glow ("Glow Mask (R) Glow Pattern (G)", 2D) = "white" {}
		[NoScaleOffset] _Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}

		[HDR] _Color ("Glow Color", Color) = (0.5,0.5,0.5,1.0)
		_Scroll ("Glow Scroll", Range(0,2)) = 1

		_ShaderDisplacement("_ShaderDisplacement", Range(0,1)) = 1
		[HDR] _DispColor ("Displace Glow Color", Color) = (0.5,0.5,0.5,1.0)
		_ObjectHigh("ObjectHigh", Float) = 1
		_ObjectLow("ObjectLow", Float) = 0
	}
	
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		//addshadows tells the surface shader to generate a new shadow pass based on out vertex shader
		#pragma surface surf ToonyColorsCustom fullforwardshadows vertex:vertexDataFunc addshadow

		uniform float _ShaderDisplacement, _ObjectLow, _ObjectHigh;

		fixed4 _Color, _DispColor;
		fixed _Scroll;
		sampler2D _MainTex, _Glow, _Ramp;
		struct Input {
			half2 uv_MainTex;
			float3 worldPos;
		};
		
		//Custom SurfaceOutput
		struct SurfaceOutputCustom {
			fixed3 Albedo;
			fixed3 Normal;
			fixed3 Emission;
			//half Specular;
			fixed Alpha;
		};
		
		void vertexDataFunc( inout appdata_full v, out Input o ) {
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float fillAmount = _ObjectLow + (_ShaderDisplacement - 0.0) * (_ObjectHigh - _ObjectLow);
			float vertexYPos = v.vertex.y;
			float level = (fillAmount - vertexYPos);
			float3 range = float3(0.0, clamp(level, 0.0, level), 0.0);
			float smoothRelease = smoothstep((fillAmount - 0.5 ), (fillAmount), vertexYPos);
			v.vertex.xyz += (range * smoothRelease).xyz;
			v.vertex.w = 1;
		}

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
			fixed3 glowScroll = tex2D(_Glow, IN.uv_MainTex + fixed2(0, _Scroll));
			float3 lights = glowTex.r * glowScroll.g * _Color.rgb * _Color.a;

			float fillAmount = (0.0 + (_ObjectLow + (_ShaderDisplacement - 0.0) * (_ObjectHigh - _ObjectLow) / (1.0 - 0.0)));
			float vertexYPos = mul(unity_WorldToObject, float4(IN.worldPos , 1)).y;
			float displaceGlowMask = clamp(smoothstep((fillAmount - 0.5), fillAmount, vertexYPos), 0, 1);
			
			//float maskCut = displaceGlowMask > 0.99 ? 0 : displaceGlowMask;
			//o.Emission = lerp(lights, float3(0,0,0), displaceGlowMask) + lerp(float3(0,0,0), _DispColor.rgb, maskCut);
			
			o.Albedo = lerp(mainTex.rgb, float3(0,0,0), displaceGlowMask);
			o.Alpha = mainTex.a;
			float3 displaceToLight = lerp(lights, _DispColor.rgb, displaceGlowMask * displaceGlowMask);
			float topEnd = clamp(displaceGlowMask * 20 - 19,0,1);
			o.Emission = lerp(displaceToLight, float3(0,0,0), topEnd);
		}
		ENDCG
	}
	Fallback "Diffuse"
}