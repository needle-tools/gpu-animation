using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Elaborate.AnimationBakery
{
	[CreateAssetMenu(menuName = "Animation/" + nameof(BakedAnimation), order = -1000)] 
	public class BakedAnimation : ScriptableObject
	{
		[Header("Runtime Data")] public BakedMeshSkinningData SkinBake;
		public BakedAnimationData AnimationBake;
		
		[SerializeField, HideInInspector] private List<Texture> hiddenTextureAssets = new List<Texture>();

		
		
		
#if UNITY_EDITOR
		[Header("Setup")] [SerializeField] private ComputeShader Shader;

		[Header("Input Data")] [SerializeField]
		private GameObject GameObject;

		[SerializeField] private List<AnimationClip> Animations;

		[SerializeField] private bool UpdateImmediately = true;

		private int previousHash;

		private int validateCounter;

		private async void OnValidate()
		{
			// if (!Selection.Contains(this)) return;
			var count = ++validateCounter;
			await Task.Delay(100);
			while (EditorApplication.isCompiling || EditorApplication.isUpdating) await Task.Delay(100);
			if (validateCounter != count) return;
			if (UpdateImmediately)
			{
				if (GameObject)
				{
					var hash = GameObject.GetHashCode() + Animations.Sum(e => e ? e.GetHashCode() : 0);
					var changed = previousHash != hash;
					previousHash = hash;
					if (changed)
						Bake();
				}
			}
		}

		[ContextMenu(nameof(Bake))]
		private void Bake()
		{
			if (!GameObject)
			{
				Debug.LogWarning("Can not bake: No GameObject assigned", this);
				return;
			}

			if (!Animations.Any(a => a))
			{
				Debug.LogWarning("Can not bake: No Animations assigned", this);
				return;
			}

			var instance = Instantiate(GameObject);
			try
			{
				instance.hideFlags = HideFlags.HideAndDontSave;
				var animator = instance.GetComponentInChildren<Animator>();
				var renderer = instance.GetComponentInChildren<SkinnedMeshRenderer>();
				var animData = AnimationDataProvider.GetAnimations(animator, Animations, renderer, 0, -1);
				AnimationBake = AnimationTextureProvider.BakeAnimation(animData, Shader);
				SkinBake = AnimationTextureProvider.BakeSkinning(renderer.sharedMesh, Shader);
				Save();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			finally
			{
				DestroyImmediate(instance);
			}
		}


		[ContextMenu(nameof(Save))]
		public void Save()
		{
			if (EditorUtility.IsPersistent(this) == false)
			{
				Debug.LogWarning("Can't save " + this + " as asset because it is no asset");
				return;
			}

			SaveRenderTexture(SkinBake, "skinning");
			SaveRenderTexture(AnimationBake, "animation");

			for (var index = hiddenTextureAssets.Count - 1; index >= 0; index--)
			{
				var tex = hiddenTextureAssets[index];
				if (tex == SkinBake.Texture || tex == AnimationBake.Texture) continue;
				DestroyImmediate(tex, true);
				hiddenTextureAssets.RemoveAt(index);
			}

			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		private void SaveRenderTexture(BakedData baked, string _name)
		{
			if (baked == null) return;
			var texture = baked.Texture;
			if (!texture) return;
			if (EditorUtility.IsPersistent(texture))
			{
				Debug.Log("Texture is already an asset: " + baked, texture);
				return;
			}

			if (texture is RenderTexture rt)
			{
				var nt = new Texture2D(texture.width, texture.height, GraphicsFormatUtility.GetTextureFormat(rt.graphicsFormat), rt.mipmapCount > 0);
				RenderTexture.active = rt;
				nt.ReadPixels(new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), 0, 0);
				RenderTexture.active = null;
				texture = nt;
				// EditorUtility.CompressTexture(nt, TextureFormat.ASTC_RGBA_4x4, TextureCompressionQuality.Best);
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