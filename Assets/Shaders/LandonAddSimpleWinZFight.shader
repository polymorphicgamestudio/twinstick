Shader "Landon/Add Simple Win ZFight" {
	Properties {
		_Color ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
	}

	Category {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha One
		Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
		Offset -1, -1
	
		//BindChannels {
		//	Bind "Color", color
		//	Bind "Vertex", vertex
		//	Bind "TexCoord", texcoord
		//}
	
		SubShader {
			Pass {
				SetTexture [_MainTex] {
					constantColor[_Color]
					combine texture * constant
				}
			}
		}
	}
}