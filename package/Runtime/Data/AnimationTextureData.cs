using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Elaborate.AnimationBakery
{
	[Serializable]
	public class AnimationTextureData : BakedData
	{
		public List<Clip> Animations;

		public int TotalFrames => Animations.Sum(c => c.TotalLength);

		[Serializable]
		public struct Clip
		{
			public int IndexStart;
			public int TotalLength;
			/// <summary>
			/// How many frames does this animation clip have
			/// </summary>
			public int Frames;
			
			public static int Stride => sizeof(int) * 3;

			public override string ToString()
			{
				return "Start=" + IndexStart + ", Length=" + TotalLength;
			}
		}
	}
}