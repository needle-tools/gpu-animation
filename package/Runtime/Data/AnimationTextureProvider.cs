using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Elaborate.AnimationBakery
{

	[Serializable]
	public class AnimationTextureData
	{
		public Texture Texture;
		public List<Clip> Animations;
		
		
		[Serializable]
		public struct Clip
		{
			public int IndexStart;
			public int Length;
			public int Stride => sizeof(int) * 2;

			public override string ToString()
			{
				return "Start=" + IndexStart + ", Length=" + Length;
			}
		}
	}
	
	public static class AnimationTextureProvider
	{
		public static AnimationTextureData BakeToTexture(List<AnimationTransformationData> animationData, ComputeShader shader)
		{
			var matrixData = new List<Matrix4x4>();
			var clipInfos = new List<AnimationTextureData.Clip>();
			foreach (var anim in animationData)
			{
				var clip = new AnimationTextureData.Clip();
				clip.IndexStart = matrixData.Count;
				foreach (var boneData in anim.BoneData)
				{
					foreach (var frame in boneData.Transformations)
					{
						Debug.Log(frame.Matrix);
						matrixData.Add(frame.Matrix);
					}
				}
				clip.Length = matrixData.Count - clip.IndexStart;
				Debug.Log("Clip \t" + clipInfos.Count + "\t" + clip);
				clipInfos.Add(clip);
			}

			var squared = Mathf.CeilToInt(Mathf.Sqrt(matrixData.Count * 4));
			var texture = new RenderTexture(squared, squared, 0, RenderTextureFormat.ARGBFloat);
			texture.enableRandomWrite = true;
			texture.useMipMap = false;
			texture.filterMode = FilterMode.Point;
			texture.Create();
			var res = new AnimationTextureData();
			res.Texture = texture;
			res.Animations = clipInfos;
			
			Debug.Log(clipInfos.Count + " clip(s), data is " + matrixData.Count + " matrices: 4x4 = " + (matrixData.Count * 4) + " pixel. TextureSize: " + texture.width + "x" + texture.height);
			using (var matrixBuffer = new ComputeBuffer(matrixData.Count, sizeof(float) * 4 * 4))
			{
				matrixBuffer.SetData(matrixData);
				shader.SetBuffer(0, "Matrices", matrixBuffer);
				shader.SetTexture(0, "Texture", texture);
				shader.Dispatch(0, Mathf.CeilToInt(matrixData.Count / 32f), 1, 1);
			}
			
			return res;
		}
	}
}