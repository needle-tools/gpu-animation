using System;
using UnityEngine;

namespace needle.GpuAnimation.TestAssets
{
	[ExecuteInEditMode]
	public class MyCustomComponent : MonoBehaviour
	{
		public BakedAnimation Bake;
		public Material Material;
		public int Animation;

		private IBakedAnimationRenderer rend;

		private void OnEnable()
		{
			rend = new BakedAnimationRendererSingle(Bake, this.transform, Material);
			rend.StartRendering();
		}

		private void OnDisable()
		{
			rend?.StopRendering();
		}

		private void OnValidate()
		{
			if (!Bake)
			{
				rend?.StopRendering();
			}
			else
			{
				rend?.StartRendering();
			}
		}

		private void Update()
		{
			rend.CurrentClipIndex = Animation;
		}
	}
}