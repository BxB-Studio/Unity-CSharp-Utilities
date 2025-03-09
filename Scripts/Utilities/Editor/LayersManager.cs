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
	/// Manages layers in Unity.
	/// </summary>
	public static class LayersManager
	{
		#region Constants

		/// <summary>
		/// The maximum number of layers in Unity.
		/// </summary>
		public const int MaxLayersCount = 32; // Total number of layers in Unity

		#endregion

		#region Variables

		/// <summary>
		/// The built-in layer names.
		/// </summary>
		private static readonly string[] BuiltInLayerNames = { "Default", "TransparentFX", "Ignore Raycast", "Water", "UI" };
		/// <summary>
		/// The built-in layer indices.
		/// </summary>
		private static readonly int[] BuiltInLayerIndices = { 0, 1, 2, 4, 5 };

		#endregion

		#region Methods

		/// <summary>
		/// Adds a layer.
		/// </summary>
		/// <param name="name">The name of the layer.</param>
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
		/// Removes a layer.
		/// </summary>
		/// <param name="layerIndex">The index of the layer.</param>
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
		/// Removes a layer.
		/// </summary>
		/// <param name="name">The name of the layer.</param>
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
		/// Renames a layer.
		/// </summary>
		/// <param name="layerIndex">The index of the layer.</param>
		/// <param name="name">The new name of the layer.</param>
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
		/// Renames a layer.
		/// </summary>
		/// <param name="currentName">The current name of the layer.</param>
		/// <param name="newName">The new name of the layer.</param>
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
		/// Checks if a layer is empty.
		/// </summary>
		/// <param name="layerIndex">The index of the layer.</param>
		/// <returns>True if the layer is empty, false otherwise.</returns>
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
		/// Checks if a layer exists.
		/// </summary>
		/// <param name="name">The name of the layer.</param>
		/// <returns>True if the layer exists, false otherwise.</returns>
		public static bool LayerExists(string name)
		{
			return LayerMask.NameToLayer(name) > -1;
		}
		/// <summary>
		/// Gets the layers.
		/// </summary>
		/// <returns>The layers.</returns>
		public static string[] GetLayers()
		{
			// Returning the currently used layers without empty slots
			return InternalEditorUtility.layers;
		}

		/// <summary>
		/// Gets the layers from TagManager.asset.
		/// </summary>
		/// <returns>The layers.</returns>
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
		/// Sets the layers.
		/// </summary>
		/// <param name="layers">The layers.</param>
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
		/// Finds an empty layer slot.
		/// </summary>
		/// <param name="layers">The layers.</param>
		/// <returns>The index of the empty layer slot.</returns>
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
