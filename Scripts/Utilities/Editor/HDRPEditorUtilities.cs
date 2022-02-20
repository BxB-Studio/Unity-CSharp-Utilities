using System;
using UnityEngine;
using UnityEditor;

namespace Utilities
{
	public static class HDRPEditorUtilities
	{
		[MenuItem("Tools/Utilities/HDRP/Upgrade selected terrains to HDRP", true)]
		public static bool UpgradeTerrainsToHDRPCheck()
		{
			if (Selection.activeGameObject)
			{
				for (int i = 0; i < Selection.gameObjects.Length; i++)
					if (!Selection.gameObjects[i].GetComponent<Terrain>())
						return false;

				return true;
			}

			return false;
		}
		[MenuItem("Tools/Utilities/HDRP/Upgrade selected terrains to HDRP"), Obsolete]
		public static void UpgradeTerrainsToHDRP()
		{
			if (!Selection.activeGameObject)
				return;

			for (int i = 0; i < Selection.gameObjects.Length; i++)
				if (Selection.gameObjects[i].GetComponent<Terrain>())
				{
					Terrain terrain = Selection.gameObjects[i].GetComponent<Terrain>();

					terrain.materialType = Terrain.MaterialType.Custom;

					terrain.materialTemplate = new Material(Shader.Find("HDRP/TerrainLit"))
					{
						name = "TerrainLitMaterial"
					};

					terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbesAndSkybox;
				}
		}
		[MenuItem("Tools/Utilities/HDRP/Reset selected terrains to Standard", true)]
		public static bool ResetTerrainsToStandardCheck()
		{
			if (Selection.activeGameObject)
			{
				for (int i = 0; i < Selection.gameObjects.Length; i++)
					if (!Selection.gameObjects[i].GetComponent<Terrain>())
						return false;

				return true;
			}

			return false;
		}
		[MenuItem("Tools/Utilities/HDRP/Reset selected terrains to Standard"), Obsolete]
		public static void ResetTerrainsToStandard()
		{
			if (!Selection.activeGameObject)
				return;

			for (int i = 0; i < Selection.gameObjects.Length; i++)
				if (Selection.gameObjects[i].GetComponent<Terrain>())
				{
					Terrain terrain = Selection.gameObjects[i].GetComponent<Terrain>();

					terrain.materialType = Terrain.MaterialType.BuiltInStandard;

					terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
				}
		}
	}
}
