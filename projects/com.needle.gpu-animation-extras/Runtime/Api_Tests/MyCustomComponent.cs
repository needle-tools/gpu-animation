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

		private BakedAnimationRenderer rend;

		private void OnEnable()
		{
			rend = Bake.StartRendering(this.transform, Material, Animation);
		}

		private void OnDisable()
		{
			rend?.StopRendering();
		}

		private void Update()
		{
			rend.CurrentClipIndex = Animation;
		}
	}
}