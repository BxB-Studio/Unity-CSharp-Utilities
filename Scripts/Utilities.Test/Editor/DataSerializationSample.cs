#region Namespaces

using System;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;

#endregion

namespace Utilities.Editor
{
	/// <summary>
	/// A sample editor window that demonstrates data serialization and deserialization functionality.
	/// This window allows users to create, view, edit, save, and load serialized data through a simple interface.
	/// </summary>
	internal class DataSerializationSample : EditorWindow
	{
		#region Modules

		/// <summary>
		/// A sample data class for serialization that stores basic personal information.
		/// This class demonstrates how to make a simple data structure serializable
		/// by using the [Serializable] attribute and basic data types.
		/// </summary>
		[Serializable]
		private class DataSample
		{
			/// <summary>
			/// The first name of the person represented by this data sample.
			/// </summary>
			public string firstName;
			
			/// <summary>
			/// The last name of the person represented by this data sample.
			/// </summary>
			public string lastName;
			
			/// <summary>
			/// The age of the person represented by this data sample.
			/// Must be a non-negative integer.
			/// </summary>
			public int age;
		}

		#endregion

		#region Variables

		/// <summary>
		/// The serialization utility for the data sample.
		/// Handles the low-level operations of saving and loading the DataSample object to/from disk.
		/// </summary>
		private DataSerializationUtility<DataSample> serializationUtility;
		
		/// <summary>
		/// The data sample instance that is currently being edited in the window.
		/// When null, the UI will show empty fields and offer to load existing data or create new data.
		/// </summary>
		private DataSample data;
		
		/// <summary>
		/// The full file path where the data sample will be saved to or loaded from.
		/// Located in the Resources/Assets folder within the project's Assets directory.
		/// </summary>
		private string DataPath => $"{Application.dataPath}/Resources/Assets/DataSample.data";

		#endregion

		#region Methods

		/// <summary>
		/// Shows the data serialization sample window.
		/// Creates and displays a new instance of the DataSerializationSample window,
		/// making it visible in the Unity Editor.
		/// </summary>
		[MenuItem("Tools/Utilities/Debug/Data File Sample...")]
		public static void ShowWindow()
		{
			GetWindow<DataSerializationSample>(true, "Data File Sample").Show();
		}

		/// <summary>
		/// Loads the data sample from disk.
		/// Initializes the serialization utility if needed, then loads the data
		/// from the specified file path and updates the UI to reflect the loaded data.
		/// </summary>
		private void Load()
		{
			if (!serializationUtility)
				serializationUtility = new DataSerializationUtility<DataSample>(DataPath, false);

			data = serializationUtility.Load();
		}
		
		/// <summary>
		/// Saves the current data sample to disk.
		/// Initializes the serialization utility if needed, then saves the data
		/// to the specified file path and refreshes the AssetDatabase to ensure
		/// Unity recognizes the new or updated file.
		/// </summary>
		private void Save()
		{
			if (!serializationUtility)
				serializationUtility = new DataSerializationUtility<DataSample>(DataPath, false);

			serializationUtility.SaveOrCreate(data);

			AssetDatabase.Refresh();
		}
		
		/// <summary>
		/// Draws the GUI for the data serialization sample window.
		/// Handles two main states:
		/// 1. When data is loaded: Displays editable fields and a save button
		/// 2. When no data is loaded: Shows disabled fields and options to load existing data or create new data
		/// The UI adapts based on whether the data file exists on disk.
		/// </summary>
		private void OnGUI()
		{

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Data Serialization Sample");
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical(GUI.skin.box);

			if (data != null)
			{
				data.firstName = EditorGUILayout.TextField("First Name", data.firstName);
				data.lastName = EditorGUILayout.TextField("Last Name", data.lastName);
				data.age = math.max(EditorGUILayout.IntField("Age", data.age), 0);

				if (GUILayout.Button("Serialize Data"))
				{
					Save();

					data = null;
				}
			}
			else
			{
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.TextField("First Name", string.Empty);
				EditorGUILayout.TextField("Last Name", string.Empty);
				EditorGUILayout.IntField("Age", 0);
				EditorGUI.EndDisabledGroup();

				if (File.Exists(DataPath))
				{
					if (GUILayout.Button("Deserialize Data"))
						Load();
				}
				else
					data = new DataSample();
			}

			EditorGUILayout.EndVertical();
		}

		#endregion
	}
}