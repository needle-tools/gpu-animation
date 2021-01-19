using System.Collections.Generic;
using UnityEngine;

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
		public AnimationTextureData Texture;

		public Renderer Visualize;

#if UNITY_EDITOR
		private void OnValidate()
		{
			Bake();
		}

		[ContextMenu(nameof(Bake))]
		public void Bake()
		{
			Output = AnimationDataProvider.GetAnimations(Animator, Clips, Renderer, Skip, FrameRate);
			Texture = AnimationTextureProvider.BakeToTexture(Output, TextureBakingShader);
			if (Visualize && Visualize.sharedMaterial) Visualize.material.mainTexture = Texture.Texture;
		}
#endif
	}
}