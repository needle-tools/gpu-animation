using UnityEngine;
using UnityEngine.Rendering;

namespace Experimental
{
	[ExecuteAlways]
	public class IndexTextureUtility : MonoBehaviour
	{
		public Material DebugOutput;
		public ComputeShader Shader;
		public int RWIndex = 4;

		private Camera mainCam;
		private CommandBuffer commandBuffer;
		private RenderTexture idMap;
		private RenderTexture debugOutput;

		private void OnEnable()
		{
			mainCam = Camera.main;
			Camera.onPreCull += OnBeforeRender;
			Camera.onPostRender += OnAfterRender;
		}

		private void OnDisable()
		{
			Camera.onPreCull -= OnBeforeRender;
			Camera.onPostRender -= OnAfterRender;
			
			if (idMap)
			{
				idMap.Release();
				idMap = null;
			}

			if (debugOutput)
			{
				debugOutput.Release();
				debugOutput = null;
			}
		}
		
		private void OnBeforeRender(Camera cam)
		{
			if (mainCam != cam) return;
			Execute();
		}

		private void Execute()
		{
			RenderTexture.active = idMap;
			GL.Clear(false, true, Color.black);
			RenderTexture.active = null;

			if (commandBuffer == null)
			{
				if (idMap)
				{
					idMap.Release();
					idMap = null;
				}
				const int size = 100;
				idMap = new RenderTexture(size, size, 0, RenderTextureFormat.RInt, RenderTextureReadWrite.Linear);
				idMap.enableRandomWrite = true;
				idMap.name = "Id Tex";
				idMap.filterMode = FilterMode.Point;
				idMap.Create();
				
				mainCam.RemoveCommandBuffers(CameraEvent.BeforeForwardOpaque);
				commandBuffer = new CommandBuffer();
				commandBuffer.name = "Render Id Texture";
				commandBuffer.ClearRandomWriteTargets();
				commandBuffer.SetRandomWriteTarget(RWIndex, idMap);
				mainCam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
			}
		}

		private void OnAfterRender(Camera cam)
		{
			if (cam != mainCam) return;
			// important:
			Graphics.ClearRandomWriteTargets();

			if (!idMap) return;
			
			Shader.SetTexture(0, "IdMap", idMap);
			Shader.SetVector("SampleUV", Input.mousePosition / new Vector2(Screen.width, Screen.height));
			using (var buf = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Structured))
			{
				Shader.SetBuffer(0, "Result", buf);
				Shader.Dispatch(0, 1, 1, 1);
				var data = new int[1];
				buf.GetData(data);
				Debug.Log(data[0]);
			}

			if (DebugOutput)
			{
				if (!debugOutput)
				{
					debugOutput = new RenderTexture(idMap);
					debugOutput.format = RenderTextureFormat.RInt;
					debugOutput.filterMode = FilterMode.Point;
					debugOutput.Create();
				}
				Graphics.Blit(idMap, debugOutput);
				DebugOutput.mainTexture = debugOutput;
			}
		}
	}
}