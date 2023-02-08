Shader "Landon/Hologram" {
	Properties {
		_Scale("Scale", Float) = 1
		_Power("Power", Float) = 1
		[HDR]_EmissionColor("EmissionColor", Color) = (0,0,0,0)
		_Hologram("Hologram", 2D) = "white" {}
		[Toggle(_INVERT_ON)] _Invert("Invert", Float) = 0
		_UV("UV", Float) = 1
		_VertexMovement("VertexMovement", Float) = 0.05
		[HideInInspector] __dirty( "", Int ) = 1
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
		};

		uniform sampler2D _Hologram;
		uniform float _UV;
		uniform float _Scale;
		uniform float _Power;
		uniform float _VertexMovement;
		uniform float4 _EmissionColor;

		void vertexDataFunc( inout appdata_full v, out Input o ) {
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 appendResult24 = (float2(0.0 , 0.1));
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 appendResult10 = (float2(ase_worldPos.x , ase_worldPos.y));
			float2 panner13 = ( 1.0 * _Time.y * appendResult24 + ( appendResult10 * _UV ));
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float fresnelNdotV28 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode28 = ( 0.0 + _Scale * pow( 1.0 - fresnelNdotV28, _Power ) );
			float clampResult30 = clamp( ( 1.0 - fresnelNode28 ) , 0.0 , 1.0 );
			#ifdef _INVERT_ON
				float staticSwitch25 = clampResult30;
			#else
				float staticSwitch25 = fresnelNode28;
			#endif
			float4 temp_output_18_0 = ( tex2Dlod( _Hologram, float4( panner13, 0, 0.0) ) * staticSwitch25 );
			v.vertex.xyz += ( temp_output_18_0 * _VertexMovement ).rgb;
		}

		void surf( Input i , inout SurfaceOutputStandard o ) {
			o.Emission = _EmissionColor.rgb;
			float2 appendResult24 = (float2(0.0 , 0.1));
			float3 ase_worldPos = i.worldPos;
			float2 appendResult10 = (float2(ase_worldPos.x , ase_worldPos.y));
			float2 panner13 = ( 1.0 * _Time.y * appendResult24 + ( appendResult10 * _UV ));
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV28 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode28 = ( 0.0 + _Scale * pow( 1.0 - fresnelNdotV28, _Power ) );
			float clampResult30 = clamp( ( 1.0 - fresnelNode28 ) , 0.0 , 1.0 );
			#ifdef _INVERT_ON
				float staticSwitch25 = clampResult30;
			#else
				float staticSwitch25 = fresnelNode28;
			#endif
			float4 temp_output_18_0 = ( tex2D( _Hologram, panner13 ) * staticSwitch25 );
			float4 clampResult31 = clamp( temp_output_18_0 , float4( 0,0,0,0 ) , float4( 1,0,0,0 ) );
			o.Alpha = clampResult31.r;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows vertex:vertexDataFunc 
		ENDCG
	}
}