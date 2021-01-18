#ifndef _GPU_SKINNING_
#define _GPU_SKINNING_

struct Bone
{
    float4x4 transform;
}; 

struct BindPose 
{
    float4x4 transform;
};

struct BoneWeight 
{
    int boneIndex;
    float weight;
};

#if SHADER_TARGET >= 35 && (defined(SHADER_API_D3D11)) 

float4 skin4(float4 vert, float4x4 m1, float w1, float4x4 m2, float w2, float4x4 m3, float w3, float4x4 m4, float w4)
{
	float4x4 matrices = m1 * w1 + m2 * w2 + m3 * w3 + m4 * w4;
	return mul(matrices, vert);
}

float4 skin3(float4 vert, float4x4 m1, float w1, float4x4 m2, float w2, float4x4 m3)
{
	float4x4 matrices = m1 * w1 + m2 * w2 + m3 * (1 - (w1 + w2));
	return mul(matrices, vert);
}

float4 skin2(float4 vert, float4x4 m1, float w1, float4x4 m2)
{
	float4x4 matrices = m1 * w1 + m2 * (1 - w1);
	return mul(matrices, vert);
}

float4 skin1(float4 vert, float4x4 m1)
{
	float4x4 matrices = m1 * 1;
	return mul(matrices, vert);
}

float4 skin4(int instanceId, int vertexId, float4 vertex, StructuredBuffer<BoneWeight> boneWeights, StructuredBuffer<Bone> bones)
{
	int index = vertexId;
	int weightIndex = index * 4;

	BoneWeight bw1 = boneWeights[weightIndex];
	BoneWeight bw2 = boneWeights[weightIndex + 1];
	BoneWeight bw3 = boneWeights[weightIndex + 2];
	BoneWeight bw4 = boneWeights[weightIndex + 3];

	float w1 = bw1.weight;
	float w2 = bw2.weight;
	float w3 = bw3.weight;
	float w4 = bw4.weight;

	int bi1 = bw1.boneIndex;
	int bi2 = bw2.boneIndex;
	int bi3 = bw3.boneIndex;
	int bi4 = bw4.boneIndex;

	Bone bone1 = bones[bi1];
	Bone bone2 = bones[bi2];
	Bone bone3 = bones[bi3];
	Bone bone4 = bones[bi4];

	float4x4 m1 = bone1.transform;
	float4x4 m2 = bone2.transform;
	float4x4 m3 = bone3.transform;
	float4x4 m4 = bone4.transform;

	return skin4(vertex, m1, w1, m2, w2, m3, w3, m4, w4);
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