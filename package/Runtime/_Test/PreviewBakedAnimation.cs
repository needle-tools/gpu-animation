using UnityEngine;

namespace Elaborate.AnimationBakery
{
	[ExecuteAlways]
	public class PreviewBakedAnimation : MonoBehaviour
	{
		public BakedAnimation Animation;
		public Material PreviewMaterial;
		public Vector3 Offset = new Vector3(0, 0, 1);

		private BakedMeshSkinningData SkinBake => Animation.SkinBake;
		private BakedAnimationData AnimationBake => Animation.AnimationBake;

		private static readonly int Animation1 = Shader.PropertyToID("_Animation");
		private static readonly int Skinning = Shader.PropertyToID("_Skinning");
		private static readonly int CurrentAnimation = Shader.PropertyToID("_CurrentAnimation");

		private Material[] _previewMaterial;
		private AnimationBaker _baker;

		private void Update()
		{
			if ((!Animation || !_baker) && TryGetComponent(out _baker))
				Animation = _baker.Target;

			if (!Animation) return;
			if (!PreviewMaterial) return;
			if (SkinBake == null || !SkinBake.Texture) return;
			if (AnimationBake == null || !AnimationBake.Texture) return;

			if (_previewMaterial == null || _previewMaterial.Length != AnimationBake.ClipsInfos.Count)
				_previewMaterial = new Material[AnimationBake.ClipsInfos.Count];

			for (var i = 0; i < AnimationBake.ClipsInfos.Count; i++)
			{
				var clip = AnimationBake.ClipsInfos[i];
				if (!_previewMaterial[i]) _previewMaterial[i] = new Material(PreviewMaterial);
				var mat = _previewMaterial[i];
				mat.CopyPropertiesFromMaterial(PreviewMaterial);
				mat.SetTexture(Animation1, AnimationBake.Texture);
				mat.SetTexture(Skinning, SkinBake.Texture);
				mat.SetVector(CurrentAnimation, clip.AsVector4);
				Graphics.DrawMesh(Animation.SkinBake.Mesh, Matrix4x4.Translate(transform.position + Offset * (1 + i)), mat, 0);
			}
		}
	}
}