using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Elaborate.AnimationBakery
{
	[ExecuteInEditMode]
	public class AnimationBaker : MonoBehaviour
	{
		public Animator Animator;
		public SkinnedMeshRenderer Renderer;
		public List<AnimationClip> Clips;
		public ComputeShader TextureBakingShader;
		public int Skip;
		public float FrameRate = -1;

		[Header("Output")] 
		public List<AnimationTransformationData> AnimationData;
		public BakedAnimation Target;

		[Header("Test")] 
		public Material AnimationTex;
		public Material SkinTex;
		
#if UNITY_EDITOR

		private void OnValidate()
		{
			Bake();
		}

		[ContextMenu(nameof(Bake))]
		public void Bake()
		{
			Debug.Log("Bake");
			AnimationData = AnimationDataProvider.GetAnimations(Animator, Clips, Renderer, Skip, FrameRate);
			if (!Target) return;
			Target.SkinBake = AnimationTextureProvider.BakeSkinning(Renderer.sharedMesh, TextureBakingShader);
			Target.AnimationBake = AnimationTextureProvider.BakeAnimation(AnimationData, TextureBakingShader);
			

			if (AnimationTex)
			{
				AnimationTex.mainTexture = Target.AnimationBake.Texture;
			}

			if (SkinTex)
			{
				SkinTex.mainTexture = Target.SkinBake.Texture;
			}
		}
#endif
	}
}