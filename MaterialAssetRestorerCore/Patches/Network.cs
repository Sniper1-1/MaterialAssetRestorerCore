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
        public static LNetworkVariable<int> waitingPlayerCount = LNetworkVariable<int>.Connect(
            identifier: "waitingPlayerCount",
            onValueChanged: OnWaitingPlayerCountChanged,
            offlineValue: 0,
            writePerms: LNetworkVariableWritePerms.Everyone
        );

        private static void OnMaterialsInitializedChanged(bool oldValue, bool newValue)
        {
            MaterialAssetRestorerCore.Logger.LogWarning($"Materials initialized changed from {oldValue} to {newValue}");
            if (newValue == true)
            {
                MaterialAssetRestorerCore.Logger.LogDebug("Pull the lever, Kronk!");
            }
            else
            {
                MaterialAssetRestorerCore.Logger.LogDebug("Wrong lever!");
            }
        }

        private static void OnWaitingPlayerCountChanged(int oldValue, int newValue)
        {
            MaterialAssetRestorerCore.Logger.LogWarning($"M.A.R.C. Waiting player count changed from {oldValue} to {newValue}");
            if (newValue <= 0)
            {
                MaterialsNetworkSync.materialsInitialized.Value = true;
            }
            else
            {
                MaterialsNetworkSync.materialsInitialized.Value = false;
            }
        }
    }
}
