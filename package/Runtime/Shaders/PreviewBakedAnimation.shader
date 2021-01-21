Shader "BakedAnimation/PreviewAnimation"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_EmissionFactor ("Emission Factor", float) = 0
		_Emission ("Emission", 2D) = "white" {}
		[Header(Skinning)]
		[KeywordEnum(Four, Three, Two, One, Dynamic)] Skin_Quality("Skin Quality", Float) = 0
	}
	SubShader
	{
		Tags
		{
			"RenderType"="Opaque"
		}
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
		#pragma target 4.5
		#pragma multi_compile_instancing
		#pragma multi_compile SKIN_QUALITY_FOUR SKIN_QUALITY_THREE SKIN_QUALITY_TWO SKIN_QUALITY_ONE
		#include "Include/Skinning.cginc"

		sampler2D _MainTex, _Emission;

		half _Glossiness;
		half _Metallic;
		half _EmissionFactor;
		fixed4 _Color;

		struct appdata
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
			float4 tangent : TANGENT;
			uint vertex_id : SV_VertexID;
			uint instance_id : SV_InstanceID;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct Input
		{
			float2 uv_MainTex;
			float4 color;
			float2 skinCoords;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		sampler2D _Animation, _Skinning;
		float4 _Animation_TexelSize, _Skinning_TexelSize;
		float4 _CurrentAnimation;

		// #if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)
		// StructuredBuffer<BoneWeight> _BoneWeights;
		// StructuredBuffer<Bone> _Animations;
		// #endif

		

		void vert(inout appdata v, out Input result)
		{
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_OUTPUT(Input, result);

			#if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)
			TextureClipInfo clip = ToTextureClipInfo(_CurrentAnimation);
			// v.vertex = skin(v.vertex, v.vertex_id, _BoneWeights, _Animations, _CurrentAnimation.x, _CurrentAnimation.y, _CurrentAnimation.z);
			v.vertex = skin(v.vertex, v.vertex_id, _Skinning, _Skinning_TexelSize, _Animation, _Animation_TexelSize, clip.IndexStart, clip.Frames, (_Time.y * (clip.FramesPerSecond)));
			#endif


		}

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 col = tex2D(_MainTex, IN.uv_MainTex) * _Color; 
			o.Albedo = col.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Emission = tex2D(_Emission, IN.uv_MainTex) * _EmissionFactor;
			o.Alpha = col.a;
		}
		ENDCG

		//UsePass "Standard/SHADOWCASTER"
	}
	FallBack "Diffuse"
}