using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace needle.GpuAnimation
{
	public static class AnimationTextureProvider
	{
		public static bool DebugLog = false;

		public static BakedAnimationData BakeAnimation(IEnumerable<AnimationTransformationData> animationData, ComputeShader shader, bool bakeMesh,
			Mesh mesh = null)
		{
			if (bakeMesh && !mesh) throw new ArgumentNullException(nameof(mesh));

			using (var animationBuffer = GetAnimatedBonesBuffer(animationData, out var clipInfos))
			{
				return BakeAnimation(shader, animationBuffer, clipInfos, bakeMesh, mesh);
			}
		}

		public static BakedAnimationData BakeAnimation(ComputeShader shader, ComputeBuffer animationBuffer, List<TextureClipInfo> clipInfos, bool bakeMesh,
			Mesh mesh = null)
		{
			if (bakeMesh && (!mesh || mesh == null)) throw new ArgumentNullException(nameof(mesh));
			Vector2Int textureSize;
			if (bakeMesh)
			{
				var animationsLength = clipInfos.Sum(clip => clip.Length);
				Debug.Log(animationsLength);
				const int requiredPixel = 1;
				textureSize = ToTextureSize(animationsLength * mesh.vertexCount * requiredPixel);
			}
			else
			{
				textureSize = ToTextureSize(animationBuffer.count * 4);
			}

			if (DebugLog) Debug.Log("Bake into " + textureSize + " Texture");
			var texture = new RenderTexture(textureSize.x, textureSize.y, 0, RenderTextureFormat.ARGBHalf);
			texture.name = "animation";
			texture.enableRandomWrite = true;
			texture.useMipMap = false;
			texture.filterMode = FilterMode.Point;
			texture.Create();
			
			var res = new BakedAnimationData
			{
				Texture = texture, 
				ClipsInfos = clipInfos
			};

			if (!bakeMesh)
			{
				var kernel = shader.FindKernel("BakeAnimationTexture_Float4");
				shader.SetBuffer(kernel, "Matrices", animationBuffer);
				shader.SetTexture(kernel, "Texture", texture);
				shader.Dispatch(kernel, Mathf.CeilToInt(animationBuffer.count / 32f), 1, 1);
			}
			else
			{
				var kernel = shader.FindKernel("BakeAnimationMeshTexture");
				var boneWeights = mesh.boneWeights;
				using (var vertexBuffer = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 3))
				using (var normalBuffer = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 3))
				using (var tangentBuffer = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 4))
				using(var weightsBuffer = new ComputeBuffer(boneWeights.Length, sizeof(float)*4 + sizeof(int)*4))
				using (var clips = new ComputeBuffer(res.ClipsInfos.Count, TextureClipInfo.Stride))
				{
					vertexBuffer.SetData(mesh.vertices);
					normalBuffer.SetData(mesh.normals);
					tangentBuffer.SetData(mesh.tangents);
					weightsBuffer.SetData(boneWeights);
					clips.SetData(clipInfos);
					shader.SetBuffer(kernel, "VertexPositions", vertexBuffer);
					shader.SetBuffer(kernel, "Normals", normalBuffer);
					shader.SetBuffer(kernel, "Tangents", tangentBuffer);
					shader.SetBuffer(kernel, "Weights", weightsBuffer);
					shader.SetBuffer(kernel, "Clips", clips);
					shader.SetBuffer(kernel, "Matrices", animationBuffer);
					shader.SetTexture(kernel, "Texture", texture);
					shader.SetInt("TotalLength", res.TotalLength);
					var tx = Mathf.CeilToInt(vertexBuffer.count / 64f);
					Debug.Log(vertexBuffer.count + ", " + tx);
					shader.Dispatch(kernel, tx, 1, 1);
				}
			}

			return res;
		}

		public static ComputeBuffer GetAnimatedBonesBuffer(IEnumerable<AnimationTransformationData> animationData, out List<TextureClipInfo> clipInfos)
		{
			var matrixData = new List<Bone>();
			clipInfos = new List<TextureClipInfo>();
			var anyScaled = false;
			foreach (var anim in animationData)
			{
				var clip = new TextureClipInfo();
				clip.IndexStart = matrixData.Count;
				foreach (var boneData in anim.BoneData)
				{
					clip.Frames = boneData.Transformations.Count;
					foreach (var frame in boneData.Transformations)
					{
						matrixData.Add(new Bone(frame.Matrix));
						if (frame.Scaled) anyScaled = true;
					}
				}

				clip.FPS = anim.SampledFramesPerSecond;
				clip.Length = matrixData.Count - clip.IndexStart;
				// if (DebugLog)
					Debug.Log("Clip \t" + clipInfos.Count + "\t" + clip);
				clipInfos.Add(clip);
			}

			if (DebugLog)
				Debug.Log(clipInfos.Count + " clip(s), data is " + matrixData.Count + " 4x4 matrices = " + (matrixData.Count * 4) + " pixel, Need Scale? " +
				          anyScaled);
			var bonesBuffer = new ComputeBuffer(matrixData.Count, Bone.Stride);
			bonesBuffer.SetData(matrixData);
			return bonesBuffer;
		}

		public static BakedMeshSkinningData BakeSkinning(Mesh mesh, ComputeShader shader)
		{
			var res = new BakedMeshSkinningData();
			res.Mesh = mesh;

			var kernel = shader.FindKernel("BakeBoneWeights");
			using (var boneWeights = CreateVertexBoneWeightBuffer(mesh))
			{
				var buffer = boneWeights;
				var textureSize = ToTextureSize(buffer.count * 2);
				var texture = new RenderTexture(textureSize.x, textureSize.y, 0, RenderTextureFormat.ARGBHalf);
				texture.name = "skinning";
				texture.enableRandomWrite = true;
				texture.useMipMap = false;
				texture.filterMode = FilterMode.Point;
				texture.Create();
				res.Texture = texture;

				if (DebugLog)
					Debug.Log("Bake skinning for " + mesh.vertexCount + " vertices, weights: " + buffer.count + ", texture: " + texture.width + "x" +
					          texture.height + " texture. " + texture.format);

				shader.SetBuffer(kernel, "Weights", buffer);
				shader.SetTexture(kernel, "Texture", texture);
				shader.Dispatch(kernel, Mathf.CeilToInt(buffer.count / 32f), 1, 1);
			}

			return res;
		}

		[Serializable]
		public struct Bone
		{
			public Matrix4x4 Transformation;

			public Bone(Matrix4x4 mat) => Transformation = mat;

			public static int Stride => sizeof(float) * 4 * 4;
		}

		public static ComputeBuffer ReadAnimation(BakedAnimationData data, ComputeShader shader)
		{
			var kernel = shader.FindKernel("ReadAnimation");
			var res = new ComputeBuffer(data.TotalLength, Bone.Stride);
			using (var clipsBuffer = new ComputeBuffer(data.ClipsInfos.Count, TextureClipInfo.Stride))
			{
				clipsBuffer.SetData(data.ClipsInfos);
				shader.SetBuffer(kernel, "Clips", clipsBuffer);
				shader.SetTexture(kernel, "Texture", data.Texture);
				shader.SetBuffer(kernel, "Bones", res);
				var tx = Mathf.CeilToInt(data.ClipsInfos.Count / 32f);
				shader.Dispatch(kernel, tx, 1, 1);
			}

			return res;
		}

		private static Vector2Int ToTextureSize(int requiredPixelCount)
		{
			var px = 4;
			var py = 4;
			while ((px * py) < requiredPixelCount)
			{
				px *= 2;
				if ((px * py) >= requiredPixelCount) break;
				py *= 2;
			}

			return new Vector2Int(px, py);
		}

		private static ComputeBuffer CreateVertexBoneWeightBuffer(Mesh mesh)
		{
			var boneWeights = mesh.boneWeights;
			var weightBuffer = new ComputeBuffer(boneWeights.Length, sizeof(float) * 4 + sizeof(int) * 4);
			weightBuffer.SetData(boneWeights);
			return weightBuffer;
		}
	}
}