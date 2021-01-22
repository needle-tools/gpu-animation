using System.IO;
using UnityEditor;
using UnityEngine;

namespace Elaborate.AnimationBakery
{
	// public class BakedAnimationContextMenus
	// {
	// 	[MenuItem("Assets/Create/Animation/Bake", true)]
	// 	public static bool Bake_Validate()
	// 	{
	// 		var activeObject = Selection.activeObject;
 //         
	// 		// make sure it is a text asset
	// 		if ((!activeObject) || !(activeObject is DefaultAsset) && !(activeObject is GameObject)) 
	// 		{
	// 			return false;
	// 		}
	//
	// 		return true;
	// 	}
	// 	
	// 	[MenuItem("Assets/Create/Animation/Bake", false)]
	// 	public static void Bake()
	// 	{
	// 		var activeObject = Selection.activeObject;
	// 		var path = AssetDatabase.GetAssetPath(activeObject);
	// 		var dir = activeObject is DefaultAsset ? path : Path.GetDirectoryName(path);
	// 		Debug.Log(dir);
	// 	}
	// }
}