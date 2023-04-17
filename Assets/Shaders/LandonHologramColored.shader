Shader "Landon/Hologram Colored" {
	Properties {
		_Scale("Scale", Float) = 1
		_Power("Power", Float) = 1
		_Light("Brightness", Float) = 1
		_ColorTex("Color Texture", 2D) = "white" {}
		_Hologram("Hologram", 2D) = "white" {}
		_UV("UV", Float) = 1
		_VertexMovement("VertexMovement", Float) = 0.05
	}

	SubShader {
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Offset 1,1//lose Z Fights
		Pass { ZWrite On ColorMask 0 }

		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature _INVERT_ON
		struct Input {
			float3 worldPos;
			float3 worldNormal;
			float2 uv_ColorTex;
		};

		uniform sampler2D _Hologram;
		uniform float _UV;
		uniform float _Scale, _Power, _Light;
		uniform float _VertexMovement;
		sampler2D _ColorTex;

		void vertexDataFunc( inout appdata_full v, out Input o ) {
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 scrollDirection = float2(0.0 , 0.1);
			float3 worldPosition = mul( unity_ObjectToWorld, v.vertex );
			float2 appendResult10 = float2(worldPosition.x , worldPosition.y);
			float2 scrollingUVs = _Time.y * scrollDirection + ( appendResult10 * _UV );
			float3 worldViewDirection = normalize( UnityWorldSpaceViewDir( worldPosition ) );
			float3 worldNormals = UnityObjectToWorldNormal( v.normal );
			float fresnelDirection = dot( worldNormals, worldViewDirection );
			float fresnel = ( 0.0 + _Scale * pow( 1.0 - fresnelDirection, _Power ) );
			
			float4 temp_output_18_0 = ( tex2Dlod( _Hologram, float4( scrollingUVs, 0, 0.0) ) * fresnel );
			v.vertex.xyz += ( temp_output_18_0 * _VertexMovement ).rgb;
		}

		void surf( Input i , inout SurfaceOutputStandard o ) {
			o.Emission = tex2D (_ColorTex, i.uv_ColorTex).rgb * _Light;
			float2 scrollDirection = (float2(0.0 , 0.1));
			float3 worldPosition = i.worldPos;
			float2 appendResult10 = (float2(worldPosition.x , worldPosition.y));
			float2 scrollingUVs = ( 1.0 * _Time.y * scrollDirection + ( appendResult10 * _UV ));
			float3 worldViewDirection = normalize( UnityWorldSpaceViewDir( worldPosition ) );
			float3 worldNormals = i.worldNormal;
			float fresnelDirection = dot( worldNormals, worldViewDirection );
			float fresnel = ( 0.0 + _Scale * pow( 1.0 - fresnelDirection, _Power ) );
			
			float4 temp_output_18_0 = ( tex2D( _Hologram, scrollingUVs ) * fresnel );
			float4 clampResult31 = clamp( temp_output_18_0 , float4( 0,0,0,0 ) , float4( 1,0,0,0 ) );
			o.Alpha = clampResult31.r;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows vertex:vertexDataFunc 
		ENDCG
	}
}