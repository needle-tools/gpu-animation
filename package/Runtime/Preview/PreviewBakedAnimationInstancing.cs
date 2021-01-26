using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace needle.GpuAnimation
{
	public class PreviewBakedAnimationInstancing : PreviewBakedAnimationBase, IDisposable
	{
		public int ClipIndex = -1;
		public Vector2Int Count = new Vector2Int(10, 10);
		public Vector2 Offset = new Vector2(1, 1);
		public bool UseInstancedIndirect = true;

		private class RenderData : IDisposable
		{
			public ComputeBuffer Buffer;

			public Matrix4x4[] Matrices;

			// args buffer per sub mesh
			public List<ComputeBuffer> Args;

			public void Dispose()
			{
				Buffer?.Dispose();
				if (Args != null)
					foreach (var ab in Args)
						ab?.Dispose();
			}
		}

		private readonly Dictionary<string, RenderData> buffers = new Dictionary<string, RenderData>();
		private static readonly int PositionBuffer = Shader.PropertyToID("_InstanceTransforms");
		private uint[] args;

		private ComputeBuffer _timeOffsets;

		[Header("Internal")] public int InstanceCount = default;
		public int VertexCount = 0;
		private static readonly int InstanceTimeOffsets = Shader.PropertyToID("_InstanceTimeOffsets");


		private void OnValidate()
		{
#if UNITY_WEBGL
			if (!UseInstancedIndirect && PreviewMaterial && !PreviewMaterial.enableInstancing)
			{
				Debug.LogWarning("Instancing is disabled, please enable instancing: " + PreviewMaterial, PreviewMaterial);
			}
#endif

#if UNITY_EDITOR
			if (Selection.Contains(this.gameObject)) Dispose();
#endif
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			Dispose();
		}

		public void Dispose()
		{
			foreach (var data in buffers.Values) data.Dispose();
			buffers.Clear();
			_timeOffsets?.Dispose();
		}

		private bool EnsureBuffers(Object obj, int clipIndex, int clipsCount, out string key)
		{
			var count = Count.x * Count.y;
			InstanceCount = count * clipsCount;

			var offset = Offset;
			if (ClipIndex < 0)
			{
				offset.x *= clipsCount;
			}

			RenderData CreateNewBuffer()
			{
				var buffer = new ComputeBuffer(count, sizeof(float) * 4 * 4);
				var positions = new Matrix4x4[count];
				var i = 0;
				for (var x = 0; x < Count.x; x++)
				{
					var ox = x * offset.x;
					if (ClipIndex < 0) ox += clipIndex;
					for (var y = 0; y < Count.y; y++)
					{
						positions[i] = Matrix4x4.Translate(new Vector3(ox, 0, y * offset.y)) * transform.localToWorldMatrix *
						               Matrix4x4.TRS(Vector3.zero,
							               Quaternion.identity, Vector3.one);
						++i;
					}
				}

				buffer.SetData(positions);
				var data = new RenderData();
				data.Buffer = buffer;
				data.Matrices = positions;
				data.Args = new List<ComputeBuffer>();
				return data;
			}

			key = obj.name + clipIndex;

			if (!buffers.ContainsKey(key))
				buffers.Add(key, CreateNewBuffer());
			else if (buffers.ContainsKey(key) && !buffers[key].Buffer.IsValid() || buffers[key].Buffer.count != count)
			{
				buffers[key].Buffer.Dispose();
				buffers[key] = CreateNewBuffer();
			}

			if (_timeOffsets == null || !_timeOffsets.IsValid() || _timeOffsets.count != InstanceCount)
			{
				var times = new float[InstanceCount];
				for (int i = 0; i < times.Length; i++)
				{
					times[i] = Random.value * 100;
				}

				_timeOffsets = new ComputeBuffer(times.Length, sizeof(float));
				_timeOffsets.SetData(times);
			}

			return true;
		}


		protected override void Render(Camera cam, Mesh mesh, Material material, MaterialPropertyBlock block, int clipIndex, int clipsCount)
		{
			if (ClipIndex >= 0 && ClipIndex != clipIndex) return;

			if (transform.hasChanged)
			{
				transform.hasChanged = false;
				Dispose();
			}
			
			if (!EnsureBuffers(mesh, clipIndex, clipsCount, out var key)) return;

			VertexCount = mesh.vertexCount * InstanceCount;

			var data = buffers[key];
			if (UseInstancedIndirect)
			{
				if (args == null) args = new uint[5];
				block.SetBuffer(PositionBuffer, data.Buffer);
			}

			block.SetBuffer(InstanceTimeOffsets, _timeOffsets);

			for (var k = 0; k < mesh.subMeshCount; k++)
			{
				if (UseInstancedIndirect)
				{
					args[0] = mesh.GetIndexCount(k);
					args[1] = (uint) InstanceCount;
					args[2] = mesh.GetIndexStart(k);
					args[3] = mesh.GetBaseVertex(k);
					if (data.Args.Count <= k) data.Args.Add(new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments));
					var argsBuffer = data.Args[k];
					argsBuffer.SetData(args);

					Graphics.DrawMeshInstancedIndirect(mesh, k, material,
						new Bounds(transform.position, Vector3.one * 100000), argsBuffer, 0, block,
						ShadowCastingMode.On, true, 0, cam);
				}
				else
				{
					if (!material.enableInstancing) material.enableInstancing = true;
					var mats = data.Matrices;
					var count = mats.Length;
					Graphics.DrawMeshInstanced(mesh, k, material, mats, count, block, ShadowCastingMode.On, true, 0, cam);
				}
			}
		}
	}
}