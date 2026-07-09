using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MaterialAssetRestorerCore
{
    public class MaterialGet
    {
        /// <summary>
        /// Gets a material by name, optionally searching within a specific prefab or scene.
        /// </summary>
        /// <param name="materialToFind">The name of the material to find.</param>
        /// <param name="prefabToSearch">The name of the prefab to search within (optional).</param>
        /// <param name="sceneToSearch">The name of the scene to search within (optional).</param>
        /// <param name="onComplete">Callback invoked with the found material, or null if not found.</param>
        /// <returns>An IEnumerator to be used with StartCoroutine.</returns>
        public static IEnumerator GET_material(string materialToFind, string prefabToSearch=null, string sceneToSearch=null, System.Action<Material> onComplete = null) 
        {
            
            Material materialToReturn = null;

            if (string.IsNullOrEmpty(materialToFind)) 
            {
                MaterialAssetRestorerCore.Logger.LogWarning("Material to find is null.");
                onComplete?.Invoke(null);
                yield break;
            }

            // if a prefabToSearch is provided, try to find the material in that prefab first
            if (!string.IsNullOrEmpty(prefabToSearch))
            {
                GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (GameObject prefab in objects)
                {
                    if (prefab.name == prefabToSearch)
                    {
                        MaterialAssetRestorerCore.Logger.LogDebug($"Found a prefab '{prefabToSearch}'.");
                        materialToReturn = GetFromRenderers(prefab, materialToFind);
                        if (materialToReturn != null){break;} //only stop checking prefabs after matierial is found as there may be multiple prefabs with the same name
                    }
                }
            }

            // check scenes if one is provided (and a prefab wasn't)
            else if (!string.IsNullOrEmpty(sceneToSearch))
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneToSearch);

                if (sceneToSearch != "SampleSceneRelay" && !scene.isLoaded) //don't load SampleSceneRelay (any scene, really, but mainly that one) twice as I fear that will be bad
                {
                    MaterialAssetRestorerCore.Logger.LogDebug($"Attempting to access scene '{sceneToSearch}'.");
                    yield return SceneManager.LoadSceneAsync(sceneToSearch, LoadSceneMode.Additive);
                    scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneToSearch);
                    MaterialAssetRestorerCore.Logger.LogDebug($"Found scene '{sceneToSearch}'? {scene.isLoaded}");
                    
                    if (!scene.IsValid())
                    {
                        MaterialAssetRestorerCore.Logger.LogWarning($"Scene '{sceneToSearch}' not found.");
                        onComplete?.Invoke(null);
                        yield break;
                    }
                }

                //find materials
                GameObject[] gameObjects = scene.GetRootGameObjects(); //only search the specified scene, avoiding searching SampleSceneRelay that's always loaded (unless it is the target scene of course)
                foreach (GameObject gameObject in gameObjects) 
                {
                    materialToReturn = GetFromRenderers(gameObject, materialToFind);
                    if (materialToReturn != null){break; } //stop checking gameobjects after material is found
                }

                //unload scene (never SampleSceneRelay though)
                if(sceneToSearch != "SampleSceneRelay" && scene.isLoaded) { 
                    yield return SceneManager.UnloadSceneAsync(scene);
                    MaterialAssetRestorerCore.Logger.LogDebug($"Unloaded scene '{sceneToSearch}'.");
                }
            }

            MaterialAssetRestorerCore.Logger.LogInfo($"Material '{materialToFind}' search completed. Found: {(materialToReturn != null ? "Yes" : "No")}");
            onComplete?.Invoke(materialToReturn); // send the found material back to the caller
        }

        /// <summary>
        /// Takes a GameObject and checks all of its and its children's renderers for the material, returning it if found.
        /// </summary>
        /// <param name="objToSearch">The GameObject to search through (including its children).</param>
        /// <param name="materialToFind">The name of the material to find.</param>
        /// <returns>The found material, or null if not found.</returns>
        private static Material GetFromRenderers(GameObject objToSearch, string materialToFind)
        {
            foreach (Renderer renderer in objToSearch.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.sharedMaterial != null && renderer.sharedMaterial.name == materialToFind && renderer.sharedMaterial.shader.name != "Hidden/InternalErrorShader") //if mods add prefabs of the same name (like Wesley's CaveWaterTile being named same as vanilla's CaveWaterTile), ensure we get the non-broken one.
                {
                    MaterialAssetRestorerCore.Logger.LogDebug($"Found material '{materialToFind}' in '{objToSearch}'.");
                    return renderer.sharedMaterial;
                }
            }
            return null;
        }
    }
}
