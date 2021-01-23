using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace needle.GpuAnimation
{
	public class BakedAnimationBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
	{
		public int callbackOrder { get; }

		public void OnPreprocessBuild(BuildReport report)
		{
			BakeIntoSubAssets();
		}

		public void OnPostprocessBuild(BuildReport report)
		{
		}

		private void BakeIntoSubAssets()
		{
			var guids = AssetDatabase.FindAssets("t:" + typeof(BakedAnimation));
			var paths = guids.Select(AssetDatabase.GUIDToAssetPath);
			var assets = paths.Select(AssetDatabase.LoadAssetAtPath<BakedAnimation>);
			foreach (var baked in assets)
			{
				if (!baked) continue;
				var output = "Assets/_Baked/" + baked.name;
				baked.BakeAnimations();
				foreach (var entry in baked.Models)
				{
					CreatePersistentAsset(entry.Animations, output, entry.Mesh.name + "-animations");
					CreatePersistentAsset(entry.Skinning, output, entry.Mesh.name + "-skinning");
				}
			}
			AssetDatabase.SaveAssets();
		}

		private bool CreatePersistentAsset(BakedData data, string outputDirectory, string name)
		{
			if (data == null) return false;
			if (!data.Texture) return false;
			var src = data.Texture as RenderTexture;
			if (!src) return false;
			var dst = new Texture2D(src.width, src.height, GraphicsFormatUtility.GetTextureFormat(src.graphicsFormat), src.mipmapCount > 0);
			RenderTexture.active = src;
			dst.ReadPixels(new Rect(Vector2.zero, new Vector2(dst.width, dst.height)), 0, 0);
			RenderTexture.active = null;
			if (!Directory.Exists(outputDirectory))
				Directory.CreateDirectory(outputDirectory);
			var path = outputDirectory + "/" + name + ".asset";
			if (File.Exists(path)) File.Delete(path);
			AssetDatabase.CreateAsset(dst, path);
			data.Texture = dst;
			return true;
		}
	}
}