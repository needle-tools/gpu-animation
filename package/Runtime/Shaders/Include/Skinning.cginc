#ifndef _GPU_SKINNING_
#define _GPU_SKINNING_
#include "AnimationTypes.cginc"

// struct Bone
// {
//     float4x4 transform;
// }; 

struct BindPose 
{
    float4x4 transform;
};

#if SHADER_TARGET >= 35 && (defined(SHADER_API_D3D11)) 

float4 skin4(float4 vert, float4x4 m1, float w1, float4x4 m2, float w2, float4x4 m3, float w3, float4x4 m4, float w4)
{
	const float4x4 mat = m1 * w1 + m2 * w2 + m3 * w3 + m4 * w4;
	return mul(mat, vert);
}
//
// float4 skin3(float4 vert, float4x4 m1, float w1, float4x4 m2, float w2, float4x4 m3)
// {
// 	float4x4 matrices = m1 * w1 + m2 * w2 + m3 * (1 - (w1 + w2));
// 	return mul(matrices, vert);
// }
//
// float4 skin2(float4 vert, float4x4 m1, float w1, float4x4 m2)
// {
// 	float4x4 matrices = m1 * w1 + m2 * (1 - w1);
// 	return mul(matrices, vert);
// }
//
// float4 skin1(float4 vert, float4x4 m1)
// {
// 	float4x4 matrices = m1 * 1;
// 	return mul(matrices, vert);
// }


float4 IndexToCoord(float index, float4 texelSize)
{
	index = floor(index);
	float2 size = float2( texelSize.z,  texelSize.w);
	float2 uv = float2(index % size.x, (int)(index / size.y)) / size;
	return float4(uv, 0, 0);
}

float4 IndexToUv(float index, float4 texelSize)
{
	int iIndex = trunc(index + 0.5);
	float2 wh = float2(texelSize.z, texelSize.w);
	int row = (int)(iIndex / wh.x);
	float col = iIndex - row*wh.x;
	return float4((col+0.5)/wh.x, (row+0.5) /wh.y, 0, 0);
}

float4x4 SampleMatrix(sampler2D animationTex, float4 animationTexel, uint index)
{
	float4 coord0 = IndexToUv(index, animationTexel);
	float4 p0 = tex2Dlod(animationTex, float4(coord0));
	float4 coord1 = IndexToUv(index+1, animationTexel);
	float4 p1 = tex2Dlod(animationTex, float4(coord1));
	float4 coord2 = IndexToUv(index+2, animationTexel);
	float4 p2 = tex2Dlod(animationTex, float4(coord2));
	float4 coord3 = IndexToUv(index+3, animationTexel);
	float4 p3 = tex2Dlod(animationTex, float4(coord3));
	float4x4 mat;
	mat[0] = p0;
	mat[1] = p1;
	mat[2] = p2;
	mat[3] = p3;
	return mat;
	// return float4x4(p0, p1, p2, p3);
}

float4 skin4(float4 vertex, int vertexId, StructuredBuffer<BoneWeight> boneWeights, sampler2D animation, float4 animationTexel, uint animationIndex, uint animationLength, uint frame)
{
	int index = vertexId;
	int weightIndex = index;

	BoneWeight bw = boneWeights[weightIndex];

	float w0 = bw.weight0;
	float w1 = bw.weight1;
	float w2 = bw.weight2;
	float w3 = bw.weight3;

	uint bi0 = bw.boneIndex0;
	uint bi1 = bw.boneIndex1;
	uint bi2 = bw.boneIndex2;
	uint bi3 = bw.boneIndex3;

	bi0 += animationLength;
	bi1 += animationLength;
	bi2 += animationLength;
	bi3 += animationLength;

	uint i0 = (bi0 + frame) * 4;
	uint i1 = (bi1 + frame) * 4;
	uint i2 = (bi2 + frame) * 4;
	uint i3 = (bi3 + frame) * 4;

	float4x4 m0 = SampleMatrix(animation, animationTexel, i0);
	float4x4 m1 = SampleMatrix(animation, animationTexel, i1);
	float4x4 m2 = SampleMatrix(animation, animationTexel, i2);
	float4x4 m3 = SampleMatrix(animation, animationTexel, i3);

	// Bone bone1 = bones[bi1];
	// Bone bone2 = bones[bi2];
	// Bone bone3 = bones[bi3];
	// Bone bone4 = bones[bi4];
	//
	// float4x4 m1 = bone1.transform;
	// float4x4 m2 = bone2.transform;
	// float4x4 m3 = bone3.transform;
	// float4x4 m4 = bone4.transform;

	// return mul(m0, vertex);
	return skin4(vertex, m0, w0, m1, w1, m2, w2, m3, w3);
	return vertex;
}
#endif

/*
float4x4 getAbsoluteMatrix(int boneIndex, StructuredBuffer<Bone> bones, StructuredBuffer<BindPose> boneBindPoses)
{
    Bone bone = bones[boneIndex];
    BindPose bind = boneBindPoses[boneIndex];
			
    float4x4 currentMatrix;
    currentMatrix = bone.transform;
    currentMatrix = mul(currentMatrix, bind.transform);

    int i = 0;
    while (bone.parentIndex >= 0)
    {
        boneIndex = bone.parentIndex;
        bone = bones[boneIndex];
        bind = boneBindPoses[boneIndex];

        currentMatrix = mul(bone.transform, currentMatrix);
        currentMatrix = mul(currentMatrix, bind.transform);
        ++i;
        if (i > 30)
            break;
    }

    return currentMatrix;
}
*/

#endif