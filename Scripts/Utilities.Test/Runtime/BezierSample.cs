#region Namespaces

using UnityEngine;

#endregion

namespace Utilities
{
	/// <summary>
	/// Sample class for demonstrating Bezier path creation and manipulation.
	/// Provides functionality for creating, resetting, and visualizing Bezier paths with mesh generation.
	/// </summary>
	public class BezierSample : MonoBehaviour
	{
		#region Variables

		/// <summary>
		/// The Bezier path object that stores and manages all curve data including control points and segments.
		/// This is the core data structure that defines the shape of the path being edited.
		/// </summary>
		[HideInInspector]
		public Bezier.Path path;
		
		/// <summary>
		/// The material applied to the generated mesh along the Bezier path.
		/// Controls the visual appearance (color, texture, shader) of the path when rendered.
		/// </summary>
		public Material meshMaterial;
		
		/// <summary>
		/// The spacing between points along the Bezier path when generating the mesh.
		/// Lower values create a denser mesh with more vertices and smoother appearance.
		/// </summary>
		public float meshSpacing = .1f;
		
		/// <summary>
		/// The resolution of the generated mesh, controlling the level of detail.
		/// Higher values create more detailed meshes with additional geometry.
		/// </summary>
		public int meshResolution = 1;
		
		/// <summary>
		/// The texture tiling factor for the generated mesh.
		/// Controls how many times textures repeat along the length of the path.
		/// </summary>
		public float meshTiling = 1f;
		
		/// <summary>
		/// The width of the generated mesh perpendicular to the path direction.
		/// Determines how wide the path appears when visualized.
		/// </summary>
		public float meshWidth = 3f;
		
		/// <summary>
		/// Property that controls the visibility of the mesh representation of the Bezier path.
		/// When set to true, generates and displays a mesh along the path.
		/// When set to false, removes any existing mesh visualization.
		/// Automatically updates the mesh when the value changes.
		/// </summary>
		public bool ShowMesh
		{
			get
			{
				return showMesh;
			}
			set
			{
				if (showMesh == value)
					return;

				showMesh = value;

				UpdateMesh();
			}
		}

		/// <summary>
		/// Backing field for the ShowMesh property that stores the current visibility state of the mesh.
		/// Hidden from the Inspector to prevent direct modification, as changes should go through the property.
		/// </summary>
		[SerializeField, HideInInspector]
		private bool showMesh;

		#endregion

		#region Methods

		#region Utilities

		/// <summary>
		/// Creates a new Bezier path starting at the current transform position.
		/// Initializes a path with default settings and positions the first anchor point at the GameObject's location.
		/// The -1 parameter likely refers to an initialization setting for the path (possibly auto-generation of control points).
		/// </summary>
		public void CreatePath()
		{
			path = new Bezier.Path(transform.position, -1);
		}
		
		/// <summary>
		/// Resets the Bezier path to its default state.
		/// Clears all existing points and segments, creating a new empty path.
		/// The -1 parameter likely refers to an initialization setting for the path.
		/// </summary>
		public void ResetPath()
		{
			path = new Bezier.Path(-1);
		}
		
		/// <summary>
		/// Updates the mesh representation of the Bezier path.
		/// Creates or updates a child GameObject with mesh components to visualize the path.
		/// If ShowMesh is true, generates a new mesh based on current path and mesh settings.
		/// If ShowMesh is false, removes any existing mesh visualization.
		/// The mesh GameObject is hidden in the hierarchy to reduce clutter.
		/// </summary>
		public void UpdateMesh()
		{
			Transform meshTransform = transform.Find("Mesh");

			if (!meshTransform)
			{
				meshTransform = new GameObject("Mesh").transform;
				meshTransform.parent = transform;
				meshTransform.gameObject.hideFlags = HideFlags.HideInHierarchy;
			}

			if (ShowMesh)
			{
				MeshFilter filter = meshTransform.GetComponent<MeshFilter>();

				if (!filter)
					filter = meshTransform.gameObject.AddComponent<MeshFilter>();

				filter.mesh = path.CreateMesh(meshWidth, meshSpacing, meshResolution, meshTiling);

				MeshRenderer renderer = meshTransform.GetComponent<MeshRenderer>();

				if (!renderer)
					renderer = meshTransform.gameObject.AddComponent<MeshRenderer>();

				renderer.material = meshMaterial;
			}
			else if (meshTransform)
				Utility.Destroy(true, meshTransform.gameObject);
		}

		#endregion

		#region Reset

		/// <summary>
		/// Unity callback that is called when the script instance is being loaded or reset in the editor.
		/// Automatically resets the Bezier path to its default state when the component is added or reset.
		/// Ensures the path is properly initialized when working with the component in the editor.
		/// </summary>
		private void Reset()
		{
			ResetPath();
		}

		#endregion

		#endregion
	}
}
