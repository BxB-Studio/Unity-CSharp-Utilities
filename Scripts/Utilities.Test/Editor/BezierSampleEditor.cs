﻿#region Namespaces

using Unity.Mathematics;
using UnityEngine;
using UnityEditor;

#endregion

namespace Utilities.Editor
{
	/// <summary>
	/// Custom editor for the BezierSample class.
	/// </summary>
	[CustomEditor(typeof(BezierSample))]
	public class BezierSampleEditor : UnityEditor.Editor
	{
		#region Variables

		#region Static Variables

		/// <summary>
		/// The color of the bezier curve.
		/// </summary>
		public static Color bezierCurveColor = Color.cyan;
		/// <summary>
		/// The color of the bezier curve when it is selected.
		/// </summary>
		public static Color bezierCurveSelectedColor = Color.yellow;
		/// <summary>
		/// The color of the anchor point.
		/// </summary>
		public static Color anchorPointColor = Color.blue;
		/// <summary>
		/// The color of the control point.
		/// </summary>
		public static Color controlPointColor = Color.white;
		/// <summary>
		/// The color of the control point when it is disabled.
		/// </summary>
		public static Color controlPointDisabledColor = Color.gray;
		/// <summary>
		/// The color of the control line.
		/// </summary>
		public static Color controlLineColor = Color.black;
		/// <summary>
		/// The width of the bezier curve.
		/// </summary>
		public static float bezierCurveWidth = 2f;
		/// <summary>
		/// The size of the anchor point.
		/// </summary>
		public static float anchorPointSize = .2f;
		/// <summary>
		/// The size of the control point.
		/// </summary>
		public static float controlPointSize = .05f;

		#endregion

		#region Global Variables

		/// <summary>
		/// The instance of the BezierSample class.
		/// </summary>
		private BezierSample Instance
		{
			get
			{
				if (!instance)
					instance = (BezierSample)target;

				return instance;
			}
		}
		/// <summary>
		/// The instance of the BezierSample class.
		/// </summary>
		private BezierSample instance;
		/// <summary>
		/// The path of the BezierSample class.
		/// </summary>
		private Bezier.Path Path => Instance.path;
		/// <summary>
		/// The selected segment index.
		/// </summary>
		private int selectedSegmentIndex = -1;

		#endregion

		#endregion

		#region Methods

		#region Editor

		/// <summary>
		/// Draws the inspector GUI.
		/// </summary>
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Bezier Path", EditorStyles.boldLabel);

			if (GUILayout.Button("New", EditorStyles.miniButton))
			{
				Undo.RegisterCompleteObjectUndo(Instance, "New Path");
				Instance.CreatePath();
				EditorUtility.SetDirty(Instance);
			}

