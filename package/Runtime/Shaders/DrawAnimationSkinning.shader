Shader "Custom/AnimationSkinning"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_EmissionFactor ("Emission Factor", float) = 0
		_Emission ("Emission", 2D) = "black" {}
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
		#pragma target 5.0
		#pragma multi_compile_instancing
		#pragma multi_compile SKIN_QUALITY_FOUR SKIN_QUALITY_THREE _KIN_QUALITY_TWO SKIN_QUALITY_ONE

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
			uint id : SV_VertexID;
			uint instance_id : SV_InstanceID;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct Input
		{
			float2 uv_MainTex;
			float4 color;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		#include "Include/Skinning.cginc"
		#include "Include/Animation.cginc"
		#include "Include/Quaternion.cginc"


		#ifdef SHADER_API_D3D11
		StructuredBuffer<BoneWeight> boneWeights;
		StructuredBuffer<AnimationClip> animations;
		StructuredBuffer<BoneAnimation> animationClips;
		StructuredBuffer<Keyframe> keyframes;
		#endif

		struct AnimationInfo
		{
			int StartIndex;
			int Length;
		};
		sampler2D _Animation;
		uint2 _AnimationTextureSize;
		AnimationInfo CurrentAnimation;

		float Time;
		float AnimationIndex = 0;

		float remap(float p, float p0, float p1, float t0, float t1)
		{
			return t0 + (t1 - t0) * ((p - p0) / (p1 - p0));
		}

		void vert(inout appdata v, out Input result)
		{
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_OUTPUT(Input, result);

			int id = v.id;
			int index = v.id;
			int instanceId = v.instance_id;
			result.color = 1;
			

			// #ifdef SHADER_API_D3D11
			// Horse horse = horses[instanceId];
			// float4 rotatedVertex = float4(rotateWithQuaternion(v.vertex.xyz, horse.rotation) + horse.position, 1);
			//
			// int i1 = horse.animationIndex;
			// int i2 = horse.nextAnimationIndex;
			//
			// float4 anim1 = skin(
			// 	Time, i1, instanceId, id, v.vertex,
			// 	animations, boneWeights, animationClips, keyframes);
			//
			// if (i1 != i2)
			// {
			// 	float4 anim2 = skin(
			// 		Time, i2, instanceId, id, v.vertex,
			// 		animations, boneWeights, animationClips, keyframes);
			// 	v.vertex = lerp(anim1, anim2, horse.animationLerp);
			// }
			// else v.vertex = anim1;
			//
			// v.vertex.xyz = rotateWithQuaternion(v.vertex.xyz, horse.rotation);
			// v.vertex.xyz = v.vertex.xyz + horse.position;
			//
			// #endif
		}

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 col = tex2D(_MainTex, IN.uv_MainTex) * _Color * IN.color; 
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