using System;
using System.Collections.Generic;
using System.Linq;

namespace needle.GpuAnimation
{
	[Serializable]
	public class BakedAnimationData : BakedData
	{
		public List<TextureClipInfo> ClipsInfos;
		public int TotalFrames => ClipsInfos.Sum(c => c.Length);
	}
	
}