using System.Collections.Generic;
using UnityEngine;

namespace Elaborate.AnimationBakery
{
	public class AnimationClipData
	{
		public string Name;
		public float Duration;
		public float FrameRate;
		public Dictionary<Transform, AnimationClipCurveData> Curves;
	}
}