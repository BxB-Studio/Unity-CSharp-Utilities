#region Namespaces

using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

#endregion

namespace Utilities.Editor
{
	/// <summary>
	/// Utility class for editor-related functions, providing helper methods and UI elements for Unity Editor extensions.
	/// </summary>
	public static class EditorUtilities
	{
		#region Modules

		/// <summary>
		/// Utility class for editor-related styles, providing customized GUIStyle instances for consistent UI appearance.
		/// </summary>
		public static class Styles
		{
			/// <summary>
			/// The standard button style with customized text color for better visibility.
			/// </summary>
			public static GUIStyle Button => new GUIStyle("Button")
			{
#if UNITY_2019_3_OR_NEWER
				normal = new GUIStyleState()
				{
					textColor = Utility.Color.lightGray
				}
#endif
			};
			/// <summary>
			/// The active button style that changes appearance when selected, with text color adjusted based on editor skin.
			/// </summary>
			public static GUIStyle ButtonActive
			{
				get
				{
					GUIStyle style = new GUIStyle("Button");

#if UNITY_2019_3_OR_NEWER
					style.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
#else
					style.normal = style.active;
#endif

					return style;
				}
			}
			/// <summary>
			/// The mini button style with reduced size and customized text color for better visibility.
			/// </summary>
			public static GUIStyle MiniButton => new GUIStyle("MiniButton")
			{
#if UNITY_2019_3_OR_NEWER
				normal = new GUIStyleState()
				{
					textColor = Utility.Color.lightGray
				}
#endif
			};
			/// <summary>
			/// The active mini button style that changes appearance when selected, with background and text color adjusted based on editor skin.
			/// </summary>
			public static GUIStyle MiniButtonActive
			{
				get
				{
					GUIStyle style = new GUIStyle("MiniButton");

#if UNITY_2019_3_OR_NEWER
					style.normal = new GUIStyleState()
					{
						background = style.active.background,
						scaledBackgrounds = style.active.scaledBackgrounds,
						textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
					};
#else
					style.normal = style.active;
#endif

					return style;
				}
			}
			/// <summary>
			/// The middle button style for multi-button groups with customized text color for better visibility.
			/// </summary>
			public static GUIStyle MiniButtonMiddle => new GUIStyle("MiniButtonMid")
			{
#if UNITY_2019_3_OR_NEWER
				normal = new GUIStyleState()
				{
					textColor = Utility.Color.lightGray
				}
#endif
			};
			/// <summary>
			/// The active middle button style for multi-button groups that changes appearance when selected, with text color adjusted based on editor skin.
			/// </summary>
			public static GUIStyle MiniButtonMiddleActive
			{
				get
				{
					GUIStyle style = new GUIStyle("MiniButtonMid");

#if UNITY_2019_3_OR_NEWER
					style.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
#else
					style.normal = style.active;
#endif

					return style;
				}
			}
			/// <summary>
			/// The left button style for multi-button groups with customized text color for better visibility.
			/// </summary>
			public static GUIStyle MiniButtonLeft => new GUIStyle("MiniButtonLeft")
			{
#if UNITY_2019_3_OR_NEWER
				normal = new GUIStyleState()
				{
					textColor = Utility.Color.lightGray
				}
#endif
			};
			/// <summary>
			/// The active left button style for multi-button groups that changes appearance when selected, with text color adjusted based on editor skin.
			/// </summary>
			public static GUIStyle MiniButtonLeftActive
			{
				get
				{
					GUIStyle style = new GUIStyle("MiniButtonLeft");

#if UNITY_2019_3_OR_NEWER
					style.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
#else
					style.normal = style.active;
#endif

					return style;
				}
			}
			/// <summary>
			/// The right button style for multi-button groups with customized text color for better visibility.
			/// </summary>
			public static GUIStyle MiniButtonRight => new GUIStyle("MiniButtonRight")
			{
#if UNITY_2019_3_OR_NEWER
				normal = new GUIStyleState()
				{
					textColor = Utility.Color.lightGray
				}
#endif
			};
			/// <summary>
			/// The active right button style for multi-button groups that changes appearance when selected, with text color adjusted based on editor skin.
			/// </summary>
			public static GUIStyle MiniButtonRightActive
			{
				get
				{
					GUIStyle style = new GUIStyle("MiniButtonRight");

#if UNITY_2019_3_OR_NEWER
					style.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
#else
					style.normal = style.active;
#endif

					return style;
				}
			}
		}
		/// <summary>
		/// Utility class for editor-related icons, providing access to common UI icons with theme-appropriate variants.
		/// </summary>
		public static class Icons
		{
			/// <summary>
			/// The add/plus icon for adding new items or elements.
			/// </summary>
			public static Texture2D Add => Resources.Load($"{IconsPath}/{IconsThemeFolder}/plus") as Texture2D;
			/// <summary>
			/// The upward-pointing caret icon for navigation or expansion controls.
			/// </summary>
			public static Texture2D CaretUp => Resources.Load($"{IconsPath}/{IconsThemeFolder}/caret-up") as Texture2D;
			/// <summary>
			/// The downward-pointing caret icon for navigation or collapse controls.
			/// </summary>
			public static Texture2D CaretDown => Resources.Load($"{IconsPath}/{IconsThemeFolder}/caret-down") as Texture2D;
			/// <summary>
			/// The leftward-pointing caret icon for navigation or back controls.
			/// </summary>
			public static Texture2D CaretLeft => Resources.Load($"{IconsPath}/{IconsThemeFolder}/caret-left") as Texture2D;
			/// <summary>
			/// The rightward-pointing caret icon for navigation or forward controls.
			/// </summary>
			public static Texture2D CaretRight => Resources.Load($"{IconsPath}/{IconsThemeFolder}/caret-right") as Texture2D;
			/// <summary>
			/// The chart icon for data visualization or statistics features.
			/// </summary>
			public static Texture2D Chart => Resources.Load($"{IconsPath}/{IconsThemeFolder}/chart") as Texture2D;
			/// <summary>
			/// The upward-pointing chevron icon for navigation or expansion controls.
			/// </summary>
			public static Texture2D ChevronUp => Resources.Load($"{IconsPath}/{IconsThemeFolder}/chevron-up") as Texture2D;
			/// <summary>
			/// The downward-pointing chevron icon for navigation or collapse controls.
			/// </summary>
			public static Texture2D ChevronDown => Resources.Load($"{IconsPath}/{IconsThemeFolder}/chevron-down") as Texture2D;
			/// <summary>
			/// The leftward-pointing chevron icon for navigation or back controls.
			/// </summary>
			public static Texture2D ChevronLeft => Resources.Load($"{IconsPath}/{IconsThemeFolder}/chevron-left") as Texture2D;
			/// <summary>
			/// The rightward-pointing chevron icon for navigation or forward controls.
			/// </summary>
			public static Texture2D ChevronRight => Resources.Load($"{IconsPath}/{IconsThemeFolder}/chevron-right") as Texture2D;
			/// <summary>
			/// The check mark icon for indicating selected or completed items.
			/// </summary>
			public static Texture2D Check => Resources.Load($"{IconsPath}/{IconsThemeFolder}/check") as Texture2D;
			/// <summary>
			/// The check mark in a circle icon for indicating successful operations or validated items.
			/// </summary>
			public static Texture2D CheckCircle => Resources.Load($"{IconsPath}/{IconsThemeFolder}/check-circle") as Texture2D;
			/// <summary>
			/// The colored version of the check mark in a circle icon, not affected by theme settings.
			/// </summary>
			public static Texture2D CheckCircleColored => Resources.Load($"{IconsPath}/check-circle") as Texture2D;
			/// <summary>
			/// The clone/duplicate icon for copying or duplicating items.
			/// </summary>
			public static Texture2D Clone => Resources.Load($"{IconsPath}/{IconsThemeFolder}/clone") as Texture2D;
			/// <summary>
			/// The box icon for packaging or container-related features.
			/// </summary>
			public static Texture2D Box => Resources.Load($"{IconsPath}/{IconsThemeFolder}/box") as Texture2D;
			/// <summary>
			/// The cross/X icon for closing, canceling, or removing items.
			/// </summary>
			public static Texture2D Cross => Resources.Load($"{IconsPath}/{IconsThemeFolder}/cross") as Texture2D;
			/// <summary>
			/// The error icon (exclamation in a circle) for indicating errors or critical issues.
			/// </summary>
			public static Texture2D Error => Resources.Load($"{IconsPath}/exclamation-circle") as Texture2D;
			/// <summary>
			/// The exclamation mark in a circle icon for indicating warnings or important notices.
			/// </summary>
			public static Texture2D ExclamationCircle => Resources.Load($"{IconsPath}/{IconsThemeFolder}/exclamation-circle") as Texture2D;
			/// <summary>
			/// The exclamation mark in a triangle icon for indicating warnings or cautions.
			/// </summary>
			public static Texture2D ExclamationTriangle => Resources.Load($"{IconsPath}/{IconsThemeFolder}/exclamation-triangle") as Texture2D;
			/// <summary>
			/// The exclamation mark in a square icon for indicating important information or notices.
			/// </summary>
			public static Texture2D ExclamationSquare => Resources.Load($"{IconsPath}/{IconsThemeFolder}/exclamation-square") as Texture2D;
			/// <summary>
			/// The eye icon for visibility controls or view-related features.
			/// </summary>
			public static Texture2D Eye => Resources.Load($"{IconsPath}/{IconsThemeFolder}/eye") as Texture2D;
			/// <summary>
			/// The information icon (exclamation in a square) for indicating informational messages.
			/// </summary>
			public static Texture2D Info => Resources.Load($"{IconsPath}/exclamation-square") as Texture2D;
			/// <summary>
			/// The pencil icon for editing or modifying items.
			/// </summary>
			public static Texture2D Pencil => Resources.Load($"{IconsPath}/{IconsThemeFolder}/pencil") as Texture2D;
			/// <summary>
			/// The reload/refresh icon for reloading or refreshing content.
			/// </summary>
			public static Texture2D Reload => Resources.Load($"{IconsPath}/{IconsThemeFolder}/reload") as Texture2D;
			/// <summary>
			/// The settings/cog icon for configuration or preferences features.
			/// </summary>
			public static Texture2D Settings => Resources.Load($"{IconsPath}/{IconsThemeFolder}/cog") as Texture2D;
			/// <summary>
			/// The save icon for saving or storing data.
			/// </summary>
			public static Texture2D Save => Resources.Load($"{IconsPath}/{IconsThemeFolder}/save") as Texture2D;
			/// <summary>
			/// The sort icon for sorting or ordering features.
			/// </summary>
			public static Texture2D Sort => Resources.Load($"{IconsPath}/{IconsThemeFolder}/sort") as Texture2D;
			/// <summary>
			/// The trash/delete icon for removing or deleting items.
			/// </summary>
			public static Texture2D Trash => Resources.Load($"{IconsPath}/{IconsThemeFolder}/trash") as Texture2D;
			/// <summary>
			/// The warning icon (exclamation in a triangle) for indicating warnings or potential issues.
			/// </summary>
			public static Texture2D Warning => Resources.Load($"{IconsPath}/exclamation-triangle") as Texture2D;
		}

