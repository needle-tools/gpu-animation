using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor.Experimental.SceneManagement;

#endif
namespace needle.GpuAnimation
{
	[ExecuteAlways]
	public class PreviewBakedAnimation : PreviewBakedAnimationBase
	{
		public Vector3 Offset = new Vector3(0, 0, 1);
		public int Clip = -1;

		public Material Mat;

		protected override void Render(Camera cam, Mesh mesh, Material material, MaterialPropertyBlock block, int clipIndex, int clipsCount)
		{
			if (Clip != -1 && clipIndex != Clip)
				return;
			
			var offset = Offset;
			offset *= clipIndex;
			var matrix = transform.localToWorldMatrix * Matrix4x4.Translate(offset);

			for (var k = 0; k < mesh.subMeshCount; k++)
				Graphics.DrawMesh(mesh, matrix, material, 0, cam, k, block);

			Mat.mainTexture = Animation.Models[0].Animations.Texture;
		}
	}
}