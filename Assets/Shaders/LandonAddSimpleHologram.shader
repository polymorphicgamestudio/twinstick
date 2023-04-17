Shader "Landon/Add Simple Hologram" {
	Properties {
		_ColorA ("ColorA (RGB), Stregth(A)", Color) = (0.5,0.5,0.5,0.5)
		_ColorB ("ColorB (RGB), Stregth(A)", Color) = (0.5,0.5,0.5,0.5)
		[NoScaleOffset] _MainTex ("Glow Area (R), HologramTex(B)", 2D) = "white" {}
		_RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
		_scroll ("ScrollSpeed", Range(0.0,1.0)) = 0.1
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha One
		Offset 1, 1 //lose ZFights
		Cull Back Lighting Off Fog { Color (0,0,0,0) }
		Pass { ZWrite On ColorMask 0 }

		CGPROGRAM
		#pragma surface surf Lambert
		struct Input {
			float2 uv_MainTex;
			float3 viewDir;
			float3 worldNormal;
			float3 worldPos;
		};
		sampler2D _MainTex;
		float4 _ColorA, _ColorB;
		float _RimPower,_scroll;

		void surf (Input IN, inout SurfaceOutput o) {
			float glow = tex2D (_MainTex, IN.uv_MainTex).r;

			float2 scrollDirection = float2(0.0 , 1.0) * _scroll;
			float2 scrollingUVs = _Time.y * scrollDirection + IN.worldPos.xy;
			float hologram = tex2D (_MainTex, scrollingUVs).b;

			half rim = saturate(1 - dot (IN.worldNormal, IN.viewDir));
			half fresnel = pow(rim, _RimPower);

			float3 colorA = _ColorA.rgb * 10 * _ColorA.a * hologram;
			float3 colorB =  _ColorB * 10 * _ColorB.a * (glow + fresnel);

			o.Albedo = colorA + colorB;
		}
		ENDCG
	}
}