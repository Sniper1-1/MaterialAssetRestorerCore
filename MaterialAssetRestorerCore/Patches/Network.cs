using LethalNetworkAPI;

namespace MaterialAssetRestorerCore
{
    internal static class MaterialsNetworkSync
    {
        // keeps track of how many players aren't done getting the materials
        internal static LNetworkVariable<int> waitingPlayerCount = LNetworkVariable<int>.Connect(
            identifier: "waitingPlayerCount",
            onValueChanged: OnWaitingPlayerCountChanged,
            offlineValue: 0,
            writePerms: LNetworkVariableWritePerms.Everyone
        );

        // determines if MARC can lock/unlock lever. Becomes true when players join game. Becomes false after lever pulled
        // for the first time after everyone is done caching materials because I don't want to get in the way of vanilla, LLL, or DawnLib.
        internal static LNetworkVariable<bool> MARCPatchingLever = LNetworkVariable<bool>.Connect(
            identifier: "MARCPatchingLever",
            onValueChanged: OnMARCPatchingLeverChanged,
            offlineValue: true,
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
                MARCPatchingLever.Value = true;
                MaterialAssetRestorerCore.Logger.LogDebug("Wrong lever!");
            }
        }

        private static void OnMARCPatchingLeverChanged(bool oldValue, bool newValue)
        {
            MaterialAssetRestorerCore.Logger.LogWarning($"M.A.R.C. MARCPatchingLever changed from {oldValue} to {newValue}");
        }

        internal static void printDebug() { MaterialAssetRestorerCore.Logger.LogDebug("\n\n###############\nwaitingPlayerCount initialized\n###############\n"); }
    }
}
