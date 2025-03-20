#region Namespaces

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

#endregion

namespace Utilities
{
	/// <summary>
	/// Utility class for serializing and deserializing data to and from files.
	/// Provides methods for saving, loading, and deleting serialized data with support for both regular file paths and Unity Resources.
	/// </summary>
	/// <typeparam name="T">The type of data to serialize and deserialize. Must be a reference type.</typeparam>
	public class DataSerializationUtility<T> where T : class
	{
		#region Variables

		/// <summary>
		/// The path to save and load the data.
		/// For regular file operations, this is a file system path.
		/// For Resources, this is the path within the Resources folder without the extension.
		/// </summary>
		private readonly string path;
		
		/// <summary>
		/// Whether to load the data from Unity's Resources system instead of the file system.
		/// When true, data is loaded from Resources using the path as a resource path.
		/// When false, data is loaded directly from the file system.
		/// </summary>
		private readonly bool useResources;
		
		/// <summary>
		/// Whether to bypass exceptions by returning default values instead of throwing errors.
		/// When true, methods will return null or false on failure instead of throwing exceptions.
		/// When false, exceptions will be propagated to the caller.
		/// </summary>
		private readonly bool bypassExceptions;

		#endregion

		#region Methods

		#region Utilities

		/// <summary>
		/// Saves data to a file at the specified path.
		/// Creates the directory structure if it doesn't exist.
		/// Appends ".bytes" extension when saving for Resources.
		/// </summary>
		/// <param name="data">The data object to serialize and save.</param>
		/// <returns>True if the data was saved successfully, false if an error occurred and exceptions are bypassed.</returns>
		/// <exception cref="Exception">Thrown when an error occurs during saving and bypassExceptions is false.</exception>
		public bool SaveOrCreate(T data)
		{
			CheckValidity();

			FileStream stream = null;

			try
			{
				if (!Directory.Exists(Path.GetDirectoryName(path)))
					Directory.CreateDirectory(Path.GetDirectoryName(path));

				stream = File.Open($"{path}{(useResources ? ".bytes" : "")}", FileMode.OpenOrCreate);

				BinaryFormatter formatter = new BinaryFormatter();

				formatter.Serialize(stream, data);

				return true;
			}
			catch (Exception e)
			{
				if (!bypassExceptions)
					throw e;
				else
					return false;
			}
			finally
			{
				stream?.Close();
			}
		}
		
		/// <summary>
		/// Loads and deserializes data from a file at the specified path.
		/// For Resources, loads from a TextAsset. For regular files, loads directly from the file system.
		/// </summary>
		/// <returns>The deserialized data object, or null if the file doesn't exist or an error occurred and exceptions are bypassed.</returns>
		/// <exception cref="ArgumentException">Thrown when the file doesn't exist and bypassExceptions is false.</exception>
		/// <exception cref="Exception">Thrown when an error occurs during loading and bypassExceptions is false.</exception>
		public T Load()
		{
			CheckValidity();

			if (useResources && !Resources.Load<TextAsset>(path) || !useResources && !File.Exists(path))
			{
				if (bypassExceptions)
					return null;
				else
					throw new ArgumentException($"The file ({path}) doesn't exist");
			}

			Stream stream = null;

			try
			{
				if (useResources)
					stream = new MemoryStream(Resources.Load<TextAsset>(path).bytes);
				else
					stream = File.Open(path, FileMode.OpenOrCreate);

				BinaryFormatter formatter = new BinaryFormatter();
				T data = formatter.Deserialize(stream) as T;

				return data;
			}
			catch (Exception e)
			{
				if (bypassExceptions)
					return null;
				else
					throw e;
			}
			finally
			{
				stream?.Close();
			}
		}
		
		/// <summary>
		/// Deletes the file at the specified path and its associated .meta file if it exists.
		/// Cannot delete files from Resources as they are compiled into the application.
		/// </summary>
		/// <returns>True if the file was deleted successfully, false if the file doesn't exist, is a resource, or an error occurred and exceptions are bypassed.</returns>
		/// <exception cref="FileNotFoundException">Thrown when the file doesn't exist and bypassExceptions is false.</exception>
		/// <exception cref="Exception">Thrown when an error occurs during deletion and bypassExceptions is false.</exception>
		public bool Delete()
		{
			CheckValidity();

			if (useResources)
			{
				Debug.LogError("You can't delete a resource file, use normal path finding instead!");

				return false;
			}

			if (!File.Exists(path))
			{
				if (!bypassExceptions)
					throw new FileNotFoundException($"We couldn't delete ({path}), as it doesn't exist!");

				return false;
			}

			try
			{
				File.Delete(path);

				string metaFilePath = $"{path}.meta";

				if (File.Exists(metaFilePath))
					File.Delete(metaFilePath);
			}
			catch (Exception e)
			{
				if (!bypassExceptions)
					throw e;

				return false;
			}

			return true;
		}

		/// <summary>
		/// Validates the path and ensures the directory structure exists.
		/// For non-resource paths, creates the directory if it doesn't exist and verifies it's a valid directory.
		/// For resource paths, no validation is performed as Resources are read-only.
		/// </summary>
		/// <exception cref="DirectoryNotFoundException">Thrown when the path is not a valid directory.</exception>
		private void CheckValidity()
		{
			if (useResources)
				return;

			if (!Directory.Exists(Path.GetDirectoryName(path)))
				Directory.CreateDirectory(Path.GetDirectoryName(path));

			FileAttributes fileAttributes = File.GetAttributes(Path.GetDirectoryName(path));

			if (!fileAttributes.HasFlag(FileAttributes.Directory))
				throw new DirectoryNotFoundException($"The `path` argument of value \"{Path.GetDirectoryName(path)}\" must be a valid directory");
		}

		#endregion

		#region Constructors & Operators

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the DataSerializationUtility class with the specified parameters.
		/// Validates the path upon initialization to ensure it's ready for operations.
		/// </summary>
		/// <param name="path">The path to save and load the data. For Resources, this is the path within the Resources folder without extension.</param>
		/// <param name="loadFromResources">Whether to load the data from Unity's Resources system instead of the file system.</param>
		/// <param name="bypassExceptions">Whether to bypass exceptions by returning default values instead of throwing errors. Defaults to false.</param>
		public DataSerializationUtility(string path, bool loadFromResources, bool bypassExceptions = false)
		{
			this.path = path;
			useResources = loadFromResources;
			this.bypassExceptions = bypassExceptions;

			CheckValidity();
		}

		#endregion

		#region Operators

		/// <summary>
		/// Implicitly converts a DataSerializationUtility<T> instance to a boolean value.
		/// Allows for null-checking syntax like: if(serializationUtility) { ... }
		/// </summary>
		/// <param name="serializationUtility">The DataSerializationUtility<T> instance to convert.</param>
		/// <returns>True if the DataSerializationUtility<T> instance is not null, false otherwise.</returns>
		public static implicit operator bool(DataSerializationUtility<T> serializationUtility) => serializationUtility != null;

		#endregion

		#endregion

		#endregion
	}
}
