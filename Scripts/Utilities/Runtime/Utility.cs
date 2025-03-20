#region Namespaces

using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

#endregion

namespace Utilities
{
	/// <summary>
	/// Provides extension methods for various Unity and .NET types.
	/// These extensions enhance the functionality of common types like Transform, AnimationCurve, and string
	/// with additional utility methods that simplify common programming tasks.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Finds a direct child of the transform with the specified name.
		/// Performs a case-sensitive or case-insensitive comparison based on the caseSensitive parameter.
		/// Only searches immediate children, not descendants further down the hierarchy.
		/// </summary>
		/// <param name="transform">The transform to search in.</param>
		/// <param name="name">The name of the child to find.</param>
		/// <param name="caseSensitive">Whether the name comparison should be case-sensitive. Defaults to true.</param>
		/// <returns>The found child transform, or null if no matching child was found.</returns>
		public static Transform Find(this Transform transform, string name, bool caseSensitive = true)
		{
			for (int i = 0; i < transform.childCount; i++)
				if (!caseSensitive && transform.GetChild(i).name.ToUpper() == name.ToUpper() || transform.GetChild(i).name == name)
					return transform.GetChild(i);

			return null;
		}
		/// <summary>
		/// Finds a direct child of the transform whose name starts with the specified string.
		/// Performs a case-sensitive or case-insensitive prefix comparison based on the caseSensitive parameter.
		/// Only searches immediate children, not descendants further down the hierarchy.
		/// </summary>
		/// <param name="transform">The transform to search in.</param>
		/// <param name="name">The prefix to search for.</param>
		/// <param name="caseSensitive">Whether the name comparison should be case-sensitive. Defaults to true.</param>
		/// <returns>The found child transform, or null if no matching child was found.</returns>
		public static Transform FindStartsWith(this Transform transform, string name, bool caseSensitive = true)
		{
			for (int i = 0; i < transform.childCount; i++)
				if (!caseSensitive && transform.GetChild(i).name.ToUpper().StartsWith(name.ToUpper()) || transform.GetChild(i).name.StartsWith(name))
					return transform.GetChild(i);

			return null;
		}
		/// <summary>
		/// Finds a direct child of the transform whose name ends with the specified string.
		/// Performs a case-sensitive or case-insensitive suffix comparison based on the caseSensitive parameter.
		/// Only searches immediate children, not descendants further down the hierarchy.
		/// </summary>
		/// <param name="transform">The transform to search in.</param>
		/// <param name="name">The suffix to search for.</param>
		/// <param name="caseSensitive">Whether the name comparison should be case-sensitive. Defaults to true.</param>
		/// <returns>The found child transform, or null if no matching child was found.</returns>
		public static Transform FindEndsWith(this Transform transform, string name, bool caseSensitive = true)
		{
			for (int i = 0; i < transform.childCount; i++)
				if (!caseSensitive && transform.GetChild(i).name.ToUpper().EndsWith(name.ToUpper()) || transform.GetChild(i).name.EndsWith(name))
					return transform.GetChild(i);

			return null;
		}
		/// <summary>
		/// Finds a direct child of the transform whose name contains the specified string.
		/// Performs a case-sensitive or case-insensitive substring search based on the caseSensitive parameter.
		/// Only searches immediate children, not descendants further down the hierarchy.
		/// </summary>
		/// <param name="transform">The transform to search in.</param>
		/// <param name="name">The substring to search for.</param>
		/// <param name="caseSensitive">Whether the name comparison should be case-sensitive. Defaults to true.</param>
		/// <returns>The found child transform, or null if no matching child was found.</returns>
		public static Transform FindContains(this Transform transform, string name, bool caseSensitive = true)
		{
			for (int i = 0; i < transform.childCount; i++)
				if (!caseSensitive && transform.GetChild(i).name.ToUpper().Contains(name.ToUpper()) || transform.GetChild(i).name.Contains(name))
					return transform.GetChild(i);

			return null;
		}
		/// <summary>
		/// Clamps all keyframes in the animation curve to the range [0,1] for both time and value.
		/// Creates a new animation curve with the same shape but rescaled to fit within the unit square.
		/// Useful for normalizing curves for use in lerp operations or shader parameters.
		/// </summary>
		/// <param name="curve">The animation curve to clamp.</param>
		/// <returns>A new animation curve with clamped keyframes.</returns>
		public static AnimationCurve Clamp01(this AnimationCurve curve)
		{
			return curve.Clamp(0f, 1f, 0f, 1f);
		}
		/// <summary>
		/// Clamps all keyframes in the animation curve to the range defined by min and max keyframes.
		/// Creates a new animation curve with the same shape but rescaled to fit within the specified bounds.
		/// Both time and value components are clamped according to the provided keyframes.
		/// </summary>
		/// <param name="curve">The animation curve to clamp.</param>
		/// <param name="min">The minimum keyframe defining the lower bounds for time and value.</param>
		/// <param name="max">The maximum keyframe defining the upper bounds for time and value.</param>
		/// <returns>A new animation curve with clamped keyframes.</returns>
		public static AnimationCurve Clamp(this AnimationCurve curve, Keyframe min, Keyframe max)
		{
			return curve.Clamp(min.time, max.time, min.value, max.value);
		}
		/// <summary>
		/// Clamps all keyframes in the animation curve to the specified ranges for time and value.
		/// Creates a new animation curve with the same shape but rescaled to fit within the specified bounds.
		/// Preserves the relative positions and shapes of the keyframes while mapping them to the new range.
		/// If the curve has fewer than 2 keyframes, special handling is applied to ensure valid output.
		/// </summary>
		/// <param name="curve">The animation curve to clamp.</param>
		/// <param name="timeMin">The minimum time value.</param>
		/// <param name="timeMax">The maximum time value.</param>
		/// <param name="valueMin">The minimum keyframe value.</param>
		/// <param name="valueMax">The maximum keyframe value.</param>
		/// <returns>A new animation curve with clamped keyframes.</returns>
		public static AnimationCurve Clamp(this AnimationCurve curve, float timeMin, float timeMax, float valueMin, float valueMax)
		{
			AnimationCurve newCurve;

			if (curve.length < 2)
			{
				newCurve = curve.Clone();

				if (newCurve.length == 1)
					newCurve.MoveKey(0, new Keyframe(timeMin, valueMin));

				return newCurve;
			}

			float curveTimeMin = curve.keys.Min(key => key.time);
			float curveValueMin = curve.keys.Min(key => key.value);
			float curveTimeMax = curve.keys.Max(key => key.time);
			float curveValueMax = curve.keys.Max(key => key.value);

			if (curveTimeMin == timeMin && curveValueMin == valueMin && curveTimeMax == timeMax && curveValueMax == valueMax)
				return curve;

			newCurve = curve.Clone();

			for (int i = 0; i < curve.length; i++)
			{
				float newKeyTime = Utility.Lerp(timeMin, timeMax, Utility.InverseLerp(curveTimeMin, curveTimeMax, curve[i].time));
				float newKeyValue = Utility.Lerp(valueMin, valueMax, Utility.InverseLerp(curveValueMin, curveValueMax, curve[i].value));

				newCurve.MoveKey(i, new Keyframe(newKeyTime, newKeyValue));
			}

			return newCurve;
		}
		/// <summary>
		/// Creates a deep copy of an AnimationCurve.
		/// Duplicates all keyframes and preserves the pre and post wrap modes of the original curve.
		/// This ensures that modifications to the cloned curve don't affect the original.
		/// </summary>
		/// <param name="curve">The animation curve to clone.</param>
		/// <returns>A new animation curve with the same keyframes and wrap modes.</returns>
		public static AnimationCurve Clone(this AnimationCurve curve)
		{
			Keyframe[] newKeys = new Keyframe[curve.length];

			curve.keys.CopyTo(newKeys, 0);

			return new AnimationCurve(newKeys)
			{
				postWrapMode = curve.postWrapMode,
				preWrapMode = curve.preWrapMode
			};
		}
		/// <summary>
		/// Determines whether the specified string is null or empty.
		/// This is a convenience wrapper around the static string.IsNullOrEmpty method
		/// that allows for more fluent syntax in method chains.
		/// </summary>
		/// <param name="str">The string to test.</param>
		/// <returns>true if the string is null or empty; otherwise, false.</returns>
		public static bool IsNullOrEmpty(this string str)
		{
			return string.IsNullOrEmpty(str);
		}
		/// <summary>
		/// Determines whether the specified string is null, empty, or consists only of white-space characters.
		/// This is a convenience wrapper around the static string.IsNullOrWhiteSpace method
		/// that allows for more fluent syntax in method chains.
		/// </summary>
		/// <param name="str">The string to test.</param>
		/// <returns>true if the string is null, empty, or consists only of white-space characters; otherwise, false.</returns>
		public static bool IsNullOrWhiteSpace(this string str)
		{
			return string.IsNullOrWhiteSpace(str);
		}
		/// <summary>
		/// Concatenates the elements of a string collection, using the specified separator between each element.
		/// This is a convenience wrapper around the static string.Join method
		/// that allows for more fluent syntax when working with collections of strings.
		/// </summary>
		/// <param name="strings">A collection of strings to join.</param>
		/// <param name="separator">The string to use as a separator.</param>
		/// <returns>A string that consists of the elements of the collection delimited by the separator string.</returns>
		public static string Join(this IEnumerable<string> strings, string separator)
		{
			return string.Join(separator, strings);
		}
		/// <summary>
		/// Replaces all occurrences of strings in the specified collection with another specified string.
		/// Iterates through each string in the find collection and performs replacements sequentially.
		/// This allows for batch replacement operations with a single method call.
		/// </summary>
		/// <param name="str">The string to perform replacements on.</param>
		/// <param name="find">A collection of strings to be replaced.</param>
		/// <param name="replacement">The string to replace all occurrences of the strings in the find collection.</param>
		/// <returns>A new string with all occurrences of the specified strings replaced.</returns>
		public static string Replace(this string str, IEnumerable<string> find, string replacement)
		{
			foreach (var r in find)
				str = str.Replace(r, replacement);

			return str;
		}
#if !UNITY_2021_2_OR_NEWER
		/// <summary>
		/// Splits a string into an array of substrings using the specified separator.
		/// This implementation provides compatibility for older Unity versions that don't include
		/// the string.Split(string) method. It handles edge cases and maintains proper ordering.
		/// </summary>
		/// <param name="str">The string to split.</param>
		/// <param name="separator">The string that delimits the substrings in the input string.</param>
		/// <returns>An array of strings that contains the substrings delimited by the separator.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
		/// <exception cref="ArgumentException">Thrown when the separator is null or empty.</exception>
		public static string[] Split(this string str, string separator)
		{
			if (str == null)
				throw new ArgumentNullException("str");
			else if (string.IsNullOrEmpty(separator))
				throw new ArgumentException("String separator cannot be Null or empty", "separator");

			List<int> partIndices = new List<int>();
			int lastPartIndex = str.Length;

			while (true)
			{
				lastPartIndex = str.Substring(0, lastPartIndex).LastIndexOf(separator);

				if (lastPartIndex < 0)
					break;

				partIndices.Add(lastPartIndex);
			}

			partIndices.Sort();

			List<string> parts = new List<string>(partIndices.Count + 1);

			for (int i = 0; i < partIndices.Count + 1; i++)
			{
				int startIndex = i > 0 ? partIndices[i - 1] : 0;
				int lastIndex = i < partIndices.Count ? partIndices[i] : str.Length;
				int length = lastIndex - startIndex;
				int finalStartIndex = startIndex;

				if (i > 0)
				{
					finalStartIndex += separator.Length;
					length -= separator.Length;
				}

				string part = str.Substring(finalStartIndex, length);

				if (!string.IsNullOrEmpty(part))
					parts.Add(part);
			}

			return parts.ToArray();
		}
#endif
		/// <summary>
		/// Gets the first attribute of the specified type applied to the enum value.
		/// Uses reflection to access custom attributes defined on enum members.
		/// Useful for retrieving metadata associated with enum values through attributes.
		/// </summary>
		/// <typeparam name="T">The type of attribute to retrieve.</typeparam>
		/// <param name="enumValue">The enum value to get the attribute from.</param>
		/// <returns>The first attribute of the specified type applied to the enum value.</returns>
		public static T GetAttribute<T>(this Enum enumValue) where T : Attribute
		{
			return enumValue.GetType().GetMember(enumValue.ToString()).First().GetCustomAttribute<T>();
		}
		/// <summary>
		/// Sets the alpha component of a color and returns the modified color.
		/// Creates a new color with the same RGB values but with the specified alpha.
		/// Useful for adjusting transparency without changing the color itself.
		/// </summary>
		/// <param name="color">The color to modify.</param>
		/// <param name="alpha">The new alpha value to set (0-1 range).</param>
		/// <returns>A new color with the specified alpha value.</returns>
		public static Color SetAlpha(this Color color, float alpha)
		{
			color.a = alpha;

			return color;
		}
	}
	/// <summary>
	/// Provides utility functions for various operations in Unity development.
	/// Contains methods for unit conversion, mathematical operations, file handling,
	/// object manipulation, and other common utility tasks used throughout the codebase.
	/// </summary>
	public static class Utility
	{
		#region Modules & Enumerators

		#region Enumerators

		/// <summary>
		/// Defines precision levels for calculations and display.
		/// Simple precision uses fewer decimal places for general use cases,
		/// while Advanced provides higher precision for more detailed calculations.
		/// </summary>
		public enum Precision { Simple, Advanced }
		
		/// <summary>
		/// Defines unit systems for measurements.
		/// Metric uses meters, kilograms, etc. (SI units),
		/// while Imperial uses feet, pounds, etc. Used for unit conversion operations.
		/// </summary>
		public enum UnitType { Metric, Imperial }
		
		/// <summary>
		/// Defines various unit types for different physical quantities.
		/// Each enum value represents a specific measurement category that can be
		/// converted between different systems using the utility's conversion methods.
		/// </summary>
		public enum Units { AngularVelocity, Area, AreaAccurate, AreaLarge, ElectricConsumption, Density, Distance, DistanceAccurate, DistanceLong, ElectricCapacity, Force, Frequency, FuelConsumption, Liquid, Power, Pressure, Size, SizeAccurate, Speed, Time, TimeAccurate, Torque, Velocity, Volume, VolumeAccurate, VolumeLarge, Weight }
		
		/// <summary>
		/// Defines the available render pipelines in Unity.
		/// Used to detect and handle different rendering systems appropriately.
		/// Unknown represents an unidentified pipeline, while Standard, URP, HDRP, and Custom
		/// represent the built-in pipeline, Universal Render Pipeline, High Definition Render Pipeline,
		/// and custom rendering solutions respectively.
		/// </summary>
		public enum RenderPipeline { Unknown = -1, Standard, URP, HDRP, Custom }
		
		/// <summary>
		/// Defines texture encoding formats for saving textures.
		/// EXR supports high dynamic range with floating-point precision,
		/// JPG provides compressed lossy format,
		/// PNG offers lossless compression with alpha support,
		/// TGA provides another lossless format with alpha channel support.
		/// </summary>
		public enum TextureEncodingType { EXR, JPG, PNG, TGA }
		
		/// <summary>
		/// Defines relative positions in world space.
		/// Left (-1) represents the negative side of an axis,
		/// Center (0) represents the middle point or origin,
		/// Right (1) represents the positive side of an axis.
		/// Used for positioning objects relative to a reference point.
		/// </summary>
		public enum WorldSide { Left = -1, Center, Right }
		
		/// <summary>
		/// Defines coordinate planes in 3D space.
		/// XY represents the horizontal-vertical plane (front view),
		/// XZ represents the horizontal-depth plane (top-down view),
		/// YZ represents the vertical-depth plane (side view).
		/// Used for operations that work on specific 2D planes within 3D space.
		/// </summary>
		public enum WorldSurface { XY, XZ, YZ }
		
		/// <summary>
		/// Defines axes in 2D space.
		/// X represents the horizontal axis,
		/// Y represents the vertical axis.
		/// Used for operations that need to reference a specific 2D axis.
		/// </summary>
		public enum Axis2 { X, Y }
		
		/// <summary>
		/// Defines axes in 3D space.
		/// X represents the right direction (horizontal),
		/// Y represents the up direction (vertical),
		/// Z represents the forward direction (depth).
		/// Used for operations that need to reference a specific 3D axis.
		/// </summary>
		public enum Axis3 { X, Y, Z }
		
		/// <summary>
		/// Defines axes in 4D space.
		/// X, Y, and Z represent the standard 3D axes,
		/// W represents the fourth dimension, often used for quaternions or homogeneous coordinates.
		/// Used for operations involving 4D vectors or quaternions.
		/// </summary>
		public enum Axis4 { X, Y, Z, W }

		#endregion

		#region Modules

		#region Static Modules

		/// <summary>
		/// Provides additional color constants not available in UnityEngine.Color.
		/// These pre-defined colors extend Unity's built-in color palette with commonly used colors
		/// that aren't included in the standard UnityEngine.Color class. All colors use the default
		/// alpha value of 1.0 (fully opaque) except for the transparent color.
		/// </summary>
		public static class Color
		{
			/// <summary>
			/// A dark gray color (R:0.25, G:0.25, B:0.25, A:1.0).
			/// This color is darker than Unity's built-in gray color and is useful for UI elements,
			/// shadows, or any design elements requiring a deeper gray tone.
			/// </summary>
			public static UnityEngine.Color darkGray = new UnityEngine.Color(.25f, .25f, .25f);
			
			/// <summary>
			/// A light gray color (R:0.67, G:0.67, B:0.67, A:1.0).
			/// This color is lighter than Unity's built-in gray color and is useful for subtle UI elements,
			/// highlights, or any design elements requiring a softer gray tone.
			/// </summary>
			public static UnityEngine.Color lightGray = new UnityEngine.Color(.67f, .67f, .67f);
			
			/// <summary>
			/// An orange color (R:1.0, G:0.5, B:0.0, A:1.0).
			/// This vibrant orange is useful for warning indicators, highlighting important UI elements,
			/// or creating warm visual accents in your application.
			/// </summary>
			public static UnityEngine.Color orange = new UnityEngine.Color(1f, .5f, 0f);
			
			/// <summary>
			/// A purple color (R:0.5, G:0.0, B:1.0, A:1.0).
			/// This rich purple tone can be used for special effects, UI elements requiring visual distinction,
			/// or creating cool visual accents in your application.
			/// </summary>
			public static UnityEngine.Color purple = new UnityEngine.Color(.5f, 0f, 1f);
			
			/// <summary>
			/// A fully transparent color (R:0.0, G:0.0, B:0.0, A:0.0).
			/// This color has zero alpha, making it completely invisible. It's useful for fade effects,
			/// temporarily hiding objects, or as a default state for elements that should start invisible.
			/// </summary>
			public static UnityEngine.Color transparent = new UnityEngine.Color(0f, 0f, 0f, 0f);
		}
		/// <summary>
		/// Provides interpolation formulas for smooth transitions between values.
		/// These formulas are useful for animations, camera movements, UI transitions,
		/// and any scenario requiring non-linear progression between states.
		/// Each formula takes a normalized input parameter (0 to 1) and returns
		/// a transformed value that follows a specific curve pattern.
		/// </summary>
		public static class FormulaInterpolation
		{
			/// <summary>
			/// Linear interpolation formula that clamps the input parameter between 0 and 1.
			/// This is the simplest form of interpolation, providing a constant rate of change.
			/// The output matches the input directly (after clamping), resulting in a straight line
			/// when graphed. Useful as a baseline interpolation or when a uniform transition is desired.
			/// </summary>
			/// <param name="t">The interpolation parameter (0 to 1). Values outside this range will be clamped.</param>
			/// <returns>The clamped value of t, ensuring the result is always between 0 and 1.</returns>
			public static float Linear(float t)
			{
				return Clamp01(t);
			}
			
			/// <summary>
			/// Circular interpolation formula that starts slow and ends fast.
			/// This creates an accelerating effect where the rate of change increases over time.
			/// The formula uses a cosine-based function to produce a smooth curve that begins
			/// with a gentle slope and progressively steepens. Ideal for animations that should
			/// start subtly and finish with emphasis, like fade-ins or entrance animations.
			/// </summary>
			/// <param name="t">The interpolation parameter (0 to 1). Values outside this range will be clamped.</param>
			/// <returns>The interpolated value using a circular function, ranging from 0 to 1.</returns>
			public static float CircularLowToHigh(float t)
			{
				return Clamp01(1f - math.pow(math.cos(math.PI * Mathf.Rad2Deg * Clamp01(t) * .5f), .5f));
			}
			
			/// <summary>
			/// Circular interpolation formula that starts fast and ends slow.
			/// This creates a decelerating effect where the rate of change decreases over time.
			/// The formula uses a sine-based function to produce a smooth curve that begins
			/// with a steep slope and gradually flattens. Perfect for animations that should
			/// start with immediate impact but ease into their final state, like camera movements
			/// or deceleration effects.
			/// </summary>
			/// <param name="t">The interpolation parameter (0 to 1). Values outside this range will be clamped.</param>
			/// <returns>The interpolated value using a circular function, ranging from 0 to 1.</returns>
			public static float CircularHighToLow(float t)
			{
				return Clamp01(math.pow(math.abs(math.sin(math.PI * Mathf.Rad2Deg * Clamp01(t) * .5f)), .5f));
			}
		}

		#endregion

		#region Global Modules

		/// <summary>
		/// A serializable wrapper for arrays that can be used in JSON serialization.
		/// Provides a convenient way to serialize arrays in Unity's JSON format while maintaining
		/// type safety and offering additional utility methods for array manipulation.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array.</typeparam>
		[Serializable]
		public class JsonArray<T>
		{
			#region Variables

			#region  Global Variables

			/// <summary>
			/// Gets the length of the array. Returns 0 if the array is null.
			/// This property provides a safe way to check the array size without causing null reference exceptions.
			/// </summary>
			public int Length => items != null ? items.Length : 0;

			/// <summary>
			/// The internal array that stores the elements.
			/// This field is serialized to enable JSON serialization of the array contents.
			/// </summary>
			[SerializeField]
#pragma warning disable IDE0044 // Add readonly modifier
			private T[] items;
#pragma warning restore IDE0044 // Add readonly modifier

			#endregion

			#region Indexers

			/// <summary>
			/// Gets or sets the element at the specified index.
			/// Provides array-like access to the elements in the JsonArray.
			/// </summary>
			/// <param name="index">The zero-based index of the element to get or set.</param>
			/// <returns>The element at the specified index.</returns>
			/// <exception cref="IndexOutOfRangeException">Thrown when index is outside the bounds of the array.</exception>
			public T this[int index]
			{
				get
				{
					return items[index];
				}
			}

			#endregion

			#endregion

			#region Methods

			/// <summary>
			/// Returns an enumerator that iterates through the array.
			/// Enables the JsonArray to be used in foreach loops and LINQ queries.
			/// </summary>
			/// <returns>An enumerator that can be used to iterate through the array.</returns>
			public IEnumerator GetEnumerator()
			{
				return items.GetEnumerator();
			}
			/// <summary>
			/// Converts the JsonArray to a regular array.
			/// Useful when you need to pass the data to methods that require a standard array.
			/// </summary>
			/// <returns>A regular array containing the elements of the JsonArray.</returns>
			public T[] ToArray()
			{
				return items;
			}

			#endregion

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the JsonArray class with the specified array.
			/// Automatically removes duplicate elements using Distinct() to ensure uniqueness.
			/// </summary>
			/// <param name="array">The array to initialize the JsonArray with.</param>
			public JsonArray(T[] array)
			{
				items = array.Distinct().ToArray();
			}

			#endregion
		}
		/// <summary>
		/// Represents a range of values defined by minimum and maximum boundaries.
		/// Provides methods for clamping, interpolation, and range checking.
		/// Supports optional constraints like preventing min from exceeding max and clamping to zero.
		/// </summary>
		[Serializable]
		public struct Interval
		{
			#region Variables

			/// <summary>
			/// Gets or sets the minimum value of the interval.
			/// When setting, the value is clamped to ensure it doesn't exceed the maximum value unless OverrideBorders is true.
			/// If ClampToZero is true and OverrideBorders is true, the minimum value will never be less than zero.
			/// </summary>
			public float Min
			{
				readonly get
				{
					return min;
				}
				set
				{
					min = math.clamp(value, -math.INFINITY, OverrideBorders ? math.INFINITY : Max);
				}
			}
			
			/// <summary>
			/// Gets or sets the maximum value of the interval.
			/// When setting, the value is clamped to ensure it isn't less than the minimum value unless OverrideBorders is true.
			/// This maintains the integrity of the interval by ensuring Max ≥ Min when OverrideBorders is false.
			/// </summary>
			public float Max
			{
				readonly get
				{
					return max;
				}
				set
				{
					max = math.clamp(value, OverrideBorders ? -math.INFINITY : Min, math.INFINITY);
				}
			}
			
			/// <summary>
			/// Gets or sets whether the interval allows Min to be greater than Max.
			/// When set to false, the Min and Max values are adjusted to maintain the constraint that Min ≤ Max.
			/// This property enables creating either traditional intervals (Min ≤ Max) or inverted intervals (Min > Max).
			/// </summary>
			public bool OverrideBorders
			{
				readonly get
				{
					return overrideBorders;
				}
				set
				{
					if (!value)
					{
						min = math.clamp(min, ClampToZero ? 0f : -math.INFINITY, max);
						max = math.clamp(max, min, math.INFINITY);
					}

					overrideBorders = value;
				}
			}
			
			/// <summary>
			/// Gets or sets whether the minimum value should be clamped to zero.
			/// This property only has an effect when OverrideBorders is true.
			/// Useful for intervals that should never contain negative values, such as ranges for scale or opacity.
			/// </summary>
			public bool ClampToZero
			{
				readonly get
				{
					return OverrideBorders && clampToZero;
				}
				set
				{
					clampToZero = value;
					OverrideBorders = overrideBorders;
				}
			}

			/// <summary>
			/// The minimum value of the interval.
			/// This field is serialized to enable persistence of the interval's minimum value.
			/// </summary>
			[SerializeField]
			private float min;
			
			/// <summary>
			/// The maximum value of the interval.
			/// This field is serialized to enable persistence of the interval's maximum value.
			/// </summary>
			[SerializeField]
			private float max;
			
			/// <summary>
			/// Flag indicating whether the interval allows Min to be greater than Max.
			/// This field is serialized to enable persistence of the interval's border override setting.
			/// </summary>
			[SerializeField, MarshalAs(UnmanagedType.U1)]
			private bool overrideBorders;
			
			/// <summary>
			/// Flag indicating whether the minimum value should be clamped to zero.
			/// This field is serialized to enable persistence of the interval's zero clamping setting.
			/// </summary>
			[SerializeField, MarshalAs(UnmanagedType.U1)]
			private bool clampToZero;

			#endregion

			#region Methods

			#region Virtual Methods

			/// <summary>
			/// Determines whether the specified object is equal to the current Interval.
			/// Two intervals are considered equal if they have the same min, max, overrideBorders, and clampToZero values.
			/// </summary>
			/// <param name="obj">The object to compare with the current Interval.</param>
			/// <returns>true if the specified object is equal to the current Interval; otherwise, false.</returns>
			public override readonly bool Equals(object obj)
			{
				return obj is Interval interval &&
					   min == interval.min &&
					   max == interval.max &&
					   overrideBorders == interval.overrideBorders &&
					   clampToZero == interval.clampToZero;
			}
			
			/// <summary>
			/// Returns a hash code for the current Interval.
			/// The hash code is computed based on the min, max, overrideBorders, and clampToZero values.
			/// This method supports the use of Interval objects in hash-based collections.
			/// </summary>
			/// <returns>A hash code for the current Interval.</returns>
			public override readonly int GetHashCode()
			{
#if UNITY_2021_2_OR_NEWER
				return HashCode.Combine(min, max, overrideBorders, clampToZero);
#else
				int hashCode = 1847768447;

				hashCode = hashCode * -1521134295 + min.GetHashCode();
				hashCode = hashCode * -1521134295 + max.GetHashCode();
				hashCode = hashCode * -1521134295 + overrideBorders.GetHashCode();
				hashCode = hashCode * -1521134295 + clampToZero.GetHashCode();

				return hashCode;
#endif
			}

			#endregion

			#region Global Methods

			/// <summary>
			/// Determines whether the specified integer value is within the interval range.
			/// A value is considered within range if it is greater than or equal to the minimum value
			/// and less than or equal to the maximum value of the interval.
			/// </summary>
			/// <param name="value">The integer value to check.</param>
			/// <returns>true if the value is within the interval range; otherwise, false.</returns>
			public readonly bool InRange(int value)
			{
				return value >= min && value <= max;
			}
			
			/// <summary>
			/// Determines whether the specified float value is within the interval range.
			/// A value is considered within range if it is greater than or equal to the minimum value
			/// and less than or equal to the maximum value of the interval.
			/// </summary>
			/// <param name="value">The float value to check.</param>
			/// <returns>true if the value is within the interval range; otherwise, false.</returns>
			public readonly bool InRange(float value)
			{
				return value >= min && value <= max;
			}
			
			/// <summary>
			/// Linearly interpolates between the minimum and maximum values of the interval.
			/// The interpolation parameter determines the position between Min and Max.
			/// </summary>
			/// <param name="time">The integer interpolation parameter (typically 0 to 1).</param>
			/// <param name="clamped">Whether to clamp the interpolation parameter between 0 and 1. If true, values outside [0,1] will be clamped.</param>
			/// <returns>The interpolated value between Min and Max.</returns>
			public readonly float Lerp(int time, bool clamped = true)
			{
				return clamped ? Utility.Lerp(Min, Max, time) : LerpUnclamped(Min, Max, time);
			}
			
			/// <summary>
			/// Linearly interpolates between the minimum and maximum values of the interval.
			/// The interpolation parameter determines the position between Min and Max.
			/// </summary>
			/// <param name="time">The float interpolation parameter (typically 0 to 1).</param>
			/// <param name="clamped">Whether to clamp the interpolation parameter between 0 and 1. If true, values outside [0,1] will be clamped.</param>
			/// <returns>The interpolated value between Min and Max.</returns>
			public readonly float Lerp(float time, bool clamped = true)
			{
				return clamped ? Utility.Lerp(Min, Max, time) : LerpUnclamped(Min, Max, time);
			}
			
			/// <summary>
			/// Calculates the interpolation parameter that would result in the specified value when linearly interpolating between the minimum and maximum values of the interval.
			/// This is the inverse operation of Lerp - it finds the t value that would produce the given result.
			/// </summary>
			/// <param name="value">The integer value to find the interpolation parameter for.</param>
			/// <param name="clamped">Whether to clamp the result between 0 and 1. If true, values outside [0,1] will be clamped.</param>
			/// <returns>The interpolation parameter that would produce the specified value.</returns>
			public readonly float InverseLerp(int value, bool clamped = true)
			{
				return clamped ? Utility.InverseLerp(Min, Max, value) : InverseLerpUnclamped(Min, Max, value);
			}
			
			/// <summary>
			/// Calculates the interpolation parameter that would result in the specified value when linearly interpolating between the minimum and maximum values of the interval.
			/// This is the inverse operation of Lerp - it finds the t value that would produce the given result.
			/// </summary>
			/// <param name="value">The float value to find the interpolation parameter for.</param>
			/// <param name="clamped">Whether to clamp the result between 0 and 1. If true, values outside [0,1] will be clamped.</param>
			/// <returns>The interpolation parameter that would produce the specified value.</returns>
			public readonly float InverseLerp(float value, bool clamped = true)
			{
				return clamped ? Utility.InverseLerp(Min, Max, value) : InverseLerpUnclamped(Min, Max, value);
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the Interval struct with the specified minimum and maximum values.
			/// Applies constraints based on the overrideBorders and clampToZero parameters to ensure the interval
			/// maintains its integrity according to the specified rules.
			/// </summary>
			/// <param name="min">The minimum value of the interval.</param>
			/// <param name="max">The maximum value of the interval.</param>
			/// <param name="overrideBorders">Whether to allow the minimum value to be greater than the maximum value.</param>
			/// <param name="clampToZero">Whether to clamp the minimum value to zero (only applies when overrideBorders is true).</param>
			public Interval(float min, float max, bool overrideBorders = false, bool clampToZero = false)
			{
				this.min = math.clamp(min, clampToZero ? 0f : -math.INFINITY, overrideBorders ? math.INFINITY : max);
				this.max = math.clamp(max, overrideBorders ? -math.INFINITY : min, math.INFINITY);
				this.overrideBorders = overrideBorders;
				this.clampToZero = clampToZero;
			}
			
			/// <summary>
			/// Initializes a new instance of the Interval struct by copying values from an existing interval.
			/// Creates a deep copy of the source interval, preserving all its properties and constraints.
			/// </summary>
			/// <param name="interval">The source interval to copy values from.</param>
			public Interval(Interval interval)
			{
				min = interval.Min;
				max = interval.Max;
				overrideBorders = interval.OverrideBorders;
				clampToZero = interval.clampToZero;
			}

			#endregion

			#region Operators

			/// <summary>
			/// Adds a scalar value to both the minimum and maximum values of an interval.
			/// This shifts the entire interval by the specified amount.
			/// </summary>
			/// <param name="a">The interval to add to.</param>
			/// <param name="b">The scalar value to add.</param>
			/// <returns>A new interval with the result of the addition.</returns>
			public static Interval operator +(Interval a, float b)
			{
				return new Interval(a.min + b, a.max + b);
			}
			
			/// <summary>
			/// Adds the minimum and maximum values of two intervals.
			/// This combines the intervals by adding their respective min and max values.
			/// </summary>
			/// <param name="a">The first interval.</param>
			/// <param name="b">The second interval.</param>
			/// <returns>A new interval with the result of the addition.</returns>
			public static Interval operator +(Interval a, Interval b)
			{
				return new Interval(a.min + b.min, a.max + b.max);
			}
			
			/// <summary>
			/// Subtracts a scalar value from both the minimum and maximum values of an interval.
			/// This shifts the entire interval by the negative of the specified amount.
			/// </summary>
			/// <param name="a">The interval to subtract from.</param>
			/// <param name="b">The scalar value to subtract.</param>
			/// <returns>A new interval with the result of the subtraction.</returns>
			public static Interval operator -(Interval a, float b)
			{
				return new Interval(a.min - b, a.max - b);
			}
			
			/// <summary>
			/// Subtracts the minimum and maximum values of the second interval from the first.
			/// This combines the intervals by subtracting their respective min and max values.
			/// </summary>
			/// <param name="a">The interval to subtract from.</param>
			/// <param name="b">The interval to subtract.</param>
			/// <returns>A new interval with the result of the subtraction.</returns>
			public static Interval operator -(Interval a, Interval b)
			{
				return new Interval(a.min - b.min, a.max - b.max);
			}
			
			/// <summary>
			/// Multiplies both the minimum and maximum values of an interval by a scalar value.
			/// This scales the interval by the specified amount.
			/// </summary>
			/// <param name="a">The interval to multiply.</param>
			/// <param name="b">The scalar value to multiply by.</param>
			/// <returns>A new interval with the result of the multiplication.</returns>
			public static Interval operator *(Interval a, float b)
			{
				return new Interval(a.min * b, a.max * b);
			}
			
			/// <summary>
			/// Multiplies the minimum and maximum values of two intervals.
			/// This combines the intervals by multiplying their respective min and max values.
			/// </summary>
			/// <param name="a">The first interval.</param>
			/// <param name="b">The second interval.</param>
			/// <returns>A new interval with the result of the multiplication.</returns>
			public static Interval operator *(Interval a, Interval b)
			{
				return new Interval(a.min * b.min, a.max * b.max);
			}
			
			/// <summary>
			/// Divides both the minimum and maximum values of an interval by a scalar value.
			/// This scales the interval by the reciprocal of the specified amount.
			/// </summary>
			/// <param name="a">The interval to divide.</param>
			/// <param name="b">The scalar value to divide by.</param>
			/// <returns>A new interval with the result of the division.</returns>
			public static Interval operator /(Interval a, float b)
			{
				return new Interval(a.min / b, a.max / b);
			}
			
			/// <summary>
			/// Divides the minimum and maximum values of the first interval by those of the second.
			/// This combines the intervals by dividing their respective min and max values.
			/// </summary>
			/// <param name="a">The interval to divide.</param>
			/// <param name="b">The interval to divide by.</param>
			/// <returns>A new interval with the result of the division.</returns>
			public static Interval operator /(Interval a, Interval b)
			{
				return new Interval(a.min / b.min, a.max / b.max);
			}
			
			/// <summary>
			/// Determines whether two intervals are equal.
			/// Two intervals are considered equal if they have the same min, max, overrideBorders, and clampToZero values.
			/// </summary>
			/// <param name="a">The first interval to compare.</param>
			/// <param name="b">The second interval to compare.</param>
			/// <returns>true if the intervals are equal; otherwise, false.</returns>
			public static bool operator ==(Interval a, Interval b)
			{
				return a.Equals(b);
			}
			
			/// <summary>
			/// Determines whether two intervals are not equal.
			/// Two intervals are considered not equal if any of their min, max, overrideBorders, or clampToZero values differ.
			/// </summary>
			/// <param name="a">The first interval to compare.</param>
			/// <param name="b">The second interval to compare.</param>
			/// <returns>true if the intervals are not equal; otherwise, false.</returns>
			public static bool operator !=(Interval a, Interval b)
			{
				return !(a == b);
			}

			#endregion

			#endregion

		}
		/// <summary>
		/// A simplified interval structure that represents a range between two values without enforcing min/max ordering.
		/// Provides methods for range checking, interpolation, and value manipulation.
		/// Unlike the Interval struct, SimpleInterval does not enforce that 'a' is less than 'b'.
		/// </summary>
		[Serializable]
		public struct SimpleInterval
		{
			#region Variables

			/// <summary>
			/// Gets the absolute length of the interval, calculated as the absolute difference between points 'a' and 'b'.
			/// This property always returns a positive value regardless of the order of 'a' and 'b'.
			/// </summary>
			public readonly float Length => math.abs(a - b);

			/// <summary>
			/// The first endpoint of the interval. Can be either the minimum or maximum value.
			/// </summary>
			public float a;

			/// <summary>
			/// The second endpoint of the interval. Can be either the minimum or maximum value.
			/// </summary>
			public float b;

			#endregion

			#region Methods

			#region Virtual Methods

			/// <summary>
			/// Determines whether the specified object is equal to the current SimpleInterval.
			/// Two SimpleIntervals are considered equal if both their 'a' and 'b' values are identical.
			/// </summary>
			/// <param name="obj">The object to compare with the current SimpleInterval.</param>
			/// <returns>true if the specified object is equal to the current SimpleInterval; otherwise, false.</returns>
			public override readonly bool Equals(object obj)
			{
				return obj is SimpleInterval interval &&
						a == interval.a &&
						b == interval.b;
			}

			/// <summary>
			/// Serves as the default hash function for SimpleInterval.
			/// Returns a hash code for the current SimpleInterval based on its 'a' and 'b' values.
			/// Uses different hash code generation methods depending on the Unity version.
			/// </summary>
			/// <returns>A hash code for the current SimpleInterval.</returns>
			public override readonly int GetHashCode()
			{
#if UNITY_2021_2_OR_NEWER
				return HashCode.Combine(a, b);
#else
				int hashCode = 2118541809;

				hashCode = hashCode * -1521134295 + a.GetHashCode();
				hashCode = hashCode * -1521134295 + b.GetHashCode();

				return hashCode;
#endif
			}

			#endregion

			#region Global Methods

			/// <summary>
			/// Determines whether a value is within the range defined by this interval.
			/// The range is defined as the span between the minimum and maximum of 'a' and 'b'.
			/// </summary>
			/// <param name="value">The value to check.</param>
			/// <returns>true if the value is within the interval's range; otherwise, false.</returns>
			public readonly bool InRange(float value)
			{
				return value >= math.min(a, b) && value <= math.max(a, b);
			}

			/// <summary>
			/// Linearly interpolates between the interval's endpoints based on a parameter.
			/// </summary>
			/// <param name="value">The interpolation parameter.</param>
			/// <param name="clamped">If true, the parameter is clamped between 0 and 1; otherwise, extrapolation is allowed.</param>
			/// <returns>The interpolated value.</returns>
			public readonly float Lerp(float value, bool clamped)
			{
				return clamped ? Lerp(value) : LerpUnclamped(value);
			}

			/// <summary>
			/// Linearly interpolates between the interval's endpoints with the parameter clamped between 0 and 1.
			/// </summary>
			/// <param name="time">The interpolation parameter (clamped between 0 and 1).</param>
			/// <returns>The interpolated value.</returns>
			public readonly float Lerp(float time)
			{
				return Utility.Lerp(a, b, time);
			}

			/// <summary>
			/// Linearly interpolates between the interval's endpoints without clamping the parameter.
			/// Allows for extrapolation beyond the interval's endpoints.
			/// </summary>
			/// <param name="time">The interpolation parameter (unclamped).</param>
			/// <returns>The interpolated or extrapolated value.</returns>
			public readonly float LerpUnclamped(float time)
			{
				return Utility.LerpUnclamped(a, b, time);
			}

			/// <summary>
			/// Calculates the normalized position of a value within the interval.
			/// </summary>
			/// <param name="value">The value to find the normalized position for.</param>
			/// <param name="clamped">If true, the result is clamped between 0 and 1; otherwise, values outside the interval return results outside [0,1].</param>
			/// <returns>The normalized position of the value within the interval.</returns>
			public readonly float InverseLerp(float value, bool clamped)
			{
				return clamped ? InverseLerp(value) : InverseLerpUnclamped(value);
			}

			/// <summary>
			/// Calculates the normalized position of a value within the interval, clamped between 0 and 1.
			/// </summary>
			/// <param name="value">The value to find the normalized position for.</param>
			/// <returns>The normalized position of the value within the interval, clamped between 0 and 1.</returns>
			public readonly float InverseLerp(float value)
			{
				return Utility.InverseLerp(a, b, value);
			}

			/// <summary>
			/// Calculates the normalized position of a value within the interval without clamping.
			/// Values outside the interval will return results outside the [0,1] range.
			/// </summary>
			/// <param name="value">The value to find the normalized position for.</param>
			/// <returns>The normalized position of the value relative to the interval.</returns>
			public readonly float InverseLerpUnclamped(float value)
			{
				return Utility.InverseLerpUnclamped(a, b, value);
			}

			/// <summary>
			/// Clamps a value to be within the interval's range.
			/// </summary>
			/// <param name="value">The value to clamp.</param>
			/// <returns>The value clamped to be within the interval's range.</returns>
			public readonly float Clamp(float value)
			{
				return math.clamp(value, math.min(a, b), math.max(a, b));
			}

			/// <summary>
			/// Generates a random value within the interval's range.
			/// </summary>
			/// <returns>A random float value between the minimum and maximum of the interval.</returns>
			public readonly float Random()
			{
				return UnityEngine.Random.Range(math.min(a, b), math.max(a, b));
			}

			/// <summary>
			/// Expands the interval to include the specified value if it's outside the current range.
			/// Modifies the interval's endpoints to ensure the value is contained within the interval.
			/// </summary>
			/// <param name="value">The value to include in the interval.</param>
			public void Encapsulate(float value)
			{
				if (value < math.min(a, b))
				{
					b = math.max(a, b);
					a = value;
				}
				else if (value > math.max(a, b))
				{
					a = math.min(a, b);
					b = value;
				}
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the SimpleInterval struct with the specified endpoints.
			/// </summary>
			/// <param name="a">The first endpoint of the interval.</param>
			/// <param name="b">The second endpoint of the interval.</param>
			public SimpleInterval(float a, float b)
			{
				this.a = a;
				this.b = b;
			}

			/// <summary>
			/// Initializes a new instance of the SimpleInterval struct by copying another SimpleInterval.
			/// </summary>
			/// <param name="interval">The SimpleInterval to copy.</param>
			public SimpleInterval(SimpleInterval interval)
			{
				a = interval.a;
				b = interval.b;
			}

			/// <summary>
			/// Initializes a new instance of the SimpleInterval struct from an Interval struct.
			/// Uses the Min and Max values from the Interval as the endpoints.
			/// </summary>
			/// <param name="interval">The Interval to convert from.</param>
			public SimpleInterval(Interval interval)
			{
				a = interval.Min;
				b = interval.Max;
			}

			#endregion

			#region Operators

			/// <summary>
			/// Adds a scalar value to both endpoints of the interval.
			/// </summary>
			/// <param name="x">The interval to add to.</param>
			/// <param name="y">The scalar value to add.</param>
			/// <returns>A new SimpleInterval with the result of the addition.</returns>
			public static SimpleInterval operator +(SimpleInterval x, float y)
			{
				return new SimpleInterval(x.a + y, x.b + y);
			}

			/// <summary>
			/// Adds the corresponding endpoints of two intervals.
			/// </summary>
			/// <param name="x">The first interval.</param>
			/// <param name="y">The second interval.</param>
			/// <returns>A new SimpleInterval with the result of the addition.</returns>
			public static SimpleInterval operator +(SimpleInterval x, SimpleInterval y)
			{
				return new SimpleInterval(x.a + y.a, x.b + y.b);
			}

			/// <summary>
			/// Subtracts a scalar value from both endpoints of the interval.
			/// </summary>
			/// <param name="x">The interval to subtract from.</param>
			/// <param name="y">The scalar value to subtract.</param>
			/// <returns>A new SimpleInterval with the result of the subtraction.</returns>
			public static SimpleInterval operator -(SimpleInterval x, float y)
			{
				return new SimpleInterval(x.a - y, x.b - y);
			}

			/// <summary>
			/// Subtracts the corresponding endpoints of the second interval from the first.
			/// </summary>
			/// <param name="x">The interval to subtract from.</param>
			/// <param name="y">The interval to subtract.</param>
			/// <returns>A new SimpleInterval with the result of the subtraction.</returns>
			public static SimpleInterval operator -(SimpleInterval x, SimpleInterval y)
			{
				return new SimpleInterval(x.a - y.a, x.b - y.b);
			}

			/// <summary>
			/// Multiplies both endpoints of the interval by a scalar value.
			/// </summary>
			/// <param name="x">The interval to multiply.</param>
			/// <param name="y">The scalar value to multiply by.</param>
			/// <returns>A new SimpleInterval with the result of the multiplication.</returns>
			public static SimpleInterval operator *(SimpleInterval x, float y)
			{
				return new SimpleInterval(x.a * y, x.b * y);
			}

			/// <summary>
			/// Multiplies the corresponding endpoints of two intervals.
			/// </summary>
			/// <param name="x">The first interval.</param>
			/// <param name="y">The second interval.</param>
			/// <returns>A new SimpleInterval with the result of the multiplication.</returns>
			public static SimpleInterval operator *(SimpleInterval x, SimpleInterval y)
			{
				return new SimpleInterval(x.a * y.a, x.b * y.b);
			}

			/// <summary>
			/// Divides both endpoints of the interval by a scalar value.
			/// </summary>
			/// <param name="x">The interval to divide.</param>
			/// <param name="y">The scalar value to divide by.</param>
			/// <returns>A new SimpleInterval with the result of the division.</returns>
			public static SimpleInterval operator /(SimpleInterval x, float y)
			{
				return new SimpleInterval(x.a / y, x.b / y);
			}

			/// <summary>
			/// Divides the corresponding endpoints of the first interval by those of the second.
			/// </summary>
			/// <param name="x">The interval to divide.</param>
			/// <param name="y">The interval to divide by.</param>
			/// <returns>A new SimpleInterval with the result of the division.</returns>
			public static SimpleInterval operator /(SimpleInterval x, SimpleInterval y)
			{
				return new SimpleInterval(x.a / y.a, x.b / y.b);
			}

			/// <summary>
			/// Determines whether two SimpleIntervals are equal.
			/// Two SimpleIntervals are considered equal if both their 'a' and 'b' values are identical.
			/// </summary>
			/// <param name="x">The first SimpleInterval to compare.</param>
			/// <param name="y">The second SimpleInterval to compare.</param>
			/// <returns>true if the SimpleIntervals are equal; otherwise, false.</returns>
			public static bool operator ==(SimpleInterval x, SimpleInterval y)
			{
				return x.Equals(y);
			}

			/// <summary>
			/// Determines whether two SimpleIntervals are not equal.
			/// Two SimpleIntervals are considered not equal if either their 'a' or 'b' values differ.
			/// </summary>
			/// <param name="x">The first SimpleInterval to compare.</param>
			/// <param name="y">The second SimpleInterval to compare.</param>
			/// <returns>true if the SimpleIntervals are not equal; otherwise, false.</returns>
			public static bool operator !=(SimpleInterval x, SimpleInterval y)
			{
				return !(x == y);
			}

			#endregion

			#endregion
		}
		/// <summary>
		/// Represents a range of integer values defined by minimum and maximum boundaries.
		/// Provides methods for clamping, interpolation, and range checking with integer values.
		/// </summary>
		[Serializable]
		public struct IntervalInt
		{
			#region Variables

			/// <summary>
			/// Gets or sets the minimum value of the interval.
			/// When setting, the value is clamped to ensure it doesn't exceed the maximum value
			/// unless OverrideBorders is true, in which case it can be set to any value.
			/// </summary>
			public int Min
			{
				readonly get
				{
					return min;
				}
				set
				{
					min = (int)math.clamp(value, -math.INFINITY, OverrideBorders ? math.INFINITY : max);
				}
			}

			/// <summary>
			/// Gets or sets the maximum value of the interval.
			/// When setting, the value is clamped to ensure it isn't less than the minimum value
			/// unless OverrideBorders is true, in which case it can be set to any value.
			/// </summary>
			public int Max
			{
				readonly get
				{
					return max;
				}
				set
				{
					max = (int)math.clamp(value, OverrideBorders ? -math.INFINITY : min, math.INFINITY);
				}
			}

			/// <summary>
			/// Gets or sets whether the interval allows min and max values to be set independently.
			/// When false, ensures min ≤ max by clamping values during assignment.
			/// When true, allows min and max to be set to any values, potentially creating an inverted interval.
			/// </summary>
			public bool OverrideBorders
			{
				readonly get
				{
					return overrideBorders;
				}
				set
				{
					if (!value)
					{
						min = (int)math.clamp(min, ClampToZero ? 0f : -math.INFINITY, max);
						max = (int)math.clamp(max, min, math.INFINITY);
					}

					overrideBorders = value;
				}
			}

			/// <summary>
			/// Gets or sets whether the minimum value should be clamped to zero.
			/// Only takes effect when OverrideBorders is true.
			/// When set to true, ensures the minimum value is never negative.
			/// </summary>
			public bool ClampToZero
			{
				readonly get
				{
					return OverrideBorders && clampToZero;
				}
				set
				{
					clampToZero = value;
					OverrideBorders = overrideBorders;
				}
			}

			/// <summary>
			/// The serialized minimum value of the interval.
			/// </summary>
			[SerializeField]
			private int min;

			/// <summary>
			/// The serialized maximum value of the interval.
			/// </summary>
			[SerializeField]
			private int max;

			/// <summary>
			/// Flag indicating whether min and max values can be set independently without enforcing min ≤ max.
			/// </summary>
			[SerializeField, MarshalAs(UnmanagedType.U1)]
			private bool overrideBorders;

			/// <summary>
			/// Flag indicating whether the minimum value should be clamped to zero.
			/// Only effective when overrideBorders is true.
			/// </summary>
			[SerializeField, MarshalAs(UnmanagedType.U1)]
			private bool clampToZero;

			#endregion

			#region Methods

			#region Virtual Methods

			/// <summary>
			/// Determines whether the specified object is equal to the current IntervalInt.
			/// Two IntervalInt instances are considered equal if they have identical min, max,
			/// overrideBorders, and clampToZero values.
			/// </summary>
			/// <param name="obj">The object to compare with the current IntervalInt.</param>
			/// <returns>true if the specified object is equal to the current IntervalInt; otherwise, false.</returns>
			public override readonly bool Equals(object obj)
			{
				return obj is IntervalInt interval &&
					   min == interval.min &&
					   max == interval.max &&
					   overrideBorders == interval.overrideBorders &&
					   clampToZero == interval.clampToZero;
			}

			/// <summary>
			/// Serves as the default hash function for IntervalInt.
			/// Returns a hash code for the current IntervalInt based on its min, max, 
			/// overrideBorders, and clampToZero values.
			/// Uses different hash code generation methods depending on the Unity version.
			/// </summary>
			/// <returns>A hash code for the current IntervalInt.</returns>
			public override readonly int GetHashCode()
			{
#if UNITY_2021_2_OR_NEWER
				return HashCode.Combine(min, max, overrideBorders, clampToZero);
#else
				int hashCode = 1847768447;

				hashCode = hashCode * -1521134295 + min.GetHashCode();
				hashCode = hashCode * -1521134295 + max.GetHashCode();
				hashCode = hashCode * -1521134295 + overrideBorders.GetHashCode();
				hashCode = hashCode * -1521134295 + clampToZero.GetHashCode();

				return hashCode;
#endif
			}

			#endregion

			#region Global Methods

			/// <summary>
			/// Determines whether the specified integer value falls within the interval's range.
			/// A value is considered in range if it is greater than or equal to min and less than or equal to max.
			/// </summary>
			/// <param name="value">The integer value to check.</param>
			/// <returns>true if the value is within the interval's range; otherwise, false.</returns>
			public readonly bool InRange(int value)
			{
				return value >= min && value <= max;
			}

			/// <summary>
			/// Determines whether the specified float value falls within the interval's range.
			/// A value is considered in range if it is greater than or equal to min and less than or equal to max.
			/// </summary>
			/// <param name="value">The float value to check.</param>
			/// <returns>true if the value is within the interval's range; otherwise, false.</returns>
			public readonly bool InRange(float value)
			{
				return value >= min && value <= max;
			}

			/// <summary>
			/// Linearly interpolates between the min and max values of the interval based on an integer time parameter.
			/// When clamped is true, the time parameter is restricted to the range [0,1].
			/// </summary>
			/// <param name="time">The interpolation parameter (0 returns min, 1 returns max).</param>
			/// <param name="clamped">Whether to clamp the time parameter to the range [0,1].</param>
			/// <returns>The interpolated value.</returns>
			public readonly float Lerp(int time, bool clamped = true)
			{
				return clamped ? Utility.Lerp(Min, Max, time) : LerpUnclamped(Min, Max, time);
			}

			/// <summary>
			/// Linearly interpolates between the min and max values of the interval based on a float time parameter.
			/// When clamped is true, the time parameter is restricted to the range [0,1].
			/// </summary>
			/// <param name="time">The interpolation parameter (0 returns min, 1 returns max).</param>
			/// <param name="clamped">Whether to clamp the time parameter to the range [0,1].</param>
			/// <returns>The interpolated value.</returns>
			public readonly float Lerp(float time, bool clamped = true)
			{
				return clamped ? Utility.Lerp(Min, Max, time) : LerpUnclamped(Min, Max, time);
			}

			/// <summary>
			/// Calculates the normalized position of an integer value within the interval.
			/// Returns a value between 0 and 1 where 0 represents min and 1 represents max.
			/// When clamped is true, the result is restricted to the range [0,1].
			/// </summary>
			/// <param name="value">The value to find the normalized position of.</param>
			/// <param name="clamped">Whether to clamp the result to the range [0,1].</param>
			/// <returns>The normalized position of the value within the interval.</returns>
			public readonly float InverseLerp(int value, bool clamped = true)
			{
				return clamped ? Utility.InverseLerp(Min, Max, value) : InverseLerpUnclamped(Min, Max, value);
			}

			/// <summary>
			/// Calculates the normalized position of a float value within the interval.
			/// Returns a value between 0 and 1 where 0 represents min and 1 represents max.
			/// When clamped is true, the result is restricted to the range [0,1].
			/// </summary>
			/// <param name="value">The value to find the normalized position of.</param>
			/// <param name="clamped">Whether to clamp the result to the range [0,1].</param>
			/// <returns>The normalized position of the value within the interval.</returns>
			public readonly float InverseLerp(float value, bool clamped = true)
			{
				return clamped ? Utility.InverseLerp(Min, Max, value) : InverseLerpUnclamped(Min, Max, value);
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the IntervalInt struct with specified minimum and maximum values.
			/// Optionally allows overriding the normal min ≤ max constraint and clamping the minimum to zero.
			/// </summary>
			/// <param name="min">The minimum value of the interval.</param>
			/// <param name="max">The maximum value of the interval.</param>
			/// <param name="overrideBorders">Whether to allow min > max. Defaults to false.</param>
			/// <param name="clampToZero">Whether to ensure min ≥ 0. Only effective when overrideBorders is true. Defaults to false.</param>
			public IntervalInt(int min, int max, bool overrideBorders = false, bool clampToZero = false)
			{
				this.min = (int)math.clamp(min, clampToZero ? 0f : -math.INFINITY, overrideBorders ? math.INFINITY : max);
				this.max = (int)math.clamp(max, overrideBorders ? -math.INFINITY : min, math.INFINITY);
				this.overrideBorders = overrideBorders;
				this.clampToZero = clampToZero;
			}

			/// <summary>
			/// Initializes a new instance of the IntervalInt struct by copying values from an existing interval.
			/// Creates a deep copy of the provided interval with identical min, max, overrideBorders, and clampToZero values.
			/// </summary>
			/// <param name="interval">The interval to copy values from.</param>
			public IntervalInt(IntervalInt interval)
			{
				min = interval.Min;
				max = interval.Max;
				overrideBorders = interval.OverrideBorders;
				clampToZero = interval.clampToZero;
			}

			#endregion

			#region Operators

			/// <summary>
			/// Adds an integer value to both endpoints of an interval.
			/// </summary>
			/// <param name="a">The interval to add to.</param>
			/// <param name="b">The integer value to add.</param>
			/// <returns>A new IntervalInt with the result of the addition.</returns>
			public static IntervalInt operator +(IntervalInt a, int b)
			{
				return new IntervalInt(a.min + b, a.max + b);
			}

			/// <summary>
			/// Adds the corresponding endpoints of two intervals.
			/// </summary>
			/// <param name="a">The first interval.</param>
			/// <param name="b">The second interval.</param>
			/// <returns>A new IntervalInt with the result of the addition.</returns>
			public static IntervalInt operator +(IntervalInt a, IntervalInt b)
			{
				return new IntervalInt(a.min + b.min, a.max + b.max);
			}

			/// <summary>
			/// Subtracts an integer value from both endpoints of an interval.
			/// </summary>
			/// <param name="a">The interval to subtract from.</param>
			/// <param name="b">The integer value to subtract.</param>
			/// <returns>A new IntervalInt with the result of the subtraction.</returns>
			public static IntervalInt operator -(IntervalInt a, int b)
			{
				return new IntervalInt(a.min - b, a.max - b);
			}

			/// <summary>
			/// Subtracts the corresponding endpoints of the second interval from the first.
			/// </summary>
			/// <param name="a">The interval to subtract from.</param>
			/// <param name="b">The interval to subtract.</param>
			/// <returns>A new IntervalInt with the result of the subtraction.</returns>
			public static IntervalInt operator -(IntervalInt a, IntervalInt b)
			{
				return new IntervalInt(a.min - b.min, a.max - b.max);
			}

			/// <summary>
			/// Multiplies both endpoints of an interval by an integer value.
			/// </summary>
			/// <param name="a">The interval to multiply.</param>
			/// <param name="b">The integer value to multiply by.</param>
			/// <returns>A new IntervalInt with the result of the multiplication.</returns>
			public static IntervalInt operator *(IntervalInt a, int b)
			{
				return new IntervalInt(a.min * b, a.max * b);
			}

			/// <summary>
			/// Multiplies the corresponding endpoints of two intervals.
			/// </summary>
			/// <param name="a">The first interval.</param>
			/// <param name="b">The second interval.</param>
			/// <returns>A new IntervalInt with the result of the multiplication.</returns>
			public static IntervalInt operator *(IntervalInt a, IntervalInt b)
			{
				return new IntervalInt(a.min * b.min, a.max * b.max);
			}

			/// <summary>
			/// Divides both endpoints of an interval by an integer value.
			/// </summary>
			/// <param name="a">The interval to divide.</param>
			/// <param name="b">The integer value to divide by.</param>
			/// <returns>A new IntervalInt with the result of the division.</returns>
			public static IntervalInt operator /(IntervalInt a, int b)
			{
				return new IntervalInt(a.min / b, a.max / b);
			}

			/// <summary>
			/// Divides the corresponding endpoints of the first interval by those of the second.
			/// </summary>
			/// <param name="a">The interval to divide.</param>
			/// <param name="b">The interval to divide by.</param>
			/// <returns>A new IntervalInt with the result of the division.</returns>
			public static IntervalInt operator /(IntervalInt a, IntervalInt b)
			{
				return new IntervalInt(a.min / b.min, a.max / b.max);
			}

			/// <summary>
			/// Determines whether two IntervalInt instances are equal.
			/// Two intervals are considered equal if they have identical min, max, overrideBorders, and clampToZero values.
			/// </summary>
			/// <param name="a">The first interval to compare.</param>
			/// <param name="b">The second interval to compare.</param>
			/// <returns>true if the intervals are equal; otherwise, false.</returns>
			public static bool operator ==(IntervalInt a, IntervalInt b)
			{
				return a.Equals(b);
			}

			/// <summary>
			/// Determines whether two IntervalInt instances are not equal.
			/// Two intervals are considered not equal if any of their min, max, overrideBorders, or clampToZero values differ.
			/// </summary>
			/// <param name="a">The first interval to compare.</param>
			/// <param name="b">The second interval to compare.</param>
			/// <returns>true if the intervals are not equal; otherwise, false.</returns>
			public static bool operator !=(IntervalInt a, IntervalInt b)
			{
				return !(a == b);
			}

			#endregion

			#endregion

		}
		/// <summary>
		/// Represents a 2D interval with separate X and Y ranges.
		/// Useful for defining rectangular regions and performing range checks in 2D space.
		/// </summary>
		[Serializable]
		public struct Interval2
		{
			#region Variables

			/// <summary>
			/// The interval representing the X-axis range.
			/// Defines the minimum and maximum values along the horizontal dimension.
			/// </summary>
			public Interval x;

			/// <summary>
			/// The interval representing the Y-axis range.
			/// Defines the minimum and maximum values along the vertical dimension.
			/// </summary>
			public Interval y;

			#endregion

			#region Methods

			#region Virtual Methods

			/// <summary>
			/// Determines whether the specified object is equal to the current Interval2.
			/// Two Interval2 instances are considered equal if both their X and Y intervals are identical.
			/// </summary>
			/// <param name="obj">The object to compare with the current Interval2.</param>
			/// <returns>true if the specified object is equal to the current Interval2; otherwise, false.</returns>
			public override readonly bool Equals(object obj)
			{
				return obj is Interval2 interval &&
					   EqualityComparer<Interval>.Default.Equals(x, interval.x) &&
					   EqualityComparer<Interval>.Default.Equals(y, interval.y);
			}

			/// <summary>
			/// Serves as the default hash function for Interval2.
			/// Returns a hash code for the current Interval2 based on its X and Y intervals.
			/// Uses different hash code generation methods depending on the Unity version.
			/// </summary>
			/// <returns>A hash code for the current Interval2.</returns>
			public override readonly int GetHashCode()
			{
#if UNITY_2021_2_OR_NEWER
				return HashCode.Combine(x, y);
#else
				int hashCode = 1502939027;

				hashCode = hashCode * -1521134295 + x.GetHashCode();
				hashCode = hashCode * -1521134295 + y.GetHashCode();

				return hashCode;
#endif
			}

			#endregion

			#region Global Methods

			/// <summary>
			/// Determines whether the specified float value falls within both X and Y intervals.
			/// A value is considered in range if it is within both the X and Y intervals simultaneously.
			/// </summary>
			/// <param name="value">The float value to check.</param>
			/// <returns>true if the value is within both X and Y intervals; otherwise, false.</returns>
			public readonly bool InRange(float value)
			{
				return x.InRange(value) && y.InRange(value);
			}

			/// <summary>
			/// Determines whether the specified Vector2 falls within the 2D interval region.
			/// Checks if the X component is within the X interval and the Y component is within the Y interval.
			/// </summary>
			/// <param name="value">The Vector2 value to check.</param>
			/// <returns>true if the Vector2 is within the 2D interval region; otherwise, false.</returns>
			public readonly bool InRange(Vector2 value)
			{
				return x.InRange(value.x) && y.InRange(value.y);
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the Interval2 struct with specified minimum and maximum values for X and Y axes.
			/// Creates a rectangular region defined by the given boundaries.
			/// </summary>
			/// <param name="xMin">The minimum value for the X interval.</param>
			/// <param name="xMax">The maximum value for the X interval.</param>
			/// <param name="yMin">The minimum value for the Y interval.</param>
			/// <param name="yMax">The maximum value for the Y interval.</param>
			public Interval2(float xMin, float xMax, float yMin, float yMax)
			{
				x = new Interval(xMin, xMax);
				y = new Interval(yMin, yMax);
			}

			/// <summary>
			/// Initializes a new instance of the Interval2 struct with specified X and Y intervals.
			/// Creates a rectangular region defined by the given interval components.
			/// </summary>
			/// <param name="x">The interval for the X axis.</param>
			/// <param name="y">The interval for the Y axis.</param>
			public Interval2(Interval x, Interval y)
			{
				this.x = x;
				this.y = y;
			}

			/// <summary>
			/// Initializes a new instance of the Interval2 struct by copying another Interval2.
			/// Creates a deep copy where both X and Y intervals are cloned.
			/// </summary>
			/// <param name="interval">The Interval2 to copy.</param>
			public Interval2(Interval2 interval)
			{
				x = new Interval(interval.x);
				y = new Interval(interval.y);
			}

			#endregion

			#region Operators

			/// <summary>
			/// Determines whether two Interval2 instances are equal.
			/// Two Interval2 instances are considered equal if both their X and Y intervals are identical.
			/// </summary>
			/// <param name="a">The first Interval2 to compare.</param>
			/// <param name="b">The second Interval2 to compare.</param>
			/// <returns>true if the Interval2 instances are equal; otherwise, false.</returns>
			public static bool operator ==(Interval2 a, Interval2 b)
			{
				return a.Equals(b);
			}

			/// <summary>
			/// Determines whether two Interval2 instances are not equal.
			/// Two Interval2 instances are considered not equal if either their X or Y intervals differ.
			/// </summary>
			/// <param name="a">The first Interval2 to compare.</param>
			/// <param name="b">The second Interval2 to compare.</param>
			/// <returns>true if the Interval2 instances are not equal; otherwise, false.</returns>
			public static bool operator !=(Interval2 a, Interval2 b)
			{
				return !(a == b);
			}

			/// <summary>
			/// Implicitly converts an Interval2 to an Interval by extracting its X component.
			/// This allows using a 2D interval in contexts where a 1D interval is expected.
			/// </summary>
			/// <param name="interval">The Interval2 to convert.</param>
			/// <returns>A new Interval containing the X component of the Interval2.</returns>
			public static implicit operator Interval(Interval2 interval)
			{
				return new Interval(interval.x);
			}

			#endregion

			#endregion
		}
		/// <summary>
		/// A simplified 2D interval structure that represents a rectangular region without enforcing min/max ordering.
		/// Provides methods for range checking, interpolation, and region manipulation in 2D space.
		/// </summary>
		[Serializable]
		public struct SimpleInterval2
		{
			#region Constants

			/// <summary>
			/// Represents an empty SimpleInterval2 with all components set to zero.
			/// Useful as a default value or for initialization purposes.
			/// </summary>
			public static readonly SimpleInterval2 Empty = new SimpleInterval2(0f, 0f, 0f, 0f);

			#endregion

			#region Variables

			#region Properties

			/// <summary>
			/// Gets the center point of the interval as a Vector2.
			/// Calculated as the average of the endpoints for both X and Y intervals.
			/// </summary>
			public readonly Vector2 CenterVector2 => new Vector2((x.a + x.b) * .5f, (y.a + y.b) * .5f);

			/// <summary>
			/// Gets the center point of the interval as a float2.
			/// Calculated as the average of the endpoints for both X and Y intervals.
			/// Useful for DOTS and mathematics operations.
			/// </summary>
			public readonly float2 CenterFloat2 => new float2((x.a + x.b) * .5f, (y.a + y.b) * .5f);

			#endregion

			#region Fields

			/// <summary>
			/// The interval representing the X-axis range.
			/// Unlike regular Interval, SimpleInterval does not enforce that a ≤ b.
			/// </summary>
			public SimpleInterval x;

			/// <summary>
			/// The interval representing the Y-axis range.
			/// Unlike regular Interval, SimpleInterval does not enforce that a ≤ b.
			/// </summary>
			public SimpleInterval y;

			#endregion

			#endregion

			#region Methods

			#region Virtual Methods

			/// <summary>
			/// Determines whether the specified object is equal to the current SimpleInterval2.
			/// Two SimpleInterval2 instances are considered equal if both their X and Y intervals are identical.
			/// </summary>
			/// <param name="obj">The object to compare with the current SimpleInterval2.</param>
			/// <returns>true if the specified object is equal to the current SimpleInterval2; otherwise, false.</returns>
			public readonly override bool Equals(object obj)
			{
				return obj is SimpleInterval2 interval2 &&
					x.Equals(interval2.x) &&
					y.Equals(interval2.y);
			}

			/// <summary>
			/// Serves as the default hash function for SimpleInterval2.
			/// Returns a hash code for the current SimpleInterval2 based on its X and Y intervals.
			/// Uses different hash code generation methods depending on the Unity version.
			/// </summary>
			/// <returns>A hash code for the current SimpleInterval2.</returns>
			public readonly override int GetHashCode()
			{
#if UNITY_2021_2_OR_NEWER
				return HashCode.Combine(x, y);
#else
				int hashCode = 1502939027;

				hashCode = hashCode * -1521134295 + x.GetHashCode();
				hashCode = hashCode * -1521134295 + y.GetHashCode();

				return hashCode;
#endif
			}

			#endregion

			#region Global Methods

			/// <summary>
			/// Determines whether the specified float value falls within both X and Y intervals.
			/// A value is considered in range if it is within both the X and Y intervals simultaneously.
			/// </summary>
			/// <param name="value">The float value to check.</param>
			/// <returns>true if the value is within both X and Y intervals; otherwise, false.</returns>
			public readonly bool InRange(float value)
			{
				return x.InRange(value) && y.InRange(value);
			}

			/// <summary>
			/// Determines whether the specified Vector2 falls within the 2D interval region.
			/// Checks if the X component is within the X interval and the Y component is within the Y interval.
			/// </summary>
			/// <param name="value">The Vector2 value to check.</param>
			/// <returns>true if the Vector2 is within the 2D interval region; otherwise, false.</returns>
			public readonly bool InRange(Vector2 value)
			{
				return x.InRange(value.x) && y.InRange(value.y);
			}

			/// <summary>
			/// Determines whether the specified float2 falls within the 2D interval region.
			/// Checks if the X component is within the X interval and the Y component is within the Y interval.
			/// Useful for DOTS and mathematics operations.
			/// </summary>
			/// <param name="value">The float2 value to check.</param>
			/// <returns>true if the float2 is within the 2D interval region; otherwise, false.</returns>
			public readonly bool InRange(float2 value)
			{
				return x.InRange(value.x) && y.InRange(value.y);
			}

			/// <summary>
			/// Expands the interval to include the specified Vector2 point.
			/// Modifies both X and Y intervals to ensure they contain the given point.
			/// </summary>
			/// <param name="value">The Vector2 point to include in the interval.</param>
			public void Encapsulate(Vector2 value)
			{
				x.Encapsulate(value.x);
				y.Encapsulate(value.y);
			}

			/// <summary>
			/// Expands the interval to include the specified float2 point.
			/// Modifies both X and Y intervals to ensure they contain the given point.
			/// Useful for DOTS and mathematics operations.
			/// </summary>
			/// <param name="value">The float2 point to include in the interval.</param>
			public void Encapsulate(float2 value)
			{
				x.Encapsulate(value.x);
				y.Encapsulate(value.y);
			}

			/// <summary>
			/// Expands the interval to include the specified X and Y coordinates.
			/// Modifies both X and Y intervals to ensure they contain the given point.
			/// </summary>
			/// <param name="x">The X coordinate to include in the X interval.</param>
			/// <param name="y">The Y coordinate to include in the Y interval.</param>
			public void Encapsulate(float x, float y)
			{
				this.x.Encapsulate(x);
				this.y.Encapsulate(y);
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the SimpleInterval2 struct with specified endpoints for X and Y axes.
			/// Creates a rectangular region defined by the given boundaries without enforcing min/max ordering.
			/// </summary>
			/// <param name="xA">The first endpoint for the X interval.</param>
			/// <param name="xB">The second endpoint for the X interval.</param>
			/// <param name="yA">The first endpoint for the Y interval.</param>
			/// <param name="yB">The second endpoint for the Y interval.</param>
			public SimpleInterval2(float xA, float xB, float yA, float yB)
			{
				x = new SimpleInterval(xA, xB);
				y = new SimpleInterval(yA, yB);
			}

			/// <summary>
			/// Initializes a new instance of the SimpleInterval2 struct with specified X and Y SimpleIntervals.
			/// Creates a rectangular region defined by the given interval components.
			/// </summary>
			/// <param name="x">The SimpleInterval for the X axis.</param>
			/// <param name="y">The SimpleInterval for the Y axis.</param>
			public SimpleInterval2(SimpleInterval x, SimpleInterval y)
			{
				this.x = x;
				this.y = y;
			}

			/// <summary>
			/// Initializes a new instance of the SimpleInterval2 struct by copying another SimpleInterval2.
			/// Creates a deep copy where both X and Y intervals are cloned.
			/// </summary>
			/// <param name="interval">The SimpleInterval2 to copy.</param>
			public SimpleInterval2(SimpleInterval2 interval)
			{
				x = new SimpleInterval(interval.x);
				y = new SimpleInterval(interval.y);
			}

			#endregion

			#region Operators

			/// <summary>
			/// Determines whether two SimpleInterval2 instances are equal.
			/// Two SimpleInterval2 instances are considered equal if both their X and Y intervals are identical.
			/// </summary>
			/// <param name="a">The first SimpleInterval2 to compare.</param>
			/// <param name="b">The second SimpleInterval2 to compare.</param>
			/// <returns>true if the SimpleInterval2 instances are equal; otherwise, false.</returns>
			public static bool operator ==(SimpleInterval2 a, SimpleInterval2 b)
			{
				return a.Equals(b);
			}

			/// <summary>
			/// Determines whether two SimpleInterval2 instances are not equal.
			/// Two SimpleInterval2 instances are considered not equal if either their X or Y intervals differ.
			/// </summary>
			/// <param name="a">The first SimpleInterval2 to compare.</param>
			/// <param name="b">The second SimpleInterval2 to compare.</param>
			/// <returns>true if the SimpleInterval2 instances are not equal; otherwise, false.</returns>
			public static bool operator !=(SimpleInterval2 a, SimpleInterval2 b)
			{
				return !(a == b);
			}

			/// <summary>
			/// Implicitly converts a SimpleInterval2 to a SimpleInterval by extracting its X component.
			/// This allows using a 2D interval in contexts where a 1D interval is expected.
			/// </summary>
			/// <param name="interval">The SimpleInterval2 to convert.</param>
			/// <returns>A new SimpleInterval containing the X component of the SimpleInterval2.</returns>
			public static implicit operator SimpleInterval(SimpleInterval2 interval)
			{
				return new SimpleInterval(interval.x);
			}

			#endregion

			#endregion
		}
		/// <summary>
		/// Represents a 3D interval with separate X, Y, and Z ranges.
		/// Useful for defining cuboid regions and performing range checks in 3D space.
		/// </summary>
		[Serializable]
		public struct Interval3
		{
			#region Variables

			/// <summary>
			/// The interval representing the X-axis range.
			/// Defines the minimum and maximum values along the horizontal dimension.
			/// </summary>
			public Interval x;

			/// <summary>
			/// The interval representing the Y-axis range.
			/// Defines the minimum and maximum values along the vertical dimension.
			/// </summary>
			public Interval y;

			/// <summary>
			/// The interval representing the Z-axis range.
			/// Defines the minimum and maximum values along the depth dimension.
			/// </summary>
			public Interval z;

			#endregion

			#region Methods

			#region Virtual Methods

			/// <summary>
			/// Determines whether the specified object is equal to the current Interval3.
			/// Two Interval3 instances are considered equal if all their X, Y, and Z intervals are identical.
			/// </summary>
			/// <param name="obj">The object to compare with the current Interval3.</param>
			/// <returns>true if the specified object is equal to the current Interval3; otherwise, false.</returns>
			public override readonly bool Equals(object obj)
			{
				return obj is Interval3 interval &&
					   EqualityComparer<Interval>.Default.Equals(x, interval.x) &&
					   EqualityComparer<Interval>.Default.Equals(y, interval.y) &&
					   EqualityComparer<Interval>.Default.Equals(z, interval.z);
			}

			/// <summary>
			/// Serves as the default hash function for Interval3.
			/// Returns a hash code for the current Interval3 based on its X, Y, and Z intervals.
			/// Uses different hash code generation methods depending on the Unity version.
			/// </summary>
			/// <returns>A hash code for the current Interval3.</returns>
			public override readonly int GetHashCode()
			{
#if UNITY_2021_2_OR_NEWER
				return HashCode.Combine(x, y, z);
#else
				int hashCode = 373119288;

				hashCode = hashCode * -1521134295 + x.GetHashCode();
				hashCode = hashCode * -1521134295 + y.GetHashCode();
				hashCode = hashCode * -1521134295 + z.GetHashCode();

				return hashCode;
#endif
			}

			#endregion

			#region Global Methods

			/// <summary>
			/// Determines whether the specified float value falls within all three intervals (X, Y, and Z).
			/// A value is considered in range if it is within the range of all three component intervals.
			/// </summary>
			/// <param name="value">The float value to check.</param>
			/// <returns>true if the value is within all three intervals' ranges; otherwise, false.</returns>
			public readonly bool InRange(float value)
			{
				return x.InRange(value) && y.InRange(value) && z.InRange(value);
			}

			/// <summary>
			/// Determines whether the specified Vector3 falls within the 3D interval.
			/// Each component of the vector is checked against its corresponding interval.
			/// </summary>
			/// <param name="value">The Vector3 to check.</param>
			/// <returns>true if all components of the vector are within their respective intervals; otherwise, false.</returns>
			public readonly bool InRange(Vector3 value)
			{
				return x.InRange(value.x) && y.InRange(value.y) && z.InRange(value.z);
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the Interval3 structure with the specified minimum and maximum values for each axis.
			/// </summary>
			/// <param name="xMin">The minimum value for the X interval.</param>
			/// <param name="xMax">The maximum value for the X interval.</param>
			/// <param name="yMin">The minimum value for the Y interval.</param>
			/// <param name="yMax">The maximum value for the Y interval.</param>
			/// <param name="zMin">The minimum value for the Z interval.</param>
			/// <param name="zMax">The maximum value for the Z interval.</param>
			public Interval3(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
			{
				x = new Interval(xMin, xMax);
				y = new Interval(yMin, yMax);
				z = new Interval(zMin, zMax);
			}

			/// <summary>
			/// Initializes a new instance of the Interval3 structure with the specified intervals for each axis.
			/// </summary>
			/// <param name="x">The interval for the X axis.</param>
			/// <param name="y">The interval for the Y axis.</param>
			/// <param name="z">The interval for the Z axis.</param>
			public Interval3(Interval x, Interval y, Interval z)
			{
				this.x = x;
				this.y = y;
				this.z = z;
			}

			/// <summary>
			/// Initializes a new instance of the Interval3 structure by copying another Interval3.
			/// Creates deep copies of each component interval.
			/// </summary>
			/// <param name="interval">The Interval3 to copy.</param>
			public Interval3(Interval3 interval)
			{
				x = new Interval(interval.x);
				y = new Interval(interval.y);
				z = new Interval(interval.z);
			}

			#endregion

			#region Operators

			/// <summary>
			/// Determines whether two Interval3 instances are equal.
			/// Two Interval3 instances are considered equal if all their X, Y, and Z intervals are identical.
			/// </summary>
			/// <param name="a">The first Interval3 to compare.</param>
			/// <param name="b">The second Interval3 to compare.</param>
			/// <returns>true if the Interval3 instances are equal; otherwise, false.</returns>
			public static bool operator ==(Interval3 a, Interval3 b)
			{
				return a.Equals(b);
			}

			/// <summary>
			/// Determines whether two Interval3 instances are not equal.
			/// Two Interval3 instances are considered not equal if any of their X, Y, or Z intervals differ.
			/// </summary>
			/// <param name="a">The first Interval3 to compare.</param>
			/// <param name="b">The second Interval3 to compare.</param>
			/// <returns>true if the Interval3 instances are not equal; otherwise, false.</returns>
			public static bool operator !=(Interval3 a, Interval3 b)
			{
				return !(a == b);
			}

			/// <summary>
			/// Implicitly converts an Interval3 to an Interval2 by extracting its X and Y components.
			/// This allows using a 3D interval in contexts where a 2D interval is expected.
			/// </summary>
			/// <param name="interval">The Interval3 to convert.</param>
			/// <returns>A new Interval2 containing the X and Y components of the Interval3.</returns>
			public static implicit operator Interval2(Interval3 interval)
			{
				return new Interval2(interval.x, interval.y);
			}

			/// <summary>
			/// Implicitly converts an Interval3 to an Interval by extracting its X component.
			/// This allows using a 3D interval in contexts where a 1D interval is expected.
			/// </summary>
			/// <param name="interval">The Interval3 to convert.</param>
			/// <returns>A new Interval containing the X component of the Interval3.</returns>
			public static implicit operator Interval(Interval3 interval)
			{
				return new Interval(interval.x);
			}

			#endregion

			#endregion
		}
		/// <summary>
		/// A simplified 3D interval structure that represents a cuboid region without enforcing min/max ordering.
		/// Provides methods for range checking, interpolation, and region manipulation in 3D space.
		/// </summary>
		[Serializable]
		public struct SimpleInterval3
		{
			#region Constants

			/// <summary>
			/// Represents an empty SimpleInterval3 with all components set to zero.
			/// Useful as a default value or for initialization.
			/// </summary>
			public static readonly SimpleInterval3 Empty = new SimpleInterval3(0f, 0f, 0f, 0f, 0f, 0f);

			#endregion

			#region Variables

			#region Properties

			/// <summary>
			/// Gets the center point of the 3D interval as a Vector3.
			/// Calculated as the average of the endpoints for each axis.
			/// </summary>
			public readonly Vector3 CenterVector3 => new Vector3((x.a + x.b) * .5f, (y.a + y.b) * .5f, (z.a + z.b) * .5f);

			/// <summary>
			/// Gets the center point of the 3D interval as a float3.
			/// Calculated as the average of the endpoints for each axis.
			/// Useful for Unity.Mathematics integration.
			/// </summary>
			public readonly float3 CenterFloat3 => new float3((x.a + x.b) * .5f, (y.a + y.b) * .5f, (z.a + z.b) * .5f);

			#endregion

			#region Fields

			/// <summary>
			/// The interval representing the X-axis range.
			/// Defines two boundary values along the horizontal dimension without enforcing min/max ordering.
			/// </summary>
			public SimpleInterval x;

			/// <summary>
			/// The interval representing the Y-axis range.
			/// Defines two boundary values along the vertical dimension without enforcing min/max ordering.
			/// </summary>
			public SimpleInterval y;

			/// <summary>
			/// The interval representing the Z-axis range.
			/// Defines two boundary values along the depth dimension without enforcing min/max ordering.
			/// </summary>
			public SimpleInterval z;

			#endregion

			#endregion

			#region Methods

			#region Virtual Methods

			/// <summary>
			/// Determines whether the specified object is equal to the current SimpleInterval3.
			/// Two SimpleInterval3 instances are considered equal if all their X, Y, and Z intervals are identical.
			/// </summary>
			/// <param name="obj">The object to compare with the current SimpleInterval3.</param>
			/// <returns>true if the specified object is equal to the current SimpleInterval3; otherwise, false.</returns>
			public readonly override bool Equals(object obj)
			{
				return obj is SimpleInterval3 interval3 &&
					x.Equals(interval3.x) &&
					y.Equals(interval3.y) &&
					z.Equals(interval3.z);
			}

			/// <summary>
			/// Serves as the default hash function for SimpleInterval3.
			/// Returns a hash code for the current SimpleInterval3 based on its X, Y, and Z intervals.
			/// Uses different hash code generation methods depending on the Unity version.
			/// </summary>
			/// <returns>A hash code for the current SimpleInterval3.</returns>
			public readonly override int GetHashCode()
			{
#if UNITY_2021_2_OR_NEWER
				return HashCode.Combine(x, y);
#else
				int hashCode = 1502939027;

				hashCode = hashCode * -1521134295 + x.GetHashCode();
				hashCode = hashCode * -1521134295 + y.GetHashCode();
				hashCode = hashCode * -1521134295 + z.GetHashCode();

				return hashCode;
#endif
			}

			#endregion

			#region Global Methods

			/// <summary>
			/// Determines whether the specified float value falls within both X and Y intervals.
			/// Note: This method only checks X and Y intervals, not Z.
			/// </summary>
			/// <param name="value">The float value to check.</param>
			/// <returns>true if the value is within both X and Y intervals' ranges; otherwise, false.</returns>
			public readonly bool InRange(float value)
			{
				return x.InRange(value) && y.InRange(value);
			}

			/// <summary>
			/// Determines whether the specified Vector3 falls within the 3D interval.
			/// Each component of the vector is checked against its corresponding interval.
			/// </summary>
			/// <param name="value">The Vector3 to check.</param>
			/// <returns>true if all components of the vector are within their respective intervals; otherwise, false.</returns>
			public readonly bool InRange(Vector3 value)
			{
				return x.InRange(value.x) && y.InRange(value.y) && z.InRange(value.z);
			}

			/// <summary>
			/// Determines whether the specified float3 falls within the 3D interval.
			/// Each component of the float3 is checked against its corresponding interval.
			/// Useful for Unity.Mathematics integration.
			/// </summary>
			/// <param name="value">The float3 to check.</param>
			/// <returns>true if all components of the float3 are within their respective intervals; otherwise, false.</returns>
			public readonly bool InRange(float3 value)
			{
				return x.InRange(value.x) && y.InRange(value.y) && z.InRange(value.z);
			}

			/// <summary>
			/// Expands the interval to include the specified Vector3 point.
			/// Each component interval is expanded to include the corresponding component of the point.
			/// </summary>
			/// <param name="value">The Vector3 point to include in the interval.</param>
			public void Encapsulate(Vector3 value)
			{
				x.Encapsulate(value.x);
				y.Encapsulate(value.y);
				z.Encapsulate(value.z);
			}

			/// <summary>
			/// Expands the interval to include the specified float3 point.
			/// Each component interval is expanded to include the corresponding component of the point.
			/// Useful for Unity.Mathematics integration.
			/// </summary>
			/// <param name="value">The float3 point to include in the interval.</param>
			public void Encapsulate(float3 value)
			{
				x.Encapsulate(value.x);
				y.Encapsulate(value.y);
				z.Encapsulate(value.z);
			}

			/// <summary>
			/// Expands the interval to include the specified point defined by individual coordinates.
			/// Each component interval is expanded to include the corresponding coordinate value.
			/// </summary>
			/// <param name="x">The x-coordinate to include in the X interval.</param>
			/// <param name="y">The y-coordinate to include in the Y interval.</param>
			/// <param name="z">The z-coordinate to include in the Z interval.</param>
			public void Encapsulate(float x, float y, float z)
			{
				this.x.Encapsulate(x);
				this.y.Encapsulate(y);
				this.z.Encapsulate(z);
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the SimpleInterval3 structure with the specified boundary values for each axis.
			/// Does not enforce min/max ordering of the boundary values.
			/// </summary>
			/// <param name="xA">The first boundary value for the X interval.</param>
			/// <param name="xB">The second boundary value for the X interval.</param>
			/// <param name="yA">The first boundary value for the Y interval.</param>
			/// <param name="yB">The second boundary value for the Y interval.</param>
			/// <param name="zA">The first boundary value for the Z interval.</param>
			/// <param name="zB">The second boundary value for the Z interval.</param>
			public SimpleInterval3(float xA, float xB, float yA, float yB, float zA, float zB)
			{
				x = new SimpleInterval(xA, xB);
				y = new SimpleInterval(yA, yB);
				z = new SimpleInterval(zA, zB);
			}

			/// <summary>
			/// Initializes a new instance of the SimpleInterval3 structure with the specified boundary values for X and Y axes,
			/// and sets Z interval to zero. Useful for creating a 2D interval with a flat Z dimension.
			/// </summary>
			/// <param name="xA">The first boundary value for the X interval.</param>
			/// <param name="xB">The second boundary value for the X interval.</param>
			/// <param name="yA">The first boundary value for the Y interval.</param>
			/// <param name="yB">The second boundary value for the Y interval.</param>
			public SimpleInterval3(float xA, float xB, float yA, float yB)
			{
				x = new SimpleInterval(xA, xB);
				y = new SimpleInterval(yA, yB);
				z = new SimpleInterval(0f, 0f);
			}

			/// <summary>
			/// Initializes a new instance of the SimpleInterval3 structure with the specified intervals for each axis.
			/// </summary>
			/// <param name="x">The interval for the X axis.</param>
			/// <param name="y">The interval for the Y axis.</param>
			/// <param name="z">The interval for the Z axis.</param>
			public SimpleInterval3(SimpleInterval x, SimpleInterval y, SimpleInterval z)
			{
				this.x = x;
				this.y = y;
				this.z = z;
			}

			/// <summary>
			/// Initializes a new instance of the SimpleInterval3 structure by copying another SimpleInterval3.
			/// Creates deep copies of each component interval.
			/// </summary>
			/// <param name="interval">The SimpleInterval3 to copy.</param>
			public SimpleInterval3(SimpleInterval3 interval)
			{
				x = new SimpleInterval(interval.x);
				y = new SimpleInterval(interval.y);
				z = new SimpleInterval(interval.z);
			}

			#endregion

			#region Operators

			/// <summary>
			/// Determines whether two SimpleInterval3 instances are equal.
			/// Two SimpleInterval3 instances are considered equal if all their X, Y, and Z intervals are identical.
			/// </summary>
			/// <param name="a">The first SimpleInterval3 to compare.</param>
			/// <param name="b">The second SimpleInterval3 to compare.</param>
			/// <returns>true if the SimpleInterval3 instances are equal; otherwise, false.</returns>
			public static bool operator ==(SimpleInterval3 a, SimpleInterval3 b)
			{
				return a.Equals(b);
			}

			/// <summary>
			/// Determines whether two SimpleInterval3 instances are not equal.
			/// Two SimpleInterval3 instances are considered not equal if any of their X, Y, or Z intervals differ.
			/// </summary>
			/// <param name="a">The first SimpleInterval3 to compare.</param>
			/// <param name="b">The second SimpleInterval3 to compare.</param>
			/// <returns>true if the SimpleInterval3 instances are not equal; otherwise, false.</returns>
			public static bool operator !=(SimpleInterval3 a, SimpleInterval3 b)
			{
				return !(a == b);
			}

			/// <summary>
			/// Implicitly converts a SimpleInterval3 to a SimpleInterval2 by extracting its X and Y components.
			/// This allows using a 3D interval in contexts where a 2D interval is expected.
			/// </summary>
			/// <param name="interval">The SimpleInterval3 to convert.</param>
			/// <returns>A new SimpleInterval2 containing the X and Y components of the SimpleInterval3.</returns>
			public static implicit operator SimpleInterval2(SimpleInterval3 interval)
			{
				return new SimpleInterval2(interval.x, interval.y);
			}

			#endregion

			#endregion
		}
		/// <summary>
		/// Represents a 2D vector with separate X and Y components.
		/// Provides methods for vector operations and serialization.
		/// Allows for easy conversion between Unity's Vector2, Mathematics float2, and this serializable format.
		/// </summary>
		[Serializable]
		public struct SerializableVector2
		{
			#region Variables

			/// <summary>
			/// The X component of the vector.
			/// Represents the horizontal coordinate in a 2D space.
			/// </summary>
			public float x;

			/// <summary>
			/// The Y component of the vector.
			/// Represents the vertical coordinate in a 2D space.
			/// </summary>
			public float y;

			#endregion

			#region Constructors & Operators

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the SerializableVector2 structure with the specified X and Y components.
			/// </summary>
			/// <param name="x">The X component of the vector.</param>
			/// <param name="y">The Y component of the vector.</param>
			public SerializableVector2(float x, float y)
			{
				this.x = x;
				this.y = y;
			}

			/// <summary>
			/// Initializes a new instance of the SerializableVector2 structure from a Unity Vector2.
			/// Copies the X and Y components from the provided Vector2.
			/// </summary>
			/// <param name="vector">The Unity Vector2 to convert.</param>
			public SerializableVector2(Vector2 vector)
			{
				x = vector.x;
				y = vector.y;
			}

			/// <summary>
			/// Initializes a new instance of the SerializableVector2 structure from a Mathematics float2.
			/// Copies the X and Y components from the provided float2.
			/// </summary>
			/// <param name="vector">The Mathematics float2 to convert.</param>
			public SerializableVector2(float2 vector)
			{
				x = vector.x;
				y = vector.y;
			}

			#endregion

			#region Operators

			/// <summary>
			/// Multiplies a SerializableVector2 by a scalar value.
			/// Scales both X and Y components by the specified value.
			/// </summary>
			/// <param name="a">The SerializableVector2 to multiply.</param>
			/// <param name="b">The scalar value to multiply by.</param>
			/// <returns>A new SerializableVector2 with scaled components.</returns>
			public static SerializableVector2 operator *(SerializableVector2 a, float b)
			{
				return new SerializableVector2(new Vector2(a.x, a.y) * b);
			}

			/// <summary>
			/// Adds a scalar value to a SerializableVector2.
			/// Adds the specified value to both X and Y components.
			/// </summary>
			/// <param name="a">The SerializableVector2 to add to.</param>
			/// <param name="b">The scalar value to add.</param>
			/// <returns>A new SerializableVector2 with the scalar added to each component.</returns>
			public static SerializableVector2 operator +(SerializableVector2 a, float b)
			{
				return new SerializableVector2(new Vector2(a.x + b, a.y + b));
			}

			/// <summary>
			/// Multiplies two SerializableVector2 instances component-wise.
			/// Multiplies the X components together and the Y components together.
			/// </summary>
			/// <param name="a">The first SerializableVector2.</param>
			/// <param name="b">The second SerializableVector2.</param>
			/// <returns>A new SerializableVector2 with component-wise multiplication results.</returns>
			public static SerializableVector2 operator *(SerializableVector2 a, SerializableVector2 b)
			{
				return new SerializableVector2(a.x * b.x, a.y * b.y);
			}

			/// <summary>
			/// Adds two SerializableVector2 instances component-wise.
			/// Adds the X components together and the Y components together.
			/// </summary>
			/// <param name="a">The first SerializableVector2.</param>
			/// <param name="b">The second SerializableVector2.</param>
			/// <returns>A new SerializableVector2 with component-wise addition results.</returns>
			public static SerializableVector2 operator +(SerializableVector2 a, SerializableVector2 b)
			{
				return new SerializableVector2(a.x + b.x, a.y + b.y);
			}

			/// <summary>
			/// Implicitly converts a SerializableVector2 to a Unity Vector2.
			/// Creates a new Vector2 with the same X and Y components.
			/// </summary>
			/// <param name="vector">The SerializableVector2 to convert.</param>
			/// <returns>A new Unity Vector2 with the same component values.</returns>
			public static implicit operator Vector2(SerializableVector2 vector)
			{
				return new Vector2(vector.x, vector.y);
			}

			/// <summary>
			/// Implicitly converts a Unity Vector2 to a SerializableVector2.
			/// Creates a new SerializableVector2 with the same X and Y components.
			/// </summary>
			/// <param name="vector">The Unity Vector2 to convert.</param>
			/// <returns>A new SerializableVector2 with the same component values.</returns>
			public static implicit operator SerializableVector2(Vector2 vector)
			{
				return new SerializableVector2(vector);
			}

			/// <summary>
			/// Implicitly converts a SerializableVector2 to a Mathematics float2.
			/// Creates a new float2 with the same X and Y components.
			/// </summary>
			/// <param name="vector">The SerializableVector2 to convert.</param>
			/// <returns>A new Mathematics float2 with the same component values.</returns>
			public static implicit operator float2(SerializableVector2 vector)
			{
				return new float2(vector.x, vector.y);
			}

			/// <summary>
			/// Implicitly converts a Mathematics float2 to a SerializableVector2.
			/// Creates a new SerializableVector2 with the same X and Y components.
			/// </summary>
			/// <param name="vector">The Mathematics float2 to convert.</param>
			/// <returns>A new SerializableVector2 with the same component values.</returns>
			public static implicit operator SerializableVector2(float2 vector)
			{
				return new SerializableVector2(vector);
			}

			#endregion

			#endregion
		}
		/// <summary>
		/// Represents a 2D rectangle with separate X, Y, width, and height components.
		/// Provides methods for rectangle operations, point containment testing, and serialization.
		/// Allows for easy conversion between Unity's Rect and this serializable format.
		/// </summary>
		[Serializable]
		public struct SerializableRect
		{
			#region Variables

			/// <summary>
			/// The X coordinate of the rectangle's position.
			/// Represents the horizontal position of the rectangle's origin (typically the bottom-left corner).
			/// </summary>
			public float x;

			/// <summary>
			/// The Y coordinate of the rectangle's position.
			/// Represents the vertical position of the rectangle's origin (typically the bottom-left corner).
			/// </summary>
			public float y;

			/// <summary>
			/// The width of the rectangle.
			/// Represents the horizontal size of the rectangle.
			/// </summary>
			public float width;

			/// <summary>
			/// The height of the rectangle.
			/// Represents the vertical size of the rectangle.
			/// </summary>
			public float height;

			/// <summary>
			/// The position of the rectangle as a SerializableVector2.
			/// Provides access to the rectangle's position as a vector.
			/// </summary>
			public SerializableVector2 position;

			/// <summary>
			/// The size of the rectangle as a SerializableVector2.
			/// Provides access to the rectangle's dimensions as a vector.
			/// </summary>
			public SerializableVector2 size;

			#endregion

			#region Methods

			/// <summary>
			/// Determines whether the rectangle contains the specified 2D point.
			/// A point is considered inside the rectangle if it is between the rectangle's minimum and maximum bounds.
			/// </summary>
			/// <param name="point">The 2D point to check.</param>
			/// <returns>true if the point is inside the rectangle; otherwise, false.</returns>
			public readonly bool Contains(Vector2 point)
			{
				return new Rect(x, y, width, height).Contains(point);
			}

			/// <summary>
			/// Determines whether the rectangle contains the specified 3D point (ignoring the Z component).
			/// A point is considered inside the rectangle if its X and Y coordinates are between the rectangle's minimum and maximum bounds.
			/// </summary>
			/// <param name="point">The 3D point to check.</param>
			/// <returns>true if the point is inside the rectangle; otherwise, false.</returns>
			public readonly bool Contains(Vector3 point)
			{
				return new Rect(x, y, width, height).Contains(point);
			}

			/// <summary>
			/// Determines whether the rectangle contains the specified 3D point (ignoring the Z component),
			/// with an option to consider the rectangle as inverted.
			/// When allowInverse is true, the rectangle can have negative width or height.
			/// </summary>
			/// <param name="point">The 3D point to check.</param>
			/// <param name="allowInverse">Whether to allow the rectangle to have negative width or height.</param>
			/// <returns>true if the point is inside the rectangle; otherwise, false.</returns>
			public readonly bool Contains(Vector3 point, bool allowInverse)
			{
				return new Rect(x, y, width, height).Contains(point, allowInverse);
			}

			#endregion

			#region Constructors & Operators

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the SerializableRect structure from a Unity Rect.
			/// Copies the position, size, width, and height from the provided Rect.
			/// </summary>
			/// <param name="rect">The Unity Rect to convert.</param>
			public SerializableRect(Rect rect)
			{
				x = rect.x;
				y = rect.y;
				width = rect.width;
				height = rect.height;
				position = rect.position;
				size = rect.size;
			}

			#endregion

			#region Operators

			/// <summary>
			/// Implicitly converts a SerializableRect to a Unity Rect.
			/// Creates a new Rect with the same position and size components.
			/// </summary>
			/// <param name="rect">The SerializableRect to convert.</param>
			/// <returns>A new Unity Rect with the same position and size values.</returns>
			public static implicit operator Rect(SerializableRect rect)
			{
				return new Rect(rect.x, rect.y, rect.width, rect.height);
			}

			/// <summary>
			/// Implicitly converts a Unity Rect to a SerializableRect.
			/// Creates a new SerializableRect with the same position and size components.
			/// </summary>
			/// <param name="rect">The Unity Rect to convert.</param>
			/// <returns>A new SerializableRect with the same position and size values.</returns>
			public static implicit operator SerializableRect(Rect rect)
			{
				return new SerializableRect(rect);
			}

			#endregion

			#endregion
		}
		/// <summary>
		/// Represents a color sheet with a name, color, metallic, and smoothness value.
		/// Provides methods for setting the material properties of a color sheet.
		/// Useful for defining and applying consistent material appearances across objects.
		/// </summary>
		[Serializable]
		public struct ColorSheet
		{
			#region Variables

			/// <summary>
			/// The name of the color sheet.
			/// Used for identification and reference purposes.
			/// </summary>
			public string name;

			/// <summary>
			/// The color of the sheet as a SerializableColor.
			/// Defines the base color or albedo of the material.
			/// </summary>
			public SerializableColor color;

			/// <summary>
			/// The metallic value of the sheet.
			/// Controls how metallic the material appears, typically ranging from 0 (non-metallic) to 1 (fully metallic).
			/// </summary>
			public float metallic;

			/// <summary>
			/// The smoothness value of the sheet.
			/// Controls how smooth/glossy the material appears, typically ranging from 0 (rough) to 1 (smooth).
			/// </summary>
			public float smoothness;

			#endregion

			#region Methods

			#region Virtual Methods

			/// <summary>
			/// Determines whether the specified object is equal to the current ColorSheet.
			/// Two ColorSheet instances are considered equal if they have identical name, color, metallic, and smoothness values.
			/// </summary>
			/// <param name="obj">The object to compare with the current ColorSheet.</param>
			/// <returns>true if the specified object is equal to the current ColorSheet; otherwise, false.</returns>
			public override readonly bool Equals(object obj)
			{
				return obj is ColorSheet sheet &&
					name == sheet.name &&
					EqualityComparer<SerializableColor>.Default.Equals(color, sheet.color) &&
					metallic == sheet.metallic &&
					smoothness == sheet.smoothness;
			}

			/// <summary>
			/// Serves as the default hash function for ColorSheet.
			/// Returns a hash code for the current ColorSheet based on its name, color, metallic, and smoothness values.
			/// Uses different hash code generation methods depending on the Unity version.
			/// </summary>
			/// <returns>A hash code for the current ColorSheet.</returns>
			public override readonly int GetHashCode()
			{
#if UNITY_2021_2_OR_NEWER
				return HashCode.Combine(name, color, metallic, smoothness);
#else
				int hashCode = 998921542;

				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
				hashCode = hashCode * -1521134295 + color.GetHashCode();
				hashCode = hashCode * -1521134295 + metallic.GetHashCode();
				hashCode = hashCode * -1521134295 + smoothness.GetHashCode();

				return hashCode;
#endif
			}

			#endregion

			#region Global Methods

			/// <summary>
			/// Applies the color sheet properties to the specified material.
			/// Sets the base color, metallic value, and smoothness properties on the material.
			/// Also sets smoothness remapping values to ensure consistent appearance.
			/// </summary>
			/// <param name="material">The material to modify with the color sheet properties.</param>
			public readonly void SetMaterial(Material material)
			{
				material.SetColor("_BaseColor", color);
				material.SetFloat("_Metallic", metallic);
				material.SetFloat("_Smoothness", smoothness);
				material.SetFloat("_SmoothnessRemapMin", smoothness);
				material.SetFloat("_SmoothnessRemapMax", smoothness);
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the ColorSheet structure with the specified name.
			/// Sets default values for color (white), metallic (0), and smoothness (0.5).
			/// </summary>
			/// <param name="name">The name of the color sheet.</param>
			public ColorSheet(string name)
			{
				this.name = name;
				color = UnityEngine.Color.white;
				metallic = 0f;
				smoothness = .5f;
			}

			/// <summary>
			/// Initializes a new instance of the ColorSheet structure by copying another ColorSheet.
			/// Creates a deep copy of the name, color, metallic, and smoothness values.
			/// </summary>
			/// <param name="sheet">The ColorSheet to copy.</param>
			public ColorSheet(ColorSheet sheet)
			{
				name = sheet.name;
				color = sheet.color;
				metallic = sheet.metallic;
				smoothness = sheet.smoothness;
			}

			#endregion

			#region Operators

			/// <summary>
			/// Implicitly converts a Unity Color to a ColorSheet.
			/// Creates a new ColorSheet with the specified color and default values for other properties.
			/// </summary>
			/// <param name="color">The Unity Color to convert.</param>
			/// <returns>A new ColorSheet with the specified color.</returns>
			public static implicit operator ColorSheet(UnityEngine.Color color)
			{
				return new ColorSheet()
				{
					color = color
				};
			}

			/// <summary>
			/// Implicitly converts a Unity Material to a ColorSheet.
			/// Extracts the base color, metallic, and smoothness properties from the material.
			/// Returns null if the material doesn't use the HDRP/Lit shader.
			/// </summary>
			/// <param name="material">The Unity Material to convert.</param>
			/// <returns>A new ColorSheet with properties from the material, or null if incompatible.</returns>
			public static implicit operator ColorSheet(Material material)
			{
				if (material.shader != Shader.Find("HDRP/Lit"))
					return null;

				return new ColorSheet()
				{
					color = material.GetColor("_BaseColor"),
					metallic = material.GetFloat("_Metallic"),
					smoothness = material.GetFloat("_Smoothness")
				};
			}

			/// <summary>
			/// Determines whether two ColorSheet instances are equal.
			/// Two ColorSheet instances are considered equal if they have identical name, color, metallic, and smoothness values.
			/// </summary>
			/// <param name="sheetA">The first ColorSheet to compare.</param>
			/// <param name="sheetB">The second ColorSheet to compare.</param>
			/// <returns>true if the ColorSheet instances are equal; otherwise, false.</returns>
			public static bool operator ==(ColorSheet sheetA, ColorSheet sheetB)
			{
				return sheetA.name == sheetB.name && sheetA.color == sheetB.color && sheetA.metallic == sheetB.metallic && sheetA.smoothness == sheetB.smoothness;
			}

			/// <summary>
			/// Determines whether two ColorSheet instances are not equal.
			/// Two ColorSheet instances are considered not equal if any of their name, color, metallic, or smoothness values differ.
			/// </summary>
			/// <param name="sheetA">The first ColorSheet to compare.</param>
			/// <param name="sheetB">The second ColorSheet to compare.</param>
			/// <returns>true if the ColorSheet instances are not equal; otherwise, false.</returns>
			public static bool operator !=(ColorSheet sheetA, ColorSheet sheetB)
			{
				return !(sheetA == sheetB);
			}

			#endregion

			#endregion
		}
		/// <summary>
		/// Represents a color with separate red, green, blue, and alpha components.
		/// Provides methods for color comparison, serialization, and conversion between Unity's Color and this serializable format.
		/// </summary>
		[Serializable]
		public struct SerializableColor
		{
			#region Variables

			/// <summary>
			/// The red component of the color, typically in the range of 0 to 1.
			/// </summary>
			public float r;

			/// <summary>
			/// The green component of the color, typically in the range of 0 to 1.
			/// </summary>
			public float g;

			/// <summary>
			/// The blue component of the color, typically in the range of 0 to 1.
			/// </summary>
			public float b;

			/// <summary>
			/// The alpha (transparency) component of the color, typically in the range of 0 to 1.
			/// A value of 0 represents fully transparent, while 1 represents fully opaque.
			/// </summary>
			public float a;

			#endregion

			#region Methods

			/// <summary>
			/// Determines whether the specified object is equal to the current SerializableColor.
			/// Supports comparison with both SerializableColor and UnityEngine.Color objects.
			/// </summary>
			/// <param name="obj">The object to compare with the current SerializableColor.</param>
			/// <returns>true if the specified object is equal to the current SerializableColor; otherwise, false.</returns>
			public override readonly bool Equals(object obj)
			{
				bool equalsColor = obj is SerializableColor color && r == color.r && g == color.g && b == color.b && a == color.a;
				bool equalsUColor = obj is UnityEngine.Color uColor && r == uColor.r && g == uColor.g && b == uColor.b && a == uColor.a;

				return equalsColor || equalsUColor;
			}

			/// <summary>
			/// Serves as the default hash function for SerializableColor.
			/// Returns a hash code for the current SerializableColor based on its r, g, b, and a components.
			/// Uses different hash code generation methods depending on the Unity version.
			/// </summary>
			/// <returns>A hash code for the current SerializableColor.</returns>
			public override readonly int GetHashCode()
			{
#if UNITY_2021_2_OR_NEWER
				return HashCode.Combine(r, g, b, a);
#else
				int hashCode = -490236692;
				
				hashCode = hashCode * -1521134295 + r.GetHashCode();
				hashCode = hashCode * -1521134295 + g.GetHashCode();
				hashCode = hashCode * -1521134295 + b.GetHashCode();
				hashCode = hashCode * -1521134295 + a.GetHashCode();
				
				return hashCode;
#endif
			}

			#endregion

			#region Constructors & Operators

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the SerializableColor structure from a Unity Color.
			/// Copies the r, g, b, and a components from the provided Color.
			/// </summary>
			/// <param name="color">The Unity Color to convert.</param>
			public SerializableColor(UnityEngine.Color color)
			{
				r = color.r;
				g = color.g;
				b = color.b;
				a = color.a;
			}

			#endregion

			#region Operators

			/// <summary>
			/// Implicitly converts a SerializableColor to a Unity Color.
			/// This allows using a SerializableColor in contexts where a Unity Color is expected.
			/// </summary>
			/// <param name="color">The SerializableColor to convert.</param>
			/// <returns>A new Unity Color with the same component values.</returns>
			public static implicit operator UnityEngine.Color(SerializableColor color)
			{
				return new UnityEngine.Color(color.r, color.g, color.b, color.a);
			}

			/// <summary>
			/// Implicitly converts a Unity Color to a SerializableColor.
			/// This allows using a Unity Color in contexts where a SerializableColor is expected.
			/// </summary>
			/// <param name="color">The Unity Color to convert.</param>
			/// <returns>A new SerializableColor with the same component values.</returns>
			public static implicit operator SerializableColor(UnityEngine.Color color)
			{
				return new SerializableColor(color);
			}

			/// <summary>
			/// Determines whether two SerializableColor instances are equal.
			/// Two SerializableColor instances are considered equal if all their r, g, b, and a components are identical.
			/// </summary>
			/// <param name="colorA">The first SerializableColor to compare.</param>
			/// <param name="colorB">The second SerializableColor to compare.</param>
			/// <returns>true if the SerializableColor instances are equal; otherwise, false.</returns>
			public static bool operator ==(SerializableColor colorA, SerializableColor colorB)
			{
				return colorA.Equals(colorB);
			}

			/// <summary>
			/// Determines whether two SerializableColor instances are not equal.
			/// Two SerializableColor instances are considered not equal if any of their r, g, b, or a components differ.
			/// </summary>
			/// <param name="colorA">The first SerializableColor to compare.</param>
			/// <param name="colorB">The second SerializableColor to compare.</param>
			/// <returns>true if the SerializableColor instances are not equal; otherwise, false.</returns>
			public static bool operator !=(SerializableColor colorA, SerializableColor colorB)
			{
				return !(colorA == colorB);
			}

			/// <summary>
			/// Determines whether a SerializableColor and a Unity Color are equal.
			/// They are considered equal if all their r, g, b, and a components are identical.
			/// </summary>
			/// <param name="colorA">The SerializableColor to compare.</param>
			/// <param name="colorB">The Unity Color to compare.</param>
			/// <returns>true if the colors are equal; otherwise, false.</returns>
			public static bool operator ==(SerializableColor colorA, UnityEngine.Color colorB)
			{
				return colorA.Equals(colorB);
			}

			/// <summary>
			/// Determines whether a SerializableColor and a Unity Color are not equal.
			/// They are considered not equal if any of their r, g, b, or a components differ.
			/// </summary>
			/// <param name="colorA">The SerializableColor to compare.</param>
			/// <param name="colorB">The Unity Color to compare.</param>
			/// <returns>true if the colors are not equal; otherwise, false.</returns>
			public static bool operator !=(SerializableColor colorA, UnityEngine.Color colorB)
			{
				return !(colorA == colorB);
			}

			#endregion

			#endregion
		}
		/// <summary>
		/// Represents an audio clip with a resource path and a reference to the audio clip.
		/// Provides methods for loading the audio clip from a resource path and reloading it when needed.
		/// Supports implicit conversion to AudioClip and comparison operations.
		/// </summary>
		[Serializable]
		public class SerializableAudioClip
		{
			#region Variables

			/// <summary>
			/// The path to the audio clip resource in the Resources folder.
			/// Used to load or reload the audio clip when needed.
			/// </summary>
			public string resourcePath;

			/// <summary>
			/// Gets the AudioClip referenced by this SerializableAudioClip.
			/// Automatically reloads the clip if it's not loaded or if the resource path has changed.
			/// </summary>
			public AudioClip Clip
			{
				get
				{
					if (!clip || resourcePath != path)
						Reload();

					return clip;
				}
			}

			/// <summary>
			/// The cached reference to the loaded AudioClip.
			/// </summary>
			private AudioClip clip;

			/// <summary>
			/// The cached resource path used to load the current clip.
			/// Used to detect when the resourcePath has changed and the clip needs to be reloaded.
			/// </summary>
			private string path;

			#endregion

			#region Methods

			#region Virtual Methods

			/// <summary>
			/// Determines whether the specified object is equal to the current SerializableAudioClip.
			/// Supports comparison with both SerializableAudioClip and AudioClip objects.
			/// </summary>
			/// <param name="obj">The object to compare with the current SerializableAudioClip.</param>
			/// <returns>true if the specified object is equal to the current SerializableAudioClip; otherwise, false.</returns>
			public override bool Equals(object obj)
			{
				bool equalsClip = obj is SerializableAudioClip clip && clip.Clip == Clip;
				bool equalsUClip = obj is AudioClip uClip && uClip == Clip;

				return equalsClip || equalsUClip;
			}

			/// <summary>
			/// Serves as the default hash function for SerializableAudioClip.
			/// Returns a hash code for the current SerializableAudioClip based on its Clip property.
			/// </summary>
			/// <returns>A hash code for the current SerializableAudioClip.</returns>
			public override int GetHashCode()
			{
				return -2053173677 + EqualityComparer<AudioClip>.Default.GetHashCode(Clip);
			}

			#endregion

			#region Global Methods

			/// <summary>
			/// Reloads the AudioClip from the resource path.
			/// Updates the cached path and clip reference.
			/// Called automatically when the clip is accessed but not loaded or when the resource path changes.
			/// </summary>
			private void Reload()
			{
				path = resourcePath;
				clip = Resources.Load(path) as AudioClip;
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the SerializableAudioClip class with the specified resource path.
			/// Immediately loads the AudioClip from the provided path.
			/// </summary>
			/// <param name="path">The path to the audio clip resource in the Resources folder.</param>
			public SerializableAudioClip(string path)
			{
				resourcePath = path;

				Reload();
			}

			#endregion

			#region Operators

			/// <summary>
			/// Implicitly converts a SerializableAudioClip to a boolean value.
			/// Returns true if the SerializableAudioClip is not null.
			/// </summary>
			/// <param name="audioClip">The SerializableAudioClip to convert.</param>
			/// <returns>true if the SerializableAudioClip is not null; otherwise, false.</returns>
			public static implicit operator bool(SerializableAudioClip audioClip) => audioClip != null;

			/// <summary>
			/// Implicitly converts a SerializableAudioClip to an AudioClip.
			/// This allows using a SerializableAudioClip in contexts where an AudioClip is expected.
			/// </summary>
			/// <param name="audioClip">The SerializableAudioClip to convert.</param>
			/// <returns>The AudioClip referenced by the SerializableAudioClip.</returns>
			public static implicit operator AudioClip(SerializableAudioClip audioClip) => audioClip.Clip;

			/// <summary>
			/// Determines whether two SerializableAudioClip instances are equal.
			/// Two SerializableAudioClip instances are considered equal if they reference the same AudioClip.
			/// </summary>
			/// <param name="clipA">The first SerializableAudioClip to compare.</param>
			/// <param name="clipB">The second SerializableAudioClip to compare.</param>
			/// <returns>true if the SerializableAudioClip instances are equal; otherwise, false.</returns>
			public static bool operator ==(SerializableAudioClip clipA, SerializableAudioClip clipB)
			{
				return clipA.Equals(clipB);
			}

			/// <summary>
			/// Determines whether two SerializableAudioClip instances are not equal.
			/// Two SerializableAudioClip instances are considered not equal if they reference different AudioClips.
			/// </summary>
			/// <param name="clipA">The first SerializableAudioClip to compare.</param>
			/// <param name="clipB">The second SerializableAudioClip to compare.</param>
			/// <returns>true if the SerializableAudioClip instances are not equal; otherwise, false.</returns>
			public static bool operator !=(SerializableAudioClip clipA, SerializableAudioClip clipB)
			{
				return !(clipA == clipB);
			}

			/// <summary>
			/// Determines whether a SerializableAudioClip and an AudioClip are equal.
			/// They are considered equal if the SerializableAudioClip references the same AudioClip.
			/// </summary>
			/// <param name="clipA">The SerializableAudioClip to compare.</param>
			/// <param name="clipB">The AudioClip to compare.</param>
			/// <returns>true if the clips are equal; otherwise, false.</returns>
			public static bool operator ==(SerializableAudioClip clipA, AudioClip clipB)
			{
				return clipA.Equals(clipB);
			}

			/// <summary>
			/// Determines whether a SerializableAudioClip and an AudioClip are not equal.
			/// They are considered not equal if the SerializableAudioClip references a different AudioClip.
			/// </summary>
			/// <param name="clipA">The SerializableAudioClip to compare.</param>
			/// <param name="clipB">The AudioClip to compare.</param>
			/// <returns>true if the clips are not equal; otherwise, false.</returns>
			public static bool operator !=(SerializableAudioClip clipA, AudioClip clipB)
			{
				return !(clipA == clipB);
			}

			#endregion

			#endregion
		}
		/// <summary>
		/// Represents a particle system with a resource path and a reference to the particle system.
		/// Provides methods for loading the particle system from a resource path and reloading it when needed.
		/// </summary>
		[Serializable]
		public class SerializableParticleSystem
		{
			#region Variables

			/// <summary>
			/// The path to the particle system resource in the Resources folder.
			/// Used to load the particle system when needed.
			/// </summary>
			public string resourcePath;

			/// <summary>
			/// Gets the referenced ParticleSystem.
			/// Automatically reloads the particle system if it's null or if the resource path has changed.
			/// </summary>
			public ParticleSystem Particle
			{
				get
				{
					if (!particle || resourcePath != path)
						Reload();

					return particle;
				}
			}

			/// <summary>
			/// The cached reference to the loaded ParticleSystem.
			/// </summary>
			private ParticleSystem particle;

			/// <summary>
			/// The last resource path used to load the particle system.
			/// Used to detect changes in the resource path.
			/// </summary>
			private string path;

			#endregion

			#region Methods

			#region Virtual Methods

			/// <summary>
			/// Determines whether the specified object is equal to the current SerializableParticleSystem.
			/// Supports comparison with both SerializableParticleSystem and ParticleSystem objects.
			/// </summary>
			/// <param name="obj">The object to compare with the current SerializableParticleSystem.</param>
			/// <returns>true if the specified object is equal to the current SerializableParticleSystem; otherwise, false.</returns>
			public override bool Equals(object obj)
			{
				bool equalsParticle = obj is SerializableParticleSystem particle && particle.Particle == Particle;
				bool equalsUParticle = obj is ParticleSystem uParticle && uParticle == Particle;

				return equalsParticle || equalsUParticle;
			}

			/// <summary>
			/// Serves as the default hash function for SerializableParticleSystem.
			/// Returns a hash code for the current SerializableParticleSystem based on its referenced ParticleSystem.
			/// </summary>
			/// <returns>A hash code for the current SerializableParticleSystem.</returns>
			public override int GetHashCode()
			{
				return 1500868535 + EqualityComparer<ParticleSystem>.Default.GetHashCode(Particle);
			}

			#endregion

			#region Global Methods

			/// <summary>
			/// Reloads the particle system from the resource path.
			/// Updates the cached path and loads the particle system from Resources.
			/// </summary>
			private void Reload()
			{
				path = resourcePath;
				particle = Resources.Load(path) as ParticleSystem;
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the SerializableParticleSystem class with the specified resource path.
			/// Loads the particle system from the provided path.
			/// </summary>
			/// <param name="path">The resource path to the particle system.</param>
			public SerializableParticleSystem(string path)
			{
				resourcePath = path;

				Reload();
			}

			#endregion

			#region Operators

			/// <summary>
			/// Implicitly converts a SerializableParticleSystem to a boolean value.
			/// Returns true if the SerializableParticleSystem is not null.
			/// </summary>
			/// <param name="particleSystem">The SerializableParticleSystem to convert.</param>
			/// <returns>true if the SerializableParticleSystem is not null; otherwise, false.</returns>
			public static implicit operator bool(SerializableParticleSystem particleSystem) => particleSystem != null;

			/// <summary>
			/// Implicitly converts a SerializableParticleSystem to a ParticleSystem.
			/// Returns the referenced ParticleSystem.
			/// </summary>
			/// <param name="particleSystem">The SerializableParticleSystem to convert.</param>
			/// <returns>The referenced ParticleSystem.</returns>
			public static implicit operator ParticleSystem(SerializableParticleSystem particleSystem) => particleSystem.Particle;

			/// <summary>
			/// Determines whether two SerializableParticleSystem instances are equal.
			/// Two SerializableParticleSystem instances are considered equal if they reference the same ParticleSystem.
			/// </summary>
			/// <param name="particleA">The first SerializableParticleSystem to compare.</param>
			/// <param name="particleB">The second SerializableParticleSystem to compare.</param>
			/// <returns>true if the SerializableParticleSystem instances are equal; otherwise, false.</returns>
			public static bool operator ==(SerializableParticleSystem particleA, SerializableParticleSystem particleB)
			{
				return particleA.Equals(particleB);
			}

			/// <summary>
			/// Determines whether two SerializableParticleSystem instances are not equal.
			/// Two SerializableParticleSystem instances are considered not equal if they reference different ParticleSystems.
			/// </summary>
			/// <param name="particleA">The first SerializableParticleSystem to compare.</param>
			/// <param name="particleB">The second SerializableParticleSystem to compare.</param>
			/// <returns>true if the SerializableParticleSystem instances are not equal; otherwise, false.</returns>
			public static bool operator !=(SerializableParticleSystem particleA, SerializableParticleSystem particleB)
			{
				return !(particleA == particleB);
			}

			/// <summary>
			/// Determines whether a SerializableParticleSystem and a ParticleSystem are equal.
			/// They are considered equal if the SerializableParticleSystem references the same ParticleSystem.
			/// </summary>
			/// <param name="particleA">The SerializableParticleSystem to compare.</param>
			/// <param name="particleB">The ParticleSystem to compare.</param>
			/// <returns>true if the particle systems are equal; otherwise, false.</returns>
			public static bool operator ==(SerializableParticleSystem particleA, ParticleSystem particleB)
			{
				return particleA.Equals(particleB);
			}

			/// <summary>
			/// Determines whether a SerializableParticleSystem and a ParticleSystem are not equal.
			/// They are considered not equal if the SerializableParticleSystem references a different ParticleSystem.
			/// </summary>
			/// <param name="particleA">The SerializableParticleSystem to compare.</param>
			/// <param name="particleB">The ParticleSystem to compare.</param>
			/// <returns>true if the particle systems are not equal; otherwise, false.</returns>
			public static bool operator !=(SerializableParticleSystem particleA, ParticleSystem particleB)
			{
				return !(particleA == particleB);
			}

			#endregion

			#endregion
		}
		/// <summary>
		/// Represents a material with a resource path and a reference to the material.
		/// Provides methods for loading the material from a resource path and reloading it when needed.
		/// </summary>
		[Serializable]
		public class SerializableMaterial
		{
			#region Variables

			/// <summary>
			/// The path to the material resource in the Resources folder.
			/// Used to load the material when needed.
			/// </summary>
			public string resourcePath;

			/// <summary>
			/// Gets the referenced Material.
			/// Automatically reloads the material if it's null or if the resource path has changed.
			/// </summary>
			public Material Material
			{
				get
				{
					if (!material || resourcePath != path)
						Reload();

					return material;
				}
			}

			/// <summary>
			/// The cached reference to the loaded Material.
			/// </summary>
			private Material material;

			/// <summary>
			/// The last resource path used to load the material.
			/// Used to detect changes in the resource path.
			/// </summary>
			private string path;

			#endregion

			#region Methods

			#region Virtual Methods

			/// <summary>
			/// Determines whether the specified object is equal to the current SerializableMaterial.
			/// Supports comparison with both SerializableMaterial and Material objects.
			/// </summary>
			/// <param name="obj">The object to compare with the current SerializableMaterial.</param>
			/// <returns>true if the specified object is equal to the current SerializableMaterial; otherwise, false.</returns>
			public override bool Equals(object obj)
			{
				bool equalsMaterial = obj is SerializableMaterial material && material.Material == Material;
				bool equalsUMaterial = obj is Material uMaterial && uMaterial == Material;

				return equalsMaterial || equalsUMaterial;
			}

			/// <summary>
			/// Serves as the default hash function for SerializableMaterial.
			/// Returns a hash code for the current SerializableMaterial based on its referenced Material.
			/// </summary>
			/// <returns>A hash code for the current SerializableMaterial.</returns>
			public override int GetHashCode()
			{
				return 1578056576 + EqualityComparer<Material>.Default.GetHashCode(Material);
			}

			#endregion

			#region Global Methods

			/// <summary>
			/// Reloads the material from the resource path.
			/// Updates the cached path and loads the material from Resources.
			/// </summary>
			private void Reload()
			{
				path = resourcePath;
				material = Resources.Load(path) as Material;
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the SerializableMaterial class with the specified resource path.
			/// Loads the material from the provided path.
			/// </summary>
			/// <param name="path">The resource path to the material.</param>
			public SerializableMaterial(string path)
			{
				resourcePath = path;

				Reload();
			}

			#endregion

			#region Operators

			/// <summary>
			/// Implicitly converts a SerializableMaterial to a boolean value.
			/// Returns true if the SerializableMaterial is not null.
			/// </summary>
			/// <param name="material">The SerializableMaterial to convert.</param>
			/// <returns>true if the SerializableMaterial is not null; otherwise, false.</returns>
			public static implicit operator bool(SerializableMaterial material) => material != null;

			/// <summary>
			/// Implicitly converts a SerializableMaterial to a Material.
			/// Returns the referenced Material.
			/// </summary>
			/// <param name="material">The SerializableMaterial to convert.</param>
			/// <returns>The referenced Material.</returns>
			public static implicit operator Material(SerializableMaterial material) => material.Material;

			/// <summary>
			/// Determines whether two SerializableMaterial instances are equal.
			/// Two SerializableMaterial instances are considered equal if they reference the same Material.
			/// </summary>
			/// <param name="materialA">The first SerializableMaterial to compare.</param>
			/// <param name="materialB">The second SerializableMaterial to compare.</param>
			/// <returns>true if the SerializableMaterial instances are equal; otherwise, false.</returns>
			public static bool operator ==(SerializableMaterial materialA, SerializableMaterial materialB)
			{
				return materialA.Equals(materialB);
			}

			/// <summary>
			/// Determines whether two SerializableMaterial instances are not equal.
			/// Two SerializableMaterial instances are considered not equal if they reference different Materials.
			/// </summary>
			/// <param name="materialA">The first SerializableMaterial to compare.</param>
			/// <param name="materialB">The second SerializableMaterial to compare.</param>
			/// <returns>true if the SerializableMaterial instances are not equal; otherwise, false.</returns>
			public static bool operator !=(SerializableMaterial materialA, SerializableMaterial materialB)
			{
				return !(materialA == materialB);
			}

			/// <summary>
			/// Determines whether a SerializableMaterial and a Material are equal.
			/// They are considered equal if the SerializableMaterial references the same Material.
			/// </summary>
			/// <param name="materialA">The SerializableMaterial to compare.</param>
			/// <param name="materialB">The Material to compare.</param>
			/// <returns>true if the materials are equal; otherwise, false.</returns>
			public static bool operator ==(SerializableMaterial materialA, Material materialB)
			{
				return materialA.Equals(materialB);
			}

			/// <summary>
			/// Determines whether a SerializableMaterial and a Material are not equal.
			/// They are considered not equal if the SerializableMaterial references a different Material.
			/// </summary>
			/// <param name="materialA">The SerializableMaterial to compare.</param>
			/// <param name="materialB">The Material to compare.</param>
			/// <returns>true if the materials are not equal; otherwise, false.</returns>
			public static bool operator !=(SerializableMaterial materialA, Material materialB)
			{
				return !(materialA == materialB);
			}

			#endregion

			#endregion
		}
		/// <summary>
		/// Represents a light with a resource path and a reference to the light.
		/// Provides methods for loading the light from a resource path and reloading it when needed.
		/// </summary>
		[Serializable]
		public class SerializableLight
		{
			#region Variables

			/// <summary>
			/// The path to the light resource in the Resources folder.
			/// Used to load the light when needed.
			/// </summary>
			public string resourcePath;

			/// <summary>
			/// Gets the referenced Light.
			/// Automatically reloads the light if it's null or if the resource path has changed.
			/// </summary>
			public Light Light
			{
				get
				{
					if (!light || resourcePath != path)
						Reload();

					return light;
				}
			}

			/// <summary>
			/// The cached reference to the loaded Light.
			/// </summary>
			private Light light;

			/// <summary>
			/// The last resource path used to load the light.
			/// Used to detect changes in the resource path.
			/// </summary>
			private string path;

			#endregion

			#region Methods

			#region Virtual Methods

			/// <summary>
			/// Determines whether the specified object is equal to the current SerializableLight.
			/// Supports comparison with both SerializableLight and Light objects.
			/// </summary>
			/// <param name="obj">The object to compare with the current SerializableLight.</param>
			/// <returns>true if the specified object is equal to the current SerializableLight; otherwise, false.</returns>
			public override bool Equals(object obj)
			{
				bool equalsLight = obj is SerializableLight light && light.Light == Light;
				bool equalsULight = obj is Light uLight && uLight == Light;

				return equalsLight || equalsULight;
			}

			/// <summary>
			/// Serves as the default hash function for SerializableLight.
			/// Returns a hash code for the current SerializableLight based on its referenced Light.
			/// </summary>
			/// <returns>A hash code for the current SerializableLight.</returns>
			public override int GetHashCode()
			{
				return 1344377895 + EqualityComparer<Light>.Default.GetHashCode(Light);
			}

			#endregion

			#region Global Methods

			/// <summary>
			/// Reloads the light from the resource path.
			/// Updates the cached path and loads the light from Resources.
			/// </summary>
			private void Reload()
			{
				path = resourcePath;
				light = Resources.Load(path) as Light;
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the SerializableLight class with the specified resource path.
			/// Loads the light from the provided path.
			/// </summary>
			/// <param name="path">The resource path to the light.</param>
			public SerializableLight(string path)
			{
				resourcePath = path;

				Reload();
			}

			#endregion

			#region Operators

			/// <summary>
			/// Implicitly converts a SerializableLight to a boolean value.
			/// Returns true if the SerializableLight is not null.
			/// </summary>
			/// <param name="light">The SerializableLight to convert.</param>
			/// <returns>true if the SerializableLight is not null; otherwise, false.</returns>
			public static implicit operator bool(SerializableLight light) => light != null;

			/// <summary>
			/// Implicitly converts a SerializableLight to a Light.
			/// Returns the referenced Light.
			/// </summary>
			/// <param name="light">The SerializableLight to convert.</param>
			/// <returns>The referenced Light.</returns>
			public static implicit operator Light(SerializableLight light) => light.Light;

			/// <summary>
			/// Determines whether two SerializableLight instances are equal.
			/// Two SerializableLight instances are considered equal if they reference the same Light.
			/// </summary>
			/// <param name="lightA">The first SerializableLight to compare.</param>
			/// <param name="lightB">The second SerializableLight to compare.</param>
			/// <returns>true if the SerializableLight instances are equal; otherwise, false.</returns>
			public static bool operator ==(SerializableLight lightA, SerializableLight lightB)
			{
				return lightA.Equals(lightB);
			}

			/// <summary>
			/// Determines whether two SerializableLight instances are not equal.
			/// Two SerializableLight instances are considered not equal if they reference different Lights.
			/// </summary>
			/// <param name="lightA">The first SerializableLight to compare.</param>
			/// <param name="lightB">The second SerializableLight to compare.</param>
			/// <returns>true if the SerializableLight instances are not equal; otherwise, false.</returns>
			public static bool operator !=(SerializableLight lightA, SerializableLight lightB)
			{
				return !(lightA == lightB);
			}

			/// <summary>
			/// Determines whether a SerializableLight and a Light are equal.
			/// They are considered equal if the SerializableLight references the same Light.
			/// </summary>
			/// <param name="lightA">The SerializableLight to compare.</param>
			/// <param name="lightB">The Light to compare.</param>
			/// <returns>true if the lights are equal; otherwise, false.</returns>
			public static bool operator ==(SerializableLight lightA, Light lightB)
			{
				return lightA.Equals(lightB);
			}

			/// <summary>
			/// Determines whether a SerializableLight and a Light are not equal.
			/// They are considered not equal if the SerializableLight references a different Light.
			/// </summary>
			/// <param name="lightA">The SerializableLight to compare.</param>
			/// <param name="lightB">The Light to compare.</param>
			/// <returns>true if the lights are not equal; otherwise, false.</returns>
			public static bool operator !=(SerializableLight lightA, Light lightB)
			{
				return !(lightA == lightB);
			}

			#endregion

			#endregion
		}
		/// <summary>
		/// Represents a transform access with various properties of a Unity Transform.
		/// Provides methods for accessing and manipulating the transform's properties.
		/// This struct allows for efficient access to transform data without direct references to Transform objects.
		/// </summary>
		[Serializable]
		public struct TransformAccess
		{
			#region Variables

			/// <summary>
			/// Indicates whether this TransformAccess instance has been properly initialized with a valid Transform.
			/// Returns true if the instance was created from a valid Transform; otherwise, false.
			/// </summary>
			[MarshalAs(UnmanagedType.U1)]
			public readonly bool isCreated;

			/// <summary>
			/// The number of child transforms parented to the referenced transform.
			/// Equivalent to Transform.childCount.
			/// </summary>
			public int childCount;

			/// <summary>
			/// The rotation of the transform in world space stored as Euler angles in degrees.
			/// Equivalent to Transform.eulerAngles.
			/// </summary>
			public float3 eulerAngles;

			/// <summary>
			/// The blue axis of the transform in world space.
			/// Represents the direction the transform is facing.
			/// Equivalent to Transform.forward.
			/// </summary>
			public float3 forward;

			/// <summary>
			/// Indicates whether the transform has been changed since the last frame.
			/// Can be used to detect changes in position, rotation, or scale.
			/// Equivalent to Transform.hasChanged.
			/// </summary>
			[MarshalAs(UnmanagedType.U1)]
			public bool hasChanged;

			/// <summary>
			/// The total capacity of the transform's hierarchy array.
			/// Equivalent to Transform.hierarchyCapacity.
			/// </summary>
			public int hierarchyCapacity;

			/// <summary>
			/// The number of transforms in the transform's hierarchy.
			/// Equivalent to Transform.hierarchyCount.
			/// </summary>
			public int hierarchyCount;

			/// <summary>
			/// The rotation of the transform relative to its parent, stored as Euler angles in degrees.
			/// Equivalent to Transform.localEulerAngles.
			/// </summary>
			public float3 localEulerAngles;

			/// <summary>
			/// The position of the transform relative to its parent.
			/// Equivalent to Transform.localPosition.
			/// </summary>
			public float3 localPosition;

			/// <summary>
			/// The rotation of the transform relative to its parent, stored as a quaternion.
			/// Equivalent to Transform.localRotation.
			/// </summary>
			public quaternion localRotation;

			/// <summary>
			/// The scale of the transform relative to its parent.
			/// Equivalent to Transform.localScale.
			/// </summary>
			public float3 localScale;

			/// <summary>
			/// The transformation matrix that transforms from local space to world space.
			/// Equivalent to Transform.localToWorldMatrix.
			/// </summary>
			public float4x4 localToWorldMatrix;

			/// <summary>
			/// The global scale of the transform, which may be affected by the scale of its parent transforms.
			/// Equivalent to Transform.lossyScale.
			/// </summary>
			public float3 lossyScale;

			/// <summary>
			/// The position of the transform in world space.
			/// Equivalent to Transform.position.
			/// </summary>
			public float3 position;

			/// <summary>
			/// The red axis of the transform in world space.
			/// Represents the right direction relative to the transform's orientation.
			/// Equivalent to Transform.right.
			/// </summary>
			public float3 right;

			/// <summary>
			/// The rotation of the transform in world space, stored as a quaternion.
			/// Equivalent to Transform.rotation.
			/// </summary>
			public quaternion rotation;

			/// <summary>
			/// The green axis of the transform in world space.
			/// Represents the up direction relative to the transform's orientation.
			/// Equivalent to Transform.up.
			/// </summary>
			public float3 up;

			/// <summary>
			/// The transformation matrix that transforms from world space to local space.
			/// Equivalent to Transform.worldToLocalMatrix.
			/// </summary>
			public float4x4 worldToLocalMatrix;

			#endregion

			#region Constructors & Operators

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the TransformAccess structure from a Unity Transform.
			/// Copies all relevant transform properties into this structure for efficient access.
			/// If the provided transform is null, creates an invalid TransformAccess with isCreated set to false.
			/// </summary>
			/// <param name="transform">The Unity Transform to copy properties from.</param>
			public TransformAccess(Transform transform) : this()
			{
				if (!transform)
					return;

				childCount = transform.childCount;
				eulerAngles = transform.eulerAngles;
				forward = transform.forward;
				hasChanged = transform.hasChanged;
				hierarchyCapacity = transform.hierarchyCapacity;
				hierarchyCount = transform.hierarchyCount;
				localEulerAngles = transform.localEulerAngles;
				localPosition = transform.localPosition;
				localRotation = transform.localRotation;
				localScale = transform.localScale;
				localToWorldMatrix = transform.localToWorldMatrix;
				lossyScale = transform.lossyScale;
				position = transform.position;
				right = transform.right;
				rotation = transform.rotation;
				up = transform.up;
				worldToLocalMatrix = transform.worldToLocalMatrix;
				isCreated = true;
			}

			#endregion

			#region Operators

			/// <summary>
			/// Implicitly converts a Unity Transform to a TransformAccess.
			/// Allows using a Transform directly in contexts where a TransformAccess is expected.
			/// </summary>
			/// <param name="transform">The Unity Transform to convert.</param>
			/// <returns>A new TransformAccess containing the properties of the Transform.</returns>
			public static implicit operator TransformAccess(Transform transform) => new TransformAccess(transform);

			#endregion

			#endregion
		}
		/// <summary>
		/// Represents a single float value with a serialized field.
		/// Provides methods for conversion between float and float1 types.
		/// Useful for serializing single float values in a consistent manner with other float vector types.
		/// </summary>
		[Serializable]
#pragma warning disable IDE1006 // Naming Styles
		public struct float1
#pragma warning restore IDE1006 // Naming Styles
		{
			#region Variables

			/// <summary>
			/// The underlying float value stored in this float1 structure.
			/// This field is serialized to allow persistence between sessions.
			/// </summary>
			[SerializeField]
#pragma warning disable IDE0044 // Add readonly modifier
			private float value;
#pragma warning restore IDE0044 // Add readonly modifier

			#endregion

			#region Operators

			/// <summary>
			/// Implicitly converts a float to a float1.
			/// Allows using a float directly in contexts where a float1 is expected.
			/// </summary>
			/// <param name="value">The float value to convert.</param>
			/// <returns>A new float1 containing the specified value.</returns>
			public static implicit operator float1(float value) => new float1(value);

			/// <summary>
			/// Implicitly converts a float1 to a float.
			/// Allows using a float1 directly in contexts where a float is expected.
			/// </summary>
			/// <param name="value">The float1 to convert.</param>
			/// <returns>The float value contained in the float1.</returns>
			public static implicit operator float(float1 value) => value.value;

			#endregion

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the float1 structure with the specified float value.
			/// This private constructor is used by the implicit conversion operator.
			/// </summary>
			/// <param name="value">The float value to store in this float1.</param>
			private float1(float value)
			{
				this.value = value;
			}

			#endregion
		}

		#endregion

		#endregion

		#endregion

		#region Constants

		/// <summary>
		/// Represents the standard air density at sea level and 20°C (68°F) in kilograms per cubic meter (kg/m³).
		/// This constant is commonly used in physics calculations involving air resistance, aerodynamics, and fluid dynamics.
		/// </summary>
		public const float airDensity = 1.29f;

		/// <summary>
		/// Represents an empty string constant ("").
		/// Provides a reusable, memory-efficient way to reference an empty string without creating new string instances.
		/// Useful for string comparisons, initializations, and as a default value for string parameters.
		/// </summary>
		public const string emptyString = "";

		#endregion

		#region Variables

		/// <summary>
		/// Gets the appropriate time delta between frames, automatically adjusting based on the current time step mode.
		/// Returns Time.fixedDeltaTime when in fixed time step mode, otherwise returns Time.deltaTime.
		/// This property is useful for physics calculations and animations that need to work correctly in both update modes.
		/// </summary>
		public static float DeltaTime => Time.inFixedTimeStep ? Time.fixedDeltaTime : Time.deltaTime;

		/// <summary>
		/// Represents a float2 vector with components (1, 1).
		/// Useful as a scaling factor that maintains proportions or as a direction vector for diagonal movement.
		/// This is the 2D equivalent of Vector2.one in Unity.
		/// </summary>
		public readonly static float2 Float2One = new float2(1f, 1f);

		/// <summary>
		/// Represents a float2 vector with components (0, 1).
		/// Defines the upward direction in 2D space, commonly used for vertical movement or alignment.
		/// This is the 2D equivalent of Vector2.up in Unity.
		/// </summary>
		public readonly static float2 Float2Up = new float2(0f, 1f);

		/// <summary>
		/// Represents a float2 vector with components (1, 0).
		/// Defines the rightward direction in 2D space, commonly used for horizontal movement or alignment.
		/// This is the 2D equivalent of Vector2.right in Unity.
		/// </summary>
		public readonly static float2 Float2Right = new float2(1f, 0f);

		/// <summary>
		/// Represents a float3 vector with components (1, 1, 1).
		/// Useful as a uniform scaling factor or as a normalization reference.
		/// This is the 3D equivalent of Vector3.one in Unity.
		/// </summary>
		public readonly static float3 Float3One = new float3(1f, 1f, 1f);

		/// <summary>
		/// Represents a float3 vector with components (0, 1, 0).
		/// Defines the upward direction in 3D space, commonly used for vertical movement, alignment, or as the Y-axis.
		/// This is the 3D equivalent of Vector3.up in Unity.
		/// </summary>
		public readonly static float3 Float3Up = new float3(0f, 1f, 0f);

		/// <summary>
		/// Represents a float3 vector with components (1, 0, 0).
		/// Defines the rightward direction in 3D space, commonly used for horizontal movement, alignment, or as the X-axis.
		/// This is the 3D equivalent of Vector3.right in Unity.
		/// </summary>
		public readonly static float3 Float3Right = new float3(1f, 0f, 0f);

		/// <summary>
		/// Represents a float3 vector with components (0, 0, 1).
		/// Defines the forward direction in 3D space, commonly used for depth movement, alignment, or as the Z-axis.
		/// This is the 3D equivalent of Vector3.forward in Unity.
		/// </summary>
		public readonly static float3 Float3Forward = new float3(0f, 0f, 1f);

		/// <summary>
		/// Represents a float4 vector with components (1, 1, 1, 1).
		/// Useful in graphics programming for colors with full opacity or as a uniform scaling factor in homogeneous coordinates.
		/// When used as a color, represents white with full opacity.
		/// </summary>
		public readonly static float4 Float4One = new float4(1f, 1f, 1f, 1f);

		/// <summary>
		/// Represents a float4 vector with components (0, 1, 0, 0).
		/// In a coordinate system, this represents the Y-axis direction with no contribution from other dimensions.
		/// Can be used in shader calculations or as a basis vector in 4D space.
		/// </summary>
		public readonly static float4 Float4Up = new float4(0f, 1f, 0f, 0f);

		/// <summary>
		/// Represents a float4 vector with components (1, 0, 0, 0).
		/// In a coordinate system, this represents the X-axis direction with no contribution from other dimensions.
		/// Can be used in shader calculations or as a basis vector in 4D space.
		/// </summary>
		public readonly static float4 Float4Right = new float4(1f, 0f, 0f, 0f);

		/// <summary>
		/// Represents a float4 vector with components (0, 0, 1, 0).
		/// In a coordinate system, this represents the Z-axis direction with no contribution from other dimensions.
		/// Can be used in shader calculations or as a basis vector in 4D space.
		/// </summary>
		public readonly static float4 Float4Forward = new float4(0f, 0f, 1f, 0f);

		/// <summary>
		/// Represents a float4 vector with components (0, 0, 0, 1).
		/// In homogeneous coordinates, this represents a point at the origin.
		/// When used in quaternion calculations, this represents the identity quaternion (no rotation).
		/// </summary>
		public readonly static float4 Float4Identity = new float4(0f, 0f, 0f, 1f);

		/// <summary>
		/// Represents a float4 vector with components (0, 0, 0, 0).
		/// Useful as an initialization value or to represent the absence of a value.
		/// In graphics programming, can represent a fully transparent color regardless of RGB values.
		/// </summary>
		public readonly static float4 Float4Zero = new float4(0f, 0f, 0f, 0f);

		/// <summary>
		/// A comprehensive list of known disposable email domains.
		/// Used for email validation to prevent temporary or throwaway email addresses from being used in registration processes.
		/// This array contains domain names of services that provide temporary, disposable, or anonymous email addresses
		/// which are often used to bypass verification systems or avoid spam.
		/// </summary>
		private static readonly string[] disposableEmailDomains = new string[]
		{
			"nvhrw",
			"ampswipe",
			"trash-mail",
			"terasd",
			"cyadp",
			"zwoho",
			"necra",
			"0815.ru",
			"0wnd.net",
			"0wnd.org",
			"10minutemail.co.za",
			"10minutemail.com",
			"123-m.com",
			"1fsdfdsfsdf.tk",
			"1pad.de",
			"20minutemail.com",
			"21cn.com",
			"2fdgdfgdfgdf.tk",
			"2prong.com",
			"30minutemail.com",
			"33mail.com",
			"3trtretgfrfe.tk",
			"4gfdsgfdgfd.tk",
			"4warding.com",
			"5ghgfhfghfgh.tk",
			"6hjgjhgkilkj.tk",
			"6paq.com",
			"7tags.com",
			"9ox.net",
			"a-bc.net",
			"agedmail.com",
			"ama-trade.de",
			"amilegit.com",
			"amiri.net",
			"amiriindustries.com",
			"anonmails.de",
			"anonymbox.com",
			"antichef.com",
			"antichef.net",
			"antireg.ru",
			"antispam.de",
			"antispammail.de",
			"armyspy.com",
			"artman-conception.com",
			"azmeil.tk",
			"baxomale.ht.cx",
			"beefmilk.com",
			"bigstring.com",
			"binkmail.com",
			"bio-muesli.net",
			"bobmail.info",
			"bodhi.lawlita.com",
			"bofthew.com",
			"bootybay.de",
			"boun.cr",
			"bouncr.com",
			"breakthru.com",
			"brefmail.com",
			"bsnow.net",
			"bspamfree.org",
			"bugmenot.com",
			"bund.us",
			"burstmail.info",
			"buymoreplays.com",
			"byom.de",
			"c2.hu",
			"card.zp.ua",
			"casualdx.com",
			"cek.pm",
			"centermail.com",
			"centermail.net",
			"chammy.info",
			"childsavetrust.org",
			"chogmail.com",
			"choicemail1.com",
			"clixser.com",
			"cmail.net",
			"cmail.org",
			"coldemail.info",
			"cool.fr.nf",
			"courriel.fr.nf",
			"courrieltemporaire.com",
			"crapmail.org",
			"cust.in",
			"cuvox.de",
			"d3p.dk",
			"dacoolest.com",
			"dandikmail.com",
			"dayrep.com",
			"dcemail.com",
			"deadaddress.com",
			"deadspam.com",
			"delikkt.de",
			"despam.it",
			"despammed.com",
			"devnullmail.com",
			"dfgh.net",
			"digitalsanctuary.com",
			"dingbone.com",
			"disposableaddress.com",
			"disposableemailaddresses.com",
			"disposableinbox.com",
			"dispose.it",
			"dispostable.com",
			"dodgeit.com",
			"dodgit.com",
			"donemail.ru",
			"dontreg.com",
			"dontsendmespam.de",
			"drdrb.net",
			"dump-email.info",
			"dumpandjunk.com",
			"dumpyemail.com",
			"e-mail.com",
			"e-mail.org",
			"e4ward.com",
			"easytrashmail.com",
			"einmalmail.de",
			"einrot.com",
			"eintagsmail.de",
			"emailgo.de",
			"emailias.com",
			"emaillime.com",
			"emailsensei.com",
			"emailtemporanea.com",
			"emailtemporanea.net",
			"emailtemporar.ro",
			"emailtemporario.com.br",
			"emailthe.net",
			"emailtmp.com",
			"emailwarden.com",
			"emailx.at.hm",
			"emailxfer.com",
			"emeil.in",
			"emeil.ir",
			"emz.net",
			"ero-tube.org",
			"evopo.com",
			"explodemail.com",
			"express.net.ua",
			"eyepaste.com",
			"fakeinbox.com",
			"fakeinformation.com",
			"fansworldwide.de",
			"fantasymail.de",
			"fightallspam.com",
			"filzmail.com",
			"fivemail.de",
			"fleckens.hu",
			"frapmail.com",
			"friendlymail.co.uk",
			"fuckingduh.com",
			"fudgerub.com",
			"fyii.de",
			"garliclife.com",
			"gehensiemirnichtaufdensack.de",
			"get2mail.fr",
			"getairmail.com",
			"getmails.eu",
			"getonemail.com",
			"giantmail.de",
			"girlsundertheinfluence.com",
			"gishpuppy.com",
			"gmial.com",
			"goemailgo.com",
			"gotmail.net",
			"gotmail.org",
			"gotti.otherinbox.com",
			"great-host.in",
			"greensloth.com",
			"grr.la",
			"gsrv.co.uk",
			"guerillamail.biz",
			"guerillamail.com",
			"guerrillamail.biz",
			"guerrillamail.com",
			"guerrillamail.de",
			"guerrillamail.info",
			"guerrillamail.net",
			"guerrillamail.org",
			"guerrillamailblock.com",
			"gustr.com",
			"harakirimail.com",
			"hat-geld.de",
			"hatespam.org",
			"herp.in",
			"hidemail.de",
			"hidzz.com",
			"hmamail.com",
			"hopemail.biz",
			"ieh-mail.de",
			"ikbenspamvrij.nl",
			"imails.info",
			"inbax.tk",
			"inbox.si",
			"inboxalias.com",
			"inboxclean.com",
			"inboxclean.org",
			"infocom.zp.ua",
			"instant-mail.de",
			"ip6.li",
			"irish2me.com",
			"iwi.net",
			"jetable.com",
			"jetable.fr.nf",
			"jetable.net",
			"jetable.org",
			"jnxjn.com",
			"jourrapide.com",
			"jsrsolutions.com",
			"kasmail.com",
			"kaspop.com",
			"killmail.com",
			"killmail.net",
			"klassmaster.com",
			"klzlk.com",
			"koszmail.pl",
			"kurzepost.de",
			"lawlita.com",
			"letthemeatspam.com",
			"lhsdv.com",
			"lifebyfood.com",
			"link2mail.net",
			"litedrop.com",
			"lol.ovpn.to",
			"lolfreak.net",
			"lookugly.com",
			"lortemail.dk",
			"lr78.com",
			"lroid.com",
			"lukop.dk",
			"m21.cc",
			"mail-filter.com",
			"mail-temporaire.fr",
			"mail.by",
			"mail.mezimages.net",
			"mail.zp.ua",
			"mail1a.de",
			"mail21.cc",
			"mail2rss.org",
			"mail333.com",
			"mailbidon.com",
			"mailbiz.biz",
			"mailblocks.com",
			"mailbucket.org",
			"mailcat.biz",
			"mailcatch.com",
			"mailde.de",
			"mailde.info",
			"maildrop.cc",
			"maileimer.de",
			"mailexpire.com",
			"mailfa.tk",
			"mailforspam.com",
			"mailfreeonline.com",
			"mailguard.me",
			"mailin8r.com",
			"mailinater.com",
			"mailinator.com",
			"mailinator.net",
			"mailinator.org",
			"mailinator2.com",
			"mailincubator.com",
			"mailismagic.com",
			"mailme.lv",
			"mailme24.com",
			"mailmetrash.com",
			"mailmoat.com",
			"mailms.com",
			"mailnesia.com",
			"mailnull.com",
			"mailorg.org",
			"mailpick.biz",
			"mailrock.biz",
			"mailscrap.com",
			"mailshell.com",
			"mailsiphon.com",
			"mailtemp.info",
			"mailtome.de",
			"mailtothis.com",
			"mailtrash.net",
			"mailtv.net",
			"mailtv.tv",
			"mailzilla.com",
			"makemetheking.com",
			"manybrain.com",
			"mbx.cc",
			"mega.zik.dj",
			"meinspamschutz.de",
			"meltmail.com",
			"messagebeamer.de",
			"mezimages.net",
			"ministry-of-silly-walks.de",
			"mintemail.com",
			"misterpinball.de",
			"moncourrier.fr.nf",
			"monemail.fr.nf",
			"monmail.fr.nf",
			"monumentmail.com",
			"mt2009.com",
			"mt2014.com",
			"mycard.net.ua",
			"mycleaninbox.net",
			"mymail-in.net",
			"mypacks.net",
			"mypartyclip.de",
			"myphantomemail.com",
			"mysamp.de",
			"mytempemail.com",
			"mytempmail.com",
			"mytrashmail.com",
			"nabuma.com",
			"neomailbox.com",
			"nepwk.com",
			"nervmich.net",
			"nervtmich.net",
			"netmails.com",
			"netmails.net",
			"neverbox.com",
			"nice-4u.com",
			"nincsmail.hu",
			"nnh.com",
			"no-spam.ws",
			"noblepioneer.com",
			"nomail.pw",
			"nomail.xl.cx",
			"nomail2me.com",
			"nomorespamemails.com",
			"nospam.ze.tc",
			"nospam4.us",
			"nospamfor.us",
			"nospammail.net",
			"notmailinator.com",
			"nowhere.org",
			"nowmymail.com",
			"nurfuerspam.de",
			"nus.edu.sg",
			"objectmail.com",
			"obobbo.com",
			"odnorazovoe.ru",
			"oneoffemail.com",
			"onewaymail.com",
			"onlatedotcom.info",
			"online.ms",
			"opayq.com",
			"ordinaryamerican.net",
			"otherinbox.com",
			"ovpn.to",
			"owlpic.com",
			"pancakemail.com",
			"pcusers.otherinbox.com",
			"pjjkp.com",
			"plexolan.de",
			"poczta.onet.pl",
			"politikerclub.de",
			"poofy.org",
			"pookmail.com",
			"privacy.net",
			"privatdemail.net",
			"proxymail.eu",
			"prtnx.com",
			"putthisinyourspamdatabase.com",
			"putthisinyourspamdatabase.com",
			"qq.com",
			"quickinbox.com",
			"rcpt.at",
			"reallymymail.com",
			"realtyalerts.ca",
			"recode.me",
			"recursor.net",
			"reliable-mail.com",
			"rhyta.com",
			"rmqkr.net",
			"royal.net",
			"rtrtr.com",
			"s0ny.net",
			"safe-mail.net",
			"safersignup.de",
			"safetymail.info",
			"safetypost.de",
			"saynotospams.com",
			"sceath.com",
			"schafmail.de",
			"schrott-email.de",
			"secretemail.de",
			"secure-mail.biz",
			"senseless-entertainment.com",
			"services391.com",
			"sharklasers.com",
			"shieldemail.com",
			"shiftmail.com",
			"shitmail.me",
			"shitware.nl",
			"shmeriously.com",
			"shortmail.net",
			"sibmail.com",
			"sinnlos-mail.de",
			"slapsfromlastnight.com",
			"slaskpost.se",
			"smashmail.de",
			"smellfear.com",
			"snakemail.com",
			"sneakemail.com",
			"sneakmail.de",
			"snkmail.com",
			"sofimail.com",
			"solvemail.info",
			"sogetthis.com",
			"soodonims.com",
			"spam4.me",
			"spamail.de",
			"spamarrest.com",
			"spambob.net",
			"spambog.ru",
			"spambox.us",
			"spamcannon.com",
			"spamcannon.net",
			"spamcon.org",
			"spamcorptastic.com",
			"spamcowboy.com",
			"spamcowboy.net",
			"spamcowboy.org",
			"spamday.com",
			"spamex.com",
			"spamfree.eu",
			"spamfree24.com",
			"spamfree24.de",
			"spamfree24.org",
			"spamgoes.in",
			"spamgourmet.com",
			"spamgourmet.net",
			"spamgourmet.org",
			"spamherelots.com",
			"spamherelots.com",
			"spamhereplease.com",
			"spamhereplease.com",
			"spamhole.com",
			"spamify.com",
			"spaml.de",
			"spammotel.com",
			"spamobox.com",
			"spamslicer.com",
			"spamspot.com",
			"spamthis.co.uk",
			"spamtroll.net",
			"speed.1s.fr",
			"spoofmail.de",
			"stuffmail.de",
			"super-auswahl.de",
			"supergreatmail.com",
			"supermailer.jp",
			"superrito.com",
			"superstachel.de",
			"suremail.info",
			"talkinator.com",
			"teewars.org",
			"teleworm.com",
			"teleworm.us",
			"temp-mail.org",
			"temp-mail.ru",
			"tempe-mail.com",
			"tempemail.co.za",
			"tempemail.com",
			"tempemail.net",
			"tempemail.net",
			"tempinbox.co.uk",
			"tempinbox.com",
			"tempmail.eu",
			"tempmaildemo.com",
			"tempmailer.com",
			"tempmailer.de",
			"tempomail.fr",
			"temporaryemail.net",
			"temporaryforwarding.com",
			"temporaryinbox.com",
			"temporarymailaddress.com",
			"tempthe.net",
			"thankyou2010.com",
			"thc.st",
			"thelimestones.com",
			"thisisnotmyrealemail.com",
			"thismail.net",
			"throwawayemailaddress.com",
			"tilien.com",
			"tittbit.in",
			"tizi.com",
			"tmailinator.com",
			"toomail.biz",
			"topranklist.de",
			"tradermail.info",
			"trash2009.com",
			"trashdevil.com",
			"trashemail.de",
			"trashmail.at",
			"trashmail.com",
			"trashmail.de",
			"trashmail.me",
			"trashmail.net",
			"trashmail.org",
			"trashymail.com",
			"trialmail.de",
			"trillianpro.com",
			"twinmail.de",
			"tyldd.com",
			"uggsrock.com",
			"umail.net",
			"uroid.com",
			"us.af",
			"venompen.com",
			"veryrealemail.com",
			"viditag.com",
			"viralplays.com",
			"vpn.st",
			"vsimcard.com",
			"vubby.com",
			"wasteland.rfc822.org",
			"webemail.me",
			"weg-werf-email.de",
			"wegwerf-emails.de",
			"wegwerfadresse.de",
			"wegwerfemail.com",
			"wegwerfemail.de",
			"wegwerfmail.de",
			"wegwerfmail.info",
			"wegwerfmail.net",
			"wegwerfmail.org",
			"wh4f.org",
			"whyspam.me",
			"willhackforfood.biz",
			"willselfdestruct.com",
			"winemaven.info",
			"wronghead.com",
			"www.e4ward.com",
			"www.mailinator.com",
			"wwwnew.eu",
			"x.ip6.li",
			"xagloo.com",
			"xemaps.com",
			"xents.com",
			"xmaily.com",
			"xoxy.net",
			"yep.it",
			"yogamaven.com",
			"yopmail.com",
			"yopmail.fr",
			"yopmail.net",
			"yourdomain.com",
			"yuurok.com",
			"z1p.biz",
			"za.com",
			"zehnminuten.de",
			"zehnminutenmail.de",
			"zippymail.info",
			"zoemail.net",
			"zomg.info"
		};

		#endregion

		#region Methods

		/// <summary>
		/// Empty method that does nothing. Can be used as a placeholder or no-operation function.
		/// This method serves as a utility for various scenarios such as:
		/// - Providing a do-nothing callback for event handlers
		/// - Serving as a temporary placeholder during development
		/// - Creating empty delegate instances where a method reference is required
		/// - Avoiding null reference exceptions when a method reference is expected
		/// - Testing method invocation mechanisms without side effects
		/// </summary>
		public static void Dummy()
		{
			// Intentionally left empty
		}
		/// <summary>
		/// Gets the conversion multiplier for a specific unit type between metric and imperial systems.
		/// This method provides the appropriate scaling factor needed to convert measurements between
		/// metric and imperial unit systems for various physical quantities such as area, distance, 
		/// volume, weight, and more.
		/// 
		/// For example, when converting meters to feet (distance), the multiplier is 3.28084.
		/// When converting from metric to imperial, multiply by this value.
		/// When converting from imperial to metric, divide by this value.
		/// 
		/// The method handles a wide range of unit types defined in the Units enum, including
		/// specialized variants like "accurate" and "large" versions for different scales of measurement.
		/// </summary>
		/// <param name="unit">The unit type to get the multiplier for, such as Distance, Weight, Volume, etc.</param>
		/// <param name="unitType">The unit system (Metric or Imperial) that determines the direction of conversion.</param>
		/// <returns>The conversion multiplier between metric and imperial units. Returns 1.0 for unit types without a defined conversion.</returns>
		public static float UnitMultiplier(Units unit, UnitType unitType)
		{
			return unit switch
			{
				Units.Area => unitType == UnitType.Metric ? 1f : 10.7639f,
				Units.AreaAccurate => unitType == UnitType.Metric ? 1f : 0.155f,
				Units.AreaLarge => unitType == UnitType.Metric ? 1f : 1f / 2.59f,
				Units.ElectricConsumption => unitType == UnitType.Metric ? 1f : 1f / 1.609f,
				Units.Density => unitType == UnitType.Metric ? 1f : 0.06242796f,
				Units.Distance => unitType == UnitType.Metric ? 1f : 3.28084f,
				Units.DistanceAccurate => unitType == UnitType.Metric ? 1f : 1.09361f,
				Units.DistanceLong => unitType == UnitType.Metric ? 1f : 0.621371f,
				Units.FuelConsumption => unitType == UnitType.Metric ? 1f : 235.215f,
				Units.Liquid => unitType == UnitType.Metric ? 1f : 1f / 4.546f,
				Units.Power => unitType == UnitType.Metric ? 1f : 0.7457f,
				Units.Pressure => unitType == UnitType.Metric ? 1f : 14.503773773f,
				Units.Size => unitType == UnitType.Metric ? 1f : 1f / 2.54f,
				Units.Speed => unitType == UnitType.Metric ? 1f : 0.621371f,
				Units.Torque => unitType == UnitType.Metric ? 1f : 0.73756f,
				Units.Velocity => unitType == UnitType.Metric ? 1f : 3.28084f,
				Units.Volume => unitType == UnitType.Metric ? 1f : 35.3147f,
				Units.VolumeAccurate => unitType == UnitType.Metric ? 1f : 1f / 16.3871f,
				Units.VolumeLarge => unitType == UnitType.Metric ? 1f : 1f / 4.16818f,
				Units.Weight => unitType == UnitType.Metric ? 1f : 2.20462262185f,
				_ => 1f,
			};
		}
		/// <summary>
		/// Gets the abbreviated unit symbol for a specific unit type in either metric or imperial system.
		/// This method returns the standard abbreviated notation (symbol) used to represent various physical quantities
		/// in either the metric or imperial measurement system.
		/// 
		/// For example:
		/// - For distance in metric: "m" (meters)
		/// - For distance in imperial: "ft" (feet)
		/// - For weight in metric: "kg" (kilograms)
		/// - For weight in imperial: "lbs" (pounds)
		/// 
		/// These symbols are commonly used in scientific notation, engineering documents, and user interfaces
		/// to concisely represent units of measurement. The method handles a comprehensive range of physical
		/// quantities defined in the Units enum, including specialized variants for different scales.
		/// </summary>
		/// <param name="unit">The unit type to get the symbol for, such as Distance, Weight, Volume, etc.</param>
		/// <param name="unitType">The unit system (Metric or Imperial) that determines which symbol set to use.</param>
		/// <returns>The abbreviated unit symbol as a string. Returns default (null) for unrecognized unit types.</returns>
		public static string Unit(Units unit, UnitType unitType)
		{
			return unit switch
			{
				Units.AngularVelocity => unitType == UnitType.Metric ? "rad/s" : "rad/s",
				Units.Area => unitType == UnitType.Metric ? "m²" : "ft²",
				Units.AreaAccurate => unitType == UnitType.Metric ? "cm²" : "in²",
				Units.AreaLarge => unitType == UnitType.Metric ? "km²" : "mi²",
				Units.ElectricConsumption => unitType == UnitType.Metric ? "kW⋅h/100km" : "kW⋅h/100m",
				Units.Density => unitType == UnitType.Metric ? "kg/m³" : "lbᵐ/ft³",
				Units.Distance => unitType == UnitType.Metric ? "m" : "ft",
				Units.DistanceAccurate => unitType == UnitType.Metric ? "m" : "yd",
				Units.DistanceLong => unitType == UnitType.Metric ? "km" : "mi",
				Units.ElectricCapacity => unitType == UnitType.Metric ? "kW⋅h" : "kW⋅h",
				Units.Force => unitType == UnitType.Metric ? "N" : "N",
				Units.Frequency => unitType == UnitType.Metric ? "Hz" : "Hz",
				Units.FuelConsumption => unitType == UnitType.Metric ? "L/100km" : "mpg",
				Units.Liquid => unitType == UnitType.Metric ? "L" : "gal",
				Units.Power => unitType == UnitType.Metric ? "hp" : "kW",
				Units.Pressure => unitType == UnitType.Metric ? "bar" : "psi",
				Units.Size => unitType == UnitType.Metric ? "cm" : "in",
				Units.SizeAccurate => unitType == UnitType.Metric ? "mm" : "mm",
				Units.Speed => unitType == UnitType.Metric ? "km/h" : "mph",
				Units.Time => unitType == UnitType.Metric ? "s" : "s",
				Units.TimeAccurate => unitType == UnitType.Metric ? "ms" : "ms",
				Units.Torque => unitType == UnitType.Metric ? "N⋅m" : "ft-lb",
				Units.Velocity => unitType == UnitType.Metric ? "m/s" : "ft/s",
				Units.Volume => unitType == UnitType.Metric ? "m³" : "ft³",
				Units.VolumeAccurate => unitType == UnitType.Metric ? "cm³" : "in³",
				Units.VolumeLarge => unitType == UnitType.Metric ? "km³" : "mi³",
				Units.Weight => unitType == UnitType.Metric ? "kg" : "lbs",
				_ => default,
			};
		}
		/// <summary>
		/// Gets the full name of the unit for a specific unit type in either metric or imperial system.
		/// 
		/// This method returns the complete, human-readable name of various physical quantities
		/// in either the metric or imperial measurement system, suitable for display in user interfaces
		/// or documentation where abbreviations might be unclear.
		/// 
		/// For example:
		/// - For distance in metric: "Metre"
		/// - For distance in imperial: "Foot"
		/// - For weight in metric: "Kilogram"
		/// - For weight in imperial: "Pound"
		/// 
		/// The method handles a comprehensive range of physical quantities defined in the Units enum,
		/// including specialized variants for different scales (accurate, large) and provides
		/// appropriate localized full names that follow standard conventions for scientific
		/// and engineering terminology.
		/// </summary>
		/// <param name="unit">The unit type to get the full name for, such as Distance, Weight, Volume, etc.</param>
		/// <param name="unitType">The unit system (Metric or Imperial) that determines which name set to use.</param>
		/// <returns>The full name of the unit as a string. Returns default (null) for unrecognized unit types.</returns>
		public static string FullUnit(Units unit, UnitType unitType)
		{
			return unit switch
			{
				Units.AngularVelocity => unitType == UnitType.Metric ? "Radians per Second" : "Radians per Second",
				Units.Area => unitType == UnitType.Metric ? "Square Metre" : "Square Feet",
				Units.AreaAccurate => unitType == UnitType.Metric ? "Square Centimetre" : "Square Inch",
				Units.AreaLarge => unitType == UnitType.Metric ? "Square Kilometre" : "Square Mile",
				Units.ElectricConsumption => unitType == UnitType.Metric ? "KiloWatt Hour per 100 Kilometres" : "KiloWatt Hour per 100 Miles",
				Units.Density => unitType == UnitType.Metric ? "Kilogram per Cubic Metre" : "Pound-mass per Cubic Foot",
				Units.Distance => unitType == UnitType.Metric ? "Metre" : "Foot",
				Units.DistanceAccurate => unitType == UnitType.Metric ? "Metre" : "Yard",
				Units.DistanceLong => unitType == UnitType.Metric ? "Kilometre" : "Mile",
				Units.ElectricCapacity => unitType == UnitType.Metric ? "KiloWatt Hour" : "KiloWatt Hour",
				Units.Force => unitType == UnitType.Metric ? "Newton" : "Newton",
				Units.Frequency => unitType == UnitType.Metric ? "Hertz" : "Hertz",
				Units.FuelConsumption => unitType == UnitType.Metric ? "Litres per 100 Kilometre" : "Miles per Gallon",
				Units.Liquid => unitType == UnitType.Metric ? "Litre" : "Gallon",
				Units.Power => unitType == UnitType.Metric ? "Horsepower" : "KiloWatt",
				Units.Pressure => unitType == UnitType.Metric ? "Bar" : "Pounds per Square Inch",
				Units.Size => unitType == UnitType.Metric ? "Centimetre" : "Inch",
				Units.SizeAccurate => unitType == UnitType.Metric ? "Millimetre" : "Millimetre",
				Units.Speed => unitType == UnitType.Metric ? "Kilometres per Hour" : "Miles per Hour",
				Units.Time => unitType == UnitType.Metric ? "Second" : "Second",
				Units.TimeAccurate => unitType == UnitType.Metric ? "Millisecond" : "Millisecond",
				Units.Torque => unitType == UnitType.Metric ? "Newton⋅Metre" : "Pound⋅Feet",
				Units.Velocity => unitType == UnitType.Metric ? "Metres per Second" : "Feet per Second",
				Units.Volume => unitType == UnitType.Metric ? "Cubic Metre" : "Cubic Foot",
				Units.VolumeAccurate => unitType == UnitType.Metric ? "Cubic Centimetre" : "Cubic Inch",
				Units.VolumeLarge => unitType == UnitType.Metric ? "Cubic Kilometre" : "Cubic Mile",
				Units.Weight => unitType == UnitType.Metric ? "Kilogram" : "Pound",
				_ => default,
			};
		}
		/// <summary>
		/// Extracts a numeric value from a string that may contain a unit suffix or prefix.
		/// This method parses the input string, identifies any numeric values present, and returns the first valid number found.
		/// The method handles strings that may contain both numbers and text (such as "10 kg" or "5.2 meters"),
		/// ignoring any non-numeric parts. If multiple numbers are present, only the first one is returned.
		/// </summary>
		/// <param name="value">The string containing a number and possibly a unit (e.g., "10 kg", "5.2 meters").</param>
		/// <returns>The extracted numeric value as a float. Returns 0 if no valid number is found or if the string is null, empty, or contains only whitespace.</returns>
		/// <remarks>
		/// The method splits the input string by spaces and attempts to parse each segment as a float.
		/// It preserves only the numeric parts, discarding any text that cannot be parsed as a number.
		/// </remarks>
		public static float ValueWithUnitToNumber(string value)
		{
			string[] valueArray = value.Split(' ');

			value = "";

			for (int i = 0; i < valueArray.Length; i++)
				if (float.TryParse(valueArray[i], out float number))
					value += number + (i < valueArray.Length - 1 ? " " : "");

			return !value.IsNullOrEmpty() && !value.IsNullOrWhiteSpace() && float.TryParse(value, out float result) ? result : 0f;
		}
		/// <summary>
		/// Extracts a numeric value from a string that may contain a unit, and converts it to the specified unit system.
		/// This method parses the input string, identifies any numeric values present, and applies the appropriate
		/// conversion factor based on the specified unit type and system. It handles special cases for units that
		/// require division rather than multiplication during conversion (like fuel consumption units).
		/// </summary>
		/// <param name="value">The string containing a number and possibly a unit (e.g., "10 kg", "5.2 miles").</param>
		/// <param name="unit">The unit type to convert to, such as Weight, Distance, or Speed.</param>
		/// <param name="unitType">The unit system (Metric or Imperial) to convert the value to.</param>
		/// <returns>The extracted and converted numeric value as a float. For divider units (like fuel consumption),
		/// the conversion is applied differently. Returns 0 if no valid number is found or if the string is invalid.</returns>
		/// <remarks>
		/// The method first extracts the numeric portion from the string, then applies the appropriate conversion
		/// factor based on the unit type and system. For most units, the extracted value is divided by the unit multiplier,
		/// but for divider units (when IsDividerUnit returns true), the unit multiplier is divided by the extracted value
		/// when using non-metric units.
		/// </remarks>
		public static float ValueWithUnitToNumber(string value, Units unit, UnitType unitType)
		{
			string[] valueArray = value.Split(' ');

			value = "";

			for (int i = 0; i < valueArray.Length; i++)
				if (float.TryParse(valueArray[i], out float number))
					value += number + (i < valueArray.Length - 1 ? " " : "");

			return !value.IsNullOrEmpty() && !value.IsNullOrWhiteSpace() && float.TryParse(value, out float result) ? (IsDividerUnit(unit) && unitType != UnitType.Metric ? UnitMultiplier(unit, unitType) / result : result / UnitMultiplier(unit, unitType)) : 0f;
		}
		/// <summary>
		/// Formats a numeric value as a string with an appended unit, with optional rounding to the nearest integer.
		/// This method handles special cases for infinity values and provides consistent formatting for numeric values with units.
		/// </summary>
		/// <param name="number">The numeric value to format. Can be any float value including special values like infinity.</param>
		/// <param name="unit">The unit string to append to the number (e.g., "kg", "m/s", "mph"). This string is separated from the number by a space.</param>
		/// <param name="rounded">When true, the number will be rounded to the nearest integer before formatting. When false, the exact value is used.</param>
		/// <returns>
		/// A formatted string in the format "number unit". Returns "Infinity" or "-Infinity" for infinite values.
		/// For regular numbers, returns the number (rounded if specified) followed by a space and the unit string.
		/// </returns>
		/// <remarks>
		/// This method is useful for displaying measurements in user interfaces where consistent formatting is important.
		/// The method handles both positive and negative infinity as special cases with text representations.
		/// Note that there's a redundant check for negative infinity in the condition that should be simplified.
		/// </remarks>
		public static string NumberToValueWithUnit(float number, string unit, bool rounded)
		{
			if (number == math.INFINITY)
				return "Infinity";
			else if (number == -math.INFINITY || number == -math.INFINITY)
				return "-Infinity";

			return $"{(rounded ? math.round(number) : number)} {unit}";
		}
		/// <summary>
		/// Formats a number with a unit string, with a specified number of decimal places.
		/// This method handles special cases for infinity values and provides consistent formatting for numeric values with units.
		/// </summary>
		/// <param name="number">The numeric value to format. Can be any float value including special values like infinity.</param>
		/// <param name="unit">The unit string to append to the number (e.g., "kg", "m/s", "mph"). This string is separated from the number by a space.</param>
		/// <param name="decimals">The number of decimal places to include in the formatted number.</param>
		/// <returns>
		/// A formatted string in the format "number unit". Returns "Infinity" or "-Infinity" for infinite values.
		/// For regular numbers, returns the number (rounded to the specified decimal places) followed by a space and the unit string.
		/// </returns>
		public static string NumberToValueWithUnit(float number, string unit, uint decimals)
		{
			if (number == math.INFINITY)
				return "Infinity";
			else if (number == -math.INFINITY || number == -math.INFINITY)
				return "-Infinity";

			return $"{Round(number, decimals)} {unit}";
		}
		/// <summary>
		/// Formats a number with a unit type and system, with optional rounding.
		/// This method applies the appropriate unit conversion based on the unit type and system,
		/// handling special cases for divider units and infinity values.
		/// </summary>
		/// <param name="number">The numeric value to format. Can be any float value including special values like infinity.</param>
		/// <param name="unit">The unit type to use for conversion and display (e.g., Weight, Distance, Speed).</param>
		/// <param name="unitType">The unit system (Metric or Imperial) to convert the value to.</param>
		/// <param name="rounded">Whether to round the number to the nearest integer. If true, the number is rounded; if false, the exact value is used.</param>
		/// <returns>
		/// A formatted string with the converted number and appropriate unit symbol. Returns "Infinity" or "-Infinity" for infinite values.
		/// For regular numbers, returns the converted number (rounded if specified) followed by a space and the unit symbol.
		/// </returns>
		public static string NumberToValueWithUnit(float number, Units unit, UnitType unitType, bool rounded)
		{
			if (number == math.INFINITY)
				return "Infinity";
			else if (number == -math.INFINITY)
				return "-Infinity";

			if (IsDividerUnit(unit) && unitType != UnitType.Metric)
				number = UnitMultiplier(unit, unitType) / number;
			else
				number *= UnitMultiplier(unit, unitType);

			return $"{(rounded ? math.round(number) : number)} {Unit(unit, unitType)}";
		}
		/// <summary>
		/// Formats a number with a unit type and system, with a specified number of decimal places.
		/// This method applies the appropriate unit conversion based on the unit type and system,
		/// handling special cases for divider units and infinity values.
		/// </summary>
		/// <param name="number">The numeric value to format. Can be any float value including special values like infinity.</param>
		/// <param name="unit">The unit type to use for conversion and display (e.g., Weight, Distance, Speed).</param>
		/// <param name="unitType">The unit system (Metric or Imperial) to convert the value to.</param>
		/// <param name="decimals">The number of decimal places to include in the formatted number.</param>
		/// <returns>
		/// A formatted string with the converted number and appropriate unit symbol. Returns "Infinity" or "-Infinity" for infinite values.
		/// For regular numbers, returns the converted number (rounded to the specified decimal places) followed by a space and the unit symbol.
		/// </returns>
		public static string NumberToValueWithUnit(float number, Units unit, UnitType unitType, uint decimals)
		{
			if (number == math.INFINITY)
				return "Infinity";
			else if (number == -math.INFINITY || number == -math.INFINITY)
				return "-Infinity";

			if (IsDividerUnit(unit) && unitType != UnitType.Metric)
				number = UnitMultiplier(unit, unitType) / number;
			else
				number *= UnitMultiplier(unit, unitType);

			return $"{Round(number, decimals)} {Unit(unit, unitType)}";
		}
		/// <summary>
		/// Converts a number to its ordinal representation (1st, 2nd, 3rd, etc.).
		/// This method examines the last digit of the number to determine the appropriate ordinal suffix.
		/// </summary>
		/// <param name="number">The integer number to convert to an ordinal representation.</param>
		/// <returns>
		/// A string containing the number with its appropriate ordinal suffix:
		/// - Numbers ending in 1 (except 11) get "st" (e.g., 1st, 21st)
		/// - Numbers ending in 2 (except 12) get "nd" (e.g., 2nd, 22nd)
		/// - Numbers ending in 3 (except 13) get "rd" (e.g., 3rd, 23rd)
		/// - All other numbers get "th" (e.g., 4th, 11th, 12th, 13th)
		/// </returns>
		public static string ClassifyNumber(int number)
		{
			return number.ToString().LastOrDefault() switch
			{
				'1' => number + "st",
				'2' => number + "nd",
				'3' => number + "rd",
				_ => number + "th",
			};
		}
		/// <summary>
		/// Gets all child GameObjects of a parent GameObject.
		/// This method returns all direct and indirect children of the specified GameObject,
		/// excluding the parent GameObject itself.
		/// </summary>
		/// <param name="gameObject">The parent GameObject whose children should be retrieved.</param>
		/// <returns>
		/// An array of GameObject references containing all children of the specified parent GameObject.
		/// The array does not include the parent GameObject itself.
		/// </returns>
		public static GameObject[] GetChilds(GameObject gameObject)
		{
			return (from Transform child in gameObject.GetComponentsInChildren<Transform>() where gameObject.transform != child select child.gameObject).ToArray();
		}
#if UNITY_6000_0_OR_NEWER
		/// <summary>
		/// Evaluates the friction between two physics materials based on slip value and friction combine mode.
		/// This method determines whether to use static or dynamic friction based on the slip value,
		/// then applies the appropriate friction combine mode from the reference material.
		/// </summary>
		/// <param name="slip">The slip value to determine whether to use static friction (when slip ≈ 0) or dynamic friction (when slip > 0).</param>
		/// <param name="refMaterial">The reference physics material that defines the friction combine mode.</param>
		/// <param name="material">The second physics material to combine with the reference material.</param>
		/// <returns>
		/// The calculated friction value based on the combine mode of the reference material:
		/// - Average: The average of the two materials' friction values
		/// - Multiply: The product of the two materials' friction values
		/// - Minimum: The minimum of the two materials' friction values
		/// - Maximum: The maximum of the two materials' friction values
		/// Returns 0 if the combine mode is not recognized.
		/// </returns>
		public static float EvaluateFriction(float slip, PhysicsMaterial refMaterial, PhysicsMaterial material)
		{
			return refMaterial.frictionCombine switch
			{
				PhysicsMaterialCombine.Average => Round(slip, 2) != 0f ? (refMaterial.dynamicFriction + material.dynamicFriction) * .5f : (refMaterial.staticFriction + material.staticFriction) * .5f,
				PhysicsMaterialCombine.Multiply => Round(slip, 2) != 0f ? refMaterial.dynamicFriction * material.dynamicFriction : refMaterial.staticFriction * material.staticFriction,
				PhysicsMaterialCombine.Minimum => Round(slip, 2) != 0f ? math.min(refMaterial.dynamicFriction, material.dynamicFriction) : math.min(refMaterial.staticFriction, material.staticFriction),
				PhysicsMaterialCombine.Maximum => Round(slip, 2) != 0f ? math.max(refMaterial.dynamicFriction, material.dynamicFriction) : math.max(refMaterial.staticFriction, material.staticFriction),
				_ => 0f,
			};
		}
		
		/// <summary>
		/// Evaluates the friction between a physics material and a stiffness value based on slip value and friction combine mode.
		/// This method determines whether to use static or dynamic friction based on the slip value,
		/// then applies the appropriate friction combine mode from the reference material, treating the stiffness as a friction value.
		/// </summary>
		/// <param name="slip">The slip value to determine whether to use static friction (when slip ≈ 0) or dynamic friction (when slip > 0).</param>
		/// <param name="refMaterial">The reference physics material that defines the friction combine mode.</param>
		/// <param name="stiffness">The stiffness value to combine with the material's friction, treated as a friction coefficient.</param>
		/// <returns>
		/// The calculated friction value based on the combine mode of the reference material:
		/// - Average: The average of the material's friction and the stiffness value
		/// - Multiply: The product of the material's friction and the stiffness value
		/// - Minimum: The minimum of the material's friction and the stiffness value
		/// - Maximum: The maximum of the material's friction and the stiffness value
		/// Returns 0 if the combine mode is not recognized.
		/// </returns>
		public static float EvaluateFriction(float slip, PhysicsMaterial refMaterial, float stiffness)
		{
			return refMaterial.frictionCombine switch
			{
				PhysicsMaterialCombine.Average => Round(slip, 2) != 0f ? (refMaterial.dynamicFriction + stiffness) * .5f : (refMaterial.staticFriction + stiffness) * .5f,
				PhysicsMaterialCombine.Multiply => Round(slip, 2) != 0f ? refMaterial.dynamicFriction * stiffness : refMaterial.staticFriction * stiffness,
				PhysicsMaterialCombine.Minimum => Round(slip, 2) != 0f ? math.min(refMaterial.dynamicFriction, stiffness) : math.max(refMaterial.staticFriction, stiffness),
				PhysicsMaterialCombine.Maximum => Round(slip, 2) != 0f ? math.max(refMaterial.dynamicFriction, stiffness) : math.max(refMaterial.staticFriction, stiffness),
				_ => 0f,
			};
		}
#else
		/// <summary>
		/// Evaluates the friction between two physics materials based on slip value and friction combine mode.
		/// This method determines whether to use static or dynamic friction based on the slip value,
		/// then applies the appropriate friction combine mode from the reference material.
		/// </summary>
		/// <param name="slip">The slip value to determine whether to use static friction (when slip ≈ 0) or dynamic friction (when slip > 0).</param>
		/// <param name="refMaterial">The reference physics material that defines the friction combine mode.</param>
		/// <param name="material">The second physics material to combine with the reference material.</param>
		/// <returns>
		/// The calculated friction value based on the combine mode of the reference material:
		/// - Average: The average of the two materials' friction values
		/// - Multiply: The product of the two materials' friction values
		/// - Minimum: The minimum of the two materials' friction values
		/// - Maximum: The maximum of the two materials' friction values
		/// Returns 0 if the combine mode is not recognized.
		/// </returns>
		public static float EvaluateFriction(float slip, PhysicMaterial refMaterial, PhysicMaterial material)
		{
			return refMaterial.frictionCombine switch
			{
				PhysicMaterialCombine.Average => Round(slip, 2) != 0f ? (refMaterial.dynamicFriction + material.dynamicFriction) * .5f : (refMaterial.staticFriction + material.staticFriction) * .5f,
				PhysicMaterialCombine.Multiply => Round(slip, 2) != 0f ? refMaterial.dynamicFriction * material.dynamicFriction : refMaterial.staticFriction * material.staticFriction,
				PhysicMaterialCombine.Minimum => Round(slip, 2) != 0f ? math.min(refMaterial.dynamicFriction, material.dynamicFriction) : math.min(refMaterial.staticFriction, material.staticFriction),
				PhysicMaterialCombine.Maximum => Round(slip, 2) != 0f ? math.max(refMaterial.dynamicFriction, material.dynamicFriction) : math.max(refMaterial.staticFriction, material.staticFriction),
				_ => 0f,
			};
		}
		/// <summary>
		/// Evaluates the friction between a physics material and a stiffness value based on slip value and friction combine mode.
		/// This method determines whether to use static or dynamic friction based on the slip value,
		/// then applies the appropriate friction combine mode from the reference material, treating the stiffness as a friction value.
		/// </summary>
		/// <param name="slip">The slip value to determine whether to use static friction (when slip ≈ 0) or dynamic friction (when slip > 0).</param>
		/// <param name="refMaterial">The reference physics material that defines the friction combine mode.</param>
		/// <param name="stiffness">The stiffness value to combine with the material's friction, treated as a friction coefficient.</param>
		/// <returns>
		/// The calculated friction value based on the combine mode of the reference material:
		/// - Average: The average of the material's friction and the stiffness value
		/// - Multiply: The product of the material's friction and the stiffness value
		/// - Minimum: The minimum of the material's friction and the stiffness value
		/// - Maximum: The maximum of the material's friction and the stiffness value
		/// Returns 0 if the combine mode is not recognized.
		/// </returns>
		public static float EvaluateFriction(float slip, PhysicMaterial refMaterial, float stiffness)
		{
			return refMaterial.frictionCombine switch
			{
				PhysicMaterialCombine.Average => Round(slip, 2) != 0f ? (refMaterial.dynamicFriction + stiffness) * .5f : (refMaterial.staticFriction + stiffness) * .5f,
				PhysicMaterialCombine.Multiply => Round(slip, 2) != 0f ? refMaterial.dynamicFriction * stiffness : refMaterial.staticFriction * stiffness,
				PhysicMaterialCombine.Minimum => Round(slip, 2) != 0f ? math.min(refMaterial.dynamicFriction, stiffness) : math.max(refMaterial.staticFriction, stiffness),
				PhysicMaterialCombine.Maximum => Round(slip, 2) != 0f ? math.max(refMaterial.dynamicFriction, stiffness) : math.max(refMaterial.staticFriction, stiffness),
				_ => 0f,
			};
		}
#endif
		/// <summary>
		/// Calculates the braking distance based on speed and friction using a simplified model.
		/// This method uses a custom interpolation-based approximation rather than the standard physics formula.
		/// </summary>
		/// <param name="speed">The initial speed of the object.</param>
		/// <param name="friction">The friction coefficient affecting the braking.</param>
		/// <returns>The estimated braking distance based on the simplified model.</returns>
		/// <remarks>
		/// This method is marked as obsolete and should be replaced with the more accurate physics-based
		/// braking distance calculations. It uses a custom interpolation between predefined distance values
		/// based on the friction and speed.
		/// </remarks>
		[Obsolete]
		public static float BrakingDistance(float speed, float friction)
		{
			friction = InverseLerpUnclamped(1f, 1.5f, friction);

			return ClampInfinity(LerpUnclamped(LerpUnclamped(30f, 26f, friction), LerpUnclamped(143f, 113f, friction), InverseLerpUnclamped(40f, 110f, speed)));
		}
		
		/// <summary>
		/// Calculates the braking distance using the standard physics formula for stopping distance.
		/// This method applies the formula: d = v²/(2μg), where d is distance, v is velocity, 
		/// μ is the friction coefficient, and g is the gravitational acceleration.
		/// </summary>
		/// <param name="velocity">The initial velocity of the object in meters per second.</param>
		/// <param name="friction">The friction coefficient between the object and the surface.</param>
		/// <param name="gravity">The gravitational acceleration in meters per second squared. Defaults to Earth's gravity (9.81 m/s²).</param>
		/// <returns>The calculated braking distance in meters required to bring the object to a complete stop.</returns>
		public static float BrakingDistance(float velocity, float friction, float gravity = 9.81f)
		{
			return velocity * velocity / (2f * friction * gravity);
		}
		
		/// <summary>
		/// Calculates the braking distance required to slow down from an initial velocity to a target velocity.
		/// This method applies the formula: d = (v₁² - v₂²)/(2μg), where d is distance, v₁ is initial velocity,
		/// v₂ is target velocity, μ is the friction coefficient, and g is the gravitational acceleration.
		/// </summary>
		/// <param name="velocity">The initial velocity of the object in meters per second.</param>
		/// <param name="targetVelocity">The target velocity to slow down to in meters per second.</param>
		/// <param name="friction">The friction coefficient between the object and the surface.</param>
		/// <param name="gravity">The gravitational acceleration in meters per second squared. Defaults to Earth's gravity (9.81 m/s²).</param>
		/// <returns>The calculated braking distance in meters required to slow down to the target velocity.</returns>
		public static float BrakingDistance(float velocity, float targetVelocity, float friction, float gravity = 9.81f)
		{
			return (velocity * velocity - targetVelocity * targetVelocity) / (2f * friction * gravity);
		}
		
		/// <summary>
		/// Converts RPM (Revolutions Per Minute) to linear speed based on the radius of a rotating object.
		/// This method applies the formula: v = ωr, where v is linear speed, ω is angular velocity, and r is radius.
		/// The constant 0.377 is derived from 2π/60 to convert from RPM to radians per second.
		/// </summary>
		/// <param name="rpm">The rotational speed in Revolutions Per Minute.</param>
		/// <param name="radius">The radius of the rotating object in meters.</param>
		/// <returns>The linear speed in meters per second at the specified radius.</returns>
		public static float RPMToSpeed(float rpm, float radius)
		{
			return radius * .377f * rpm;
		}
		
		/// <summary>
		/// Converts linear speed to RPM (Revolutions Per Minute) based on the radius of a rotating object.
		/// This method applies the formula: ω = v/r, where ω is angular velocity, v is linear speed, and r is radius.
		/// The constant 0.377 is derived from 2π/60 to convert between radians per second and RPM.
		/// </summary>
		/// <param name="speed">The linear speed in meters per second.</param>
		/// <param name="radius">The radius of the rotating object in meters.</param>
		/// <returns>
		/// The rotational speed in RPM. If the radius is zero or negative, the method calculates RPM
		/// without considering the radius to avoid division by zero errors.
		/// </returns>
		public static float SpeedToRPM(float speed, float radius)
		{
			if (radius <= 0f)
				return speed / .377f;

			return speed / radius / .377f;
		}
		/// <summary>
		/// Checks if a layer mask contains a specific layer by comparing the layer's bit position in the mask.
		/// This method uses bitwise operations to determine if the specified layer is included in the mask.
		/// </summary>
		/// <param name="mask">The layer mask to check, represented as a Unity LayerMask.</param>
		/// <param name="layer">The layer index to check for (0-31).</param>
		/// <returns>True if the layer mask contains the specified layer, false otherwise.</returns>
		public static bool MaskHasLayer(LayerMask mask, int layer)
		{
			return MaskHasLayer(mask.value, layer);
		}
		/// <summary>
		/// Checks if a layer mask contains a specific layer by comparing the layer's bit position in the mask.
		/// This method uses bitwise operations to determine if the specified layer is included in the mask.
		/// </summary>
		/// <param name="mask">The layer mask to check, represented as an integer.</param>
		/// <param name="layer">The layer index to check for (0-31).</param>
		/// <returns>True if the layer mask contains the specified layer, false otherwise.</returns>
		public static bool MaskHasLayer(int mask, int layer)
		{
			return (mask & 1 << layer) != 0;
		}
		/// <summary>
		/// Checks if a layer mask contains a specific layer identified by its name.
		/// This method converts the layer name to its index and then checks if that layer is included in the mask.
		/// </summary>
		/// <param name="mask">The layer mask to check, represented as a Unity LayerMask.</param>
		/// <param name="layer">The name of the layer to check for.</param>
		/// <returns>True if the layer mask contains the specified layer, false otherwise.</returns>
		public static bool MaskHasLayer(LayerMask mask, string layer)
		{
			return MaskHasLayer(mask.value, layer);
		}
		/// <summary>
		/// Checks if a layer mask contains a specific layer identified by its name.
		/// This method converts the layer name to its index and then checks if that layer is included in the mask.
		/// </summary>
		/// <param name="mask">The layer mask to check, represented as an integer.</param>
		/// <param name="layer">The name of the layer to check for.</param>
		/// <returns>True if the layer mask contains the specified layer, false otherwise.</returns>
		public static bool MaskHasLayer(int mask, string layer)
		{
			return MaskHasLayer(mask, LayerMask.NameToLayer(layer));
		}
		/// <summary>
		/// Creates an exclusive layer mask that includes all layers EXCEPT the specified layer.
		/// This is useful for raycasts or collision detection that should ignore a specific layer.
		/// </summary>
		/// <param name="name">The name of the layer to exclude from the mask.</param>
		/// <returns>An integer representing a layer mask that includes all layers except the specified one.</returns>
		public static int ExclusiveMask(string name)
		{
			return ExclusiveMask(new string[] { name });
		}
		/// <summary>
		/// Creates an exclusive layer mask that includes all layers EXCEPT the specified layer.
		/// This is useful for raycasts or collision detection that should ignore a specific layer.
		/// </summary>
		/// <param name="layer">The index of the layer to exclude from the mask (0-31).</param>
		/// <returns>An integer representing a layer mask that includes all layers except the specified one.</returns>
		public static int ExclusiveMask(int layer)
		{
			return ExclusiveMask(new int[] { layer });
		}
		/// <summary>
		/// Creates an exclusive layer mask that includes all layers EXCEPT the specified layers.
		/// This is useful for raycasts or collision detection that should ignore multiple specific layers.
		/// </summary>
		/// <param name="layers">An array of layer names to exclude from the mask.</param>
		/// <returns>An integer representing a layer mask that includes all layers except those specified.</returns>
		public static int ExclusiveMask(params string[] layers)
		{
			return ~LayerMask.GetMask(layers);
		}
		/// <summary>
		/// Creates an exclusive layer mask that includes all layers EXCEPT the specified layers.
		/// This is useful for raycasts or collision detection that should ignore multiple specific layers.
		/// </summary>
		/// <param name="layers">An array of layer indices (0-31) to exclude from the mask.</param>
		/// <returns>An integer representing a layer mask that includes all layers except those specified.</returns>
		public static int ExclusiveMask(params int[] layers)
		{
			return ExclusiveMask(layers.Select(layer => LayerMask.LayerToName(layer)).ToArray());
		}
		/// <summary>
		/// Converts a boolean value to an integer: 1 for true, 0 for false.
		/// This is useful for mathematical operations that need to use boolean conditions as numeric values.
		/// </summary>
		/// <param name="condition">The boolean value to convert.</param>
		/// <returns>1 if the condition is true, 0 if the condition is false.</returns>
		public static int BoolToNumber(bool condition)
		{
			return condition ? 1 : 0;
		}
		/// <summary>
		/// Smoothly transitions a numeric value toward 0 or 1 based on a boolean condition.
		/// This method is useful for creating smooth animations or transitions based on boolean states.
		/// </summary>
		/// <param name="source">The current numeric value to be transitioned.</param>
		/// <param name="condition">The boolean condition determining the target value (1 for true, 0 for false).</param>
		/// <param name="damping">The speed of the transition. Higher values result in faster transitions.</param>
		/// <returns>
		/// A value that has moved from the source value toward either 0 or 1 (based on the condition)
		/// at a rate determined by the damping factor and the time since the last frame.
		/// </returns>
		public static float BoolToNumber(float source, bool condition, float damping = 2.5f)
		{
			return Mathf.MoveTowards(source, BoolToNumber(condition), Time.deltaTime * damping);
		}
		/// <summary>
		/// Inverts the sign of a number based on a boolean condition: -1 for true, 1 for false.
		/// This is useful for flipping directions or values based on a boolean state.
		/// </summary>
		/// <param name="invert">The boolean condition determining whether to invert the sign.</param>
		/// <returns>-1 if the condition is true, 1 if the condition is false.</returns>
		public static int InvertSign(bool invert)
		{
			return invert ? -1 : 1;
		}
		/// <summary>
		/// Converts a numeric value to a boolean value.
		/// This method rounds the number to the nearest integer, clamps it between 0 and 1,
		/// and then returns true if the result is not zero.
		/// </summary>
		/// <param name="number">The numeric value to convert to a boolean.</param>
		/// <returns>True if the rounded and clamped number is not zero, false otherwise.</returns>
		public static bool NumberToBool(float number)
		{
			return Clamp01((int)math.round(number)) != 0f;
		}
		/// <summary>
		/// Validates a username string according to specific rules for online systems.
		/// </summary>
		/// <param name="username">The username string to validate.</param>
		/// <returns>True if the username meets all validation criteria, false otherwise.</returns>
		/// <remarks>
		/// A valid username must:
		/// - Not be null or empty
		/// - Be between 6 and 64 characters long
		/// - Contain only alphanumeric characters and the symbols '_', '-', and '.'
		/// - Not contain any other symbols
		/// </remarks>
		public static bool ValidateUsername(string username)
		{
			if (username.IsNullOrEmpty())
				return false;

			int count = username.Length;

			if (count < 6 || count > 64)
				return false;

			for (int i = 0; i < count; i++)
				if (IsSymbol(username[i]) && username[i] != '_' && username[i] != '-' && username[i] != '.')
					return false;

			return true;
		}
		/// <summary>
		/// Validates a name string according to specific rules for personal names.
		/// </summary>
		/// <param name="name">The name string to validate.</param>
		/// <returns>True if the name meets all validation criteria, false otherwise.</returns>
		/// <remarks>
		/// A valid name must:
		/// - Not be null or empty (after trimming whitespace)
		/// - Be between 2 and 64 characters long
		/// - Contain only alphabetic characters and the symbols ' ' (space), '.', and '-'
		/// - Not contain any numeric characters or other symbols
		/// </remarks>
		public static bool ValidateName(string name)
		{
			if ((name?.Trim()).IsNullOrEmpty())
				return false;

			int count = name.Length;

			if (count < 2 || count > 64)
				return false;

			for (int i = 0; i < count; i++)
				if (IsNumber(name[i]) || IsSymbol(name[i]) && name[i] != ' ' && name[i] != '.' && name[i] != '-')
					return false;

			return true;
		}
		/// <summary>
		/// Validates an email address according to standard rules and optionally checks the domain.
		/// This method first checks the email format using the MailAddress class, then optionally
		/// verifies that the domain is not a known disposable email domain and that it exists.
		/// </summary>
		/// <param name="email">The email address to validate.</param>
		/// <param name="lookUpDomain">Whether to check if the domain exists by performing a DNS lookup.</param>
		/// <returns>
		/// True if the email meets all validation criteria, false otherwise.
		/// If lookUpDomain is true, also returns false if the domain doesn't exist or is unreachable.
		/// </returns>
		public static bool ValidateEmail(string email, bool lookUpDomain)
		{
			if (email.IsNullOrEmpty())
				return false;

			try
			{
				new MailAddress(email);
			}
			catch
			{
				return false;
			}

			string emailDomain = email.Split('@')[1].ToLower();
			string emailDomainName = emailDomain.Split('.')[0];

			foreach (string disposableDomain in disposableEmailDomains)
				if (emailDomain == disposableDomain || emailDomainName.Contains(disposableDomain))
					return false;

			if (!lookUpDomain)
				return true;

			try
			{
				IPHostEntry entry = Dns.GetHostEntry(emailDomain);

				return entry.AddressList != null && entry.AddressList.Length > 0;
			}
			catch
			{
				return false;
			}
		}
		/// <summary>
		/// Validates a URL and optionally checks if it's accessible.
		/// This method first validates the URL format, ensuring it has a proper protocol (http/https),
		/// then optionally attempts to connect to the URL to verify its accessibility.
		/// </summary>
		/// <param name="url">The URL to validate. May be modified to include a protocol if missing.</param>
		/// <param name="lookUpURL">Whether to check if the URL is accessible by making a network request.</param>
		/// <param name="throwOnError">Whether to throw exceptions on network errors or return false silently.</param>
		/// <returns>True if the URL is valid and (if lookUpURL is true) accessible, false otherwise.</returns>
		public static bool ValidateURL(ref string url, bool lookUpURL, bool throwOnError = true)
		{
			if (url.IsNullOrEmpty())
				return false;

			url = url.Replace("\\", "/");

			if (!url.ToLower().StartsWith("https://") && !url.ToLower().StartsWith("http://"))
			{
				if (url.Contains("://"))
					return false;

				url = $"http://{url}";
			}

			string noProtocolURL = url.ToLower().Split("://")[1];

			if (noProtocolURL == "localhost" || noProtocolURL.StartsWith("localhost/"))
				return false;

			if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
				return false;

			if (lookUpURL)
			{
				using UnityWebRequest request = UnityWebRequest.Get(uri);

				request.method = "HEAD";
				request.timeout = 15;
				request.disposeDownloadHandlerOnDispose = true;
				request.disposeCertificateHandlerOnDispose = true;

				try
				{
					request.SendWebRequest();

					while (request.result == UnityWebRequest.Result.InProgress)
						continue;

					return request.result == UnityWebRequest.Result.Success;
				}
				catch (Exception e)
				{
					if (throwOnError)
						throw e;

					return false;
				}
				finally
				{
					request.Dispose();
				}
			}

			return true;
		}
		/// <summary>
		/// Validates a date string format.
		/// Checks if the date string follows the format "MM/DD/YYYY" with proper separators
		/// and ensures all characters are either digits or forward slashes.
		/// </summary>
		/// <param name="date">The date string to validate.</param>
		/// <returns>True if the date string is valid, false otherwise.</returns>
		public static bool ValidateDate(string date)
		{
			if (date.Length != 10)
				return false;
			else if (date[2] != '/' || date[5] != '/')
				return false;
			else if (date.Length == 10)
				return false;
			else
				for (int i = 0; i < date.Length; i++)
					if (date[i] != '0' && date[i] != '1' && date[i] != '2' && date[i] != '3' && date[i] != '4' && date[i] != '5' && date[i] != '6' && date[i] != '7' && date[i] != '8' && date[i] != '9' && date[i] != '/')
						return false;

			return true;
		}
		/// <summary>
		/// Validates a date string format.
		/// This is an obsolete method that has been replaced by ValidateDate.
		/// </summary>
		/// <param name="data">The date string to validate.</param>
		/// <returns>True if the date string is valid, false otherwise.</returns>
		[Obsolete("Use Utility.ValidateDate instead", true)]
		public static bool ValidDate(string data)
		{
			return false;
		}
		/// <summary>
		/// Converts a time value in seconds to a formatted string in the format "HH:MM:SS" or "MM:SS".
		/// Hours are only displayed if the time is at least one hour.
		/// </summary>
		/// <param name="time">The time value in seconds to convert.</param>
		/// <returns>A string representing the time in the format "HH:MM:SS" or "MM:SS".</returns>
		public static string TimeConverter(float time)
		{
			int seconds = (int)math.floor(time % 60);
			int minutes = (int)math.floor(time / 60);
			int hours = (int)math.floor(time / 3600);

			return (hours == 0 ? minutes.ToString() : (hours + ":" + minutes.ToString("00"))) + ":" + seconds.ToString("00");
		}
		/// <summary>
		/// Checks if a character is an alphabet letter (A-Z or a-z).
		/// The method converts the character to uppercase before checking.
		/// </summary>
		/// <param name="c">The character to check.</param>
		/// <returns>True if the character is an alphabet letter, false otherwise.</returns>
		public static bool IsAlphabet(char c)
		{
			c = char.ToUpper(c);

			return c == 'A' || c == 'B' || c == 'C' || c == 'D' || c == 'E' || c == 'F' || c == 'G' || c == 'H' || c == 'I' || c == 'J' || c == 'K' || c == 'L'
				|| c == 'M' || c == 'N' || c == 'O' || c == 'P' || c == 'Q' || c == 'R' || c == 'S' || c == 'T' || c == 'U' || c == 'V' || c == 'W' || c == 'X'
				|| c == 'Y' || c == 'Z';
		}
		/// <summary>
		/// Checks if a character is a numeric digit (0-9).
		/// </summary>
		/// <param name="c">The character to check.</param>
		/// <returns>True if the character is a numeric digit, false otherwise.</returns>
		public static bool IsNumber(char c)
		{
			return c == '0' || c == '1' || c == '2' || c == '3' || c == '4' || c == '5' || c == '6' || c == '7' || c == '8' || c == '9';
		}
		/// <summary>
		/// Checks if a character is a symbol (not an alphabet letter or numeric digit).
		/// Uses IsAlphabet and IsNumber methods to determine if the character is a symbol.
		/// </summary>
		/// <param name="c">The character to check.</param>
		/// <returns>True if the character is a symbol, false otherwise.</returns>
		public static bool IsSymbol(char c)
		{
			return !IsAlphabet(c) && !IsNumber(c);
		}
		/// <summary>
		/// Generates a random string with specified characteristics.
		/// The string can include uppercase letters, lowercase letters, numbers, and symbols
		/// based on the provided parameters.
		/// </summary>
		/// <param name="length">The length of the random string to generate.</param>
		/// <param name="upperChars">Whether to include uppercase letters (A-Z).</param>
		/// <param name="lowerChars">Whether to include lowercase letters (a-z).</param>
		/// <param name="numbers">Whether to include numeric digits (0-9).</param>
		/// <param name="symbols">Whether to include symbols (!@#$%^&()_+-{}[],.;).</param>
		/// <returns>A randomly generated string with the specified characteristics.</returns>
		public static string RandomString(int length, bool upperChars = true, bool lowerChars = true, bool numbers = true, bool symbols = true)
		{
			string chars = "";

			chars += upperChars ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ" : "";
			chars += lowerChars ? "abcdefghijklmnopqrstuvwxyz" : "";
			chars += numbers ? "0123456789" : "";
			chars += symbols ? "!@#$%^&()_+-{}[],.;" : "";

			return new string(Enumerable.Repeat(chars, length).Select(s => s[UnityEngine.Random.Range(0, s.Length)]).ToArray());
		}
		/// <summary>
		/// Converts a byte size to a human-readable string with appropriate units.
		/// Automatically scales the size to the most appropriate unit (B, KB, MB, GB, etc.)
		/// and formats the result with two decimal places.
		/// </summary>
		/// <param name="length">The size in bytes.</param>
		/// <returns>A formatted string representing the size with appropriate units (B, KB, MB, etc.).</returns>
		public static string GetReadableSize(long length)
		{
			string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB", "BB" };
			float size = length;
			int index = 0;

			while (size >= 1024f && index < sizes.Length - 1)
			{
				size /= 1024f;

				index++;
			}

			return $"{Round(size, 2):0.00} {sizes[index]}";
		}
		/// <summary>
		/// Draws an arrow using Gizmos for visualization in the Scene view.
		/// Creates a line with an arrowhead at the end point, with customizable arrowhead properties.
		/// </summary>
		/// <param name="pos">The starting position of the arrow.</param>
		/// <param name="direction">The direction and length of the arrow.</param>
		/// <param name="arrowHeadLength">The length of the arrow head lines.</param>
		/// <param name="arrowHeadAngle">The angle of the arrow head lines in degrees.</param>
		public static void DrawArrowForGizmos(Vector3 pos, Vector3 direction, float arrowHeadLength = .25f, float arrowHeadAngle = 20f)
		{
			Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f + arrowHeadAngle, 0f) * Vector3.forward;
			Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f - arrowHeadAngle, 0f) * Vector3.forward;

			Gizmos.DrawRay(pos, direction);
			Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
			Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
		}
		/// <summary>
		/// Draws a colored arrow using Gizmos for visualization in the Scene view.
		/// Creates a line with an arrowhead at the end point, with customizable color and arrowhead properties.
		/// Restores the original Gizmos color after drawing.
		/// </summary>
		/// <param name="pos">The starting position of the arrow.</param>
		/// <param name="direction">The direction and length of the arrow.</param>
		/// <param name="color">The color of the arrow.</param>
		/// <param name="arrowHeadLength">The length of the arrow head lines.</param>
		/// <param name="arrowHeadAngle">The angle of the arrow head lines in degrees.</param>
		public static void DrawArrowForGizmos(Vector3 pos, Vector3 direction, UnityEngine.Color color, float arrowHeadLength = .25f, float arrowHeadAngle = 20f)
		{
			UnityEngine.Color orgColor = Gizmos.color;

			Gizmos.color = color;

			DrawArrowForGizmos(pos, direction, arrowHeadLength, arrowHeadAngle);

			Gizmos.color = orgColor;
		}
		/// <summary>
		/// Draws an arrow using Debug.DrawLine for visualization in the Game view.
		/// Creates a line with an arrowhead at the end point, with customizable arrowhead properties.
		/// Uses white color by default.
		/// </summary>
		/// <param name="pos">The starting position of the arrow.</param>
		/// <param name="direction">The direction and length of the arrow.</param>
		/// <param name="arrowHeadLength">The length of the arrow head lines.</param>
		/// <param name="arrowHeadAngle">The angle of the arrow head lines in degrees.</param>
		public static void DrawArrowForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = .25f, float arrowHeadAngle = 20f)
		{
			DrawArrowForDebug(pos, direction, UnityEngine.Color.white, arrowHeadLength, arrowHeadAngle);
		}
		/// <summary>
		/// Draws a colored arrow using Debug.DrawLine for visualization in the Game view.
		/// Creates a line with an arrowhead at the end point, with customizable color and arrowhead properties.
		/// </summary>
		/// <param name="pos">The starting position of the arrow.</param>
		/// <param name="direction">The direction and length of the arrow.</param>
		/// <param name="color">The color of the arrow.</param>
		/// <param name="arrowHeadLength">The length of the arrow head lines.</param>
		/// <param name="arrowHeadAngle">The angle of the arrow head lines in degrees.</param>
		public static void DrawArrowForDebug(Vector3 pos, Vector3 direction, UnityEngine.Color color, float arrowHeadLength = .25f, float arrowHeadAngle = 20f)
		{
			Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f + arrowHeadAngle, 0f) * Vector3.forward;
			Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f - arrowHeadAngle, 0f) * Vector3.forward;

			Debug.DrawRay(pos, direction, color);
			Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
			Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
		}
		/// <summary>
		/// Draws the bounds of an object using Debug.DrawLine for visualization in the Game view.
		/// Creates a wireframe representation of the bounds with different colors for each edge.
		/// </summary>
		/// <param name="bounds">The bounds to draw.</param>
		public static void DrawBoundsForDebug(Bounds bounds)
		{
			// bottom
			Vector3 p1 = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
			Vector3 p2 = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
			Vector3 p3 = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
			Vector3 p4 = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);

			Debug.DrawLine(p1, p2, UnityEngine.Color.blue);
			Debug.DrawLine(p2, p3, UnityEngine.Color.red);
			Debug.DrawLine(p3, p4, UnityEngine.Color.yellow);
			Debug.DrawLine(p4, p1, UnityEngine.Color.magenta);

			// top
			Vector3 p5 = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
			Vector3 p6 = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
			Vector3 p7 = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z);
			Vector3 p8 = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);

			Debug.DrawLine(p5, p6, UnityEngine.Color.blue);
			Debug.DrawLine(p6, p7, UnityEngine.Color.red);
			Debug.DrawLine(p7, p8, UnityEngine.Color.yellow);
			Debug.DrawLine(p8, p5, UnityEngine.Color.magenta);

			// sides
			Debug.DrawLine(p1, p5, UnityEngine.Color.white);
			Debug.DrawLine(p2, p6, UnityEngine.Color.gray);
			Debug.DrawLine(p3, p7, UnityEngine.Color.green);
			Debug.DrawLine(p4, p8, UnityEngine.Color.cyan);
		}
		/// <summary>
		/// Checks if a point is inside a triangle defined by three points.
		/// Uses the sign test method to determine if the point is inside the triangle.
		/// </summary>
		/// <param name="point">The point to check.</param>
		/// <param name="point1">The first point of the triangle.</param>
		/// <param name="point2">The second point of the triangle.</param>
		/// <param name="point3">The third point of the triangle.</param>
		/// <returns>True if the point is inside the triangle, false otherwise.</returns>
		public static bool PointInTriangle(Vector2 point, Vector2 point1, Vector2 point2, Vector2 point3)
		{
			float d1, d2, d3;
			bool isNegative, isPositive;

			d1 = PointSign(point, point1, point2);
			d2 = PointSign(point, point2, point3);
			d3 = PointSign(point, point3, point1);

			isNegative = (d1 < 0) || (d2 < 0) || (d3 < 0);
			isPositive = (d1 > 0) || (d2 > 0) || (d3 > 0);

			return !(isNegative && isPositive);
		}
		/// <summary>
		/// Calculates a point on a circle based on a given angle, radius, and surface plane.
		/// The point is calculated in local space and then transformed to world space using the provided rotation.
		/// </summary>
		/// <param name="center">The center of the circle.</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <param name="angle">The angle in degrees around the circle.</param>
		/// <param name="surface">The plane on which the circle lies (XY, XZ, or YZ).</param>
		/// <param name="rotation">The rotation to apply to the calculated point.</param>
		/// <returns>A point on the circle at the specified angle.</returns>
		public static Vector3 PointFromCircle(Vector3 center, float radius, float angle, WorldSurface surface, Quaternion rotation)
		{
			Vector3 newPosition = Vector3.zero;

			switch (surface)
			{
				case WorldSurface.XY:
					newPosition.x += radius * math.sin(math.radians(angle));
					newPosition.y += radius * math.cos(math.radians(angle));
					break;

				case WorldSurface.XZ:
					newPosition.x += radius * math.sin(math.radians(angle));
					newPosition.z += radius * math.cos(math.radians(angle));
					break;

				case WorldSurface.YZ:
					newPosition.y += radius * math.cos(math.radians(angle));
					newPosition.z += radius * math.sin(math.radians(angle));
					break;
			}

			return center + rotation * newPosition;
		}
		/// <summary>
		/// Calculates a point along a line based on a normalized position.
		/// The position parameter should be between 0 and 1, where 0 is the start point
		/// and 1 is the end point (start + direction * length).
		/// </summary>
		/// <param name="start">The starting point of the line.</param>
		/// <param name="direction">The direction of the line (should be normalized).</param>
		/// <param name="length">The total length of the line.</param>
		/// <param name="position">The normalized position along the line (0 to 1).</param>
		/// <returns>A point along the line at the specified position.</returns>
		public static Vector3 PointFromLine(Vector3 start, Vector3 direction, float length, float position)
		{
			return start + length * position * direction;
		}
		/// <summary>
		/// Converts a point from world space to local space relative to a parent transform.
		/// This transformation includes only position and rotation, not scaling.
		/// </summary>
		/// <param name="worldPoint">The point in world space.</param>
		/// <param name="parentPosition">The position of the parent transform.</param>
		/// <param name="parentRotation">The rotation of the parent transform.</param>
		/// <returns>The point in local space relative to the parent transform.</returns>
		public static Vector3 PointWorldToLocal(Vector3 worldPoint, Vector3 parentPosition, Quaternion parentRotation)
		{
			return Quaternion.Inverse(parentRotation) * (worldPoint - parentPosition);
		}
		/// <summary>
		/// Converts a point from world space to local space relative to a parent transform using float3.
		/// This transformation includes only position and rotation, not scaling.
		/// Uses Unity's mathematics library for better performance.
		/// </summary>
		/// <param name="worldPoint">The point in world space.</param>
		/// <param name="parentPosition">The position of the parent transform.</param>
		/// <param name="parentRotation">The rotation of the parent transform.</param>
		/// <returns>The point in local space relative to the parent transform.</returns>
		public static float3 PointWorldToLocal(float3 worldPoint, float3 parentPosition, quaternion parentRotation)
		{
			return math.mul(math.inverse(parentRotation), worldPoint - parentPosition);
		}
		/// <summary>
		/// Converts a point from world space to local space relative to a parent transform with scaling.
		/// This transformation includes position, rotation, and inverse scaling.
		/// </summary>
		/// <param name="worldPoint">The point in world space.</param>
		/// <param name="parentPosition">The position of the parent transform.</param>
		/// <param name="parentRotation">The rotation of the parent transform.</param>
		/// <param name="parentScale">The scale of the parent transform.</param>
		/// <returns>The point in local space relative to the parent transform.</returns>
		public static Vector3 PointWorldToLocal(Vector3 worldPoint, Vector3 parentPosition, Quaternion parentRotation, Vector3 parentScale)
		{
			return Vector3.Scale(Quaternion.Inverse(parentRotation) * (worldPoint - parentPosition), new Vector3(1f/parentScale.x, 1f/parentScale.y, 1f/parentScale.z));
		}
		/// <summary>
		/// Converts a point from world space to local space relative to a parent transform with scaling using float3.
		/// This transformation includes position, rotation, and inverse scaling.
		/// Uses Unity's mathematics library for better performance.
		/// </summary>
		/// <param name="worldPoint">The point in world space.</param>
		/// <param name="parentPosition">The position of the parent transform.</param>
		/// <param name="parentRotation">The rotation of the parent transform.</param>
		/// <param name="parentScale">The scale of the parent transform.</param>
		/// <returns>The point in local space relative to the parent transform.</returns>
		public static float3 PointWorldToLocal(float3 worldPoint, float3 parentPosition, quaternion parentRotation, float3 parentScale)
		{
			return math.mul(math.inverse(parentRotation), worldPoint - parentPosition) / parentScale;
		}
		/// <summary>
		/// Converts a point from local space to world space relative to a parent transform with scaling.
		/// This transformation includes position, rotation, and scaling.
		/// </summary>
		/// <param name="localPoint">The point in local space.</param>
		/// <param name="parentPosition">The position of the parent transform.</param>
		/// <param name="parentRotation">The rotation of the parent transform.</param>
		/// <param name="parentScale">The scale of the parent transform.</param>
		/// <returns>The point in world space.</returns>
		public static Vector3 PointLocalToWorld(Vector3 localPoint, Vector3 parentPosition, Quaternion parentRotation, Vector3 parentScale)
		{
			return parentRotation * Vector3.Scale(localPoint, parentScale) + parentPosition;
		}
		/// <summary>
		/// Converts a point from local space to world space relative to a parent transform with scaling using float3.
		/// This transformation includes position, rotation, and scaling.
		/// Uses Unity's mathematics library for better performance.
		/// </summary>
		/// <param name="localPoint">The point in local space.</param>
		/// <param name="parentPosition">The position of the parent transform.</param>
		/// <param name="parentRotation">The rotation of the parent transform.</param>
		/// <param name="parentScale">The scale of the parent transform.</param>
		/// <returns>The point in world space.</returns>
		public static float3 PointLocalToWorld(float3 localPoint, float3 parentPosition, quaternion parentRotation, float3 parentScale)
		{
			return math.mul(parentRotation, localPoint * parentScale) + parentPosition;
		}
		/// <summary>
		/// Returns the absolute values of the components of a vector.
		/// Creates a new Vector3 with the absolute values of x, y, and z components.
		/// </summary>
		/// <param name="vector">The vector to get the absolute values of.</param>
		/// <returns>A vector with the absolute values of the components of the input vector.</returns>
		public static Vector3 Abs(Vector3 vector)
		{
			return new Vector3(vector.x, vector.y, vector.z);
		}
		/// <summary>
		/// Returns the absolute values of the components of a vector using float3.
		/// Creates a new float3 with the absolute values of x, y, and z components.
		/// Uses Unity's mathematics library for better performance.
		/// </summary>
		/// <param name="vector">The vector to get the absolute values of.</param>
		/// <returns>A vector with the absolute values of the components of the input vector.</returns>
		public static float3 Abs(float3 vector)
		{
			return new float3(vector.x, vector.y, vector.z);
		}
		/// <summary>
		/// Returns a Vector3 with the rounded values of the components of the input vector.
		/// Uses math.round to round each component to the nearest integer value.
		/// Useful for snapping vectors to grid positions or removing decimal precision.
		/// </summary>
		/// <param name="vector">The Vector3 to round.</param>
		/// <returns>A new Vector3 with each component rounded to the nearest integer.</returns>
		public static Vector3 Round(Vector3 vector)
		{
			return new Vector3(math.round(vector.x), math.round(vector.y), math.round(vector.z));
		}

		/// <summary>
		/// Returns a Vector3 with the components rounded to a specified number of decimal places.
		/// Provides precise control over the rounding precision for each component.
		/// Useful for reducing floating-point precision while maintaining some decimal accuracy.
		/// </summary>
		/// <param name="vector">The Vector3 to round.</param>
		/// <param name="decimals">The number of decimal places to preserve in the rounded result.</param>
		/// <returns>A new Vector3 with each component rounded to the specified number of decimal places.</returns>
		public static Vector3 Round(Vector3 vector, uint decimals)
		{
			return new Vector3(Round(vector.x, decimals), Round(vector.y, decimals), Round(vector.z, decimals));
		}

		/// <summary>
		/// Returns a Vector2 with the rounded values of the components of the input vector.
		/// Uses math.round to round each component to the nearest integer value.
		/// Useful for 2D grid snapping or UI element positioning.
		/// </summary>
		/// <param name="vector">The Vector2 to round.</param>
		/// <returns>A new Vector2 with each component rounded to the nearest integer.</returns>
		public static Vector2 Round(Vector2 vector)
		{
			return new Vector2(math.round(vector.x), math.round(vector.y));
		}

		/// <summary>
		/// Returns a Vector2 with the components rounded to a specified number of decimal places.
		/// Provides precise control over the rounding precision for each component.
		/// Useful for 2D applications requiring controlled decimal precision.
		/// </summary>
		/// <param name="vector">The Vector2 to round.</param>
		/// <param name="decimals">The number of decimal places to preserve in the rounded result.</param>
		/// <returns>A new Vector2 with each component rounded to the specified number of decimal places.</returns>
		public static Vector2 Round(Vector2 vector, uint decimals)
		{
			return new Vector2(Round(vector.x, decimals), Round(vector.y, decimals));
		}

		/// <summary>
		/// Converts a Vector3 to a Vector3Int by rounding each component to the nearest integer.
		/// Useful for converting world positions to grid coordinates or pixel positions.
		/// This method handles the conversion from floating-point to integer space in one operation.
		/// </summary>
		/// <param name="vector">The Vector3 to convert to integer coordinates.</param>
		/// <returns>A Vector3Int with each component rounded to the nearest integer value.</returns>
		public static Vector3Int RoundToInt(Vector3 vector)
		{
			return new Vector3Int((int)math.round(vector.x), (int)math.round(vector.y), (int)math.round(vector.z));
		}

		/// <summary>
		/// Converts a Vector2 to a Vector2Int by rounding each component to the nearest integer.
		/// Particularly useful for 2D applications like UI positioning, tilemap coordinates, or pixel art.
		/// Provides a clean conversion from floating-point to integer space.
		/// </summary>
		/// <param name="vector">The Vector2 to convert to integer coordinates.</param>
		/// <returns>A Vector2Int with each component rounded to the nearest integer value.</returns>
		public static Vector2Int RoundToInt(Vector2 vector)
		{
			return new Vector2Int((int)math.round(vector.x), (int)math.round(vector.y));
		}

		/// <summary>
		/// Rounds a floating-point number to a specified number of decimal places.
		/// Uses a multiplier approach to achieve precise decimal rounding without string conversions.
		/// This method handles edge cases better than simple truncation and is more efficient than string-based approaches.
		/// </summary>
		/// <param name="number">The floating-point number to round.</param>
		/// <param name="decimals">The number of decimal places to preserve in the result.</param>
		/// <returns>The input number rounded to the specified number of decimal places.</returns>
		public static float Round(float number, uint decimals)
		{
			float multiplier = math.pow(10f, decimals);

			return math.round(number * multiplier) / multiplier;
		}
		/// <summary>
		/// Calculates the normalized direction vector from origin to destination.
		/// This method returns a unit vector (magnitude of 1) pointing from the origin towards the destination.
		/// Useful for determining the direction of movement or for ray casting operations.
		/// </summary>
		/// <param name="origin">The starting point in 3D space.</param>
		/// <param name="destination">The end point in 3D space.</param>
		/// <returns>A normalized direction vector pointing from origin to destination.</returns>
		public static Vector3 Direction(Vector3 origin, Vector3 destination)
		{
			return (destination - origin).normalized;
		}

		/// <summary>
		/// Calculates the unnormalized direction vector from origin to destination.
		/// Returns the raw vector difference between destination and origin without normalizing.
		/// Useful when both direction and magnitude (distance) information is needed.
		/// </summary>
		/// <param name="origin">The starting point in 3D space.</param>
		/// <param name="destination">The end point in 3D space.</param>
		/// <returns>An unnormalized vector pointing from origin to destination.</returns>
		public static Vector3 DirectionUnNormalized(Vector3 origin, Vector3 destination)
		{
			return destination - origin;
		}

		/// <summary>
		/// Calculates the right vector based on forward and up vectors.
		/// Uses quaternion rotation to find the perpendicular vector to the right of the forward direction.
		/// Essential for establishing a complete coordinate system or for camera orientation.
		/// </summary>
		/// <param name="forward">The forward direction vector.</param>
		/// <param name="up">The up direction vector that defines the rotation plane.</param>
		/// <returns>The right vector perpendicular to both forward and up vectors.</returns>
		public static Vector3 DirectionRight(Vector3 forward, Vector3 up)
		{
			return Quaternion.AngleAxis(90f, up) * forward;
		}

		/// <summary>
		/// Calculates the normalized direction vector from origin to destination using float3.
		/// This is the Unity Mathematics equivalent of the Vector3 Direction method.
		/// Optimized for performance in DOTS (Data-Oriented Technology Stack) contexts.
		/// </summary>
		/// <param name="origin">The starting point as float3.</param>
		/// <param name="destination">The end point as float3.</param>
		/// <returns>A normalized direction vector pointing from origin to destination.</returns>
		public static float3 Direction(float3 origin, float3 destination)
		{
			return math.normalize(destination - origin);
		}

		/// <summary>
		/// Calculates the unnormalized direction vector from origin to destination using float3.
		/// This is the Unity Mathematics equivalent of the Vector3 DirectionUnNormalized method.
		/// Preserves both direction and magnitude information in DOTS-compatible format.
		/// </summary>
		/// <param name="origin">The starting point as float3.</param>
		/// <param name="destination">The end point as float3.</param>
		/// <returns>An unnormalized vector pointing from origin to destination.</returns>
		public static float3 DirectionUnNormalized(float3 origin, float3 destination)
		{
			return destination - origin;
		}

		/// <summary>
		/// Calculates the right vector based on forward and up vectors using float3.
		/// Uses quaternion rotation in Unity Mathematics format to find the perpendicular vector.
		/// Provides a DOTS-compatible method for establishing coordinate systems.
		/// </summary>
		/// <param name="forward">The forward direction as float3.</param>
		/// <param name="up">The up direction as float3 that defines the rotation plane.</param>
		/// <returns>The right vector perpendicular to both forward and up vectors.</returns>
		public static float3 DirectionRight(float3 forward, float3 up)
		{
			return math.mul(quaternion.AxisAngle(up, math.PI * .5f), forward);
		}

		/// <summary>
		/// Calculates the normalized direction vector from origin to destination in 2D space.
		/// Returns a unit vector (magnitude of 1) pointing from the origin towards the destination.
		/// Useful for 2D games, UI positioning, or any 2D directional calculations.
		/// </summary>
		/// <param name="origin">The starting point in 2D space.</param>
		/// <param name="destination">The end point in 2D space.</param>
		/// <returns>A normalized direction vector pointing from origin to destination.</returns>
		public static Vector2 Direction(Vector2 origin, Vector2 destination)
		{
			return (destination - origin).normalized;
		}

		/// <summary>
		/// Calculates the unnormalized direction vector from origin to destination in 2D space.
		/// Returns the raw vector difference between destination and origin without normalizing.
		/// Preserves both direction and magnitude information for 2D calculations.
		/// </summary>
		/// <param name="origin">The starting point in 2D space.</param>
		/// <param name="destination">The end point in 2D space.</param>
		/// <returns>An unnormalized vector pointing from origin to destination.</returns>
		public static Vector2 DirectionUnNormalized(Vector2 origin, Vector2 destination)
		{
			return destination - origin;
		}

		/// <summary>
		/// Calculates the normalized direction vector from origin to destination using float2.
		/// This is the Unity Mathematics equivalent of the Vector2 Direction method.
		/// Optimized for performance in DOTS contexts for 2D operations.
		/// </summary>
		/// <param name="origin">The starting point as float2.</param>
		/// <param name="destination">The end point as float2.</param>
		/// <returns>A normalized direction vector pointing from origin to destination.</returns>
		public static float2 Direction(float2 origin, float2 destination)
		{
			return math.normalize(destination - origin);
		}

		/// <summary>
		/// Calculates the unnormalized direction vector from origin to destination using float2.
		/// This is the Unity Mathematics equivalent of the Vector2 DirectionUnNormalized method.
		/// Preserves both direction and magnitude information in DOTS-compatible format for 2D operations.
		/// </summary>
		/// <param name="origin">The starting point as float2.</param>
		/// <param name="destination">The end point as float2.</param>
		/// <returns>An unnormalized vector pointing from origin to destination.</returns>
		public static float2 DirectionUnNormalized(float2 origin, float2 destination)
		{
			return destination - origin;
		}

		/// <summary>
		/// Determines the relative position of a point compared to a reference point and direction.
		/// Calculates whether a point is to the left, right, or directly in line with a reference point's forward direction.
		/// Useful for AI decision making, spatial reasoning, or user interface positioning.
		/// </summary>
		/// <param name="point">The point to evaluate.</param>
		/// <param name="comparingPoint">The reference point for comparison.</param>
		/// <param name="comparingForward">The forward direction of the reference point.</param>
		/// <param name="comparingUp">The up direction used to determine the sign of the angle.</param>
		/// <returns>WorldSide.Right if the point is to the right, WorldSide.Left if to the left, or WorldSide.Center if directly in line.</returns>
		public static WorldSide GetPointSideCompared(Vector3 point, Vector3 comparingPoint, Vector3 comparingForward, Vector3 comparingUp)
		{
			Vector3 pointForward = Direction(comparingPoint, point);
			float compareAngle = Vector3.SignedAngle(comparingForward, pointForward, comparingUp);

			if (compareAngle > 0f)
				return WorldSide.Right;
			else if (compareAngle < 0f)
				return WorldSide.Left;
			else
				return WorldSide.Center;
		}

		/// <summary>
		/// Calculates the angle around an axis between a direction and a reference forward direction.
		/// Measures the angle in degrees by projecting the direction onto the plane perpendicular to the axis.
		/// Essential for rotational calculations, camera controls, or object orientation in 3D space.
		/// </summary>
		/// <param name="direction">The direction vector to measure.</param>
		/// <param name="axis">The axis to measure around (should be normalized).</param>
		/// <param name="forward">The reference forward direction that represents 0 degrees.</param>
		/// <returns>The angle in degrees, measured clockwise when looking along the axis direction.</returns>
		public static float AngleAroundAxis(Vector3 direction, Vector3 axis, Vector3 forward)
		{
			Vector3 right = Vector3.Cross(axis, forward).normalized;

			forward = Vector3.Cross(right, axis).normalized;

			return math.atan2(Vector3.Dot(direction, right), Vector3.Dot(direction, forward)) * Mathf.Rad2Deg;
		}

		/// <summary>
		/// Calculates the angle around an axis between a direction and a reference forward direction using float3.
		/// This is the Unity Mathematics equivalent of the Vector3 AngleAroundAxis method.
		/// Provides a DOTS-compatible solution for measuring angles in 3D space.
		/// </summary>
		/// <param name="direction">The direction vector to measure as float3.</param>
		/// <param name="axis">The axis to measure around as float3 (should be normalized).</param>
		/// <param name="forward">The reference forward direction as float3 that represents 0 degrees.</param>
		/// <returns>The angle in degrees, measured clockwise when looking along the axis direction.</returns>
		public static float AngleAroundAxis(float3 direction, float3 axis, float3 forward)
		{
			float3 right = math.normalizesafe(math.cross(axis, forward), Float3Right);

			forward = math.normalizesafe(math.cross(right, axis), Float3Forward);

			return math.atan2(math.dot(direction, right), math.dot(direction, forward)) * Mathf.Rad2Deg;
		}

		/// <summary>
		/// Calculates the interpolation parameter that would result in the value t when linearly interpolating from a to b.
		/// Projects point t onto the line from a to b and returns the normalized projection parameter, clamped between 0 and 1.
		/// Useful for determining how far along a path a point is, or for parametric calculations on lines.
		/// </summary>
		/// <param name="a">The start point of the interpolation.</param>
		/// <param name="b">The end point of the interpolation.</param>
		/// <param name="t">The point to find the interpolation parameter for.</param>
		/// <returns>The interpolation parameter clamped between 0 and 1.</returns>
		public static float InverseLerp(Vector3 a, Vector3 b, Vector3 t)
		{
			return Clamp01(InverseLerpUnclamped(a, b, t));
		}		

		/// <summary>
		/// Calculates the interpolation parameter that would result in the value t when linearly interpolating from a to b.
		/// Projects point t onto the line from a to b and returns the raw projection parameter without clamping.
		/// Provides the exact relative position of a point along a line, even if the point is before a or after b.
		/// </summary>
		/// <param name="a">The start point of the interpolation.</param>
		/// <param name="b">The end point of the interpolation.</param>
		/// <param name="t">The point to find the interpolation parameter for.</param>
		/// <returns>The interpolation parameter (can be outside the 0-1 range).</returns>
		public static float InverseLerpUnclamped(Vector3 a, Vector3 b, Vector3 t)
		{
			Vector3 AB = b - a;
			Vector3 AT = t - a;

			return Vector3.Dot(AT, AB) / Vector3.Dot(AB, AB);
		}

		/// <summary>
		/// Calculates the interpolation parameter that would result in the value t when linearly interpolating from a to b.
		/// Projects point t onto the line from a to b and returns the normalized projection parameter, clamped between 0 and 1.
		/// This is the Unity Mathematics equivalent of the Vector3 InverseLerp method.
		/// </summary>
		/// <param name="a">The start point of the interpolation as float3.</param>
		/// <param name="b">The end point of the interpolation as float3.</param>
		/// <param name="t">The point to find the interpolation parameter for as float3.</param>
		/// <returns>The interpolation parameter clamped between 0 and 1.</returns>
		public static float InverseLerp(float3 a, float3 b, float3 t)
		{
			return Clamp01(InverseLerpUnclamped(a, b, t));
		}

		/// <summary>
		/// Calculates the interpolation parameter that would result in the value t when linearly interpolating from a to b.
		/// Projects point t onto the line from a to b and returns the raw projection parameter without clamping.
		/// This is the Unity Mathematics equivalent of the Vector3 InverseLerpUnclamped method.
		/// </summary>
		/// <param name="a">The start point of the interpolation as float3.</param>
		/// <param name="b">The end point of the interpolation as float3.</param>
		/// <param name="t">The point to find the interpolation parameter for as float3.</param>
		/// <returns>The interpolation parameter (can be outside the 0-1 range).</returns>
		public static float InverseLerpUnclamped(float3 a, float3 b, float3 t)
		{
			float3 AB = b - a;
			float3 AT = t - a;

			return math.dot(AT, AB) / math.dot(AB, AB);
		}

		/// <summary>
		/// Calculates the interpolation parameter that would result in the value t when linearly interpolating from a to b.
		/// Returns the normalized position of t between a and b, clamped between 0 and 1.
		/// A fundamental utility for many interpolation and mapping operations with scalar values.
		/// </summary>
		/// <param name="a">The start value of the interpolation.</param>
		/// <param name="b">The end value of the interpolation.</param>
		/// <param name="t">The value to find the interpolation parameter for.</param>
		/// <returns>The interpolation parameter clamped between 0 and 1.</returns>
		public static float InverseLerp(float a, float b, float t)
		{
			return Clamp01(InverseLerpUnclamped(a, b, t));
		}

		/// <summary>
		/// Calculates the interpolation parameter that would result in the value t when linearly interpolating from a to b.
		/// Returns the raw position of t between a and b without clamping.
		/// Useful for extrapolation and for determining relative positions beyond the a-b range.
		/// </summary>
		/// <param name="a">The start value of the interpolation.</param>
		/// <param name="b">The end value of the interpolation.</param>
		/// <param name="t">The value to find the interpolation parameter for.</param>
		/// <returns>The interpolation parameter (can be outside the 0-1 range).</returns>
		public static float InverseLerpUnclamped(float a, float b, float t)
		{
			return (t - a) / (b - a);
		}

		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// Performs linear interpolation without clamping the t parameter, allowing for extrapolation.
		/// A fundamental utility for animation, transitions, and procedural generation.
		/// </summary>
		/// <param name="a">The start value of the interpolation.</param>
		/// <param name="b">The end value of the interpolation.</param>
		/// <param name="t">The interpolation parameter (can be outside the 0-1 range).</param>
		/// <returns>The value resulting from the interpolation or extrapolation.</returns>
		public static float LerpUnclamped(float a, float b, float t)
		{
			return a + (b - a) * t;
		}

		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// Performs component-wise linear interpolation on float3 vectors without clamping the t parameter.
		/// Useful for 3D position interpolation, color transitions, or any three-component value blending.
		/// </summary>
		/// <param name="a">The start point of the interpolation as float3.</param>
		/// <param name="b">The end point of the interpolation as float3.</param>
		/// <param name="t">The interpolation parameter (can be outside the 0-1 range).</param>
		/// <returns>The float3 value resulting from the interpolation or extrapolation.</returns>
		public static float3 LerpUnclamped(float3 a, float3 b, float t)
		{
			return new float3(LerpUnclamped(a.x, b.x, t), LerpUnclamped(a.y, b.y, t), LerpUnclamped(a.z, b.z, t));
		}

		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// Performs component-wise linear interpolation on Vector3 values without clamping the t parameter.
		/// Standard utility for Unity Vector3 interpolation with support for extrapolation.
		/// </summary>
		/// <param name="a">The start point of the interpolation as Vector3.</param>
		/// <param name="b">The end point of the interpolation as Vector3.</param>
		/// <param name="t">The interpolation parameter (can be outside the 0-1 range).</param>
		/// <returns>The Vector3 value resulting from the interpolation or extrapolation.</returns>
		public static Vector3 LerpUnclamped(Vector3 a, Vector3 b, float t)
		{
			return new float3(LerpUnclamped(a.x, b.x, t), LerpUnclamped(a.y, b.y, t), LerpUnclamped(a.z, b.z, t));
		}

		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// Performs component-wise linear interpolation on float2 vectors without clamping the t parameter.
		/// Useful for 2D position interpolation, UV coordinates, or any two-component value blending.
		/// </summary>
		/// <param name="a">The start point of the interpolation as float2.</param>
		/// <param name="b">The end point of the interpolation as float2.</param>
		/// <param name="t">The interpolation parameter (can be outside the 0-1 range).</param>
		/// <returns>The float2 value resulting from the interpolation or extrapolation.</returns>
		public static float2 LerpUnclamped(float2 a, float2 b, float t)
		{
			return new float2(LerpUnclamped(a.x, b.x, t), LerpUnclamped(a.y, b.y, t));
		}

		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// Performs component-wise linear interpolation on Vector2 values without clamping the t parameter.
		/// Standard utility for Unity Vector2 interpolation with support for extrapolation.
		/// </summary>
		/// <param name="a">The start point of the interpolation as Vector2.</param>
		/// <param name="b">The end point of the interpolation as Vector2.</param>
		/// <param name="t">The interpolation parameter (can be outside the 0-1 range).</param>
		/// <returns>The Vector2 value resulting from the interpolation or extrapolation.</returns>
		public static Vector2 LerpUnclamped(Vector2 a, Vector2 b, float t)
		{
			return new float2(LerpUnclamped(a.x, b.x, t), LerpUnclamped(a.y, b.y, t));
		}

		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// Performs normalized linear interpolation (nlerp) between quaternions without clamping the t parameter.
		/// Provides a more efficient alternative to slerp for many rotation interpolation cases.
		/// </summary>
		/// <param name="a">The start rotation as quaternion.</param>
		/// <param name="b">The end rotation as quaternion.</param>
		/// <param name="t">The interpolation parameter (can be outside the 0-1 range).</param>
		/// <returns>The quaternion resulting from the normalized linear interpolation.</returns>
		public static quaternion LerpUnclamped(quaternion a, quaternion b, float t)
		{
			return math.nlerp(a, b, t);
		}

		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// Performs linear interpolation with the t parameter clamped between 0 and 1.
		/// A fundamental utility for animation, transitions, and value blending with guaranteed in-range results.
		/// </summary>
		/// <param name="a">The start value of the interpolation.</param>
		/// <param name="b">The end value of the interpolation.</param>
		/// <param name="t">The interpolation parameter (will be clamped between 0 and 1).</param>
		/// <returns>The value resulting from the interpolation.</returns>
		public static float Lerp(float a, float b, float t)
		{
			return LerpUnclamped(a, b, Clamp01(t));
		}

		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// Performs component-wise linear interpolation on Vector3 values with the t parameter clamped between 0 and 1.
		/// Standard utility for Unity Vector3 interpolation with guaranteed in-range results.
		/// </summary>
		/// <param name="a">The start point of the interpolation as Vector3.</param>
		/// <param name="b">The end point of the interpolation as Vector3.</param>
		/// <param name="t">The interpolation parameter (will be clamped between 0 and 1).</param>
		/// <returns>The Vector3 value resulting from the interpolation.</returns>
		public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
		{
			return LerpUnclamped(a, b, Clamp01(t));
		}

		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// Performs component-wise linear interpolation on float3 vectors with the t parameter clamped between 0 and 1.
		/// Useful for 3D position interpolation, color transitions, or any three-component value blending with guaranteed in-range results.
		/// </summary>
		/// <param name="a">The start point of the interpolation as float3.</param>
		/// <param name="b">The end point of the interpolation as float3.</param>
		/// <param name="t">The interpolation parameter (will be clamped between 0 and 1).</param>
		/// <returns>The float3 value resulting from the interpolation.</returns>
		public static float3 Lerp(float3 a, float3 b, float t)
		{
			return LerpUnclamped(a, b, Clamp01(t));
		}

		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// Performs component-wise linear interpolation on Vector2 values with the t parameter clamped between 0 and 1.
		/// Standard utility for Unity Vector2 interpolation with guaranteed in-range results.
		/// </summary>
		/// <param name="a">The start point of the interpolation as Vector2.</param>
		/// <param name="b">The end point of the interpolation as Vector2.</param>
		/// <param name="t">The interpolation parameter (will be clamped between 0 and 1).</param>
		/// <returns>The Vector2 value resulting from the interpolation.</returns>
		public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
		{
			return LerpUnclamped(a, b, Clamp01(t));
		}

		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// Performs component-wise linear interpolation on float2 vectors with the t parameter clamped between 0 and 1.
		/// Useful for 2D position interpolation, UV coordinates, or any two-component value blending with guaranteed in-range results.
		/// </summary>
		/// <param name="a">The start point of the interpolation as float2.</param>
		/// <param name="b">The end point of the interpolation as float2.</param>
		/// <param name="t">The interpolation parameter (will be clamped between 0 and 1).</param>
		/// <returns>The float2 value resulting from the interpolation.</returns>
		public static float2 Lerp(float2 a, float2 b, float t)
		{
			return LerpUnclamped(a, b, Clamp01(t));
		}

		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// Performs normalized linear interpolation (nlerp) between quaternions with the t parameter clamped between 0 and 1.
		/// Provides a more efficient alternative to slerp for many rotation interpolation cases with guaranteed in-range results.
		/// </summary>
		/// <param name="a">The start rotation as quaternion.</param>
		/// <param name="b">The end rotation as quaternion.</param>
		/// <param name="t">The interpolation parameter (will be clamped between 0 and 1).</param>
		/// <returns>The quaternion resulting from the normalized linear interpolation.</returns>
		public static quaternion Lerp(quaternion a, quaternion b, float t)
		{
			return LerpUnclamped(a, b, Clamp01(t));
		}

		/// <summary>
		/// Moves a color towards a target color by a maximum delta.
		/// Gradually changes each component (RGBA) of the color towards the target by at most maxDelta per call.
		/// Useful for smooth color transitions, UI effects, or gradual visual feedback.
		/// </summary>
		/// <param name="a">The current color.</param>
		/// <param name="b">The target color to move towards.</param>
		/// <param name="maxDelta">The maximum change that should be applied to each component per call.</param>
		/// <returns>The new color resulting from the movement, closer to the target by at most maxDelta per component.</returns>
		public static UnityEngine.Color MoveTowards(UnityEngine.Color a, UnityEngine.Color b, float maxDelta)
		{
			return new UnityEngine.Color()
			{
				r = Mathf.MoveTowards(a.r, b.r, maxDelta),
				g = Mathf.MoveTowards(a.g, b.g, maxDelta),
				b = Mathf.MoveTowards(a.b, b.b, maxDelta),
				a = Mathf.MoveTowards(a.a, b.a, maxDelta)
			};
		}
		
		/// <summary>
		/// Moves a color towards a target color by component-specific maximum deltas.
		/// Allows for different rates of change for each color component (RGBA), providing fine-grained control over transitions.
		/// Particularly useful for effects where different color channels need to change at different rates.
		/// </summary>
		/// <param name="a">The current color.</param>
		/// <param name="b">The target color to move towards.</param>
		/// <param name="maxDelta">The maximum change that should be applied to each component, specified as a color where each component represents the max delta for the corresponding component in a.</param>
		/// <returns>The new color resulting from the movement, with each component closer to the target by at most its corresponding maxDelta.</returns>
		public static UnityEngine.Color MoveTowards(UnityEngine.Color a, UnityEngine.Color b, UnityEngine.Color maxDelta)
		{
			return new UnityEngine.Color()
			{
				r = Mathf.MoveTowards(a.r, b.r, maxDelta.r),
				g = Mathf.MoveTowards(a.g, b.g, maxDelta.g),
				b = Mathf.MoveTowards(a.b, b.b, maxDelta.b),
				a = Mathf.MoveTowards(a.a, b.a, maxDelta.a)
			};
		}
		/// <summary>
		/// Calculates the Euclidean distance between a series of Transform points.
		/// Measures the total path length by summing the distances between consecutive Transform positions.
		/// Useful for calculating path lengths, trajectory distances, or spatial relationships between multiple objects.
		/// </summary>
		/// <param name="transforms">The series of Transform points to measure distance between.</param>
		/// <returns>The total distance between all consecutive points in the sequence.</returns>
		public static float Distance(params Transform[] transforms)
		{
			float distance = 0f;

			for (int i = 0; i < transforms.Length - 1; i++)
				distance += Distance(transforms[i], transforms[i + 1]);

			return distance;
		}
		/// <summary>
		/// Calculates the Euclidean distance between two Transform points.
		/// Measures the straight-line distance between the positions of two Transform objects in 3D space.
		/// Equivalent to Vector3.Distance(a.position, b.position).
		/// </summary>
		/// <param name="a">The first Transform point.</param>
		/// <param name="b">The second Transform point.</param>
		/// <returns>The distance between the positions of the two transforms.</returns>
		public static float Distance(Transform a, Transform b)
		{
			return Utility.Distance(a.position, b.position);
		}
		/// <summary>
		/// Calculates the Euclidean distance between a series of Vector3 points.
		/// Measures the total path length by summing the distances between consecutive Vector3 positions.
		/// Useful for calculating path lengths, trajectory distances, or spatial relationships in 3D space.
		/// </summary>
		/// <param name="vectors">The series of Vector3 points to measure distance between.</param>
		/// <returns>The total distance between all consecutive points in the sequence.</returns>
		public static float Distance(params Vector3[] vectors)
		{
			float distance = 0f;

			for (int i = 0; i < vectors.Length - 1; i++)
				distance += Distance(vectors[i], vectors[i + 1]);

			return distance;
		}
		/// <summary>
		/// Calculates the Euclidean distance between a series of float3 points.
		/// Measures the total path length by summing the distances between consecutive float3 positions.
		/// Provides a Unity.Mathematics alternative to the Vector3-based distance calculation.
		/// </summary>
		/// <param name="vectors">The series of float3 points to measure distance between.</param>
		/// <returns>The total distance between all consecutive points in the sequence.</returns>
		public static float Distance(params float3[] vectors)
		{
			float distance = 0f;

			for (int i = 0; i < vectors.Length - 1; i++)
				distance += Distance(vectors[i], vectors[i + 1]);

			return distance;
		}
		/// <summary>
		/// Calculates the Euclidean distance between two Vector3 points.
		/// Measures the straight-line distance between two points in 3D space.
		/// Equivalent to Vector3.Distance(a, b) or (a - b).magnitude.
		/// </summary>
		/// <param name="a">The first Vector3 point.</param>
		/// <param name="b">The second Vector3 point.</param>
		/// <returns>The distance between the two points.</returns>
		public static float Distance(Vector3 a, Vector3 b)
		{
			return (a - b).magnitude;
		}		
		/// <summary>
		/// Calculates the Euclidean distance between two float3 points.
		/// Measures the straight-line distance between two points in 3D space using Unity.Mathematics.
		/// Equivalent to math.length(a - b).
		/// </summary>
		/// <param name="a">The first float3 point.</param>
		/// <param name="b">The second float3 point.</param>
		/// <returns>The distance between the two points.</returns>
		public static float Distance(float3 a, float3 b)
		{
			return math.length(a - b);
		}
		/// <summary>
		/// Calculates the Euclidean distance between a series of Vector2 points.
		/// Measures the total path length by summing the distances between consecutive Vector2 positions.
		/// Useful for calculating path lengths, trajectory distances, or spatial relationships in 2D space.
		/// </summary>
		/// <param name="vectors">The series of Vector2 points to measure distance between.</param>
		/// <returns>The total distance between all consecutive points in the sequence.</returns>
		public static float Distance(params Vector2[] vectors)
		{
			float distance = 0f;

			for (int i = 0; i < vectors.Length - 1; i++)
				distance += Distance(vectors[i], vectors[i + 1]);

			return distance;
		}
		/// <summary>
		/// Calculates the Euclidean distance between a series of float2 points.
		/// Measures the total path length by summing the distances between consecutive float2 positions.
		/// Provides a Unity.Mathematics alternative to the Vector2-based distance calculation.
		/// </summary>
		/// <param name="vectors">The series of float2 points to measure distance between.</param>
		/// <returns>The total distance between all consecutive points in the sequence.</returns>
		public static float Distance(params float2[] vectors)
		{
			float distance = 0f;

			for (int i = 0; i < vectors.Length - 1; i++)
				distance += Distance(vectors[i], vectors[i + 1]);

			return distance;
		}
		/// <summary>
		/// Calculates the Euclidean distance between two Vector2 points.
		/// Measures the straight-line distance between two points in 2D space.
		/// Equivalent to Vector2.Distance(a, b) or (a - b).magnitude.
		/// </summary>
		/// <param name="a">The first Vector2 point.</param>
		/// <param name="b">The second Vector2 point.</param>
		/// <returns>The distance between the two points.</returns>
		public static float Distance(Vector2 a, Vector2 b)
		{
			return (a - b).magnitude;
		}
		/// <summary>
		/// Calculates the Euclidean distance between two float2 points.
		/// Measures the straight-line distance between two points in 2D space using Unity.Mathematics.
		/// Equivalent to math.length(a - b).
		/// </summary>
		/// <param name="a">The first float2 point.</param>
		/// <param name="b">The second float2 point.</param>
		/// <returns>The distance between the two points.</returns>
		public static float Distance(float2 a, float2 b)
		{
			return math.length(a - b);
		}
		/// <summary>
		/// Calculates the squared Euclidean distance between two Vector3 points.
		/// Provides a more efficient alternative to Distance() when only comparing distances,
		/// as it avoids the costly square root operation. Useful for performance-critical code.
		/// </summary>
		/// <param name="a">The first Vector3 point.</param>
		/// <param name="b">The second Vector3 point.</param>
		/// <returns>The squared distance between the two points.</returns>
		public static float DistanceSqr(Vector3 a, Vector3 b)
		{
			return (a - b).sqrMagnitude;
		}
		/// <summary>
		/// Calculates the squared Euclidean distance between two float3 points.
		/// Provides a more efficient alternative to Distance() when only comparing distances,
		/// as it avoids the costly square root operation. Uses Unity.Mathematics for calculation.
		/// </summary>
		/// <param name="a">The first float3 point.</param>
		/// <param name="b">The second float3 point.</param>
		/// <returns>The squared distance between the two points.</returns>
		public static float DistanceSqr(float3 a, float3 b)
		{
			return math.lengthsq(a - b);
		}
		/// <summary>
		/// Calculates the absolute difference between two float values.
		/// Returns the positive distance between two numbers on the number line.
		/// Equivalent to Math.Abs(a - b) but implemented using math.max/min for efficiency.
		/// </summary>
		/// <param name="a">The first float value.</param>
		/// <param name="b">The second float value.</param>
		/// <returns>The absolute difference between the two values.</returns>
		public static float Distance(float a, float b)
		{
			return math.max(a, b) - math.min(a, b);
		}
		/// <summary>
		/// Calculates the instantaneous velocity of a value between two states over time.
		/// Computes the rate of change by dividing the difference between the previous and current values by the time elapsed.
		/// Useful for physics calculations, animations, and tracking object movement rates.
		/// </summary>
		/// <param name="current">The current float value.</param>
		/// <param name="last">The previous float value.</param>
		/// <param name="deltaTime">The time elapsed between the two states in seconds.</param>
		/// <returns>The velocity (rate of change) of the value per second.</returns>
		public static float Velocity(float current, float last, float deltaTime)
		{
			return (last - current) / deltaTime;
		}
		/// <summary>
		/// Calculates the instantaneous velocity vector between two Vector3 states over time.
		/// Computes the rate of change for each component by dividing the difference between the previous and current positions by the time elapsed.
		/// Essential for physics simulations, character controllers, and tracking object movement in 3D space.
		/// </summary>
		/// <param name="current">The current Vector3 position.</param>
		/// <param name="last">The previous Vector3 position.</param>
		/// <param name="deltaTime">The time elapsed between the two states in seconds.</param>
		/// <returns>The velocity vector representing the rate of change per second.</returns>
		public static Vector3 Velocity(Vector3 current, Vector3 last, float deltaTime)
		{
			return Divide(last - current, deltaTime);
		}
		/// <summary>
		/// Loops a number within the range [0, after), wrapping around when exceeding boundaries.
		/// Similar to modulo operation but handles negative numbers correctly by adding the range until the value is positive.
		/// Useful for circular arrays, angle normalization, periodic functions, and cyclic behaviors.
		/// </summary>
		/// <param name="number">The number to loop within the range.</param>
		/// <param name="after">The upper bound (exclusive) of the range.</param>
		/// <returns>The looped number within the range [0, after).</returns>
		public static float LoopNumber(float number, float after)
		{
			while (number >= after)
				number -= after;

			while (number < 0)
				number += after;

			return number;
		}
		/// <summary>
		/// Determines if a numeric value has transitioned from off to on (false to true) between states.
		/// Converts numeric values to boolean (0 = false, non-zero = true) and detects rising edge transitions.
		/// Useful for detecting button presses, trigger activations, or state changes in numeric signals.
		/// </summary>
		/// <param name="current">The current float value.</param>
		/// <param name="last">The previous float value.</param>
		/// <returns>True if the value has transitioned from zero to non-zero, false otherwise.</returns>
		public static bool IsDownFromLastState(float current, float last)
		{
			return IsDownFromLastState(NumberToBool(current), NumberToBool(last));
		}
		/// <summary>
		/// Determines if an integer value has transitioned from off to on (0 to non-zero) between states.
		/// Converts integer values to boolean (0 = false, non-zero = true) and detects rising edge transitions.
		/// Useful for detecting discrete state changes, counter activations, or flag transitions.
		/// </summary>
		/// <param name="current">The current integer value.</param>
		/// <param name="last">The previous integer value.</param>
		/// <returns>True if the value has transitioned from zero to non-zero, false otherwise.</returns>
		public static bool IsDownFromLastState(int current, int last)
		{
			return IsDownFromLastState(NumberToBool(current), NumberToBool(last));
		}
		/// <summary>
		/// Determines if a boolean value has transitioned from false to true between states.
		/// Detects rising edge transitions in boolean signals, which is essential for event detection.
		/// Fundamental for implementing edge-triggered behaviors in state machines, input systems, and event handling.
		/// </summary>
		/// <param name="current">The current boolean value.</param>
		/// <param name="last">The previous boolean value.</param>
		/// <returns>True if the value has transitioned from false to true, false otherwise.</returns>
		public static bool IsDownFromLastState(bool current, bool last)
		{
			return !last && current;
		}
		/// <summary>
		/// Determines if a numeric value has transitioned from on to off (true to false) between states.
		/// Converts numeric values to boolean (0 = false, non-zero = true) and detects falling edge transitions.
		/// Useful for detecting button releases, trigger deactivations, or state changes in numeric signals.
		/// </summary>
		/// <param name="current">The current float value.</param>
		/// <param name="last">The previous float value.</param>
		/// <returns>True if the value has transitioned from non-zero to zero, false otherwise.</returns>
		public static bool IsUpFromLastState(float current, float last)
		{
			return IsUpFromLastState(NumberToBool(current), NumberToBool(last));
		}
		/// <summary>
		/// Determines if an integer value has transitioned from on to off (non-zero to 0) between states.
		/// Converts integer values to boolean (0 = false, non-zero = true) and detects falling edge transitions.
		/// Useful for detecting discrete state changes, counter deactivations, or flag transitions.
		/// </summary>
		/// <param name="current">The current integer value.</param>
		/// <param name="last">The previous integer value.</param>
		/// <returns>True if the value has transitioned from non-zero to zero, false otherwise.</returns>
		public static bool IsUpFromLastState(int current, int last)
		{
			return IsUpFromLastState(NumberToBool(current), NumberToBool(last));
		}
		/// <summary>
		/// Determines if a boolean value has transitioned from true to false between states.
		/// Detects falling edge transitions in boolean signals, which is essential for event detection.
		/// Fundamental for implementing edge-triggered behaviors in state machines, input systems, and event handling.
		/// </summary>
		/// <param name="current">The current boolean value.</param>
		/// <param name="last">The previous boolean value.</param>
		/// <returns>True if the value has transitioned from true to false, false otherwise.</returns>
		public static bool IsUpFromLastState(bool current, bool last)
		{
			return last && !current;
		}
		/// <summary>
		/// Divides a float3 vector by another float3 vector component-wise, with zero protection.
		/// Performs element-wise division while preventing division by zero by returning 0 for any component where the divisor is zero.
		/// Useful for safe vector operations, scaling, and normalization with potential zero components.
		/// </summary>
		/// <param name="a">The dividend float3 vector.</param>
		/// <param name="b">The divisor float3 vector.</param>
		/// <returns>A new float3 vector with the result of the component-wise division.</returns>
		public static float3 Divide(float3 a, float3 b)
		{
			return new Vector3(b.x != 0f ? a.x / b.x : 0f, b.y != 0f ? a.y / b.y : 0f, b.z != 0f ? a.z / b.z : 0f);
		}
		/// <summary>
		/// Divides a Vector3 by another Vector3 component-wise, with zero protection.
		/// Performs element-wise division while preventing division by zero by returning 0 for any component where the divisor is zero.
		/// Useful for safe vector operations, scaling, and normalization with potential zero components.
		/// </summary>
		/// <param name="a">The dividend Vector3.</param>
		/// <param name="b">The divisor Vector3.</param>
		/// <returns>A new Vector3 with the result of the component-wise division.</returns>
		public static Vector3 Divide(Vector3 a, Vector3 b)
		{
			return new Vector3(b.x != 0f ? a.x / b.x : 0f, b.y != 0f ? a.y / b.y : 0f, b.z != 0f ? a.z / b.z : 0f);
		}
		/// <summary>
		/// Divides a series of Vector3 vectors component-wise.
		/// Takes the first vector as the dividend and divides it by each subsequent vector in sequence.
		/// Provides a convenient way to perform multiple divisions in a single operation.
		/// </summary>
		/// <param name="vectors">The series of Vector3 vectors, where the first is divided by all others.</param>
		/// <returns>The result of the sequential component-wise division.</returns>
		public static Vector3 Divide(params Vector3[] vectors)
		{
			return Divide(vectors as IEnumerable<Vector3>);
		}
		/// <summary>
		/// Divides a series of Vector3 vectors component-wise from an enumerable collection.
		/// Takes the first vector as the dividend and divides it by each subsequent vector in sequence.
		/// Provides a flexible way to perform multiple divisions with collections like Lists or Arrays.
		/// </summary>
		/// <param name="vectors">The enumerable collection of Vector3 vectors, where the first is divided by all others.</param>
		/// <returns>The result of the sequential component-wise division, or default(Vector3) if the collection is empty.</returns>
		public static Vector3 Divide(IEnumerable<Vector3> vectors)
		{
			if (vectors.Count() < 1)
				return default;

			Vector3 result = vectors.FirstOrDefault();

			for (int i = 1; i < vectors.Count(); i++)
			{
				result.x /= vectors.ElementAt(i).x != 0f ? vectors.ElementAt(i).x : 0f;
				result.y /= vectors.ElementAt(i).y != 0f ? vectors.ElementAt(i).y : 0f;
				result.z /= vectors.ElementAt(i).z != 0f ? vectors.ElementAt(i).z : 0f;
			}

			return result;
		}
		/// <summary>
		/// Divides a Vector3 by a scalar float value, with zero protection.
		/// Scales down each component of the vector by the same factor, returning default(Vector3) if divider is zero.
		/// Essential for uniform scaling operations, normalization, and converting between different units of measurement.
		/// </summary>
		/// <param name="vector">The Vector3 to be divided.</param>
		/// <param name="divider">The scalar divisor to divide by.</param>
		/// <returns>The result of dividing the vector by the scalar, or default(Vector3) if divider is zero.</returns>
		public static Vector3 Divide(Vector3 vector, float divider)
		{
			if (divider == 0f)
				return default;

			return new Vector3(vector.x / divider, vector.y / divider, vector.z / divider);
		}
		/// <summary>
		/// Divides a float3 by a scalar float value, with zero protection.
		/// Scales down each component of the vector by the same factor, returning default(float3) if divider is zero.
		/// Provides a Unity.Mathematics alternative to Vector3 division for performance-critical code.
		/// </summary>
		/// <param name="vector">The float3 to be divided.</param>
		/// <param name="divider">The scalar divisor to divide by.</param>
		/// <returns>The result of dividing the vector by the scalar, or default(float3) if divider is zero.</returns>
		public static float3 Divide(float3 vector, float divider)
		{
			if (divider == 0f)
				return default;

			return new float3(vector.x / divider, vector.y / divider, vector.z / divider);
		}
		/// <summary>
		/// Multiplies two float3 vectors component-wise.
		/// Performs element-wise multiplication, where each component of the result is the product of the corresponding components.
		/// Useful for scaling, masking, or applying weights to individual vector components.
		/// </summary>
		/// <param name="a">The first float3 vector.</param>
		/// <param name="b">The second float3 vector.</param>
		/// <returns>A new float3 vector with the component-wise product of the inputs.</returns>
		public static float3 Multiply(float3 a, float3 b)
		{
			return new float3(a.x * b.x, a.y * b.y, a.z * b.z);
		}
		/// <summary>
		/// Multiplies two Vector3 vectors component-wise.
		/// Performs element-wise multiplication, where each component of the result is the product of the corresponding components.
		/// Useful for scaling, masking, or applying weights to individual vector components.
		/// </summary>
		/// <param name="a">The first Vector3.</param>
		/// <param name="b">The second Vector3.</param>
		/// <returns>A new Vector3 with the component-wise product of the inputs.</returns>
		public static Vector3 Multiply(Vector3 a, Vector3 b)
		{
			return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
		}
		/// <summary>
		/// Multiplies a series of Vector3 vectors component-wise.
		/// Takes all vectors and multiplies them together component by component.
		/// Provides a convenient way to perform multiple multiplications in a single operation.
		/// </summary>
		/// <param name="vectors">The series of Vector3 vectors to multiply together.</param>
		/// <returns>The result of the component-wise multiplication of all vectors.</returns>
		public static Vector3 Multiply(params Vector3[] vectors)
		{
			return Multiply(vectors as IEnumerable<Vector3>);
		}
		/// <summary>
		/// Multiplies a series of Vector3 vectors component-wise from an enumerable collection.
		/// Takes all vectors in the collection and multiplies them together component by component.
		/// Provides a flexible way to perform multiple multiplications with collections like Lists or Arrays.
		/// </summary>
		/// <param name="vectors">The enumerable collection of Vector3 vectors to multiply together.</param>
		/// <returns>The result of the component-wise multiplication of all vectors, or default(Vector3) if the collection is empty.</returns>
		public static Vector3 Multiply(IEnumerable<Vector3> vectors)
		{
			if (vectors.Count() < 1)
				return default;

			Vector3 result = vectors.FirstOrDefault();

			for (int i = 1; i < vectors.Count(); i++)
			{
				result.x *= vectors.ElementAt(i).x;
				result.y *= vectors.ElementAt(i).y;
				result.z *= vectors.ElementAt(i).z;
			}

			return result;
		}
		/// <summary>
		/// Calculates the average of two float3 vectors.
		/// Computes the arithmetic mean of each component, resulting in a point halfway between the two input vectors.
		/// Useful for finding midpoints, interpolation, and balancing between two positions or directions.
		/// </summary>
		/// <param name="a">The first float3 vector.</param>
		/// <param name="b">The second float3 vector.</param>
		/// <returns>A new float3 vector representing the component-wise average of the inputs.</returns>
		public static float3 Average(float3 a, float3 b)
		{
			return new float3((a.x + b.x) * .5f, (a.y + b.y) * .5f, (a.z + b.z) * .5f);
		}
		/// <summary>
		/// Calculates the average of three float3 vectors.
		/// Computes the arithmetic mean of each component across all three vectors.
		/// Useful for finding the centroid of a triangle, balancing between three positions, or weighted blending.
		/// </summary>
		/// <param name="a">The first float3 vector.</param>
		/// <param name="b">The second float3 vector.</param>
		/// <param name="c">The third float3 vector.</param>
		/// <returns>A new float3 vector representing the component-wise average of the three inputs.</returns>
		public static float3 Average(float3 a, float3 b, float3 c)
		{
			return new float3((a.x + b.x + c.x) / 3f, (a.y + b.y + c.y) / 3f, (a.z + b.z + c.z) / 3f);
		}
		/// <summary>
		/// Calculates the average of multiple Vector3 vectors.
		/// Computes the arithmetic mean of each component across all provided vectors.
		/// Useful for finding centroids, balancing multiple positions, or calculating center of mass.
		/// </summary>
		/// <param name="vectors">The Vector3 vectors to average.</param>
		/// <returns>A new Vector3 representing the component-wise average of all input vectors.</returns>
		public static Vector3 Average(params Vector3[] vectors)
		{
			return Average(vectors as IEnumerable<Vector3>);
		}
		/// <summary>
		/// Calculates the average of multiple Vector3 vectors from an enumerable collection.
		/// Computes the arithmetic mean of each component across all vectors in the collection.
		/// Provides a flexible way to average vectors from various collection types like Lists or Arrays.
		/// </summary>
		/// <param name="vectors">The enumerable collection of Vector3 vectors to average.</param>
		/// <returns>A new Vector3 representing the component-wise average of all input vectors, or default(Vector3) if the collection is empty.</returns>
		public static Vector3 Average(IEnumerable<Vector3> vectors)
		{
			if (vectors.Count() < 1)
				return default;

			return new Vector3()
			{
				x = vectors.Average(vector => vector.x),
				y = vectors.Average(vector => vector.y),
				z = vectors.Average(vector => vector.z)
			};
		}
		/// <summary>
		/// Calculates the average of multiple Vector2 vectors.
		/// Computes the arithmetic mean of each component across all provided vectors.
		/// Useful for finding centroids, balancing multiple 2D positions, or calculating center of mass in 2D space.
		/// </summary>
		/// <param name="vectors">The Vector2 vectors to average.</param>
		/// <returns>A new Vector2 representing the component-wise average of all input vectors.</returns>
		public static Vector2 Average(params Vector2[] vectors)
		{
			return Average(vectors as IEnumerable<Vector2>);
		}
		/// <summary>
		/// Calculates the average of multiple Vector2 vectors from an enumerable collection.
		/// Computes the arithmetic mean of each component across all vectors in the collection.
		/// Provides a flexible way to average 2D vectors from various collection types like Lists or Arrays.
		/// </summary>
		/// <param name="vectors">The enumerable collection of Vector2 vectors to average.</param>
		/// <returns>A new Vector2 representing the component-wise average of all input vectors, or default(Vector2) if the collection is empty.</returns>
		public static Vector2 Average(IEnumerable<Vector2> vectors)
		{
			if (vectors.Count() < 1)
				return default;

			return new Vector2()
			{
				x = vectors.Average(vector => vector.x),
				y = vectors.Average(vector => vector.y)
			};
		}
		/// <summary>
		/// Calculates the average of multiple Vector3Int vectors.
		/// Computes the arithmetic mean of each component and rounds to the nearest integer.
		/// Useful for finding central grid positions, tile coordinates, or discrete spatial averages.
		/// </summary>
		/// <param name="vectors">The Vector3Int vectors to average.</param>
		/// <returns>A new Vector3Int representing the rounded component-wise average of all input vectors.</returns>
		public static Vector3Int Average(params Vector3Int[] vectors)
		{
			return Average(vectors as IEnumerable<Vector3Int>);
		}
		/// <summary>
		/// Calculates the average of multiple Vector3Int vectors from an enumerable collection.
		/// Computes the arithmetic mean of each component and rounds to the nearest integer.
		/// Provides a flexible way to average discrete 3D coordinates from various collection types.
		/// </summary>
		/// <param name="vectors">The enumerable collection of Vector3Int vectors to average.</param>
		/// <returns>A new Vector3Int representing the rounded component-wise average of all input vectors, or default(Vector3Int) if the collection is empty.</returns>
		public static Vector3Int Average(IEnumerable<Vector3Int> vectors)
		{
			if (vectors.Count() < 1)
				return default;

			return RoundToInt(new Vector3()
			{
				x = (float)vectors.Average(vector => vector.x),
				y = (float)vectors.Average(vector => vector.y),
				z = (float)vectors.Average(vector => vector.z)
			});
		}
		/// <summary>
		/// Calculates the average of multiple Vector2Int vectors.
		/// Computes the arithmetic mean of each component and rounds to the nearest integer.
		/// Useful for finding central grid positions, tile coordinates, or discrete 2D spatial averages.
		/// </summary>
		/// <param name="vectors">The Vector2Int vectors to average.</param>
		/// <returns>A new Vector2Int representing the rounded component-wise average of all input vectors.</returns>
		public static Vector2Int Average(params Vector2Int[] vectors)
		{
			return Average(vectors as IEnumerable<Vector2Int>);
		}
		/// <summary>
		/// Calculates the average of multiple Vector2Int vectors from an enumerable collection.
		/// Computes the arithmetic mean of each component and rounds to the nearest integer.
		/// Provides a flexible way to average discrete 2D coordinates from various collection types.
		/// </summary>
		/// <param name="vectors">The enumerable collection of Vector2Int vectors to average.</param>
		/// <returns>A new Vector2Int representing the rounded component-wise average of all input vectors, or default(Vector2Int) if the collection is empty.</returns>
		public static Vector2Int Average(IEnumerable<Vector2Int> vectors)
		{
			if (vectors.Count() < 1)
				return default;

			return RoundToInt(new Vector2()
			{
				x = (float)vectors.Average(vector => vector.x),
				y = (float)vectors.Average(vector => vector.y)
			});
		}
		/// <summary>
		/// Calculates the average of multiple Quaternion rotations.
		/// Uses a weighted spherical linear interpolation (Slerp) approach to properly average rotations in 3D space.
		/// This method maintains proper rotation interpolation, avoiding issues that can occur with simple component-wise averaging.
		/// </summary>
		/// <param name="quaternions">The quaternions to average.</param>
		/// <returns>A new Quaternion representing the average of all input quaternions, or the identity quaternion if the array is empty.</returns>
		public static Quaternion Average(params Quaternion[] quaternions)
		{
			return Average(quaternions as IEnumerable<Quaternion>);
		}
		/// <summary>
		/// Calculates the average of multiple Quaternion vectors from an enumerable collection.
		/// Implements a progressive weighted averaging algorithm using spherical linear interpolation (Slerp).
		/// This approach properly handles the non-linear nature of quaternion rotation space, ensuring correct
		/// interpolation between orientations regardless of their distribution in 3D space.
		/// </summary>
		/// <param name="quaternions">The enumerable collection of quaternions to average.</param>
		/// <returns>A new Quaternion representing the average of all input quaternions, or the default quaternion if the collection is empty.</returns>
		public static Quaternion Average(IEnumerable<Quaternion> quaternions)
		{
			if (quaternions.Count() < 1)
				return default;

			Quaternion average = quaternions.FirstOrDefault();
			float weight;

			for (int i = 1; i < quaternions.Count(); i++)
			{
				weight = 1f / (i + 1);
				average = Quaternion.Slerp(average, quaternions.ElementAt(i), weight);
			}

			return average;
		}		
		/// <summary>
		/// Calculates the average of two float values.
		/// Computes the arithmetic mean by adding the values and multiplying by 0.5.
		/// This optimized implementation avoids division operations.
		/// </summary>
		/// <param name="a">The first value.</param>
		/// <param name="b">The second value.</param>
		/// <returns>The average of the two values.</returns>
		public static float Average(float a, float b)
		{
			return (a + b) * .5f;
		}
		/// <summary>
		/// Calculates the average of three float values.
		/// Computes the arithmetic mean by adding all values and dividing by 3.
		/// </summary>
		/// <param name="a">The first value.</param>
		/// <param name="b">The second value.</param>
		/// <param name="c">The third value.</param>
		/// <returns>The average of the three values.</returns>
		public static float Average(float a, float b, float c)
		{
			return (a + b + c) / 3f;
		}
		/// <summary>
		/// Calculates the average of four float values.
		/// Computes the arithmetic mean by adding all values and multiplying by 0.25.
		/// This optimized implementation avoids division operations.
		/// </summary>
		/// <param name="a">The first value.</param>
		/// <param name="b">The second value.</param>
		/// <param name="c">The third value.</param>
		/// <param name="d">The fourth value.</param>
		/// <returns>The average of the four values.</returns>
		public static float Average(float a, float b, float c, float d)
		{
			return (a + b + c + d) * .25f;
		}
		/// <summary>
		/// Calculates the average of five float values.
		/// Computes the arithmetic mean by adding all values and multiplying by 0.2.
		/// This optimized implementation avoids division operations.
		/// </summary>
		/// <param name="a">The first value.</param>
		/// <param name="b">The second value.</param>
		/// <param name="c">The third value.</param>
		/// <param name="d">The fourth value.</param>
		/// <param name="e">The fifth value.</param>
		/// <returns>The average of the five values.</returns>
		public static float Average(float a, float b, float c, float d, float e)
		{
			return (a + b + c + d + e) * .2f;
		}
		/// <summary>
		/// Calculates the average of multiple float values.
		/// Leverages LINQ's Average method for efficient computation of the arithmetic mean.
		/// Handles edge cases by returning 0 for empty arrays.
		/// </summary>
		/// <param name="floats">The values to average.</param>
		/// <returns>The average of all input values, or 0 if the array is empty.</returns>
		public static float Average(params float[] floats)
		{
			if (floats.Length < 1)
				return 0f;

			return floats.Average();
		}
		/// <summary>
		/// Calculates the average of multiple byte values.
		/// Converts bytes to floats for averaging, then rounds and converts back to byte.
		/// This ensures proper handling of the arithmetic mean for discrete byte values.
		/// </summary>
		/// <param name="bytes">The values to average.</param>
		/// <returns>The average of all input values rounded to the nearest byte, or 0 if the array is empty.</returns>
		public static byte Average(params byte[] bytes)
		{
			if (bytes.Length < 1)
				return 0;

			return (byte)(int)math.round(bytes.Select(@byte => (float)@byte).Average());
		}
		/// <summary>
		/// Calculates the average of multiple integer values.
		/// Computes the arithmetic mean and rounds to the nearest integer.
		/// This ensures a proper integer average rather than truncating the decimal portion.
		/// </summary>
		/// <param name="integers">The values to average.</param>
		/// <returns>The average of all input values rounded to the nearest integer, or 0 if the array is empty.</returns>
		public static int Average(params int[] integers)
		{
			if (integers.Length < 1)
				return 0;

			return (int)math.round((float)integers.Average());
		}
		/// <summary>
		/// Squares an integer value.
		/// Multiplies the number by itself to compute the square.
		/// Useful for various mathematical calculations where squared values are needed.
		/// </summary>
		/// <param name="number">The value to square.</param>
		/// <returns>The square of the input value.</returns>
		public static int Square(int number)
		{
			return number * number;
		}
		/// <summary>
		/// Squares a float value.
		/// Multiplies the number by itself to compute the square.
		/// Useful for various mathematical calculations where squared values are needed.
		/// </summary>
		/// <param name="number">The value to square.</param>
		/// <returns>The square of the input value.</returns>
		public static float Square(float number)
		{
			return number * number;
		}
		/// <summary>
		/// Finds the maximum value among multiple byte values.
		/// Iterates through the array to find the largest value.
		/// Handles edge cases by returning default(byte) for empty arrays.
		/// </summary>
		/// <param name="numbers">The values to compare.</param>
		/// <returns>The maximum value among the input values, or default(byte) if the array is empty.</returns>
		public static byte Max(params byte[] numbers)
		{
			if (numbers.Length < 1)
				return default;

			byte max = numbers[0];

			for (int i = 1; i < numbers.Length; i++)
				if (numbers[i] > max)
					max = numbers[i];

			return max;
		}
		/// <summary>
		/// Finds the minimum value among multiple byte values.
		/// Iterates through the array to find the smallest value.
		/// Handles edge cases by returning default(byte) for empty arrays.
		/// </summary>
		/// <param name="numbers">The values to compare.</param>
		/// <returns>The minimum value among the input values, or default(byte) if the array is empty.</returns>
		public static byte Min(params byte[] numbers)
		{
			if (numbers.Length < 1)
				return default;

			byte min = numbers[0];

			for (int i = 1; i < numbers.Length; i++)
				if (min > numbers[i])
					min = numbers[i];

			return min;
		}
		/// <summary>
		/// Finds the maximum value among three float values.
		/// Uses Unity.Mathematics math.max function for efficient comparison.
		/// Useful for finding the largest value in a small set without creating an array.
		/// </summary>
		/// <param name="a">The first value.</param>
		/// <param name="b">The second value.</param>
		/// <param name="c">The third value.</param>
		/// <returns>The maximum of the three values.</returns>
		public static float Max(float a, float b, float c)
		{
			return math.max(a, math.max(b, c));
		}		
		/// <summary>
		/// Finds the minimum value among three float values.
		/// Uses Unity.Mathematics math.min function for efficient comparison.
		/// Useful for finding the smallest value in a small set without creating an array.
		/// </summary>
		/// <param name="a">The first value.</param>
		/// <param name="b">The second value.</param>
		/// <param name="c">The third value.</param>
		/// <returns>The minimum of the three values.</returns>
		public static float Min(float a, float b, float c)
		{
			return math.min(a, math.min(b, c));
		}
		/// <summary>
		/// Finds the maximum value among four float values.
		/// Uses Unity.Mathematics math.max function for efficient comparison.
		/// Optimizes the comparison by pairing values and comparing the results.
		/// </summary>
		/// <param name="a">The first value.</param>
		/// <param name="b">The second value.</param>
		/// <param name="c">The third value.</param>
		/// <param name="d">The fourth value.</param>
		/// <returns>The maximum of the four values.</returns>
		public static float Max(float a, float b, float c, float d)
		{
			return math.max(math.max(a, b), math.max(c, d));
		}
		/// <summary>
		/// Finds the minimum value among four float values.
		/// Uses Unity.Mathematics math.min function for efficient comparison.
		/// Optimizes the comparison by pairing values and comparing the results.
		/// </summary>
		/// <param name="a">The first value.</param>
		/// <param name="b">The second value.</param>
		/// <param name="c">The third value.</param>
		/// <param name="d">The fourth value.</param>
		/// <returns>The minimum of the four values.</returns>
		public static float Min(float a, float b, float c, float d)
		{
			return math.min(math.min(a, b), math.min(c, d));
		}
		/// <summary>
		/// Finds the maximum value among five float values.
		/// Uses Unity.Mathematics math.max function for efficient comparison.
		/// Optimizes the comparison by grouping values and comparing the results.
		/// </summary>
		/// <param name="a">The first value.</param>
		/// <param name="b">The second value.</param>
		/// <param name="c">The third value.</param>
		/// <param name="d">The fourth value.</param>
		/// <param name="e">The fifth value.</param>
		/// <returns>The maximum of the five values.</returns>
		public static float Max(float a, float b, float c, float d, float e)
		{
			return math.max(math.max(math.max(a, b), math.max(c, d)), e);
		}
		/// <summary>
		/// Finds the minimum value among five float values.
		/// Uses Unity.Mathematics math.min function for efficient comparison.
		/// Optimizes the comparison by grouping values and comparing the results.
		/// </summary>
		/// <param name="a">The first value.</param>
		/// <param name="b">The second value.</param>
		/// <param name="c">The third value.</param>
		/// <param name="d">The fourth value.</param>
		/// <param name="e">The fifth value.</param>
		/// <returns>The minimum of the five values.</returns>
		public static float Min(float a, float b, float c, float d, float e)
		{
			return math.min(math.min(math.min(a, b), math.min(c, d)), e);
		}
		/// <summary>
		/// Finds the maximum value among six float values.
		/// Uses Unity.Mathematics math.max function for efficient comparison.
		/// Optimizes the comparison by pairing values and comparing the results in a balanced tree structure.
		/// </summary>
		/// <param name="a">The first value.</param>
		/// <param name="b">The second value.</param>
		/// <param name="c">The third value.</param>
		/// <param name="d">The fourth value.</param>
		/// <param name="e">The fifth value.</param>
		/// <param name="f">The sixth value.</param>
		/// <returns>The maximum of the six values.</returns>
		public static float Max(float a, float b, float c, float d, float e, float f)
		{
			return math.max(math.max(math.max(a, b), math.max(c, d)), math.max(e, f));
		}
		/// <summary>
		/// Finds the minimum value among six float values.
		/// Uses Unity.Mathematics math.min function for efficient comparison.
		/// Optimizes the comparison by pairing values and comparing the results in a balanced tree structure.
		/// </summary>
		/// <param name="a">The first value.</param>
		/// <param name="b">The second value.</param>
		/// <param name="c">The third value.</param>
		/// <param name="d">The fourth value.</param>
		/// <param name="e">The fifth value.</param>
		/// <param name="f">The sixth value.</param>
		/// <returns>The minimum of the six values.</returns>
		public static float Min(float a, float b, float c, float d, float e, float f)
		{
			return math.min(math.min(math.min(a, b), math.min(c, d)), math.min(e, f));
		}
		/// <summary>
		/// Finds the maximum value among multiple float values.
		/// Iterates through the array to find the largest value.
		/// Handles edge cases by returning default(float) for empty arrays.
		/// </summary>
		/// <param name="numbers">The values to compare.</param>
		/// <returns>The maximum value among the input values, or default(float) if the array is empty.</returns>
		public static float Max(params float[] numbers)
		{
			if (numbers.Length < 1)
				return default;

			float max = numbers[0];

			for (int i = 1; i < numbers.Length; i++)
				if (numbers[i] > max)
					max = numbers[i];

			return max;
		}
		/// <summary>
		/// Finds the minimum value among multiple float values.
		/// Iterates through the array to find the smallest value.
		/// Handles edge cases by returning default(float) for empty arrays.
		/// </summary>
		/// <param name="numbers">The values to compare.</param>
		/// <returns>The minimum value among the input values, or default(float) if the array is empty.</returns>
		public static float Min(params float[] numbers)
		{
			if (numbers.Length < 1)
				return default;

			float min = numbers[0];

			for (int i = 1; i < numbers.Length; i++)
				if (min > numbers[i])
					min = numbers[i];

			return min;
		}
		/// <summary>
		/// Clamps a float value between a minimum value and positive infinity.
		/// If min is negative, clamps between negative infinity and min.
		/// </summary>
		/// <param name="number">The value to clamp.</param>
		/// <param name="min">The minimum value. If positive, acts as lower bound. If negative, acts as upper bound.</param>
		/// <returns>The clamped value, either not less than min (when min ≥ 0) or not greater than min (when min < 0).</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static float ClampInfinity(float number, float min = 0f)
		{
			return min >= 0f ? math.max(number, min) : math.min(number, min);
		}
		/// <summary>
		/// Clamps an integer value between a minimum value and positive infinity.
		/// If min is negative, clamps between negative infinity and min.
		/// </summary>
		/// <param name="number">The value to clamp.</param>
		/// <param name="min">The minimum value. If positive, acts as lower bound. If negative, acts as upper bound.</param>
		/// <returns>The clamped value, either not less than min (when min ≥ 0) or not greater than min (when min < 0).</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static int ClampInfinity(int number, int min = 0)
		{
			return min >= 0 ? math.max(number, min) : math.min(number, min);
		}
		/// <summary>
		/// Clamps the absolute value of a float to be not less than the specified minimum value.
		/// Ensures the magnitude of the number is at least the minimum value.
		/// </summary>
		/// <param name="number">The value whose absolute value will be clamped.</param>
		/// <param name="min">The minimum absolute value (must be non-negative).</param>
		/// <returns>The number with an absolute value not less than min, preserving the original sign.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static float ClampInfinityAbs(float number, float min = 0f)
		{
			return math.max(math.abs(number), min);
		}
		/// <summary>
		/// Clamps the absolute value of an integer to be not less than the specified minimum value.
		/// Ensures the magnitude of the number is at least the minimum value.
		/// </summary>
		/// <param name="number">The value whose absolute value will be clamped.</param>
		/// <param name="min">The minimum absolute value (must be non-negative).</param>
		/// <returns>The number with an absolute value not less than min, preserving the original sign.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static int ClampInfinityAbs(int number, int min = 0)
		{
			return math.max(math.abs(number), min);
		}
		/// <summary>
		/// Clamps each component of a Vector3 between a minimum scalar value and positive/negative infinity.
		/// If min is positive, ensures no component is less than min.
		/// If min is negative, ensures no component is greater than min.
		/// </summary>
		/// <param name="vector">The Vector3 to clamp.</param>
		/// <param name="min">The minimum value. If positive, acts as lower bound. If negative, acts as upper bound.</param>
		/// <returns>The clamped Vector3 with each component appropriately bounded.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static Vector3 ClampInfinity(Vector3 vector, float min = 0f)
		{
			return min >= 0f ? new Vector3(math.max(vector.x, min), math.max(vector.y, min), math.max(vector.z, min)) : new Vector3(math.min(vector.x, min), math.min(vector.y, min), math.min(vector.z, min));
		}
		/// <summary>
		/// Clamps each component of a Vector3 between corresponding components of a minimum Vector3.
		/// Uses the average of min's components to determine whether to apply upper or lower bounds.
		/// If average is positive, ensures no component is less than corresponding min component.
		/// If average is negative, ensures no component is greater than corresponding min component.
		/// </summary>
		/// <param name="vector">The Vector3 to clamp.</param>
		/// <param name="min">The Vector3 containing minimum values for each component.</param>
		/// <returns>The clamped Vector3 with each component appropriately bounded.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static Vector3 ClampInfinity(Vector3 vector, Vector3 min)
		{
			return Average(min.x, min.y, min.z) >= 0f ? new Vector3(math.max(vector.x, min.x), math.max(vector.y, min.y), math.max(vector.z, min.z)) : new Vector3(math.min(vector.x, min.x), math.min(vector.y, min.y), math.min(vector.z, min.z));
		}
		/// <summary>
		/// Clamps each component of a Vector2 between a minimum scalar value and positive/negative infinity.
		/// If min is positive, ensures no component is less than min.
		/// If min is negative, ensures no component is greater than min.
		/// </summary>
		/// <param name="vector">The Vector2 to clamp.</param>
		/// <param name="min">The minimum value. If positive, acts as lower bound. If negative, acts as upper bound.</param>
		/// <returns>The clamped Vector2 with each component appropriately bounded.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static Vector2 ClampInfinity(Vector2 vector, float min = 0f)
		{
			return min >= 0f ? new Vector2(math.max(vector.x, min), math.max(vector.y, min)) : new Vector2(math.min(vector.x, min), math.min(vector.y, min));
		}
		/// <summary>
		/// Clamps each component of a Vector2 between corresponding components of a minimum Vector2.
		/// Uses the average of min's components to determine whether to apply upper or lower bounds.
		/// If average is positive, ensures no component is less than corresponding min component.
		/// If average is negative, ensures no component is greater than corresponding min component.
		/// </summary>
		/// <param name="vector">The Vector2 to clamp.</param>
		/// <param name="min">The Vector2 containing minimum values for each component.</param>
		/// <returns>The clamped Vector2 with each component appropriately bounded.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static Vector2 ClampInfinity(Vector2 vector, Vector2 min)
		{
			return Average(min.x, min.y) >= 0f ? new Vector2(math.max(vector.x, min.x), math.max(vector.y, min.y)) : new Vector2(math.min(vector.x, min.x), math.min(vector.y, min.y));
		}
		/// <summary>
		/// Clamps each component of a float3 between a minimum scalar value and positive/negative infinity.
		/// If min is positive, ensures no component is less than min.
		/// If min is negative, ensures no component is greater than min.
		/// </summary>
		/// <param name="vector">The float3 to clamp.</param>
		/// <param name="min">The minimum value. If positive, acts as lower bound. If negative, acts as upper bound.</param>
		/// <returns>The clamped float3 with each component appropriately bounded.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static float3 ClampInfinity(float3 vector, float min = 0f)
		{
			return min >= 0f ? new float3(math.max(vector.x, min), math.max(vector.y, min), math.max(vector.z, min)) : new float3(math.min(vector.x, min), math.min(vector.y, min), math.min(vector.z, min));
		}
		/// <summary>
		/// Clamps each component of a float3 between corresponding components of a minimum float3.
		/// Uses the average of min's components to determine whether to apply upper or lower bounds.
		/// If average is positive, ensures no component is less than corresponding min component.
		/// If average is negative, ensures no component is greater than corresponding min component.
		/// </summary>
		/// <param name="vector">The float3 to clamp.</param>
		/// <param name="min">The float3 containing minimum values for each component.</param>
		/// <returns>The clamped float3 with each component appropriately bounded.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static float3 ClampInfinity(float3 vector, float3 min)
		{
			return Average(min.x, min.y, min.z) >= 0f ? new float3(math.max(vector.x, min.x), math.max(vector.y, min.y), math.max(vector.z, min.z)) : new float3(math.min(vector.x, min.x), math.min(vector.y, min.y), math.min(vector.z, min.z));
		}
		/// <summary>
		/// Clamps each component of a float2 between a minimum scalar value and positive/negative infinity.
		/// If min is positive, ensures no component is less than min.
		/// If min is negative, ensures no component is greater than min.
		/// </summary>
		/// <param name="vector">The float2 to clamp.</param>
		/// <param name="min">The minimum value. If positive, acts as lower bound. If negative, acts as upper bound.</param>
		/// <returns>The clamped float2 with each component appropriately bounded.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static float2 ClampInfinity(float2 vector, float min = 0f)
		{
			return min >= 0f ? new float2(math.max(vector.x, min), math.max(vector.y, min)) : new float2(math.min(vector.x, min), math.min(vector.y, min));
		}
		/// <summary>
		/// Clamps each component of a float2 between corresponding components of a minimum float2.
		/// Uses the average of min's components to determine whether to apply upper or lower bounds.
		/// If average is positive, ensures no component is less than corresponding min component.
		/// If average is negative, ensures no component is greater than corresponding min component.
		/// </summary>
		/// <param name="vector">The float2 to clamp.</param>
		/// <param name="min">The float2 containing minimum values for each component.</param>
		/// <returns>The clamped float2 with each component appropriately bounded.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static float2 ClampInfinity(float2 vector, float2 min)
		{
			return Average(min.x, min.y) >= 0f ? new float2(math.max(vector.x, min.x), math.max(vector.y, min.y)) : new float2(math.min(vector.x, min.x), math.min(vector.y, min.y));
		}
		/// <summary>
		/// Clamps a float value between 0 and 1.
		/// A convenience wrapper around math.clamp that specifically clamps to the 0-1 range,
		/// commonly used for normalized values like percentages, lerp factors, and color components.
		/// </summary>
		/// <param name="number">The value to clamp.</param>
		/// <returns>The value clamped between 0 and 1.</returns>
		public static float Clamp01(float number)
		{
			return math.clamp(number, 0f, 1f);
		}
		/// <summary>
		/// Determines if two Vector3 vectors are approximately equal.
		/// Compares each component (x, y, z) using Mathf.Approximately to account for floating-point imprecision.
		/// Returns true only if all three components are approximately equal.
		/// </summary>
		/// <param name="vector1">The first Vector3 to compare.</param>
		/// <param name="vector2">The second Vector3 to compare.</param>
		/// <returns>True if all components of the vectors are approximately equal, false otherwise.</returns>
		public static bool Approximately(Vector3 vector1, Vector3 vector2)
		{
			return Mathf.Approximately(vector1.x, vector2.x) && Mathf.Approximately(vector1.y, vector2.y) && Mathf.Approximately(vector1.z, vector2.z);
		}
		/// <summary>
		/// Determines if two float3 vectors are approximately equal.
		/// Compares each component (x, y, z) using Mathf.Approximately to account for floating-point imprecision.
		/// Returns true only if all three components are approximately equal.
		/// </summary>
		/// <param name="vector1">The first float3 to compare.</param>
		/// <param name="vector2">The second float3 to compare.</param>
		/// <returns>True if all components of the vectors are approximately equal, false otherwise.</returns>
		public static bool Approximately(float3 vector1, float3 vector2)
		{
			return Mathf.Approximately(vector1.x, vector2.x) && Mathf.Approximately(vector1.y, vector2.y) && Mathf.Approximately(vector1.z, vector2.z);
		}
		/// <summary>
		/// Determines if two Vector2 vectors are approximately equal.
		/// Compares each component (x, y) using Mathf.Approximately to account for floating-point imprecision.
		/// Returns true only if both components are approximately equal.
		/// </summary>
		/// <param name="vector1">The first Vector2 to compare.</param>
		/// <param name="vector2">The second Vector2 to compare.</param>
		/// <returns>True if all components of the vectors are approximately equal, false otherwise.</returns>
		public static bool Approximately(Vector2 vector1, Vector2 vector2)
		{
			return Mathf.Approximately(vector1.x, vector2.x) && Mathf.Approximately(vector1.y, vector2.y);
		}
		/// <summary>
		/// Determines if two float2 vectors are approximately equal.
		/// Compares each component (x, y) using Mathf.Approximately to account for floating-point imprecision.
		/// Returns true only if both components are approximately equal.
		/// </summary>
		/// <param name="vector1">The first float2 to compare.</param>
		/// <param name="vector2">The second float2 to compare.</param>
		/// <returns>True if all components of the vectors are approximately equal, false otherwise.</returns>
		public static bool Approximately(float2 vector1, float2 vector2)
		{
			return Mathf.Approximately(vector1.x, vector2.x) && Mathf.Approximately(vector1.y, vector2.y);
		}
		/// <summary>
		/// Converts a DateTime to a numeric timestamp in the format "yyyyMMddHHmmssffff".
		/// Creates a standardized long integer representation of the date and time,
		/// useful for sorting, comparison, and unique identifier generation.
		/// </summary>
		/// <param name="dateTime">The DateTime to convert to a timestamp.</param>
		/// <returns>A long integer timestamp representing the given DateTime.</returns>
		public static long GetTimestamp(DateTime dateTime)
		{
			return long.Parse(dateTime.ToString("yyyyMMddHHmmssffff"));
		}
		/// <summary>
		/// Gets a timestamp for the current date and time.
		/// Provides a numeric representation in the format "yyyyMMddHHmmssffff" of either local or UTC time.
		/// </summary>
		/// <param name="UTC">If true, uses Coordinated Universal Time (UTC); if false, uses local time.</param>
		/// <returns>A long integer timestamp representing the current date and time.</returns>
		public static long GetTimestamp(bool UTC = false)
		{
			return GetTimestamp(UTC ? DateTime.UtcNow : DateTime.Now);
		}
		/// <summary>
		/// Finds the intersection point of two line segments in 3D space.
		/// Calculates where two line segments (p1-p2 and p3-p4) intersect, if they do.
		/// The intersection is calculated on the XZ plane (ignoring Y values).
		/// </summary>
		/// <param name="p1">The start point of the first line segment.</param>
		/// <param name="p2">The end point of the first line segment.</param>
		/// <param name="p3">The start point of the second line segment.</param>
		/// <param name="p4">The end point of the second line segment.</param>
		/// <param name="intersection">When the method returns, contains the intersection point if the lines intersect; otherwise, Vector3.zero.</param>
		/// <returns>True if the line segments intersect, false otherwise.</returns>
		public static bool FindIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, out Vector3 intersection)
		{
			intersection = Vector3.zero;

			var d = (p2.x - p1.x) * (p4.z - p3.z) - (p2.z - p1.z) * (p4.x - p3.x);

			if (d == 0f)
				return false;

			var u = ((p3.x - p1.x) * (p4.z - p3.z) - (p3.z - p1.z) * (p4.x - p3.x)) / d;
			var v = ((p3.x - p1.x) * (p2.z - p1.z) - (p3.z - p1.z) * (p2.x - p1.x)) / d;

			if (u < 0f || u > 1f || v < 0f || v > 1f)
				return false;

			intersection.x = p1.x + u * (p2.x - p1.x);
			intersection.z = p1.z + u * (p2.z - p1.z);

			return true;
		}
		/// <summary>
		/// Applies torque to a rigidbody at a specific position by adding opposing forces at different points.
		/// Simulates rotational force by applying pairs of forces in opposite directions at offset positions.
		/// This creates rotation around the x, y, and z axes based on the torque vector components.
		/// </summary>
		/// <param name="rigid">The Rigidbody to apply torque to.</param>
		/// <param name="torque">The torque vector to apply (direction and magnitude).</param>
		/// <param name="point">The position in world space where the torque should be centered.</param>
		/// <param name="mode">The force mode to use when applying the forces.</param>
		public static void AddTorqueAtPosition(Rigidbody rigid, Vector3 torque, Vector3 point, ForceMode mode)
		{
			rigid.AddForceAtPosition(.5f * torque.y * Vector3.forward, point + Vector3.left, mode);
			rigid.AddForceAtPosition(.5f * torque.y * Vector3.back, point + Vector3.right, mode);
			rigid.AddForceAtPosition(.5f * torque.x * Vector3.forward, point + Vector3.up, mode);
			rigid.AddForceAtPosition(.5f * torque.x * Vector3.back, point + Vector3.down, mode);
			rigid.AddForceAtPosition(.5f * torque.z * Vector3.right, point + Vector3.up, mode);
			rigid.AddForceAtPosition(.5f * torque.z * Vector3.left, point + Vector3.down, mode);
		}
		/// <summary>
		/// Checks if a directory contains no files or subdirectories.
		/// Uses Directory.EnumerateFileSystemEntries for efficient checking without loading all entries into memory.
		/// </summary>
		/// <param name="path">The full path to the directory to check.</param>
		/// <returns>True if the directory exists and contains no files or subdirectories, false otherwise.</returns>
		public static bool IsDirectoryEmpty(string path)
		{
			IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path);

#pragma warning disable IDE0063 // Use simple 'using' statement
			using (IEnumerator<string> entry = items.GetEnumerator())
				return !entry.MoveNext();
#pragma warning restore IDE0063 // Use simple 'using' statement
		}
		/// <summary>
		/// Creates and configures a new AudioSource component on a new GameObject.
		/// Sets up spatial audio properties, volume, playback settings, and optional destruction after playback.
		/// Can be parented to another transform and assigned to an audio mixer group.
		/// </summary>
		/// <param name="sourceName">The name for the new GameObject with the AudioSource.</param>
		/// <param name="minDistance">The minimum distance at which the sound is heard at full volume.</param>
		/// <param name="maxDistance">The maximum distance at which the sound can be heard.</param>
		/// <param name="volume">The volume level of the audio source (0.0 to 1.0).</param>
		/// <param name="clip">The AudioClip to play.</param>
		/// <param name="loop">Whether the audio should loop.</param>
		/// <param name="playNow">Whether to start playing immediately.</param>
		/// <param name="destroyAfterFinished">Whether to destroy the GameObject after the clip finishes playing.</param>
		/// <param name="mute">Whether the audio source should be muted.</param>
		/// <param name="parent">Optional parent transform for the new GameObject.</param>
		/// <param name="mixer">Optional audio mixer group to route the audio through.</param>
		/// <param name="spatialize">Whether to enable spatialization for the audio source.</param>
		/// <returns>The configured AudioSource component.</returns>
		public static AudioSource NewAudioSource(string sourceName, float minDistance, float maxDistance, float volume, AudioClip clip, bool loop, bool playNow, bool destroyAfterFinished, bool mute = false, Transform parent = null, AudioMixerGroup mixer = null, bool spatialize = false)
		{
			AudioSource source = new GameObject(sourceName).AddComponent<AudioSource>();

			if (parent)
				source.transform.SetParent(parent, false);

			source.minDistance = minDistance;
			source.maxDistance = maxDistance;
			source.volume = volume;
			source.mute = mute;
			source.clip = clip;
			source.loop = loop && !destroyAfterFinished;
			source.outputAudioMixerGroup = mixer;
			source.spatialize = spatialize;
			source.spatialBlend = minDistance == 0 && maxDistance == 0 ? 0f : 1f;
			source.playOnAwake = playNow;

			if (playNow)
				source.Play();

			if (destroyAfterFinished)
			{
				if (clip)
					Destroy(source.gameObject, clip.length * 5f);
				else
					Destroy(false, source.gameObject);
			}

			return source;
		}
		/// <summary>
		/// Retrieves the names of all persistent method listeners attached to a UnityEvent.
		/// Useful for debugging or inspecting event connections at runtime.
		/// </summary>
		/// <param name="unityEvent">The UnityEvent to inspect.</param>
		/// <returns>An array of strings containing the names of all persistent methods attached to the event.</returns>
		public static string[] GetEventListeners(UnityEvent unityEvent)
		{
			List<string> result = new List<string>();

			for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
				result.Add(unityEvent.GetPersistentMethodName(i));

			return result.ToArray();
		}
		/// <summary>
		/// Creates a copy of a component on another GameObject by copying all property and field values.
		/// Uses reflection to access and copy both public and non-public members.
		/// Handles exceptions gracefully if certain properties or fields cannot be copied.
		/// </summary>
		/// <typeparam name="T">The type of component to clone.</typeparam>
		/// <param name="original">The source component to copy from.</param>
		/// <param name="destination">The GameObject to add the cloned component to.</param>
		/// <returns>The newly created component with copied values.</returns>
		public static T CloneComponent<T>(T original, GameObject destination) where T : Component
		{
			Type type = typeof(T);
			T target = destination.AddComponent<T>();
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
			PropertyInfo[] propertyInfoArray = type.GetProperties(flags);

			foreach (var propertyInfo in propertyInfoArray)
				if (propertyInfo.CanWrite)
					try
					{
						propertyInfo.SetValue(target, propertyInfo.GetValue(original, null), null);
					}
					catch { }

			FieldInfo[] fieldInfoArray = type.GetFields(flags);

			foreach (var fieldInfo in fieldInfoArray)
				try
				{
					fieldInfo.SetValue(target, fieldInfo.GetValue(original));
				}
				catch { }

			return target;
		}
		/// <summary>
		/// Extracts a single texture from a Texture2DArray at the specified index.
		/// Temporarily makes the array readable if needed, creates a new Texture2D with the same format,
		/// and copies the pixel data from the specified slice of the array.
		/// </summary>
		/// <param name="array">The Texture2DArray to extract from.</param>
		/// <param name="index">The index of the texture to extract.</param>
		/// <returns>A new Texture2D containing the extracted texture data.</returns>
		public static Texture2D GetTextureArrayItem(Texture2DArray array, int index)
		{
			bool isReadable = array.isReadable;

			if (!isReadable)
				array.Apply(false, false);

			Texture2D texture = new Texture2D(array.width, array.height, array.format, array.mipMapBias > 0f);

			texture.SetPixels32(array.GetPixels32(index));
			texture.Apply();

			if (!isReadable)
				array.Apply(false, true);

			return texture;
		}
		/// <summary>
		/// Extracts all textures from a Texture2DArray into an array of individual Texture2D objects.
		/// Temporarily makes the array readable if needed, creates new Texture2D objects with the same format,
		/// and copies the pixel data from each slice of the array.
		/// </summary>
		/// <param name="array">The Texture2DArray to extract from.</param>
		/// <returns>An array of Texture2D objects containing all the textures from the array.</returns>
		public static Texture2D[] GetTextureArrayItems(Texture2DArray array)
		{
			bool isReadable = array.isReadable;

			if (!isReadable)
				array.Apply(false, false);

			Texture2D[] textures = new Texture2D[array.depth];

			for (int i = 0; i < array.depth; i++)
			{
				textures[i] = new Texture2D(array.width, array.height, array.format, array.mipMapBias > 0f);

				textures[i].SetPixels32(array.GetPixels32(i));
				textures[i].Apply(true, false);
			}

			if (!isReadable)
				array.Apply(false, true);

			return textures;
		}
		/// <summary>
		/// Saves a Texture2D to a file in the specified format.
		/// Encodes the texture data according to the chosen format (PNG, JPG, TGA, or EXR),
		/// creates the directory if it doesn't exist, and writes the encoded bytes to the file.
		/// </summary>
		/// <param name="texture">The Texture2D to save.</param>
		/// <param name="type">The encoding format to use (PNG, JPG, TGA, or EXR).</param>
		/// <param name="path">The full path where the file should be saved.</param>
		public static void SaveTexture2D(Texture2D texture, TextureEncodingType type, string path)
		{
			if (!texture)
				return;

			byte[] bytes = type switch
			{
				TextureEncodingType.EXR => texture.EncodeToEXR(),
				TextureEncodingType.JPG => texture.EncodeToJPG(),
				TextureEncodingType.TGA => texture.EncodeToTGA(),
				_ => texture.EncodeToPNG(),
			};
			string fileName = Path.GetFileNameWithoutExtension(path);

			path = Path.GetDirectoryName(path);

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			File.WriteAllBytes($"{path}/{fileName}.{type.ToString().ToLower()}", bytes);
		}
		/// <summary>
		/// Takes a screenshot from a specific camera's view.
		/// Creates a temporary render texture, renders the camera to it, and copies the pixels
		/// to a new Texture2D. Cleans up the render targets afterward to prevent memory leaks.
		/// </summary>
		/// <param name="camera">The camera to capture the view from.</param>
		/// <param name="size">The resolution of the screenshot (width and height).</param>
		/// <param name="depth">The bit depth of the render texture (default is 24-bit color + 8-bit alpha).</param>
		/// <returns>A new Texture2D containing the captured screenshot.</returns>
		public static Texture2D TakeScreenshot(Camera camera, Vector2Int size, int depth = 24)
		{
			RenderTexture renderTexture = new RenderTexture(size.x, size.y, depth);

			camera.targetTexture = renderTexture;
			RenderTexture.active = renderTexture;

			Texture2D texture = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);

			camera.Render();
			texture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);

			camera.targetTexture = null;
			RenderTexture.active = null;

			return texture;
		}
		/// <summary>
		/// Converts a Texture2D to a Sprite.
		/// Creates a new Sprite using the provided texture, setting its pivot to the center
		/// and configuring the pixels per unit for proper scaling in 2D contexts.
		/// Returns null if the input is not a Texture2D.
		/// </summary>
		/// <param name="texture">The texture to convert to a Sprite.</param>
		/// <param name="pixelsPerUnit">The number of pixels that correspond to one unit in the world (affects scaling).</param>
		/// <returns>A new Sprite created from the texture, or null if the input is not a Texture2D.</returns>
		public static Sprite TextureToSprite(Texture texture, float pixelsPerUnit = 100f)
		{
			if (texture is Texture2D)
				return Sprite.Create(texture as Texture2D, new Rect(0f, 0f, texture.width, texture.height), new Vector2(.5f, .5f), pixelsPerUnit);

			return null;
		}
		/// <summary>
		/// Determines which render pipeline is currently active in the project.
		/// Checks the GraphicsSettings to identify if the project is using the Standard (Built-in) Render Pipeline,
		/// High Definition Render Pipeline (HDRP), Universal Render Pipeline (URP, formerly LWRP),
		/// or a custom render pipeline.
		/// </summary>
		/// <returns>The enum value representing the current render pipeline.</returns>
		public static RenderPipeline GetCurrentRenderPipeline()
		{
#if UNITY_2019_3_OR_NEWER
			if (!GraphicsSettings.currentRenderPipeline)
#else
			if (!GraphicsSettings.renderPipelineAsset)
#endif
				return RenderPipeline.Standard;
#if UNITY_2019_3_OR_NEWER
			else if (GraphicsSettings.currentRenderPipeline.GetType().Name.Contains("HDRenderPipelineAsset"))
#else
			else if (GraphicsSettings.renderPipelineAsset.GetType().Name.Contains("HDRenderPipelineAsset"))
#endif
				return RenderPipeline.HDRP;
#if UNITY_2019_3_OR_NEWER
			else if (GraphicsSettings.currentRenderPipeline.GetType().Name.Contains("LightweightRenderPipelineAsset") || GraphicsSettings.currentRenderPipeline.GetType().Name.Contains("UniversalRenderPipelineAsset"))
#else
			else if (GraphicsSettings.renderPipelineAsset.GetType().Name.Contains("LightweightRenderPipelineAsset") || GraphicsSettings.renderPipelineAsset.GetType().Name.Contains("UniversalRenderPipelineAsset"))
#endif
				return RenderPipeline.URP;
			else
				return RenderPipeline.Custom;
		}
		/// <summary>
		/// Finds all GameObjects in the active scene that match the specified layer names.
		/// Converts the layer names to a layer mask and calls the overloaded method.
		/// </summary>
		/// <param name="layers">Array of layer names to search for.</param>
		/// <param name="includeInactive">Whether to include inactive GameObjects in the search results.</param>
		/// <returns>An array of GameObjects that are on any of the specified layers.</returns>
		public static GameObject[] FindGameObjectsWithLayerMask(string[] layers, bool includeInactive = true)
		{
			return FindGameObjectsWithLayerMask(LayerMask.GetMask(layers), includeInactive);
		}
		/// <summary>
		/// Finds all GameObjects in the active scene that match the specified layer mask.
		/// Converts the LayerMask to its integer value and calls the overloaded method.
		/// </summary>
		/// <param name="layerMask">The LayerMask to filter GameObjects by.</param>
		/// <param name="includeInactive">Whether to include inactive GameObjects in the search results.</param>
		/// <returns>An array of GameObjects that match the layer mask.</returns>
		public static GameObject[] FindGameObjectsWithLayerMask(LayerMask layerMask, bool includeInactive = true)
		{
			return FindGameObjectsWithLayerMask(layerMask.value, includeInactive);
		}
		/// <summary>
		/// Finds all GameObjects in the active scene that match the specified layer mask value.
		/// Traverses the scene hierarchy to find all objects whose layer is included in the mask.
		/// </summary>
		/// <param name="layerMask">The integer value of the layer mask to filter GameObjects by.</param>
		/// <param name="includeInactive">Whether to include inactive GameObjects in the search results.</param>
		/// <returns>An array of GameObjects that match the layer mask value.</returns>
		public static GameObject[] FindGameObjectsWithLayerMask(int layerMask, bool includeInactive = true)
		{
			List<GameObject> gameObjects = new List<GameObject>();
			IEnumerable<GameObject> rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();

			foreach (GameObject rootGameObject in rootGameObjects)
			{
				IEnumerable<GameObject> children = rootGameObject.GetComponentsInChildren<Transform>().Select(transform => transform.gameObject);

				if (children.Count() > 0)
					gameObjects.AddRange(children.Where(gameObject => (includeInactive || gameObject.activeInHierarchy) && MaskHasLayer(layerMask, gameObject.layer)));
			}

			return gameObjects.ToArray();
		}
		/// <summary>
		/// Calculates the combined bounds of all physics colliders attached to a GameObject and its children.
		/// Can optionally include or exclude trigger colliders, preserve or reset rotation, and maintain or normalize scale.
		/// </summary>
		/// <param name="gameObject">The GameObject to calculate physics bounds for.</param>
		/// <param name="includeTriggers">Whether to include colliders marked as triggers in the bounds calculation.</param>
		/// <param name="keepRotation">Whether to calculate bounds with the object's current rotation (true) or reset to identity rotation (false).</param>
		/// <param name="keepScale">Whether to use the object's current scale (true) or normalize to unit scale (false) during calculation.</param>
		/// <returns>A Bounds object encompassing all included colliders, or a default (zero-sized) Bounds if no colliders are found.</returns>
		public static Bounds GetObjectPhysicsBounds(GameObject gameObject, bool includeTriggers, bool keepRotation = false, bool keepScale = true)
		{
			IEnumerable<Collider> colliders = gameObject.GetComponentsInChildren<Collider>();

			if (colliders.Count() > 0)
				colliders = colliders.Where(collider => includeTriggers || !collider.isTrigger);

			Bounds bounds = default;
			Quaternion orgRotation = gameObject.transform.rotation;
			Vector3 orgScale = gameObject.transform.localScale;

			if (!keepScale)
				gameObject.transform.localScale = Vector3.one;

			if (!keepRotation)
				gameObject.transform.rotation = Quaternion.identity;

			for (int i = 0; i < colliders.Count(); i++)
				if (bounds.size == Vector3.zero)
					bounds = colliders.ElementAt(i).bounds;
				else
					bounds.Encapsulate(colliders.ElementAt(i).bounds);

			if (!keepScale)
				gameObject.transform.localScale = orgScale;

			if (!keepRotation)
				gameObject.transform.rotation = orgRotation;

			return bounds;
		}
		/// <summary>
		/// Calculates the combined bounds of all renderers attached to a GameObject and its children.
		/// Excludes particle system renderers and trail renderers which can have dynamic or infinite bounds.
		/// Can optionally preserve or reset rotation and maintain or normalize scale during calculation.
		/// </summary>
		/// <param name="gameObject">The GameObject to calculate renderer bounds for.</param>
		/// <param name="keepRotation">Whether to calculate bounds with the object's current rotation (true) or reset to identity rotation (false).</param>
		/// <param name="keepScale">Whether to use the object's current scale (true) or normalize to unit scale (false) during calculation.</param>
		/// <returns>A Bounds object encompassing all included renderers, or a default (zero-sized) Bounds if no renderers are found.</returns>
		public static Bounds GetObjectBounds(GameObject gameObject, bool keepRotation = false, bool keepScale = true)
		{
			IEnumerable<Renderer> renderers = gameObject.GetComponentsInChildren<Renderer>();

			if (renderers.Count() > 0)
				renderers = renderers.Where(renderer => !(renderer is TrailRenderer || renderer is ParticleSystemRenderer));

			Bounds bounds = default;
			Quaternion orgRotation = gameObject.transform.rotation;
			Vector3 orgScale = gameObject.transform.localScale;

			if (!keepScale)
				gameObject.transform.localScale = Vector3.one;

			if (!keepRotation)
				gameObject.transform.rotation = Quaternion.identity;

			for (int i = 0; i < renderers.Count(); i++)
				if (bounds.size == Vector3.zero)
					bounds = renderers.ElementAt(i).bounds;
				else
					bounds.Encapsulate(renderers.ElementAt(i).bounds);

			if (!keepScale)
				gameObject.transform.localScale = orgScale;

			if (!keepRotation)
				gameObject.transform.rotation = orgRotation;

			return bounds;
		}
		/// <summary>
		/// Calculates the combined bounds of a UI element and all its children in world space.
		/// Takes into account the Canvas scale factor to properly size the bounds.
		/// Useful for determining the screen space occupied by a UI hierarchy.
		/// </summary>
		/// <param name="rectTransform">The root RectTransform to calculate bounds for.</param>
		/// <param name="scaleFactor">The Canvas scale factor to apply to the UI measurements.</param>
		/// <returns>A Bounds object encompassing the UI element and its children, or a default (zero-sized) Bounds if no valid RectTransform is found.</returns>
		public static Bounds GetUIBounds(RectTransform rectTransform, float scaleFactor)
		{
			Bounds bounds = default;

			if (!rectTransform)
				return bounds;

			RectTransform[] rectTransforms = rectTransform.GetComponentsInChildren<RectTransform>();

			if (rectTransforms.Length < 1)
				return bounds;

			for (int i = 0; i < rectTransforms.Length; i++)
			{
				Bounds newBounds = new Bounds(Divide(rectTransforms[i].position * scaleFactor, rectTransforms[i].lossyScale), new Vector3(rectTransforms[i].rect.width, rectTransforms[i].rect.height) * scaleFactor);

				if (i == 0)
					bounds = newBounds;
				else
					bounds.Encapsulate(newBounds);
			}

			return bounds;
		}
		/// <summary>
		/// Determines if a point in world space is inside a collider.
		/// Uses the collider's ClosestPoint method to check if the point is contained within the collider.
		/// </summary>
		/// <param name="collider">The collider to check against.</param>
		/// <param name="point">The world space point to test.</param>
		/// <returns>True if the point is inside the collider, false otherwise.</returns>
		public static bool CheckPointInCollider(Collider collider, Vector3 point)
		{
			return CheckPointInCollider(collider, point, out _);
		}
		/// <summary>
		/// Determines if a point in world space is inside a collider and provides the closest point on the collider.
		/// Uses the collider's ClosestPoint method to check if the point is contained within the collider.
		/// If the closest point equals the input point, the point is inside the collider.
		/// </summary>
		/// <param name="collider">The collider to check against.</param>
		/// <param name="point">The world space point to test.</param>
		/// <param name="closestPoint">Output parameter that returns the closest point on the collider's surface to the input point.</param>
		/// <returns>True if the point is inside the collider, false otherwise.</returns>
		public static bool CheckPointInCollider(Collider collider, Vector3 point, out Vector3 closestPoint)
		{
			closestPoint = collider.ClosestPoint(point);

			return closestPoint == point;
		}
		/// <summary>
		/// Destroys a Unity object either immediately or at the end of the current frame.
		/// Provides a unified interface for both immediate and deferred destruction.
		/// </summary>
		/// <param name="immediate">Whether to destroy the object immediately (true) or defer until the end of the frame (false).</param>
		/// <param name="obj">The Unity object to destroy.</param>
		public static void Destroy(bool immediate, UnityEngine.Object obj)
		{
			if (immediate)
				UnityEngine.Object.DestroyImmediate(obj);
			else
				UnityEngine.Object.Destroy(obj);
		}
		/// <summary>
		/// Destroys a Unity object after a specified time delay.
		/// Wraps Unity's Object.Destroy method with a time parameter.
		/// </summary>
		/// <param name="obj">The Unity object to destroy.</param>
		/// <param name="time">The delay in seconds before the object is destroyed.</param>
		public static void Destroy(UnityEngine.Object obj, float time)
		{
			UnityEngine.Object.Destroy(obj, time);
		}
		/// <summary>
		/// Immediately destroys a Unity object with an option to destroy assets.
		/// Wraps Unity's Object.DestroyImmediate method with the allowDestroyingAssets parameter.
		/// Use with caution as destroying assets can lead to data loss.
		/// </summary>
		/// <param name="obj">The Unity object to destroy.</param>
		/// <param name="allowDestroyingAssets">Whether to allow destruction of assets (objects stored in the project, not just in the scene).</param>
		public static void Destroy(UnityEngine.Object obj, bool allowDestroyingAssets)
		{
			UnityEngine.Object.DestroyImmediate(obj, allowDestroyingAssets);
		}

		/// <summary>
		/// Determines if a unit type is classified as a divider unit in unit conversion operations.
		/// Specifically identifies Units.FuelConsumption as a divider unit, which may require special
		/// handling in conversion calculations (such as inverting the conversion factor).
		/// This is used internally to properly handle unit conversions where the relationship
		/// between units involves division rather than multiplication.
		/// </summary>
		/// <param name="unit">The unit enumeration value to check.</param>
		/// <returns>True if the unit is a divider unit (currently only Units.FuelConsumption), false otherwise.</returns>
		private static bool IsDividerUnit(Units unit)
		{
			return unit == Units.FuelConsumption;
		}
		/// <summary>
		/// Calculates the signed area of a triangle formed by three 2D points.
		/// This function computes a value proportional to twice the signed area of the triangle,
		/// which can be used to determine the relative orientation of points:
		/// - Positive result: point3 is to the left of the line from point1 to point2
		/// - Negative result: point3 is to the right of the line from point1 to point2
		/// - Zero result: all three points are collinear
		/// 
		/// This is commonly used in computational geometry for point-in-polygon tests,
		/// determining if points are in clockwise/counterclockwise order, and checking
		/// if a point is on the left or right side of a directed line segment.
		/// </summary>
		/// <param name="point1">The first vertex of the triangle.</param>
		/// <param name="point2">The second vertex of the triangle.</param>
		/// <param name="point3">The third vertex of the triangle.</param>
		/// <returns>A value proportional to twice the signed area of the triangle formed by the three points.</returns>
		private static float PointSign(Vector2 point1, Vector2 point2, Vector2 point3)
		{
			return (point1.x - point3.x) * (point2.y - point3.y) - (point2.x - point3.x) * (point1.y - point3.y);
		}

		#endregion
	}
}
