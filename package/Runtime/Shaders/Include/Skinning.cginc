#ifndef _GPU_SKINNING_
#define _GPU_SKINNING_

#if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)

#include "AnimationTypes.cginc"
#include "SkinningUtils.cginc"

void skin4(inout float4 vert, inout float3 normal, float4x4 m1, float w1, float4x4 m2, float w2, float4x4 m3, float w3, float4x4 m4, float w4)
{
    const float4x4 mat = m1 * w1 + m2 * w2 + m3 * w3 + m4 * w4;
    vert = mul(mat, vert);
    normal = mul(mat, normal);
}

void skin3(inout float4 vert, inout float3 normal, float4x4 m1, float w1, float4x4 m2, float w2, float4x4 m3)
{
    const float4x4 mat = m1 * w1 + m2 * w2 + m3 * (1 - (w1 + w2));
    vert = mul(mat, vert);
    normal = mul(mat, normal);
}

void skin2(inout float4 vert, inout float3 normal, float4x4 m1, float w1, float4x4 m2)
{
    const float4x4 mat = m1 * w1 + m2 * (1 - w1);
    vert = mul(mat, vert);
    normal = mul(mat, normal);
}

void skin1(inout float4 vert, inout float3 normal, float4x4 m1)
{
    const float4x4 mat = m1 * 1;
    vert = mul(mat, vert);
    normal = mul(mat, normal);
}


void skin(inout float4 vert, inout float3 normal, int vertId,
	StructuredBuffer<BoneWeight> boneWeights, StructuredBuffer<Bone> animations, uint startIndex, uint animationLength, uint frame)
{
	const BoneWeight bw = boneWeights[vertId];

	const float w0 = bw.weight0;
	const float w1 = bw.weight1;
	const float w2 = bw.weight2;
	const float w3 = bw.weight3;

	const uint4 indices = getBoneIndices(bw, startIndex, animationLength, frame);

	const Bone bone0 = animations[indices.x];
	const Bone bone1 = animations[indices.y];
	const Bone bone2 = animations[indices.z];
	const Bone bone3 = animations[indices.w];

	const float4x4 m0 = bone0.transformation;
	const float4x4 m1 = bone1.transformation;
	const float4x4 m2 = bone2.transformation;
	const float4x4 m3 = bone3.transformation;

	 skin4(vert, normal, m0, w0, m1, w1, m2, w2, m3, w3);
}

void skin(inout float4 vert, inout float3 normal, int vertId,
	sampler2D weights, float4 weightsTexel, sampler2D animation, float4 animationTexel, uint startIndex, uint animationLength, uint frame)
{
    const BoneWeight bw = sampleBoneWeight(weights, weightsTexel, vertId);

    const float w0 = bw.weight0;
    const float w1 = bw.weight1;
    const float w2 = bw.weight2;
    const float w3 = bw.weight3;

    const uint4 indices = getBoneIndices(bw, startIndex, animationLength, frame);

    const float4x4 m0 = sampleMatrix4x4(animation, animationTexel, indices.x);
    const float4x4 m1 = sampleMatrix4x4(animation, animationTexel, indices.y);
    const float4x4 m2 = sampleMatrix4x4(animation, animationTexel, indices.z);
    const float4x4 m3 = sampleMatrix4x4(animation, animationTexel, indices.w);

    skin4(vert, normal, m0, w0, m1, w1, m2, w2, m3, w3);
}

void skin(inout float4 vert, inout float3 normal, int vertId,
	StructuredBuffer<BoneWeight> boneWeights, sampler2D animation, float4 animationTexel, uint startIndex, uint animationLength, uint frame)
{
	const BoneWeight bw = boneWeights[vertId];
	
	const float w0 = bw.weight0;
	const float w1 = bw.weight1;
	const float w2 = bw.weight2;
	const float w3 = bw.weight3;

	const uint4 indices = getBoneIndices(bw, startIndex, animationLength, frame);
	
	const float4x4 m0 = sampleMatrix4x4(animation, animationTexel, indices.x);
	const float4x4 m1 = sampleMatrix4x4(animation, animationTexel, indices.y);
	const float4x4 m2 = sampleMatrix4x4(animation, animationTexel, indices.z);
	const float4x4 m3 = sampleMatrix4x4(animation, animationTexel, indices.w);

	skin4(vert, normal, m0, w0, m1, w1, m2, w2, m3, w3);
}

#endif
#endif