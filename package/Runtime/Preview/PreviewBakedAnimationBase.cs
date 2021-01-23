using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor.Experimental.SceneManagement;

#endif
namespace needle.GpuAnimation
{
	[ExecuteAlways]
	public abstract class PreviewBakedAnimationBase : MonoBehaviour
	{
		public BakedAnimation Animation;
		public Material PreviewMaterial;

		private static readonly int Animation1 = Shader.PropertyToID("_Animation");
		private static readonly int Skinning = Shader.PropertyToID("_Skinning");
		private static readonly int CurrentAnimation = Shader.PropertyToID("_CurrentAnimation");

		private MaterialPropertyBlock[] _blocks;

		protected virtual void OnEnable()
		{
#if SHADERGRAPH_INSTALLED
			RenderPipelineManager.beginCameraRendering += BeforeRender;
#else
			Camera.onPreCull += BeforeRender;
#endif
		}

		protected virtual  void OnDisable()
		{
			_blocks = null;
#if SHADERGRAPH_INSTALLED
			RenderPipelineManager.beginCameraRendering -= BeforeRender;
#else
			Camera.onPreCull -= BeforeRender;
#endif
		}

#if SHADERGRAPH_INSTALLED
		private void BeforeRender(ScriptableRenderContext context, Camera cam)
		{
			BeforeRender(cam);
		}
#endif

		private void BeforeRender(Camera cam)
		{
			if (cam.name == "Preview Scene Camera") return;

#if UNITY_EDITOR
			var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			if (prefabStage != null && !prefabStage.IsPartOfPrefabContents(this.gameObject)) return;
#endif

			if (!Animation || !Animation.HasBakedAnimation)
			{
				return;
			}

			if (!PreviewMaterial)
			{
				PreviewMaterial = PreviewUtility.CreateNewPreviewMaterial();
				if (!PreviewMaterial)
					return;
			}

			if (_blocks == null || _blocks.Length != Animation.ClipsCount)
			{
				_blocks = new MaterialPropertyBlock[Animation.ClipsCount];
			}
			
			OnBeforeRender(cam);

			var matIndex = 0;
			foreach (var bake in Animation.Models)
			{
				var skin = bake.Skinning;
				var animationBake = bake.Animations;
				for (var i = 0; i < animationBake.ClipsInfos.Count; i++)
				{
					var clip = animationBake.ClipsInfos[i];
					if (_blocks[matIndex] == null) _blocks[matIndex] = new MaterialPropertyBlock();
					var block = _blocks[matIndex];
					block.SetTexture(Animation1, animationBake.Texture);
					block.SetTexture(Skinning, skin.Texture);
					block.SetVector(CurrentAnimation, clip.AsVector4);
					Render(cam, skin.Mesh, PreviewMaterial, block, i, animationBake.ClipsInfos.Count);
					++matIndex;
				}
			}
		}

		protected virtual void OnBeforeRender(Camera cam){}
		protected abstract void Render(Camera cam, Mesh mesh, Material material, MaterialPropertyBlock block, int clipIndex, int clipsCount);
	}
}