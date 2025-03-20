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
	/// Utility class for handling Bezier curves, providing methods for curve evaluation and path management.
	/// </summary>
	public static class Bezier
	{
		#region Modules

		/// <summary>
		/// Represents a Bezier path composed of multiple connected cubic Bezier curve segments.
		/// Provides functionality for creating, manipulating, and rendering paths with support for
		/// looping, automatic control point calculation, and ground alignment.
		/// </summary>
		[Serializable]
		public class Path
		{
			#region Variables

			#region Global Variables

			/// <summary>
			/// Gets the number of Bezier curve segments in the path.
			/// Each segment consists of 4 points (anchor-control-control-anchor), with each segment
			/// sharing an anchor point with adjacent segments, resulting in 3 points per segment.
			/// </summary>
			public int SegmentsCount => points.Count / 3;
			
			/// <summary>
			/// Gets the total number of points in the path, including both anchor and control points.
			/// For a path with n segments, there are 3n+1 total points.
			/// </summary>
			public int PointsCount => points.Count;
			
			/// <summary>
			/// Gets or sets whether control points are automatically calculated based on anchor positions.
			/// When enabled, control points are positioned to create smooth curves between anchor points.
			/// When disabled, control points can be manually positioned.
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
			/// Gets or sets whether the path forms a closed loop.
			/// When enabled, the last anchor point connects back to the first anchor point,
			/// creating additional control points to ensure a smooth transition.
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
			/// List of segment indices that should not be rendered or included in path calculations.
			/// Disabled segments are skipped when generating meshes or calculating path points.
			/// </summary>
			public List<int> disabledSegments;
			
			/// <summary>
			/// The layer mask used for ground detection when calculating point normals.
			/// Used to align path points with the terrain or other surfaces.
			/// </summary>
			public LayerMask groundLayerMask;

			/// <summary>
			/// Determines whether the point normals need to be recalculated.
			/// Returns true if the normals array is null or doesn't match the current number of segments.
			/// </summary>
			private bool ShouldRefreshPointsNormals
			{
				get
				{
					return pointsNormals == null || pointsNormals.Count != SegmentsCount + 1;
				}
			}
			
			/// <summary>
			/// The collection of all points in the path, including both anchor and control points.
			/// For each segment, points are stored in the order: anchor, control, control, anchor (with the last anchor
			/// being shared with the next segment).
			/// </summary>
			[SerializeField, HideInInspector]
			private List<Vector3> points;
			
			/// <summary>
			/// The surface normals at each anchor point, used for aligning the path with the ground.
			/// These are calculated by raycasting against the ground layer mask.
			/// </summary>
			[SerializeField, HideInInspector]
			private List<Vector3> pointsNormals;
			
			/// <summary>
			/// Internal flag indicating whether the path forms a closed loop.
			/// When true, the last point connects back to the first point.
			/// </summary>
			[SerializeField, HideInInspector]
			private bool loopedPath;
			
			/// <summary>
			/// Internal flag indicating whether control points are automatically calculated.
			/// When true, control points are positioned to create smooth curves between anchor points.
			/// </summary>
			[SerializeField, HideInInspector]
			private bool autoCalculateControls;

			#endregion

			#region Indexers

			/// <summary>
			/// Gets or sets the point at the specified index in the path.
			/// Handles special logic for anchor points versus control points, maintaining
			/// symmetry of control points around anchors when appropriate.
			/// </summary>
			/// <param name="index">The index of the point to access.</param>
			/// <returns>The position of the point at the specified index.</returns>
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
			/// Creates a mesh representation of the path with specified width and density.
			/// The mesh is created by generating vertices along the path at regular intervals,
			/// with the width determining the distance between left and right edges.
			/// </summary>
			/// <param name="width">The width of the path mesh in world units.</param>
			/// <param name="spacing">The distance between points along the path.</param>
			/// <param name="resolution">The resolution multiplier for curve sampling.</param>
			/// <param name="tiling">The UV tiling factor for texture mapping.</param>
			/// <returns>A Unity Mesh object representing the path.</returns>
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
			/// Estimates the total length of the path by summing the estimated lengths of all segments.
			/// This provides a reasonably accurate approximation without the computational cost of
			/// precise curve integration.
			/// </summary>
			/// <returns>The estimated length of the entire path in world units.</returns>
			public float EstimatedLength()
			{
				float length = 0f;

				for (int i = 0; i < SegmentsCount; i++)
					length += EstimatedSegmentLength(i);

				return length;
			}
			
			/// <summary>
			/// Estimates the length of a specific segment in the path.
			/// Uses a heuristic that combines the direct distance between endpoints with
			/// the length of the control net to approximate the true curve length.
			/// </summary>
			/// <param name="index">The index of the segment to measure.</param>
			/// <returns>The estimated length of the segment in world units.</returns>
			public float EstimatedSegmentLength(int index)
			{
				Vector3[] segmentPoints = GetSegmentPoints(index);
				float controlNetLength = Utility.Distance(segmentPoints[0], segmentPoints[1]) + Utility.Distance(segmentPoints[1], segmentPoints[2]) + Utility.Distance(segmentPoints[2], segmentPoints[3]);
				
				return Utility.Distance(segmentPoints[0], segmentPoints[3]) + controlNetLength * .5f;
			}
			
			/// <summary>
			/// Adds a new segment to the end of the path at the specified position.
			/// Creates necessary control points based on existing path geometry.
			/// For the first point, simply adds a single anchor. For the second point,
			/// creates a simple segment with appropriate controls.
			/// </summary>
			/// <param name="position">The world position of the new anchor point.</param>
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
			/// Splits an existing segment into two segments at the specified position.
			/// Inserts a new anchor point and corresponding control points, maintaining
			/// the overall shape of the curve while adding more control.
			/// </summary>
			/// <param name="position">The world position where the segment should be split.</param>
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
			/// Checks if a specific segment is marked as disabled.
			/// Disabled segments are excluded from rendering and certain path calculations.
			/// </summary>
			/// <param name="index">The index of the segment to check.</param>
			/// <returns>True if the segment is disabled, false otherwise.</returns>
			public bool IsSegmentDisabled(int index)
			{
				return disabledSegments.IndexOf(index) > -1;
			}
			
			/// <summary>
			/// Enables a previously disabled segment, allowing it to be rendered and included in path calculations.
			/// </summary>
			/// <param name="index">The index of the segment to enable.</param>
			public void EnableSegment(int index)
			{
				disabledSegments.Remove(index);
			}
			
			/// <summary>
			/// Disables a segment, preventing it from being rendered or included in path calculations.
			/// </summary>
			/// <param name="index">The index of the segment to disable.</param>
			public void DisableSegment(int index)
			{
				if (disabledSegments.IndexOf(index) < 0)
					disabledSegments.Add(index);
			}
			
			/// <summary>
			/// Removes a segment from the path by removing its anchor point and associated control points.
			/// Handles special cases for removing the first or last segment, and prevents removing
			/// segments when it would make the path too short for its configuration.
			/// </summary>
			/// <param name="anchorIndex">The index of the anchor point of the segment to remove.</param>
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
			/// Gets evenly spaced points along the path with corresponding surface normals.
			/// Samples the Bezier curves at regular intervals to create points that are approximately
			/// equidistant, which is useful for consistent path following and visualization.
			/// </summary>
			/// <param name="spacing">The desired distance between adjacent points.</param>
			/// <param name="pointsNormals">Output array of surface normals corresponding to each point.</param>
			/// <param name="resolution">The resolution multiplier for curve sampling.</param>
			/// <returns>An array of evenly spaced points along the path.</returns>
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
			/// Gets evenly spaced points along the path without returning surface normals.
			/// A convenience overload of the full method when normals aren't needed.
			/// </summary>
			/// <param name="spacing">The desired distance between adjacent points.</param>
			/// <param name="resolution">The resolution multiplier for curve sampling.</param>
			/// <returns>An array of evenly spaced points along the path.</returns>
			public Vector3[] GetSpacedPoints(float spacing, int resolution = 1)
			{
				return GetSpacedPoints(spacing, out _, resolution);
			}
			
			/// <summary>
			/// Gets the four control points that define a specific Bezier curve segment.
			/// Returns the anchor points and control points in the order needed for curve evaluation.
			/// </summary>
			/// <param name="index">The index of the segment to retrieve.</param>
			/// <returns>An array of four Vector3 points defining the cubic Bezier curve segment.</returns>
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
			/// Finds the index of the segment closest to the specified position within a maximum distance.
			/// Useful for selecting segments for editing or interaction.
			/// </summary>
			/// <param name="position">The position to find the closest segment to.</param>
			/// <param name="distanceRange">The maximum distance to consider.</param>
			/// <returns>The index of the closest segment, or -1 if none is within range.</returns>
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
			/// Finds the index of the segment closest to the specified position with no distance limit.
			/// A convenience overload of the full method when no maximum distance is needed.
			/// </summary>
			/// <param name="position">The position to find the closest segment to.</param>
			/// <returns>The index of the closest segment.</returns>
			public int ClosestSegment(Vector3 position)
			{
				return ClosestSegment(position, Mathf.Infinity);
			}
			
			/// <summary>
			/// Determines if a point at the specified index is an anchor point.
			/// Anchor points are the main control points of the path, occurring every third index.
			/// </summary>
			/// <param name="index">The index of the point to check.</param>
			/// <returns>True if the point is an anchor point, false if it's a control point.</returns>
			public bool IsAnchorPoint(int index)
			{
				return index % 3 == 0;
			}
			
			/// <summary>
			/// Finds the index of the anchor point closest to the specified position within a maximum distance.
			/// Useful for selecting anchor points for editing or interaction.
			/// </summary>
			/// <param name="position">The position to find the closest anchor point to.</param>
			/// <param name="distanceRange">The maximum distance to consider.</param>
			/// <returns>The index of the closest anchor point, or -1 if none is within range.</returns>
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
			/// Finds the index of the anchor point closest to the specified position with no distance limit.
			/// A convenience overload of the full method when no maximum distance is needed.
			/// </summary>
			/// <param name="position">The position to find the closest anchor point to.</param>
			/// <returns>The index of the closest anchor point.</returns>
			public int ClosestAnchorPoint(Vector3 position)
			{
				return ClosestAnchorPoint(position, Mathf.Infinity);
			}
			
			/// <summary>
			/// Gets the position of the anchor point at the specified segment index.
			/// Handles index wrapping for looped paths and ensures normals are up to date.
			/// </summary>
			/// <param name="index">The segment index of the anchor point.</param>
			/// <returns>The position of the anchor point.</returns>
			public Vector3 GetAnchorPoint(int index)
			{
				if (PointsCount < 1)
					return default;

				if (!Application.isPlaying && ShouldRefreshPointsNormals)
					RefreshAnchorNormals();

				return points[LoopIndex(index * 3)];
			}
			
			/// <summary>
			/// Gets the surface normal at the anchor point of the specified segment index.
			/// Used for aligning objects to the path's underlying surface.
			/// </summary>
			/// <param name="index">The segment index of the anchor point.</param>
			/// <returns>The surface normal at the anchor point.</returns>
			public Vector3 GetAnchorPointNormal(int index)
			{
				if (PointsCount < 1)
					return Vector3.up;

				if (ShouldRefreshPointsNormals)
					RefreshAnchorNormals();

				return pointsNormals[LoopIndex(index * 3) / 3];
			}
			
			/// <summary>
			/// Sets the position of an anchor point and updates associated control points.
			/// When auto-calculate is enabled, control points are automatically adjusted.
			/// Otherwise, control points are moved along with the anchor point.
			/// </summary>
			/// <param name="index">The segment index of the anchor point.</param>
			/// <param name="value">The new position for the anchor point.</param>
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
			/// Gets an array of all anchor points in the path.
			/// Useful for visualizing or processing just the main points without control points.
			/// </summary>
			/// <returns>An array containing all anchor points in the path.</returns>
			public Vector3[] GetAnchorPoints()
			{
				return points.Where((point, index) => index % 3 == 0).ToArray();
			}
			
			/// <summary>
			/// Moves the entire path by adding an offset to all points.
			/// Preserves the shape of the path while changing its position.
			/// </summary>
			/// <param name="offset">The vector to add to all point positions.</param>
			public void OffsetAllPoints(Vector3 offset)
			{
				for (int i = 0; i < points.Count; i++)
					points[i] += offset;
			}

			/// <summary>
			/// Automatically calculates and sets all control points for the entire path.
			/// Creates smooth curves between anchor points based on their positions.
			/// </summary>
			private void AutoSetControls()
			{
				for (int i = 0; i < points.Count; i += 3)
					AutoSetAnchorControls(i);

				AutoSetStartEndControls();
			}
			
			/// <summary>
			/// Recalculates control points for an anchor that has been moved.
			/// Updates control points for the moved anchor and adjacent anchors to maintain curve smoothness.
			/// </summary>
			/// <param name="anchorIndex">The index of the moved anchor point.</param>
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
			/// Calculates and sets the control points for a specific anchor point.
			/// Positions control points to create smooth transitions between segments,
			/// taking into account the positions of adjacent anchor points.
			/// The control points are positioned along the direction vector between neighboring anchors,
			/// with their distance proportional to the distance between anchors.
			/// </summary>
			/// <param name="anchorIndex">The index of the anchor point to calculate controls for.</param>
			private void AutoSetAnchorControls(int anchorIndex)
			{
				if (SegmentsCount < 1)
					return;

				Vector3 anchorPosition = points[anchorIndex];
				Vector3 direction = Vector3.zero;
				float[] neighborDistances = new float[2];

				// Calculate influence from previous anchor point
				if (anchorIndex - 3 >= 0 || loopedPath)
				{
					Vector3 offset = points[LoopIndex(anchorIndex - 3)] - anchorPosition;

					direction += offset.normalized;
					neighborDistances[0] = offset.magnitude;
				}

				// Calculate influence from next anchor point
				if (anchorIndex + 3 >= 0 || loopedPath)
				{
					Vector3 offset = points[LoopIndex(anchorIndex + 3)] - anchorPosition;

					direction -= offset.normalized;
					neighborDistances[1] = -offset.magnitude;
				}

				direction.Normalize();

				// Set the two control points for this anchor
				for (int i = 0; i < 2; i++)
				{
					int controlIndex = anchorIndex + i * 2 - 1;

					if (controlIndex > -1 && controlIndex < points.Count || loopedPath)
						points[LoopIndex(controlIndex)] = anchorPosition + .5f * neighborDistances[i] * direction;
				}
			}

			/// <summary>
			/// Automatically sets the control points for the start and end anchors of an open path.
			/// For the first anchor, the control point is set to the average position between the anchor and its next control.
			/// For the last anchor, the control point is set to the average position between the anchor and its previous control.
			/// This creates a more natural curve at the endpoints of the path.
			/// This method has no effect if the path is looped or has no segments.
			/// </summary>
			private void AutoSetStartEndControls()
			{
				if (loopedPath || SegmentsCount < 1)
					return;

				// Set first control point (index 1) to average of first anchor and second control
				points[1] = Utility.Average(points[0], points[2]);

				// Set last control point to average of last anchor and second-to-last control
#if UNITY_2021_2_OR_NEWER
				points[^2] = Utility.Average(points[^1], points[^3]);
#else
				points[points.Count - 2] = Utility.Average(points[points.Count - 1], points[points.Count - 3]);
#endif
			}

			/// <summary>
			/// Recalculates and updates the normal vectors for all anchor points in the path.
			/// Normal vectors represent the up direction at each anchor point and are used for
			/// orienting objects along the path or aligning the path with the ground.
			/// This method clears existing normals and calculates new ones based on the current
			/// anchor point positions and ground layer mask.
			/// </summary>
			private void RefreshAnchorNormals()
			{
				pointsNormals = new List<Vector3>();

				if (SegmentsCount < 1)
					return;

				// Get the first segment's points and calculate normals for its endpoints
				Vector3[] segmentPoints = GetSegmentPoints(0);

				pointsNormals.Add(GetPointNormal(segmentPoints[0]));
				pointsNormals.Add(GetPointNormal(segmentPoints[3]));

				// Calculate normals for the end points of all remaining segments
				for (int i = 1; i < SegmentsCount; i++)
					pointsNormals.Add(GetPointNormal(GetSegmentPoints(i)[3]));
			}

			/// <summary>
			/// Determines the normal vector (up direction) for a given point in world space.
			/// Uses raycasting in both up and down directions to find the nearest ground surface.
			/// If ground is detected, returns the surface normal. If ground is detected in both
			/// directions, returns the normal of the closest surface. If no ground is detected,
			/// defaults to world up vector.
			/// </summary>
			/// <param name="point">The world position to calculate the normal for.</param>
			/// <returns>The normal vector at the specified point, representing the up direction.</returns>
			private Vector3 GetPointNormal(Vector3 point)
			{
				RaycastHit[] hits = new RaycastHit[2];

				// Cast rays in both directions to find ground
				Physics.Raycast(point + Vector3.up, Vector3.down, out hits[0], 10f, groundLayerMask, QueryTriggerInteraction.Ignore);
				Physics.Raycast(point + Vector3.down, Vector3.up, out hits[1], 10f, groundLayerMask, QueryTriggerInteraction.Ignore);

				// Determine which hit to use based on what was detected
				if (hits[0].collider && hits[1].collider)
					return hits[0].distance > hits[1].distance ? -hits[1].normal : hits[0].normal;
				else if (hits[0].collider)
					return hits[0].normal;
				else if (hits[1].collider)
					return -hits[1].normal;

				// Default to world up if no ground was detected
				return Vector3.up;
			}

			/// <summary>
			/// Wraps an index to ensure it stays within the valid range of points in the path.
			/// For looped paths, this allows negative indices or indices beyond the array bounds
			/// to wrap around, treating the path as a continuous loop.
			/// </summary>
			/// <param name="index">The index to wrap/loop.</param>
			/// <returns>
			/// A valid index within the range [0, PointsCount-1] that corresponds to the
			/// equivalent position in the path.
			/// </returns>
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
			/// Initializes a new empty Bezier path with no points.
			/// Creates the necessary collections for storing path data and sets the ground layer mask
			/// used for normal calculations.
			/// </summary>
			/// <param name="groundLayerMask">The layer mask used for ground detection when calculating point normals.</param>
			public Path(LayerMask groundLayerMask)
			{
				points = new List<Vector3>();
				disabledSegments = new List<int>();
				this.groundLayerMask = groundLayerMask;
			}

			/// <summary>
			/// Initializes a new Bezier path with a single segment centered at the specified position.
			/// Creates a simple path with two anchor points (one at center+back and one at center+forward)
			/// and appropriate control points to form a smooth curve.
			/// </summary>
			/// <param name="center">The center point around which to create the initial path segment.</param>
			/// <param name="groundLayerMask">The layer mask used for ground detection when calculating point normals.</param>
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
			/// Implicitly converts a Path to a boolean value.
			/// Allows Path objects to be used directly in conditional statements,
			/// returning true if the Path reference is not null.
			/// </summary>
			/// <param name="path">The Path object to convert.</param>
			/// <returns>True if the Path is not null, false otherwise.</returns>
			public static implicit operator bool(Path path) => path != null;

			#endregion

			#endregion
		}

		/// <summary>
		/// Represents the transformation data for a point along a Bezier curve.
		/// Stores position, rotation, and orientation vectors (forward, normal, right)
		/// that define a complete coordinate system at a specific point on the curve.
		/// Used for positioning and orienting objects that follow the curve.
		/// </summary>
		private struct PointTransform
		{
			#region Variables

			/// <summary>
			/// The world position of the point on the curve.
			/// </summary>
			public Vector3 position;

			/// <summary>
			/// The rotation of the coordinate system at this point.
			/// Determined by the forward direction and the up vector (normal).
			/// </summary>
			public Quaternion rotation;

			/// <summary>
			/// The forward direction vector at this point.
			/// Typically represents the tangent direction of the curve.
			/// </summary>
			public Vector3 forward;

			/// <summary>
			/// The up direction vector at this point.
			/// Typically represents the normal to the surface or the desired up orientation.
			/// </summary>
			public Vector3 normal;

			/// <summary>
			/// The right direction vector at this point.
			/// Calculated as the cross product of forward and normal vectors.
			/// </summary>
			public Vector3 right;

			#endregion

			#region Constructors

			/// <summary>
			/// Creates a new PointTransform with the specified position and forward direction.
			/// Calculates a complete coordinate system (forward, up, right) based on the given forward vector.
			/// Uses a temporary GameObject to compute the correct orientation vectors.
			/// </summary>
			/// <param name="point">The position of the point in world space.</param>
			/// <param name="forward">The forward direction at this point.</param>
			public PointTransform(Vector3 point, Vector3 forward)
			{
				position = point;
				rotation = Quaternion.LookRotation(forward);

				// Create a temporary transform to calculate the orientation vectors
				Transform transform = new GameObject().transform;

				transform.SetPositionAndRotation(position, rotation);

				this.forward = transform.forward;
				normal = transform.up;
				right = transform.right;

				// Clean up the temporary GameObject
				Utility.Destroy(true, transform.gameObject);
			}

			#endregion
		}

		#endregion

		#region Methods

		/// <summary>
		/// Evaluates a linear Bezier curve at the specified interpolation factor.
		/// A linear Bezier curve is simply a straight line between two points.
		/// </summary>
		/// <param name="a">The start point of the curve.</param>
		/// <param name="b">The end point of the curve.</param>
		/// <param name="t">The interpolation factor in the range [0,1], where 0 returns point a and 1 returns point b.</param>
		/// <returns>The interpolated point on the linear curve.</returns>
		public static Vector3 EvaluateLinear(Vector3 a, Vector3 b, float t)
		{
			return Vector3.Lerp(a, b, t);
		}

		/// <summary>
		/// Evaluates a quadratic Bezier curve at the specified interpolation factor.
		/// A quadratic Bezier curve is defined by two endpoints and one control point.
		/// </summary>
		/// <param name="a">The start point of the curve.</param>
		/// <param name="b">The control point that influences the curve shape.</param>
		/// <param name="c">The end point of the curve.</param>
		/// <param name="t">The interpolation factor in the range [0,1], where 0 returns point a and 1 returns point c.</param>
		/// <returns>The interpolated point on the quadratic curve.</returns>
		public static Vector3 EvaluateQuadratic(Vector3 a, Vector3 b, Vector3 c, float t)
		{
			Vector3 p0 = EvaluateLinear(a, b, t);
			Vector3 p1 = EvaluateLinear(b, c, t);

			return Vector3.Lerp(p0, p1, t);
		}

		/// <summary>
		/// Evaluates a cubic Bezier curve at the specified interpolation factor.
		/// A cubic Bezier curve is defined by two endpoints and two control points,
		/// allowing for more complex curve shapes than linear or quadratic curves.
		/// </summary>
		/// <param name="a">The start point of the curve.</param>
		/// <param name="b">The first control point that influences the curve shape from the start.</param>
		/// <param name="c">The second control point that influences the curve shape from the end.</param>
		/// <param name="d">The end point of the curve.</param>
		/// <param name="t">The interpolation factor in the range [0,1], where 0 returns point a and 1 returns point d.</param>
		/// <returns>The interpolated point on the cubic curve.</returns>
		public static Vector3 EvaluateCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
		{
			Vector3 p0 = EvaluateQuadratic(a, b, c, t);
			Vector3 p1 = EvaluateQuadratic(b, c, d, t);

			return Vector3.Lerp(p0, p1, t);
		}
		
		#endregion
	}
}
