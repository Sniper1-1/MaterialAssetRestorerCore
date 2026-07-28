using UnityEngine;
using UnityEngine.SceneManagement;

namespace MaterialAssetRestorerCore
{
    public class MaterialSet
    {
        /// <summary>
        /// Goes through all renderers in the scene and replaces the material with the original name with the replacement material.
        /// </summary>
        /// <param name="original">The original material name.</param>
        /// <param name="replacement">The replacement material.</param>
        /// <param name="sceneToReplace">The scene to perform the replacement in.</param>
        /// <param name="materialDestination">The type of the destination material (optional).</param>"
        public static void SET_material(string original, Material replacement, Scene sceneToReplace, MaterialInformationContainer.MaterialType? materialDestination=null)
        {
            foreach (GameObject rootObj in sceneToReplace.GetRootGameObjects())
            {
                //replace materials in renderers (default)
                if (materialDestination == null || materialDestination == MaterialInformationContainer.MaterialType.Renderer)
                {
                    foreach (var renderer in rootObj.GetComponentsInChildren<Renderer>(true))
                    {
                        var sharedMaterials = renderer.sharedMaterials;
                        bool changed = false;
                        for (int i = 0; i < sharedMaterials.Length; i++)
                        {
                            if (sharedMaterials[i] != null && sharedMaterials[i].name == original)
                            {
                                sharedMaterials[i] = replacement;
                                changed = true;
                                MaterialAssetRestorerCore.Logger.LogInfo($"Replaced material '{original}' with '{replacement.name}' in renderer '{renderer.gameObject.name}' in scene '{sceneToReplace.name}'.");
                            }
                        }
                        if (changed)
                        {
                            renderer.sharedMaterials = sharedMaterials;
                        }
                    }
                }
                //replace materials in particle systems
                else if (materialDestination == MaterialInformationContainer.MaterialType.ParticleSystem)
                {
                    foreach (var particleSystem in rootObj.GetComponentsInChildren<ParticleSystem>(true))
                    {
                        var system = particleSystem.GetComponent<ParticleSystemRenderer>();
                        if (system != null)
                        {
                            var sharedMaterials = system.sharedMaterials;
                            bool changed = false;
                            for (int i = 0; i < sharedMaterials.Length; i++)
                            {
                                if (sharedMaterials[i] != null && sharedMaterials[i].name == original)
                                {
                                    sharedMaterials[i] = replacement;
                                    changed = true;
                                    MaterialAssetRestorerCore.Logger.LogInfo($"Replaced material '{original}' with '{replacement.name}' in particle system '{particleSystem.gameObject.name}' in scene '{sceneToReplace.name}'.");
                                }
                            }
                            if (changed)
                            {
                                system.sharedMaterials = sharedMaterials;
                            }
                        }
                    }
                }   
            }
        }
    }
}
