using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace MaterialAssetRestorerCore
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("LethalLevelLoader",BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("DawnLib",BepInDependency.DependencyFlags.SoftDependency)]
    public class MaterialAssetRestorerCore : BaseUnityPlugin
    {
        public static MaterialAssetRestorerCore Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        // used for locking the lever pull dependon on if LethalLevelLoader or DawnLib is installed (prevent attempts to land before materials retrieved)
        public static bool hasLLL=false;
        public static bool hasDL=false;

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            NetcodePatcher();
            Patch();

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
            Logger.LogInfo("Oh hi, M.A.R.C.");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll(typeof(MaterialInit));
            Harmony.PatchAll(typeof(WaterSwap));
            JSONManager.ReadJSONFiles();
            PatchLever();

            Logger.LogDebug("Finished patching!");
        }

        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }

        private void NetcodePatcher()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
        private static void PatchLever()
        {
            try
            {
                Harmony.PatchAll(typeof(LLLLeverPatch));
                MaterialAssetRestorerCore.Logger.LogInfo("LethalLevelLoader detected, patching CheckLever()");
                hasLLL = true;
            }
            catch
            {
                MaterialAssetRestorerCore.Logger.LogInfo("LethalLevelLoader not detected, checking for DawnLib");
                try
                {
                    Harmony.PatchAll(typeof(DawnLibLeverPatch));
                    MaterialAssetRestorerCore.Logger.LogInfo("DawnLib detected, patching CheckReadyAndUpdateUI()");
                    hasDL = true;
                }
                catch
                {
                    MaterialAssetRestorerCore.Logger.LogWarning("DawnLib not detected either. What would you possible be using this for?");
                }

            }
        }
    }
}
