#region Namespaces

using UnityEngine;
using UnityEditor;

#endregion

namespace Utilities.Editor
{
	/// <summary>
	/// Editor window for calculating wheel radius based on standard tire size notation.
	/// Provides a user-friendly interface for converting tire specifications (width, aspect ratio, rim diameter)
	/// into actual wheel dimensions in meters.
	/// </summary>
	public class WheelRadiusCalculatorEditor : EditorWindow
	{
		#region Variables

		/// <summary>
		/// The width of the tire in millimeters.
		/// Represents the tire's section width from sidewall to sidewall.
		/// Standard tire sizes typically range from 155mm to 335mm.
		/// </summary>
		private static int width = 225;
		
		/// <summary>
		/// The aspect ratio of the tire as a percentage.
		/// Represents the ratio of the tire's height to its width.
		/// Lower aspect ratios (e.g., 35-55) indicate sportier, lower-profile tires,
		/// while higher values (e.g., 65-80) indicate taller sidewalls.
		/// </summary>
		private static int aspect = 55;
		
		/// <summary>
		/// The diameter of the wheel rim in inches.
		/// Represents the diameter of the metal wheel that the tire mounts onto.
		/// Common values range from 14 to 22 inches for passenger vehicles.
		/// </summary>
		private static int diameter = 17;

		#endregion

		#region Methods

		#region Static Methods

		/// <summary>
		/// Shows the Wheel Radius Calculator window in the Unity Editor.
		/// Creates a non-resizable window with fixed dimensions and registers it with Unity's window management system.
		/// This method is called when the user selects the menu item "Tools/Utilities/Wheel Radius Calculator".
		/// </summary>
		[MenuItem("Tools/Utilities/Wheel Radius Calculator")]
		public static void ShowWindow()
		{
			WheelRadiusCalculatorEditor window = GetWindow<WheelRadiusCalculatorEditor>(true, "Wheel Radius Calculator", true);

			window.minSize = new Vector2(512f, 128f);
			window.maxSize = window.minSize;
		}

		#endregion

		#region Global Methods

		/// <summary>
		/// Draws the GUI for the Wheel Radius Calculator window.
		/// This method is called by Unity for each frame when the window is visible.
		/// Handles the layout of controls for inputting tire specifications (width, aspect ratio, rim diameter)
		/// and displays the calculated wheel diameter and radius in meters.
		/// The calculation follows the standard tire size formula: diameter = (rim diameter in mm + 2 * sidewall height) / 1000,
		/// where sidewall height = (width * aspect ratio) / 100.
		/// </summary>
		private void OnGUI()
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Wheel Radius Calculator", EditorStyles.boldLabel);
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(25f);
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();

			float orgLabelWidth = EditorGUIUtility.labelWidth;

			EditorGUIUtility.labelWidth = 15f;
			width = Mathf.RoundToInt(Utility.ValueWithUnitToNumber(EditorGUILayout.TextField(Utility.NumberToValueWithUnit(width, "mm", true), new GUIStyle(EditorStyles.textField) { alignment = TextAnchor.MiddleCenter })));

			EditorGUILayout.PrefixLabel("/", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });

			aspect = Mathf.Clamp(Mathf.RoundToInt(Utility.ValueWithUnitToNumber(EditorGUILayout.TextField(Utility.NumberToValueWithUnit(aspect, "%", true), new GUIStyle(EditorStyles.textField) { alignment = TextAnchor.MiddleCenter }))), 0, 100);

			EditorGUILayout.PrefixLabel("R", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });

			diameter = Mathf.RoundToInt(Utility.ValueWithUnitToNumber(EditorGUILayout.TextField(Utility.NumberToValueWithUnit(diameter, "in", true), new GUIStyle(EditorStyles.textField) { alignment = TextAnchor.MiddleCenter })));
			EditorGUIUtility.labelWidth = orgLabelWidth;

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			GUILayout.Space(25f);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(100f);
			EditorGUILayout.BeginVertical();

			float wheelDiameter = Utility.ValueWithUnitToNumber(EditorGUILayout.TextField("Diameter", Utility.NumberToValueWithUnit((diameter * 25.4f + 2f * (aspect * width / 100f)) * .001f, "m", 6), new GUIStyle(EditorStyles.textField) { alignment = TextAnchor.MiddleCenter }));

			EditorGUILayout.TextField("Radius", Utility.NumberToValueWithUnit(wheelDiameter * .5f, "m", 6), new GUIStyle(EditorStyles.textField) { alignment = TextAnchor.MiddleCenter });
			EditorGUILayout.EndVertical();
			GUILayout.Space(100f);
			EditorGUILayout.EndHorizontal();
		}

		#endregion

		#endregion
	}
}
