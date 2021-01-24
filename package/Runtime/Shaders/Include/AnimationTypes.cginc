#ifndef NEEDLE_GPU_ANIMATION_TYPES
#define NEEDLE_GPU_ANIMATION_TYPES

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

struct TextureClipInfo
{
    int IndexStart;
    int TotalLength;
    int Frames;
    int FramesPerSecond;
};

struct AnimationClipInfo
{
    int IndexStart;
    int FrameCount;
    float CurrentTime;
};

inline TextureClipInfo ToTextureClipInfo(float4 vec)
{
    TextureClipInfo ci;
    ci.IndexStart = vec.x;
    ci.TotalLength = vec.y;
    ci.Frames = vec.z;
    ci.FramesPerSecond = vec.w;
    return ci;
}

inline AnimationClipInfo ToAnimationClipInfo(float4 vec)
{
    AnimationClipInfo ci;
    ci.IndexStart = vec.x;
    ci.FrameCount = vec.y;
    ci.CurrentTime = vec.z;
    return ci;
}

#endif
