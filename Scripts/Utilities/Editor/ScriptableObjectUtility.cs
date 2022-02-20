#region Namespaces

using System;
using UnityEngine;
using UnityEditor;

#endregion

namespace Utilities
{
	public static class ScriptableObjectUtility
	{
		public static T CreateAsset<T>(string path) where T : ScriptableObject
		{
			return CreateAsset(path, ScriptableObject.CreateInstance<T>());
		}
		public static T CreateAsset<T>(string path, T data) where T : ScriptableObject
		{
			try
			{
				if (AssetDatabase.LoadAssetAtPath<T>($"{path}.asset"))
					AssetDatabase.DeleteAsset($"{path}.asset");

				AssetDatabase.CreateAsset(data, $"{path}.asset");
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				EditorUtility.FocusProjectWindow();

				Selection.activeObject = data;

				return data;
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
			CreateAsset<ScriptableObject>($"{AssetDatabase.GetAssetPath(Selection.activeObject)}/New Scriptable Object");
		}
	}
}
