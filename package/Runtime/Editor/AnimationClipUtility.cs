#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace Elaborate.AnimationBakery
{
	public static class AnimationClipUtility
	{
		public static bool GetData(Animator animator, AnimationClip clip, out AnimationClipData data)
		{
			var bindings = AnimationUtility.GetCurveBindings(clip);

			if (bindings == null || bindings.Length <= 0)
			{
				data = null;
				return false;
			}

			// 1 - save animation data from curves for easier applying that to the skeleton per frame
			var curves = new Dictionary<Transform, AnimationClipCurve>();
			foreach (var binding in bindings)
			{
				// continue;
				var animatedObject = AnimationUtility.GetAnimatedObject(animator.gameObject, binding) as Transform;
				var propertyName = binding.propertyName;
				var path = binding.path;
				var curve = AnimationUtility.GetEditorCurve(clip, binding);

				if (path.Length <= 0 || curve.length <= 0) continue;

				// we need to rebuild the actual path used by skinning
				// because the binding path is relative to the animator object
				// and this might not match with the skinned mesh renderer paths created from the root bone
				// so rather than trying to merge two different paths later
				// get the path relative to the root bone right here:
				//List<Transform> hierarchy;
				//if (Scene.TryGetHierarchy(pathRoot, animatedObject, out hierarchy))
				//    path = Scene.ToPath(hierarchy.ToArray());
				//else
				//    Debug.LogWarning("could not get the path from " + pathRoot.name + " to " + animatedObject.name);

				if (animatedObject == null) continue;

				var clipCurve = curves.ContainsKey(animatedObject) ? curves[animatedObject] : new AnimationClipCurve();
				clipCurve.Add(propertyName, curve);
				curves[animatedObject] = clipCurve;
			}

			data = new AnimationClipData()
			{
				Name = clip.name,
				Duration = clip.length,
				FrameRate = clip.frameRate,
				Curves = curves
			};
			return true;

			//// 2 - store start bone position
			//var transformStates = GetTransformsState(gameObject);
			//// 3 - loop keyframes of animation clip and store absolute transformations
			//var transformationData = SampleAndStoreAnimationClipData(clip, animations, 2);
			//// 4 - restore start bone position
			//ApplyTransformState(gameObject, transformStates);

			//data = transformationData;
			//return data != null && data.Length > 0;
		}
	}
}
#endif