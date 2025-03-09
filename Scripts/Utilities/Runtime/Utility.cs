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
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Finds a direct child of the transform with the specified name.
		/// </summary>
		/// <param name="transform">The transform to search in.</param>
		/// <param name="name">The name of the child to find.</param>
		/// <param name="caseSensitive">Whether the name comparison should be case-sensitive.</param>
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
		/// </summary>
		/// <param name="transform">The transform to search in.</param>
		/// <param name="name">The prefix to search for.</param>
		/// <param name="caseSensitive">Whether the name comparison should be case-sensitive.</param>
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
		/// </summary>
		/// <param name="transform">The transform to search in.</param>
		/// <param name="name">The suffix to search for.</param>
		/// <param name="caseSensitive">Whether the name comparison should be case-sensitive.</param>
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
		/// </summary>
		/// <param name="transform">The transform to search in.</param>
		/// <param name="name">The substring to search for.</param>
		/// <param name="caseSensitive">Whether the name comparison should be case-sensitive.</param>
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
		/// </summary>
		/// <param name="curve">The animation curve to clamp.</param>
		/// <returns>A new animation curve with clamped keyframes.</returns>
		public static AnimationCurve Clamp01(this AnimationCurve curve)
		{
			return curve.Clamp(0f, 1f, 0f, 1f);
		}
		/// <summary>
		/// Clamps all keyframes in the animation curve to the range defined by min and max keyframes.
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
		/// </summary>
		/// <param name="str">The string to test.</param>
		/// <returns>true if the string is null or empty; otherwise, false.</returns>
		public static bool IsNullOrEmpty(this string str)
		{
			return string.IsNullOrEmpty(str);
		}
		/// <summary>
		/// Determines whether the specified string is null, empty, or consists only of white-space characters.
		/// </summary>
		/// <param name="str">The string to test.</param>
		/// <returns>true if the string is null, empty, or consists only of white-space characters; otherwise, false.</returns>
		public static bool IsNullOrWhiteSpace(this string str)
		{
			return string.IsNullOrWhiteSpace(str);
		}
		/// <summary>
		/// Concatenates the elements of a string collection, using the specified separator between each element.
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
	/// </summary>
	public static class Utility
	{
		#region Modules & Enumerators

		#region Enumerators

		/// <summary>
		/// Defines precision levels for calculations and display.
		/// </summary>
		public enum Precision { Simple, Advanced }
		/// <summary>
		/// Defines unit systems for measurements.
		/// </summary>
		public enum UnitType { Metric, Imperial }
		/// <summary>
		/// Defines various unit types for different physical quantities.
		/// </summary>
		public enum Units { AngularVelocity, Area, AreaAccurate, AreaLarge, ElectricConsumption, Density, Distance, DistanceAccurate, DistanceLong, ElectricCapacity, Force, Frequency, FuelConsumption, Liquid, Power, Pressure, Size, SizeAccurate, Speed, Time, TimeAccurate, Torque, Velocity, Volume, VolumeAccurate, VolumeLarge, Weight }
		/// <summary>
		/// Defines the available render pipelines in Unity.
		/// </summary>
		public enum RenderPipeline { Unknown = -1, Standard, URP, HDRP, Custom }
		/// <summary>
		/// Defines texture encoding formats for saving textures.
		/// </summary>
		public enum TextureEncodingType { EXR, JPG, PNG, TGA }
		/// <summary>
		/// Defines relative positions in world space.
		/// </summary>
		public enum WorldSide { Left = -1, Center, Right }
		/// <summary>
		/// Defines coordinate planes in 3D space.
		/// </summary>
		public enum WorldSurface { XY, XZ, YZ }
		/// <summary>
		/// Defines axes in 2D space.
		/// </summary>
		public enum Axis2 { X, Y }
		/// <summary>
		/// Defines axes in 3D space.
		/// </summary>
		public enum Axis3 { X, Y, Z }
		/// <summary>
		/// Defines axes in 4D space.
		/// </summary>
		public enum Axis4 { X, Y, Z, W }

		#endregion

		#region Modules

		#region Static Modules

		/// <summary>
		/// Provides additional color constants not available in UnityEngine.Color.
		/// </summary>
		public static class Color
		{
			/// <summary>
			/// A dark gray color (R:0.25, G:0.25, B:0.25, A:1.0).
			/// </summary>
			public static UnityEngine.Color darkGray = new UnityEngine.Color(.25f, .25f, .25f);
			/// <summary>
			/// A light gray color (R:0.67, G:0.67, B:0.67, A:1.0).
			/// </summary>
			public static UnityEngine.Color lightGray = new UnityEngine.Color(.67f, .67f, .67f);
			/// <summary>
			/// An orange color (R:1.0, G:0.5, B:0.0, A:1.0).
			/// </summary>
			public static UnityEngine.Color orange = new UnityEngine.Color(1f, .5f, 0f);
			/// <summary>
			/// A purple color (R:0.5, G:0.0, B:1.0, A:1.0).
			/// </summary>
			public static UnityEngine.Color purple = new UnityEngine.Color(.5f, 0f, 1f);
			/// <summary>
			/// A fully transparent color (R:0.0, G:0.0, B:0.0, A:0.0).
			/// </summary>
			public static UnityEngine.Color transparent = new UnityEngine.Color(0f, 0f, 0f, 0f);
		}
		/// <summary>
		/// Provides interpolation formulas for smooth transitions.
		/// </summary>
		public static class FormulaInterpolation
		{
			/// <summary>
			/// Linear interpolation formula that clamps the input parameter between 0 and 1.
			/// </summary>
			/// <param name="t">The interpolation parameter (0 to 1).</param>
			/// <returns>The clamped value of t.</returns>
			public static float Linear(float t)
			{
				return Clamp01(t);
			}
			/// <summary>
			/// Circular interpolation formula that starts slow and ends fast.
			/// </summary>
			/// <param name="t">The interpolation parameter (0 to 1).</param>
			/// <returns>The interpolated value using a circular function.</returns>
			public static float CircularLowToHigh(float t)
			{
				return Clamp01(1f - math.pow(math.cos(math.PI * Mathf.Rad2Deg * Clamp01(t) * .5f), .5f));
			}
			/// <summary>
			/// Circular interpolation formula that starts fast and ends slow.
			/// </summary>
			/// <param name="t">The interpolation parameter (0 to 1).</param>
			/// <returns>The interpolated value using a circular function.</returns>
			public static float CircularHighToLow(float t)
			{
				return Clamp01(math.pow(math.abs(math.sin(math.PI * Mathf.Rad2Deg * Clamp01(t) * .5f)), .5f));
			}
		}

		#endregion

		#region Global Modules

		/// <summary>
		/// A serializable wrapper for arrays that can be used in JSON serialization.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array.</typeparam>
		[Serializable]
		public class JsonArray<T>
		{
			#region Variables

			#region  Global Variables

			/// <summary>
			/// Gets the length of the array. Returns 0 if the array is null.
			/// </summary>
			public int Length => items != null ? items.Length : 0;

			[SerializeField]
#pragma warning disable IDE0044 // Add readonly modifier
			private T[] items;
#pragma warning restore IDE0044 // Add readonly modifier

			#endregion

			#region Indexers

			/// <summary>
			/// Gets or sets the element at the specified index.
			/// </summary>
			/// <param name="index">The index of the element to get or set.</param>
			/// <returns>The element at the specified index.</returns>
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
			/// </summary>
			/// <returns>An enumerator that can be used to iterate through the array.</returns>
			public IEnumerator GetEnumerator()
			{
				return items.GetEnumerator();
			}
			/// <summary>
			/// Converts the JsonArray to a regular array.
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
		/// </summary>
		[Serializable]
		public struct Interval
		{
			#region Variables

			/// <summary>
			/// Gets or sets the minimum value of the interval.
			/// When setting, the value is clamped to ensure it doesn't exceed the maximum value unless OverrideBorders is true.
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

			[SerializeField]
			private float min;
			[SerializeField]
			private float max;
			[SerializeField, MarshalAs(UnmanagedType.U1)]
			private bool overrideBorders;
			[SerializeField, MarshalAs(UnmanagedType.U1)]
			private bool clampToZero;

			#endregion

			#region Methods

			#region Virtual Methods

			/// <summary>
			/// Determines whether the specified object is equal to the current Interval.
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
			/// </summary>
			/// <param name="value">The value to check.</param>
			/// <returns>true if the value is within the interval range; otherwise, false.</returns>
			public readonly bool InRange(int value)
			{
				return value >= min && value <= max;
			}
			
			/// <summary>
			/// Determines whether the specified float value is within the interval range.
			/// </summary>
			/// <param name="value">The value to check.</param>
			/// <returns>true if the value is within the interval range; otherwise, false.</returns>
			public readonly bool InRange(float value)
			{
				return value >= min && value <= max;
			}
			
			/// <summary>
			/// Linearly interpolates between the minimum and maximum values of the interval.
			/// </summary>
			/// <param name="time">The interpolation parameter (0 to 1).</param>
			/// <param name="clamped">Whether to clamp the interpolation parameter between 0 and 1.</param>
			/// <returns>The interpolated value.</returns>
			public readonly float Lerp(int time, bool clamped = true)
			{
				return clamped ? Utility.Lerp(Min, Max, time) : LerpUnclamped(Min, Max, time);
			}
			
			/// <summary>
			/// Linearly interpolates between the minimum and maximum values of the interval.
			/// </summary>
			/// <param name="time">The interpolation parameter (0 to 1).</param>
			/// <param name="clamped">Whether to clamp the interpolation parameter between 0 and 1.</param>
			/// <returns>The interpolated value.</returns>
			public readonly float Lerp(float time, bool clamped = true)
			{
				return clamped ? Utility.Lerp(Min, Max, time) : LerpUnclamped(Min, Max, time);
			}
			
			/// <summary>
			/// Calculates the interpolation parameter that would result in the specified value when linearly interpolating between the minimum and maximum values of the interval.
			/// </summary>
			/// <param name="value">The value to find the interpolation parameter for.</param>
			/// <param name="clamped">Whether to clamp the result between 0 and 1.</param>
			/// <returns>The interpolation parameter.</returns>
			public readonly float InverseLerp(int value, bool clamped = true)
			{
				return clamped ? Utility.InverseLerp(Min, Max, value) : InverseLerpUnclamped(Min, Max, value);
			}
			
			/// <summary>
			/// Calculates the interpolation parameter that would result in the specified value when linearly interpolating between the minimum and maximum values of the interval.
			/// </summary>
			/// <param name="value">The value to find the interpolation parameter for.</param>
			/// <param name="clamped">Whether to clamp the result between 0 and 1.</param>
			/// <returns>The interpolation parameter.</returns>
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
			public Interval(Interval interval)
			{
				min = interval.Min;
				max = interval.Max;
				overrideBorders = interval.OverrideBorders;
				clampToZero = interval.clampToZero;
			}

			#endregion

			#region Operators

			public static Interval operator +(Interval a, float b)
			{
				return new Interval(a.min + b, a.max + b);
			}
			public static Interval operator +(Interval a, Interval b)
			{
				return new Interval(a.min + b.min, a.max + b.max);
			}
			public static Interval operator -(Interval a, float b)
			{
				return new Interval(a.min - b, a.max - b);
			}
			public static Interval operator -(Interval a, Interval b)
			{
				return new Interval(a.min - b.min, a.max - b.max);
			}
			public static Interval operator *(Interval a, float b)
			{
				return new Interval(a.min * b, a.max * b);
			}
			public static Interval operator *(Interval a, Interval b)
			{
				return new Interval(a.min * b.min, a.max * b.max);
			}
			public static Interval operator /(Interval a, float b)
			{
				return new Interval(a.min / b, a.max / b);
			}
			public static Interval operator /(Interval a, Interval b)
			{
				return new Interval(a.min / b.min, a.max / b.max);
			}
			public static bool operator ==(Interval a, Interval b)
			{
				return a.Equals(b);
			}
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
		/// </summary>
		[Serializable]
		public struct SimpleInterval
		{
			#region Variables

			public readonly float Length => math.abs(a - b);

			public float a;
			public float b;

			#endregion

			#region Methods

			#region Virtual Methods

			public override readonly bool Equals(object obj)
			{
				return obj is SimpleInterval interval &&
						a == interval.a &&
						b == interval.b;
			}
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

			public readonly bool InRange(float value)
			{
				return value >= math.min(a, b) && value <= math.max(a, b);
			}
			public readonly float Lerp(float value, bool clamped)
			{
				return clamped ? Lerp(value) : LerpUnclamped(value);
			}
			public readonly float Lerp(float time)
			{
				return Utility.Lerp(a, b, time);
			}
			public readonly float LerpUnclamped(float time)
			{
				return Utility.LerpUnclamped(a, b, time);
			}
			public readonly float InverseLerp(float value, bool clamped)
			{
				return clamped ? InverseLerp(value) : InverseLerpUnclamped(value);
			}
			public readonly float InverseLerp(float value)
			{
				return Utility.InverseLerp(a, b, value);
			}
			public readonly float InverseLerpUnclamped(float value)
			{
				return Utility.InverseLerpUnclamped(a, b, value);
			}
			public readonly float Clamp(float value)
			{
				return math.clamp(value, math.min(a, b), math.max(a, b));
			}
			public readonly float Random()
			{
				return UnityEngine.Random.Range(math.min(a, b), math.max(a, b));
			}
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

			public SimpleInterval(float a, float b)
			{
				this.a = a;
				this.b = b;
			}
			public SimpleInterval(SimpleInterval interval)
			{
				a = interval.a;
				b = interval.b;
			}
			public SimpleInterval(Interval interval)
			{
				a = interval.Min;
				b = interval.Max;
			}

			#endregion

			#region Operators

			public static SimpleInterval operator +(SimpleInterval x, float y)
			{
				return new SimpleInterval(x.a + y, x.b + y);
			}
			public static SimpleInterval operator +(SimpleInterval x, SimpleInterval y)
			{
				return new SimpleInterval(x.a + y.a, x.b + y.b);
			}
			public static SimpleInterval operator -(SimpleInterval x, float y)
			{
				return new SimpleInterval(x.a - y, x.b - y);
			}
			public static SimpleInterval operator -(SimpleInterval x, SimpleInterval y)
			{
				return new SimpleInterval(x.a - y.a, x.b - y.b);
			}
			public static SimpleInterval operator *(SimpleInterval x, float y)
			{
				return new SimpleInterval(x.a * y, x.b * y);
			}
			public static SimpleInterval operator *(SimpleInterval x, SimpleInterval y)
			{
				return new SimpleInterval(x.a * y.a, x.b * y.b);
			}
			public static SimpleInterval operator /(SimpleInterval x, float y)
			{
				return new SimpleInterval(x.a / y, x.b / y);
			}
			public static SimpleInterval operator /(SimpleInterval x, SimpleInterval y)
			{
				return new SimpleInterval(x.a / y.a, x.b / y.b);
			}
			public static bool operator ==(SimpleInterval x, SimpleInterval y)
			{
				return x.Equals(y);
			}
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

			[SerializeField]
			private int min;
			[SerializeField]
			private int max;
			[SerializeField, MarshalAs(UnmanagedType.U1)]
			private bool overrideBorders;
			[SerializeField, MarshalAs(UnmanagedType.U1)]
			private bool clampToZero;

			#endregion

			#region Methods

			#region Virtual Methods

			public override readonly bool Equals(object obj)
			{
				return obj is IntervalInt interval &&
					   min == interval.min &&
					   max == interval.max &&
					   overrideBorders == interval.overrideBorders &&
					   clampToZero == interval.clampToZero;
			}
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

			public readonly bool InRange(int value)
			{
				return value >= min && value <= max;
			}
			public readonly bool InRange(float value)
			{
				return value >= min && value <= max;
			}
			public readonly float Lerp(int time, bool clamped = true)
			{
				return clamped ? Utility.Lerp(Min, Max, time) : LerpUnclamped(Min, Max, time);
			}
			public readonly float Lerp(float time, bool clamped = true)
			{
				return clamped ? Utility.Lerp(Min, Max, time) : LerpUnclamped(Min, Max, time);
			}
			public readonly float InverseLerp(int value, bool clamped = true)
			{
				return clamped ? Utility.InverseLerp(Min, Max, value) : InverseLerpUnclamped(Min, Max, value);
			}
			public readonly float InverseLerp(float value, bool clamped = true)
			{
				return clamped ? Utility.InverseLerp(Min, Max, value) : InverseLerpUnclamped(Min, Max, value);
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			public IntervalInt(int min, int max, bool overrideBorders = false, bool clampToZero = false)
			{
				this.min = (int)math.clamp(min, clampToZero ? 0f : -math.INFINITY, overrideBorders ? math.INFINITY : max);
				this.max = (int)math.clamp(max, overrideBorders ? -math.INFINITY : min, math.INFINITY);
				this.overrideBorders = overrideBorders;
				this.clampToZero = clampToZero;
			}
			public IntervalInt(IntervalInt interval)
			{
				min = interval.Min;
				max = interval.Max;
				overrideBorders = interval.OverrideBorders;
				clampToZero = interval.clampToZero;
			}

			#endregion

			#region Operators

			public static IntervalInt operator +(IntervalInt a, int b)
			{
				return new IntervalInt(a.min + b, a.max + b);
			}
			public static IntervalInt operator +(IntervalInt a, IntervalInt b)
			{
				return new IntervalInt(a.min + b.min, a.max + b.max);
			}
			public static IntervalInt operator -(IntervalInt a, int b)
			{
				return new IntervalInt(a.min - b, a.max - b);
			}
			public static IntervalInt operator -(IntervalInt a, IntervalInt b)
			{
				return new IntervalInt(a.min - b.min, a.max - b.max);
			}
			public static IntervalInt operator *(IntervalInt a, int b)
			{
				return new IntervalInt(a.min * b, a.max * b);
			}
			public static IntervalInt operator *(IntervalInt a, IntervalInt b)
			{
				return new IntervalInt(a.min * b.min, a.max * b.max);
			}
			public static IntervalInt operator /(IntervalInt a, int b)
			{
				return new IntervalInt(a.min / b, a.max / b);
			}
			public static IntervalInt operator /(IntervalInt a, IntervalInt b)
			{
				return new IntervalInt(a.min / b.min, a.max / b.max);
			}
			public static bool operator ==(IntervalInt a, IntervalInt b)
			{
				return a.Equals(b);
			}
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

			public Interval x;
			public Interval y;

			#endregion

			#region Methods

			#region Virtual Methods

			public override readonly bool Equals(object obj)
			{
				return obj is Interval2 interval &&
					   EqualityComparer<Interval>.Default.Equals(x, interval.x) &&
					   EqualityComparer<Interval>.Default.Equals(y, interval.y);
			}
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

			public readonly bool InRange(float value)
			{
				return x.InRange(value) && y.InRange(value);
			}
			public readonly bool InRange(Vector2 value)
			{
				return x.InRange(value.x) && y.InRange(value.y);
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			public Interval2(float xMin, float xMax, float yMin, float yMax)
			{
				x = new Interval(xMin, xMax);
				y = new Interval(yMin, yMax);
			}
			public Interval2(Interval x, Interval y)
			{
				this.x = x;
				this.y = y;
			}
			public Interval2(Interval2 interval)
			{
				x = new Interval(interval.x);
				y = new Interval(interval.y);
			}

			#endregion

			#region Operators

			public static bool operator ==(Interval2 a, Interval2 b)
			{
				return a.Equals(b);
			}
			public static bool operator !=(Interval2 a, Interval2 b)
			{
				return !(a == b);
			}
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

			public static readonly SimpleInterval2 Empty = new SimpleInterval2(0f, 0f, 0f, 0f);

			#endregion

			#region Variables

			#region Properties

			public readonly Vector2 CenterVector2 => new Vector2((x.a + x.b) * .5f, (y.a + y.b) * .5f);
			public readonly float2 CenterFloat2 => new float2((x.a + x.b) * .5f, (y.a + y.b) * .5f);

			#endregion

			#region Fields

			public SimpleInterval x;
			public SimpleInterval y;

			#endregion

			#endregion

			#region Methods

			#region Virtual Methods

			public readonly override bool Equals(object obj)
			{
				return obj is SimpleInterval2 interval2 &&
					x.Equals(interval2.x) &&
					y.Equals(interval2.y);
			}
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

			public readonly bool InRange(float value)
			{
				return x.InRange(value) && y.InRange(value);
			}
			public readonly bool InRange(Vector2 value)
			{
				return x.InRange(value.x) && y.InRange(value.y);
			}
			public readonly bool InRange(float2 value)
			{
				return x.InRange(value.x) && y.InRange(value.y);
			}
			public void Encapsulate(Vector2 value)
			{
				x.Encapsulate(value.x);
				y.Encapsulate(value.y);
			}
			public void Encapsulate(float2 value)
			{
				x.Encapsulate(value.x);
				y.Encapsulate(value.y);
			}
			public void Encapsulate(float x, float y)
			{
				this.x.Encapsulate(x);
				this.y.Encapsulate(y);
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			public SimpleInterval2(float xA, float xB, float yA, float yB)
			{
				x = new SimpleInterval(xA, xB);
				y = new SimpleInterval(yA, yB);
			}
			public SimpleInterval2(SimpleInterval x, SimpleInterval y)
			{
				this.x = x;
				this.y = y;
			}
			public SimpleInterval2(SimpleInterval2 interval)
			{
				x = new SimpleInterval(interval.x);
				y = new SimpleInterval(interval.y);
			}

			#endregion

			#region Operators

			public static bool operator ==(SimpleInterval2 a, SimpleInterval2 b)
			{
				return a.Equals(b);
			}
			public static bool operator !=(SimpleInterval2 a, SimpleInterval2 b)
			{
				return !(a == b);
			}
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

			public Interval x;
			public Interval y;
			public Interval z;

			#endregion

			#region Methods

			#region Virtual Methods

			public override readonly bool Equals(object obj)
			{
				return obj is Interval3 interval &&
					   EqualityComparer<Interval>.Default.Equals(x, interval.x) &&
					   EqualityComparer<Interval>.Default.Equals(y, interval.y) &&
					   EqualityComparer<Interval>.Default.Equals(z, interval.z);
			}
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

			public readonly bool InRange(float value)
			{
				return x.InRange(value) && y.InRange(value) && z.InRange(value);
			}
			public readonly bool InRange(Vector3 value)
			{
				return x.InRange(value.x) && y.InRange(value.y) && z.InRange(value.z);
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			public Interval3(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
			{
				x = new Interval(xMin, xMax);
				y = new Interval(yMin, yMax);
				z = new Interval(zMin, zMax);
			}
			public Interval3(Interval x, Interval y, Interval z)
			{
				this.x = x;
				this.y = y;
				this.z = z;
			}
			public Interval3(Interval3 interval)
			{
				x = new Interval(interval.x);
				y = new Interval(interval.y);
				z = new Interval(interval.z);
			}

			#endregion

			#region Operators

			public static bool operator ==(Interval3 a, Interval3 b)
			{
				return a.Equals(b);
			}
			public static bool operator !=(Interval3 a, Interval3 b)
			{
				return !(a == b);
			}
			public static implicit operator Interval2(Interval3 interval)
			{
				return new Interval2(interval.x, interval.y);
			}
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

			public static readonly SimpleInterval3 Empty = new SimpleInterval3(0f, 0f, 0f, 0f, 0f, 0f);

			#endregion

			#region Variables

			#region Properties

			public readonly Vector3 CenterVector3 => new Vector3((x.a + x.b) * .5f, (y.a + y.b) * .5f, (z.a + z.b) * .5f);
			public readonly float3 CenterFloat3 => new float3((x.a + x.b) * .5f, (y.a + y.b) * .5f, (z.a + z.b) * .5f);

			#endregion

			#region Fields

			public SimpleInterval x;
			public SimpleInterval y;
			public SimpleInterval z;

			#endregion

			#endregion

			#region Methods

			#region Virtual Methods

			public readonly override bool Equals(object obj)
			{
				return obj is SimpleInterval3 interval3 &&
					x.Equals(interval3.x) &&
					y.Equals(interval3.y) &&
					z.Equals(interval3.z);
			}
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

			public readonly bool InRange(float value)
			{
				return x.InRange(value) && y.InRange(value);
			}
			public readonly bool InRange(Vector3 value)
			{
				return x.InRange(value.x) && y.InRange(value.y) && z.InRange(value.z);
			}
			public readonly bool InRange(float3 value)
			{
				return x.InRange(value.x) && y.InRange(value.y) && z.InRange(value.z);
			}
			public void Encapsulate(Vector3 value)
			{
				x.Encapsulate(value.x);
				y.Encapsulate(value.y);
				z.Encapsulate(value.z);
			}
			public void Encapsulate(float3 value)
			{
				x.Encapsulate(value.x);
				y.Encapsulate(value.y);
				z.Encapsulate(value.z);
			}
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

			public SimpleInterval3(float xA, float xB, float yA, float yB, float zA, float zB)
			{
				x = new SimpleInterval(xA, xB);
				y = new SimpleInterval(yA, yB);
				z = new SimpleInterval(zA, zB);
			}
			public SimpleInterval3(float xA, float xB, float yA, float yB)
			{
				x = new SimpleInterval(xA, xB);
				y = new SimpleInterval(yA, yB);
				z = new SimpleInterval(0f, 0f);
			}
			public SimpleInterval3(SimpleInterval x, SimpleInterval y, SimpleInterval z)
			{
				this.x = x;
				this.y = y;
				this.z = z;
			}
			public SimpleInterval3(SimpleInterval3 interval)
			{
				x = new SimpleInterval(interval.x);
				y = new SimpleInterval(interval.y);
				z = new SimpleInterval(interval.z);
			}

			#endregion

			#region Operators

			public static bool operator ==(SimpleInterval3 a, SimpleInterval3 b)
			{
				return a.Equals(b);
			}
			public static bool operator !=(SimpleInterval3 a, SimpleInterval3 b)
			{
				return !(a == b);
			}
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
		/// </summary>
		[Serializable]
		public struct SerializableVector2
		{
			#region Variables

			public float x;
			public float y;

			#endregion

			#region Constructors & Operators

			#region Constructors

			public SerializableVector2(float x, float y)
			{
				this.x = x;
				this.y = y;
			}
			public SerializableVector2(Vector2 vector)
			{
				x = vector.x;
				y = vector.y;
			}
			public SerializableVector2(float2 vector)
			{
				x = vector.x;
				y = vector.y;
			}

			#endregion

			#region Operators

			public static SerializableVector2 operator *(SerializableVector2 a, float b)
			{
				return new SerializableVector2(new Vector2(a.x, a.y) * b);
			}
			public static SerializableVector2 operator +(SerializableVector2 a, float b)
			{
				return new SerializableVector2(new Vector2(a.x + b, a.y + b));
			}
			public static SerializableVector2 operator *(SerializableVector2 a, SerializableVector2 b)
			{
				return new SerializableVector2(a.x * b.x, a.y * b.y);
			}
			public static SerializableVector2 operator +(SerializableVector2 a, SerializableVector2 b)
			{
				return new SerializableVector2(a.x + b.x, a.y + b.y);
			}
			public static implicit operator Vector2(SerializableVector2 vector)
			{
				return new Vector2(vector.x, vector.y);
			}
			public static implicit operator SerializableVector2(Vector2 vector)
			{
				return new SerializableVector2(vector);
			}
			public static implicit operator float2(SerializableVector2 vector)
			{
				return new float2(vector.x, vector.y);
			}
			public static implicit operator SerializableVector2(float2 vector)
			{
				return new SerializableVector2(vector);
			}

			#endregion

			#endregion
		}
		/// <summary>
		/// Represents a 2D rectangle with separate X, Y, width, and height components.
		/// Provides methods for rectangle operations and serialization.
		/// </summary>
		[Serializable]
		public struct SerializableRect
		{
			#region Variables

			public float x;
			public float y;
			public float width;
			public float height;
			public SerializableVector2 position;
			public SerializableVector2 size;

			#endregion

			#region Methods

			public readonly bool Contains(Vector2 point)
			{
				return new Rect(x, y, width, height).Contains(point);
			}
			public readonly bool Contains(Vector3 point)
			{
				return new Rect(x, y, width, height).Contains(point);
			}
			public readonly bool Contains(Vector3 point, bool allowInverse)
			{
				return new Rect(x, y, width, height).Contains(point, allowInverse);
			}

			#endregion

			#region Constructors & Operators

			#region Constructors

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

			public static implicit operator Rect(SerializableRect rect)
			{
				return new Rect(rect.x, rect.y, rect.width, rect.height);
			}
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
		/// </summary>
		[Serializable]
		public struct ColorSheet
		{
			#region Variables

			public string name;
			public SerializableColor color;
			public float metallic;
			public float smoothness;

			#endregion

			#region Methods

			#region Virtual Methods

			public override readonly bool Equals(object obj)
			{
				return obj is ColorSheet sheet &&
					name == sheet.name &&
					EqualityComparer<SerializableColor>.Default.Equals(color, sheet.color) &&
					metallic == sheet.metallic &&
					smoothness == sheet.smoothness;
			}
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

			public ColorSheet(string name)
			{
				this.name = name;
				color = UnityEngine.Color.white;
				metallic = 0f;
				smoothness = .5f;
			}
			public ColorSheet(ColorSheet sheet)
			{
				name = sheet.name;
				color = sheet.color;
				metallic = sheet.metallic;
				smoothness = sheet.smoothness;
			}

			#endregion

			#region Operators

			public static implicit operator ColorSheet(UnityEngine.Color color)
			{
				return new ColorSheet()
				{
					color = color
				};
			}
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
			public static bool operator ==(ColorSheet sheetA, ColorSheet sheetB)
			{
				return sheetA.name == sheetB.name && sheetA.color == sheetB.color && sheetA.metallic == sheetB.metallic && sheetA.smoothness == sheetB.smoothness;
			}
			public static bool operator !=(ColorSheet sheetA, ColorSheet sheetB)
			{
				return !(sheetA == sheetB);
			}

			#endregion

			#endregion
		}
		/// <summary>
		/// Represents a color with separate red, green, blue, and alpha components.
		/// Provides methods for color comparison and serialization.
		/// </summary>
		[Serializable]
		public struct SerializableColor
		{
			#region Variables

			public float r;
			public float g;
			public float b;
			public float a;

			#endregion

			#region Methods

			public override readonly bool Equals(object obj)
			{
				bool equalsColor = obj is SerializableColor color && r == color.r && g == color.g && b == color.b && a == color.a;
				bool equalsUColor = obj is UnityEngine.Color uColor && r == uColor.r && g == uColor.g && b == uColor.b && a == uColor.a;

				return equalsColor || equalsUColor;
			}
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

			public SerializableColor(UnityEngine.Color color)
			{
				r = color.r;
				g = color.g;
				b = color.b;
				a = color.a;
			}

			#endregion

			#region Operators

			public static implicit operator UnityEngine.Color(SerializableColor color)
			{
				return new UnityEngine.Color(color.r, color.g, color.b, color.a);
			}
			public static implicit operator SerializableColor(UnityEngine.Color color)
			{
				return new SerializableColor(color);
			}
			public static bool operator ==(SerializableColor colorA, SerializableColor colorB)
			{
				return colorA.Equals(colorB);
			}
			public static bool operator !=(SerializableColor colorA, SerializableColor colorB)
			{
				return !(colorA == colorB);
			}
			public static bool operator ==(SerializableColor colorA, UnityEngine.Color colorB)
			{
				return colorA.Equals(colorB);
			}
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
		/// </summary>
		[Serializable]
		public class SerializableAudioClip
		{
			#region Variables

			public string resourcePath;
			public AudioClip Clip
			{
				get
				{
					if (!clip || resourcePath != path)
						Reload();

					return clip;
				}
			}

			private AudioClip clip;
			private string path;

			#endregion

			#region Methods

			#region Virtual Methods

			public override bool Equals(object obj)
			{
				bool equalsClip = obj is SerializableAudioClip clip && clip.Clip == Clip;
				bool equalsUClip = obj is AudioClip uClip && uClip == Clip;

				return equalsClip || equalsUClip;
			}
			public override int GetHashCode()
			{
				return -2053173677 + EqualityComparer<AudioClip>.Default.GetHashCode(Clip);
			}

			#endregion

			#region Global Methods

			private void Reload()
			{
				path = resourcePath;
				clip = Resources.Load(path) as AudioClip;
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			public SerializableAudioClip(string path)
			{
				resourcePath = path;

				Reload();
			}

			#endregion

			#region Operators

			public static implicit operator bool(SerializableAudioClip audioClip) => audioClip != null;
			public static implicit operator AudioClip(SerializableAudioClip audioClip) => audioClip.Clip;
			public static bool operator ==(SerializableAudioClip clipA, SerializableAudioClip clipB)
			{
				return clipA.Equals(clipB);
			}
			public static bool operator !=(SerializableAudioClip clipA, SerializableAudioClip clipB)
			{
				return !(clipA == clipB);
			}
			public static bool operator ==(SerializableAudioClip clipA, AudioClip clipB)
			{
				return clipA.Equals(clipB);
			}
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

			public string resourcePath;
			public ParticleSystem Particle
			{
				get
				{
					if (!particle || resourcePath != path)
						Reload();

					return particle;
				}
			}

			private ParticleSystem particle;
			private string path;

			#endregion

			#region Methods

			#region Virtual Methods

			public override bool Equals(object obj)
			{
				bool equalsParticle = obj is SerializableParticleSystem particle && particle.Particle == Particle;
				bool equalsUParticle = obj is ParticleSystem uParticle && uParticle == Particle;

				return equalsParticle || equalsUParticle;
			}
			public override int GetHashCode()
			{
				return 1500868535 + EqualityComparer<ParticleSystem>.Default.GetHashCode(Particle);
			}

			#endregion

			#region Global Methods

			private void Reload()
			{
				path = resourcePath;
				particle = Resources.Load(path) as ParticleSystem;
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			public SerializableParticleSystem(string path)
			{
				resourcePath = path;

				Reload();
			}

			#endregion

			#region Operators

			public static implicit operator bool(SerializableParticleSystem particleSystem) => particleSystem != null;
			public static implicit operator ParticleSystem(SerializableParticleSystem particleSystem) => particleSystem.Particle;
			public static bool operator ==(SerializableParticleSystem particleA, SerializableParticleSystem particleB)
			{
				return particleA.Equals(particleB);
			}
			public static bool operator !=(SerializableParticleSystem particleA, SerializableParticleSystem particleB)
			{
				return !(particleA == particleB);
			}
			public static bool operator ==(SerializableParticleSystem particleA, ParticleSystem particleB)
			{
				return particleA.Equals(particleB);
			}
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

			public string resourcePath;
			public Material Material
			{
				get
				{
					if (!material || resourcePath != path)
						Reload();

					return material;
				}
			}

			private Material material;
			private string path;

			#endregion

			#region Methods

			#region Virtual Methods

			public override bool Equals(object obj)
			{
				bool equalsMaterial = obj is SerializableMaterial material && material.Material == Material;
				bool equalsUMaterial = obj is Material uMaterial && uMaterial == Material;

				return equalsMaterial || equalsUMaterial;
			}
			public override int GetHashCode()
			{
				return 1578056576 + EqualityComparer<Material>.Default.GetHashCode(Material);
			}

			#endregion

			#region Global Methods

			private void Reload()
			{
				path = resourcePath;
				material = Resources.Load(path) as Material;
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			public SerializableMaterial(string path)
			{
				resourcePath = path;

				Reload();
			}

			#endregion

			#region Operators

			public static implicit operator bool(SerializableMaterial material) => material != null;
			public static implicit operator Material(SerializableMaterial material) => material.Material;
			public static bool operator ==(SerializableMaterial materialA, SerializableMaterial materialB)
			{
				return materialA.Equals(materialB);
			}
			public static bool operator !=(SerializableMaterial materialA, SerializableMaterial materialB)
			{
				return !(materialA == materialB);
			}
			public static bool operator ==(SerializableMaterial materialA, Material materialB)
			{
				return materialA.Equals(materialB);
			}
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

			public string resourcePath;
			public Light Light
			{
				get
				{
					if (!light || resourcePath != path)
						Reload();

					return light;
				}
			}

			private Light light;
			private string path;

			#endregion

			#region Methods

			#region Virtual Methods

			public override bool Equals(object obj)
			{
				bool equalsLight = obj is SerializableLight light && light.Light == Light;
				bool equalsULight = obj is Light uLight && uLight == Light;

				return equalsLight || equalsULight;
			}
			public override int GetHashCode()
			{
				return 1344377895 + EqualityComparer<Light>.Default.GetHashCode(Light);
			}

			#endregion

			#region Global Methods

			private void Reload()
			{
				path = resourcePath;
				light = Resources.Load(path) as Light;
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			public SerializableLight(string path)
			{
				resourcePath = path;

				Reload();
			}

			#endregion

			#region Operators

			public static implicit operator bool(SerializableLight light) => light != null;
			public static implicit operator Light(SerializableLight light) => light.Light;
			public static bool operator ==(SerializableLight lightA, SerializableLight lightB)
			{
				return lightA.Equals(lightB);
			}
			public static bool operator !=(SerializableLight lightA, SerializableLight lightB)
			{
				return !(lightA == lightB);
			}
			public static bool operator ==(SerializableLight lightA, Light lightB)
			{
				return lightA.Equals(lightB);
			}
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
		/// </summary>
		[Serializable]
		public struct TransformAccess
		{
			#region Variables

			[MarshalAs(UnmanagedType.U1)]
			public readonly bool isCreated;
			public int childCount;
			public float3 eulerAngles;
			public float3 forward;
			[MarshalAs(UnmanagedType.U1)]
			public bool hasChanged;
			public int hierarchyCapacity;
			public int hierarchyCount;
			public float3 localEulerAngles;
			public float3 localPosition;
			public quaternion localRotation;
			public float3 localScale;
			public float4x4 localToWorldMatrix;
			public float3 lossyScale;
			public float3 position;
			public float3 right;
			public quaternion rotation;
			public float3 up;
			public float4x4 worldToLocalMatrix;

			#endregion

			#region Constructors & Operators

			#region Constructors

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

			public static implicit operator TransformAccess(Transform transform) => new TransformAccess(transform);

			#endregion

			#endregion
		}
		/// <summary>
		/// Represents a single float value with a serialized field.
		/// Provides methods for conversion between float and float1 types.
		/// </summary>
		[Serializable]
#pragma warning disable IDE1006 // Naming Styles
		public struct float1
#pragma warning restore IDE1006 // Naming Styles
		{
			#region Variables

			[SerializeField]
#pragma warning disable IDE0044 // Add readonly modifier
			private float value;
#pragma warning restore IDE0044 // Add readonly modifier

			#endregion

			#region Operators

			public static implicit operator float1(float value) => new float1(value);
			public static implicit operator float(float1 value) => value.value;

			#endregion

			#region Constructors

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
		/// Represents the air density in kilograms per cubic meter.
		/// </summary>
		public const float airDensity = 1.29f;
		/// <summary>
		/// Represents an empty string.
		/// </summary>
		public const string emptyString = "";

		#endregion

		#region Variables

		/// <summary>
		/// Gets the time delta between frames, adjusted for fixed time steps.
		/// </summary>
		public static float DeltaTime => Time.inFixedTimeStep ? Time.fixedDeltaTime : Time.deltaTime;
		/// <summary>
		/// Represents a float2 vector with components (1, 1).
		/// </summary>
		public readonly static float2 Float2One = new float2(1f, 1f);
		/// <summary>
		/// Represents a float2 vector with components (0, 1).
		/// </summary>
		public readonly static float2 Float2Up = new float2(0f, 1f);
		/// <summary>
		/// Represents a float2 vector with components (1, 0).
		/// </summary>
		public readonly static float2 Float2Right = new float2(1f, 0f);
		/// <summary>
		/// Represents a float3 vector with components (1, 1, 1).
		/// </summary>
		public readonly static float3 Float3One = new float3(1f, 1f, 1f);
		/// <summary>
		/// Represents a float3 vector with components (0, 1, 0).
		/// </summary>
		public readonly static float3 Float3Up = new float3(0f, 1f, 0f);
		/// <summary>
		/// Represents a float3 vector with components (1, 0, 0).
		/// </summary>
		public readonly static float3 Float3Right = new float3(1f, 0f, 0f);
		/// <summary>
		/// Represents a float3 vector with components (0, 0, 1).
		/// </summary>
		public readonly static float3 Float3Forward = new float3(0f, 0f, 1f);
		/// <summary>
		/// Represents a float4 vector with components (1, 1, 1, 1).
		/// </summary>
		public readonly static float4 Float4One = new float4(1f, 1f, 1f, 1f);
		/// <summary>
		/// Represents a float4 vector with components (0, 1, 0, 0).
		/// </summary>
		public readonly static float4 Float4Up = new float4(0f, 1f, 0f, 0f);
		/// <summary>
		/// Represents a float4 vector with components (1, 0, 0, 0).
		/// </summary>
		public readonly static float4 Float4Right = new float4(1f, 0f, 0f, 0f);
		/// <summary>
		/// Represents a float4 vector with components (0, 0, 1, 0).
		/// </summary>
		public readonly static float4 Float4Forward = new float4(0f, 0f, 1f, 0f);
		/// <summary>
		/// Represents a float4 vector with components (0, 0, 0, 1).
		/// </summary>
		public readonly static float4 Float4Identity = new float4(0f, 0f, 0f, 1f);
		/// <summary>
		/// Represents a float4 vector with components (0, 0, 0, 0).
		/// </summary>
		public readonly static float4 Float4Zero = new float4(0f, 0f, 0f, 0f);

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
		/// Empty method that does nothing. Can be used as a placeholder.
		/// </summary>
		public static void Dummy()
		{

		}
		/// <summary>
		/// Gets the conversion multiplier for a specific unit type between metric and imperial systems.
		/// </summary>
		/// <param name="unit">The unit type to get the multiplier for.</param>
		/// <param name="unitType">The unit system (Metric or Imperial).</param>
		/// <returns>The conversion multiplier between metric and imperial units.</returns>
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
		/// </summary>
		/// <param name="unit">The unit type to get the symbol for.</param>
		/// <param name="unitType">The unit system (Metric or Imperial).</param>
		/// <returns>The abbreviated unit symbol as a string.</returns>
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
		/// </summary>
		/// <param name="unit">The unit type to get the full name for.</param>
		/// <param name="unitType">The unit system (Metric or Imperial).</param>
		/// <returns>The full name of the unit as a string.</returns>
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
		/// Extracts a numeric value from a string that may contain a unit.
		/// </summary>
		/// <param name="value">The string containing a number and possibly a unit.</param>
		/// <returns>The extracted numeric value, or 0 if no valid number is found.</returns>
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
		/// Extracts a numeric value from a string that may contain a unit, with unit type conversion.
		/// </summary>
		/// <param name="value">The string containing a number and possibly a unit.</param>
		/// <param name="unit">The unit type to convert to.</param>
		/// <param name="unitType">The unit system (Metric or Imperial).</param>
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
		/// Formats a number with a unit string, with optional rounding.
		/// </summary>
		/// <param name="number">The numeric value to format.</param>
		/// <param name="unit">The unit string to append.</param>
		/// <param name="rounded">Whether to round the number to the nearest integer.</param>
		/// <returns>A formatted string with the number and unit.</returns>
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
		/// </summary>
		/// <param name="number">The numeric value to format.</param>
		/// <param name="unit">The unit string to append.</param>
		/// <param name="decimals">The number of decimal places to include.</param>
		/// <returns>A formatted string with the number and unit.</returns>
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
		/// </summary>
		/// <param name="number">The numeric value to format.</param>
		/// <param name="unit">The unit type to use.</param>
		/// <param name="unitType">The unit system (Metric or Imperial).</param>
		/// <param name="rounded">Whether to round the number to the nearest integer.</param>
		/// <returns>A formatted string with the number and appropriate unit symbol.</returns>
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
		/// </summary>
		/// <param name="number">The numeric value to format.</param>
		/// <param name="unit">The unit type to use.</param>
		/// <param name="unitType">The unit system (Metric or Imperial).</param>
		/// <param name="decimals">The number of decimal places to include.</param>
		/// <returns>A formatted string with the number and appropriate unit symbol.</returns>
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
		/// </summary>
		/// <param name="number">The number to convert.</param>
		/// <returns>The ordinal representation of the number.</returns>
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
		/// </summary>
		/// <param name="gameObject">The parent GameObject.</param>
		/// <returns>An array of child GameObjects.</returns>
		public static GameObject[] GetChilds(GameObject gameObject)
		{
			return (from Transform child in gameObject.GetComponentsInChildren<Transform>() where gameObject.transform != child select child.gameObject).ToArray();
		}
#if UNITY_6000_0_OR_NEWER
		/// <summary>
		/// Evaluates the friction between two physics materials based on slip value and friction combine mode.
		/// </summary>
		/// <param name="slip">The slip value to determine whether to use static or dynamic friction.</param>
		/// <param name="refMaterial">The reference physics material.</param>
		/// <param name="material">The second physics material.</param>
		/// <returns>The calculated friction value.</returns>
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
		/// </summary>
		/// <param name="slip">The slip value to determine whether to use static or dynamic friction.</param>
		/// <param name="refMaterial">The reference physics material.</param>
		/// <param name="stiffness">The stiffness value to combine with the material's friction.</param>
		/// <returns>The calculated friction value.</returns>
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
		/// </summary>
		/// <param name="slip">The slip value to determine whether to use static or dynamic friction.</param>
		/// <param name="refMaterial">The reference physics material.</param>
		/// <param name="material">The second physics material.</param>
		/// <returns>The calculated friction value.</returns>
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
		/// </summary>
		/// <param name="speed">The initial speed.</param>
		/// <param name="friction">The friction coefficient.</param>
		/// <returns>The estimated braking distance.</returns>
		[Obsolete]
		public static float BrakingDistance(float speed, float friction)
		{
			friction = InverseLerpUnclamped(1f, 1.5f, friction);

			return ClampInfinity(LerpUnclamped(LerpUnclamped(30f, 26f, friction), LerpUnclamped(143f, 113f, friction), InverseLerpUnclamped(40f, 110f, speed)));
		}
		
		/// <summary>
		/// Calculates the braking distance using the physics formula for stopping distance.
		/// </summary>
		/// <param name="velocity">The initial velocity.</param>
		/// <param name="friction">The friction coefficient.</param>
		/// <param name="gravity">The gravity acceleration (default is Earth's gravity).</param>
		/// <returns>The calculated braking distance.</returns>
		public static float BrakingDistance(float velocity, float friction, float gravity = 9.81f)
		{
			return velocity * velocity / (2f * friction * gravity);
		}
		
		/// <summary>
		/// Calculates the braking distance to slow down from an initial velocity to a target velocity.
		/// </summary>
		/// <param name="velocity">The initial velocity.</param>
		/// <param name="targetVelocity">The target velocity to slow down to.</param>
		/// <param name="friction">The friction coefficient.</param>
		/// <param name="gravity">The gravity acceleration (default is Earth's gravity).</param>
		/// <returns>The calculated braking distance.</returns>
		public static float BrakingDistance(float velocity, float targetVelocity, float friction, float gravity = 9.81f)
		{
			return (velocity * velocity - targetVelocity * targetVelocity) / (2f * friction * gravity);
		}
		
		/// <summary>
		/// Converts RPM (Revolutions Per Minute) to linear speed based on radius.
		/// </summary>
		/// <param name="rpm">The rotational speed in RPM.</param>
		/// <param name="radius">The radius in meters.</param>
		/// <returns>The linear speed in meters per second.</returns>
		public static float RPMToSpeed(float rpm, float radius)
		{
			return radius * .377f * rpm;
		}
		
		/// <summary>
		/// Converts linear speed to RPM (Revolutions Per Minute) based on radius.
		/// </summary>
		/// <param name="speed">The linear speed in meters per second.</param>
		/// <param name="radius">The radius in meters.</param>
		/// <returns>The rotational speed in RPM.</returns>
		public static float SpeedToRPM(float speed, float radius)
		{
			if (radius <= 0f)
				return speed / .377f;

			return speed / radius / .377f;
		}
		/// <summary>
		/// Checks if a layer mask contains a specific layer.
		/// </summary>
		/// <param name="mask">The layer mask to check.</param>
		/// <param name="layer">The layer to check for.</param>
		/// <returns>True if the layer mask contains the layer, false otherwise.</returns>
		public static bool MaskHasLayer(LayerMask mask, int layer)
		{
			return MaskHasLayer(mask.value, layer);
		}
		/// <summary>
		/// Checks if a layer mask contains a specific layer.
		/// </summary>
		/// <param name="mask">The layer mask to check.</param>
		/// <param name="layer">The layer to check for.</param>
		/// <returns>True if the layer mask contains the layer, false otherwise.</returns>
		public static bool MaskHasLayer(int mask, int layer)
		{
			return (mask & 1 << layer) != 0;
		}
		/// <summary>
		/// Checks if a layer mask contains a specific layer.
		/// </summary>
		/// <param name="mask">The layer mask to check.</param>
		/// <param name="layer">The layer to check for.</param>
		/// <returns>True if the layer mask contains the layer, false otherwise.</returns>
		public static bool MaskHasLayer(LayerMask mask, string layer)
		{
			return MaskHasLayer(mask.value, layer);
		}
		/// <summary>
		/// Checks if a layer mask contains a specific layer.
		/// </summary>
		/// <param name="mask">The layer mask to check.</param>
		/// <param name="layer">The layer to check for.</param>
		/// <returns>True if the layer mask contains the layer, false otherwise.</returns>
		public static bool MaskHasLayer(int mask, string layer)
		{
			return MaskHasLayer(mask, LayerMask.NameToLayer(layer));
		}
		/// <summary>
		/// Creates an exclusive layer mask from a single layer name.
		/// </summary>
		/// <param name="name">The name of the layer to create a mask for.</param>
		/// <returns>The exclusive layer mask.</returns>
		public static int ExclusiveMask(string name)
		{
			return ExclusiveMask(new string[] { name });
		}
		/// <summary>
		/// Creates an exclusive layer mask from a single layer.
		/// </summary>
		/// <param name="layer">The layer to create a mask for.</param>
		/// <returns>The exclusive layer mask.</returns>
		public static int ExclusiveMask(int layer)
		{
			return ExclusiveMask(new int[] { layer });
		}
		/// <summary>
		/// Creates an exclusive layer mask from a list of layer names.
		/// </summary>
		/// <param name="layers">The list of layer names to create a mask for.</param>
		/// <returns>The exclusive layer mask.</returns>
		public static int ExclusiveMask(params string[] layers)
		{
			return ~LayerMask.GetMask(layers);
		}
		/// <summary>
		/// Creates an exclusive layer mask from a list of layer indices.
		/// </summary>
		/// <param name="layers">The list of layer indices to create a mask for.</param>
		/// <returns>The exclusive layer mask.</returns>
		public static int ExclusiveMask(params int[] layers)
		{
			return ExclusiveMask(layers.Select(layer => LayerMask.LayerToName(layer)).ToArray());
		}
		/// <summary>
		/// Converts a boolean value to a number.
		/// </summary>
		/// <param name="condition">The boolean value to convert.</param>
		/// <returns>1 if the condition is true, 0 otherwise.</returns>
		public static int BoolToNumber(bool condition)
		{
			return condition ? 1 : 0;
		}
		/// <summary>
		/// Converts a boolean value to a number with damping.
		/// </summary>
		/// <param name="source">The source number.</param>
		/// <param name="condition">The boolean value to convert.</param>
		/// <param name="damping">The damping factor.</param>
		public static float BoolToNumber(float source, bool condition, float damping = 2.5f)
		{
			return Mathf.MoveTowards(source, BoolToNumber(condition), Time.deltaTime * damping);
		}
		/// <summary>
		/// Inverts the sign of a boolean value.
		/// </summary>
		/// <param name="invert">The boolean value to invert.</param>
		/// <returns>-1 if the value is true, 1 otherwise.</returns>
		public static int InvertSign(bool invert)
		{
			return invert ? -1 : 1;
		}
		/// <summary>
		/// Converts a number to a boolean value.
		/// </summary>
		/// <param name="number">The number to convert.</param>
		/// <returns>True if the number is not zero, false otherwise.</returns>
		public static bool NumberToBool(float number)
		{
			return Clamp01((int)math.round(number)) != 0f;
		}
		/// <summary>
		/// Validates a username string according to specific rules.
		/// </summary>
		/// <param name="username">The username to validate.</param>
		/// <returns>True if the username is valid, false otherwise.</returns>
		/// <remarks>
		/// A valid username must:
		/// - Not be null or empty
		/// - Be between 6 and 64 characters long
		/// - Contain only alphanumeric characters and the symbols '_', '-', and '.'
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
		/// Validates a name string according to specific rules.
		/// </summary>
		/// <param name="name">The name to validate.</param>
		/// <returns>True if the name is valid, false otherwise.</returns>
		/// <remarks>
		/// A valid name must:
		/// - Not be null or empty (after trimming)
		/// - Be between 2 and 64 characters long
		/// - Contain only alphabetic characters and the symbols ' ', '.', and '-'
		/// - Not contain any numeric characters
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
		/// </summary>
		/// <param name="email">The email address to validate.</param>
		/// <param name="lookUpDomain">Whether to check if the domain is a known disposable email domain.</param>
		/// <returns>True if the email is valid, false otherwise.</returns>
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
		/// </summary>
		/// <param name="url">The URL to validate. May be modified to include a protocol if missing.</param>
		/// <param name="lookUpURL">Whether to check if the URL is accessible.</param>
		/// <param name="throwOnError">Whether to throw exceptions on network errors.</param>
		/// <returns>True if the URL is valid, false otherwise.</returns>
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
		/// </summary>
		/// <param name="data">The date string to validate.</param>
		/// <returns>True if the date string is valid, false otherwise.</returns>
		[Obsolete("Use Utility.ValidateDate instead", true)]
		public static bool ValidDate(string data)
		{
			return false;
		}
		/// <summary>
		/// Converts a time value to a string in the format "HH:MM:SS".
		/// </summary>
		/// <param name="time">The time value to convert.</param>
		/// <returns>A string representing the time in the format "HH:MM:SS".</returns>
		public static string TimeConverter(float time)
		{
			int seconds = (int)math.floor(time % 60);
			int minutes = (int)math.floor(time / 60);
			int hours = (int)math.floor(time / 3600);

			return (hours == 0 ? minutes.ToString() : (hours + ":" + minutes.ToString("00"))) + ":" + seconds.ToString("00");
		}
		/// <summary>
		/// Checks if a character is an alphabet letter.
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
		/// Checks if a character is a numeric digit.
		/// </summary>
		/// <param name="c">The character to check.</param>
		/// <returns>True if the character is a numeric digit, false otherwise.</returns>
		public static bool IsNumber(char c)
		{
			return c == '0' || c == '1' || c == '2' || c == '3' || c == '4' || c == '5' || c == '6' || c == '7' || c == '8' || c == '9';
		}
		/// <summary>
		/// Checks if a character is a symbol.
		/// </summary>
		/// <param name="c">The character to check.</param>
		/// <returns>True if the character is a symbol, false otherwise.</returns>
		public static bool IsSymbol(char c)
		{
			return !IsAlphabet(c) && !IsNumber(c);
		}
		/// <summary>
		/// Generates a random string with specified characteristics.
		/// </summary>
		/// <param name="length">The length of the random string to generate.</param>
		/// <param name="upperChars">Whether to include uppercase letters.</param>
		/// <param name="lowerChars">Whether to include lowercase letters.</param>
		/// <param name="numbers">Whether to include numeric digits.</param>
		/// <param name="symbols">Whether to include symbols.</param>
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
		/// Draws an arrow using Gizmos for visualization in the Game view.
		/// </summary>
		/// <param name="pos">The starting position of the arrow.</param>
		/// <param name="direction">The direction and length of the arrow.</param>
		/// <param name="arrowHeadLength">The length of the arrow head lines.</param>
		public static void DrawArrowForGizmos(Vector3 pos, Vector3 direction, float arrowHeadLength = .25f, float arrowHeadAngle = 20f)
		{
			Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f + arrowHeadAngle, 0f) * Vector3.forward;
			Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f - arrowHeadAngle, 0f) * Vector3.forward;

			Gizmos.DrawRay(pos, direction);
			Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
			Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
		}
		/// <summary>
		/// Draws an arrow using Gizmos for visualization in the Game view.
		/// </summary>
		/// <param name="pos">The starting position of the arrow.</param>
		/// <param name="direction">The direction and length of the arrow.</param>
		/// <param name="color">The color of the arrow.</param>
		public static void DrawArrowForGizmos(Vector3 pos, Vector3 direction, UnityEngine.Color color, float arrowHeadLength = .25f, float arrowHeadAngle = 20f)
		{
			UnityEngine.Color orgColor = Gizmos.color;

			Gizmos.color = color;

			DrawArrowForGizmos(pos, direction, arrowHeadLength, arrowHeadAngle);

			Gizmos.color = orgColor;
		}
		/// <summary>
		/// Draws an arrow using Debug.DrawLine for visualization in the Game view.
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
		/// Checks if a point is inside a triangle.
		/// </summary>
		/// <param name="point">The point to check.</param>
		/// <param name="point1">The first point of the triangle.</param>
		/// <param name="point2">The second point of the triangle.</param>
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
		/// Calculates a point from a circle based on a given angle and surface.
		/// </summary>
		/// <param name="center">The center of the circle.</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <param name="angle">The angle in degrees.</param>
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
		/// </summary>
		/// <param name="worldPoint">The point in world space.</param>
		/// <param name="parentPosition">The position of the parent transform.</param>
		/// <param name="parentRotation">The rotation of the parent transform.</param>
		public static Vector3 PointWorldToLocal(Vector3 worldPoint, Vector3 parentPosition, Quaternion parentRotation, Vector3 parentScale)
		{
			return Vector3.Scale(Quaternion.Inverse(parentRotation) * (worldPoint - parentPosition), new Vector3(1f/parentScale.x, 1f/parentScale.y, 1f/parentScale.z));
		}
		/// <summary>
		/// Converts a point from world space to local space relative to a parent transform with scaling using float3.
		/// </summary>
		/// <param name="worldPoint">The point in world space.</param>
		/// <param name="parentPosition">The position of the parent transform.</param>
		/// <param name="parentRotation">The rotation of the parent transform.</param>
		public static float3 PointWorldToLocal(float3 worldPoint, float3 parentPosition, quaternion parentRotation, float3 parentScale)
		{
			return math.mul(math.inverse(parentRotation), worldPoint - parentPosition) / parentScale;
		}
		/// <summary>
		/// Converts a point from local space to world space relative to a parent transform.
		/// </summary>
		/// <param name="localPoint">The point in local space.</param>
		/// <param name="parentPosition">The position of the parent transform.</param>
		/// <param name="parentRotation">The rotation of the parent transform.</param>
		public static Vector3 PointLocalToWorld(Vector3 localPoint, Vector3 parentPosition, Quaternion parentRotation, Vector3 parentScale)
		{
			return parentRotation * Vector3.Scale(localPoint, parentScale) + parentPosition;
		}
		/// <summary>
		/// Converts a point from local space to world space relative to a parent transform with scaling using float3.
		/// </summary>
		/// <param name="localPoint">The point in local space.</param>
		/// <param name="parentPosition">The position of the parent transform.</param>
		/// <param name="parentRotation">The rotation of the parent transform.</param>
		public static float3 PointLocalToWorld(float3 localPoint, float3 parentPosition, quaternion parentRotation, float3 parentScale)
		{
			return math.mul(parentRotation, localPoint * parentScale) + parentPosition;
		}
		/// <summary>
		/// Returns the absolute values of the components of a vector.
		/// </summary>
		/// <param name="vector">The vector to get the absolute values of.</param>
		/// <returns>A vector with the absolute values of the components of the input vector.</returns>
		public static Vector3 Abs(Vector3 vector)
		{
			return new Vector3(vector.x, vector.y, vector.z);
		}
		/// <summary>
		/// Returns the absolute values of the components of a vector using float3.
		/// </summary>
		/// <param name="vector">The vector to get the absolute values of.</param>
		/// <returns>A vector with the absolute values of the components of the input vector.</returns>
		public static float3 Abs(float3 vector)
		{
			return new float3(vector.x, vector.y, vector.z);
		}
		/// <summary>
		/// Returns a vector with the rounded values of the components of the input vector.
		/// </summary>
		/// <param name="vector">The vector to round.</param>
		/// <returns>A vector with the rounded values of the components of the input vector.</returns>
		public static Vector3 Round(Vector3 vector)
		{
			return new Vector3(math.round(vector.x), math.round(vector.y), math.round(vector.z));
		}
		/// <summary>
		/// Returns a vector with the rounded values of the components of the input vector to a specified number of decimal places.
		/// </summary>
		/// <param name="vector">The vector to round.</param>
		/// <param name="decimals">The number of decimal places to round to.</param>
		/// <returns>A vector with the rounded values of the components of the input vector to the specified number of decimal places.</returns>
		public static Vector3 Round(Vector3 vector, uint decimals)
		{
			return new Vector3(Round(vector.x, decimals), Round(vector.y, decimals), Round(vector.z, decimals));
		}
		/// <summary>
		/// Returns a vector with the rounded values of the components of the input vector.
		/// </summary>
		/// <param name="vector">The vector to round.</param>
		/// <returns>A vector with the rounded values of the components of the input vector.</returns>
		public static Vector2 Round(Vector2 vector)
		{
			return new Vector2(math.round(vector.x), math.round(vector.y));
		}
		/// <summary>
		/// Returns a vector with the rounded values of the components of the input vector to a specified number of decimal places.
		/// </summary>
		/// <param name="vector">The vector to round.</param>
		/// <param name="decimals">The number of decimal places to round to.</param>
		/// <returns>A vector with the rounded values of the components of the input vector to the specified number of decimal places.</returns>
		public static Vector2 Round(Vector2 vector, uint decimals)
		{
			return new Vector2(Round(vector.x, decimals), Round(vector.y, decimals));
		}
		/// <summary>
		/// Returns a vector with the rounded values of the components of the input vector as an integer.
		/// </summary>
		/// <param name="vector">The vector to round.</param>
		/// <returns>A vector with the rounded values of the components of the input vector as an integer.</returns>
		public static Vector3Int RoundToInt(Vector3 vector)
		{
			return new Vector3Int((int)math.round(vector.x), (int)math.round(vector.y), (int)math.round(vector.z));
		}
		/// <summary>
		/// Returns a vector with the rounded values of the components of the input vector as an integer.
		/// </summary>
		/// <param name="vector">The vector to round.</param>
		/// <returns>A vector with the rounded values of the components of the input vector as an integer.</returns>
		public static Vector2Int RoundToInt(Vector2 vector)
		{
			return new Vector2Int((int)math.round(vector.x), (int)math.round(vector.y));
		}
		/// <summary>
		/// Returns a float with the rounded value of the input float to a specified number of decimal places.
		/// </summary>
		/// <param name="number">The float to round.</param>
		/// <param name="decimals">The number of decimal places to round to.</param>
		/// <returns>A float with the rounded value of the input float to the specified number of decimal places.</returns>
		public static float Round(float number, uint decimals)
		{
			float multiplier = math.pow(10f, decimals);

			return math.round(number * multiplier) / multiplier;
		}
		/// <summary>
		/// Calculates the normalized direction vector from origin to destination.
		/// </summary>
		/// <param name="origin">The starting point.</param>
		/// <param name="destination">The end point.</param>
		/// <returns>A normalized direction vector pointing from origin to destination.</returns>
		public static Vector3 Direction(Vector3 origin, Vector3 destination)
		{
			return (destination - origin).normalized;
		}
		/// <summary>
		/// Calculates the unnormalized direction vector from origin to destination.
		/// </summary>
		/// <param name="origin">The starting point.</param>
		/// <param name="destination">The end point.</param>
		/// <returns>An unnormalized vector pointing from origin to destination.</returns>
		public static Vector3 DirectionUnNormalized(Vector3 origin, Vector3 destination)
		{
			return destination - origin;
		}
		/// <summary>
		/// Calculates the right vector based on forward and up vectors.
		/// </summary>
		/// <param name="forward">The forward direction.</param>
		/// <param name="up">The up direction.</param>
		/// <returns>The right vector perpendicular to both forward and up.</returns>
		public static Vector3 DirectionRight(Vector3 forward, Vector3 up)
		{
			return Quaternion.AngleAxis(90f, up) * forward;
		}
		/// <summary>
		/// Calculates the normalized direction vector from origin to destination using float3.
		/// </summary>
		/// <param name="origin">The starting point.</param>
		/// <param name="destination">The end point.</param>
		/// <returns>A normalized direction vector pointing from origin to destination.</returns>
		public static float3 Direction(float3 origin, float3 destination)
		{
			return math.normalize(destination - origin);
		}
		/// <summary>
		/// Calculates the unnormalized direction vector from origin to destination using float3.
		/// </summary>
		/// <param name="origin">The starting point.</param>
		/// <param name="destination">The end point.</param>
		/// <returns>An unnormalized vector pointing from origin to destination.</returns>
		public static float3 DirectionUnNormalized(float3 origin, float3 destination)
		{
			return destination - origin;
		}
		/// <summary>
		/// Calculates the right vector based on forward and up vectors using float3.
		/// </summary>
		/// <param name="forward">The forward direction.</param>
		/// <param name="up">The up direction.</param>
		/// <returns>The right vector perpendicular to both forward and up.</returns>
		public static float3 DirectionRight(float3 forward, float3 up)
		{
			return math.mul(quaternion.AxisAngle(up, math.PI * .5f), forward);
		}
		/// <summary>
		/// Calculates the normalized direction vector from origin to destination.
		/// </summary>
		/// <param name="origin">The starting point.</param>
		/// <param name="destination">The end point.</param>
		/// <returns>A normalized direction vector pointing from origin to destination.</returns>
		public static Vector2 Direction(Vector2 origin, Vector2 destination)
		{
			return (destination - origin).normalized;
		}
		/// <summary>
		/// Calculates the unnormalized direction vector from origin to destination.
		/// </summary>
		/// <param name="origin">The starting point.</param>
		/// <param name="destination">The end point.</param>
		/// <returns>An unnormalized vector pointing from origin to destination.</returns>
		public static Vector2 DirectionUnNormalized(Vector2 origin, Vector2 destination)
		{
			return destination - origin;
		}
		/// <summary>
		/// Calculates the normalized direction vector from origin to destination using float2.
		/// </summary>
		/// <param name="origin">The starting point.</param>
		/// <param name="destination">The end point.</param>
		/// <returns>A normalized direction vector pointing from origin to destination.</returns>
		public static float2 Direction(float2 origin, float2 destination)
		{
			return math.normalize(destination - origin);
		}
		/// <summary>
		/// Calculates the unnormalized direction vector from origin to destination using float2.
		/// </summary>
		/// <param name="origin">The starting point.</param>
		/// <param name="destination">The end point.</param>
		/// <returns>An unnormalized vector pointing from origin to destination.</returns>
		public static float2 DirectionUnNormalized(float2 origin, float2 destination)
		{
			return destination - origin;
		}
		/// <summary>
		/// Determines the side of a point compared to a reference point and forward direction.
		/// </summary>
		/// <param name="point">The point to compare.</param>
		/// <param name="comparingPoint">The reference point.</param>
		/// <param name="comparingForward">The forward direction of the reference point.</param>
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
		/// </summary>
		/// <param name="direction">The direction to measure.</param>
		/// <param name="axis">The axis to measure around.</param>
		/// <param name="forward">The reference forward direction.</param>
		/// <returns>The angle in degrees.</returns>
		public static float AngleAroundAxis(Vector3 direction, Vector3 axis, Vector3 forward)
		{
			Vector3 right = Vector3.Cross(axis, forward).normalized;

			forward = Vector3.Cross(right, axis).normalized;

			return math.atan2(Vector3.Dot(direction, right), Vector3.Dot(direction, forward)) * Mathf.Rad2Deg;
		}
		/// <summary>
		/// Calculates the angle around an axis between a direction and a reference forward direction using float3.
		/// </summary>
		/// <param name="direction">The direction to measure.</param>
		/// <param name="axis">The axis to measure around.</param>
		/// <param name="forward">The reference forward direction.</param>
		/// <returns>The angle in degrees.</returns>
		public static float AngleAroundAxis(float3 direction, float3 axis, float3 forward)
		{
			float3 right = math.normalizesafe(math.cross(axis, forward), Float3Right);

			forward = math.normalizesafe(math.cross(right, axis), Float3Forward);

			return math.atan2(math.dot(direction, right), math.dot(direction, forward)) * Mathf.Rad2Deg;
		}
		/// <summary>
		/// Calculates the interpolation parameter that would result in the value t when linearly interpolating from a to b.
		/// The result is clamped between 0 and 1.
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
		/// The result is not clamped.
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
		/// The result is clamped between 0 and 1.
		/// </summary>
		/// <param name="a">The start point of the interpolation.</param>
		/// <param name="b">The end point of the interpolation.</param>
		/// <param name="t">The point to find the interpolation parameter for.</param>
		/// <returns>The interpolation parameter clamped between 0 and 1.</returns>
		public static float InverseLerp(float3 a, float3 b, float3 t)
		{
			return Clamp01(InverseLerpUnclamped(a, b, t));
		}
		/// <summary>
		/// Calculates the interpolation parameter that would result in the value t when linearly interpolating from a to b.
		/// The result is not clamped.
		/// </summary>
		/// <param name="a">The start point of the interpolation.</param>
		/// <param name="b">The end point of the interpolation.</param>
		/// <param name="t">The point to find the interpolation parameter for.</param>
		/// <returns>The interpolation parameter (can be outside the 0-1 range).</returns>
		public static float InverseLerpUnclamped(float3 a, float3 b, float3 t)
		{
			float3 AB = b - a;
			float3 AT = t - a;

			return math.dot(AT, AB) / math.dot(AB, AB);
		}
		/// <summary>
		/// Calculates the interpolation parameter that would result in the value t when linearly interpolating from a to b.
		/// The result is clamped between 0 and 1.
		/// </summary>
		/// <param name="a">The start point of the interpolation.</param>
		/// <param name="b">The end point of the interpolation.</param>
		/// <param name="t">The point to find the interpolation parameter for.</param>
		/// <returns>The interpolation parameter clamped between 0 and 1.</returns>
		public static float InverseLerp(float a, float b, float t)
		{
			return Clamp01(InverseLerpUnclamped(a, b, t));
		}
		/// <summary>
		/// Calculates the interpolation parameter that would result in the value t when linearly interpolating from a to b.
		/// The result is not clamped.
		/// </summary>
		/// <param name="a">The start point of the interpolation.</param>
		/// <param name="b">The end point of the interpolation.</param>
		/// <param name="t">The point to find the interpolation parameter for.</param>
		/// <returns>The interpolation parameter (can be outside the 0-1 range).</returns>
		public static float InverseLerpUnclamped(float a, float b, float t)
		{
			return (t - a) / (b - a);
		}
		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// The result is not clamped.
		/// </summary>
		/// <param name="a">The start point of the interpolation.</param>
		/// <param name="b">The end point of the interpolation.</param>
		/// <param name="t">The interpolation parameter.</param>
		/// <returns>The value resulting from the interpolation.</returns>
		public static float LerpUnclamped(float a, float b, float t)
		{
			return a + (b - a) * t;
		}
		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// The result is not clamped.
		/// </summary>
		/// <param name="a">The start point of the interpolation.</param>
		/// <param name="b">The end point of the interpolation.</param>
		/// <param name="t">The interpolation parameter.</param>
		/// <returns>The value resulting from the interpolation.</returns>
		public static float3 LerpUnclamped(float3 a, float3 b, float t)
		{
			return new float3(LerpUnclamped(a.x, b.x, t), LerpUnclamped(a.y, b.y, t), LerpUnclamped(a.z, b.z, t));
		}
		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// The result is not clamped.
		/// </summary>
		/// <param name="a">The start point of the interpolation.</param>
		/// <param name="b">The end point of the interpolation.</param>
		/// <param name="t">The interpolation parameter.</param>
		/// <returns>The value resulting from the interpolation.</returns>
		public static Vector3 LerpUnclamped(Vector3 a, Vector3 b, float t)
		{
			return new float3(LerpUnclamped(a.x, b.x, t), LerpUnclamped(a.y, b.y, t), LerpUnclamped(a.z, b.z, t));
		}
		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// The result is not clamped.
		/// </summary>
		/// <param name="a">The start point of the interpolation.</param>
		/// <param name="b">The end point of the interpolation.</param>
		/// <param name="t">The interpolation parameter.</param>
		/// <returns>The value resulting from the interpolation.</returns>
		public static float2 LerpUnclamped(float2 a, float2 b, float t)
		{
			return new float2(LerpUnclamped(a.x, b.x, t), LerpUnclamped(a.y, b.y, t));
		}
		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// The result is not clamped.
		/// </summary>
		/// <param name="a">The start point of the interpolation.</param>
		/// <param name="b">The end point of the interpolation.</param>
		/// <param name="t">The interpolation parameter.</param>
		/// <returns>The value resulting from the interpolation.</returns>
		public static Vector2 LerpUnclamped(Vector2 a, Vector2 b, float t)
		{
			return new float2(LerpUnclamped(a.x, b.x, t), LerpUnclamped(a.y, b.y, t));
		}
		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// The result is not clamped.
		/// </summary>
		/// <param name="a">The start point of the interpolation.</param>
		/// <param name="b">The end point of the interpolation.</param>
		/// <param name="t">The interpolation parameter.</param>
		/// <returns>The value resulting from the interpolation.</returns>
		public static quaternion LerpUnclamped(quaternion a, quaternion b, float t)
		{
			return math.nlerp(a, b, t);
		}
		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// The result is clamped between 0 and 1.
		/// </summary>
		/// <param name="a">The start point of the interpolation.</param>
		/// <param name="b">The end point of the interpolation.</param>
		/// <param name="t">The interpolation parameter.</param>
		/// <returns>The value resulting from the interpolation.</returns>
		public static float Lerp(float a, float b, float t)
		{
			return LerpUnclamped(a, b, Clamp01(t));
		}
		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// The result is clamped between 0 and 1.
		/// </summary>
		/// <param name="a">The start point of the interpolation.</param>
		/// <param name="b">The end point of the interpolation.</param>
		/// <param name="t">The interpolation parameter.</param>
		/// <returns>The value resulting from the interpolation.</returns>
		public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
		{
			return LerpUnclamped(a, b, Clamp01(t));
		}
		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// The result is clamped between 0 and 1.
		/// </summary>
		/// <param name="a">The start point of the interpolation.</param>
		/// <param name="b">The end point of the interpolation.</param>
		/// <param name="t">The interpolation parameter.</param>
		/// <returns>The value resulting from the interpolation.</returns>
		public static float3 Lerp(float3 a, float3 b, float t)
		{
			return LerpUnclamped(a, b, Clamp01(t));
		}
		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// The result is clamped between 0 and 1.
		/// </summary>
		/// <param name="a">The start point of the interpolation.</param>
		/// <param name="b">The end point of the interpolation.</param>
		/// <param name="t">The interpolation parameter.</param>
		/// <returns>The value resulting from the interpolation.</returns>
		public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
		{
			return LerpUnclamped(a, b, Clamp01(t));
		}
		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// The result is clamped between 0 and 1.
		/// </summary>
		/// <param name="a">The start point of the interpolation.</param>
		/// <param name="b">The end point of the interpolation.</param>
		/// <param name="t">The interpolation parameter.</param>
		/// <returns>The value resulting from the interpolation.</returns>
		public static float2 Lerp(float2 a, float2 b, float t)
		{
			return LerpUnclamped(a, b, Clamp01(t));
		}
		/// <summary>
		/// Calculates the value that would result from linearly interpolating between a and b by the interpolation parameter t.
		/// The result is clamped between 0 and 1.
		/// </summary>
		/// <param name="a">The start point of the interpolation.</param>
		/// <param name="b">The end point of the interpolation.</param>
		/// <param name="t">The interpolation parameter.</param>
		/// <returns>The value resulting from the interpolation.</returns>
		public static quaternion Lerp(quaternion a, quaternion b, float t)
		{
			return LerpUnclamped(a, b, Clamp01(t));
		}
		/// <summary>
		/// Moves a color towards a target color by a maximum delta.
		/// </summary>
		/// <param name="a">The current color.</param>
		/// <param name="b">The target color.</param>
		/// <param name="maxDelta">The maximum change that should be applied to each component.</param>
		/// <returns>The new color resulting from the movement.</returns>
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
		/// </summary>
		/// <param name="a">The current color.</param>
		/// <param name="b">The target color.</param>
		/// <param name="maxDelta">The maximum change that should be applied to each component, specified as a color.</param>
		/// <returns>The new color resulting from the movement.</returns>
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
		/// </summary>
		/// <param name="transforms">The series of Transform points.</param>
		/// <returns>The total distance between the points.</returns>
		public static float Distance(params Transform[] transforms)
		{
			float distance = 0f;

			for (int i = 0; i < transforms.Length - 1; i++)
				distance += Distance(transforms[i], transforms[i + 1]);

			return distance;
		}
		/// <summary>
		/// Calculates the Euclidean distance between two Transform points.
		/// </summary>
		/// <param name="a">The first point.</param>
		/// <param name="b">The second point.</param>
		/// <returns>The distance between the points.</returns>
		public static float Distance(Transform a, Transform b)
		{
			return Utility.Distance(a.position, b.position);
		}
		/// <summary>
		/// Calculates the Euclidean distance between a series of Vector3 points.
		/// </summary>
		/// <param name="vectors">The series of Vector3 points.</param>
		/// <returns>The total distance between the points.</returns>
		public static float Distance(params Vector3[] vectors)
		{
			float distance = 0f;

			for (int i = 0; i < vectors.Length - 1; i++)
				distance += Distance(vectors[i], vectors[i + 1]);

			return distance;
		}
		/// <summary>
		/// Calculates the Euclidean distance between a series of float3 points.
		/// </summary>
		/// <param name="vectors">The series of float3 points.</param>
		/// <returns>The total distance between the points.</returns>
		public static float Distance(params float3[] vectors)
		{
			float distance = 0f;

			for (int i = 0; i < vectors.Length - 1; i++)
				distance += Distance(vectors[i], vectors[i + 1]);

			return distance;
		}
		/// <summary>
		/// Calculates the Euclidean distance between two Vector3 points.
		/// </summary>
		/// <param name="a">The first point.</param>
		/// <param name="b">The second point.</param>
		/// <returns>The distance between the points.</returns>
		public static float Distance(Vector3 a, Vector3 b)
		{
			return (a - b).magnitude;
		}		
		/// <summary>
		/// Calculates the Euclidean distance between two float3 points.
		/// </summary>
		/// <param name="a">The first point.</param>
		/// <param name="b">The second point.</param>
		/// <returns>The distance between the points.</returns>
		public static float Distance(float3 a, float3 b)
		{
			return math.length(a - b);
		}
		/// <summary>
		/// Calculates the Euclidean distance between a series of Vector2 points.
		/// </summary>
		/// <param name="vectors">The series of Vector2 points.</param>
		/// <returns>The total distance between the points.</returns>
		public static float Distance(params Vector2[] vectors)
		{
			float distance = 0f;

			for (int i = 0; i < vectors.Length - 1; i++)
				distance += Distance(vectors[i], vectors[i + 1]);

			return distance;
		}
		/// <summary>
		/// Calculates the Euclidean distance between a series of float2 points.
		/// </summary>
		/// <param name="vectors">The series of float2 points.</param>
		/// <returns>The total distance between the points.</returns>
		public static float Distance(params float2[] vectors)
		{
			float distance = 0f;

			for (int i = 0; i < vectors.Length - 1; i++)
				distance += Distance(vectors[i], vectors[i + 1]);

			return distance;
		}
		/// <summary>
		/// Calculates the Euclidean distance between two Vector2 points.
		/// </summary>
		/// <param name="a">The first point.</param>
		/// <param name="b">The second point.</param>
		/// <returns>The distance between the points.</returns>
		public static float Distance(Vector2 a, Vector2 b)
		{
			return (a - b).magnitude;
		}
		/// <summary>
		/// Calculates the Euclidean distance between two float2 points.
		/// </summary>
		/// <param name="a">The first point.</param>
		/// <param name="b">The second point.</param>
		/// <returns>The distance between the points.</returns>
		public static float Distance(float2 a, float2 b)
		{
			return math.length(a - b);
		}
		/// <summary>
		/// Calculates the squared Euclidean distance between two Vector3 points.
		/// </summary>
		/// <param name="a">The first point.</param>
		/// <param name="b">The second point.</param>
		/// <returns>The squared distance between the points.</returns>
		public static float DistanceSqr(Vector3 a, Vector3 b)
		{
			return (a - b).sqrMagnitude;
		}
		/// <summary>
		/// Calculates the squared Euclidean distance between two float3 points.
		/// </summary>
		/// <param name="a">The first point.</param>
		/// <param name="b">The second point.</param>
		/// <returns>The squared distance between the points.</returns>
		public static float DistanceSqr(float3 a, float3 b)
		{
			return math.lengthsq(a - b);
		}
		/// <summary>
		/// Calculates the difference between two float values.
		/// </summary>
		/// <param name="a">The first value.</param>
		/// <param name="b">The second value.</param>
		/// <returns>The difference between the values.</returns>
		public static float Distance(float a, float b)
		{
			return math.max(a, b) - math.min(a, b);
		}
		/// <summary>
		/// Calculates the velocity of a value between two states.
		/// </summary>
		/// <param name="current">The current value.</param>
		/// <param name="last">The previous value.</param>
		/// <param name="deltaTime">The time delta.</param>
		public static float Velocity(float current, float last, float deltaTime)
		{
			return (last - current) / deltaTime;
		}
		/// <summary>
		/// Calculates the velocity of a value between two Vector3 states.
		/// </summary>
		/// <param name="current">The current value.</param>
		/// <param name="last">The previous value.</param>
		/// <param name="deltaTime">The time delta.</param>
		public static Vector3 Velocity(Vector3 current, Vector3 last, float deltaTime)
		{
			return Divide(last - current, deltaTime);
		}
		/// <summary>
		/// Loops a number within the range [0, after).
		/// Similar to the modulo operation but works correctly with negative numbers.
		/// </summary>
		/// <param name="number">The number to loop.</param>
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
		/// Determines if a value has transitioned from off to on (false to true) between states.
		/// </summary>
		/// <param name="current">The current value.</param>
		/// <param name="last">The previous value.</param>
		/// <returns>True if the value has transitioned from off to on, false otherwise.</returns>
		public static bool IsDownFromLastState(float current, float last)
		{
			return IsDownFromLastState(NumberToBool(current), NumberToBool(last));
		}
		/// <summary>
		/// Determines if a value has transitioned from off to on (0 to non-zero) between states.
		/// </summary>
		/// <param name="current">The current value.</param>
		/// <param name="last">The previous value.</param>
		/// <returns>True if the value has transitioned from off to on, false otherwise.</returns>
		public static bool IsDownFromLastState(int current, int last)
		{
			return IsDownFromLastState(NumberToBool(current), NumberToBool(last));
		}
		/// <summary>
		/// Determines if a boolean value has transitioned from false to true between states.
		/// </summary>
		/// <param name="current">The current value.</param>
		/// <param name="last">The previous value.</param>
		/// <returns>True if the value has transitioned from false to true, false otherwise.</returns>
		public static bool IsDownFromLastState(bool current, bool last)
		{
			return !last && current;
		}
		/// <summary>
		/// Determines if a value has transitioned from on to off (true to false) between states.
		/// </summary>
		/// <param name="current">The current value.</param>
		/// <param name="last">The previous value.</param>
		/// <returns>True if the value has transitioned from on to off, false otherwise.</returns>
		public static bool IsUpFromLastState(float current, float last)
		{
			return IsUpFromLastState(NumberToBool(current), NumberToBool(last));
		}
		/// <summary>
		/// Determines if a value has transitioned from on to off (non-zero to 0) between states.
		/// </summary>
		/// <param name="current">The current value.</param>
		/// <param name="last">The previous value.</param>
		/// <returns>True if the value has transitioned from on to off, false otherwise.</returns>
		public static bool IsUpFromLastState(int current, int last)
		{
			return IsUpFromLastState(NumberToBool(current), NumberToBool(last));
		}
		/// <summary>
		/// Determines if a boolean value has transitioned from true to false between states.
		/// </summary>
		/// <param name="current">The current value.</param>
		/// <param name="last">The previous value.</param>
		/// <returns>True if the value has transitioned from true to false, false otherwise.</returns>
		public static bool IsUpFromLastState(bool current, bool last)
		{
			return last && !current;
		}
		/// <summary>
		/// Divides a float3 by another float3.
		/// </summary>
		/// <param name="a">The dividend.</param>
		/// <param name="b">The divisor.</param>
		/// <returns>The result of the division.</returns>
		public static float3 Divide(float3 a, float3 b)
		{
			return new Vector3(b.x != 0f ? a.x / b.x : 0f, b.y != 0f ? a.y / b.y : 0f, b.z != 0f ? a.z / b.z : 0f);
		}
		/// <summary>
		/// Divides a Vector3 by another Vector3.
		/// </summary>
		/// <param name="a">The dividend.</param>
		/// <param name="b">The divisor.</param>
		/// <returns>The result of the division.</returns>
		public static Vector3 Divide(Vector3 a, Vector3 b)
		{
			return new Vector3(b.x != 0f ? a.x / b.x : 0f, b.y != 0f ? a.y / b.y : 0f, b.z != 0f ? a.z / b.z : 0f);
		}
		/// <summary>
		/// Divides a series of Vector3 vectors.
		/// </summary>
		/// <param name="vectors">The series of Vector3 vectors.</param>
		/// <returns>The result of the division.</returns>
		public static Vector3 Divide(params Vector3[] vectors)
		{
			return Divide(vectors as IEnumerable<Vector3>);
		}
		/// <summary>
		/// Divides a series of Vector3 vectors.
		/// </summary>
		/// <param name="vectors">The series of Vector3 vectors.</param>
		/// <returns>The result of the division.</returns>
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
		/// Divides a Vector3 by a float.
		/// </summary>
		/// <param name="vector">The dividend.</param>
		/// <param name="divider">The divisor.</param>
		/// <returns>The result of the division.</returns>
		public static Vector3 Divide(Vector3 vector, float divider)
		{
			if (divider == 0f)
				return default;

			return new Vector3(vector.x / divider, vector.y / divider, vector.z / divider);
		}
		/// <summary>
		/// Divides a float3 by a float.
		/// </summary>
		/// <param name="vector">The dividend.</param>
		/// <param name="divider">The divisor.</param>
		/// <returns>The result of the division.</returns>
		public static float3 Divide(float3 vector, float divider)
		{
			if (divider == 0f)
				return default;

			return new float3(vector.x / divider, vector.y / divider, vector.z / divider);
		}
		/// <summary>
		/// Multiplies two float3 vectors.
		/// </summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The result of the multiplication.</returns>
		public static float3 Multiply(float3 a, float3 b)
		{
			return new float3(a.x * b.x, a.y * b.y, a.z * b.z);
		}
		/// <summary>
		/// Multiplies two Vector3 vectors.
		/// </summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The result of the multiplication.</returns>
		public static Vector3 Multiply(Vector3 a, Vector3 b)
		{
			return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
		}
		/// <summary>
		/// Multiplies a series of Vector3 vectors.
		/// </summary>
		/// <param name="vectors">The series of Vector3 vectors.</param>
		/// <returns>The result of the multiplication.</returns>
		public static Vector3 Multiply(params Vector3[] vectors)
		{
			return Multiply(vectors as IEnumerable<Vector3>);
		}
		/// <summary>
		/// Multiplies a series of Vector3 vectors.
		/// </summary>
		/// <param name="vectors">The series of Vector3 vectors.</param>
		/// <returns>The result of the multiplication.</returns>
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
		/// </summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The average of the two vectors.</returns>
		public static float3 Average(float3 a, float3 b)
		{
			return new float3((a.x + b.x) * .5f, (a.y + b.y) * .5f, (a.z + b.z) * .5f);
		}
		/// <summary>
		/// Calculates the average of three float3 vectors.
		/// </summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <param name="c">The third vector.</param>
		public static float3 Average(float3 a, float3 b, float3 c)
		{
			return new float3((a.x + b.x + c.x) / 3f, (a.y + b.y + c.y) / 3f, (a.z + b.z + c.z) / 3f);
		}
		/// <summary>
		/// Calculates the average of multiple Vector3 vectors.
		/// </summary>
		/// <param name="vectors">The vectors to average.</param>
		/// <returns>A new Vector3 representing the average of all input vectors.</returns>
		public static Vector3 Average(params Vector3[] vectors)
		{
			return Average(vectors as IEnumerable<Vector3>);
		}
		/// <summary>
		/// Calculates the average of multiple Vector3 vectors from an enumerable collection.
		/// </summary>
		/// <param name="vectors">The enumerable collection of vectors to average.</param>
		/// <returns>A new Vector3 representing the average of all input vectors.</returns>
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
		/// </summary>
		/// <param name="vectors">The vectors to average.</param>
		/// <returns>A new Vector2 representing the average of all input vectors.</returns>
		public static Vector2 Average(params Vector2[] vectors)
		{
			return Average(vectors as IEnumerable<Vector2>);
		}
		/// <summary>
		/// Calculates the average of multiple Vector2 vectors from an enumerable collection.
		/// </summary>
		/// <param name="vectors">The enumerable collection of vectors to average.</param>
		/// <returns>A new Vector2 representing the average of all input vectors.</returns>
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
		/// </summary>
		/// <param name="vectors">The vectors to average.</param>
		/// <returns>A new Vector3Int representing the average of all input vectors.</returns>
		public static Vector3Int Average(params Vector3Int[] vectors)
		{
			return Average(vectors as IEnumerable<Vector3Int>);
		}
		/// <summary>
		/// Calculates the average of multiple Vector3Int vectors from an enumerable collection.
		/// </summary>
		/// <param name="vectors">The enumerable collection of vectors to average.</param>
		/// <returns>A new Vector3Int representing the average of all input vectors.</returns>
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
		/// </summary>
		/// <param name="vectors">The vectors to average.</param>
		/// <returns>A new Vector2Int representing the average of all input vectors.</returns>
		public static Vector2Int Average(params Vector2Int[] vectors)
		{
			return Average(vectors as IEnumerable<Vector2Int>);
		}
		/// <summary>
		/// Calculates the average of multiple Vector2Int vectors from an enumerable collection.
		/// </summary>
		/// <param name="vectors">The enumerable collection of vectors to average.</param>
		/// <returns>A new Vector2Int representing the average of all input vectors.</returns>
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
		/// Calculates the average of multiple Quaternion vectors.
		/// </summary>
		/// <param name="quaternions">The quaternions to average.</param>
		/// <returns>A new Quaternion representing the average of all input quaternions.</returns>
		public static Quaternion Average(params Quaternion[] quaternions)
		{
			return Average(quaternions as IEnumerable<Quaternion>);
		}
		/// <summary>
		/// Calculates the average of multiple Quaternion vectors from an enumerable collection.
		/// </summary>
		/// <param name="quaternions">The enumerable collection of quaternions to average.</param>
		/// <returns>A new Quaternion representing the average of all input quaternions.</returns>
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
		/// </summary>
		/// <param name="bytes">The values to average.</param>
		/// <returns>The average of all input values, or 0 if the array is empty.</returns>
		public static byte Average(params byte[] bytes)
		{
			if (bytes.Length < 1)
				return 0;

			return (byte)(int)math.round(bytes.Select(@byte => (float)@byte).Average());
		}
		/// <summary>
		/// Calculates the average of multiple integer values.
		/// </summary>
		/// <param name="integers">The values to average.</param>
		/// <returns>The average of all input values, or 0 if the array is empty.</returns>
		public static int Average(params int[] integers)
		{
			if (integers.Length < 1)
				return 0;

			return (int)math.round((float)integers.Average());
		}
		/// <summary>
		/// Squares an integer value.
		/// </summary>
		/// <param name="number">The value to square.</param>
		/// <returns>The square of the input value.</returns>
		public static int Square(int number)
		{
			return number * number;
		}
		/// <summary>
		/// Squares a float value.
		/// </summary>
		/// <param name="number">The value to square.</param>
		/// <returns>The square of the input value.</returns>
		public static float Square(float number)
		{
			return number * number;
		}
		/// <summary>
		/// Finds the maximum value among multiple byte values.
		/// </summary>
		/// <param name="numbers">The values to compare.</param>
		/// <returns>The maximum value among the input values, or default if the array is empty.</returns>
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
		/// </summary>
		/// <param name="numbers">The values to compare.</param>
		/// <returns>The minimum value among the input values, or default if the array is empty.</returns>
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
		/// </summary>
		/// <param name="numbers">The values to compare.</param>
		/// <returns>The maximum value among the input values, or default if the array is empty.</returns>
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
		/// </summary>
		/// <param name="numbers">The values to compare.</param>
		/// <returns>The minimum value among the input values, or default if the array is empty.</returns>
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
		/// Clamps a float value between a minimum and maximum value.
		/// </summary>
		/// <param name="number">The value to clamp.</param>
		/// <param name="min">The minimum value.</param>
		/// <returns>The clamped value.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static float ClampInfinity(float number, float min = 0f)
		{
			return min >= 0f ? math.max(number, min) : math.min(number, min);
		}
		/// <summary>
		/// Clamps an integer value between a minimum and maximum value.
		/// </summary>
		/// <param name="number">The value to clamp.</param>
		/// <param name="min">The minimum value.</param>
		/// <returns>The clamped value.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static int ClampInfinity(int number, int min = 0)
		{
			return min >= 0 ? math.max(number, min) : math.min(number, min);
		}
		/// <summary>
		/// Clamps the absolute value of a float between a minimum value.
		/// </summary>
		/// <param name="number">The value to clamp.</param>
		/// <param name="min">The minimum value.</param>
		/// <returns>The clamped value.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static float ClampInfinityAbs(float number, float min = 0f)
		{
			return math.max(math.abs(number), min);
		}
		/// <summary>
		/// Clamps the absolute value of an integer between a minimum value.
		/// </summary>
		/// <param name="number">The value to clamp.</param>
		/// <param name="min">The minimum value.</param>
		/// <returns>The clamped value.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static int ClampInfinityAbs(int number, int min = 0)
		{
			return math.max(math.abs(number), min);
		}
		/// <summary>
		/// Clamps the absolute value of a vector3 between a minimum value.
		/// </summary>
		/// <param name="vector">The value to clamp.</param>
		/// <param name="min">The minimum value.</param>
		/// <returns>The clamped value.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static Vector3 ClampInfinity(Vector3 vector, float min = 0f)
		{
			return min >= 0f ? new Vector3(math.max(vector.x, min), math.max(vector.y, min), math.max(vector.z, min)) : new Vector3(math.min(vector.x, min), math.min(vector.y, min), math.min(vector.z, min));
		}
		/// <summary>
		/// Clamps the absolute value of a vector3 between a minimum value.
		/// </summary>
		/// <param name="vector">The value to clamp.</param>
		/// <param name="min">The minimum value.</param>
		/// <returns>The clamped value.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static Vector3 ClampInfinity(Vector3 vector, Vector3 min)
		{
			return Average(min.x, min.y, min.z) >= 0f ? new Vector3(math.max(vector.x, min.x), math.max(vector.y, min.y), math.max(vector.z, min.z)) : new Vector3(math.min(vector.x, min.x), math.min(vector.y, min.y), math.min(vector.z, min.z));
		}
		/// <summary>
		/// Clamps the absolute value of a vector2 between a minimum value.
		/// </summary>
		/// <param name="vector">The value to clamp.</param>
		/// <param name="min">The minimum value.</param>
		/// <returns>The clamped value.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static Vector2 ClampInfinity(Vector2 vector, float min = 0f)
		{
			return min >= 0f ? new Vector2(math.max(vector.x, min), math.max(vector.y, min)) : new Vector2(math.min(vector.x, min), math.min(vector.y, min));
		}
		/// <summary>
		/// Clamps the absolute value of a vector2 between a minimum value.
		/// </summary>
		/// <param name="vector">The value to clamp.</param>
		/// <param name="min">The minimum value.</param>
		/// <returns>The clamped value.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static Vector2 ClampInfinity(Vector2 vector, Vector2 min)
		{
			return Average(min.x, min.y) >= 0f ? new Vector2(math.max(vector.x, min.x), math.max(vector.y, min.y)) : new Vector2(math.min(vector.x, min.x), math.min(vector.y, min.y));
		}
		/// <summary>
		/// Clamps the absolute value of a float3 between a minimum value.
		/// </summary>
		/// <param name="vector">The value to clamp.</param>
		/// <param name="min">The minimum value.</param>
		/// <returns>The clamped value.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static float3 ClampInfinity(float3 vector, float min = 0f)
		{
			return min >= 0f ? new float3(math.max(vector.x, min), math.max(vector.y, min), math.max(vector.z, min)) : new float3(math.min(vector.x, min), math.min(vector.y, min), math.min(vector.z, min));
		}
		/// <summary>
		/// Clamps the absolute value of a float3 between a minimum value.
		/// </summary>
		/// <param name="vector">The value to clamp.</param>
		/// <param name="min">The minimum value.</param>
		/// <returns>The clamped value.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static float3 ClampInfinity(float3 vector, float3 min)
		{
			return Average(min.x, min.y, min.z) >= 0f ? new float3(math.max(vector.x, min.x), math.max(vector.y, min.y), math.max(vector.z, min.z)) : new float3(math.min(vector.x, min.x), math.min(vector.y, min.y), math.min(vector.z, min.z));
		}
		/// <summary>
		/// Clamps the absolute value of a float2 between a minimum value.
		/// </summary>
		/// <param name="vector">The value to clamp.</param>
		/// <param name="min">The minimum value.</param>
		/// <returns>The clamped value.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static float2 ClampInfinity(float2 vector, float min = 0f)
		{
			return min >= 0f ? new float2(math.max(vector.x, min), math.max(vector.y, min)) : new float2(math.min(vector.x, min), math.min(vector.y, min));
		}
		/// <summary>
		/// Clamps the absolute value of a float2 between a minimum value.
		/// </summary>
		/// <param name="vector">The value to clamp.</param>
		/// <param name="min">The minimum value.</param>
		/// <returns>The clamped value.</returns>
		[Obsolete("Use `Math.Min` or `Math.Max` instead.")]
		public static float2 ClampInfinity(float2 vector, float2 min)
		{
			return Average(min.x, min.y) >= 0f ? new float2(math.max(vector.x, min.x), math.max(vector.y, min.y)) : new float2(math.min(vector.x, min.x), math.min(vector.y, min.y));
		}
		/// <summary>
		/// Clamps a float value between 0 and 1.
		/// </summary>
		/// <param name="number">The value to clamp.</param>
		/// <returns>The clamped value.</returns>
		public static float Clamp01(float number)
		{
			return math.clamp(number, 0f, 1f);
		}
		/// <summary>
		/// Determines if two float3 vectors are approximately equal.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>True if the vectors are approximately equal, false otherwise.</returns>
		public static bool Approximately(Vector3 vector1, Vector3 vector2)
		{
			return Mathf.Approximately(vector1.x, vector2.x) && Mathf.Approximately(vector1.y, vector2.y) && Mathf.Approximately(vector1.z, vector2.z);
		}
		/// <summary>
		/// Determines if two float3 vectors are approximately equal.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>True if the vectors are approximately equal, false otherwise.</returns>
		public static bool Approximately(float3 vector1, float3 vector2)
		{
			return Mathf.Approximately(vector1.x, vector2.x) && Mathf.Approximately(vector1.y, vector2.y) && Mathf.Approximately(vector1.z, vector2.z);
		}
		/// <summary>
		/// Determines if two float2 vectors are approximately equal.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>True if the vectors are approximately equal, false otherwise.</returns>
		public static bool Approximately(Vector2 vector1, Vector2 vector2)
		{
			return Mathf.Approximately(vector1.x, vector2.x) && Mathf.Approximately(vector1.y, vector2.y);
		}
		/// <summary>
		/// Determines if two float2 vectors are approximately equal.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>True if the vectors are approximately equal, false otherwise.</returns>
		public static bool Approximately(float2 vector1, float2 vector2)
		{
			return Mathf.Approximately(vector1.x, vector2.x) && Mathf.Approximately(vector1.y, vector2.y);
		}
		/// <summary>
		/// Gets the timestamp of a given date and time.
		/// </summary>
		/// <param name="dateTime">The date and time to get the timestamp of.</param>
		/// <returns>The timestamp of the given date and time.</returns>
		public static long GetTimestamp(DateTime dateTime)
		{
			return long.Parse(dateTime.ToString("yyyyMMddHHmmssffff"));
		}
		/// <summary>
		/// Gets the timestamp of the current date and time.
		/// </summary>
		/// <param name="UTC">Whether to use UTC time.</param>
		/// <returns>The timestamp of the current date and time.</returns>
		public static long GetTimestamp(bool UTC = false)
		{
			return GetTimestamp(UTC ? DateTime.UtcNow : DateTime.Now);
		}
		/// <summary>
		/// Finds the intersection point of two line segments.
		/// </summary>
		/// <param name="p1">The first point of the first line segment.</param>
		/// <param name="p2">The second point of the first line segment.</param>
		/// <param name="p3">The first point of the second line segment.</param>
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
		/// Adds torque to a rigidbody at a specific position.
		/// </summary>
		/// <param name="rigid">The rigidbody to add torque to.</param>
		/// <param name="torque">The torque to add.</param>
		/// <param name="point">The position to add the torque at.</param>
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
		/// Checks if a directory is empty.
		/// </summary>
		/// <param name="path">The path to check.</param>
		/// <returns>True if the directory is empty, false otherwise.</returns>
		public static bool IsDirectoryEmpty(string path)
		{
			IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path);

#pragma warning disable IDE0063 // Use simple 'using' statement
			using (IEnumerator<string> entry = items.GetEnumerator())
				return !entry.MoveNext();
#pragma warning restore IDE0063 // Use simple 'using' statement
		}
		/// <summary>
		/// Creates a new audio source.
		/// </summary>
		/// <param name="sourceName">The name of the audio source.</param>
		/// <param name="minDistance">The minimum distance of the audio source.</param>
		/// <param name="maxDistance">The maximum distance of the audio source.</param>
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
		/// Gets the event listeners of a UnityEvent.
		/// </summary>
		/// <param name="unityEvent">The UnityEvent to get the event listeners of.</param>
		/// <returns>An array of the event listeners.</returns>
		public static string[] GetEventListeners(UnityEvent unityEvent)
		{
			List<string> result = new List<string>();

			for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
				result.Add(unityEvent.GetPersistentMethodName(i));

			return result.ToArray();
		}
		/// <summary>
		/// Clones a component from one game object to another.
		/// </summary>
		/// <typeparam name="T">The type of the component to clone.</typeparam>
		/// <param name="original">The original component to clone.</param>
		/// <param name="destination">The destination game object to clone the component to.</param>
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
		/// Gets a specific item from a Texture2DArray.
		/// </summary>
		/// <param name="array">The Texture2DArray to get the item from.</param>
		/// <param name="index">The index of the item to get.</param>
		/// <returns>The item from the Texture2DArray.</returns>
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
		/// Gets all items from a Texture2DArray.
		/// </summary>
		/// <param name="array">The Texture2DArray to get the items from.</param>
		/// <returns>An array of the items from the Texture2DArray.</returns>
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
		/// Saves a Texture2D to a file.
		/// </summary>
		/// <param name="texture">The Texture2D to save.</param>
		/// <param name="type">The type of the file to save.</param>
		/// <param name="path">The path to save the file to.</param>
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
		/// Takes a screenshot of a camera.
		/// </summary>
		/// <param name="camera">The camera to take the screenshot of.</param>
		/// <param name="size">The size of the screenshot.</param>
		/// <param name="depth">The depth of the screenshot.</param>
		public static Texture2D TakeScreenshot(Camera camera, Vector2Int size, int depth = 72)
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
		/// Converts a texture to a sprite.
		/// </summary>
		/// <param name="texture">The texture to convert.</param>
		/// <param name="pixelsPerUnit">The number of pixels per unit.</param>
		/// <returns>The sprite.</returns>
		public static Sprite TextureToSprite(Texture texture, float pixelsPerUnit = 100f)
		{
			if (texture is Texture2D)
				return Sprite.Create(texture as Texture2D, new Rect(0f, 0f, texture.width, texture.height), new Vector2(.5f, .5f), pixelsPerUnit);

			return null;
		}
		/// <summary>
		/// Gets the current render pipeline.
		/// </summary>
		/// <returns>The current render pipeline.</returns>
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
		/// Finds all game objects with a specific layer mask.
		/// </summary>
		/// <param name="layers">The layers to find the game objects with.</param>
		/// <param name="includeInactive">Whether to include inactive game objects.</param>
		/// <returns>An array of the game objects with the specific layer mask.</returns>
		public static GameObject[] FindGameObjectsWithLayerMask(string[] layers, bool includeInactive = true)
		{
			return FindGameObjectsWithLayerMask(LayerMask.GetMask(layers), includeInactive);
		}
		/// <summary>
		/// Finds all game objects with a specific layer mask.
		/// </summary>
		/// <param name="layerMask">The layer mask to find the game objects with.</param>
		/// <param name="includeInactive">Whether to include inactive game objects.</param>
		/// <returns>An array of the game objects with the specific layer mask.</returns>
		public static GameObject[] FindGameObjectsWithLayerMask(LayerMask layerMask, bool includeInactive = true)
		{
			return FindGameObjectsWithLayerMask(layerMask.value, includeInactive);
		}
		/// <summary>
		/// Finds all game objects with a specific layer mask.
		/// </summary>
		/// <param name="layerMask">The layer mask to find the game objects with.</param>
		/// <param name="includeInactive">Whether to include inactive game objects.</param>
		/// <returns>An array of the game objects with the specific layer mask.</returns>
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
		/// Gets the physics bounds of an object.
		/// </summary>
		/// <param name="gameObject">The game object to get the physics bounds of.</param>
		/// <param name="includeTriggers">Whether to include triggers.</param>
		/// <param name="keepRotation">Whether to keep the rotation of the object.</param>
		/// <param name="keepScale">Whether to keep the scale of the object.</param>
		/// <returns>The physics bounds of the object.</returns>
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
		/// Gets the bounds of an object.
		/// </summary>
		/// <param name="gameObject">The game object to get the bounds of.</param>
		/// <param name="keepRotation">Whether to keep the rotation of the object.</param>
		/// <param name="keepScale">Whether to keep the scale of the object.</param>
		/// <returns>The bounds of the object.</returns>
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
		/// Gets the bounds of a UI element.
		/// </summary>
		/// <param name="rectTransform">The RectTransform to get the bounds of.</param>
		/// <param name="scaleFactor">The scale factor of the UI element.</param>
		/// <returns>The bounds of the UI element.</returns>
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
		/// Checks if a point is in a collider.
		/// </summary>
		/// <param name="collider">The collider to check.</param>
		/// <param name="point">The point to check.</param>
		/// <returns>True if the point is in the collider, false otherwise.</returns>
		public static bool CheckPointInCollider(Collider collider, Vector3 point)
		{
			return CheckPointInCollider(collider, point, out _);
		}
		/// <summary>
		/// Checks if a point is in a collider.
		/// </summary>
		/// <param name="collider">The collider to check.</param>
		/// <param name="point">The point to check.</param>
		/// <param name="closestPoint">The closest point on the collider to the point.</param>
		/// <returns>True if the point is in the collider, false otherwise.</returns>
		public static bool CheckPointInCollider(Collider collider, Vector3 point, out Vector3 closestPoint)
		{
			closestPoint = collider.ClosestPoint(point);

			return closestPoint == point;
		}
		/// <summary>
		/// Destroys an object.
		/// </summary>
		/// <param name="immediate">Whether to destroy the object immediately.</param>
		/// <param name="obj">The object to destroy.</param>
		/// <returns>True if the object was destroyed, false otherwise.</returns>
		public static void Destroy(bool immediate, UnityEngine.Object obj)
		{
			if (immediate)
				UnityEngine.Object.DestroyImmediate(obj);
			else
				UnityEngine.Object.Destroy(obj);
		}
		/// <summary>
		/// Destroys an object.
		/// </summary>
		/// <param name="obj">The object to destroy.</param>
		/// <param name="time">The time to destroy the object.</param>
		/// <returns>True if the object was destroyed, false otherwise.</returns>
		public static void Destroy(UnityEngine.Object obj, float time)
		{
			UnityEngine.Object.Destroy(obj, time);
		}
		/// <summary>
		/// Destroys an object.
		/// </summary>
		/// <param name="obj">The object to destroy.</param>
		/// <param name="allowDestroyingAssets">Whether to allow destroying assets.</param>
		/// <returns>True if the object was destroyed, false otherwise.</returns>
		public static void Destroy(UnityEngine.Object obj, bool allowDestroyingAssets)
		{
			UnityEngine.Object.DestroyImmediate(obj, allowDestroyingAssets);
		}

		/// <summary>
		/// Checks if a unit is a divider unit.
		/// </summary>
		/// <param name="unit">The unit to check.</param>
		/// <returns>True if the unit is a divider unit, false otherwise.</returns>
		private static bool IsDividerUnit(Units unit)
		{
			return unit == Units.FuelConsumption;
		}
		/// <summary>
		/// Gets the sign of a point.
		/// </summary>
		/// <param name="point1">The first point.</param>
		/// <param name="point2">The second point.</param>
		/// <param name="point3">The third point.</param>
		/// <returns>The sign of the point.</returns>
		private static float PointSign(Vector2 point1, Vector2 point2, Vector2 point3)
		{
			return (point1.x - point3.x) * (point2.y - point3.y) - (point2.x - point3.x) * (point1.y - point3.y);
		}

		#endregion
	}
}
