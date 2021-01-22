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
		private BakedMeshSkinningData _skinBake;
		private BakedAnimationData _animationBake;

		public BakedMeshSkinningData SkinBake
		{
			get
			{
				if (_skinBake == null || !_skinBake.Texture)
				{
					Bake();
				}
				return _skinBake;
			}
			private set => _skinBake = value;
		}

		public BakedAnimationData AnimationBake
		{
			get
			{
				if (_animationBake == null || !_animationBake.Texture)
				{
					Bake();
				}
				return _animationBake;
			}
			private set => _animationBake = value;
		}

		public bool HasBakedAnimation => ClipsCount > 0;
		public int ClipsCount => AnimationBake.ClipsInfos != null ? AnimationBake.ClipsInfos.Count : 0;
		
		
		[SerializeField] private ComputeShader Shader;

		[Header("Input Data")] [SerializeField]
		private GameObject GameObject;

		[SerializeField] private List<AnimationClip> Animations;

		
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

		
		
		
#if UNITY_EDITOR
		[SerializeField] private bool UpdateImmediately = true;
		private int previousHash;
		private int validateCounter;

		private async void OnValidate()
		{
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