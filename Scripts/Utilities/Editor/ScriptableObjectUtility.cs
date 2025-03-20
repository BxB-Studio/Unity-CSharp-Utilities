#region Namespaces

using System;
using UnityEngine;
using UnityEditor;

#endregion

namespace Utilities.Editor
{
	/// <summary>
	/// Utility class for creating and managing ScriptableObject assets in the Unity Editor.
	/// Provides methods to create, save, and select ScriptableObject instances as project assets.
	/// </summary>
	public static class ScriptableObjectUtility
	{
		/// <summary>
		/// Creates a new ScriptableObject asset at the specified path.
		/// Instantiates a new instance of the specified ScriptableObject type and saves it as an asset.
		/// </summary>
		/// <typeparam name="T">The type of ScriptableObject to create.</typeparam>
		/// <param name="path">The project path where the asset will be created (without .asset extension).</param>
		/// <returns>The created ScriptableObject instance, or null if creation failed.</returns>
		public static T CreateAsset<T>(string path) where T : ScriptableObject
		{
			return CreateAsset(path, ScriptableObject.CreateInstance<T>());
		}

		/// <summary>
		/// Creates a new ScriptableObject asset at the specified path using an existing ScriptableObject instance.
		/// If an asset already exists at the specified path, it will be deleted and replaced.
		/// After creation, the asset is saved, the AssetDatabase is refreshed, and the new asset is selected.
		/// </summary>
		/// <typeparam name="T">The type of ScriptableObject to create.</typeparam>
		/// <param name="path">The project path where the asset will be created (without .asset extension).</param>
		/// <param name="data">The existing ScriptableObject instance to save as an asset.</param>
		/// <returns>The created ScriptableObject instance, or null if creation failed.</returns>
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
		/// Creates a new empty ScriptableObject asset in the currently selected folder.
		/// This method is called when the user selects "Assets/Create/Empty Scriptable Object" from the Unity Editor menu.
		/// The asset is created with a default name "New Scriptable Object" in the currently selected directory.
		/// </summary>
		[MenuItem("Assets/Create/Empty Scriptable Object")]
		internal static void CreateEmptyAsset()
		{
			CreateAsset<ScriptableObject>($"{AssetDatabase.GetAssetPath(Selection.activeObject)}/New Scriptable Object");
		}
	}
}
