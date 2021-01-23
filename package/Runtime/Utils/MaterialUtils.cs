using System;
using System.Linq;
using UnityEngine;

namespace needle.GpuAnimation
{
	public enum SkinQuality
	{
		One = 0,
		Two = 1,
		Three = 2,
		Four = 3
	}

	public static class MaterialUtils
	{
		public static string[] SkinQualityKeywords = new[]
		{
			"SKIN_QUALITY_ONE",
			"SKIN_QUALITY_TWO",
			"SKIN_QUALITY_THREE",
			"SKIN_QUALITY_FOUR"
		};
		
		public static string ToShaderKeyword(this SkinQuality quality)
		{
			var index = (int) quality;
			return SkinQualityKeywords.Where((t, i) => index == i).FirstOrDefault();
		}

		public static void SetSkinQuality(this Material mat, SkinQuality quality)
		{
			var enable = (int) quality;
			for (var i = 0; i < SkinQualityKeywords.Length; i++)
			{
				if(i == enable)
					mat.EnableKeyword(SkinQualityKeywords[i]);
				else 
					mat.DisableKeyword(SkinQualityKeywords[i]);
			}
		}
	}
}