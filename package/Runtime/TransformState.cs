
using UnityEngine;

namespace Elaborate.AnimationBakery
{
	internal struct TransformState
	{
		public Vector3 Position;
		public Vector3 LocalPosition;
		public Quaternion Rotation;
		public Quaternion LocalRotation;
		public Vector3 LossyScale;
		public Vector3 LocalScale;
	}
}