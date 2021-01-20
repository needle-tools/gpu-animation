﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Elaborate.AnimationBakery
{
	[ExecuteInEditMode]
	public class AnimationBaker : MonoBehaviour
	{
		public Animator Animator;
		public SkinnedMeshRenderer Renderer;
		public List<AnimationClip> Clips;
		public ComputeShader TextureBakingShader;
		public int Skip;
		public float FrameRate = -1;

		[Header("Output")] public List<AnimationTransformationData> AnimationData;
		public MeshSkinningData SkinBake;
		public AnimationTextureData AnimationBake;

		[Header("Test")] public Material VertexAnim;
		public Material Test;
		public Material Test2;


#if UNITY_EDITOR

		private void OnEnable()
		{
			Bake();
		}

		private void Update()
		{
			if (!Application.isPlaying) 
				Bake();
		}

		private ComputeBuffer animBuffer, weightBuffer;

		public AnimationTextureProvider.Bone[] Bones;
		public BoneWeight[] BoneWeights;

		[ContextMenu(nameof(Bake))]
		public void Bake()
		{
			AnimationData = AnimationDataProvider.GetAnimations(Animator, Clips, Renderer, Skip, FrameRate);
			SkinBake = AnimationTextureProvider.BakeSkinning(Renderer.sharedMesh, TextureBakingShader, out weightBuffer);
			AnimationBake = AnimationTextureProvider.BakeAnimation(AnimationData, TextureBakingShader);

			if (VertexAnim)
			{
				VertexAnim.SetTexture("_Animation", AnimationBake.Texture);
				VertexAnim.SetTexture("_Skinning", SkinBake.Texture);
				VertexAnim.SetVector("_Skinning_TexelSize", new Vector4(0, 0, SkinBake.Texture.width, SkinBake.Texture.height));
				VertexAnim.SetBuffer("_BoneWeights", weightBuffer);
				BoneWeights = new BoneWeight[weightBuffer.count];
				weightBuffer.GetData(BoneWeights);
				this.animBuffer = AnimationTextureProvider.ReadAnimation(AnimationBake, TextureBakingShader);
				Bones = new AnimationTextureProvider.Bone[animBuffer.count];
				this.animBuffer.GetData(Bones);
				VertexAnim.SetBuffer("_Animations", animBuffer);
			}

			if (Test)
			{
				Test.mainTexture = AnimationBake.Texture;
			}

			if (Test2)
			{
				Test2.mainTexture = SkinBake.Texture;
			}
			
			// ReadPixels(SkinBake.Texture);
		}
#endif
	}
}