			if (GUILayout.Button("Reset", EditorStyles.miniButton))
			{
				Undo.RegisterCompleteObjectUndo(Instance, "Reset Path");
				Instance.ResetPath();
				EditorUtility.SetDirty(Instance);
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical(GUI.skin.box);
			EditorGUILayout.LabelField("Editor", EditorStyles.miniBoldLabel);
			EditorGUILayout.Space();
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(GUI.skin.box);
			EditorGUILayout.LabelField("Properties", EditorStyles.miniBoldLabel);

			EditorGUI.indentLevel++;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Loop Path", EditorStyles.miniBoldLabel);

			if (GUILayout.Button("On", Path.LoopedPath ? EditorUtilities.Styles.MiniButtonLeftActive : EditorUtilities.Styles.MiniButtonLeft))
			{
				Undo.RegisterCompleteObjectUndo(Instance, "Toggle Loop");

				Path.LoopedPath = true;

				EditorUtility.SetDirty(Instance);
			}

			if (GUILayout.Button("Off", !Path.LoopedPath ? EditorUtilities.Styles.MiniButtonRightActive : EditorUtilities.Styles.MiniButtonRight))
			{
				Undo.RegisterCompleteObjectUndo(Instance, "Toggle Loop");

				Path.LoopedPath = false;

				EditorUtility.SetDirty(Instance);
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Auto Controls", EditorStyles.miniBoldLabel);

			if (GUILayout.Button("On", Path.AutoCalculateControls ? EditorUtilities.Styles.MiniButtonLeftActive : EditorUtilities.Styles.MiniButtonLeft))
			{
				Undo.RegisterCompleteObjectUndo(Instance, "Toggle Points");

				Path.AutoCalculateControls = true;

				EditorUtility.SetDirty(Instance);
			}

			if (GUILayout.Button("Off", !Path.AutoCalculateControls ? EditorUtilities.Styles.MiniButtonRightActive : EditorUtilities.Styles.MiniButtonRight))
			{
				Undo.RegisterCompleteObjectUndo(Instance, "Toggle Points");

				Path.AutoCalculateControls = false;

				EditorUtility.SetDirty(Instance);
			}

			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel--;

			EditorGUILayout.Space();
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(GUI.skin.box);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Mesh", EditorStyles.miniBoldLabel);

			if (Instance.ShowMesh)
			{
				if (GUILayout.Button("Hide", EditorStyles.miniButton))
				{
					Undo.RegisterCompleteObjectUndo(Instance, "Toggle Visibility");

					Instance.ShowMesh = false;

					EditorUtility.SetDirty(Instance);
				}
			}
			else
			{
				if (GUILayout.Button("Show", EditorStyles.miniButton))
				{
					Undo.RegisterCompleteObjectUndo(Instance, "Toggle Visibility");

					Instance.ShowMesh = true;

					EditorUtility.SetDirty(Instance);
				}
			}

			EditorGUILayout.EndHorizontal();

			if (Instance.ShowMesh)
			{
				EditorGUI.indentLevel++;

				Material newMeshMaterial = EditorGUILayout.ObjectField("Material", Instance.meshMaterial, typeof(Material), false) as Material;
				float newMeshSpacing = math.max(EditorGUILayout.FloatField("Spacing", Instance.meshSpacing), .1f);
				int newMeshResolution = math.max(EditorGUILayout.IntField("Resolution", Instance.meshResolution), 0);
				float newMeshWidth = math.max(EditorGUILayout.FloatField("Width", Instance.meshWidth), 0f);
				float newMeshTiling = math.max(EditorGUILayout.FloatField("Tiling", Instance.meshTiling), 0f);

				if (Instance.meshMaterial != newMeshMaterial || Instance.meshSpacing != newMeshSpacing || Instance.meshResolution != newMeshResolution || Instance.meshWidth != newMeshWidth || Instance.meshTiling != newMeshTiling)
				{
					Undo.RegisterFullObjectHierarchyUndo(Instance, "Bezier Inspector");

					Instance.meshMaterial = newMeshMaterial;
					Instance.meshSpacing = newMeshSpacing;
					Instance.meshResolution = newMeshResolution;
					Instance.meshWidth = newMeshWidth;
					Instance.meshTiling = newMeshTiling;

					EditorUtility.SetDirty(Instance);
				}

				EditorGUI.indentLevel--;

				EditorGUILayout.Space();
			}

			EditorGUILayout.EndVertical();

			if (EditorGUI.EndChangeCheck())
				SceneView.RepaintAll();
		}

		#endregion

		#region Enable & Gizmos

		#region Utilities

		/// <summary>
		/// Draws the bezier gizmos.
		/// </summary>
		private void BezierGizmos()
		{
			Event e = Event.current;

			for (int i = 0; i < Path.SegmentsCount; i++)
			{
				Vector3[] segmentPoints = Path.GetSegmentPoints(i);

				Handles.color = controlLineColor;

				Handles.DrawLine(segmentPoints[1], segmentPoints[0]);
				Handles.DrawLine(segmentPoints[2], segmentPoints[3]);
				Handles.DrawBezier(segmentPoints[0], segmentPoints[3], segmentPoints[1], segmentPoints[2], selectedSegmentIndex == i && e.shift ? bezierCurveSelectedColor : bezierCurveColor, null, bezierCurveWidth);
			}

			for (int i = 0; i < Path.PointsCount; i++)
			{
				Handles.color = Path.IsAnchorPoint(i) ? anchorPointColor : Path.AutoCalculateControls ? Utility.Color.darkGray: controlPointColor;

				Vector3 position;

				if (Path.IsAnchorPoint(i))
					position = Handles.FreeMoveHandle(Path[i],
#if !UNITY_2022_2_OR_NEWER
						Quaternion.identity,
#endif
						anchorPointSize, Vector3.zero, Handles.SphereHandleCap);
				else
					position = Handles.FreeMoveHandle(Path[i],
#if !UNITY_2022_2_OR_NEWER
						Quaternion.identity,
#endif
						controlPointSize, Vector3.zero, Handles.DotHandleCap);

				if (Path[i] != position)
				{
					Undo.RegisterCompleteObjectUndo(Instance, "Move Point");

					position.y = Path[i].y;
					Path[i] = position;

					EditorUtility.SetDirty(Instance);
				}
			}

			if (e.type == EventType.Repaint && Instance.ShowMesh)
				Instance.UpdateMesh();
		}
		/// <summary>
		/// Handles user input.
		/// </summary>
		private void UserInput()
		{
			Event e = Event.current;

			if (e.shift && e.type == EventType.MouseDown)
			{
				if (e.button == 0)
				{
					if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out RaycastHit hit))
					{
						Undo.RegisterCompleteObjectUndo(Instance, "Add Point");

						if (selectedSegmentIndex > -1)
							Path.SplitSegment(hit.point, selectedSegmentIndex);
						else if (!Path.LoopedPath)
							Path.AddSegment(hit.point);

						EditorUtility.SetDirty(Instance);
					}

					GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);

					Event.current.Use();
				}
				
				if (e.button == 1)
					if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out RaycastHit hit))
					{
						int closestAnchorIndex = Path.ClosestAnchorPoint(hit.point, anchorPointSize);

						if (closestAnchorIndex > -1)
						{
							Undo.RegisterCompleteObjectUndo(Instance, "Remove Point");
							Path.RemoveSegment(closestAnchorIndex);
							EditorUtility.SetDirty(Instance);
						}
					}
			}

			if (e.type == EventType.MouseMove)
				if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out RaycastHit hit))
				{
					float distanceFromCurve = bezierCurveWidth * .05f;
					int closestSegmentIndex = -1;

					for (int i = 0; i < Path.SegmentsCount; i++)
					{
						Vector3[] segmentPoints = Path.GetSegmentPoints(i);
						float distance = HandleUtility.DistancePointBezier(hit.point, segmentPoints[0], segmentPoints[3], segmentPoints[1], segmentPoints[2]);

						if (distance < distanceFromCurve)
						{
							closestSegmentIndex = i;
							distanceFromCurve = distance;
						}
					}

					if (selectedSegmentIndex != closestSegmentIndex)
					{
						selectedSegmentIndex = closestSegmentIndex;

						HandleUtility.Repaint();
					}
				}
		}

		#endregion

		/// <summary>
		/// Called when the editor is enabled.
		/// </summary>
		private void OnEnable()
		{
			if (!Path)
				Instance.CreatePath();
		}
		/// <summary>
		/// Called when the scene is drawn.
		/// </summary>
		private void OnSceneGUI()
		{
			UserInput();
			BezierGizmos();
		}

		#endregion

		#endregion
	}
}
