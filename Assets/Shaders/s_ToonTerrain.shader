// Toony Colors Pro+Mobile 2
// (c) 2014-2022 Jean Moreno

Shader "Landon/Toon/Terrain"
{
	Properties
	{
		[TCP2HeaderHelp(Base)]
		[TCP2Separator]

		[TCP2Header(Ramp Shading)]
		_RampSmoothing ("Smoothing", Range(0.001,1)) = 0.5
		[TCP2Separator]
		[TCP2HeaderHelp(Terrain)]
		_HeightTransition ("Height Smoothing", Range(0, 1.0)) = 0.0
		[HideInInspector] TerrainMeta_maskMapTexture ("Mask Map", 2D) = "white" {}
		[Toggle(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)] _EnableInstancedPerPixelNormal("Enable Instanced per-pixel normal", Float) = 1.0
		[TCP2Separator]
		
		[TCP2TextureSingleLine] _NoTileNoiseTex ("Non-repeating Tiling Noise Texture", 2D) = "black" {}
		
		[HideInInspector] _Splat0 ("Layer 0 Albedo", 2D) = "gray" {}
		[HideInInspector] _Splat1 ("Layer 1 Albedo", 2D) = "gray" {}
		[HideInInspector] _Splat2 ("Layer 2 Albedo", 2D) = "gray" {}
		[HideInInspector] _Splat3 ("Layer 3 Albedo", 2D) = "gray" {}
		[HideInInspector] _Splat4 ("Layer 4 Albedo", 2D) = "gray" {}
		[HideInInspector] _Splat5 ("Layer 5 Albedo", 2D) = "gray" {}
		[HideInInspector] _Splat6 ("Layer 6 Albedo", 2D) = "gray" {}
		[HideInInspector] _Splat7 ("Layer 7 Albedo", 2D) = "gray" {}
		[HideInInspector] [NoScaleOffset] _Mask0 ("Layer 0 Mask", 2D) = "gray" {}
		[HideInInspector] [NoScaleOffset] _Mask1 ("Layer 1 Mask", 2D) = "gray" {}
		[HideInInspector] [NoScaleOffset] _Mask2 ("Layer 2 Mask", 2D) = "gray" {}
		[HideInInspector] [NoScaleOffset] _Mask3 ("Layer 3 Mask", 2D) = "gray" {}
		[HideInInspector] [NoScaleOffset] _Mask4 ("Layer 4 Mask", 2D) = "gray" {}
		[HideInInspector] [NoScaleOffset] _Mask5 ("Layer 5 Mask", 2D) = "gray" {}
		[HideInInspector] [NoScaleOffset] _Mask6 ("Layer 6 Mask", 2D) = "gray" {}
		[HideInInspector] [NoScaleOffset] _Mask7 ("Layer 7 Mask", 2D) = "gray" {}

		// Avoid compile error if the properties are ending with a drawer
		[HideInInspector] __dummy__ ("unused", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"Queue"="Geometry-100"
			"TerrainCompatible"="True"
			"SplatCount"="8"
		}

		CGINCLUDE

		#include "UnityCG.cginc"
		#include "UnityLightingCommon.cginc"	// needed for LightColor

		// Texture/Sampler abstraction
		#define TCP2_TEX2D_WITH_SAMPLER(tex)						UNITY_DECLARE_TEX2D(tex)
		#define TCP2_TEX2D_NO_SAMPLER(tex)							UNITY_DECLARE_TEX2D_NOSAMPLER(tex)
		#define TCP2_TEX2D_SAMPLE(tex, samplertex, coord)			UNITY_SAMPLE_TEX2D_SAMPLER(tex, samplertex, coord)
		#define TCP2_TEX2D_SAMPLE_LOD(tex, samplertex, coord, lod)	UNITY_SAMPLE_TEX2D_SAMPLER_LOD(tex, samplertex, coord, lod)

		// Terrain

		//================================================================
		// Terrain Shader specific
		
		//----------------------------------------------------------------
		// Per-layer variables
		
		CBUFFER_START(_Terrain)
			float4 _Control_ST;
			float4 _Control_TexelSize;
			half _HeightTransition;
			half _DiffuseHasAlpha0, _DiffuseHasAlpha1, _DiffuseHasAlpha2, _DiffuseHasAlpha3;
			half _LayerHasMask0, _LayerHasMask1, _LayerHasMask2, _LayerHasMask3;
			// half4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
		
			float4 _Control1_ST;
			float4 _Control1_TexelSize;
			half _DiffuseHasAlpha4, _DiffuseHasAlpha5, _DiffuseHasAlpha6, _DiffuseHasAlpha7;
			half _LayerHasMask4, _LayerHasMask5, _LayerHasMask6, _LayerHasMask7;
			// half4 _Splat4_ST, _Splat5_ST, _Splat6_ST, _Splat7_ST;
		
			#ifdef UNITY_INSTANCING_ENABLED
				float4 _TerrainHeightmapRecipSize;   // float4(1.0f/width, 1.0f/height, 1.0f/(width-1), 1.0f/(height-1))
				float4 _TerrainHeightmapScale;       // float4(hmScale.x, hmScale.y / (float)(kMaxHeight), hmScale.z, 0.0f)
			#endif
			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif
		CBUFFER_END
		
		//----------------------------------------------------------------
		// Terrain textures
		
		TCP2_TEX2D_WITH_SAMPLER(_Control);
		TCP2_TEX2D_WITH_SAMPLER(_Control1);
		
		#if defined(TERRAIN_BASE_PASS)
			TCP2_TEX2D_WITH_SAMPLER(_MainTex);
		#endif
		
		//----------------------------------------------------------------
		// Terrain Instancing
		
		#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
			#define ENABLE_TERRAIN_PERPIXEL_NORMAL
		#endif
		
		#ifdef UNITY_INSTANCING_ENABLED
			TCP2_TEX2D_NO_SAMPLER(_TerrainHeightmapTexture);
			TCP2_TEX2D_WITH_SAMPLER(_TerrainNormalmapTexture);
		#endif
		
		UNITY_INSTANCING_BUFFER_START(Terrain)
			UNITY_DEFINE_INSTANCED_PROP(float4, _TerrainPatchInstanceData)  // float4(xBase, yBase, skipScale, ~)
		UNITY_INSTANCING_BUFFER_END(Terrain)
		
		void TerrainInstancing(inout float4 positionOS, inout float3 normal, inout float2 uv)
		{
		#ifdef UNITY_INSTANCING_ENABLED
			float2 patchVertex = positionOS.xy;
			float4 instanceData = UNITY_ACCESS_INSTANCED_PROP(Terrain, _TerrainPatchInstanceData);
		
			float2 sampleCoords = (patchVertex.xy + instanceData.xy) * instanceData.z; // (xy + float2(xBase,yBase)) * skipScale
			float height = UnpackHeightmap(_TerrainHeightmapTexture.Load(int3(sampleCoords, 0)));
		
			positionOS.xz = sampleCoords * _TerrainHeightmapScale.xz;
			positionOS.y = height * _TerrainHeightmapScale.y;
		
			#ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
				normal = float3(0, 1, 0);
			#else
				normal = _TerrainNormalmapTexture.Load(int3(sampleCoords, 0)).rgb * 2 - 1;
			#endif
			uv = sampleCoords * _TerrainHeightmapRecipSize.zw;
		#endif
		}
		
		void TerrainInstancing(inout float4 positionOS, inout float3 normal)
		{
			float2 uv = { 0, 0 };
			TerrainInstancing(positionOS, normal, uv);
		}
		
		//----------------------------------------------------------------
		// Terrain Holes
		
		#if defined(_ALPHATEST_ON)
			TCP2_TEX2D_WITH_SAMPLER(_TerrainHolesTexture);
		
			void ClipHoles(float2 uv)
			{
				float hole = TCP2_TEX2D_SAMPLE(_TerrainHolesTexture, _TerrainHolesTexture, uv).r;
				clip(hole == 0.0f ? -1 : 1);
			}
		#endif
		
		//----------------------------------------------------------------
		// Height-based blending
		
		void HeightBasedSplatModify_8_Layers(inout half4 splatControl, inout half4 splatControl1, in half4 splatHeight, in half4 splatHeight1)
		{
			// We multiply by the splat Control weights to get combined height
			splatHeight *= splatControl.rgba;
			splatHeight1 *= splatControl1.rgba;
				
			half maxHeight = max(splatHeight.r, max(splatHeight.g, max(splatHeight.b, splatHeight.a)));
			half maxHeight1 = max(splatHeight1.r, max(splatHeight1.g, max(splatHeight1.b, splatHeight1.a)));
			maxHeight = max(maxHeight, maxHeight1);
					
			// Ensure that the transition height is not zero.
			half transition = max(_HeightTransition, 1e-5);
					
			// This sets the highest splat to "transition", and everything else to a lower value relative to that
			// Then we clamp this to zero and normalize everything
			half4 weightedHeights = splatHeight + transition - maxHeight.xxxx;
			weightedHeights = max(0, weightedHeights);
			half4 weightedHeights1 = splatHeight1 + transition - maxHeight.xxxx;
			weightedHeights1 = max(0, weightedHeights1);
		
			// We need to add an epsilon here for active layers (hence the blendMask again)
			// so that at least a layer shows up if everything's too low.
			weightedHeights = (weightedHeights + 1e-6) * splatControl;
			weightedHeights1 = (weightedHeights1 + 1e-6) * splatControl1;
					
			// Normalize (and clamp to epsilon to keep from dividing by zero)
			half sumHeight = max(dot(weightedHeights, half4(1, 1, 1, 1)), 1e-6);
			half sumHeight1 = max(dot(weightedHeights1, half4(1, 1, 1, 1)), 1e-6);
			sumHeight = max(sumHeight, sumHeight1);
			splatControl = weightedHeights / sumHeight.xxxx;
			splatControl1 = weightedHeights1 / sumHeight.xxxx;
		}
		
		// Shader Properties
		sampler2D _Splat0;
		sampler2D _Splat1;
		sampler2D _Splat2;
		sampler2D _Splat3;
		TCP2_TEX2D_NO_SAMPLER(_Splat4);
		TCP2_TEX2D_NO_SAMPLER(_Splat5);
		TCP2_TEX2D_NO_SAMPLER(_Splat6);
		TCP2_TEX2D_NO_SAMPLER(_Splat7);
		TCP2_TEX2D_WITH_SAMPLER(_Mask0);
		TCP2_TEX2D_NO_SAMPLER(_Mask1);
		TCP2_TEX2D_NO_SAMPLER(_Mask2);
		TCP2_TEX2D_NO_SAMPLER(_Mask3);
		TCP2_TEX2D_WITH_SAMPLER(_Mask4);
		TCP2_TEX2D_NO_SAMPLER(_Mask5);
		TCP2_TEX2D_NO_SAMPLER(_Mask6);
		TCP2_TEX2D_NO_SAMPLER(_Mask7);
		
		// Shader Properties
		float4 _Splat0_ST;
		float4 _Splat1_ST;
		float4 _Splat2_ST;
		float4 _Splat3_ST;
		float4 _Splat4_ST;
		float4 _Splat5_ST;
		float4 _Splat6_ST;
		float4 _Splat7_ST;
		float _RampSmoothing;

		// Non-repeating tiling
		sampler2D _NoTileNoiseTex;
		float4 _NoTileNoiseTex_TexelSize;
		
		// Non-repeating tiling texture fetch function
		// Adapted from: http://www.iquilezles.org/www/articles/texturerepetition/texturerepetition.htm (c) 2017 - Inigo Quilez - MIT License
		float4 tex2D_noTile(sampler2D samp, in float2 uv)
		{
			// sample variation pattern
			float k = tex2D(_NoTileNoiseTex, (1/_NoTileNoiseTex_TexelSize.zw) * uv).a; // cheap (cache friendly) lookup
		
			// compute index
			float index = k*8.0;
			float i = floor(index);
			float f = frac(index);
		
			// offsets for the different virtual patterns
			float2 offa = sin(float2(3.0,7.0)*(i+0.0)); // can replace with any other hash
			float2 offb = sin(float2(3.0,7.0)*(i+1.0)); // can replace with any other hash
		
			// compute derivatives for mip-mapping
			float2 dx = ddx(uv);
			float2 dy = ddy(uv);
		
			// sample the two closest virtual patterns
			float4 cola = tex2Dgrad(samp, uv + offa, dx, dy);
			float4 colb = tex2Dgrad(samp, uv + offb, dx, dy);
		
			// interpolate between the two virtual patterns
			return lerp(cola, colb, smoothstep(0.2,0.8,f-0.1*dot(cola-colb, 1)));
		}
		
		ENDCG

		// Main Surface Shader

		CGPROGRAM

		#pragma surface surf ToonyColorsCustom vertex:vertex_surface exclude_path:deferred exclude_path:prepass keepalpha nolightmap nofog nolppv addshadow
		#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
		#pragma target 3.0

		//================================================================
		// SHADER KEYWORDS

		#pragma shader_feature_local _TERRAIN_INSTANCED_PERPIXEL_NORMAL
		#pragma multi_compile_local_fragment __ _ALPHATEST_ON

		//================================================================
		// STRUCTS

		// Vertex input
		struct appdata_tcp2
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord0 : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			half4 tangent : TANGENT;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct Input
		{
			float2 texcoord0;
		};

		//================================================================

		// Custom SurfaceOutput
		struct SurfaceOutputCustom
		{
			half atten;
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Specular;
			half Gloss;
			half Alpha;

			Input input;

			half terrainWeight;
			half terrainWeight1;

			// Shader Properties
			float __rampThreshold;
			float __rampSmoothing;
			float3 __highlightColor;
			float3 __shadowColor;
			float __ambientIntensity;
		};

		//================================================================
		// VERTEX FUNCTION

		void vertex_surface(inout appdata_tcp2 v, out Input output)
		{
			UNITY_INITIALIZE_OUTPUT(Input, output);

			TerrainInstancing(v.vertex, v.normal, v.texcoord0.xy);
				v.tangent.xyz = cross(v.normal, float3(0,0,1));
				v.tangent.w = -1;

			// Texture Coordinates
			output.texcoord0 = v.texcoord0.xy;

		}

		//================================================================
		// SURFACE FUNCTION

		void surf(Input input, inout SurfaceOutputCustom output)
		{
			// Shader Properties Sampling
			float4 __layer0Mask = ( TCP2_TEX2D_SAMPLE(_Mask0, _Mask0, input.texcoord0.xy * _Splat0_ST.xy + _Splat0_ST.zw).rgba );
			float __layer0HeightSource = ( __layer0Mask.b );
			float __layer0HeightOffset = ( .0 );
			float4 __layer1Mask = ( TCP2_TEX2D_SAMPLE(_Mask1, _Mask0, input.texcoord0.xy * _Splat1_ST.xy + _Splat1_ST.zw).rgba );
			float __layer1HeightSource = ( __layer1Mask.b );
			float __layer1HeightOffset = ( .0 );
			float4 __layer2Mask = ( TCP2_TEX2D_SAMPLE(_Mask2, _Mask0, input.texcoord0.xy * _Splat2_ST.xy + _Splat2_ST.zw).rgba );
			float __layer2HeightSource = ( __layer2Mask.b );
			float __layer2HeightOffset = ( .0 );
			float4 __layer3Mask = ( TCP2_TEX2D_SAMPLE(_Mask3, _Mask0, input.texcoord0.xy * _Splat3_ST.xy + _Splat3_ST.zw).rgba );
			float __layer3HeightSource = ( __layer3Mask.b );
			float __layer3HeightOffset = ( .0 );
			float4 __layer4Mask = ( TCP2_TEX2D_SAMPLE(_Mask4, _Mask4, input.texcoord0.xy * _Splat4_ST.xy + _Splat4_ST.zw).rgba );
			float __layer4HeightSource = ( __layer4Mask.b );
			float __layer4HeightOffset = ( .0 );
			float4 __layer5Mask = ( TCP2_TEX2D_SAMPLE(_Mask5, _Mask4, input.texcoord0.xy * _Splat5_ST.xy + _Splat5_ST.zw).rgba );
			float __layer5HeightSource = ( __layer5Mask.b );
			float __layer5HeightOffset = ( .0 );
			float4 __layer6Mask = ( TCP2_TEX2D_SAMPLE(_Mask6, _Mask4, input.texcoord0.xy * _Splat6_ST.xy + _Splat6_ST.zw).rgba );
			float __layer6HeightSource = ( __layer6Mask.b );
			float __layer6HeightOffset = ( .0 );
			float4 __layer7Mask = ( TCP2_TEX2D_SAMPLE(_Mask7, _Mask4, input.texcoord0.xy * _Splat7_ST.xy + _Splat7_ST.zw).rgba );
			float __layer7HeightSource = ( __layer7Mask.b );
			float __layer7HeightOffset = ( .0 );
			float4 __layer0Albedo = ( tex2D_noTile(_Splat0, input.texcoord0.xy * _Splat0_ST.xy + _Splat0_ST.zw).rgba );
			float4 __layer1Albedo = ( tex2D_noTile(_Splat1, input.texcoord0.xy * _Splat1_ST.xy + _Splat1_ST.zw).rgba );
			float4 __layer2Albedo = ( tex2D_noTile(_Splat2, input.texcoord0.xy * _Splat2_ST.xy + _Splat2_ST.zw).rgba );
			float4 __layer3Albedo = ( tex2D_noTile(_Splat3, input.texcoord0.xy * _Splat3_ST.xy + _Splat3_ST.zw).rgba );
			float4 __layer4Albedo = ( TCP2_TEX2D_SAMPLE(_Splat4, _Mask4, input.texcoord0.xy * _Splat4_ST.xy + _Splat4_ST.zw).rgba );
			float4 __layer5Albedo = ( TCP2_TEX2D_SAMPLE(_Splat5, _Mask4, input.texcoord0.xy * _Splat5_ST.xy + _Splat5_ST.zw).rgba );
			float4 __layer6Albedo = ( TCP2_TEX2D_SAMPLE(_Splat6, _Mask4, input.texcoord0.xy * _Splat6_ST.xy + _Splat6_ST.zw).rgba );
			float4 __layer7Albedo = ( TCP2_TEX2D_SAMPLE(_Splat7, _Mask4, input.texcoord0.xy * _Splat7_ST.xy + _Splat7_ST.zw).rgba );
			float4 __mainColor = ( half4(1,1,1,1) );
			output.__rampThreshold = ( .5 );
			output.__rampSmoothing = ( _RampSmoothing );
			output.__highlightColor = ( half3(1,1,1) );
			output.__shadowColor = ( unity_ShadowColor.rgb );
			output.__ambientIntensity = ( 1.0 );

			output.input = input;

			// Terrain
			
			float2 terrainTexcoord0 = input.texcoord0.xy;
			
			#if defined(_ALPHATEST_ON)
				ClipHoles(terrainTexcoord0.xy);
			#endif
			
			#if defined(TERRAIN_BASE_PASS)
			
				half4 terrain_mixedDiffuse = TCP2_TEX2D_SAMPLE(_MainTex, _MainTex, terrainTexcoord0.xy).rgba;
				half3 normalTS = half3(0.0h, 0.0h, 1.0h);
			
			#else
			
				// Sample the splat control texture generated by the terrain
				// adjust splat UVs so the edges of the terrain tile lie on pixel centers
				float2 terrainSplatUV = (terrainTexcoord0.xy * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy;
				half4 terrain_splat_control_0 = TCP2_TEX2D_SAMPLE(_Control, _Control, terrainSplatUV);
				terrainSplatUV = (terrainTexcoord0.xy * (_Control1_TexelSize.zw - 1.0f) + 0.5f) * _Control1_TexelSize.xy;
				half4 terrain_splat_control_1 = TCP2_TEX2D_SAMPLE(_Control1, _Control1, terrainSplatUV);
				half height0 = __layer0HeightSource + __layer0HeightOffset;
				half height1 = __layer1HeightSource + __layer1HeightOffset;
				half height2 = __layer2HeightSource + __layer2HeightOffset;
				half height3 = __layer3HeightSource + __layer3HeightOffset;
				half height4 = __layer4HeightSource + __layer4HeightOffset;
				half height5 = __layer5HeightSource + __layer5HeightOffset;
				half height6 = __layer6HeightSource + __layer6HeightOffset;
				half height7 = __layer7HeightSource + __layer7HeightOffset;
				HeightBasedSplatModify_8_Layers(terrain_splat_control_0, terrain_splat_control_1, half4(height0, height1, height2, height3), half4(height4, height5, height6, height7));
			
				// Calculate weights and perform the texture blending
				half terrain_weight = dot(terrain_splat_control_0, half4(1,1,1,1));
				half terrain_weight_1 = dot(terrain_splat_control_1, half4(1,1,1,1));
			
				#if !defined(SHADER_API_MOBILE) && defined(TERRAIN_SPLAT_ADDPASS)
					clip(terrain_weight == 0.0f ? -1 : 1);
					clip(terrain_weight_1 == 0.0f ? -1 : 1);
				#endif
			
				// Normalize weights before lighting and restore afterwards so that the overall lighting result can be correctly weighted
				terrain_splat_control_0 /= (terrain_weight + terrain_weight_1 + 1e-3f);
				terrain_splat_control_1 /= (terrain_weight + terrain_weight_1 + 1e-3f);
			
			#endif // TERRAIN_BASE_PASS
			
			#if defined(INSTANCING_ON) && defined(SHADER_TARGET_SURFACE_ANALYSIS) && defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
				output.Normal = float3(0, 0, 1); // make sure that surface shader compiler realizes we write to normal, as UNITY_INSTANCING_ENABLED is not defined for SHADER_TARGET_SURFACE_ANALYSIS.
			#endif
				
			// Terrain normal, if using instancing and per-pixel normal map
			#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X) && defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
				float2 terrainNormalCoords = (terrainTexcoord0.xy / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
				float3 geomNormal = normalize(TCP2_TEX2D_SAMPLE(_TerrainNormalmapTexture, _TerrainNormalmapTexture, terrainNormalCoords.xy).xyz * 2 - 1);
			
				output.Normal = geomNormal;
			#endif
			
			output.Albedo = half3(1,1,1);
			output.Alpha = 1;

			#if !defined(TERRAIN_BASE_PASS)
				// Sample textures that will be blended based on the terrain splat map
				half4 splat0 = __layer0Albedo;
				half4 splat1 = __layer1Albedo;
				half4 splat2 = __layer2Albedo;
				half4 splat3 = __layer3Albedo;
				half4 splat4 = __layer4Albedo;
				half4 splat5 = __layer5Albedo;
				half4 splat6 = __layer6Albedo;
				half4 splat7 = __layer7Albedo;
			
				#define BLEND_TERRAIN_HALF4(outVariable, sourceVariable) \
					half4 outVariable = terrain_splat_control_0.r * sourceVariable##0; \
					outVariable += terrain_splat_control_0.g * sourceVariable##1; \
					outVariable += terrain_splat_control_0.b * sourceVariable##2; \
					outVariable += terrain_splat_control_0.a * sourceVariable##3; \
					outVariable += terrain_splat_control_1.r * sourceVariable##4; \
					outVariable += terrain_splat_control_1.g * sourceVariable##5; \
					outVariable += terrain_splat_control_1.b * sourceVariable##6; \
					outVariable += terrain_splat_control_1.a * sourceVariable##7;
				#define BLEND_TERRAIN_HALF(outVariable, sourceVariable) \
					half4 outVariable = dot(terrain_splat_control_0, half4(sourceVariable##0, sourceVariable##1, sourceVariable##2, sourceVariable##3)); \
					outVariable += dot(terrain_splat_control_1, half4(sourceVariable##4, sourceVariable##5, sourceVariable##6, sourceVariable##7));
			
				BLEND_TERRAIN_HALF4(terrain_mixedDiffuse, splat)
			
			#endif // !TERRAIN_BASE_PASS
			
			#if !defined(TERRAIN_BASE_PASS)
				output.terrainWeight = terrain_weight;
				output.terrainWeight1 = terrain_weight_1;
			#endif
			
			output.Albedo = terrain_mixedDiffuse.rgb;
			output.Alpha = terrain_mixedDiffuse.a;
			
			output.Albedo *= __mainColor.rgb;

		}

		//================================================================
		// LIGHTING FUNCTION

		inline half4 LightingToonyColorsCustom(inout SurfaceOutputCustom surface, UnityGI gi)
		{

			half3 lightDir = gi.light.dir;
			#if defined(UNITY_PASS_FORWARDBASE)
				half3 lightColor = _LightColor0.rgb;
				half atten = surface.atten;
			#else
				// extract attenuation from point/spot lights
				half3 lightColor = _LightColor0.rgb;
				half atten = max(gi.light.color.r, max(gi.light.color.g, gi.light.color.b)) / max(_LightColor0.r, max(_LightColor0.g, _LightColor0.b));
			#endif

			half3 normal = normalize(surface.Normal);
			half ndl = dot(normal, lightDir);
			half3 ramp;
			
			#define		RAMP_THRESHOLD	surface.__rampThreshold
			#define		RAMP_SMOOTH		surface.__rampSmoothing
			ndl = saturate(ndl);
			ramp = smoothstep(RAMP_THRESHOLD - RAMP_SMOOTH*0.5, RAMP_THRESHOLD + RAMP_SMOOTH*0.5, ndl);

			// Apply attenuation (shadowmaps & point/spot lights attenuation)
			ramp *= atten;

			// Highlight/Shadow Colors
			#if !defined(UNITY_PASS_FORWARDBASE)
				ramp = lerp(half3(0,0,0), surface.__highlightColor, ramp);
			#else
				ramp = lerp(surface.__shadowColor, surface.__highlightColor, ramp);
			#endif

			// Output color
			half4 color;
			color.rgb = surface.Albedo * lightColor.rgb * ramp;
			color.a = surface.Alpha;

			// Apply indirect lighting (ambient)
			half occlusion = 1;
			#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
				half3 ambient = gi.indirect.diffuse;
				ambient *= surface.Albedo * occlusion * surface.__ambientIntensity;

				color.rgb += ambient;
			#endif

			#if !defined(TERRAIN_BASE_PASS)
				color.rgb *= saturate(surface.terrainWeight + surface.terrainWeight1);
			#endif

			return color;
		}

		void LightingToonyColorsCustom_GI(inout SurfaceOutputCustom surface, UnityGIInput data, inout UnityGI gi)
		{
			half3 normal = surface.Normal;

			// GI without reflection probes
			gi = UnityGlobalIllumination(data, 1.0, normal); // occlusion is applied in the lighting function, if necessary

			surface.atten = data.atten; // transfer attenuation to lighting function
			gi.light.color = _LightColor0.rgb; // remove attenuation

		}

		ENDCG

		UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
		UsePass "Hidden/Nature/Terrain/Utilities/SELECTION"
	}

	Dependency "BaseMapShader"    = "Hidden/Landon/Toony Colors Pro 2/Terrain8-BasePass"
	Dependency "BaseMapGenShader" = "Hidden/Landon/Toony Colors Pro 2/Terrain8-BaseGen"

	Fallback "Diffuse"
	//CustomEditor "ToonyColorsPro.ShaderGenerator.MaterialInspector_SG2"
}

/* TCP_DATA u config(ver:"2.9.3";unity:"2021.3.3f1";tmplt:"SG2_Template_Default";features:list["UNITY_5_4","UNITY_5_5","UNITY_5_6","UNITY_2017_1","UNITY_2018_1","UNITY_2018_2","UNITY_2018_3","UNITY_2019_1","UNITY_2019_2","UNITY_2019_3","UNITY_2019_4","UNITY_2020_1","UNITY_2021_1","TERRAIN_SHADER","TERRAIN_SHADER_8_LAYERS","TERRAIN_HEIGHT_BLENDING"];flags:list[];flags_extra:dict[];keywords:dict[RENDER_TYPE="Opaque",RampTextureDrawer="[TCP2Gradient]",RampTextureLabel="Ramp Texture",SHADER_TARGET="3.0",BASEGEN_ALBEDO_DOWNSCALE="1",BASEGEN_MASKTEX_DOWNSCALE="1/2",BASEGEN_METALLIC_DOWNSCALE="1/4",BASEGEN_SPECULAR_DOWNSCALE="1/4",BASEGEN_DIFFUSEREMAPMIN_DOWNSCALE="1/4",BASEGEN_MASKMAPREMAPMIN_DOWNSCALE="1/4"];shaderProperties:list[,sp(name:"Main Color";imps:list[imp_constant(type:color_rgba;fprc:half;fv:1;f2v:(1, 1);f3v:(1, 1, 1);f4v:(1, 1, 1, 1);cv:RGBA(1, 1, 1, 1);guid:"be2f3d67-81c8-422d-a9bd-61f5da759502";op:Multiply;lbl:"Color";gpu_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];clones:dict[];isClone:False),,,sp(name:"Ramp Threshold";imps:list[imp_constant(type:float;fprc:half;fv:0.5;f2v:(1, 1);f3v:(1, 1, 1);f4v:(1, 1, 1, 1);cv:RGBA(1, 1, 1, 1);guid:"c63eb01d-f09e-44d4-b164-6ffab26b0146";op:Multiply;lbl:"Threshold";gpu_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];clones:dict[];isClone:False),,sp(name:"Highlight Color";imps:list[imp_constant(type:color;fprc:half;fv:1;f2v:(1, 1);f3v:(1, 1, 1);f4v:(1, 1, 1, 1);cv:RGBA(1, 1, 1, 1);guid:"27c5e8e2-37d5-4cd4-b8d6-32e2165a7988";op:Multiply;lbl:"Highlight Color";gpu_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];clones:dict[];isClone:False),sp(name:"Shadow Color";imps:list[imp_mp_color(def:RGBA(0.7333333, 0.5450981, 0.8666667, 1);hdr:False;cc:3;chan:"RGB";prop:"_SColor";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"9baaa033-8f7d-4986-a95a-0758f5750119";op:Multiply;lbl:"Shadow Color";gpu_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];clones:dict[];isClone:False),,,,,,,,,,,,,sp(name:"Layer 0 Albedo";imps:list[imp_mp_texture(uto:True;tov:"";tov_lbl:"";gto:False;sbt:False;scr:False;scv:"";scv_lbl:"";gsc:False;roff:False;goff:False;sin_anm:False;sin_anmv:"";sin_anmv_lbl:"";gsin:False;notile:True;triplanar_local:False;def:"gray";locked_uv:False;uv:0;cc:4;chan:"RGBA";mip:-1;mipprop:False;ssuv_vert:False;ssuv_obj:False;uv_type:Texcoord;uv_chan:"XZ";tpln_scale:0.01;uv_shaderproperty:__NULL__;uv_cmp:__NULL__;sep_sampler:__NULL__;prop:"_Splat0";md:"[HideInInspector]";gbv:False;custom:False;refs:"";pnlock:True;guid:"2e92c3fc-808a-462d-8d08-a79c7c7d7c98";op:Multiply;lbl:"Layer 0 Albedo";gpu_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];clones:dict[];isClone:False),sp(name:"Layer 1 Albedo";imps:list[imp_mp_texture(uto:True;tov:"";tov_lbl:"";gto:False;sbt:False;scr:False;scv:"";scv_lbl:"";gsc:False;roff:False;goff:False;sin_anm:False;sin_anmv:"";sin_anmv_lbl:"";gsin:False;notile:True;triplanar_local:False;def:"gray";locked_uv:False;uv:0;cc:4;chan:"RGBA";mip:-1;mipprop:False;ssuv_vert:False;ssuv_obj:False;uv_type:Texcoord;uv_chan:"XZ";tpln_scale:0.01;uv_shaderproperty:__NULL__;uv_cmp:__NULL__;sep_sampler:"_Splat0";prop:"_Splat1";md:"[HideInInspector]";gbv:False;custom:False;refs:"";pnlock:True;guid:"f79945bf-03c2-4c9d-bfdc-c9a0562e0b10";op:Multiply;lbl:"Layer 1 Albedo";gpu_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];clones:dict[];isClone:False),sp(name:"Layer 2 Albedo";imps:list[imp_mp_texture(uto:True;tov:"";tov_lbl:"";gto:False;sbt:False;scr:False;scv:"";scv_lbl:"";gsc:False;roff:False;goff:False;sin_anm:False;sin_anmv:"";sin_anmv_lbl:"";gsin:False;notile:True;triplanar_local:False;def:"gray";locked_uv:False;uv:0;cc:4;chan:"RGBA";mip:-1;mipprop:False;ssuv_vert:False;ssuv_obj:False;uv_type:Texcoord;uv_chan:"XZ";tpln_scale:0.01;uv_shaderproperty:__NULL__;uv_cmp:__NULL__;sep_sampler:"_Splat0";prop:"_Splat2";md:"[HideInInspector]";gbv:False;custom:False;refs:"";pnlock:True;guid:"2a1f9999-d686-442d-8890-4ee30640a2a3";op:Multiply;lbl:"Layer 2 Albedo";gpu_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];clones:dict[];isClone:False),sp(name:"Layer 3 Albedo";imps:list[imp_mp_texture(uto:True;tov:"";tov_lbl:"";gto:False;sbt:False;scr:False;scv:"";scv_lbl:"";gsc:False;roff:False;goff:False;sin_anm:False;sin_anmv:"";sin_anmv_lbl:"";gsin:False;notile:True;triplanar_local:False;def:"gray";locked_uv:False;uv:0;cc:4;chan:"RGBA";mip:-1;mipprop:False;ssuv_vert:False;ssuv_obj:False;uv_type:Texcoord;uv_chan:"XZ";tpln_scale:0.01;uv_shaderproperty:__NULL__;uv_cmp:__NULL__;sep_sampler:"_Splat0";prop:"_Splat3";md:"[HideInInspector]";gbv:False;custom:False;refs:"";pnlock:True;guid:"a464b4c2-1eb0-4503-b56e-f3365c93cb6a";op:Multiply;lbl:"Layer 3 Albedo";gpu_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];clones:dict[];isClone:False),,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,sp(name:"Layer 0 Height Offset";imps:list[imp_constant(type:float;fprc:half;fv:0;f2v:(1, 1);f3v:(1, 1, 1);f4v:(1, 1, 1, 1);cv:RGBA(1, 1, 1, 1);guid:"feb538d7-a7f4-4f7f-88e9-acb761e1d90b";op:Multiply;lbl:"Layer 0 Height Offset";gpu_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];clones:dict[];isClone:False),sp(name:"Layer 1 Height Offset";imps:list[imp_constant(type:float;fprc:half;fv:0;f2v:(1, 1);f3v:(1, 1, 1);f4v:(1, 1, 1, 1);cv:RGBA(1, 1, 1, 1);guid:"be6d6d82-d9d7-4e26-a549-a094527d3b8a";op:Multiply;lbl:"Layer 1 Height Offset";gpu_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];clones:dict[];isClone:False),sp(name:"Layer 2 Height Offset";imps:list[imp_constant(type:float;fprc:half;fv:0;f2v:(1, 1);f3v:(1, 1, 1);f4v:(1, 1, 1, 1);cv:RGBA(1, 1, 1, 1);guid:"4be5f2a6-8123-43dd-8918-f849e8fd4653";op:Multiply;lbl:"Layer 2 Height Offset";gpu_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];clones:dict[];isClone:False),sp(name:"Layer 3 Height Offset";imps:list[imp_constant(type:float;fprc:half;fv:0;f2v:(1, 1);f3v:(1, 1, 1);f4v:(1, 1, 1, 1);cv:RGBA(1, 1, 1, 1);guid:"41df7576-10aa-4d9f-b3eb-c0de9f5b27e7";op:Multiply;lbl:"Layer 3 Height Offset";gpu_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];clones:dict[];isClone:False),sp(name:"Layer 4 Height Offset";imps:list[imp_constant(type:float;fprc:half;fv:0;f2v:(1, 1);f3v:(1, 1, 1);f4v:(1, 1, 1, 1);cv:RGBA(1, 1, 1, 1);guid:"39635d12-ea01-49be-915d-03dec1b63d6d";op:Multiply;lbl:"Layer 4 Height Offset";gpu_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];clones:dict[];isClone:False),sp(name:"Layer 5 Height Offset";imps:list[imp_constant(type:float;fprc:half;fv:0;f2v:(1, 1);f3v:(1, 1, 1);f4v:(1, 1, 1, 1);cv:RGBA(1, 1, 1, 1);guid:"fe11aa30-f92c-4e15-b7a0-f4d25179b4ca";op:Multiply;lbl:"Layer 5 Height Offset";gpu_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];clones:dict[];isClone:False),sp(name:"Layer 6 Height Offset";imps:list[imp_constant(type:float;fprc:half;fv:0;f2v:(1, 1);f3v:(1, 1, 1);f4v:(1, 1, 1, 1);cv:RGBA(1, 1, 1, 1);guid:"d6496306-6384-4214-8196-87b848ab040b";op:Multiply;lbl:"Layer 6 Height Offset";gpu_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];clones:dict[];isClone:False),sp(name:"Layer 7 Height Offset";imps:list[imp_constant(type:float;fprc:half;fv:0;f2v:(1, 1);f3v:(1, 1, 1);f4v:(1, 1, 1, 1);cv:RGBA(1, 1, 1, 1);guid:"b91d9639-3cce-491f-a7b7-690d956e43ae";op:Multiply;lbl:"Layer 7 Height Offset";gpu_inst:False;locked:False;impl_index:-1)];layers:list[];unlocked:list[];clones:dict[];isClone:False)];customTextures:list[];codeInjection:codeInjection(injectedFiles:list[];mark:False);matLayers:list[]) */
/* TCP_HASH 4b2e4dcb6554db16d8015cad4d68c626 */