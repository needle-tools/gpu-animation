using System;
using UnityEditor;
using UnityEngine;

namespace Elaborate.AnimationBakery
{
	[ExecuteAlways]
	public class PreviewBakedAnimation : MonoBehaviour
	{
		public BakedAnimation Animation;
		public Material PreviewMaterial;
		public Vector3 Offset = new Vector3(0, 0, 1);
		public int Clip = -1;

		private BakedMeshSkinningData SkinBake => Animation.SkinBake;
		private BakedAnimationData AnimationBake => Animation.AnimationBake;

		private static readonly int Animation1 = Shader.PropertyToID("_Animation");
		private static readonly int Skinning = Shader.PropertyToID("_Skinning");
		private static readonly int CurrentAnimation = Shader.PropertyToID("_CurrentAnimation");

		private Material[] _previewMaterial;

		private void OnDisable()
		{
			_previewMaterial = null;
		}

		private void Update()
		{
			if (!Animation)
			{
				Debug.LogWarning("No Animation", this);
				return;
			}

			if (!PreviewMaterial)
			{
				Debug.LogWarning("No PreviewMaterial", this);
				return;
			}

			if (SkinBake == null || !SkinBake.Texture)
			{
				Debug.LogWarning("No Skin Bake Texture", this);
				return;
			}

			if (AnimationBake == null || !AnimationBake.Texture)
			{
				Debug.LogWarning("No Animation Bake Texture", this);
				return;
			}

			// skin = SkinBake.Texture;
			// anim = AnimationBake.Texture;

			if (_previewMaterial == null || _previewMaterial.Length != AnimationBake.ClipsInfos.Count)
				_previewMaterial = new Material[AnimationBake.ClipsInfos.Count];

			for (var i = 0; i < AnimationBake.ClipsInfos.Count; i++)
			{
				if (Clip != -1 && i != (Clip % AnimationBake.ClipsInfos.Count)) continue;
				var clip = AnimationBake.ClipsInfos[i];
				_previewMaterial[i] = new Material(PreviewMaterial);
				var mat = _previewMaterial[i];
				mat.CopyPropertiesFromMaterial(PreviewMaterial);
				mat.SetTexture(Animation1, AnimationBake.Texture);
				mat.SetTexture(Skinning, SkinBake.Texture);
				mat.SetVector(CurrentAnimation, clip.AsVector4);
				var offset = Offset;
				offset *= i;
				var matrix = transform.localToWorldMatrix * Matrix4x4.Translate(offset);
				Graphics.DrawMesh(Animation.SkinBake.Mesh, matrix, mat, 0);
			}
		}
	}
}