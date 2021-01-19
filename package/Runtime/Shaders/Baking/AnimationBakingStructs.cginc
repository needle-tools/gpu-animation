#ifndef _ANIMATION_DATA
#define _ANIMATION_DATA


struct BoneAnimation
{
    int startIndex;
    int endIndex;
};

StructuredBuffer<float4x4> BoneTransformations;



#endif