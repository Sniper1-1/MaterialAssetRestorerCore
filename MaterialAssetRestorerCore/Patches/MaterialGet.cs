using System.Collections;
using System.Linq;
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
        /// <returns>The found material, or null if not found.</returns>
        public static Material GET_material(string materialToFind, string prefabToSearch=null, string sceneToSearch=null) 
        {
            
            Material materialToReturn = null;

            if (string.IsNullOrEmpty(materialToFind)) 
            {
                MaterialAssetRestorerCore.Logger.LogWarning("Material to find is null.");
                return null; 
            }

            // if a prefabToSearch is provided, try to find the material in that prefab first
            if (!string.IsNullOrEmpty(prefabToSearch))
            {
                GameObject prefab = null;
                var objects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var obj in objects)
                {
                    if (obj.name == prefabToSearch)
                    {
                        prefab = obj;   
                    }
                }

                if (prefab == null)
                {
                    MaterialAssetRestorerCore.Logger.LogWarning($"Prefab '{prefabToSearch}' not found.");
                    return null;
                }
                else
                {
                    MaterialAssetRestorerCore.Logger.LogDebug($"Found prefab '{prefabToSearch}'.");
                    var renderers = prefab.GetComponentsInChildren<Renderer>(true);
                    foreach (var renderer in renderers)
                    {
                        if (renderer.sharedMaterial.name == materialToFind)
                        {
                            MaterialAssetRestorerCore.Logger.LogDebug($"Found material '{materialToFind}' in prefab '{prefabToSearch}'.");
                            materialToReturn= renderer.sharedMaterial;
                        }
                    }
                }
            }

            else if (!string.IsNullOrEmpty(sceneToSearch))
            {
                if (sceneToSearch != "SampleSceneRelay") //don't load SampleSceneRelay twice as I fear that will be bad
                {
                    MaterialAssetRestorerCore.Logger.LogDebug($"Found scene '{sceneToSearch}'.");
                    SceneManager.LoadScene(sceneToSearch, LoadSceneMode.Additive);

                    var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneToSearch);
                    if (scene.IsValid()) { 
                        MaterialAssetRestorerCore.Logger.LogDebug($"Loaded scene '{sceneToSearch}'.");
                    }
                    if (!scene.IsValid())
                    {
                        MaterialAssetRestorerCore.Logger.LogWarning($"Scene '{sceneToSearch}' not found.");
                        return null;
                    }
                }

                var renderers = GameObject.FindObjectsOfType<Renderer>(true);
                foreach (var renderer in renderers)
                {
                    if (renderer.sharedMaterial != null && renderer.sharedMaterial.name == materialToFind)
                    {
                        MaterialAssetRestorerCore.Logger.LogDebug($"Found material '{materialToFind}' in {sceneToSearch}.");
                        materialToReturn = renderer.sharedMaterial;
                    }
                }

                if(sceneToSearch != "SampleSceneRelay") { 
                    SceneManager.UnloadSceneAsync(sceneToSearch);
                    MaterialAssetRestorerCore.Logger.LogDebug($"Unloaded scene '{sceneToSearch}'.");
                }
            }

            // else we check the current scene (most likely SampleSceneRelay)
            //else
            //{
            //    var renderers = GameObject.FindObjectsOfType<Renderer>(true);
            //    foreach (var renderer in renderers)
            //    {
            //        if (renderer.sharedMaterial != null && renderer.sharedMaterial.name == materialToFind)
            //        {
            //            MaterialAssetRestorerCore.Logger.LogDebug($"Found material '{materialToFind}' in current scene.");
            //            materialToReturn = renderer.sharedMaterial;
            //        }
            //    }
            //}

            MaterialAssetRestorerCore.Logger.LogInfo($"Material '{materialToFind}' search completed. Found: {(materialToReturn != null ? "Yes" : "No")}");
            return materialToReturn;
        }
    }
}
