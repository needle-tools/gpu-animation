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
		[SerializeField]
		private RenderTexture idMap;
		private RenderTexture debugOutput;
		
		[SerializeField, HideInInspector]
		private CameraEvent evt, prevEvt;
		private CommandBuffer commandBuffer;

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
			GL.Clear(true, true, Color.black);
			RenderTexture.active = null;

			if (mainCam.renderingPath == RenderingPath.Forward)
				evt = CameraEvent.BeforeForwardOpaque;
			else if (mainCam.renderingPath == RenderingPath.DeferredShading)
				evt = CameraEvent.BeforeGBuffer;

			if (commandBuffer == null || evt != prevEvt)
			{
				if (idMap)
				{
					idMap.Release();
					idMap = null;
				}

				const int size = 100;
				idMap = new RenderTexture(size, size, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Default);
				idMap.name = "Id Tex";
				idMap.filterMode = FilterMode.Point;
				idMap.Create();

				RemoveCommandBuffer();
				prevEvt = evt;
				commandBuffer = new CommandBuffer();
				commandBuffer.name = "Render Id Texture";
				// commandBuffer.ClearRandomWriteTargets();
				
				var rt = new RenderTargetIdentifier[]
				{
					BuiltinRenderTextureType.CurrentActive,
					idMap
				};
				commandBuffer.SetRenderTarget(rt, BuiltinRenderTextureType.Depth);
				mainCam.AddCommandBuffer(prevEvt, commandBuffer);
			}
		}

		private void RemoveCommandBuffer()
		{
			mainCam.RemoveCommandBuffers(evt);
			mainCam.RemoveCommandBuffers(prevEvt);
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