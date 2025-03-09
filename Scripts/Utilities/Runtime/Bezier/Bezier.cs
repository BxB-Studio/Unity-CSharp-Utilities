#region Namespaces

using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Utilities
{
	/// <summary>
	/// Utility class for handling Bezier curves.
	/// </summary>
	public static class Bezier
	{
		#region Modules

		/// <summary>
		/// Represents a Bezier path.
		/// </summary>
		[Serializable]
		public class Path
		{
			#region Variables

			#region Global Variables

			/// <summary>
			/// The number of segments in the path.
			/// </summary>
			public int SegmentsCount => points.Count / 3;
			/// <summary>
			/// The number of points in the path.
			/// </summary>
			public int PointsCount => points.Count;
			/// <summary>
			/// Whether to automatically calculate the controls.
			/// </summary>
			public bool AutoCalculateControls
			{
				get
				{
					return autoCalculateControls;
				}
				set
				{
					if (autoCalculateControls == value)
						return;

					autoCalculateControls = value;

					if (autoCalculateControls)
						AutoSetControls();
				}
			}
			/// <summary>
			/// Whether the path is looped.
			/// </summary>
			public bool LoopedPath
			{
				get
				{
					if (SegmentsCount < 1)
						return false;

					return loopedPath;
				}
				set
				{
					if (SegmentsCount < 1 || loopedPath == value)
						return;

					loopedPath = value;

					if (SegmentsCount < 1)
						return;

					if (loopedPath)
					{
#if UNITY_2021_2_OR_NEWER
						points.Add(points[^1] * 2f - points[^2]);
#else
						points.Add(points[points.Count - 1] * 2f - points[points.Count - 2]);
#endif
						points.Add(points[0] * 2f - points[1]);

						if (AutoCalculateControls)
						{
							AutoSetAnchorControls(0);
							AutoSetAnchorControls(points.Count - 3);
						}
					}
					else if (SegmentsCount > 0)
					{
						points.RemoveRange(points.Count - 2, 2);

						if (AutoCalculateControls)
							AutoSetStartEndControls();
					}
				}
			}
			/// <summary>
			/// The disabled segments in the path.
			/// </summary>
			public List<int> disabledSegments;
			/// <summary>
			/// The ground layer mask.
			/// </summary>
			public LayerMask groundLayerMask;

			/// <summary>
			/// Whether to refresh the points normals.
			/// </summary>
			private bool ShouldRefreshPointsNormals
			{
				get
				{
					return pointsNormals == null || pointsNormals.Count != SegmentsCount + 1;
				}
			}
			/// <summary>
			/// The points in the path.
			/// </summary>
			[SerializeField, HideInInspector]
			private List<Vector3> points;
			/// <summary>
			/// The points normals in the path.
			/// </summary>
			[SerializeField, HideInInspector]
			private List<Vector3> pointsNormals;
			/// <summary>
			/// Whether the path is looped.
			/// </summary>
			[SerializeField, HideInInspector]
			private bool loopedPath;
			/// <summary>
			/// Whether to automatically calculate the controls.
			/// </summary>
			[SerializeField, HideInInspector]
			private bool autoCalculateControls;

			#endregion

			#region Indexers

			/// <summary>
			/// Gets or sets the point at the specified index.
			/// </summary>
			/// <param name="index">The index of the point.</param>
			/// <returns>The point at the specified index.</returns>
			public Vector3 this[int index]
			{
				get
				{
					return points[index];
				}
				set
				{
					if (points[index] == value || AutoCalculateControls && !IsAnchorPoint(index))
						return;

					if (IsAnchorPoint(index))
						SetAnchorPoint(index / 3, value);
					else
					{
						points[index] = value;

						bool nextPointIsAnchor = IsAnchorPoint(index + 1);
						int correspondingControlIndex = nextPointIsAnchor ? index + 2 : index - 2;
						int anchorIndex = nextPointIsAnchor ? index + 1 : index - 1;

						if (correspondingControlIndex > -1 && correspondingControlIndex < points.Count || loopedPath)
						{
							anchorIndex = LoopIndex(anchorIndex);
							correspondingControlIndex = LoopIndex(correspondingControlIndex);

							float controlDistanceFromAnchor = Utility.Distance(points[anchorIndex], points[correspondingControlIndex]);
							Vector3 controlDirectionFromAnchor = Utility.Direction(value, points[anchorIndex]);

							points[correspondingControlIndex] = points[anchorIndex] + controlDirectionFromAnchor * controlDistanceFromAnchor;
						}
					}
				}
			}

			#endregion

			#endregion

			#region Methods

			/// <summary>
			/// Creates a mesh for the path.
			/// </summary>
			/// <param name="width">The width of the mesh.</param>
			/// <param name="spacing">The spacing of the mesh.</param>
			/// <param name="resolution">The resolution of the mesh.</param>
			public Mesh CreateMesh(float width, float spacing, int resolution, float tiling = 1f)
			{
				Vector3[] spacedPoints = GetSpacedPoints(spacing, resolution);

				if (spacedPoints == null || spacedPoints.Length < 2)
					return new Mesh();

				Vector3[] vertices = new Vector3[spacedPoints.Length * 2];
				Vector2[] uv = new Vector2[vertices.Length];
				int trianglesCount = 2 * (spacedPoints.Length - 1) + (loopedPath ? 2 : 0);
				int[] triangles = new int[trianglesCount * 3];
				int vertexIndex = 0;
				int triangleIndex = 0;

				for (int i = 0; i < spacedPoints.Length; i++)
				{
					if ((i < spacedPoints.Length - 1 || loopedPath) && IsSegmentDisabled(ClosestAnchorPoint(spacedPoints[i]) / 3) && Utility.Distance(spacedPoints[i], spacedPoints[(i + 1) % spacedPoints.Length]) > spacing * 2f)
					{
						vertexIndex += 2;
						triangleIndex += 6;

						continue;
					}

					Vector3 forward = Vector3.zero;

					if (i < spacedPoints.Length - 1 || loopedPath)
						forward += Utility.DirectionUnNormalized(spacedPoints[i], spacedPoints[(i + 1) % spacedPoints.Length]);

					if ((i > 0 || loopedPath) && !IsSegmentDisabled(ClosestAnchorPoint(spacedPoints[(i - 1 + spacedPoints.Length) % spacedPoints.Length]) / 3))
						forward += Utility.DirectionUnNormalized(spacedPoints[(i - 1 + spacedPoints.Length) % spacedPoints.Length], spacedPoints[i]);

					forward.Normalize();

					PointTransform transform = new PointTransform(spacedPoints[i], forward);
					Vector3 left = -transform.right;

					vertices[vertexIndex] = spacedPoints[i] + .5f * width * left;
					vertices[vertexIndex + 1] = spacedPoints[i] - .5f * width * left;

					float positionInPath = i / (float)(spacedPoints.Length - 1);
					float newPositionInPath = 1f - Mathf.Abs(2f * positionInPath - 1f);

					uv[vertexIndex] = new Vector2(0, newPositionInPath * spacedPoints.Length * spacing * .05f * tiling);
					uv[vertexIndex + 1] = new Vector2(1, newPositionInPath * spacedPoints.Length * spacing * .05f * tiling);

					if (i < spacedPoints.Length - 1 || loopedPath)
					{
						triangles[triangleIndex] = vertexIndex;
						triangles[triangleIndex + 1] = (vertexIndex + 2) % vertices.Length;
						triangles[triangleIndex + 2] = vertexIndex + 1;
						triangles[triangleIndex + 3] = vertexIndex + 1;
						triangles[triangleIndex + 4] = (vertexIndex + 2) % vertices.Length;
						triangles[triangleIndex + 5] = (vertexIndex + 3) % vertices.Length;
					}

					vertexIndex += 2;
					triangleIndex += 6;
				}

				return new Mesh
				{
					vertices = vertices,
					triangles = triangles,
					uv = uv
				};
			}
			/// <summary>
			/// Estimates the length of the path.
			/// </summary>
			/// <returns>The estimated length of the path.</returns>
			public float EstimatedLength()
			{
				float length = 0f;

				for (int i = 0; i < SegmentsCount; i++)
					length += EstimatedSegmentLength(i);

				return length;
			}
			/// <summary>
			/// Estimates the length of a segment.
			/// </summary>
			/// <param name="index">The index of the segment.</param>
			/// <returns>The estimated length of the segment.</returns>
			public float EstimatedSegmentLength(int index)
			{
				Vector3[] segmentPoints = GetSegmentPoints(index);
				float controlNetLength = Utility.Distance(segmentPoints[0], segmentPoints[1]) + Utility.Distance(segmentPoints[1], segmentPoints[2]) + Utility.Distance(segmentPoints[2], segmentPoints[3]);
				
				return Utility.Distance(segmentPoints[0], segmentPoints[3]) + controlNetLength * .5f;
			}
			/// <summary>
			/// Adds a segment to the path.
			/// </summary>
			/// <param name="position">The position of the segment.</param>
			public void AddSegment(Vector3 position)
			{
				if (points.Count < 1)
				{
					points.Add(position);

					return;
				}
				else if (points.Count == 1)
				{
#if UNITY_2021_2_OR_NEWER
					points.Add(Utility.Average(points[^1], position));
#else
					points.Add(Utility.Average(points[points.Count - 1], position));
#endif
					points.Add(Utility.Average(points[0], position));
				}
				else
				{
#if UNITY_2021_2_OR_NEWER
					points.Add(points[^1] * 2f - points[^2]);
					points.Add(Utility.Average(points[^1], position));
#else
					points.Add(points[points.Count - 1] * 2f - points[points.Count - 2]);
					points.Add(Utility.Average(points[points.Count - 1], position));
#endif
				}

				points.Add(position);

				if (AutoCalculateControls)
					AutoSetMovedAnchorControls(points.Count - 1);

				RefreshAnchorNormals();
			}
			/// <summary>
			/// Splits a segment at the specified position.
			/// </summary>
			/// <param name="position">The position to split the segment at.</param>
			/// <param name="index">The index of the segment to split.</param>
			public void SplitSegment(Vector3 position, int index)
			{
				if (SegmentsCount < 1)
					return;

				points.InsertRange(index * 3 + 2,
					new Vector3[]
					{
						Vector3.zero,
						position,
						Vector3.zero
					}
				);

				if (AutoCalculateControls)
					AutoSetMovedAnchorControls(index * 3 + 3);
				else
					AutoSetAnchorControls(index * 3 + 3);
			}
			/// <summary>
			/// Checks if a segment is disabled.
			/// </summary>
			/// <param name="index">The index of the segment.</param>
			/// <returns>True if the segment is disabled, false otherwise.</returns>
			public bool IsSegmentDisabled(int index)
			{
				return disabledSegments.IndexOf(index) > -1;
			}
			/// <summary>
			/// Enables a segment.
			/// </summary>
			/// <param name="index">The index of the segment.</param>
			public void EnableSegment(int index)
			{
				disabledSegments.Remove(index);
			}
			/// <summary>
			/// Disables a segment.
			/// </summary>
			/// <param name="index">The index of the segment.</param>
			public void DisableSegment(int index)
			{
				if (disabledSegments.IndexOf(index) < 0)
					disabledSegments.Add(index);
			}
			/// <summary>
			/// Removes a segment.
			/// </summary>
			/// <param name="anchorIndex">The index of the anchor of the segment.</param>
			public void RemoveSegment(int anchorIndex)
			{
				if (SegmentsCount < 3 && loopedPath || SegmentsCount < 2)
					return;

				if (anchorIndex == 0)
				{
					if (loopedPath)
#if UNITY_2021_2_OR_NEWER
						points[^1] = points[2];
#else
						points[points.Count - 1] = points[2];
#endif

					points.RemoveRange(0, 3);
				}
				else if (anchorIndex == points.Count - 1 && !loopedPath)
					points.RemoveRange(anchorIndex - 2, 3);
				else
					points.RemoveRange(anchorIndex - 1, 3);
			}
			/// <summary>
			/// Gets the spaced points of the path.
			/// </summary>
			/// <param name="spacing">The spacing of the points.</param>
			/// <param name="pointsNormals">The normals of the points.</param>
			/// <param name="resolution">The resolution of the points.</param>
			/// <returns>The spaced points of the path.</returns>
			public Vector3[] GetSpacedPoints(float spacing, out Vector3[] pointsNormals, int resolution = 1)
			{
				pointsNormals = null;

				if (SegmentsCount < 1)
					return null;

				spacing = math.max(spacing, .1f);
				resolution = math.max(resolution, 1);

				List<Vector3> spacedPoints = new List<Vector3>();
				List<Vector3> newPointsNormals = new List<Vector3>();
				Vector3 lastPoint = points.FirstOrDefault();
				float distanceFromLastPoint = 0f;

				spacedPoints.Add(lastPoint);
				newPointsNormals.Add(GetAnchorPointNormal(0));

				for (int i = 0; i < SegmentsCount; i++)
				{
					Vector3[] segmentPoints = GetSegmentPoints(i);	
					int divisions = Mathf.CeilToInt(EstimatedSegmentLength(i) * resolution * 10f);

					for (float t = 0f; t <= 1f; t += 1f / divisions)
					{
						Vector3 pointOnCurve = EvaluateCubic(segmentPoints[0], segmentPoints[1], segmentPoints[2], segmentPoints[3], t);

						distanceFromLastPoint += Utility.Distance(lastPoint, pointOnCurve);

						while (distanceFromLastPoint >= spacing)
						{
							float overshootDistance = distanceFromLastPoint - spacing;
							Vector3 newPointOnCurve = pointOnCurve + Utility.Direction(pointOnCurve, lastPoint) * overshootDistance;

							if (disabledSegments.IndexOf(i) < 0)
							{
								spacedPoints.Add(newPointOnCurve);
								newPointsNormals.Add(Vector3.Lerp(GetAnchorPointNormal(i), GetAnchorPointNormal(i + 1), t));
							}

							distanceFromLastPoint = overshootDistance;
							lastPoint = newPointOnCurve;
						}

						lastPoint = pointOnCurve;
					}
				}

				pointsNormals = newPointsNormals.ToArray();

				return spacedPoints.ToArray();
			}
			/// <summary>
			/// Gets the spaced points of the path.
			/// </summary>
			/// <param name="spacing">The spacing of the points.</param>
			/// <param name="resolution">The resolution of the points.</param>
			/// <returns>The spaced points of the path.</returns>
			public Vector3[] GetSpacedPoints(float spacing, int resolution = 1)
			{
				return GetSpacedPoints(spacing, out _, resolution);
			}
			/// <summary>
			/// Gets the points of a segment.
			/// </summary>
			/// <param name="index">The index of the segment.</param>
			/// <returns>The points of the segment.</returns>
			public Vector3[] GetSegmentPoints(int index)
			{
				if (SegmentsCount < 1)
					return new Vector3[] { };

				return new Vector3[]
				{
					points[index * 3],
					points[index * 3 + 1],
					points[index * 3 + 2],
					points[LoopIndex(index * 3 + 3)]
				};
			}
			/// <summary>
			/// Gets the closest segment to the specified position.
			/// </summary>
			/// <param name="position">The position to get the closest segment to.</param>
			/// <param name="distanceRange">The distance range to get the closest segment to.</param>
			/// <returns>The closest segment to the specified position.</returns>
			public int ClosestSegment(Vector3 position, float distanceRange)
			{
				if (SegmentsCount < 2)
					return SegmentsCount - 1;

				int closestSegmentIndex = -1;

				for (int i = 0; i < points.Count - 3; i += 3)
				{
					float distanceToFirstAnchor = Utility.Distance(position, points[i]);
					float distanceToSecondAnchor = Utility.Distance(position, points[i + 3]);
					float distanceToAnchors = Utility.Average(distanceToFirstAnchor, distanceToSecondAnchor);

					if (distanceRange > distanceToAnchors)
					{
						distanceRange = distanceToAnchors;
						closestSegmentIndex = i;
					}
				}

				return closestSegmentIndex;
			}
			/// <summary>
			/// Gets the closest segment to the specified position.
			/// </summary>
			/// <param name="position">The position to get the closest segment to.</param>
			/// <returns>The closest segment to the specified position.</returns>
			public int ClosestSegment(Vector3 position)
			{
				return ClosestSegment(position, Mathf.Infinity);
			}
			/// <summary>
			/// Checks if an anchor point is at the specified index.
			/// </summary>
			/// <param name="index">The index of the point.</param>
			/// <returns>True if the point is an anchor point, false otherwise.</returns>
			public bool IsAnchorPoint(int index)
			{
				return index % 3 == 0;
			}
			/// <summary>
			/// Gets the closest anchor point to the specified position.
			/// </summary>
			/// <param name="position">The position to get the closest anchor point to.</param>
			/// <param name="distanceRange">The distance range to get the closest anchor point to.</param>
			/// <returns>The closest anchor point to the specified position.</returns>
			public int ClosestAnchorPoint(Vector3 position, float distanceRange)
			{
				int closestAnchorIndex = -1;

				for (int i = 0; i < points.Count; i += 3)
				{
					float distance = Utility.Distance(points[i], position);

					if (distanceRange > distance)
					{
						distanceRange = distance;
						closestAnchorIndex = i;
					}
				}

				return closestAnchorIndex;
			}
			/// <summary>
			/// Gets the closest anchor point to the specified position.
			/// </summary>
			/// <param name="position">The position to get the closest anchor point to.</param>
			/// <returns>The closest anchor point to the specified position.</returns>
			public int ClosestAnchorPoint(Vector3 position)
			{
				return ClosestAnchorPoint(position, Mathf.Infinity);
			}
			/// <summary>
			/// Gets the anchor point at the specified index.
			/// </summary>
			/// <param name="index">The index of the anchor point.</param>
			/// <returns>The anchor point at the specified index.</returns>
			public Vector3 GetAnchorPoint(int index)
			{
				if (PointsCount < 1)
					return default;

				if (!Application.isPlaying && ShouldRefreshPointsNormals)
					RefreshAnchorNormals();

				return points[LoopIndex(index * 3)];
			}
			/// <summary>
			/// Gets the anchor point normal at the specified index.
			/// </summary>
			/// <param name="index">The index of the anchor point.</param>
			/// <returns>The anchor point normal at the specified index.</returns>
			public Vector3 GetAnchorPointNormal(int index)
			{
				if (PointsCount < 1)
					return Vector3.up;

				if (ShouldRefreshPointsNormals)
					RefreshAnchorNormals();

				return pointsNormals[LoopIndex(index * 3) / 3];
			}
			/// <summary>
			/// Sets the anchor point at the specified index.
			/// </summary>
			/// <param name="index">The index of the anchor point.</param>
			/// <param name="value">The value of the anchor point.</param>
			public void SetAnchorPoint(int index, Vector3 value)
			{
				index = LoopIndex(index * 3);

				Vector3 deltaMove = value - points[index];

				points[index] = value;

				if (ShouldRefreshPointsNormals)
					RefreshAnchorNormals();
				else
					pointsNormals[index / 3] = GetPointNormal(value);

				if (AutoCalculateControls)
				{
					AutoSetMovedAnchorControls(index);

					return;
				}

				if (index + 1 < points.Count || loopedPath)
					points[LoopIndex(index + 1)] += deltaMove;

				if (index - 1 > -1 || loopedPath)
					points[LoopIndex(index - 1)] += deltaMove;
			}
			/// <summary>
			/// Gets the anchor points of the path.
			/// </summary>
			/// <returns>The anchor points of the path.</returns>
			public Vector3[] GetAnchorPoints()
			{
				return points.Where((point, index) => index % 3 == 0).ToArray();
			}
			/// <summary>
			/// Offsets all points of the path.
			/// </summary>
			/// <param name="offset">The offset to offset the points by.</param>
			public void OffsetAllPoints(Vector3 offset)
			{
				for (int i = 0; i < points.Count; i++)
					points[i] += offset;
			}

			/// <summary>
			/// Automatically sets the controls for the path.
			/// </summary>
			private void AutoSetControls()
			{
				for (int i = 0; i < points.Count; i += 3)
					AutoSetAnchorControls(i);

				AutoSetStartEndControls();
			}
			/// <summary>
			/// Automatically sets the controls for a moved anchor point.
			/// </summary>
			/// <param name="anchorIndex">The index of the anchor point.</param>
			private void AutoSetMovedAnchorControls(int anchorIndex)
			{
				if (SegmentsCount < 1)
					return;

				for (int i = anchorIndex - 3; i <= anchorIndex + 3; i += 3)
					if (i > -1 && i < points.Count || loopedPath)
						AutoSetAnchorControls(LoopIndex(i));

				AutoSetStartEndControls();
			}
			/// <summary>
			/// Automatically sets the controls for an anchor point.
			/// </summary>
			/// <param name="anchorIndex">The index of the anchor point.</param>
			private void AutoSetAnchorControls(int anchorIndex)
			{
				if (SegmentsCount < 1)
					return;

				Vector3 anchorPosition = points[anchorIndex];
				Vector3 direction = Vector3.zero;
				float[] neighborDistances = new float[2];

				if (anchorIndex - 3 >= 0 || loopedPath)
				{
					Vector3 offset = points[LoopIndex(anchorIndex - 3)] - anchorPosition;

					direction += offset.normalized;
					neighborDistances[0] = offset.magnitude;
				}

				if (anchorIndex + 3 >= 0 || loopedPath)
				{
					Vector3 offset = points[LoopIndex(anchorIndex + 3)] - anchorPosition;

					direction -= offset.normalized;
					neighborDistances[1] = -offset.magnitude;
				}

				direction.Normalize();

				for (int i = 0; i < 2; i++)
				{
					int controlIndex = anchorIndex + i * 2 - 1;

					if (controlIndex > -1 && controlIndex < points.Count || loopedPath)
						points[LoopIndex(controlIndex)] = anchorPosition + .5f * neighborDistances[i] * direction;
				}
			}
			/// <summary>
			/// Automatically sets the controls for the start and end of the path.
			/// </summary>
			private void AutoSetStartEndControls()
			{
				if (loopedPath || SegmentsCount < 1)
					return;

				points[1] = Utility.Average(points[0], points[2]);
#if UNITY_2021_2_OR_NEWER
				points[^2] = Utility.Average(points[^1], points[^3]);
#else
				points[points.Count - 2] = Utility.Average(points[points.Count - 1], points[points.Count - 3]);
#endif
			}
			/// <summary>
			/// Refreshes the anchor normals.
			/// </summary>
			private void RefreshAnchorNormals()
			{
				pointsNormals = new List<Vector3>();

				if (SegmentsCount < 1)
					return;

				Vector3[] segmentPoints = GetSegmentPoints(0);

				pointsNormals.Add(GetPointNormal(segmentPoints[0]));
				pointsNormals.Add(GetPointNormal(segmentPoints[3]));

				for (int i = 1; i < SegmentsCount; i++)
					pointsNormals.Add(GetPointNormal(GetSegmentPoints(i)[3]));
			}
			/// <summary>
			/// Gets the normal of a point.
			/// </summary>
			/// <param name="point">The point to get the normal of.</param>
			/// <returns>The normal of the point.</returns>
			private Vector3 GetPointNormal(Vector3 point)
			{
				RaycastHit[] hits = new RaycastHit[2];

				Physics.Raycast(point + Vector3.up, Vector3.down, out hits[0], 10f, groundLayerMask, QueryTriggerInteraction.Ignore);
				Physics.Raycast(point + Vector3.down, Vector3.up, out hits[1], 10f, groundLayerMask, QueryTriggerInteraction.Ignore);

				if (hits[0].collider && hits[1].collider)
					return hits[0].distance > hits[1].distance ? -hits[1].normal : hits[0].normal;
				else if (hits[0].collider)
					return hits[0].normal;
				else if (hits[1].collider)
					return -hits[1].normal;

				return Vector3.up;
			}
			/// <summary>
			/// Loops an index.
			/// </summary>
			/// <param name="index">The index to loop.</param>
			/// <returns>The looped index.</returns>
			private int LoopIndex(int index)
			{
				while (index < 0)
					index += PointsCount;

				return index % PointsCount;
			}

			#endregion

			#region Constructors & Operators

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the Path class.
			/// </summary>
			/// <param name="groundLayerMask">The ground layer mask.</param>
			public Path(LayerMask groundLayerMask)
			{
				points = new List<Vector3>();
				disabledSegments = new List<int>();
				this.groundLayerMask = groundLayerMask;
			}
			/// <summary>
			/// Initializes a new instance of the Path class.
			/// </summary>
			/// <param name="center">The center of the path.</param>
			/// <param name="groundLayerMask">The ground layer mask.</param>
			public Path(Vector3 center, LayerMask groundLayerMask)
			{
				points = new List<Vector3>
				{
					center + Vector3.back,
					center + (Vector3.back + Vector3.left) * .5f,
					center + (Vector3.forward + Vector3.right) * .5f,
					center + Vector3.forward
				};
				disabledSegments = new List<int>();
				this.groundLayerMask = groundLayerMask;
			}

			#endregion

			#region Operators

			/// <summary>
			/// Implicitly converts a Path to a boolean.
			/// </summary>
			/// <param name="path">The Path to convert.</param>
			/// <returns>True if the Path is not null, false otherwise.</returns>
			public static implicit operator bool(Path path) => path != null;

			#endregion

			#endregion
		}

		/// <summary>
		/// Represents a point transform.
		/// </summary>
		private struct PointTransform
		{
			#region Variables

			/// <summary>
			/// The position of the point.
			/// </summary>
			public Vector3 position;
			/// <summary>
			/// The rotation of the point.
			/// </summary>
			public Quaternion rotation;
			/// <summary>
			/// The forward direction of the point.
			/// </summary>
			public Vector3 forward;
			/// <summary>
			/// The normal of the point.
			/// </summary>
			public Vector3 normal;
			/// <summary>
			/// The right direction of the point.
			/// </summary>
			public Vector3 right;

			#endregion

			#region Constructors

			public PointTransform(Vector3 point, Vector3 forward)
			{
				position = point;
				rotation = Quaternion.LookRotation(forward);

				Transform transform = new GameObject().transform;

				transform.SetPositionAndRotation(position, rotation);

				this.forward = transform.forward;
				normal = transform.up;
				right = transform.right;

				Utility.Destroy(true, transform.gameObject);
			}

			#endregion
		}

		#endregion

		#region Methods

		/// <summary>
		/// Evaluates a linear Bezier curve.
		/// </summary>
		/// <param name="a">The start point.</param>
		/// <param name="b">The end point.</param>
		/// <param name="t">The interpolation factor.</param>
		/// <returns>The evaluated point.</returns>
		public static Vector3 EvaluateLinear(Vector3 a, Vector3 b, float t)
		{
			return Vector3.Lerp(a, b, t);
		}
		/// <summary>
		/// Evaluates a quadratic Bezier curve.
		/// </summary>
		/// <param name="a">The start point.</param>
		/// <param name="b">The control point.</param>
		/// <param name="c">The end point.</param>
		/// <param name="t">The interpolation factor.</param>
		/// <returns>The evaluated point.</returns>
		public static Vector3 EvaluateQuadratic(Vector3 a, Vector3 b, Vector3 c, float t)
		{
			Vector3 p0 = EvaluateLinear(a, b, t);
			Vector3 p1 = EvaluateLinear(b, c, t);

			return Vector3.Lerp(p0, p1, t);
		}
		/// <summary>
		/// Evaluates a cubic Bezier curve.
		/// </summary>
		/// <param name="a">The start point.</param>
		/// <param name="b">The first control point.</param>
		/// <param name="c">The second control point.</param>
		/// <param name="d">The end point.</param>
		/// <param name="t">The interpolation factor.</param>
		/// <returns>The evaluated point.</returns>
		public static Vector3 EvaluateCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
		{
			Vector3 p0 = EvaluateQuadratic(a, b, c, t);
			Vector3 p1 = EvaluateQuadratic(b, c, d, t);

			return Vector3.Lerp(p0, p1, t);
		}
		
		#endregion
	}
}
