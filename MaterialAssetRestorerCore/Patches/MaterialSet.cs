using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

namespace MaterialAssetRestorerCore
{
    public class MaterialSet
    {
        /// <summary>
        /// Goes through all renderers in the scene and replaces the material with the original name with the replacement material.
        /// </summary>
        /// <param name="original">The original material/vfx name.</param>
        /// <param name="replacement">The replacement material/vfx.</param>
        /// <param name="sceneToReplace">The scene to perform the replacement in.</param>
        /// <param name="materialDestination">The type of the destination material (optional).</param>"
        public static void SET_material(string original, Object replacement, Scene sceneToReplace, MaterialInformationContainer.MaterialType? materialDestination=null)
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
                                sharedMaterials[i] = (Material)replacement;
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
                                    sharedMaterials[i] = (Material)replacement;
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
                //replace materials in TerrainDetails
                if (materialDestination == MaterialInformationContainer.MaterialType.TerrainDetails)
                {
                    foreach (var terrain in rootObj.GetComponentsInChildren<Terrain>(true))
                    {
                        var terrainData = terrain.terrainData;
                        if (terrainData != null)
                        {
                            var terrainDetails = terrainData.detailPrototypes;
                            bool changed = false;
                            for (int i = 0; i < terrainDetails.Length; i++)
                            {
                                if (terrainDetails[i].prototype != null)
                                {
                                    var terrainDetailRenderer = terrainDetails[i].prototype.GetComponent<Renderer>().sharedMaterials;
                                    for (int j = 0; j < terrainDetailRenderer.Length; j++)
                                    {
                                        if (terrainDetailRenderer[j] != null && terrainDetailRenderer[j].name == original)
                                        {
                                            terrainDetailRenderer[j] = (Material)replacement;
                                            changed = true;
                                            MaterialAssetRestorerCore.Logger.LogInfo($"Replaced material '{original}' with '{replacement.name}' in terrain detail '{terrainDetails[i].prototype.name}' in scene '{sceneToReplace.name}'.");
                                        }
                                    }
                                    if (changed)
                                    {
                                        terrainDetails[i].prototype.GetComponent<Renderer>().sharedMaterials = terrainDetailRenderer;
                                        terrainData.RefreshPrototypes(); // they don't render without a refresh
                                    }
                                }
                            }
                        }
                    }
                }
                if (materialDestination == MaterialInformationContainer.MaterialType.VFX) 
                { 
                    foreach (VisualEffect vfx in rootObj.GetComponentsInChildren<VisualEffect>(true))
                    {
                        bool changed = false;
                        if (vfx != null && vfx.visualEffectAsset.name == original)
                        {
                            vfx.visualEffectAsset = (VisualEffectAsset)replacement;
                            vfx.Reinit(); // forces the component to recompile and reinitialize with the new asset
                            MaterialAssetRestorerCore.Logger.LogInfo($"Replaced vfx '{original}' with '{replacement.name}' in vfx '{vfx.name}' in scene '{sceneToReplace.name}'.");
                        }
                    }
                }
            }
        }
    }
}
