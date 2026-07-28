using System.Collections;
using System.Collections.Generic;
using DunGen;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace MaterialAssetRestorerCore
{
    // runs on player connect to lobby to kick off caching the desired materials
    [HarmonyPatch(typeof(StartMatchLever))]
    public class MaterialInit
    {
        public static List<MaterialInformationContainer> materialInformationContainers = new List<MaterialInformationContainer>();

        //after StartMatchLever's Start method, initialize the materials
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartMatchLever), nameof(StartMatchLever.Start))]
        public static void InitializeMaterialsPatch()
        {
            CoroutineHelper.Instance.StartCoroutine(MaterialInit.InitializeMaterialsCoroutine()); //coroutine so that cachign can be done async
        }
        public static IEnumerator InitializeMaterialsCoroutine()
        {
            if (!MaterialsNetworkSync.waitingPlayerCount.IsInitialized)
            {
                yield return new WaitUntil(() => MaterialsNetworkSync.waitingPlayerCount.IsInitialized); //wait until waitingPlayerCount is initialized before trying to use it (mainly for joining players as it is initialized for host player when save selected)
            }
            MaterialsNetworkSync.waitingPlayerCount.Value++;
            SceneLoadPatches.MuteSceneStateChangeEvents(); //prevent other mods from running code on scene load/unload
            MaterialAssetRestorerCore.Logger.LogInfo("Initializing materials...");
            foreach (MaterialInformationContainer container in materialInformationContainers)
            {
                yield return MaterialGet.GET_material(container.BaseMaterial, container.PrefabName, container.SceneName, container.MaterialSource, (foundMaterial) =>
                {
                    if (foundMaterial != null)
                    {
                        container.replacementMaterial = foundMaterial;
                    }
                });
            }
            SceneLoadPatches.UnmuteSceneStateChangeEvents(); //allow other mods to run code on scene load/unload again
            MaterialAssetRestorerCore.Logger.LogInfo("Finished initializing materials.");
            MaterialsNetworkSync.waitingPlayerCount.Value--;
        }
    }

    // runs after a new moon fully loads to swap the broken materials for the ones we cached at the start of the round
    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.FinishGeneratingNewLevelClientRpc))]
    public class MaterialSwap
    {
        [HarmonyPostfix]
        public static void SwapMaterials(RoundManager __instance)
        {
            Scene sceneToReplace = SceneManager.GetSceneByName(__instance.currentLevel.sceneName); //get the newly loaded moon scene (no need to search SampleSceneRelay's renders to replace)
            foreach (MaterialInformationContainer container in MaterialInit.materialInformationContainers)
            {
                if (container.replacementMaterial != null)
                {
                    MaterialSet.SET_material(container.ReplaceMaterial, container.replacementMaterial, sceneToReplace, container.MaterialDestination);
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
    /// <c>MaterialSource</c> - The type of the source material (from either Renderer, TerrainDetails, ParticleSystem, or VFX).
    /// <c>MaterialDestination</c> - The type of the destination material (replace into either Renderer, TerrainDetails, ParticleSystem, or VFX).
    /// </summary>
    public class MaterialInformationContainer
    {
        public MaterialInformationContainer(string materialToFindName, string materialToReplaceName, string prefabToSearchName, string sceneToSearchName, Material replacementMaterial, MaterialType materialSource, MaterialType materialDestination)
        {
            this.BaseMaterial = materialToFindName;
            this.ReplaceMaterial = materialToReplaceName;
            this.PrefabName = prefabToSearchName;
            this.SceneName = sceneToSearchName;
            this.replacementMaterial = replacementMaterial;
            this.MaterialSource = materialSource;
            this.MaterialDestination = materialDestination;
        }
        public string BaseMaterial = null;
        public string ReplaceMaterial = null;
        public string PrefabName = null;
        public string SceneName = null;
        public Material replacementMaterial = null;
        public enum MaterialType
        {
            Renderer,
            TerrainDetails,
            ParticleSystem,
            VFX
        }
        // These fields can be null for backwards compatibility
        public MaterialType? MaterialSource=MaterialType.Renderer; 
        public MaterialType? MaterialDestination=MaterialType.Renderer;
    }

    //patch scene load/unload to prevent other mods from running things if they are subscribed to these events
    [HarmonyPatch(typeof(SceneManager))]
    public class SceneLoadPatches
    {
        /// <summary>
        /// applies patches to scene load/unload to prevent other mods from running things if they are subscribed to these events
        /// </summary>
        public static void MuteSceneStateChangeEvents()
        {
            MaterialAssetRestorerCore.Harmony?.PatchAll(typeof(SceneLoadPatches)); 
            MaterialAssetRestorerCore.Logger.LogDebug("Suppressed scene load/unload events from triggering.");
        }
        /// <summary>
        /// removes patches to scene load/unload to allow other mods to run things if they are subscribed to these events again
        /// </summary>
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