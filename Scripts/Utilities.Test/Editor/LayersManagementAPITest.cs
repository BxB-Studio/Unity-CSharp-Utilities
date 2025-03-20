#region Namespaces

using UnityEngine;
using UnityEditor;

#endregion

namespace Utilities.Editor.Test
{
	/// <summary>
	/// A test editor window for demonstrating and testing the layers management API functionality.
	/// This window provides a user interface to interact with layer operations such as adding, removing,
	/// renaming layers, and checking if layers are empty, all through the LayersManager utility.
	/// </summary>
	public class LayersManagementAPITest : EditorWindow
	{
		#region Enumerators

		/// <summary>
		/// Defines the type of input to be used for layer operations.
		/// Integer: Use layer index for operations.
		/// String: Use layer name for operations.
		/// </summary>
		private enum InputType { Integer, String }
		
		#endregion

		#region Variables

		/// <summary>
		/// The current input type selected for the layers management API test.
		/// Controls whether operations use layer indices or layer names.
		/// Defaults to Integer input type.
		/// </summary>
		private InputType currentInputType = InputType.Integer;
		
		/// <summary>
		/// The index of the layer to be used in operations that require a layer index.
		/// This value is used when the current input type is set to Integer.
		/// </summary>
		private int layerIndex;
		
		/// <summary>
		/// The name of the layer to be used in operations that require a layer name.
		/// This value is used when the current input type is set to String.
		/// </summary>
		private string layerName = string.Empty;
		
		/// <summary>
		/// The target layer identifier (name or index as string) for the rename operation.
		/// When using String input type, this represents the layer name to be renamed.
		/// When using Integer input type, this should be a valid integer representing the layer index.
		/// </summary>
		private string renameLayerTarget = string.Empty;
		
		/// <summary>
		/// The new name to assign to the layer during a rename operation.
		/// This will replace the current name of the layer specified by renameLayerTarget.
		/// </summary>
		private string renameLayerNewName = string.Empty;

		#endregion

		#region Utilities

		/// <summary>
		/// Shows the layers management API test window in the Unity Editor.
		/// Creates a new window instance or focuses an existing one.
		/// Accessible through the Unity Editor menu at Tools/Utilities/Layers Management API Test.
		/// </summary>
		[MenuItem("Tools/Utilities/Layers Management API Test")]
		public static void ShowWindow()
		{
			GetWindow<LayersManagementAPITest>("Layers Management API Test");
		}

		/// <summary>
		/// Attempts to add a new layer with the specified name.
		/// Uses the LayersManager.AddLayer method and handles any exceptions that may occur.
		/// Logs success or failure messages to the Unity console.
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
		/// Attempts to remove a layer using its name.
		/// Uses the LayersManager.RemoveLayer(string) method and handles any exceptions that may occur.
		/// Logs success or failure messages to the Unity console.
		/// This method is used when the current input type is set to String.
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
		/// Attempts to remove a layer using its index.
		/// Uses the LayersManager.RemoveLayer(int) method and handles any exceptions that may occur.
		/// Logs success or failure messages to the Unity console.
		/// This method is used when the current input type is set to Integer.
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
		/// Attempts to rename a layer based on the current input type.
		/// For String input type, uses LayersManager.RenameLayer(string, string) to rename by layer name.
		/// For Integer input type, parses the target string to an integer and uses LayersManager.RenameLayer(int, string).
		/// Handles any exceptions that may occur and logs appropriate messages to the Unity console.
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
		/// Attempts to check if a layer at the specified index is empty.
		/// Uses the LayersManager.IsLayerEmpty method to determine if any GameObjects are assigned to the layer.
		/// Handles any exceptions that may occur and logs the result or error messages to the Unity console.
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
		/// Draws the GUI for the layers management API test window.
		/// Provides UI elements for selecting input type, entering layer information,
		/// and buttons for performing various layer operations.
		/// This method is called automatically by Unity for each frame when the window is visible.
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
