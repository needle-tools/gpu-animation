using UnityEngine;

namespace needle.GpuAnimation
{
	public class BakedAnimationRendererSingle : BakedAnimationRendererBase
	{
		public Transform Transform;
		
		protected override void OnInternalRender(Camera cam)
		{
			if (!Transform) return;
			animation.Render(Transform.localToWorldMatrix, Material, CurrentClipIndex, PropertyBlock, cam);
		}

		public BakedAnimationRendererSingle(BakedAnimation baked, Transform transform, Material material, MaterialPropertyBlock propertyBlock = null) : base(transform, baked, material, propertyBlock)
		{
			this.Transform = transform;
		}
	}
}