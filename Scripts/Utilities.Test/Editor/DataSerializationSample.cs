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
	/// A sample editor window for data serialization.
	/// </summary>
	internal class DataSerializationSample : EditorWindow
	{
		#region Modules

		/// <summary>
		/// A sample data class for serialization.
		/// </summary>
		[Serializable]
		private class DataSample
		{
			public string firstName;
			public string lastName;
			public int age;
		}

		#endregion

		#region Variables

		/// <summary>
		/// The serialization utility for the data sample.
		/// </summary>
		private DataSerializationUtility<DataSample> serializationUtility;
		/// <summary>
		/// The data sample.
		/// </summary>
		private DataSample data;
		/// <summary>
		/// The path to the data sample.
		/// </summary>
		private string DataPath => $"{Application.dataPath}/Resources/Assets/DataSample.data";

		#endregion

		#region Methods

		/// <summary>
		/// Shows the data serialization sample window.
		/// </summary>
		[MenuItem("Tools/Utilities/Debug/Data File Sample...")]
		public static void ShowWindow()
		{
			GetWindow<DataSerializationSample>(true, "Data File Sample").Show();
		}

		/// <summary>
		/// Loads the data sample.
		/// </summary>
		private void Load()
		{
			if (!serializationUtility)
				serializationUtility = new DataSerializationUtility<DataSample>(DataPath, false);

			data = serializationUtility.Load();
		}
		/// <summary>
		/// Saves the data sample.
		/// </summary>
		private void Save()
		{
			if (!serializationUtility)
				serializationUtility = new DataSerializationUtility<DataSample>(DataPath, false);

			serializationUtility.SaveOrCreate(data);

			AssetDatabase.Refresh();
		}
		/// <summary>
		/// Draws the GUI for the data serialization sample.
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