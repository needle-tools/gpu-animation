using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace needle.GpuAnimation
{
	public class StressTestBakedAnimation : PreviewBakedAnimationBase
	{
		public Vector2Int Count = new Vector2Int(10, 10);
		public Vector2 Offset = new Vector2(1, 1);
		public bool UseInstancedIndirect = true;

		private readonly Dictionary<string, (ComputeBuffer buffer, Matrix4x4[] matrices)> buffers = new Dictionary<string, (ComputeBuffer, Matrix4x4[])>();
		private static readonly int PositionBuffer = Shader.PropertyToID("positions");
		private int previousCount;
		private ComputeBuffer argsBuffer;
		private uint[] args;

		[Header("Internal")] public int CurrentCount;


		private void OnValidate()
		{
#if UNITY_WEBGL
			if (!UseInstancedIndirect && PreviewMaterial && !PreviewMaterial.enableInstancing)
			{
				Debug.LogWarning("Instancing is disabled, please enable instancing: " + PreviewMaterial, PreviewMaterial);
			}
#endif
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			foreach (var buf in buffers.Values)
			{
				buf.buffer.Dispose();
			}

			buffers.Clear();

			argsBuffer?.Release();
			argsBuffer = null;
		}

		private bool EnsureBuffers(Object obj, int index, int clipsCount, out string key)
		{
			var count = Count.x * Count.y;
			CurrentCount = count * clipsCount;

			index += 1;
			var offset = Offset;
			offset.x *= clipsCount;

			(ComputeBuffer, Matrix4x4[]) CreateNewBuffer()
			{
				var buffer = new ComputeBuffer(count, sizeof(float) * 4 * 4);
				var positions = new Matrix4x4[count];
				var i = 0;
				for (var x = 0; x < Count.x; x++)
				{
					for (var y = 0; y < Count.y; y++)
					{
						positions[i] = Matrix4x4.Translate(new Vector3(x * offset.x + index * Offset.x * .8f, 0, y * offset.y)) * transform.localToWorldMatrix *
						               Matrix4x4.TRS(Vector3.zero,
							               Quaternion.identity, Vector3.one);
						++i;
					}
				}

				buffer.SetData(positions);
				return (buffer, positions);
			}

			key = obj.name + index;

			if (!buffers.ContainsKey(key))
				buffers.Add(key, CreateNewBuffer());
			else if (buffers.ContainsKey(key) && !buffers[key].buffer.IsValid() || buffers[key].buffer.count != count)
			{
				buffers[key].buffer.Dispose();
				buffers[key] = CreateNewBuffer();
			}

			return true;
		}


		protected override void Render(Camera cam, Mesh mesh, Material material, MaterialPropertyBlock block, int clipIndex, int clipsCount)
		{
			if (!EnsureBuffers(mesh, clipIndex, clipsCount, out var key)) return;

			for (var k = 0; k < mesh.subMeshCount; k++)
			{
				if (UseInstancedIndirect)
				{
					if (args == null) args = new uint[5];
					if (argsBuffer == null || !argsBuffer.IsValid()) argsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
					block.SetBuffer(PositionBuffer, buffers[key].buffer);
					args[0] = (uint) mesh.GetIndexCount(k);
					args[1] = (uint) (Count.x * Count.y);
					args[2] = (uint) mesh.GetIndexStart(k);
					args[3] = (uint) mesh.GetBaseVertex(k);
					argsBuffer.SetData(args);
					Graphics.DrawMeshInstancedIndirect(mesh, k, material, 
						new Bounds(transform.position, Vector3.one * 100000), argsBuffer, 0, block,
						ShadowCastingMode.On, true, 0, cam);
				}
				else
				{
					if (!material.enableInstancing) material.enableInstancing = true;
					var mats = buffers[key].matrices;
					var count = mats.Length;
					Graphics.DrawMeshInstanced(mesh, k, material, mats, count, block, ShadowCastingMode.On, true, 0, cam);
				}
			}
		}
	}
}