using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

namespace Elaborate.AnimationBakery
{
	[CreateAssetMenu(menuName = "Animation/" + nameof(BakedAnimation))]
	public class BakedAnimation : ScriptableObject
	{
		public BakedMeshSkinningData SkinBake;
		public BakedAnimationData AnimationBake;
		
		[SerializeField, HideInInspector]
		private List<Texture> hiddenTextureAssets = new List<Texture>();

#if UNITY_EDITOR
		[ContextMenu(nameof(SaveAssets))]
		public void SaveAssets()
		{
			if (EditorUtility.IsPersistent(this) == false)
			{
				Debug.LogWarning("Can't save " + this + " as asset because it is no asset");
				return;
			}

			for (var index = hiddenTextureAssets.Count - 1; index >= 0; index--)
			{
				var tex = hiddenTextureAssets[index];
				DestroyImmediate(tex, true);
				hiddenTextureAssets.RemoveAt(index);
			}

			SaveRenderTexture(SkinBake, "skinning");
			SaveRenderTexture(AnimationBake, "animation");
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
		}

		private void SaveRenderTexture(BakedData baked, string _name)
		{
			if (baked == null) return;
			var texture = baked.Texture;
			if (!texture) return;

			if (texture is RenderTexture rt)
			{
				var nt = new Texture2D(texture.width, texture.height, GraphicsFormatUtility.GetTextureFormat(rt.graphicsFormat), rt.mipmapCount > 0);
				RenderTexture.active = rt;
				nt.ReadPixels(new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), 0, 0);
				RenderTexture.active = null;
				texture = nt;
				// EditorUtility.CompressTexture(nt, TextureFormat.DXT5Crunched, TextureCompressionQuality.Best);
				rt.Release();
			}
			
			hiddenTextureAssets.Add(texture);
			texture.name = _name;
			texture.hideFlags = HideFlags.HideInHierarchy;
			AssetDatabase.AddObjectToAsset(texture, this);
			baked.Texture = texture;
		}

		// [ContextMenu(nameof(DeleteSubAssets))]
		// private void DeleteSubAssets()
		// {
		// 	foreach (var sub in GetSubAssets())
		// 	{
		// 		DestroyImmediate(sub, true);
		// 	}
		// }
		//
		// [ContextMenu(nameof(HideSubAssets))]
		// private void HideSubAssets()
		// {
		// 	foreach (var sub in GetSubAssets())
		// 	{
		// 		sub.hideFlags = HideFlags.HideInHierarchy;
		// 		EditorUtility.SetDirty(sub);
		// 	}
		// 	AssetDatabase.SaveAssets();
		// }
		//
		// [ContextMenu(nameof(ShowSubAssets))]
		// private void ShowSubAssets()
		// {
		// 	foreach (var sub in GetSubAssets())
		// 	{
		// 		sub.hideFlags = HideFlags.None;
		// 		EditorUtility.SetDirty(sub);
		// 	}
		// 	AssetDatabase.SaveAssets();
		// }
		//
		// private IEnumerable<Object> GetSubAssets()
		// {
		// 	var objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this));
		//
		// 	var list = new List<Object>();
		//
		// 	foreach (var o in objs)
		// 	{
		// 		if (AssetDatabase.IsSubAsset(o))
		// 			list.Add(o);
		// 	}
		//
		// 	return list;
		// }
#endif
	}
}