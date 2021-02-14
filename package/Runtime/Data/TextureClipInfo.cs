using System;
using UnityEngine;

namespace needle.GpuAnimation
{
	// keep in sync with AnimationTypes.TextureClipInfo in shader
	[Serializable]
	public struct TextureClipInfo
	{
		public int IndexStart;
		/// <summary>
		/// Essentially BoneCount * Frames
		/// </summary>
		public int Length;
			
		/// <summary>
		/// How many frames does this animation clip have
		/// </summary>
		public int Frames;
		public int FPS;
			
		// Must be in sync with AnimationTypes.cginc
		public static int Stride => sizeof(int) * 4;

		public Vector4 AsVector4 => new Vector4(IndexStart, Length, Frames, FPS);

		public override string ToString()
		{
			return "Start=" + IndexStart + ", Length=" + Length + ", Frames=" + Frames + ", FPS=" + FPS;
		}
	}
}