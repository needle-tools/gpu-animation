using System.Collections.Generic;
using UnityEngine;

namespace needle.GpuAnimation
{
	public class AnimationClipData
	{
		public string Name;
		public float Duration;
		public float FrameRate;
		public Dictionary<Transform, AnimationClipCurveData> Curves;
	}
}