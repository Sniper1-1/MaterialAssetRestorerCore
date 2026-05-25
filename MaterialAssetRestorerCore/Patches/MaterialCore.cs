using System.Collections;
using System.Collections.Generic;
using DunGen;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace MaterialAssetRestorerCore
{
    [HarmonyPatch(typeof(StartOfRound))]
    public class MaterialInit
    {
        public static List<MaterialInformationContainer> materialInformationContainers = new List<MaterialInformationContainer>();

        //after StartOfRound, initialize the materials
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
        public static void InitializeMaterialsPatch()
        {
            CoroutineHelper.Instance.StartCoroutine(MaterialInit.InitializeMaterialsCoroutine());
        }
        public static IEnumerator InitializeMaterialsCoroutine()
        {
            MaterialsNetworkSync.materialsInitialized.Value = false;
            SceneLoadPatches.MuteSceneStateChangeEvents(); //prevent other mods from running code on scene load/unload
            MaterialAssetRestorerCore.Logger.LogInfo("Initializing materials...");
            foreach (MaterialInformationContainer container in materialInformationContainers)
            {
                yield return MaterialGet.GET_material(container.BaseMaterial, container.PrefabName, container.SceneName, (foundMaterial) =>
                {
                    if (foundMaterial != null)
                    {
                        container.replacementMaterial = foundMaterial;
                    }
                });
            }
            yield return new WaitForSeconds(10); //debug testing. REMOVE THIS
            SceneLoadPatches.UnmuteSceneStateChangeEvents(); //allow other mods to run code on scene load/unload again
            MaterialAssetRestorerCore.Logger.LogInfo("Finished initializing materials.");
            MaterialsNetworkSync.materialsInitialized.Value = true;
        }
    }

    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.FinishGeneratingNewLevelClientRpc))]
    public class WaterSwap
    {
        [HarmonyPostfix]
        public static void SwapWaterMaterials(RoundManager __instance)
        {
            Scene sceneToReplace = SceneManager.GetSceneByName(__instance.currentLevel.sceneName); //get the newly loaded moon scene (no need to search SampleSceneRelay's renders to replace)
            foreach (MaterialInformationContainer container in MaterialInit.materialInformationContainers)
            {
                if (container.replacementMaterial != null)
                {
                    MaterialSet.SET_material(container.ReplaceMaterial, container.replacementMaterial, sceneToReplace);
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

    //patch scene load/unload to prevent other mods from running things if they are subscribed to these events
    [HarmonyPatch(typeof(SceneManager))]
    public class SceneLoadPatches
    {
        public static void MuteSceneStateChangeEvents()
        {
            MaterialAssetRestorerCore.Harmony?.PatchAll(typeof(SceneLoadPatches)); //patch scene load/unload to prevent other mods from running things if they are subscribed to these events
            MaterialAssetRestorerCore.Logger.LogDebug("Suppressed scene load/unload events from triggering.");
        }
        public static void UnmuteSceneStateChangeEvents()
        {
            MaterialAssetRestorerCore.Harmony?.Unpatch(AccessTools.Method(typeof(SceneManager), "Internal_SceneLoaded"), HarmonyPatchType.All, MaterialAssetRestorerCore.Harmony.Id);
            MaterialAssetRestorerCore.Harmony?.Unpatch(AccessTools.Method(typeof(SceneManager), "Internal_SceneUnloaded"), HarmonyPatchType.All, MaterialAssetRestorerCore.Harmony.Id);
            MaterialAssetRestorerCore.Logger.LogDebug("Re-enabled scene load/unload events.");
        }
        [HarmonyPrefix]
        [HarmonyPriority(800)] //make sure this runs before other potential patches
        [HarmonyPatch(nameof(SceneManager.Internal_SceneLoaded))]
        public static bool MuteSceneLoadedEventTrigger()
        {
            return false;
        }
        [HarmonyPrefix]
        [HarmonyPriority(800)] //make sure this runs before other potential patches
        [HarmonyPatch(nameof(SceneManager.Internal_SceneUnloaded))]
        public static bool MuteSceneUnloadedEventTrigger()
        {
            return false;
        }
    }
}