using UnityEngine;

namespace Elaborate.AnimationBakery
{
	[ExecuteAlways]
	public class PlayBakedAnimation : MonoBehaviour
	{
		public BakedAnimation Animation;
		public int ClipIndex;

		private MeshSkinningData SkinBake => Animation.SkinBake;
		private AnimationTextureData AnimationBake => Animation.AnimationBake;

		private Renderer rend;
		private MaterialPropertyBlock block;

		private void Update()
		{
			if (!Animation) return;
			
			if (!rend && !TryGetComponent(out rend))
				return;

			if (block == null) block = new MaterialPropertyBlock();

			block.SetTexture("_Animation", AnimationBake.Texture);
			block.SetTexture("_Skinning", SkinBake.Texture);
			var index = ClipIndex % AnimationBake.ClipsInfos.Count;
			var clip = AnimationBake.ClipsInfos[index];
			block.SetVector("_CurrentAnimation", clip.AsVector4);
			rend.SetPropertyBlock(block);
		}
	}
}