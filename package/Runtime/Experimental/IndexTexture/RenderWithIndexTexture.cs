using System;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Experimental
{
	[ExecuteAlways]
	public class RenderWithIndexTexture : MonoBehaviour
	{
		public Camera Cam;
		public Mesh Mesh;
		public Material Material;
		public Material Output;

		private RenderTexture rt;
		private RenderTexture temp;

		private void OnEnable()
		{
			Camera.onPreCull += OnBeforeRender;
			Camera.onPostRender += OnAfterRender;
			once = true;
		}

		private void OnDisable()
		{
			Camera.onPreCull -= OnBeforeRender;
			Camera.onPostRender -= OnAfterRender;
			if (rt)
			{
				rt.Release();
				rt = null;
			}
		}

		private void Update()
		{
			if (Input.GetMouseButtonUp(0))
			{
				once = true;
			}
		}

		private void OnBeforeRender(Camera cam)
		{
			Execute(cam);
		}

		public bool once = true;

		private void Execute(Camera cam)
		{
			if (Cam != cam) return;

			Graphics.ClearRandomWriteTargets();
			if (once)
			{
			
				// if (!rt)
				{
					if (rt)
					{
						rt.Release();
						rt = null;
					}
					int size = 100;
					rt = new RenderTexture(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
					rt.enableRandomWrite = true;
					rt.name = "Id Tex";
					rt.filterMode = FilterMode.Point;
					rt.Create();
					// Shader.SetGlobalTexture("_MyTex", rt);
					// Graphics.ClearRandomWriteTargets();
					// Graphics.SetRandomWriteTarget(4, rt);
					// Output.mainTexture = rt;
					// Material.SetTexture("_MyTex", rt);
				}
				
				Cam.RemoveCommandBuffers(CameraEvent.BeforeForwardOpaque);
				var cmd = new CommandBuffer();
				cmd.ClearRandomWriteTargets();
				cmd.SetRandomWriteTarget(4, rt);
				Cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cmd);
			}
		}

		private void OnAfterRender(Camera cam)
		{
			if (cam != Cam) return;
			
			once = false;
			Cam.RemoveCommandBuffers(CameraEvent.BeforeForwardOpaque);

			if (rt)
			{
				if (!temp)
				{
					temp = new RenderTexture(rt);
					temp.filterMode = FilterMode.Point;
					temp.Create();
				}
				Graphics.Blit(rt, temp);
				Output.mainTexture = temp;
			}
		}
	}
}