using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Experimental.SceneManagement;
#endif
namespace needle.GpuAnimation
{
	[ExecuteAlways]
	public class PreviewBakedAnimation : MonoBehaviour
	{
		public BakedAnimation Animation;
		public Material PreviewMaterial;
		
		public Vector3 Offset = new Vector3(0, 0, 1);
		public int Clip = -1;

		private static readonly int Animation1 = Shader.PropertyToID("_Animation");
		private static readonly int Skinning = Shader.PropertyToID("_Skinning");
		private static readonly int CurrentAnimation = Shader.PropertyToID("_CurrentAnimation");
		
		private Material[] _materials;

		private void OnEnable()
		{
			Camera.onPreCull += BeforeRender;
		}

		private void OnDisable()
		{
			Camera.onPreCull -= BeforeRender;
			_materials = null;
		}

		private void BeforeRender(Camera cam)
		{
			if (cam.name == "Preview Scene Camera") return;

#if UNITY_EDITOR
			var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			if (prefabStage != null && !prefabStage.IsPartOfPrefabContents(this.gameObject)) return;
#endif

			if (!Animation || !Animation.HasBakedAnimation)
			{
				Debug.Log("No anim");
				return;
			}

			if (!PreviewMaterial)
			{
				PreviewMaterial = PreviewUtility.CreateNewPreviewMaterial();
				if (!PreviewMaterial)
					return;
			}

			if (_materials == null || _materials.Length != Animation.ClipsCount)
			{
				_materials = new Material[Animation.ClipsCount];
			}

			var matIndex = 0;
			foreach (var bake in Animation.Models)
			{
				var skin = bake.Skinning;
				var animationBake = bake.Animations;
				for (var i = 0; i < animationBake.ClipsInfos.Count; i++)
				{
					if (Clip != -1 && i != (Clip % animationBake.ClipsInfos.Count))
					{
						Debug.Log("no clip");
						continue;
					}
					var clip = animationBake.ClipsInfos[i];
					
					if (!_materials[matIndex]) _materials[matIndex] = new Material(PreviewMaterial);
					var mat = _materials[matIndex];
					
					mat.CopyPropertiesFromMaterial(PreviewMaterial);
					mat.SetTexture(Animation1, animationBake.Texture);
					mat.SetTexture(Skinning, skin.Texture);
					mat.SetVector(CurrentAnimation, clip.AsVector4);
					var offset = Offset;
					offset *= i;
					var matrix = transform.localToWorldMatrix * Matrix4x4.Translate(offset);
					
					for (var k = 0; k < skin.Mesh.subMeshCount; k++)
						Graphics.DrawMesh(skin.Mesh, matrix, _materials[matIndex], 0, cam, k);
					++matIndex;
				}
			}
		}
	}
}