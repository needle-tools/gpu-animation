using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Elaborate.AnimationBakery
{
	public static class AnimationTextureProvider
	{
		public static AnimationTextureData BakeAnimation(
			ComputeShader shader, ComputeBuffer animationBuffer, List<AnimationTextureData.Clip> clipInfos)
		{
			var textureSize = ToTextureSize(animationBuffer.count*4);
			Debug.Log("Bake into " + textureSize + "x" + textureSize + " Texture");
			var texture = new RenderTexture(textureSize.x, textureSize.y, 0, RenderTextureFormat.ARGBFloat); // TODO: try ARGBHalf
			texture.enableRandomWrite = true;
			texture.useMipMap = false;
			texture.filterMode = FilterMode.Point;
			texture.Create();
			var res = new AnimationTextureData();
			res.Texture = texture;
			res.Animations = clipInfos;
			
			var kernel = shader.FindKernel("BakeAnimationTexture_Float4");
			shader.SetBuffer(kernel, "Matrices", animationBuffer);
			shader.SetTexture(kernel, "Texture", texture);
			shader.Dispatch(kernel, Mathf.CeilToInt(animationBuffer.count / 32f), 1, 1);
			return res;
		}
		
		public static ComputeBuffer GetBuffer(IEnumerable<AnimationTransformationData> animationData, out List<AnimationTextureData.Clip> clipInfos)
		{
			var matrixData = new List<Bone>();
			clipInfos = new List<AnimationTextureData.Clip>();
			var anyScaled = false;
			foreach (var anim in animationData)
			{
				var clip = new AnimationTextureData.Clip();
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

				clip.TotalLength = matrixData.Count - clip.IndexStart;
				Debug.Log("Clip \t" + clipInfos.Count + "\t" + clip);
				clipInfos.Add(clip);
			}
			Debug.Log(clipInfos.Count + " clip(s), data is " + matrixData.Count + " 4x4 matrices = " + (matrixData.Count * 4) + " pixel, Need Scale? " + anyScaled);
			var bonesBuffer = new ComputeBuffer(matrixData.Count, Bone.Stride);
			bonesBuffer.SetData(matrixData);
			return bonesBuffer;
		}

		public static MeshSkinningData BakeSkinning(Mesh mesh, ComputeShader shader, out ComputeBuffer boneWeights)
		{
			var res = new MeshSkinningData();
			res.Mesh = mesh;

			var kernel = shader.FindKernel("BakeBoneWeights");
			boneWeights =  CreateVertexBoneWeightBuffer(mesh);
			var buffer = boneWeights;
			// using ()
			{
				var textureSize = ToTextureSize(buffer.count*2);
				var texture = new RenderTexture(textureSize.x, textureSize.y, 0, RenderTextureFormat.ARGBHalf);
				texture.enableRandomWrite = true;
				texture.useMipMap = false;
				texture.filterMode = FilterMode.Point;
				texture.Create();
				res.Texture = texture;

				Debug.Log("Bake skinning for " + mesh.vertexCount + " vertices, weights: " + buffer.count + ", texture: " + texture.width + "x" + texture.height + " texture. " + texture.format);

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

		public static ComputeBuffer ReadAnimation(AnimationTextureData data, ComputeShader shader)
		{
			var kernel = shader.FindKernel("ReadAnimation");
			var res = new ComputeBuffer(data.TotalFrames, Bone.Stride);
			using (var clipsBuffer = new ComputeBuffer(data.Animations.Count, AnimationTextureData.Clip.Stride))
			{
				clipsBuffer.SetData(data.Animations);
				shader.SetBuffer(kernel, "Clips", clipsBuffer);
				shader.SetTexture(kernel, "Texture", data.Texture);
				shader.SetBuffer(kernel, "Bones", res);
				var tx = Mathf.CeilToInt(data.Animations.Count / 32f);
				shader.Dispatch(kernel, tx, 1, 1);
			}

			return res;
		}

		private static Vector2Int ToTextureSize(int pixel)
		{
			var px = 4;
			var py = 4;
			while ((px*py) < pixel)
			{
				px *= 2;
				if ((px * py) >= pixel) break;
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