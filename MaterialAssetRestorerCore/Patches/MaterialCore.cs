using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;


namespace MaterialAssetRestorerCore
{
    [HarmonyPatch(typeof(StartOfRound))]
    public class MaterialInit
    {
        public static List<MaterialInformationContainer> materialInformationContainers = new List<MaterialInformationContainer>();

        public static Material WAR_company_flooded = null;
        public static Material WAR_cave = null;
        public static Material WAR_pool = null;
        public static Material WAR_adamance_march_vow = null;

        //after StartOfRound, initialize the materials
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void InitializeMaterials()
        {
            MaterialAssetRestorerCore.Logger.LogInfo("Initializing materials...");
            foreach (MaterialInformationContainer container in materialInformationContainers)
            {
                Material foundMaterial = MaterialGet.GET_material(container.BaseMaterial, container.PrefabName, container.SceneName);
                container.replacementMaterial = foundMaterial;
            }
            MaterialAssetRestorerCore.Logger.LogInfo("Finished initializing materials.");
        }
    }

    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.FinishGeneratingNewLevelClientRpc))]
    public class WaterSwap
    {
        [HarmonyPostfix]
        public static void SwapWaterMaterials()
        {
            foreach (MaterialInformationContainer container in MaterialInit.materialInformationContainers)
            {
                if (container.replacementMaterial != null)
                {
                    MaterialSet.SET_material(container.ReplaceMaterial, container.replacementMaterial);
                }
            }
        }
    }

    /// <summary>
    /// Contains information required to identify and replace materials within a specific prefab and scene.
    /// <c>BaseMaterial</c> - The name of the material to find.
    /// <c>ReplaceMaterial</c> - The name of the material to replace.
    /// <c>PrefabName</c> - The name of the prefab to search within.
    /// <c>SceneName</c> - The name of the scene to search within.
    /// <c>replacementMaterial</c> - The material itself to use as a replacement, found at runtime.
    /// </summary>
    public class MaterialInformationContainer
    {
        public MaterialInformationContainer(string materialToFindName, string materialToReplaceName, string prefabToSearchName, string sceneToSearchName, Material replacementMaterial)
        {
            this.BaseMaterial = materialToFindName;
            this.ReplaceMaterial = materialToReplaceName;
            this.PrefabName = prefabToSearchName;
            this.SceneName = sceneToSearchName;
            this.replacementMaterial = replacementMaterial;
        }
        public string BaseMaterial = null;
        public string ReplaceMaterial = null;
        public string PrefabName = null;
        public string SceneName = null;
        public Material replacementMaterial = null;
    }
}