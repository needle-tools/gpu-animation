#ifndef NEEDLE_GPU_ANIMATION_SKINNING_PROPERTIES
#define NEEDLE_GPU_ANIMATION_SKINNING_PROPERTIES

sampler2D _Animation, _Skinning;
float4 _Animation_TexelSize, _Skinning_TexelSize;
float4 _CurrentAnimation;


#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
StructuredBuffer<float4x4> _InstanceTransforms;
#endif


#if SHADER_TARGET < 45
uniform float _InstanceTimeOffsets[1024];
#elif defined(SHADER_API_D3D11) || defined(SHADER_API_METAL) || defined(SHADER_API_GLES3)
StructuredBuffer<float> _InstanceTimeOffsets;
#endif
		
inline float GetTime(uint id)
{
    #if defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) || defined(INSTANCING_ON)
    return _InstanceTimeOffsets[id];
    #endif
    return 0;
}


#endif
