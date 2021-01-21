using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.XR;

namespace Elaborate.AnimationBakery
{
	[ExecuteInEditMode]
	public class AnimationBaker : MonoBehaviour
	{
		[FormerlySerializedAs("BakeShader")] [FormerlySerializedAs("TextureBakingShader")] public ComputeShader Shader;
		public Animator Animator;
		public SkinnedMeshRenderer Renderer;
		public List<AnimationClip> Clips;
		public int Skip;

		[Header("Output")] 
		public List<AnimationTransformationData> AnimationData;
		public BakedAnimation Target;

		[Header("Debug")] 
		public bool DebugLog;

#if UNITY_EDITOR

		private void OnValidate()
		{
			Bake();
		}

		[ContextMenu(nameof(Bake))]
		public void Bake()
		{
			if (Clips.Count <= 0)
			{
				Debug.LogError("Missing clips");
				return;
			}

			AnimationTextureProvider.DebugLog = DebugLog;
			AnimationData = AnimationDataProvider.GetAnimations(Animator, Clips, Renderer, Skip, -1);
			if (!Target) return;
			Target.SkinBake = AnimationTextureProvider.BakeSkinning(Renderer.sharedMesh, Shader);
			Target.AnimationBake = AnimationTextureProvider.BakeAnimation(AnimationData, Shader);
			AnimationTextureProvider.DebugLog = false;
		}

#endif
	}
}