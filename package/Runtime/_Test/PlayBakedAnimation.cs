using UnityEngine;

namespace Elaborate.AnimationBakery
{
	[ExecuteAlways]
	public class PlayBakedAnimation : MonoBehaviour
	{
		public BakedAnimation Animation;
		public int ClipIndex;

		private BakedMeshSkinningData SkinBake => Animation.SkinBake;
		private BakedAnimationData BakedAnimationBake => Animation.bakedAnimationBake;

		private Renderer rend;
		private MaterialPropertyBlock block;

		private void Update()
		{
			if (!Animation) return;
			
			if (!rend && !TryGetComponent(out rend))
				return;

			if (block == null) block = new MaterialPropertyBlock();

			block.SetTexture("_Animation", BakedAnimationBake.Texture);
			block.SetTexture("_Skinning", SkinBake.Texture);
			var index = ClipIndex % BakedAnimationBake.ClipsInfos.Count;
			var clip = BakedAnimationBake.ClipsInfos[index];
			block.SetVector("_CurrentAnimation", clip.AsVector4);
			rend.SetPropertyBlock(block);
		}
	}
}