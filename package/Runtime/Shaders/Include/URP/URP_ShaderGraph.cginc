// UNITY_SHADER_NO_UPGRADE
#if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)
#ifndef URP_AnimationBaking
#define URP_AnimationBaking

#include "../Skinning.cginc"

void GpuSkinning_float(float vertexId, float4 vert, float3 normal, float time, out float4 skinnedVertex, out float3 skinnedNormal)
{
    skinnedVertex = vert;
    skinnedNormal = normal;

    const TextureClipInfo clip = ToTextureClipInfo(_CurrentAnimation);
    float4 tangent = 0;
    skin(skinnedVertex, skinnedNormal, tangent, (uint)vertexId, _Skinning, _Skinning_TexelSize, _Animation, _Animation_TexelSize, clip.IndexStart, clip.Frames, (time * (clip.FramesPerSecond))); 
}


#endif
#endif
