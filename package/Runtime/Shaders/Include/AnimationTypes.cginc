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

struct TextureClipInfo
{
    int IndexStart;
    int TotalLength;
    int Frames;
    int FramesPerSecond;
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

#endif
