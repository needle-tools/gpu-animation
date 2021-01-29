using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Experimental
{
	[ExecuteInEditMode]
	public class DrawMRTK : MonoBehaviour
	{
		public Mesh Mesh;
		public Material Material;
		public RenderTexture RT;
		public Material Display;

		private void OnEnable()
		{
			Camera.onPreCull += OnRender;
			RT.Create();
		}

		private void OnDisable()
		{
			Camera.onPreCull -= OnRender;
		}

		private void OnRender(Camera cam)
		{
			var crt = cam.activeTexture;
			if (!crt) crt = RenderTexture.GetTemporary(UnityEngine.Display.main.renderingWidth,UnityEngine.Display.main.renderingHeight, 16);

			if (!RT)
			{
				RT = new RenderTexture(crt);
				RT.Create();
			}
			
			var rt = new RenderBuffer[]
			{
				// crt.colorBuffer,
				RT.colorBuffer
			};
			var col = crt.colorBuffer;// RT.colorBuffer;
			// GL.sRGBWrite = true;
			// Graphics.SetRenderTarget(rt, crt.depthBuffer);      
			
			cam.SetTargetBuffers(col, crt.depthBuffer);
			Graphics.DrawMesh(Mesh, transform.localToWorldMatrix, Material, 0, cam);

			if (Display) Display.mainTexture = RT;
		}
	}
}