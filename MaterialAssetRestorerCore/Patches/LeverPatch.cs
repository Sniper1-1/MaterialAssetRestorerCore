using System.Collections;
using Dawn.Internal;
using DunGen;
using HarmonyLib;

namespace MaterialAssetRestorerCore
{
    internal static class LLLLeverPatch
    {
        //runs before LLL does its check for if the lever should be locked/unlocked. Once I'm done, LLL can do its checks
        [HarmonyPatch(typeof(LethalLevelLoader.Patches), nameof(LethalLevelLoader.Patches.CheckLever)), HarmonyPrefix]
        public static bool LeverPatch(InteractTrigger trigger)
        {
            MaterialAssetRestorerCore.Logger.LogWarning("Lever enabled: " + MaterialInit.materialsInitialized);
            if(!MaterialInit.materialsInitialized){
                trigger.disabledHoverTip = "[ M.A.R.C. still caching materials!]";
                trigger.interactable = false;
            }            
            return true;
        }        
    }

    internal static class DawnLibLeverPatch
    {
        [HarmonyPatch(typeof(Dawn.Internal.DawnMoonNetworker), nameof(Dawn.Internal.DawnMoonNetworker.CheckReadyAndUpdateUI)), HarmonyPrefix]
        public static void LeverPatch()
        {
            CoroutineHelper.Instance.StartCoroutine(LeverUpdateLoop());
        }
        public static IEnumerator LeverUpdateLoop()
        {
            while (!MaterialInit.materialsInitialized)
            {
                StartMatchLeverRefs.Instance.triggerScript.disabledHoverTip = "[ M.A.R.C. still caching materials! ]";
                StartMatchLeverRefs.Instance.triggerScript.interactable = false;
                yield return null;
            }
            DawnMoonNetworker.Instance.CheckReadyAndUpdateUI(); //retrigger DawnLib to check if it needs to lock/unlock
        }
    }
}
