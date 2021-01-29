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
			rt.Release();
		}

		// private void OnRenderImage(RenderTexture src, RenderTexture dest)
		// {
		// 	Execute();
		// 	Graphics.Blit(src, dest);
		// }

		private void OnBeforeRender(Camera cam)
		{
			Debug.Log("Render " + cam); // + " - " + Time.frameCount, cam); 
			Execute(cam);
		}

		public bool once = true;

		private void Execute(Camera cam)
		{
			if (Cam != cam) return;
			
			if (!rt)
			{
				rt = new RenderTexture(5, 5, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
				rt.enableRandomWrite = true;
				rt.name = "MyTex";
				rt.filterMode = FilterMode.Point;
				rt.Create();
				Shader.SetGlobalTexture("_MyTex", rt);
				Graphics.ClearRandomWriteTargets();
				Graphics.SetRandomWriteTarget(4, rt);
				;
			}

			Output.mainTexture = rt;
			Material.SetTexture("_MyTex", rt);

			Graphics.SetRandomWriteTarget(4, rt);
			// Material.enableInstancing = true;
			// Graphics.DrawMeshInstanced(Mesh, 0, Material, new []{transform.localToWorldMatrix});
			Graphics.ClearRandomWriteTargets();

			Cam.RemoveCommandBuffers(CameraEvent.BeforeForwardOpaque);
			if (once)
			{
				if (cam == Camera.main)
					once = false;
				var cmd = new CommandBuffer();
				cmd.ClearRandomWriteTargets();
				cmd.SetRandomWriteTarget(4, new RenderTargetIdentifier(rt));
				Cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cmd);
			}
		}

		private void OnAfterRender(Camera cam)
		{
			if (cam != Cam) return;
			Cam.RemoveCommandBuffers(CameraEvent.BeforeForwardOpaque);
			once = false;
		}

		// ()
		// {
		// }
	}
}