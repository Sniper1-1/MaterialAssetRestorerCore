using System.Linq;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace MaterialAssetRestorerCore
{

    // Creates a network prefab, which prevents joining between those with and without the mod. I'm not sure it's really needed for this mod,
    // but I worry that someone could use it to maybe give certain walls a clear material or scrap a brighter one. idk, I feel like there could be cheating
    [HarmonyPatch(typeof(NetworkManager))]
    internal static class MARCNetworkPrefabPatch
    {
        private static readonly string MOD_GUID = MyPluginInfo.PLUGIN_GUID;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(NetworkManager.SetSingleton))]
        private static void RegisterPrefab()
        {
            MaterialAssetRestorerCore.Logger.LogInfo("Registering network prefab for M.A.R.C....");

            var prefab = new GameObject(MOD_GUID + " Prefab");
            prefab.hideFlags |= HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(prefab);
            var networkObject = prefab.AddComponent<NetworkObject>();
            networkObject.GlobalObjectIdHash = GetHash(MOD_GUID);

            prefab.AddComponent<MARCNetworker>();
            NetworkManager.Singleton.PrefabHandler.AddNetworkPrefab(prefab);

            return;

            static uint GetHash(string value)
            {
                return value?.Aggregate(17u, (current, c) => unchecked((current * 31) ^ c)) ?? 0u;
            }
        }

    }
    public class MARCNetworker : NetworkBehaviour
    {

        public static MARCNetworker Instance { get; private set; }

        public override void OnNetworkSpawn()
        {
            Instance = this;
            MaterialAssetRestorerCore.Logger.LogInfo($"MARCNetworker spawned on client {NetworkManager.Singleton.LocalClientId}");
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetMaterialsInitializedServerRpc(bool value)
        {
            SetMaterialsInitializedClientRpc(value);
        }
        [ClientRpc]
        public void SetMaterialsInitializedClientRpc(bool value)
        {
            MaterialAssetRestorerCore.Logger.LogInfo($"Setting materialsInitialized to {value} on client {NetworkManager.Singleton.LocalClientId}");
            MaterialInit.materialsInitialized = value;
        }
    }
}