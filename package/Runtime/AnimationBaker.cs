using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Elaborate.AnimationBakery
{
	public class AnimationBaker : MonoBehaviour
	{
		public Animator Animator;
		public SkinnedMeshRenderer Renderer;
		public List<AnimationClip> Clips;
		public ComputeShader TextureBakingShader;
		public int Skip;
		public float FrameRate = -1;

		[Header("Output")] public List<AnimationTransformationData> Output;
		public MeshSkinningData SkinTexture;
		public AnimationTextureData AnimationTexture;

		[Header("Test")] public Renderer Visualize;
		public Material Test;

		private Material mat;

#if UNITY_EDITOR
		private void OnValidate()
		{
			Bake();
		}

		[ContextMenu(nameof(Bake))]
		public void Bake()
		{
			Output = AnimationDataProvider.GetAnimations(Animator, Clips, Renderer, Skip, FrameRate);
			SkinTexture = AnimationTextureProvider.BakeSkinning(Renderer.sharedMesh, TextureBakingShader);
			AnimationTexture = AnimationTextureProvider.BakeAnimation(Output, TextureBakingShader);

			if (Test)
				Test.SetTexture("_Animation", AnimationTexture.Texture);

			if (Visualize && Visualize.sharedMaterial)
			{
				mat = new Material(Visualize.sharedMaterial);
				mat.mainTexture = AnimationTexture.Texture;
				Visualize.sharedMaterial = mat;
			}
		}
#endif
	}
}