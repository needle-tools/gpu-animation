// #ifndef NEEDLE_GPU_ANIMATION
// #define NEEDLE_GPU_ANIMATION
//
// struct AnimationClip
// {
//     int clipStartIndex;
//     int clipLength;
//
//     float duration;
//     int frameRate;
// };
//
//
// // per bone animation data
// struct BoneAnimation
// {
//     int boneIndex;
//     int keyframeStartIndex;
//     int keyframeLength; 
// };
//
// struct Keyframe
// { 
//     float time;
//     float4x4 transform;
// };
//
//
// #if defined(_GPU_SKINNING_) && SHADER_TARGET >= 35 && (defined(SHADER_API_D3D11))
//
//
// bool in_frustum(float4x4 ViewProjectionMatrix, float4 p, float pad = 0) {
// 	float4 Pclip = mul(ViewProjectionMatrix, float4(p.x, p.y, p.z, 1.0));
// 	return (abs(Pclip.x) + pad < Pclip.w)
// 		&& abs(Pclip.y) + pad < Pclip.w
// 		&& 0 < Pclip.z
// 		&& Pclip.z < Pclip.w;
// }
//
// float4 skin(
// 	float Time, int animationIndex, int instanceId, int vertexId, float4 vertex,
// 	StructuredBuffer<AnimationClip> animations,
// 	StructuredBuffer<BoneWeight> boneWeights,
// 	StructuredBuffer<BoneAnimation> animationClips,
// 	StructuredBuffer<Keyframe> keyframes)
// {
// 	//int animationIndex = animationIndex;
// 	AnimationClip usedAnimation = animations[animationIndex];
// 	int animationClipStart = usedAnimation.clipStartIndex;
//
// 	// calculate position in animationclip
// 	float animationDuration = usedAnimation.duration;
// 	int animationFrameRate = usedAnimation.frameRate;
//
// 	float time = Time / animationDuration;
// 	float animationClipPosition = time;
// 	// clip position is from 0 (start) - 1 (end)
// 	animationClipPosition = fmod(animationClipPosition, 1);
//
// 	// get bone data per animation clip
// 	int index = vertexId;
// 	int weightIndex = index * 4;
//
// 	BoneWeight bw1 = boneWeights[weightIndex];
// 	BoneWeight bw2 = boneWeights[weightIndex + 1];
// 	BoneWeight bw3 = boneWeights[weightIndex + 2];
// 	BoneWeight bw4 = boneWeights[weightIndex + 3];
//
// 	float w1 = bw1.weight;
// 	float w2 = bw2.weight;
// 	float w3 = bw3.weight;
// 	float w4 = bw4.weight;
//
// 	int bi1 = bw1.boneIndex;
// 	int bi2 = bw2.boneIndex;
// 	int bi3 = bw3.boneIndex;
// 	int bi4 = bw4.boneIndex;
//
// 	int iClip1 = animationClipStart + bi1;
// 	int iClip2 = animationClipStart + bi2;
// 	int iClip3 = animationClipStart + bi3;
// 	int iClip4 = animationClipStart + bi4;
//
// 	BoneAnimation clip1 = animationClips[iClip1];
// 	BoneAnimation clip2 = animationClips[iClip2];
// 	BoneAnimation clip3 = animationClips[iClip3];
// 	BoneAnimation clip4 = animationClips[iClip4];
//
// 	int clip1_startIndex = clip1.keyframeStartIndex;
// 	int clip2_startIndex = clip2.keyframeStartIndex;
// 	int clip3_startIndex = clip3.keyframeStartIndex;
// 	int clip4_startIndex = clip4.keyframeStartIndex;
//
// 	int clip1_length = clip1.keyframeLength;
// 	int clip2_length = clip2.keyframeLength;
// 	int clip3_length = clip3.keyframeLength;
// 	int clip4_length = clip4.keyframeLength;
//
// 	// get current keyframe index based on animation clip time
// 	int kf1_index = lerp(clip1_startIndex, clip1_startIndex + clip1_length - 1, animationClipPosition);
// 	int kf2_index = lerp(clip2_startIndex, clip2_startIndex + clip2_length - 1, animationClipPosition);
// 	int kf3_index = lerp(clip3_startIndex, clip3_startIndex + clip3_length - 1, animationClipPosition);
// 	int kf4_index = lerp(clip4_startIndex, clip4_startIndex + clip4_length - 1, animationClipPosition);
//
// 	Keyframe kf1 = keyframes[kf1_index];
// 	Keyframe kf2 = keyframes[kf2_index];
// 	Keyframe kf3 = keyframes[kf3_index];
// 	Keyframe kf4 = keyframes[kf4_index];
//
// 	float4x4 m1 = kf1.transform;
// 	float4x4 m2 = kf2.transform;
// 	float4x4 m3 = kf3.transform;
// 	float4x4 m4 = kf4.transform;
//
// 	float4 vert = float4(vertex.xyz, 1);
// #ifdef SKIN_QUALITY_FOUR
// 	return skin4(vert, m1, w1, m2, w2, m3, w3, m4, w4);
// #elif SKIN_QUALITY_THREE
// 	return skin3(vert, m1, w1, m2, w2, m3);
// #elif SKIN_QUALITY_TWO
// 	return skin2(vert, m1, w1, m2);
// #elif SKIN_QUALITY_ONE
// 	return skin1(vert, m1);
// #else 
// 	return vert;
// #endif
//
// }
//
//
// #endif
//
//
// #endif