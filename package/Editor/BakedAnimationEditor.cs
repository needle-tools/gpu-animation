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

		private bool ShowPreview
		{
			get => SessionState.GetBool("_ShowBakedAnimationPreview", true);
			set => SessionState.SetBool("_ShowBakedAnimationPreview", value);
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			EditorGUILayout.Space(5);
			ShowPreview = EditorGUILayout.Foldout(ShowPreview, "Preview");
			if (ShowPreview)
				DrawPreview();
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
			for (var i = 0; i < baked.AnimationBake.ClipsInfos.Count; i++)
			{
				rect = new Rect(rect.x, rect.y, width, width);
				DrawAnimationPreview(baked, i, rect, block, time, angle);
				rect.x += width + spacing;
				if (rect.x + width > Screen.width)
				{
					rect.x = start.x;
					rect.y += width + spacing;
					GUILayout.Space(width * .35f);
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

		private static void DrawAnimationPreview(BakedAnimation baked, int i, Rect rect, MaterialPropertyBlock block, float time, float angle)
		{
			if (!baked) return;
			if (!baked.HasBakedAnimation) return;
			var clip = baked.AnimationBake.ClipsInfos[i];
			
			if (renderUtility == null)
				renderUtility = new PreviewRenderUtility();
			
			renderUtility.BeginPreview(rect, GUIStyle.none);

			if (block == null) block = new MaterialPropertyBlock();
			block.SetTexture(Animation1, baked.AnimationBake.Texture);
			block.SetTexture(Skinning, baked.SkinBake.Texture);
			block.SetVector(CurrentAnimation, clip.AsVector4);
			block.SetVector(Time, new Vector4(0, time, 0, 0));

			var bounds = baked.SkinBake.Mesh.bounds;
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


			renderUtility.DrawMesh(baked.SkinBake.Mesh, Matrix4x4.identity, PreviewMaterial, 0, block);
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

				DrawAnimationPreview(target as BakedAnimation, index, r, block, time, angle);
			}
		}


		// ReSharper disable once Unity.IncorrectMethodSignature
		public void OnSceneDrag(SceneView sceneView)
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
					if (bake.HasBakedAnimation)
						prev.Offset = bake.SkinBake.Mesh.bounds.extents.z * Vector3.back * 2.5f;
					else
						prev.Offset = Vector3.back;
					Selection.activeObject = go;
					break;
			}
		}

		// private void OnSceneDrag(SceneView sceneView, int index)
		// {
		//
		// 	// if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
		// 	// {
		// 	// 	DragAndDrop.visualMode = DragAndDropVisualMode.Copy; // show a drag-add icon on the mouse cursor
		// 	//
		// 	// 	if (draggedObj == null)
		// 	// 		draggedObj = (GameObject)Object.Instantiate(DragAndDrop.objectReferences[0]);
		// 	//
		// 	// 	// compute mouse position on the world y=0 plane
		// 	// 	Ray mouseRay = Camera.current.ScreenPointToRay(new Vector3(Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y, 0.0f));
		// 	// 	if (mouseRay.direction.y &lt; 0.0f)
		// 	// 	{
		// 	// 		float t = -mouseRay.origin.y / mouseRay.direction.y;
		// 	// 		Vector3 mouseWorldPos = mouseRay.origin + t * mouseRay.direction;
		// 	// 		mouseWorldPos.y = 0.0f;
		// 	//
		// 	// 		draggedObj.transform.position = terrain.SnapToNearestTileCenter(mouseWorldPos);
		// 	// 	}
		// 	//
		// 	// 	if (Event.current.type == EventType.DragPerform)
		// 	// 	{
		// 	// 		DragAndDrop.AcceptDrag();
		// 	// 		draggedObj = null;
		// 	// 	}
		// 	//
		// 	// 	Event.current.Use();
		// 	// }
		// }
	}
}