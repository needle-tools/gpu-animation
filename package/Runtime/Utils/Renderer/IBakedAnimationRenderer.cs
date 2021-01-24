using UnityEngine;

namespace needle.GpuAnimation
{
	public interface IBakedAnimationRenderer
	{
		Object Owner { get; set; }
		void StartRendering();
		void StopRendering();
		Material Material { get; set; }
		int CurrentClipIndex { get; set; }
	}
}