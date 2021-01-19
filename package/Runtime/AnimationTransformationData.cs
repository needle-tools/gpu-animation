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
	}

	[Serializable]
	public class BoneTransformationData
	{
		public int BoneIndex;
		public List<BoneTransformation> Transformations;

		public BoneTransformationData(int index, List<BoneTransformation> transformations)
		{
			this.BoneIndex = index;
			this.Transformations = transformations;
		}
	}

	[Serializable]
	public class BoneTransformation
	{
		public float Time;
		public Matrix4x4 Matrix;
		public bool NeedsScale = false;

		public BoneTransformation(float time, Matrix4x4 mat)
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