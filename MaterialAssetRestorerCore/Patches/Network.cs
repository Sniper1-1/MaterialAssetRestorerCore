using System.Linq;
using HarmonyLib;
using LethalNetworkAPI;


namespace MaterialAssetRestorerCore
{
    internal static class MaterialsNetworkSync
    {
        public static LNetworkVariable<bool> materialsInitialized = LNetworkVariable<bool>.Connect(
            identifier: "materialsInitialized",
            onValueChanged: OnMaterialsInitializedChanged,
            offlineValue: false,
            writePerms: LNetworkVariableWritePerms.Everyone
        );

        private static void OnMaterialsInitializedChanged(bool oldValue, bool newValue)
        {
            MaterialAssetRestorerCore.Logger.LogWarning($"Materials initialized changed from {oldValue} to {newValue}");
        }
    }
}
