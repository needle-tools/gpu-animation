using UnityEngine;
using UnityEngine.Rendering;

namespace Experimental
{
	[ExecuteAlways]
	public class RenderWithIndexTexture : MonoBehaviour
	{
		public Camera Cam;
		public Mesh Mesh;
		public Material Material;
		public Material Output;
		public ComputeShader Shader;

		private RenderTexture rt;
		private RenderTexture temp;
		private CommandBuffer cmd;

		private void OnEnable()
		{
			Camera.onPreCull += OnBeforeRender;
			Camera.onPostRender += OnAfterRender;
			run = true;
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
		
		private void OnBeforeRender(Camera cam)
		{
			Execute(cam);
		}

		public bool run = true;

		private void Execute(Camera cam)
		{
			if (Cam != cam) return;
			Sample();
		}

		private void Sample()
		{
			if (!run) return;

			RenderTexture.active = rt;
			GL.Clear(false, true, Color.black);
			RenderTexture.active = null;

			if (cmd == null)
			{
				if (rt)
				{
					rt.Release();
					rt = null;
				}
				const int size = 100;
				rt = new RenderTexture(size, size, 0, RenderTextureFormat.RInt, RenderTextureReadWrite.Linear);
				rt.enableRandomWrite = true;
				rt.name = "Id Tex";
				rt.filterMode = FilterMode.Point;
				rt.Create();
				
				Cam.RemoveCommandBuffers(CameraEvent.BeforeForwardOpaque);
				cmd = new CommandBuffer();
				cmd.name = "Render Id Texture";
				cmd.ClearRandomWriteTargets();
				cmd.SetRandomWriteTarget(4, rt);
				Cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cmd);
			}
		}

		private void OnAfterRender(Camera cam)
		{
			if (!run) return;
			if (cam != Cam) return;
			
			// important:
			Graphics.ClearRandomWriteTargets();
			
			if (run && rt)
			{
				Shader.SetTexture(0, "IdMap", rt);
				Shader.SetVector("SampleUV", Input.mousePosition / new Vector2(Screen.width, Screen.height));
				using (var buf = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Structured))
				{
					Shader.SetBuffer(0, "Result", buf);
					Shader.Dispatch(0, 1, 1, 1);
					var data = new int[1];
					buf.GetData(data);
					Debug.Log(data[0]);
				}
				
				if (!temp)
				{
					temp = new RenderTexture(rt);
					temp.format = RenderTextureFormat.RInt;
					temp.filterMode = FilterMode.Point;
					temp.Create();
				}
				Graphics.Blit(rt, temp);
				Output.mainTexture = temp;
			}
		}
	}
}