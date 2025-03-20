#region Namespaces

using System;
using System.Data;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

#endregion

namespace Utilities.Editor
{
	/// <summary>
	/// Manages Unity layers by providing methods to add, remove, rename, and query layers in the project.
	/// This utility class interacts with Unity's TagManager to modify layer settings.
	/// </summary>
	public static class LayersManager
	{
		#region Constants

		/// <summary>
		/// The maximum number of layers supported by Unity's layer system.
		/// Unity restricts projects to 32 total layers (0-31).
		/// </summary>
		public const int MaxLayersCount = 32; // Total number of layers in Unity

		#endregion

		#region Variables

		/// <summary>
		/// The names of Unity's built-in layers that cannot be modified or removed.
		/// These layers have special functionality in Unity's rendering and physics systems.
		/// </summary>
		private static readonly string[] BuiltInLayerNames = { "Default", "TransparentFX", "Ignore Raycast", "Water", "UI" };
		
		/// <summary>
		/// The indices of Unity's built-in layers in the layer array.
		/// These indices correspond to the BuiltInLayerNames and are protected from modification.
		/// </summary>
		private static readonly int[] BuiltInLayerIndices = { 0, 1, 2, 4, 5 };

		#endregion

		#region Methods

		/// <summary>
		/// Adds a new layer to the project with the specified name.
		/// The layer will be added to the first available empty slot (starting from index 8 to avoid built-in layers).
		/// </summary>
		/// <param name="name">The name of the layer to add.</param>
		/// <exception cref="DuplicateNameException">Thrown when a layer with the same name already exists.</exception>
		/// <exception cref="Exception">Thrown when no empty layer slots are available.</exception>
		public static void AddLayer(string name)
		{
			// Get current layers
			string[] layers = GetLayersFromTagManager();
			
			// Check if layer name already exists
			foreach (var layer in layers)
			{
				if (layer.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					throw new DuplicateNameException($"Layer '{name}' already exists.");
				}
			}

			// Find first empty slot in the layer array (starting from index 8 to avoid built-in layers)
			int emptySlotIndex = FindEmptyLayerSlot(layers);
			if (emptySlotIndex == -1)
			{
				throw new Exception("Error adding layer: No empty layer slots available.");
			}

			// Add the layer at the first empty slot
			layers[emptySlotIndex] = name;

			// Save the updated layers to TagManager
			SetLayers(layers);
		}
		
		/// <summary>
		/// Removes a layer at the specified index by clearing its name.
		/// Built-in layers cannot be removed.
		/// </summary>
		/// <param name="layerIndex">The index of the layer to remove (0-31).</param>
		/// <exception cref="ArgumentException">Thrown when attempting to remove a built-in layer.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the layer index is outside the valid range.</exception>
		public static void RemoveLayer(int layerIndex)
		{
			if (Array.Exists(BuiltInLayerIndices, index => index == layerIndex))
			{
				throw new ArgumentException($"Cannot remove built-in layer at index {layerIndex}.");
			}

			string[] layers = GetLayersFromTagManager();
			if (layerIndex < 0 || layerIndex >= layers.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(layerIndex), $"Layer index {layerIndex} is out of range.");
			}

			layers[layerIndex] = string.Empty; // Mark the layer as empty
			SetLayers(layers); // Save the updated layers
		}
		
		/// <summary>
		/// Removes a layer with the specified name by clearing its entry in the layers array.
		/// Built-in layers cannot be removed.
		/// </summary>
		/// <param name="name">The name of the layer to remove.</param>
		/// <exception cref="ArgumentException">Thrown when attempting to remove a built-in layer.</exception>
		/// <exception cref="Exception">Thrown when the specified layer name is not found.</exception>
		public static void RemoveLayer(string name)
		{
			// Check if the name is one of the built-in layers
			if (Array.Exists(BuiltInLayerNames, builtInName => builtInName.Equals(name, StringComparison.OrdinalIgnoreCase)))
			{
				throw new ArgumentException($"Cannot remove built-in layer with name '{name}'.");
			}

			int layerIndex = LayerMask.NameToLayer(name);
			if (layerIndex == -1)
			{
				throw new Exception($"Layer '{name}' not found.");
			}

			RemoveLayer(layerIndex); // Reuse RemoveLayer by index
		}
		
