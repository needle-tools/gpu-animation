#ifndef _SKINNING_UTILS
#define _SKINNING_UTILS

inline float4 IndexToCoord(uint index, float4 texelSize)
{
    // index = trunc(index + 0.5);
    const float2 size = float2(texelSize.z, texelSize.w);
    const int row = (int)(index / size.x);
    const float col = index % size.x;
    const float2 coord = float2(col+0.5, row+0.5) / size;
    return float4(coord, 0, 0);
}

float4x4 SampleMatrix(sampler2D animationTex, float4 animationTexel, uint index)
{
    index *= 4;
    const float4 coord0 = IndexToCoord(index, animationTexel);
    const float4 p0 = tex2Dlod(animationTex, coord0);
    const float4 coord1 = IndexToCoord(index+1, animationTexel);
    const float4 p1 = tex2Dlod(animationTex, coord1);
    const float4 coord2 = IndexToCoord(index+2, animationTexel);
    const float4 p2 = tex2Dlod(animationTex, coord2);
    const float4 coord3 = IndexToCoord(index+3, animationTexel);
    const float4 p3 = tex2Dlod(animationTex, coord3);
    return float4x4(p0, p1, p2, p3);
}

uint4 getBoneIndices(BoneWeight boneWeight, uint startIndex, uint animationLength, uint frame)
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