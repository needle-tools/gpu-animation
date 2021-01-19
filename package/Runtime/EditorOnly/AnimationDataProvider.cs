#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Elaborate.AnimationBakery
{
	public static class AnimationDataProvider
	{
		public static List<AnimationTransformationData> GetAnimations(
			Animator animator, List<AnimationClip> animationClips, SkinnedMeshRenderer skinnedMeshRenderer,
			int skip, float frameRate = -1
			)
		{
			var rootBone = skinnedMeshRenderer.rootBone;
			var bones = skinnedMeshRenderer.bones;

			var boneData = GetData(skinnedMeshRenderer);
			var bonesInfo = new Dictionary<Transform, SkinnedMesh_BoneData>();
			for (var i = 0; i < boneData.Length; i++)
				bonesInfo.Add(boneData[i].Bone, boneData[i]);

			var result = new List<AnimationTransformationData>();
			var clips = animationClips;// //AnimationUtility.GetAnimationClips(animator.gameObject);
			foreach (var clip in clips)
			{
				// make sure not to add one clip multiple times
				for (var i = 0; i < result.Count; i++)
					if (result[i].Clip == clip) continue;

				var data = SampleAnimationData(animator, rootBone, bones, bonesInfo, clip, skip, frameRate);
				result.Add(new AnimationTransformationData(clip, data));
			}

			return result;
		}
		
		private static BoneTransformationData[] SampleAnimationData(
			Animator animatedObject, Transform rootBone, Transform[] bones, Dictionary<Transform, SkinnedMesh_BoneData> bonesInfo,
			AnimationClip clip, int skip, float frameRate = -1)
		{
			if (!AnimationClipUtility.GetData(animatedObject, clip, out var data)) return null;
			
			// 1: save bind transformations of bones
			var bindStates = GetTransformationState(rootBone);

			// 2: sample transformations in clip
			var transformations = SampleAndStoreAnimationClipData(bones, bonesInfo, data, skip, frameRate);

			// 3: restore transformation state
			RestoreTransformationState(rootBone, bindStates);

			return transformations;
		}

		private static BoneTransformationData[] SampleAndStoreAnimationClipData(Transform[] bones, Dictionary<Transform, SkinnedMesh_BoneData> bonesInfo,
			AnimationClipData data, int skip, float frameRate = -1)
		{
			var boneTransformations = new Dictionary<Transform, BoneTransformationData>();

			var duration = data.Duration;
			frameRate = frameRate <= 0 ? data.FrameRate : frameRate;

			var frames = duration * frameRate;
			//var frameDuration = 1 / frameRate;

			skip += 1;
			for (var i = 0; i < frames; i++)
			{
				if (skip > 1 && i % skip != 0) continue;

				var frame = i;
				var time = ((float) frame / frames) * duration;

				// set pose
				foreach (var curveData in data.Curves)
				{
					var bone = curveData.Key;
					var curve = curveData.Value;

					if (curve.HasPositionKeyframes)
						bone.localPosition = curve.Position(time);
					if (curve.HasRotationKeyframes)
						bone.localRotation = curve.Rotation(time);
					if (curve.HasScaleKeyframes)
						bone.localScale = curve.Scale(time);
				}

				// add keyframes for all bones for now...
				foreach (var kvp in bonesInfo)
				{
					var info = kvp.Value;
					var index = info.Index;
					var bone = bones[index];

					//Debug.Log("store transformation for " + path);

					var pos = bone.position;
					var rot = bone.rotation;
					var scale = bone.lossyScale;
					var boneMatrix = Matrix4x4.TRS(pos, rot, scale);

					if (boneTransformations.ContainsKey(bone) == false)
						boneTransformations.Add(bone, new BoneTransformationData(info.Index, new List<BoneTransformation>()));

					boneTransformations[bone].Transformations.Add(new BoneTransformation(time, boneMatrix, scale != Vector3.one));
				}
			}

			return boneTransformations.Select(v => v.Value).ToArray();
		}


		public static SkinnedMesh_BoneData[] GetData(SkinnedMeshRenderer renderer)
		{
			var boneHierarchy = new List<SkinnedMesh_BoneData>();

			//Debug.LogWarning("make sure we start with the parent of the skinned mesh root bone because thats how it is stored in the animation clip as well");
			var rootBone = renderer.rootBone;
			var bones = renderer.bones;

			for (int i = 0; i < bones.Length; i++)
			{
				var bone = bones[i];
				var newData = new SkinnedMesh_BoneData()
				{
					Bone = bone,
					Index = i,
				};
				boneHierarchy.Add(newData);
			}

			return boneHierarchy.ToArray();
		}

		
		private static void RestoreTransformationState(Transform root, Dictionary<Transform, TransformState> states)
		{
			var children = root.GetComponentsInChildren<Transform>();
			foreach (var child in children)
			{
				if (states.ContainsKey(child))
				{
					var state = states[child];
					child.localPosition = state.LocalPosition;
					child.localRotation = state.LocalRotation;
					child.localScale = state.LocalScale;
				}
			}
		}

		private static Dictionary<Transform, TransformState> GetTransformationState(Transform root)
		{
			var state = new Dictionary<Transform, TransformState>();
			var children = root.GetComponentsInChildren<Transform>();
			foreach (var child in children)
				state.Add(child, new TransformState()
				{
					Position = child.position,
					LocalPosition = child.localPosition,
					Rotation = child.rotation,
					LocalRotation = child.localRotation,
					LossyScale = child.lossyScale,
					LocalScale = child.localScale
				});
			return state;
		}

	}
}
#endif