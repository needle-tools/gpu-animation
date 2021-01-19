using UnityEngine;

namespace Elaborate.AnimationBakery
{
	[System.Serializable]
	public struct AnimationClipCurveData
	{
		public AnimationCurve positionX, positionY, positionZ;
		public AnimationCurve rotationX, rotationY, rotationZ, rotationW;
		public AnimationCurve eulerX, eulerY, eulerZ;
		public AnimationCurve scaleX, scaleY, scaleZ;

		public bool HasKeyframes => HasPositionKeyframes || HasRotationKeyframes || HasScaleKeyframes;

		public Vector3 Position(float time)
		{
			var x = positionX?.Evaluate(time) ?? 0;
			var y = positionY?.Evaluate(time) ?? 0;
			var z = positionZ?.Evaluate(time) ?? 0;
			return new Vector3(x, y, z);
		}

		public bool HasPositionKeyframes =>
			(positionX != null && positionX.keys.Length > 0) ||
			(positionY != null && positionY.keys.Length > 0) ||
			(positionZ != null && positionZ.keys.Length > 0);

		public Quaternion Rotation(float time)
		{
			if (rotationX == null && rotationY == null && rotationZ == null && rotationW == null)
			{
				var x = eulerX?.Evaluate(time) ?? 0;
				var y = eulerY?.Evaluate(time) ?? 0;
				var z = eulerZ?.Evaluate(time) ?? 0;
				return Quaternion.Euler(x, y, z);
			}
			else
			{
				var x = rotationX?.Evaluate(time) ?? 0;
				var y = rotationY?.Evaluate(time) ?? 0;
				var z = rotationZ?.Evaluate(time) ?? 0;
				var w = rotationW?.Evaluate(time) ?? 0;
				return new Quaternion(x, y, z, w);
			}
		}

		public bool HasRotationKeyframes =>
			(rotationX != null && rotationX.keys.Length > 0) ||
			(rotationY != null && rotationY.keys.Length > 0) ||
			(rotationZ != null && rotationZ.keys.Length > 0) ||
			(rotationW != null && rotationW.keys.Length > 0) ||
			(eulerX != null && eulerX.length > 0) ||
			(eulerY != null && eulerY.length > 0) ||
			(eulerZ != null && eulerZ.length > 0);

		public Vector3 Scale(float time)
		{
			var x = scaleX?.Evaluate(time) ?? 0;
			var y = scaleY?.Evaluate(time) ?? 0;
			var z = scaleZ?.Evaluate(time) ?? 0;
			return new Vector3(x, y, z);
		}

		public bool HasScaleKeyframes =>
			(scaleX != null && scaleX.keys.Length > 0) ||
			(scaleY != null && scaleY.keys.Length > 0) ||
			(scaleZ != null && scaleZ.keys.Length > 0);

		public void Add(string propertyName, AnimationCurve curve)
		{
			propertyName = propertyName.ToLower();

			if (propertyName.Contains("eulerangles"))
			{
				if (propertyName.EndsWith("x"))
					eulerX = curve;
				else if (propertyName.EndsWith("y"))
					eulerY = curve;
				else if (propertyName.EndsWith("z"))
					eulerZ = curve;
			}
			else if (propertyName.Contains("rotation"))
			{
				if (propertyName.EndsWith("x"))
					rotationX = curve;
				else if (propertyName.EndsWith("y"))
					rotationY = curve;
				else if (propertyName.EndsWith("z"))
					rotationZ = curve;
				else if (propertyName.EndsWith("w"))
					rotationW = curve;
			}
			else if (propertyName.Contains("position"))
			{
				if (propertyName.EndsWith("x"))
					positionX = curve;
				else if (propertyName.EndsWith("y"))
					positionY = curve;
				else if (propertyName.EndsWith("z"))
					positionZ = curve;
			}
			else if (propertyName.Contains("scale"))
			{
				if (propertyName.EndsWith("x"))
					scaleX = curve;
				else if (propertyName.EndsWith("y"))
					scaleY = curve;
				else if (propertyName.EndsWith("z"))
					scaleZ = curve;
			}
		}
	}
}