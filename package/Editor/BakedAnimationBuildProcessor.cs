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
			BakeTexturesAsAssets(true);
		}

		public void OnPostprocessBuild(BuildReport report)
		{
		}

		[MenuItem("Tools/Gpu Animation/" + nameof(ForceBakeTexturesAsAssets))]
		private static void ForceBakeTexturesAsAssets()
		{
			BakeTexturesAsAssets(false);
		}
		
		private static void BakeTexturesAsAssets(bool skipIfHasData)
		{
			var guids = AssetDatabase.FindAssets("t:" + typeof(BakedAnimation));
			var paths = guids.Select(AssetDatabase.GUIDToAssetPath);
			var assets = paths.Select(AssetDatabase.LoadAssetAtPath<BakedAnimation>);
			var savedAssets = 0;
			foreach (var baked in assets)
			{
				if (!baked) continue;
				if (skipIfHasData && baked.HasBakedAnimation)
				{
					Debug.Log("Skip " + baked + " because it has data", baked);
					continue;
				}
				
				var output = "Assets/_Baked/" + baked.name;
				if (!baked.BakeAnimations())
				{
					Debug.LogWarning("Failed baking " + baked, baked);
				}

				foreach (var entry in baked.Models)
				{
					if (!CreatePersistentAsset(entry.Animations, output, entry.Mesh.name + "-animations", ref savedAssets))
						Debug.LogWarning("Failed storing animations texture " + baked, baked);
					if (!CreatePersistentAsset(entry.Skinning, output, entry.Mesh.name + "-skinning", ref savedAssets))
						Debug.LogWarning("Failed storing skinning texture " + baked, baked);
				}

				EditorUtility.SetDirty(baked);
			}


			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			AssetDatabase.SaveAssets();
			Debug.Log("Saved " + savedAssets + " baked textures");
		}

		private static bool CreatePersistentAsset(BakedData data, string outputDirectory, string name, ref int savedFilesCounter)
		{
			if (data == null) return false;
			if (!data.Texture) return false;
			var src = data.Texture as RenderTexture;
			if (!src) return false;
			if (EditorUtility.IsPersistent(src)) return false;
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
			savedFilesCounter += 1;
			return true;
		}
	}
}