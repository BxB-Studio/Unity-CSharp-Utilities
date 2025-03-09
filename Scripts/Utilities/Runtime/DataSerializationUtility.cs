#region Namespaces

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

#endregion

namespace Utilities
{
	/// <summary>
	/// Utility class for serializing and deserializing data to a file.
	/// </summary>
	/// <typeparam name="T">The type of data to serialize and deserialize.</typeparam>
	public class DataSerializationUtility<T> where T : class
	{
		#region Variables

		/// <summary>
		/// The path to save and load the data.
		/// </summary>
		private readonly string path;
		/// <summary>
		/// Whether to load the data from resources.
		/// </summary>
		private readonly bool useResources;
		/// <summary>
		/// Whether to bypass exceptions.
		/// </summary>
		private readonly bool bypassExceptions;

		#endregion

		#region Methods

		#region Utilities

		/// <summary>
		/// Saves data to a file.
		/// </summary>
		/// <param name="data">The data to save.</param>
		/// <returns>True if the data was saved, false otherwise.</returns>
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
		/// Loads data from a file.
		/// </summary>
		/// <returns>The loaded data.</returns>
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
		/// Deletes a file.
		/// </summary>
		/// <returns>True if the file was deleted, false otherwise.</returns>
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
		/// Checks if the path is valid.
		/// </summary>
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
		/// Initializes a new instance of the DataSerializationUtility class.
		/// </summary>
		/// <param name="path">The path to save and load the data.</param>
		/// <param name="loadFromResources">Whether to load the data from resources.</param>
		/// <param name="bypassExceptions">Whether to bypass exceptions.</param>
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
		/// Implicitly converts a DataSerializationUtility<T> to a boolean.
		/// </summary>
		/// <param name="serializationUtility">The DataSerializationUtility<T> to convert.</param>
		/// <returns>True if the DataSerializationUtility<T> is not null, false otherwise.</returns>
		public static implicit operator bool(DataSerializationUtility<T> serializationUtility) => serializationUtility != null;

		#endregion

		#endregion

		#endregion
	}
}
