using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace MaterialAssetRestorerCore
{
    [HarmonyPatch]
    internal static class MARCLeverPatchClass
    {
        private static string MARCDisabledHoverTip = "[ M.A.R.C. still caching materials! ]";
        private static string PreviousDisabledHoverTip = null;
        private static StartOfRound startOfRoundInstance = null;

        [HarmonyPatch(typeof(StartMatchLever), nameof(StartMatchLever.Update)), HarmonyPostfix]
        private static void LeverPatch(StartMatchLever __instance)
        {
            if (MaterialsNetworkSync.MARCPatchingLever.Value)
            {
                if (startOfRoundInstance == null)
                {
                    startOfRoundInstance = GameObject.FindObjectOfType<StartOfRound>();
                }
                if (__instance.triggerScript.disabledHoverTip != MARCDisabledHoverTip) //set PreviousDisabledHoverTip for restoration later, (ingoring MARC's own)
                {
                    PreviousDisabledHoverTip = __instance.triggerScript.disabledHoverTip;
                }
                if (MaterialsNetworkSync.waitingPlayerCount.Value > 0) //keep lever locked until MARC is done
                {
                    __instance.triggerScript.disabledHoverTip = MARCDisabledHoverTip;
                    __instance.triggerScript.interactable = false;
                }
                else
                {
                    __instance.triggerScript.disabledHoverTip = PreviousDisabledHoverTip; //restore the original disabledHoverTip (both LLL and DawnLib do set disabled hover tips)

                    //only unlock on the server because MARC runs at the start of the game, and vanilla only lets hosts pull first
                    //also only unlock while in orbit (not travelling)
                    if (__instance.IsServer && startOfRoundInstance.inShipPhase && !startOfRoundInstance.travellingToNewLevel)
                    {

                        __instance.triggerScript.interactable = true;
                    }

                }
            }
        }

        [HarmonyPatch(typeof(StartMatchLever), nameof(StartMatchLever.Start)), HarmonyPrefix]
        private static void SubscribeToLeverPull(StartMatchLever __instance)
        {
            if (MaterialsNetworkSync.MARCPatchingLever.Value)
            {
                MaterialAssetRestorerCore.Logger.LogDebug("Subscribing to lever pull event");
                //trigger when lever is pulled (if MARC is allowed to touch the lever) to disable MARC's ability to change the lever.
                //I don't want to interfere with LLL/DawnLib or vanilla behavior touching the lever. It should become true again though if someone joins with a latejoin mod.
                __instance.triggerScript.onInteract.AddListener(OnLeverPulled);
            }
        }
        private static void OnLeverPulled(PlayerControllerB player)
        {
            MaterialsNetworkSync.MARCPatchingLever.Value = false;
            MaterialAssetRestorerCore.Logger.LogDebug("Lever pulled, M.A.R.C. patching of lever disabled");
        }
    }
}
