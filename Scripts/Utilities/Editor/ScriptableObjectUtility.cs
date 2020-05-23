using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Utilities
{
	public static class ScriptableObjectUtility
	{
		public static T CreateAsset<T>(string name = "", string path = "") where T : ScriptableObject
		{
			try
			{
				T asset = ScriptableObject.CreateInstance<T>();

				if (string.IsNullOrEmpty(path))
					path = AssetDatabase.GetAssetPath(Selection.activeObject);
				else
					path = path.Replace(Path.GetFileName(path), "");

				if (string.IsNullOrEmpty(path))
					path = "Assets/";
				else if (Path.GetExtension(path) != "")
					path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");

				string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + (string.IsNullOrEmpty(name) ? "New " + typeof(T).ToString() : name) + ".asset");

				AssetDatabase.CreateAsset(asset, assetPathAndName);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				EditorUtility.FocusProjectWindow();

				Selection.activeObject = asset;

				return asset;
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);

				return null;
			}
		}

		[MenuItem("Assets/Create/ScriptableObjects/Empty Scriptable Object")]
		public static void CreateNewAsset()
		{
			CreateAsset<ScriptableObject>();
		}
	}
}