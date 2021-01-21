using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Elaborate.AnimationBakery
{
	[Serializable]
	public class BakedAnimationData : BakedData
	{
		public List<TextureClipInfo> ClipsInfos;
		public int TotalFrames => ClipsInfos.Sum(c => c.Length);
	}
	
	[Serializable]
	public struct TextureClipInfo
	{
		public int IndexStart;
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