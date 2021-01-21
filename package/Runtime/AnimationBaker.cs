using System;
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

		[Header("Test")] public bool TextureToStructuredBuffer = true;
		public Material VertexAnim;
		public Material Test;
		public Material Test2;


		public int Frame;
		
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

		public AnimationTextureProvider.Bone[] Bones, Bones2;
		public BoneWeight[] BoneWeights;

		[ContextMenu(nameof(Bake))]
		public void Bake()
		{
			AnimationData = AnimationDataProvider.GetAnimations(Animator, Clips, Renderer, Skip, FrameRate);
			SkinBake = AnimationTextureProvider.BakeSkinning(Renderer.sharedMesh, TextureBakingShader, out weightBuffer);
			AnimationBake = AnimationTextureProvider.BakeAnimation(AnimationData, TextureBakingShader, out animBuffer);

			if (VertexAnim)
			{
				VertexAnim.SetTexture("_Animation", AnimationBake.Texture);
				VertexAnim.SetTexture("_Skinning", SkinBake.Texture);
				VertexAnim.SetBuffer("_BoneWeights", weightBuffer);
				BoneWeights = new BoneWeight[weightBuffer.count];
				weightBuffer.GetData(BoneWeights);
				Bones = new AnimationTextureProvider.Bone[animBuffer.count];
				this.animBuffer.GetData(Bones);
				if (TextureToStructuredBuffer)
				{
					this.animBuffer = AnimationTextureProvider.ReadAnimation(AnimationBake, TextureBakingShader);
					Bones2 = new AnimationTextureProvider.Bone[animBuffer.count];
					this.animBuffer.GetData(Bones2);
				}
				VertexAnim.SetBuffer("_Animations", animBuffer);
				VertexAnim.SetInt("_BonesCount", Renderer.bones.Length);
				VertexAnim.SetVector("_CurrentAnimation", new Vector4(AnimationBake.Animations[0].IndexStart, AnimationBake.Animations[0].Frames, Frame, 0));
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