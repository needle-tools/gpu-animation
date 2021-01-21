using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Elaborate.AnimationBakery
{
	[ExecuteInEditMode]
	public class AnimationBaker : MonoBehaviour
	{
		[FormerlySerializedAs("BakeShader")] [FormerlySerializedAs("TextureBakingShader")]
		public ComputeShader Shader;

		public bool ImmediateMode = true;

		public Animator Animator;
		public SkinnedMeshRenderer Renderer;
		public List<AnimationClip> Clips;
		public int Skip;

		[Header("Output")] public List<AnimationTransformationData> AnimationData;
		public BakedAnimation Target;

		[Header("Debug")] public bool DebugLog;

#if UNITY_EDITOR

		private void OnValidate()
		{
			if (ImmediateMode && Selection.Contains(this.gameObject))
				Bake();
		}

		[ContextMenu(nameof(Bake))]
		public void Bake()
		{
			if (!Target) return;

			Target.SkinBake = null;
			Target.AnimationBake = null;

			if (Clips.Count <= 0)
			{
				Debug.LogError("Missing clips");
				return;
			}

			if (!Animator) Animator = GetComponentInChildren<Animator>();
			if (!Renderer) Renderer = GetComponentInChildren<SkinnedMeshRenderer>();

			if (!Animator || !Renderer) return;

			AnimationTextureProvider.DebugLog = DebugLog;
			AnimationData = AnimationDataProvider.GetAnimations(Animator, Clips, Renderer, Skip, -1);
			Target.SkinBake = AnimationTextureProvider.BakeSkinning(Renderer.sharedMesh, Shader);
			Target.AnimationBake = AnimationTextureProvider.BakeAnimation(AnimationData, Shader);
			Target.SaveAssets();
			AnimationTextureProvider.DebugLog = false;
		}

#endif
	}
}