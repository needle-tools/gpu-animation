using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Elaborate.AnimationBakery
{
	[Serializable]
	public class BakedData : IDisposable
	{
		public Texture Texture;

		public void Dispose()
		{
			#if UNITY_EDITOR
			if (EditorUtility.IsPersistent(Texture)) return;
			#endif
			if (Texture is RenderTexture rt && rt)
			{
				Debug.Log("Release " + this);
				rt.Release();
			}
		}
	}
}