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

		[Header("Output")] public List<AnimationTransformationData> AnimationData;
		public MeshSkinningData SkinBake;
		public AnimationTextureData AnimationBake;

		[Header("Test")] public Material VertexAnim;
		public Material Test;
		public Material Test2;

		

#if UNITY_EDITOR

		private void OnEnable()
		{
			Bake();
		}

		private void Update()
		{
			if (!Application.isPlaying) Bake();
		}

		[ContextMenu(nameof(Bake))]
		public void Bake()
		{
			AnimationData = AnimationDataProvider.GetAnimations(Animator, Clips, Renderer, Skip, FrameRate);
			SkinBake = AnimationTextureProvider.BakeSkinning(Renderer.sharedMesh, TextureBakingShader);
			AnimationBake = AnimationTextureProvider.BakeAnimation(AnimationData, TextureBakingShader);

			if (VertexAnim)
			{
				VertexAnim.SetTexture("_Animation", AnimationBake.Texture);
				VertexAnim.SetTexture("_Skinning", SkinBake.Texture);
			}

			if (Test)
			{
				Test.mainTexture = AnimationBake.Texture;
			}
			
			if (Test2)
			{
				Test2.mainTexture = SkinBake.Texture;
			}
		}
#endif
	}
}