		#endregion

		#region Variables

		/// <summary>
		/// The relative path to the icons resources folder, used for loading icon textures.
		/// </summary>
		private static readonly string IconsPath = "Editor/Icons";
		/// <summary>
		/// The theme-specific subfolder name for icons, automatically selected based on whether the editor is using the Pro skin.
		/// </summary>
		private static readonly string IconsThemeFolder = EditorGUIUtility.isProSkin ? "Pro" : "Personal";

		#endregion

		#region Methods

		#region Utilities

		/// <summary>
		/// Adds a scripting define symbol to the current build target's compilation settings.
		/// If the symbol already exists, it will not be added again.
		/// </summary>
		/// <param name="symbol">The symbol to add to the scripting define symbols.</param>
		public static void AddScriptingDefineSymbol(string symbol)
		{
			string[] scriptingDefineSymbols = GetScriptingDefineSymbols();
			bool emptySymbols = scriptingDefineSymbols.Length < 1;

			if (emptySymbols || !ScriptingDefineSymbolExists(scriptingDefineSymbols, symbol))
#if UNITY_2021_2_OR_NEWER
				PlayerSettings.SetScriptingDefineSymbols(GetCurrentNamedBuildTarget(), $"{(!emptySymbols ? $"{string.Join(';', scriptingDefineSymbols)};" : "")}{symbol}");
#else
				PlayerSettings.SetScriptingDefineSymbolsForGroup(GetCurrentBuildTargetGroup(), $"{(!emptySymbols ? $"{string.Join(";", scriptingDefineSymbols)};" : "")}{symbol}");
#endif
		}
		/// <summary>
		/// Removes a scripting define symbol from the current build target's compilation settings.
		/// If the symbol doesn't exist, no action is taken.
		/// </summary>
		/// <param name="symbol">The symbol to remove from the scripting define symbols.</param>
		public static void RemoveScriptingDefineSymbol(string symbol)
		{
			string[] scriptingDefineSymbols = GetScriptingDefineSymbols();

			if (scriptingDefineSymbols.Length > 0 && ScriptingDefineSymbolExists(scriptingDefineSymbols, symbol, out int symbolIndex))
			{
				ArrayUtility.RemoveAt(ref scriptingDefineSymbols, symbolIndex);
#if UNITY_2021_2_OR_NEWER
				PlayerSettings.SetScriptingDefineSymbols(GetCurrentNamedBuildTarget(), scriptingDefineSymbols.Length > 0 ? string.Join(';', scriptingDefineSymbols) : "");
#else
				PlayerSettings.SetScriptingDefineSymbolsForGroup(GetCurrentBuildTargetGroup(), scriptingDefineSymbols.Length > 0 ? string.Join(";", scriptingDefineSymbols) : "");
#endif
			}
		}
		/// <summary>
		/// Removes a scripting define symbol at a specific index from the current build target's compilation settings.
		/// If the index is out of range, no action is taken.
		/// </summary>
		/// <param name="symbolIndex">The zero-based index of the symbol to remove.</param>
		public static void RemoveScriptingDefineSymbol(int symbolIndex)
		{
			string[] scriptingDefineSymbols = GetScriptingDefineSymbols();

			if (symbolIndex > -1 && symbolIndex < scriptingDefineSymbols.Length)
			{
				ArrayUtility.RemoveAt(ref scriptingDefineSymbols, symbolIndex);
#if UNITY_2021_2_OR_NEWER
				PlayerSettings.SetScriptingDefineSymbols(GetCurrentNamedBuildTarget(), scriptingDefineSymbols.Length > 0 ? string.Join(';', scriptingDefineSymbols) : "");
#else
				PlayerSettings.SetScriptingDefineSymbolsForGroup(GetCurrentBuildTargetGroup(), scriptingDefineSymbols.Length > 0 ? string.Join(";", scriptingDefineSymbols) : "");
#endif
			}
		}
		/// <summary>
		/// Checks if a specific scripting define symbol exists in the current build target's compilation settings.
		/// </summary>
		/// <param name="symbol">The symbol to check for existence.</param>
		/// <returns>True if the symbol exists, false otherwise.</returns>
		public static bool ScriptingDefineSymbolExists(string symbol)
		{
			string[] scriptingDefineSymbols = GetScriptingDefineSymbols();

			return ScriptingDefineSymbolExists(scriptingDefineSymbols, symbol);
		}
		/// <summary>
		/// Checks if a specific scripting define symbol exists in the current build target's compilation settings
		/// and returns its index if found.
		/// </summary>
		/// <param name="symbol">The symbol to check for existence.</param>
		/// <param name="symbolIndex">When this method returns, contains the zero-based index of the symbol if found; otherwise, -1.</param>
		/// <returns>True if the symbol exists, false otherwise.</returns>
		public static bool ScriptingDefineSymbolExists(string symbol, out int symbolIndex)
		{
			string[] scriptingDefineSymbols = GetScriptingDefineSymbols();

			return ScriptingDefineSymbolExists(scriptingDefineSymbols, symbol, out symbolIndex);
		}
		/// <summary>
		/// Gets all scripting define symbols for the current build target as an array.
		/// </summary>
		/// <returns>An array containing all scripting define symbols for the current build target.</returns>
		public static string[] GetScriptingDefineSymbols()
		{
#if UNITY_2021_2_OR_NEWER
			GetScriptingDefineSymbols(GetCurrentNamedBuildTarget(), out string[] defines);
#else
			GetScriptingDefineSymbols(GetCurrentBuildTargetGroup(), out string[] defines);
#endif

			return defines;
		}
#if UNITY_2021_2_OR_NEWER
		/// <summary>
		/// Gets all scripting define symbols for the specified named build target as an array.
		/// This method is only available in Unity 2021.2 or newer.
		/// </summary>
		/// <param name="namedBuildTarget">The named build target to get scripting define symbols for.</param>
		/// <param name="defines">When this method returns, contains an array of all scripting define symbols for the specified build target.</param>
		public static void GetScriptingDefineSymbols(NamedBuildTarget namedBuildTarget, out string[] defines)
		{
			PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out defines);
		}
		/// <summary>
		/// Gets the current named build target based on the selected build target group.
		/// This method is only available in Unity 2021.2 or newer.
		/// </summary>
		/// <returns>The current named build target.</returns>
		public static NamedBuildTarget GetCurrentNamedBuildTarget()
		{
			return NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
		}
#else
		/// <summary>
		/// Gets all scripting define symbols for the specified build target group as an array.
		/// This method is used in Unity versions prior to 2021.2.
		/// </summary>
		/// <param name="buildTargetGroup">The build target group to get scripting define symbols for.</param>
		/// <param name="defines">When this method returns, contains an array of all scripting define symbols for the specified build target group.</param>
		public static void GetScriptingDefineSymbols(BuildTargetGroup buildTargetGroup, out string[] defines)
		{
			PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup, out defines);
		}
