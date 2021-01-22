using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace needle.GpuAnimation
{
	public static class AnimationDataProvider
	{
		public static List<AnimationTransformationData> GetAnimations(
			GameObject animatedObject, List<AnimationClip> animationClips, SkinnedMeshRenderer skinnedMeshRenderer,
			int skip, float frameRate = -1
		)
		{
			var bones = skinnedMeshRenderer.bones;
			var boneData = GetData(skinnedMeshRenderer);
			var bonesInfo = new Dictionary<Transform, SkinnedMesh_BoneData>();
			for (var i = 0; i < boneData.Count; i++)
				bonesInfo.Add(boneData[i].Bone, boneData[i]);

			var result = new List<AnimationTransformationData>();
			var clips = animationClips;
			foreach (var clip in clips)
			{
				if (!clip) continue;
				
				// make sure not to add one clip multiple times
				if (result.Any(r => r.Clip == clip)) continue;

				var data = SampleAnimationData(skinnedMeshRenderer.sharedMesh, animatedObject, skinnedMeshRenderer.rootBone, bones, bonesInfo, clip, skip, out int fps, frameRate);
				result.Add(new AnimationTransformationData(clip, data, fps));
			}

			return result;
		}

		private static BoneTransformationData[] SampleAnimationData(
			Mesh mesh, 
			GameObject animatedObject,
			Transform rootBone,
			Transform[] bones,
			Dictionary<Transform, SkinnedMesh_BoneData> bonesInfo,
			AnimationClip clip, int skip, 
			out int sampledFramesPerSecond,
			float frameRate = -1
		)
		{
			// 1: save bind transformations of bones
			var transformStates = CaptureTransformationAndPrepareForSampling(rootBone, bones);

			// 2: sample transformations in clip
			var transformations = SampleAndStoreAnimationClipData(animatedObject, clip, mesh, bones, bonesInfo, skip, frameRate, out sampledFramesPerSecond);

			// 3: restore transformation stat
			RestoreTransformationState(transformStates);

			return transformations;
		}

		private static BoneTransformationData[] SampleAndStoreAnimationClipData(
			GameObject animatedObject, AnimationClip clip,
			Mesh mesh, IReadOnlyList<Transform> bones,
			Dictionary<Transform, SkinnedMesh_BoneData> bonesInfo,
			int skip, float frameRate,
			out int sampledFramesPerSecond
			)
		{

			frameRate = frameRate <= 0 ? clip.frameRate : frameRate;
			skip = Mathf.Max(0, skip);
			skip += 1;
			sampledFramesPerSecond = Mathf.FloorToInt((float) frameRate / skip);
			
			var duration = clip.length;
			var frames = duration * frameRate;

			var boneTransformations = new Dictionary<Transform, BoneTransformationData>();
			for (var i = 0; i < frames; i++)
			{
				if (skip > 1 && i % skip != 0) continue;

				var frame = i;
				var time = ((float) frame / frames) * duration;
				
				clip.SampleAnimation(animatedObject, time);
				
				// sample all bones
				foreach (var kvp in bonesInfo)
				{
					var info = kvp.Value;
					var index = info.Index;
					var bone = bones[index];

					var pos = bone.position;
					var rot = bone.rotation;
					var scale = bone.lossyScale;

					var boneMatrix = Matrix4x4.TRS(pos, rot, scale);
					// bindposes are already inverted!!!
					var bp = mesh.bindposes[index];
					boneMatrix *= bp;

					if (boneTransformations.ContainsKey(bone) == false)
						boneTransformations.Add(bone, new BoneTransformationData(bone.name, bone, info.Index, new List<BoneTransformation>()));

					boneTransformations[bone].Transformations.Add(new BoneTransformation(time, boneMatrix, scale != Vector3.one));
				}
			}
			
			return boneTransformations.Values.ToArray();
		}


		public static List<SkinnedMesh_BoneData> GetData(SkinnedMeshRenderer renderer)
		{
			var boneHierarchy = new List<SkinnedMesh_BoneData>();

			//Debug.LogWarning("make sure we start with the parent of the skinned mesh root bone because thats how it is stored in the animation clip as well");
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

			return boneHierarchy;
		}


		private static void RestoreTransformationState(Dictionary<Transform, TransformState> states)
		{
			foreach (var kvp in states)
			{
				var transform = kvp.Key;
				var state = kvp.Value;
				RestoreTransformationState(transform, state);
			}
		}

		private static void RestoreTransformationState(this Transform transform, TransformState state)
		{
			transform.localPosition = state.LocalPosition;
			transform.localRotation = state.LocalRotation;
			transform.localScale = state.LocalScale;
		}

		private static Dictionary<Transform, TransformState> GetTransformationState(IEnumerable<Transform> bones)
		{
			return bones.ToDictionary(bone => bone, bone => bone.GetTransformationState());
		}

		private static TransformState GetTransformationState(this Transform bone)
		{
			return new TransformState()
			{
				Position = bone.position,
				LocalPosition = bone.localPosition,
				Rotation = bone.rotation,
				LocalRotation = bone.localRotation,
				LossyScale = bone.lossyScale,
				LocalScale = bone.localScale
			};
		}

		private static Dictionary<Transform, TransformState> CaptureTransformationAndPrepareForSampling(Transform rootBone, IEnumerable<Transform> bones)
		{
			var states = GetTransformationState(bones);
			if(!states.ContainsKey(rootBone)) 
				states.Add(rootBone, GetTransformationState(rootBone));
			var parent = rootBone.parent;
			while (parent)
			{
				if(!states.ContainsKey(parent)) 
					states.Add(parent, GetTransformationState(parent));
				parent.localPosition = Vector3.zero;
				parent.localRotation = Quaternion.identity;
				parent.localScale = Vector3.one;
				parent = parent.parent;
			}

			return states;
		}
	}
}