#region Namespaces

using System;
using UnityEngine;
using UnityEditor;

#endregion

namespace Utilities.Editor
{
	/// <summary>
	/// Utility class for creating Scriptable Objects.
	/// </summary>
	public static class ScriptableObjectUtility
	{
		/// <summary>
		/// Creates a new Scriptable Object asset at the specified path.
		/// </summary>
		/// <typeparam name="T">The type of Scriptable Object to create.</typeparam>
		public static T CreateAsset<T>(string path) where T : ScriptableObject
		{
			return CreateAsset(path, ScriptableObject.CreateInstance<T>());
		}
		/// <summary>
		/// Creates a new Scriptable Object asset at the specified path.
		/// </summary>
		/// <typeparam name="T">The type of Scriptable Object to create.</typeparam>
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

		/// <summary>
		/// Creates a new Scriptable Object asset at the specified path.
		/// </summary>
		[MenuItem("Assets/Create/Empty Scriptable Object")]
		internal static void CreateEmptyAsset()
		{
			CreateAsset<ScriptableObject>($"{AssetDatabase.GetAssetPath(Selection.activeObject)}/New Scriptable Object");
		}
	}
}
