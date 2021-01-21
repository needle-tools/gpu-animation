using UnityEngine;

namespace Elaborate.AnimationBakery
{
	[CreateAssetMenu(menuName = "Animation/" + nameof(BakedAnimation), fileName = "BakedAnimation", order = 0)]
	public class BakedAnimation : ScriptableObject
	{
		public BakedMeshSkinningData SkinBake;
		public BakedAnimationData bakedAnimationBake;
	}
}