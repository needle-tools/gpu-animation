using System;
using System.Collections.Generic;
using UnityEngine;

namespace needle.GpuAnimation
{
	public static class BakedAnimationExtensions
	{
		private static readonly MaterialPropertyBlock fallbackBlock = new MaterialPropertyBlock();

		public static readonly int AnimationProp = Shader.PropertyToID("_Animation");
		public static readonly int SkinningProp = Shader.PropertyToID("_Skinning");
		public static readonly int CurrentAnimationProp = Shader.PropertyToID("_CurrentAnimation");

		private static readonly Dictionary<Transform, BakedAnimationRenderer> renderers = new Dictionary<Transform, BakedAnimationRenderer>();

		public static BakedAnimationRenderer StartRendering(this BakedAnimation bakedAnimation, Transform transform, Material material, int animationClipIndex, MaterialPropertyBlock block = null)
		{
			if (!renderers.ContainsKey(transform))
			{
				renderers.Add(transform, new BakedAnimationRenderer(bakedAnimation, transform, material, block));
			}

			var renderer = renderers[transform];
			renderer.CurrentClipIndex = animationClipIndex;
			renderer.StartRendering();
			return renderer;
		}

		public static void StopRendering(this BakedAnimation bakedAnimation, Transform transform)
		{
			if (!transform) return;
			if (renderers.ContainsKey(transform)) renderers[transform].StopRendering();
		}
		
		public static void Render(this BakedAnimation bakedAnimation, Matrix4x4 transform, Material material, int animationClipIndex,
			MaterialPropertyBlock block = null, Camera cam = null)
		{
			if (!bakedAnimation) throw new Exception("Missing BakedAnimation reference");
			if (animationClipIndex < 0 || animationClipIndex >= bakedAnimation.ClipsCount) return;
			
			foreach (var model in bakedAnimation.Models)
			{
				if (!model.IsValid) continue;
				var skin = model.Skinning;
				var animationBake = model.Animations;
				var clip = animationBake.ClipsInfos[animationClipIndex];
				var mesh = skin.Mesh;
					
				if (block == null)
				{
					fallbackBlock.Clear();
					block = fallbackBlock;
				}
					
				block.SetTexture(AnimationProp, animationBake.Texture);
				block.SetTexture(SkinningProp, skin.Texture);
				block.SetVector(CurrentAnimationProp, clip.AsVector4);
				for (var k = 0; k < mesh.subMeshCount; k++)
					Graphics.DrawMesh(mesh, transform, material, 0, cam, k, block);
			}
		}
	}
}