using System;
using System.Collections.Generic;
using UnityEngine;

namespace Elaborate.AnimationBakery
{
	[Serializable]
	public class AnimationTransformationData
	{
		public AnimationClip Clip;
		public BoneTransformationData[] BoneData;

		public AnimationTransformationData(AnimationClip clip, BoneTransformationData[] boneData)
		{
			this.Clip = clip;
			this.BoneData = boneData;
		}
	}

	[Serializable]
	public class BoneTransformationData
	{
		public string Name;
		public Transform Bone;
		
		public int BoneIndex;
		public List<BoneTransformation> Transformations;

		public BoneTransformationData(string name, Transform bone, int index, List<BoneTransformation> transformations)
		{
			this.Name = name;
			this.Bone = bone;
			this.BoneIndex = index;
			this.Transformations = transformations;
		}
	}

	[Serializable]
	public class BoneTransformation
	{
		public float Time;
		public Matrix4x4 Matrix;
		public bool Scaled;

		public BoneTransformation(float time, Matrix4x4 mat, bool scaled)
		{
			this.Time = time;
			this.Matrix = mat;
		}
	}
	
	[System.Serializable]
	public struct SkinnedMesh_BoneData
	{
		public Transform Bone;

		/// <summary>
		/// index of bone in skinned mesh renderer
		/// </summary>
		public int Index;
	}
}