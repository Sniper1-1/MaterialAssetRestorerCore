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
                            string json = File.ReadAllText(file.FullName, Encoding.UTF8);
                            MaterialSetsWrapper materialContainerWrapper = Newtonsoft.Json.JsonConvert.DeserializeObject<MaterialSetsWrapper>(json);
                            
                            if(materialContainerWrapper==null || materialContainerWrapper.MaterialSets.Count == 0) { 
                                MaterialAssetRestorerCore.Logger.LogWarning($"No material sets found in JSON file '{file.Name}', skipping.");
                                continue;
                            }

                            foreach (MaterialInformationContainer container in materialContainerWrapper.MaterialSets)
                            {
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

                                MaterialInit.materialInformationContainers.Add(container);
                                MaterialAssetRestorerCore.Logger.LogDebug(
                                    $"Registered: '{container.BaseMaterial}' -> '{container.ReplaceMaterial}'" +
                                    (container.SceneName != null ? $" in scene '{container.SceneName}'" : "") +
                                    (container.PrefabName != null ? $" from prefab '{container.PrefabName}'" : "")
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
