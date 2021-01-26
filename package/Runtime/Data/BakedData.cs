using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace needle.GpuAnimation
{
	[Serializable]
	public class BakedData : IDisposable
	{
		public Texture Texture;
		public ComputeBuffer Buffer;

		public void Dispose()
		{
			if(Buffer?.IsValid() ?? false)
				Buffer.Release();
			Buffer = null;
			
			#if UNITY_EDITOR
			if (EditorUtility.IsPersistent(Texture)) return;
			#endif
			if (Texture is RenderTexture rt && rt)
			{
				rt.Release();
			}
		}
	}
}