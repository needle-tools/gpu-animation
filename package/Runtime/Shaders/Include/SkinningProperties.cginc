#ifndef NEEDLE_GPU_ANIMATION_SKINNING_PROPERTIES
#define NEEDLE_GPU_ANIMATION_SKINNING_PROPERTIES

sampler2D _Animation, _Skinning;
float4 _Animation_TexelSize, _Skinning_TexelSize;
float4 _CurrentAnimation;


#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
StructuredBuffer<float4x4> _InstanceTransforms;
#endif


#if defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
StructuredBuffer<float4> _InstanceTimeOffsets;
#endif


float GetTime(uint instanceId)
{
    #if defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    return _Time.y + _InstanceTimeOffsets[instanceId].y;
    #else
    return _Time.y;
    #endif
}

#endif
