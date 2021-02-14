#ifndef _SKINNING_BAKED
#define _SKINNING_BAKED

#include "AnimationTypes.cginc"
#include "SkinningUtils.cginc"

void skinBaked(inout float4 vert, uint vertId, sampler2D baked, float4 bakedTexel, uint frame, TextureClipInfo clip)
{
    const int totalFrames = 7310;
    const uint index = (vertId * totalFrames) + (frame % clip.Frames);
    const float4 uv = indexToCoord(index, bakedTexel);
    vert = tex2Dlod(baked, uv);
}


#endif