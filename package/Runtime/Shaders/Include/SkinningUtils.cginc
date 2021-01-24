#ifndef _SKINNING_UTILS
#define _SKINNING_UTILS

#include "AnimationTypes.cginc"

static const int BONEWEIGHT_PIXEL_COUNT = 2; // weight takes 2 pixel (one BoneWeight is 2 x 4 floats)
static const int ANIMATION_MATRIX_4x4_PIXEL_COUNT = 4; // 4x4 matrix takes 4 pixel (one matrix is 4 x 4 floats)

inline float4 indexToCoord(uint index, float4 texelSize)
{
    const float2 size = float2(texelSize.z, texelSize.w);
    const int row = (int)(index / size.x);
    const float col = index % size.x;
    const float2 coord = float2(col+0.5, row+0.5) / size;
    return float4(coord, 0, 0);
}

inline BoneWeight sampleBoneWeight(sampler2D weightsTex, float4 weightsTexel, uint index)
{
    index *= BONEWEIGHT_PIXEL_COUNT;
    const float4 coord0 = indexToCoord(index, weightsTexel);
    const float4 coord1 = indexToCoord(index+1, weightsTexel);
    const float4 p0 = tex2Dlod(weightsTex, coord0);
    const float4 p1 = tex2Dlod(weightsTex, coord1);
    BoneWeight bw;
    bw.weight0 = p0.x;
    bw.boneIndex0 = p0.y;
    bw.weight1 = p0.z;
    bw.boneIndex1 = p0.w;
    bw.weight2 = p1.x;
    bw.boneIndex2 = p1.y;
    bw.weight3 = p1.z;
    bw.boneIndex3 = p1.w;
    return bw;
}

float4x4 sampleMatrix4x4(sampler2D animationTex, float4 animationTexel, uint index)
{
    index *= ANIMATION_MATRIX_4x4_PIXEL_COUNT; 
    const float4 coord0 = indexToCoord(index, animationTexel);
    const float4 coord1 = indexToCoord(index+1, animationTexel);
    const float4 coord2 = indexToCoord(index+2, animationTexel);
    const float4 coord3 = indexToCoord(index+3, animationTexel);
    const float4 p0 = tex2Dlod(animationTex, coord0);
    const float4 p1 = tex2Dlod(animationTex, coord1);
    const float4 p2 = tex2Dlod(animationTex, coord2);
    const float4 p3 = tex2Dlod(animationTex, coord3);
    return float4x4(p0, p1, p2, p3);
}

inline uint4 getBoneIndices(BoneWeight boneWeight, uint startIndex, uint animationLength, uint frame)
{
    uint bi0 = boneWeight.boneIndex0;
    uint bi1 = boneWeight.boneIndex1;
    uint bi2 = boneWeight.boneIndex2;
    uint bi3 = boneWeight.boneIndex3;
	
    bi0 *= animationLength;
    bi1 *= animationLength;
    bi2 *= animationLength;
    bi3 *= animationLength;

    frame %= animationLength;

    bi0 += frame;
    bi1 += frame;
    bi2 += frame;
    bi3 += frame;

    bi0 += startIndex;
    bi1 += startIndex;
    bi2 += startIndex;
    bi3 += startIndex;

    return uint4(bi0, bi1, bi2, bi3);
}

#endif