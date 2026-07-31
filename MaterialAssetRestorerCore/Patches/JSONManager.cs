using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace MaterialAssetRestorerCore
{
    public class JSONManager
    {
        public static void ReadJSONFiles()
        {
            DirectoryInfo pluginsFolder = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent;
            //find the replacementDirectory subdirectory of pluginsFolder
            DirectoryInfo[] replacementDirectory = pluginsFolder.GetDirectories("MaterialReplacements", SearchOption.AllDirectories);
            if (replacementDirectory.Length == 0)
            {
                MaterialAssetRestorerCore.Logger.LogWarning("Could not find MaterialReplacements in plugins folder.");
                return;
            }
            else
            {
                MaterialAssetRestorerCore.Logger.LogDebug("Found MaterialReplacements in plugins folder with " + replacementDirectory.Length + " files.");

                foreach (DirectoryInfo directory in replacementDirectory) {       
                    foreach (FileInfo file in directory.GetFiles("*.json", SearchOption.AllDirectories))
                    {
                        MaterialAssetRestorerCore.Logger.LogDebug("Reading JSON file: " + file.FullName);
                        try
                        {
                            // reading and parsing json file
                            string json = File.ReadAllText(file.FullName, Encoding.UTF8);
                            MaterialSetsWrapper materialContainerWrapper = Newtonsoft.Json.JsonConvert.DeserializeObject<MaterialSetsWrapper>(json);
                            
                            if(materialContainerWrapper==null || materialContainerWrapper.MaterialSets.Count == 0) { 
                                MaterialAssetRestorerCore.Logger.LogWarning($"No material sets found in JSON file '{file.Name}', skipping.");
                                continue;
                            }

                            foreach (MaterialInformationContainer container in materialContainerWrapper.MaterialSets)
                            {
                                // validating the material container
                                if (string.IsNullOrEmpty(container.BaseMaterial))
                                {
                                    MaterialAssetRestorerCore.Logger.LogWarning($"Skipping entry in '{file.Name}': missing 'BaseMaterial'.");
                                    continue;
                                }
                                if (string.IsNullOrEmpty(container.ReplaceMaterial))
                                {
                                    MaterialAssetRestorerCore.Logger.LogWarning($"Skipping entry in '{file.Name}': missing 'ReplaceMaterial'.");
                                    continue;
                                }
                                if (string.IsNullOrEmpty(container.PrefabName) && string.IsNullOrEmpty(container.SceneName))
                                {
                                    MaterialAssetRestorerCore.Logger.LogWarning($"Skipping entry in '{file.Name}': While both aren't needed, either a valid 'PrefabName' or 'SceneName' is required.");
                                    continue;
                                }
                                if (
                                    (container.MaterialSource.Value==MaterialInformationContainer.MaterialType.VFX && container.MaterialDestination.Value != MaterialInformationContainer.MaterialType.VFX)
                                    ||
                                    (container.MaterialDestination.Value == MaterialInformationContainer.MaterialType.VFX && container.MaterialSource.Value != MaterialInformationContainer.MaterialType.VFX)
                                   )
                                {
                                    MaterialAssetRestorerCore.Logger.LogWarning($"Skipping entry in '{file.Name}': VFX must be paired with VFX");
                                    continue;
                                }
                                if (container.MaterialSource.Value==MaterialInformationContainer.MaterialType.TerrainDetail && string.IsNullOrEmpty(container.SceneName))
                                {
                                    MaterialAssetRestorerCore.Logger.LogWarning($"Skipping entry in '{file.Name}': TerrainDetails as a 'MaterialSource' must have a valid 'SceneName'");
                                    continue;
                                }
                                
                                // add it to the list of material information containers if valid
                                MaterialInit.materialInformationContainers.Add(container);
                                MaterialAssetRestorerCore.Logger.LogDebug(
                                    $"Registered: '{container.BaseMaterial}' -> '{container.ReplaceMaterial}'" +
                                    (container.SceneName != null ? $" in scene '{container.SceneName}'" : "") +
                                    (container.PrefabName != null ? $" from prefab '{container.PrefabName}'" : "") +
                                    $" (Source: {container.MaterialSource.Value}, Destination: {container.MaterialDestination.Value})"
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            MaterialAssetRestorerCore.Logger.LogError($"Error reading JSON file '{file.Name}': {ex.Message}");
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Used for deserializing the JSON file containing the material sets. The JSON file should contain an array of MaterialInformationContainer objects under the "MaterialSets" property.
        /// </summary>
        private class MaterialSetsWrapper
        {
            public List<MaterialInformationContainer> MaterialSets { get; set; }
        }
    }
}
