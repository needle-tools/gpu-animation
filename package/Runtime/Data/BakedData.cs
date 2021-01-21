using System;
using UnityEngine;

namespace Elaborate.AnimationBakery
{
	[Serializable]
	public class BakedData : IDisposable
	{
		public Texture Texture;

		public void Dispose()
		{
			if (Texture is RenderTexture rt && rt) rt.Release();
		}
	}
}