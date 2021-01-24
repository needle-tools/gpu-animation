using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace needle.GpuAnimation
{
	public static class BakedAnimationExtensions
	{
		private static readonly MaterialPropertyBlock fallbackBlock = new MaterialPropertyBlock();
		private static readonly Dictionary<Transform, BakedAnimationRenderer> renderers = new Dictionary<Transform, BakedAnimationRenderer>();

		public static readonly int AnimationProp = Shader.PropertyToID("_Animation");
		public static readonly int SkinningProp = Shader.PropertyToID("_Skinning");
		public static readonly int CurrentAnimationProp = Shader.PropertyToID("_CurrentAnimation");
		public static readonly int PositionBuffer = Shader.PropertyToID("positions");


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
			if (!bakedAnimation.ClipIndexInRange(animationClipIndex)) return;
			
			bakedAnimation.InternalRenderLoop(animationClipIndex, block, c =>
			{
				Graphics.DrawMesh(c.Mesh, transform, material, 0, cam, c.SubMeshIndex, c.Block);
			});
		}

		private static readonly ComputeBuffer argsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
		private static readonly uint[] argsData = new uint[5];
		
		public static void Render(this BakedAnimation bakedAnimation, Material material, int animationClipIndex, 
			ComputeBuffer matrices, MaterialPropertyBlock block = null, Camera cam = null)
		{
			if (!bakedAnimation.ClipIndexInRange(animationClipIndex)) return;
			
			bakedAnimation.InternalRenderLoop(animationClipIndex, block, c =>
			{
				var mesh = c.Mesh;
				c.Block.SetBuffer(PositionBuffer, matrices);
				argsData[0] = mesh.GetIndexCount(c.SubMeshIndex);
				argsData[1] = (uint) (matrices.count);
				argsData[2] = mesh.GetIndexStart(c.SubMeshIndex);
				argsData[3] = mesh.GetBaseVertex(c.SubMeshIndex);
				argsBuffer.SetData(argsData);
				Graphics.DrawMeshInstancedIndirect(mesh, c.SubMeshIndex, material, 
					new Bounds(Vector3.zero, Vector3.one * 100000), argsBuffer, 0, c.Block,
					ShadowCastingMode.On, true, 0, cam);
			});
		}

		
		
		private struct RenderLoopCallbackData
		{
			public Mesh Mesh;
			public int SubMeshIndex;
			public MaterialPropertyBlock Block;
		}

		private static bool ClipIndexInRange(this BakedAnimation bakedAnimation, int animationClipIndex)
		{
			if (!bakedAnimation) return false;
			return animationClipIndex >= 0 && (animationClipIndex < bakedAnimation.ClipsCount);
		}
		
		private static void InternalRenderLoop(this BakedAnimation bakedAnimation, int clipIndex, MaterialPropertyBlock block, 
			Action<RenderLoopCallbackData> callback)
		{
			if (!bakedAnimation) return;
			
			foreach (var model in bakedAnimation.Models)
			{
				if (!model.IsValid) continue;
				var skin = model.Skinning;
				var animationBake = model.Animations;
				var clip = animationBake.ClipsInfos[clipIndex];
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
				{
					callback(new RenderLoopCallbackData()
					{
						Mesh = mesh,
						SubMeshIndex = k,
						Block = block
					});
				}
			}
		}
	}
}