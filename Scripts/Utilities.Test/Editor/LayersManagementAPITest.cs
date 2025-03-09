#region Namespaces

using UnityEngine;
using UnityEditor;

#endregion

namespace Utilities.Editor.Test
{
	/// <summary>
	/// A test editor window for layers management API.
	/// </summary>
	public class LayersManagementAPITest : EditorWindow
	{
		#region Enumerators

		/// <summary>
		/// The type of input for the layers management API test.
		/// </summary>
		private enum InputType { Integer, String }
		
		#endregion

		#region Variables

		/// <summary>
		/// The current input type for the layers management API test.
		/// </summary>
		private InputType currentInputType = InputType.Integer;
		/// <summary>
		/// The index of the layer.
		/// </summary>
		private int layerIndex;
		/// <summary>
		/// The name of the layer.
		/// </summary>
		private string layerName = string.Empty;
		/// <summary>
		/// The target layer for renaming.
		/// </summary>
		private string renameLayerTarget = string.Empty;
		/// <summary>
		/// The new name for the layer.
		/// </summary>
		private string renameLayerNewName = string.Empty;

		#endregion

		#region Utilities

		/// <summary>
		/// Shows the layers management API test window.
		/// </summary>
		[MenuItem("Tools/Utilities/Layers Management API Test")]
		public static void ShowWindow()
		{
			GetWindow<LayersManagementAPITest>("Layers Management API Test");
		}

		/// <summary>
		/// Tries to add a layer.
		/// </summary>
		private void TryAddLayer()
		{
			try
			{
				LayersManager.AddLayer(layerName);
				Debug.Log($"Layer '{layerName}' added successfully.");
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Error adding layer: {ex.Message}");
			}
		}
		/// <summary>
		/// Tries to remove a layer by name.
		/// </summary>
		private void TryRemoveLayerByName()
		{
			try
			{
				LayersManager.RemoveLayer(layerName);
				Debug.Log($"Layer '{layerName}' removed successfully.");
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Error removing layer by name: {ex.Message}");
			}
		}
		/// <summary>
		/// Tries to remove a layer by index.
		/// </summary>
		private void TryRemoveLayerByIndex()
		{
			try
			{
				LayersManager.RemoveLayer(layerIndex);
				Debug.Log($"Layer at index '{layerIndex}' removed successfully.");
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Error removing layer by index: {ex.Message}");
			}
		}
		/// <summary>
		/// Tries to rename a layer.
		/// </summary>
		private void TryRenameLayer()
		{
			try
			{
				if (currentInputType == InputType.String)
				{
					LayersManager.RenameLayer(renameLayerTarget, renameLayerNewName);
					Debug.Log($"Layer '{renameLayerTarget}' renamed to '{renameLayerNewName}' successfully.");
				}
				else
				{
					// Convert renameLayerTarget to an int safely
					if (int.TryParse(renameLayerTarget, out int targetLayerIndex))
					{
						LayersManager.RenameLayer(targetLayerIndex, renameLayerNewName);
						Debug.Log($"Layer at index '{targetLayerIndex}' renamed to '{renameLayerNewName}' successfully.");
					}
					else
					{
						Debug.LogError("Invalid index entered for renaming. Please enter a valid integer.");
					}
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Error renaming layer: {ex.Message}");
			}
		}
		/// <summary>
		/// Tries to check if a layer is empty.
		/// </summary>
		private void TryCheckIfLayerIsEmpty()
		{
			try
			{
				bool isEmpty = LayersManager.IsLayerEmpty(layerIndex);
				Debug.Log($"Layer at index '{layerIndex}' is {(isEmpty ? "empty" : "not empty")}.");
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Error checking if layer is empty: {ex.Message}");
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Draws the GUI for the layers management API test.
		/// </summary>
		private void OnGUI()
		{
			GUILayout.Label("Layers Management API Test", EditorStyles.boldLabel);

			// Popup to switch between Integer and String inputs
			currentInputType = (InputType)EditorGUILayout.EnumPopup("Input Type:", currentInputType);

			// Based on the selection, show either integer or string input for the main operations
			if (currentInputType == InputType.Integer)
			{
				layerIndex = EditorGUILayout.IntField("Layer Index:", layerIndex);
			}
			else
			{
				layerName = EditorGUILayout.TextField("Layer Name:", layerName);
			}

			GUILayout.Space(10);

			// Buttons for various API functions
			if (GUILayout.Button("Add Layer"))
			{
				TryAddLayer();
			}

			if (currentInputType == InputType.String)
			{
				if (GUILayout.Button("Remove Layer by Name"))
				{
					TryRemoveLayerByName();
				}
			}
			else
			{
				if (GUILayout.Button("Remove Layer by Index"))
				{
					TryRemoveLayerByIndex();
				}
			}

			GUILayout.Space(20);

			// Section for renaming layers: provide target layer (name or index) and the new name
			GUILayout.Label("Rename Layer", EditorStyles.boldLabel);
			if (currentInputType == InputType.String)
			{
				renameLayerTarget = EditorGUILayout.TextField("Target Layer Name:", renameLayerTarget);
			}
			else
			{
				renameLayerTarget = EditorGUILayout.TextField("Target Layer Index:", renameLayerTarget);
			}
			renameLayerNewName = EditorGUILayout.TextField("New Name:", renameLayerNewName);

			if (GUILayout.Button("Rename Layer"))
			{
				TryRenameLayer();
			}

			GUILayout.Space(10);

			if (GUILayout.Button("Check if Layer is Empty"))
			{
				TryCheckIfLayerIsEmpty();
			}
		}

		#endregion
	}
}