#endif
		/// <summary>
		/// Gets the currently selected build target group in the Unity Editor.
		/// </summary>
		/// <returns>The currently selected build target group.</returns>
		public static BuildTargetGroup GetCurrentBuildTargetGroup()
		{
			return EditorUserBuildSettings.selectedBuildTargetGroup;
		}
		/// <summary>
		/// Gets the currently active build target in the Unity Editor.
		/// </summary>
		/// <returns>The currently active build target.</returns>
		public static BuildTarget GetCurrentBuildTarget()
		{
			return EditorUserBuildSettings.activeBuildTarget;
		}

		/// <summary>
		/// Checks if a specific scripting define symbol exists in the provided array of symbols.
		/// </summary>
		/// <param name="symbols">The array of symbols to search in.</param>
		/// <param name="symbol">The symbol to check for existence.</param>
		/// <returns>True if the symbol exists in the array, false otherwise.</returns>
		private static bool ScriptingDefineSymbolExists(string[] symbols, string symbol)
		{
			return Array.IndexOf(symbols, symbol) > -1;
		}
		/// <summary>
		/// Checks if a specific scripting define symbol exists in the provided array of symbols
		/// and returns its index if found.
		/// </summary>
		/// <param name="symbols">The array of symbols to search in.</param>
		/// <param name="symbol">The symbol to check for existence.</param>
		/// <param name="index">When this method returns, contains the zero-based index of the symbol if found; otherwise, -1.</param>
		/// <returns>True if the symbol exists in the array, false otherwise.</returns>
		private static bool ScriptingDefineSymbolExists(string[] symbols, string symbol, out int index)
		{
			index = Array.IndexOf(symbols, symbol);

			return index > -1;
		}

		#endregion

		#region Menu Items

		/// <summary>
		/// Validation method for the Debug GameObject Bounds menu item.
		/// Ensures that exactly one GameObject is selected before enabling the menu item.
		/// </summary>
		/// <returns>True if exactly one GameObject is selected, false otherwise.</returns>
		[MenuItem("Tools/Utilities/Debug/GameObject Bounds", true)]
		public static bool DebugBoundsCheck()
		{
			if (Selection.activeGameObject && Selection.gameObjects.Length == 1)
				return true;

			return false;
		}
		/// <summary>
		/// Menu item that displays the bounds information of the currently selected GameObject in the console.
		/// Shows dimensions and center position of the object's bounds.
		/// </summary>
		[MenuItem("Tools/Utilities/Debug/GameObject Bounds", false, 0)]
		public static void DebugBounds()
		{
			if (!DebugBoundsCheck())
				return;

			Bounds bounds = Utility.GetObjectBounds(Selection.activeGameObject);

			Debug.Log($"{Selection.activeGameObject.name} Dimensions (Click to see more...)\r\n\r\nSize in meters\r\nX: {bounds.size.x}\r\nY: {bounds.size.y}\r\nZ: {bounds.size.z}\r\n\r\nCenter in Unity coordinates\r\nX: {bounds.center.x}\r\nY: {bounds.center.y}\r\nZ: {bounds.center.z}\r\n\r\n", Selection.activeGameObject);
		}
		/// <summary>
		/// Validation method for the Debug GameObject Meshes menu item.
		/// Ensures that exactly one GameObject with at least one MeshFilter component is selected before enabling the menu item.
		/// </summary>
		/// <returns>True if exactly one GameObject with a MeshFilter is selected, false otherwise.</returns>
		[MenuItem("Tools/Utilities/Debug/GameObject Meshes", true)]
		public static bool DebugMeshCheck()
		{
			if (Selection.activeGameObject && Selection.gameObjects.Length == 1 && Selection.activeGameObject.GetComponentInChildren<MeshFilter>())
				return true;

			return false;
		}
		/// <summary>
		/// Menu item that displays mesh information of the currently selected GameObject in the console.
		/// Shows total vertex and triangle counts across all meshes in the GameObject and its children.
		/// </summary>
		[MenuItem("Tools/Utilities/Debug/GameObject Meshes", false, 0)]
		public static void DebugMesh()
		{
			if (!DebugMeshCheck())
				return;

			MeshFilter[] meshFilters = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>();
			int vertices = 0;
			int triangles = 0;

			for (int i = 0; i < meshFilters.Length; i++)
				if (meshFilters[i].sharedMesh)
				{
					vertices += meshFilters[i].sharedMesh.vertexCount;
					triangles += meshFilters[i].sharedMesh.triangles.Length;

#if UNITY_2019_3_OR_NEWER
					for (int j = 0; j < meshFilters[i].sharedMesh.subMeshCount; j++)
						vertices += meshFilters[i].sharedMesh.GetSubMesh(j).vertexCount;
#endif
				}

			Debug.Log($"{Selection.activeGameObject.name} Mesh Details (Click to see more...)\r\n\r\nVertices: {vertices}\r\nTriangles: {triangles}", Selection.activeGameObject);
		}
		/// <summary>
		/// Validation method for the Place Object on Surface menu item.
		/// Ensures that exactly one GameObject is selected before enabling the menu item.
		/// </summary>
		/// <returns>True if exactly one GameObject is selected, false otherwise.</returns>
		[MenuItem("Tools/Utilities/Place the selected object on top of the Zero surface", true)]
		public static bool PlaceObjectOnSurfaceCheck()
		{
			if (Selection.activeGameObject && Selection.gameObjects.Length == 1)
				return true;

			return false;
		}
		/// <summary>
		/// Menu item that places the selected GameObject on the XZ plane (y=0) with its bottom edge touching the plane.
		/// Calculates the object's bounds and adjusts its position accordingly.
		/// </summary>
		[MenuItem("Tools/Utilities/Place the selected object on top of the Zero surface", false, 100)]
		public static void PlaceObjectOnSurface()
		{
			if (!Selection.activeGameObject)
				return;

			Bounds bounds = Utility.GetObjectBounds(Selection.activeGameObject, true);

			Selection.activeGameObject.transform.position = new Vector3();
			Selection.activeGameObject.transform.position = Vector3.up * (bounds.center.y * -1f + bounds.extents.y);

			Debug.Log(Selection.activeGameObject.name + " placed on the surface successfully!", Selection.activeGameObject);
		}
		/// <summary>
		/// Validation method for the Export textures from Texture Array menu item.
		/// Ensures that exactly one Texture2DArray asset is selected before enabling the menu item.
		/// </summary>
		/// <returns>True if exactly one Texture2DArray is selected, false otherwise.</returns>
		[MenuItem("Tools/Utilities/Export textures from Texture Array", true)]
		public static bool Texture2DArrayExportCheck()
		{
			if (Selection.activeObject && Selection.objects.Length == 1)
				return Selection.activeObject is Texture2DArray;

			return false;
		}
		/// <summary>
		/// Menu item that exports all textures from a selected Texture2DArray asset to individual PNG files.
		/// Prompts the user to select a destination folder and saves each texture with an indexed filename.
		/// </summary>
		[MenuItem("Tools/Utilities/Export textures from Texture Array")]
		public static void Texture2DArrayExport()
		{
			if (!Texture2DArrayExportCheck())
				return;

			Texture2DArray array = Selection.activeObject as Texture2DArray;
			Texture2D[] textures = Utility.GetTextureArrayItems(array);
			string path = EditorUtility.SaveFolderPanel("Choose a save destination...", Path.GetDirectoryName(AssetDatabase.GetAssetPath(array)), array.name);

			if (path.IsNullOrEmpty())
				return;

			for (int i = 0; i < textures.Length; i++)
			{
				EditorUtility.DisplayProgressBar("Exporting...", $"{array.name}_{i}", (float)i / textures.Length);
				Utility.SaveTexture2D(textures[i], Utility.TextureEncodingType.PNG, $"{path}/{array.name}_{i}");
			}

			EditorUtility.DisplayProgressBar("Exporting...", "Finishing...", 1f);
			AssetDatabase.Refresh();
			EditorUtility.ClearProgressBar();
		}

		#endregion

		#endregion
	}
}
