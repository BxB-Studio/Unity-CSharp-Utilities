#region Namespaces

using UnityEngine;

#endregion

namespace Utilities
{
	/// <summary>
	/// Sample class for demonstrating Bezier path creation and manipulation.
	/// </summary>
	public class BezierSample : MonoBehaviour
	{
		#region Variables

		/// <summary>
		/// The Bezier path.
		/// </summary>
		[HideInInspector]
		public Bezier.Path path;
		/// <summary>
		/// The material for the mesh.
		/// </summary>
		public Material meshMaterial;
		/// <summary>
		/// The spacing between the mesh points.
		/// </summary>
		public float meshSpacing = .1f;
		/// <summary>
		/// The resolution of the mesh.
		/// </summary>
		public int meshResolution = 1;
		/// <summary>
		/// The tiling of the mesh.
		/// </summary>
		public float meshTiling = 1f;
		/// <summary>
		/// The width of the mesh.
		/// </summary>
		public float meshWidth = 3f;
		/// <summary>
		/// Whether to show the mesh.
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
		/// Whether to show the mesh.
		/// </summary>
		[SerializeField, HideInInspector]
		private bool showMesh;

		#endregion

		#region Methods

		#region Utilities

		/// <summary>
		/// Creates a new Bezier path.
		/// </summary>
		public void CreatePath()
		{
			path = new Bezier.Path(transform.position, -1);
		}
		/// <summary>
		/// Resets the Bezier path.
		/// </summary>
		public void ResetPath()
		{
			path = new Bezier.Path(-1);
		}
		/// <summary>
		/// Updates the mesh.
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
		/// Resets the Bezier path.
		/// </summary>
		private void Reset()
		{
			ResetPath();
		}

		#endregion

		#endregion
	}
}
