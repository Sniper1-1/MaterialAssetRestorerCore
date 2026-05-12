using UnityEngine;

namespace MaterialAssetRestorerCore
{
    public class MaterialGet
    {
        /// <summary>
        /// Gets a material by name, optionally searching within a specific prefab.
        /// </summary>
        /// <param name="materialToFind">The name of the material to find.</param>
        /// <param name="prefabToSearch">The name of the prefab to search within (optional).</param>
        /// <returns>The found material, or null if not found.</returns>
        public static Material GET_material(string materialToFind, string prefabToSearch=null) 
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

            // else we check the current scene
            else
            {
                var renderers = GameObject.FindObjectsOfType<Renderer>(true);
                foreach (var renderer in renderers)
                {
                    if (renderer.sharedMaterial!=null && renderer.sharedMaterial.name == materialToFind)
                    {
                        MaterialAssetRestorerCore.Logger.LogDebug($"Found material '{materialToFind}' in scene.");
                        materialToReturn = renderer.sharedMaterial;
                    }
                }
            }

            MaterialAssetRestorerCore.Logger.LogInfo($"Material '{materialToFind}' search completed. Found: {(materialToReturn != null ? "Yes" : "No")}");
            return materialToReturn;
        }
    }
}
