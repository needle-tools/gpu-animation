using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace needle.GpuAnimation
{
	[CustomEditor(typeof(BakedAnimation), true)]
	public class BakedAnimationEditor : Editor
	{
		private void OnDisable()
		{
			renderUtility?.Cleanup();
			renderUtility = null;
		}

		private bool ShowInternalData
		{
			get => SessionState.GetBool("_ShowBakedAnimationInternalData", false);
			set => SessionState.SetBool("_ShowBakedAnimationInternalData", value);
		}

		private bool ShowPreview
		{
			get => SessionState.GetBool("_ShowBakedAnimationPreview", true);
			set => SessionState.SetBool("_ShowBakedAnimationPreview", value);
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			ShowInternalData = EditorGUILayout.Foldout(ShowInternalData, "Internal Data");
			if (ShowInternalData)
				DrawInternalData();

			EditorGUILayout.Space(5);
			ShowPreview = EditorGUILayout.Foldout(ShowPreview, "Preview");
			if (ShowPreview)
				DrawPreview();
		}

		private void DrawInternalData()
		{
			var baked = target as BakedAnimation;
			if (!baked || !baked.HasBakedAnimation) return;
			foreach (var bake in baked.Models)
			{
				EditorGUILayout.ObjectField("Skinning", bake.Skinning.Texture, bake.Skinning.Texture.GetType(), false);
				EditorGUILayout.ObjectField("Animation", bake.Animations.Texture, bake.Animations.Texture.GetType(), false);
			}
		}

		private void DrawPreview()
		{
			var baked = target as BakedAnimation;
			if (!baked || !baked.HasBakedAnimation) return;

			time += 1f / 30;
			if (Event.current.type == EventType.MouseDrag)
			{
				angle += Event.current.delta.x * 2f;
			}

			var rect = GUILayoutUtility.GetLastRect();
			rect.y += EditorGUIUtility.singleLineHeight * 1;

			const int row = 3;
			var width = Screen.width / (float) row - rect.x * 1.5f;
			var spacing = 5;
			var start = rect;
			GUILayout.Space(width);
			foreach (var bake in baked.Models)
			{
				for (var i = 0; i < bake.Animations.ClipsInfos.Count; i++)
				{
					rect = new Rect(rect.x, rect.y, width, width);
					DrawAnimationPreview(bake, i, rect, block, time, angle);
					rect.x += width + spacing;
					if (rect.x + width > Screen.width)
					{
						rect.x = start.x;
						rect.y += width + spacing;
						GUILayout.Space(width * .35f);
					}
				}
			}
		}

		private static Material _previewMaterial;

		private static Material PreviewMaterial
		{
			get
			{
				if (!_previewMaterial) _previewMaterial = PreviewUtility.CreateNewPreviewMaterial();
				return _previewMaterial;
			}
		}

		private static PreviewRenderUtility renderUtility;

		private static readonly int Animation1 = Shader.PropertyToID("_Animation");
		private static readonly int Skinning = Shader.PropertyToID("_Skinning");
		private static readonly int CurrentAnimation = Shader.PropertyToID("_CurrentAnimation");
		private static readonly int Time = Shader.PropertyToID("_Time");

		private MaterialPropertyBlock block;
		private float angle;
		private float time;

		private static void DrawAnimationPreview(BakedModel baked, int i, Rect rect, MaterialPropertyBlock block, float time, float angle)
		{
			if (baked == null) return;
			if (!baked.IsValid) return;
			if (i < 0 || i >= baked.Animations.ClipsInfos.Count) return;
			var clip = baked.Animations.ClipsInfos[i];

			if (renderUtility == null)
				renderUtility = new PreviewRenderUtility();

			renderUtility.BeginPreview(rect, GUIStyle.none);

			if (block == null) block = new MaterialPropertyBlock();
			block.SetTexture(Animation1, baked.Animations.Texture);
			block.SetTexture(Skinning, baked.Skinning.Texture);
			block.SetVector(CurrentAnimation, clip.AsVector4);
			block.SetVector(Time, new Vector4(0, time, 0, 0));

			var bounds = baked.Skinning.Mesh.bounds;
			var cam = renderUtility.camera;
			cam.nearClipPlane = .01f;
			cam.farClipPlane = bounds.size.magnitude * 2f;
			cam.fieldOfView = 60;
			var previewCameraTransform = renderUtility.camera.transform;
			var pos = bounds.size * 1.3f - (Vector3.up * (bounds.max.y * .2f)) + Vector3.right;
			var lookTarget = bounds.center + Vector3.up * .2f;
			pos = Quaternion.Euler(0, angle, 0) * pos;
			var dir = lookTarget - pos;
			if (dir == Vector3.zero) dir = Vector3.forward;
			var camRot = Quaternion.LookRotation(dir, Vector3.up);
			previewCameraTransform.position = pos;
			previewCameraTransform.rotation = camRot;
			var l0 = renderUtility.lights[0];
			l0.transform.rotation = camRot;


			renderUtility.DrawMesh(baked.Skinning.Mesh, Matrix4x4.identity, PreviewMaterial, 0, block);
			renderUtility.Render(true, false);
			var tex = renderUtility.EndPreview();
			EditorGUI.DrawTextureTransparent(rect, tex);
		}

		[CustomPreview(typeof(BakedAnimation))]
		public class BakedAnimationPreview : ObjectPreview
		{
			public override void Initialize(Object[] targets)
			{
				base.Initialize(targets);
				block = new MaterialPropertyBlock();
			}

			public override bool HasPreviewGUI()
			{
				return true;
			}

			private float angle, time;
			private int index;

			public override void OnPreviewSettings()
			{
				base.OnPreviewSettings();
				var t = (BakedAnimation) target;
				EditorGUI.BeginDisabledGroup(!t.HasBakedAnimation);
				var rect = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, 150, 18f, 18f);
				index = EditorGUI.IntSlider(rect, index, 0, t.ClipsCount - 1);
				// index = EditorGUI.IntField(rect, index);
				// index %= t.ClipsCount;
				EditorGUI.EndDisabledGroup();
			}

			private static MaterialPropertyBlock block;


			public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
			{
				base.OnInteractivePreviewGUI(r, background);

				if (Event.current.type == EventType.MouseDrag)
				{
					angle += Event.current.delta.x * 2f;
				}

				if (Event.current.type == EventType.Layout)
					time += 1f / 30;

				var baked = target as BakedAnimation;
				if (baked)
				{
					foreach (var bake in baked.Models)
					{
						DrawAnimationPreview(bake, index, r, block, time, angle);
					}
				}
			}
		}


#if UNITY_2020_2_OR_NEWER
		// ReSharper disable once Unity.IncorrectMethodSignature
		// ReSharper disable once UnusedMember.Global
		public void OnSceneDrag(SceneView sceneView, int index)
#else
		// ReSharper disable once Unity.IncorrectMethodSignature
		// ReSharper disable once UnusedMember.Global
		public void OnSceneDrag(SceneView sceneView)
#endif
		{
			var bake = target as BakedAnimation;
			if (!bake) return;
			// if (bake.HasBakedAnimation)
			DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

			var evt = Event.current;
			switch (evt.type)
			{
				case EventType.DragPerform:
					var go = new GameObject();
					go.name = bake.name;
					var prev = go.AddComponent<PreviewBakedAnimation>();
					prev.Animation = bake;
					prev.PreviewMaterial = new Material(PreviewMaterial);
					prev.Offset = Vector3.back;
					Selection.activeObject = go;
					break;
			}
		}
	}
}