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

		private Material[] _materials;

		private void OnDisable()
		{
			_materials = null;
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

			if (_materials == null || _materials.Length != AnimationBake.ClipsInfos.Count)
			{
				_materials = new Material[AnimationBake.ClipsInfos.Count];
			}

			for (var i = 0; i < AnimationBake.ClipsInfos.Count; i++)
			{
				if (Clip != -1 && i != (Clip % AnimationBake.ClipsInfos.Count)) continue;
				var clip = AnimationBake.ClipsInfos[i];
				if (!_materials[i])
				{
					_materials[i] = new Material(PreviewMaterial);
				}
				var mat = _materials[i];
				mat.CopyPropertiesFromMaterial(PreviewMaterial);
				mat.SetTexture(Animation1, AnimationBake.Texture);
				mat.SetTexture(Skinning, SkinBake.Texture);
				mat.SetVector(CurrentAnimation, clip.AsVector4);
				var offset = Offset;
				offset *= i;
				var matrix = transform.localToWorldMatrix * Matrix4x4.Translate(offset);
				Graphics.DrawMesh(Animation.SkinBake.Mesh, matrix, _materials[i], 0);
			}
		}
	}
}