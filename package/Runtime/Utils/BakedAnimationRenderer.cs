using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Experimental.SceneManagement;
#endif

namespace needle.GpuAnimation
{
	public class BakedAnimationRenderer
	{
		public BakedAnimation BakedAnimation;
		public Transform Transform;
		public Material Material;
		public MaterialPropertyBlock PropertyBlock;

		public int CurrentClipIndex
		{
			get { return currentClipIndex; }
			set
			{
#if UNITY_EDITOR
				if (value < 0 || value >= BakedAnimation.ClipsCount)
				{
					if (!currentClipIndexOutOfRange)
					{
						currentClipIndexOutOfRange = true;
						Debug.LogWarning("No clip at index " + value + " in " + BakedAnimation, Transform);
					}
				}
				else currentClipIndexOutOfRange = false;
#endif
				currentClipIndex = value;
			}
		}

#if UNITY_EDITOR
		private bool currentClipIndexOutOfRange = false;
#endif

		private int currentClipIndex;
		private bool isRendering = false;

		public BakedAnimationRenderer(BakedAnimation animationData, Transform transform, Material material, MaterialPropertyBlock propertyBlock = null)
		{
			this.BakedAnimation = animationData;
			this.Transform = transform;
			this.Material = material;
			this.PropertyBlock = propertyBlock;
		}

		public void StartRendering()
		{
			if (isRendering) return;
			isRendering = true;
			// TODO: URP callback
			Camera.onPreCull += OnRender;
		}

		public void StopRendering()
		{
			if (!isRendering) return;
			isRendering = false;
			Camera.onPreCull -= OnRender;
		}

		private void OnRender(Camera cam)
		{
			if (!Transform)
			{
				StopRendering();
				return;
			}

#if UNITY_EDITOR
			if (cam.name == "Preview Scene Camera") return;
			var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			if (prefabStage != null && !prefabStage.IsPartOfPrefabContents(this.Transform.gameObject)) return;
#endif

			BakedAnimation.Render(Transform.localToWorldMatrix, Material, CurrentClipIndex, PropertyBlock, cam);
		}
	}
}