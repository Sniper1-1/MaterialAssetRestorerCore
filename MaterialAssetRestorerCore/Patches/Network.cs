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
            var prefab = new GameObject(MOD_GUID + " Prefab");
            prefab.hideFlags |= HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(prefab);
            var networkObject = prefab.AddComponent<NetworkObject>();
            networkObject.GlobalObjectIdHash = GetHash(MOD_GUID);
            prefab.AddComponent<NetworkBool>();

            NetworkManager.Singleton.PrefabHandler.AddNetworkPrefab(prefab);
            return;

            static uint GetHash(string value)
            {
                return value?.Aggregate(17u, (current, c) => unchecked((current * 31) ^ c)) ?? 0u;
            }
        }
    }

    internal class NetworkBool: NetworkBehaviour
    {
        public static NetworkBool Instance { get; internal set; }
        public override void OnNetworkSpawn()
        {
            Instance = this;
        }

        public static NetworkVariable<bool> materialsInitialized = new NetworkVariable<bool>(false);
        internal static void Init()
        {
            NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<bool>();
            NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<bool>();
        }
        [Rpc(SendTo.ClientsAndHost)]
        public void SetBoolServerRpc(bool value)
        {
            materialsInitialized.Value = value;
        }
    }
}
