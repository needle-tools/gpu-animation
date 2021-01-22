// UNITY_SHADER_NO_UPGRADE
#if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)
#ifndef URP_AnimationBaking
#define URP_AnimationBaking

#include "../Skinning.cginc"
#include "../AnimationTypes.cginc"

sampler2D _Animation, _Skinning;
float4 _Animation_TexelSize, _Skinning_TexelSize;
float4 _CurrentAnimation; 

void GpuSkinning_float(float4 vert, float3 normal, float time, out float4 skinnedVertex, out float3 skinnedNormal)
{
    skinnedVertex = vert;
    skinnedNormal = normal;

    const TextureClipInfo clip = ToTextureClipInfo(_CurrentAnimation);
    skin(skinnedVertex, skinnedNormal, 0, _Skinning, _Skinning_TexelSize, _Animation, _Animation_TexelSize, clip.IndexStart, clip.Frames, (time * (clip.FramesPerSecond)));
}


#endif
#endif