		/// <summary>
		/// Renames a layer at the specified index to the new name.
		/// Built-in layers cannot be renamed, and the new name must not already exist.
		/// </summary>
		/// <param name="layerIndex">The index of the layer to rename (0-31).</param>
		/// <param name="name">The new name for the layer.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the layer index is outside the valid range.</exception>
		/// <exception cref="ArgumentException">Thrown when attempting to rename a built-in layer.</exception>
		/// <exception cref="DuplicateNameException">Thrown when a layer with the new name already exists.</exception>
		public static void RenameLayer(int layerIndex, string name)
		{
			// Validate index
			if (layerIndex < 0 || layerIndex >= MaxLayersCount)
			{
				throw new ArgumentOutOfRangeException(nameof(layerIndex), $"Layer index {layerIndex} is out of range.");
			}

			// Check if it's a built-in layer
			if (Array.Exists(BuiltInLayerIndices, index => index == layerIndex))
			{
				throw new ArgumentException($"Cannot rename built-in layer at index {layerIndex}.");
			}

			// Check if new name already exists
			string[] layers = GetLayersFromTagManager();
			foreach (var layer in layers)
			{
				if (layer.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					throw new DuplicateNameException($"Layer '{name}' already exists.");
				}
			}

			// Set the new name
			layers[layerIndex] = name;
			SetLayers(layers);
		}
		
		/// <summary>
		/// Renames a layer with the specified current name to the new name.
		/// Built-in layers cannot be renamed, and the new name must not already exist.
		/// </summary>
		/// <param name="currentName">The current name of the layer to rename.</param>
		/// <param name="newName">The new name for the layer.</param>
		/// <exception cref="Exception">Thrown when the layer with the current name is not found.</exception>
		public static void RenameLayer(string currentName, string newName)
		{
			int layerIndex = LayerMask.NameToLayer(currentName);
			if (layerIndex == -1)
			{
				throw new Exception($"Layer '{currentName}' not found.");
			}

			RenameLayer(layerIndex, newName);
		}
		
		/// <summary>
		/// Checks if a layer at the specified index is empty (has no name assigned).
		/// </summary>
		/// <param name="layerIndex">The index of the layer to check (0-31).</param>
		/// <returns>True if the layer is empty (has no name), false otherwise.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the layer index is outside the valid range.</exception>
		public static bool IsLayerEmpty(int layerIndex)
		{
			if (layerIndex < 0 || layerIndex >= MaxLayersCount)
			{
				throw new ArgumentOutOfRangeException(nameof(layerIndex), $"Layer index {layerIndex} is out of range.");
			}

			string[] layers = GetLayersFromTagManager();
			return string.IsNullOrEmpty(layers[layerIndex]);
		}
		
		/// <summary>
		/// Checks if a layer with the specified name exists in the project.
		/// </summary>
		/// <param name="name">The name of the layer to check.</param>
		/// <returns>True if a layer with the specified name exists, false otherwise.</returns>
		public static bool LayerExists(string name)
		{
			return LayerMask.NameToLayer(name) > -1;
		}
		
		/// <summary>
		/// Gets all currently defined layers in the project, excluding empty slots.
		/// Uses Unity's InternalEditorUtility to retrieve the active layers.
		/// </summary>
		/// <returns>An array of strings containing the names of all defined layers.</returns>
		public static string[] GetLayers()
		{
			// Returning the currently used layers without empty slots
			return InternalEditorUtility.layers;
		}

		/// <summary>
		/// Retrieves the complete layer array from Unity's TagManager asset.
		/// This includes all 32 layer slots, including empty ones.
		/// </summary>
		/// <returns>An array of strings representing all layer slots (0-31), with empty strings for undefined layers.</returns>
		private static string[] GetLayersFromTagManager()
		{
			// Get the TagManager.asset file, which contains all the layers
			SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty layersProp = tagManager.FindProperty("layers");

			string[] layers = new string[MaxLayersCount];
			for (int i = 0; i < MaxLayersCount; i++)
			{
				layers[i] = layersProp.GetArrayElementAtIndex(i).stringValue;
			}

			return layers;
		}
		
		/// <summary>
		/// Saves the provided layer array back to Unity's TagManager asset.
		/// This updates the project's layer definitions.
		/// </summary>
		/// <param name="layers">The array of layer names to save. Must be exactly 32 elements long.</param>
		private static void SetLayers(string[] layers)
		{
			// Save the layers back to TagManager.asset
			SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty layersProp = tagManager.FindProperty("layers");

			for (int i = 0; i < MaxLayersCount; i++)
			{
				layersProp.GetArrayElementAtIndex(i).stringValue = layers[i];
			}

			tagManager.ApplyModifiedProperties();
		}
		
		/// <summary>
		/// Finds the first empty layer slot in the provided layer array.
		/// An empty slot is one with a null, empty, or space-only string.
		/// </summary>
		/// <param name="layers">The array of layer names to search through.</param>
		/// <returns>The index of the first empty layer slot, or -1 if no empty slots are available.</returns>
		private static int FindEmptyLayerSlot(string[] layers)
		{
			// Start searching from index 8 to avoid built-in layers (0-7 are reserved)
			for (int i = 0; i < MaxLayersCount; i++)
			{
				if (string.IsNullOrEmpty(layers[i]) || layers[i] == " ") // Allow space as valid empty layer
				{
					return i;
				}
			}

			return -1; // No empty slot found
		}

		#endregion
	}
}
