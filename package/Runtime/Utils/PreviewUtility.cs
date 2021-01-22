﻿using UnityEngine;

namespace needle.GpuAnimation
{
	public static class PreviewUtility
	{
		public const string PreviewShaderName = "BakedAnimation/PreviewAnimation";
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
				var val = 75 / 255f;
				mat.SetColor(Color, new Color(val, val, val));
				mat.SetFloat(EmissionFactor, 0.5f);
				return mat;
			}

			return null;
		}
		
		private static readonly int Color = Shader.PropertyToID("_Color");
		private static readonly int EmissionFactor = Shader.PropertyToID("_EmissionFactor");

	}
}