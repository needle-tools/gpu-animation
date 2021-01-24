using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Experimental.SceneManagement;
#endif

namespace needle.GpuAnimation
{
	public abstract class BakedAnimationRendererBase : IBakedAnimationRenderer
	{
		public Object Owner { get; set; }
		public Material Material { get; set; }
		public MaterialPropertyBlock PropertyBlock;

		protected BakedAnimation animation;

		public int CurrentClipIndex
		{
			get { return currentClipIndex; }
			set
			{
#if UNITY_EDITOR
				if (value < 0 || value >= animation.ClipsCount)
				{
					if (!currentClipIndexOutOfRange)
					{
						currentClipIndexOutOfRange = true;
						Debug.LogWarning("No clip at index " + value + " in " + animation, Owner);
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

		protected BakedAnimationRendererBase(Component owner, BakedAnimation baked, Material material, MaterialPropertyBlock propertyBlock = null)
		{
			this.Owner = owner;
			this.animation = baked;
			this.Material = material;
			PropertyBlock = propertyBlock;
		}

		public void StartRendering()
		{
			if (isRendering) return;
			isRendering = true;
			// TODO: SRP callback
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
			if (!Owner)
			{
				Debug.LogWarning("Owner stopped existing, stopping rendering " + Owner, Owner);
				StopRendering();
				return;
			}

#if UNITY_EDITOR
			if (cam.name == "Preview Scene Camera") return;
			GameObject go = null;
			switch (Owner)
			{
				case Component comp:
					go = comp.gameObject;
					break;
				case GameObject _go:
					go = _go;
					break;
			}
			if (go != null)
			{
				var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
				if (prefabStage != null && !prefabStage.IsPartOfPrefabContents(go)) return;
			}
#endif

			OnInternalRender(cam);
		}

		protected abstract void OnInternalRender(Camera cam);
	}
}