using UnityEngine;

namespace needle.GpuAnimation
{
	public static class PreviewUtility
	{
		public const string PreviewShaderName =
#if SHADERGRAPH_INSTALLED
			"Shader Graphs/PreviewBakedAnimation";
#else
			"BakedAnimation/PreviewAnimation";
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