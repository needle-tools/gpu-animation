using UnityEngine;

namespace needle.GpuAnimation
{
	public static class PreviewUtility
	{
		public const string PreviewShaderNameURP = "Shader Graphs/PreviewBakedAnimation";
		public const string PreviewShaderNameBuiltIn = "BakedAnimation/PreviewAnimation";
		public const string PreviewShaderName =
#if SHADERGRAPH_INSTALLED
			PreviewShaderNameURP;
#else
			PreviewShaderNameBuiltIn;
#endif

		private static bool _searchedPreviewShader;
		private static Shader _previewShader;

		public static Shader PreviewShader
		{
			get
			{
				if (!_searchedPreviewShader && !_previewShader)
				{
					_searchedPreviewShader = true;
					_previewShader = Shader.Find(PreviewShaderName);
				}

				return _previewShader;
			}
		}

		public static bool IsPreviewMaterialForWrongRenderPipeline(Material mat)
		{
#if SHADERGRAPH_INSTALLED
			if (mat.shader.name == PreviewShaderNameBuiltIn) return true;
#else
			if (mat.shader.name == PreviewShaderNameURP) return true;
#endif
			return false;
		}

		public static Material CreateNewPreviewMaterial()
		{
			if (PreviewShader)
			{
				var mat = new Material(PreviewShader);
				mat.name = nameof(BakedAnimation) + "-Preview";
				var val = 102 / 255f;
				mat.SetColor(Color, new Color(val, val, val));
				mat.SetFloat(EmissionFactor, 0.2f);
				return mat;
			}

			return null;
		}

		private static readonly int Color = Shader.PropertyToID("_Color");
		private static readonly int EmissionFactor = Shader.PropertyToID("_EmissionFactor");
	}
}