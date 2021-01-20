#ifndef _ANIMATION_DATA
#define _ANIMATION_DATA

struct BoneWeight
{
    float weight0;
    float weight1;
    float weight2;
    float weight3;
    int boneIndex0;
    int boneIndex1;
    int boneIndex2;
    int boneIndex3;
};

struct Bone
{
    float4x4 transformation;
};

struct AnimationClip
{
    int IndexStart;
    int Length;
};

#endif
