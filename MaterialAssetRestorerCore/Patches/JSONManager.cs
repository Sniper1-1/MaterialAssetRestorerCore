using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace MaterialAssetRestorer
{
    public class JSONManager
    {
        public static void ReadJSONfiles()
        {
            var pluginsFolder = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent;
            //find the replacementDirectory subdirectory of pluginsFolder
            var replacementDirectory = pluginsFolder.GetDirectories("MaterialReplacements", SearchOption.AllDirectories);
            if (replacementDirectory.Length == 0)
            {
                MaterialAssetRestorerCore.Logger.LogWarning("Could not find MaterialReplacements in plugins folder.");
                return;
            }
            else
            {
                MaterialAssetRestorerCore.Logger.LogInfo("Found MaterialReplacements in plugins folder with " + replacementDirectory.Length + " files.");
            }
        }
    }
}
