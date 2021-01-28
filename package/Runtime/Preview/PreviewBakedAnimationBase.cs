using System;
using UnityEngine;
#if SHADERGRAPH_INSTALLED
using UnityEngine.Rendering;
#endif
#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.Rendering;

#endif
namespace needle.GpuAnimation
{
	[ExecuteAlways]
	public abstract class PreviewBakedAnimationBase : MonoBehaviour
	{
		public BakedAnimation Animation;
		public Material PreviewMaterial;
		public SkinQuality skinQuality = SkinQuality.Four;

		private static readonly int Animation1 = Shader.PropertyToID("_Animation");
		private static readonly int Skinning = Shader.PropertyToID("_Skinning");
		private static readonly int CurrentAnimation = Shader.PropertyToID("_CurrentAnimation");
		private static readonly int AnimationTime = Shader.PropertyToID("_AnimationTime");

		private Material _material;
		private MaterialPropertyBlock[] _blocks;

#if UNITY_EDITOR
		private static event Action RequestDestroyMaterial;
#endif

		protected virtual void OnEnable()
		{
			_material = null;

#if UNITY_EDITOR
			EditorApplication.projectChanged += () => _material = null;
			RequestDestroyMaterial += () => _material = null;
#endif

#if SHADERGRAPH_INSTALLED
			RenderPipelineManager.beginCameraRendering += BeforeRender;
#else
			Camera.onPreCull += BeforeRender;
#endif
		}

		protected virtual void OnDisable()
		{
			_material = null;
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

#if UNITY_EDITOR
			if (cam.name == "Preview Scene Camera") return;
			var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			if (prefabStage != null && !prefabStage.IsPartOfPrefabContents(this.gameObject)) return;
#endif

			if (!Animation || !Animation.HasBakedAnimation)
			{
				return;
			}

			if (!PreviewMaterial || PreviewUtility.IsPreviewMaterialForWrongRenderPipelineOrError(PreviewMaterial))
			{
				PreviewMaterial = PreviewUtility.CreateNewPreviewMaterial();
				if (!PreviewMaterial)
					return;
			}

			if (_blocks == null || _blocks.Length != Animation.ClipsCount)
			{
				_blocks = new MaterialPropertyBlock[Animation.ClipsCount];
			}

#if UNITY_EDITOR
			if (ShaderUtil.anythingCompiling)
			{
				RequestDestroyMaterial?.Invoke();
			}
#endif

			if (!_material || _material.shader != PreviewMaterial.shader || !_material.shader)
			{
				_material = new Material(PreviewMaterial);
			}
			else if (_material != PreviewMaterial)
				_material.CopyPropertiesFromMaterial(PreviewMaterial);

			_material.SetSkinQuality(this.skinQuality);

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
					block.SetFloat(AnimationTime, Time.time);
					Render(cam, skin.Mesh, _material, block, i, animationBake.ClipsInfos.Count);
					++matIndex;
				}
			}

#if UNITY_EDITOR
			EditorUtility.SetDirty(this.gameObject);
#endif
		}

		protected virtual void OnBeforeRender(Camera cam)
		{
		}

		protected abstract void Render(Camera cam, Mesh mesh, Material material, MaterialPropertyBlock block, int clipIndex, int clipsCount);
	}
}
