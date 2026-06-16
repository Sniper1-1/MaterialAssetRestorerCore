using LethalNetworkAPI;


namespace MaterialAssetRestorerCore
{
    internal static class MaterialsNetworkSync
    {
        public static LNetworkVariable<int> waitingPlayerCount = LNetworkVariable<int>.Connect(
            identifier: "waitingPlayerCount",
            onValueChanged: OnWaitingPlayerCountChanged,
            offlineValue: 0,
            writePerms: LNetworkVariableWritePerms.Everyone
        );

        private static void OnWaitingPlayerCountChanged(int oldValue, int newValue)
        {
            MaterialAssetRestorerCore.Logger.LogWarning($"M.A.R.C. Waiting player count changed from {oldValue} to {newValue}");
            if (newValue <= 0)
            {
                MaterialAssetRestorerCore.Logger.LogDebug("Pull the lever, Kronk!");
            }
            else
            {
                MaterialAssetRestorerCore.Logger.LogDebug("Wrong lever!");
            }
        }

        public static void printDebug() { MaterialAssetRestorerCore.Logger.LogDebug("\n\n###############\nwaitingPlayerCount initialized\n###############\n"); }
    }
}
