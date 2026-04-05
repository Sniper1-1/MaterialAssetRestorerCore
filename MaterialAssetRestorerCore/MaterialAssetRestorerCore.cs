using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace MaterialAssetRestorerCore
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class MaterialAssetRestorerCore : BaseUnityPlugin
    {
        public static MaterialAssetRestorerCore Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            Patch();

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");

            //log current directory and files within
            Logger.LogDebug($"Current directory: {new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent}");
            var files = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent.GetFiles();
            Logger.LogDebug($"Files in current directory: {string.Join(", ", files.ToString())}");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll();

            Logger.LogDebug("Finished patching!");
        }

        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }
    }
}
