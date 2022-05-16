#region Namespaces

using System;
using UnityEngine;
using UnityEditor;

#endregion

namespace Utilities
{
	public static class URPEditorUtilities
	{
		[MenuItem("Tools/Utilities/URP/Upgrade selected terrains to URP"), Obsolete]
		public static void UpgradeTerrainsToHDRP()
		{
			if (!Selection.activeGameObject)
				return;

			for (int i = 0; i < Selection.gameObjects.Length; i++)
			{
				Terrain terrain = Selection.gameObjects[i].GetComponent<Terrain>();
				
				if (terrain)
				{
					terrain.materialType = Terrain.MaterialType.Custom;
					terrain.materialTemplate = new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"))
					{
						name = "TerrainLitMaterial"
					};
					terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbesAndSkybox;
				}
			}
		}
		[MenuItem("Tools/Utilities/URP/Reset selected terrains to Standard"), Obsolete]
		public static void ResetTerrainsToStandard()
		{
			if (!Selection.activeGameObject)
				return;

			for (int i = 0; i < Selection.gameObjects.Length; i++)
			{
				Terrain terrain = Selection.gameObjects[i].GetComponent<Terrain>();

				if (terrain)
				{
					terrain.materialType = Terrain.MaterialType.BuiltInStandard;
					terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
				}
			}
		}
		[MenuItem("Tools/Utilities/URP/Upgrade selected terrains to URP", true)]
		[MenuItem("Tools/Utilities/URP/Reset selected terrains to Standard", true)]
		public static bool UpgradeOrResetTerrainsCheck()
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
	}
}
