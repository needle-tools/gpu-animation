#ifndef NEEDLE_GPU_ANIMATION_SKINNING_PROPERTIES
#define NEEDLE_GPU_ANIMATION_SKINNING_PROPERTIES

sampler2D _Animation, _Skinning;
float4 _Animation_TexelSize, _Skinning_TexelSize;
float4 _CurrentAnimation;


#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
StructuredBuffer<float4x4> _InstanceTransforms;
#endif


#endif