#region Namespaces

using UnityEngine;
using UnityEditor;

#endregion

namespace Utilities.Editor
{
	/// <summary>
	/// Editor window for converting between metric and imperial units.
	/// Provides a user-friendly interface for real-time unit conversion across different measurement types.
	/// </summary>
	public class UnitsConverterEditor : EditorWindow
	{
		#region Variables

		/// <summary>
		/// The current unit type being converted (Distance, Weight, Volume, etc.).
		/// Controls which conversion factors and unit labels are displayed in the interface.
		/// </summary>
		private static Utility.Units unit = Utility.Units.Distance;
		
		/// <summary>
		/// The current value in metric units.
		/// This value is synchronized with the imperial value using the appropriate conversion factor.
		/// </summary>
		private static float metricValue = 1f;
		
		/// <summary>
		/// The current value in imperial units.
		/// This value is synchronized with the metric value using the appropriate conversion factor.
		/// Default is 3.28084 (feet), which is the imperial equivalent of 1 meter.
		/// </summary>
		private static float imperialValue = 3.28084f;

		#endregion

		#region Methods

		#region Static Methods

		/// <summary>
		/// Shows the Units Converter window in the Unity Editor.
		/// Creates a non-resizable window with fixed dimensions and registers it with Unity's window management system.
		/// This method is called when the user selects the menu item "Tools/Utilities/Units Converter".
		/// </summary>
		[MenuItem("Tools/Utilities/Units Converter")]
		public static void ShowWindow()
		{
			UnitsConverterEditor window = GetWindow<UnitsConverterEditor>(true, "Units Converter", true);

			window.minSize = new Vector2(512f, 128f);
			window.maxSize = window.minSize;
		}

		#endregion

		#region Global Methods

		/// <summary>
		/// Draws the GUI for the Units Converter window.
		/// This method is called by Unity for each frame when the window is visible.
		/// Handles the layout of controls, user input processing, and real-time conversion between metric and imperial units.
		/// </summary>
		private void OnGUI()
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Units Converter", EditorStyles.boldLabel);
			EditorGUILayout.Space();

			Utility.Units newUnit = (Utility.Units)EditorGUILayout.EnumPopup("Unit", unit);
			string unitPrefix = newUnit == Utility.Units.Speed || newUnit == Utility.Units.Torque ? string.Empty : "s";

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField($"Metric ({Utility.FullUnit(newUnit, Utility.UnitType.Metric)}{unitPrefix})", EditorStyles.miniBoldLabel);
			EditorGUILayout.LabelField($"Imperial ({Utility.FullUnit(newUnit, Utility.UnitType.Imperial)}{unitPrefix})", EditorStyles.miniBoldLabel);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();

			metricValue = EditorGUILayout.FloatField(imperialValue / Utility.UnitMultiplier(unit, Utility.UnitType.Imperial));

			if (unit != newUnit)
				unit = newUnit;

			imperialValue = EditorGUILayout.FloatField(metricValue * Utility.UnitMultiplier(unit, Utility.UnitType.Imperial));

			EditorGUILayout.EndHorizontal();
		}

		#endregion

		#endregion
	}
}
